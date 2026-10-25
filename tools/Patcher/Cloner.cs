// Copies the types of a freshly compiled assembly (compiled against a publicized Assembly-CSharp) into the real
// Assembly-CSharp module, re-targeting every reference so that nothing points at a "second" Assembly-CSharp.
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public class Cloner
{
    readonly ModuleDefinition src, dst;
    readonly string selfName;
    readonly Dictionary<TypeDefinition, TypeDefinition> typeMap = new Dictionary<TypeDefinition, TypeDefinition>();
    readonly Dictionary<MethodDefinition, MethodDefinition> methodMap = new Dictionary<MethodDefinition, MethodDefinition>();
    readonly Dictionary<FieldDefinition, FieldDefinition> fieldMap = new Dictionary<FieldDefinition, FieldDefinition>();
    readonly Dictionary<string, TypeDefinition> dstTypes = new Dictionary<string, TypeDefinition>();
    public readonly List<TypeDefinition> NewTypes = new List<TypeDefinition>();
    public readonly HashSet<string> PublicizedLog = new HashSet<string>();
    public readonly List<string> KeptNonPublicLog = new List<string>();
    // Diagnostic bisection (PATCH_SKIP=publicize): clone the types without flipping any game member public.
    public bool DoPublicize = true;

    public Cloner(ModuleDefinition source, ModuleDefinition destination)
    {
        src = source; dst = destination; selfName = dst.Assembly.Name.Name;
        foreach (var t in Walk(dst.Types)) dstTypes[t.FullName] = t;
    }

    static IEnumerable<TypeDefinition> Walk(IEnumerable<TypeDefinition> ts)
    {
        foreach (var t in ts) { yield return t; foreach (var n in Walk(t.NestedTypes)) yield return n; }
    }

    public TypeDefinition Mapped(string srcFullName)
    {
        foreach (var kv in typeMap) if (kv.Key.FullName == srcFullName) return kv.Value;
        return null;
    }

    // ------------------------------------------------------------------ driver

    public void CloneAll()
    {
        var top = src.Types.Where(t => t.Name != "<Module>").ToList();
        foreach (var t in top) CreateType(t, null);
        foreach (var kv in typeMap.ToList()) FillType(kv.Key, kv.Value);
        foreach (var kv in methodMap.ToList()) CloneBody(kv.Key, kv.Value);
    }

    void CreateType(TypeDefinition s, TypeDefinition declaring)
    {
        if (s.HasGenericParameters) throw new Exception("generic types are not supported: " + s.FullName);
        string name = s.Name;
        if (declaring == null && dstTypes.ContainsKey(s.FullName))
        {
            if (name.StartsWith("<PrivateImplementationDetails>")) name = name.Replace(">", "_GC>");
            else throw new Exception("type already exists in destination: " + s.FullName);
        }
        var d = new TypeDefinition(declaring == null ? s.Namespace : "", name, s.Attributes);
        if (declaring == null) dst.Types.Add(d); else declaring.NestedTypes.Add(d);
        typeMap[s] = d;
        NewTypes.Add(d);
        foreach (var n in s.NestedTypes) CreateType(n, d);
    }

    void FillType(TypeDefinition s, TypeDefinition d)
    {
        if (s.BaseType != null) d.BaseType = ImportType(s.BaseType);
        foreach (var i in s.Interfaces) d.Interfaces.Add(new InterfaceImplementation(ImportType(i.InterfaceType)));
        if (s.PackingSize != -1) d.PackingSize = s.PackingSize;
        if (s.ClassSize != -1) d.ClassSize = s.ClassSize;
        CopyAttributes(s, d);

        foreach (var f in s.Fields)
        {
            var nf = new FieldDefinition(f.Name, f.Attributes, ImportType(f.FieldType));
            if (f.HasConstant) nf.Constant = f.Constant;
            if (f.InitialValue != null && f.InitialValue.Length > 0) nf.InitialValue = f.InitialValue;
            d.Fields.Add(nf);
            fieldMap[f] = nf;
            CopyAttributes(f, nf);
        }
        foreach (var m in s.Methods)
        {
            if (m.HasGenericParameters) throw new Exception("generic methods are not supported: " + m.FullName);
            var nm = new MethodDefinition(m.Name, m.Attributes, ImportType(m.ReturnType));
            nm.ImplAttributes = m.ImplAttributes;
            nm.SemanticsAttributes = m.SemanticsAttributes;
            foreach (var p in m.Parameters)
            {
                var np = new ParameterDefinition(p.Name, p.Attributes, ImportType(p.ParameterType));
                if (p.HasConstant) np.Constant = p.Constant;
                nm.Parameters.Add(np);
            }
            d.Methods.Add(nm);
            methodMap[m] = nm;
            CopyAttributes(m, nm);
        }
        foreach (var m in s.Methods)
            foreach (var o in m.Overrides) methodMap[m].Overrides.Add(ImportMethod(o));
        foreach (var p in s.Properties)
        {
            var np = new PropertyDefinition(p.Name, p.Attributes, ImportType(p.PropertyType));
            if (p.GetMethod != null) np.GetMethod = methodMap[p.GetMethod];
            if (p.SetMethod != null) np.SetMethod = methodMap[p.SetMethod];
            d.Properties.Add(np);
        }
        foreach (var e in s.Events)
        {
            var ne = new EventDefinition(e.Name, e.Attributes, ImportType(e.EventType));
            if (e.AddMethod != null) ne.AddMethod = methodMap[e.AddMethod];
            if (e.RemoveMethod != null) ne.RemoveMethod = methodMap[e.RemoveMethod];
            d.Events.Add(ne);
        }
    }

    void CopyAttributes(ICustomAttributeProvider s, ICustomAttributeProvider d)
    {
        foreach (var ca in s.CustomAttributes)
        {
            var c = new CustomAttribute(ImportMethod(ca.Constructor), ca.GetBlob());
            d.CustomAttributes.Add(c);
        }
    }

    // ------------------------------------------------------------------ bodies

    void CloneBody(MethodDefinition s, MethodDefinition d)
    {
        if (!s.HasBody) return;
        var sb = s.Body; var db = d.Body;
        db.InitLocals = sb.InitLocals;
        db.MaxStackSize = sb.MaxStackSize;
        foreach (var v in sb.Variables) db.Variables.Add(new VariableDefinition(v.Name, ImportType(v.VariableType)));

        var map = new Dictionary<Instruction, Instruction>();
        foreach (var i in sb.Instructions)
        {
            Instruction ni;
            object o = i.Operand;
            if (o == null) ni = Instruction.Create(i.OpCode);
            else if (o is TypeReference) ni = Instruction.Create(i.OpCode, ImportType((TypeReference)o));
            else if (o is MethodReference) ni = Instruction.Create(i.OpCode, ImportMethod((MethodReference)o));
            else if (o is FieldReference) ni = Instruction.Create(i.OpCode, ImportField((FieldReference)o));
            else if (o is VariableDefinition) ni = Instruction.Create(i.OpCode, db.Variables[sb.Variables.IndexOf((VariableDefinition)o)]);
            else if (o is ParameterDefinition)
            {
                var p = (ParameterDefinition)o;
                if (s.HasThis && p == sb.ThisParameter) ni = Instruction.Create(i.OpCode, db.ThisParameter);
                else ni = Instruction.Create(i.OpCode, d.Parameters[s.Parameters.IndexOf(p)]);
            }
            else if (o is Instruction) ni = Instruction.Create(i.OpCode, (Instruction)o);   // fixed below
            else if (o is Instruction[]) ni = Instruction.Create(i.OpCode, (Instruction[])o); // fixed below
            else if (o is string) ni = Instruction.Create(i.OpCode, (string)o);
            else if (o is int) ni = Instruction.Create(i.OpCode, (int)o);
            else if (o is long) ni = Instruction.Create(i.OpCode, (long)o);
            else if (o is sbyte) ni = Instruction.Create(i.OpCode, (sbyte)o);
            else if (o is byte) ni = Instruction.Create(i.OpCode, (byte)o);
            else if (o is float) ni = Instruction.Create(i.OpCode, (float)o);
            else if (o is double) ni = Instruction.Create(i.OpCode, (double)o);
            else throw new Exception("unsupported operand " + o.GetType() + " in " + s.FullName);
            map[i] = ni;
            db.Instructions.Add(ni);
        }
        foreach (var ni in db.Instructions)
        {
            if (ni.Operand is Instruction) ni.Operand = map[(Instruction)ni.Operand];
            else if (ni.Operand is Instruction[]) ni.Operand = ((Instruction[])ni.Operand).Select(x => map[x]).ToArray();
        }
        foreach (var h in sb.ExceptionHandlers)
        {
            var nh = new ExceptionHandler(h.HandlerType);
            nh.TryStart = map[h.TryStart];
            nh.TryEnd = h.TryEnd == null ? null : map[h.TryEnd];
            nh.HandlerStart = map[h.HandlerStart];
            nh.HandlerEnd = h.HandlerEnd == null ? null : map[h.HandlerEnd];
            if (h.CatchType != null) nh.CatchType = ImportType(h.CatchType);
            if (h.FilterStart != null) nh.FilterStart = map[h.FilterStart];
            db.ExceptionHandlers.Add(nh);
        }
    }

    // ------------------------------------------------------------------ reference import

    bool IsSelf(IMetadataScope scope)
    {
        var an = scope as AssemblyNameReference;
        return an != null && an.Name == selfName;
    }

    public TypeReference ImportType(TypeReference t)
    {
        if (t == null) return null;
        if (t is ArrayType) { var a = (ArrayType)t; var na = new ArrayType(ImportType(a.ElementType), a.Rank); return na; }
        if (t is ByReferenceType) return new ByReferenceType(ImportType(((ByReferenceType)t).ElementType));
        if (t is PointerType) return new PointerType(ImportType(((PointerType)t).ElementType));
        if (t is GenericInstanceType)
        {
            var g = (GenericInstanceType)t;
            var ng = new GenericInstanceType(ImportType(g.ElementType));
            foreach (var a in g.GenericArguments) ng.GenericArguments.Add(ImportType(a));
            return ng;
        }
        if (t is GenericParameter) throw new Exception("open generic parameter in " + t.FullName);
        if (t is RequiredModifierType) { var r = (RequiredModifierType)t; return new RequiredModifierType(ImportType(r.ModifierType), ImportType(r.ElementType)); }
        if (t is OptionalModifierType) { var r = (OptionalModifierType)t; return new OptionalModifierType(ImportType(r.ModifierType), ImportType(r.ElementType)); }

        var td = t as TypeDefinition;
        if (td != null && typeMap.ContainsKey(td)) return typeMap[td];
        if (td == null && t.Module == src && IsInSource(t)) { var x = FindSrcDef(t); if (x != null) return typeMap[x]; }
        if (IsSelf(t.Scope) || (t.DeclaringType != null && IsSelfType(t)))
        {
            TypeDefinition target;
            if (dstTypes.TryGetValue(t.FullName, out target)) { Publicize(target); return target; }
            throw new Exception("type not found in destination: " + t.FullName);
        }
        return dst.ImportReference(t);
    }

    bool IsSelfType(TypeReference t)
    {
        while (t.DeclaringType != null) t = t.DeclaringType;
        return IsSelf(t.Scope);
    }

    bool IsInSource(TypeReference t) { return FindSrcDef(t) != null; }
    TypeDefinition FindSrcDef(TypeReference t)
    {
        foreach (var kv in typeMap) if (kv.Key.FullName == t.FullName) return kv.Key;
        return null;
    }

    public FieldReference ImportField(FieldReference f)
    {
        var fd = f as FieldDefinition;
        if (fd != null && fieldMap.ContainsKey(fd)) return fieldMap[fd];
        var declaring = f.DeclaringType;
        if (declaring is GenericInstanceType) return dst.ImportReference(f);
        var dt = ImportType(declaring) as TypeDefinition;
        if (dt != null)
        {
            var found = dt.Fields.FirstOrDefault(x => x.Name == f.Name);
            if (found == null) throw new Exception("field not found: " + f.FullName);
            Publicize(found);
            return found;
        }
        return dst.ImportReference(f);
    }

    static string Sig(MethodReference m, Func<TypeReference, TypeReference> imp)
    {
        return m.Name + "(" + string.Join(",", m.Parameters.Select(p => (imp(p.ParameterType) ?? p.ParameterType).FullName).ToArray()) + ")" + (m.HasGenericParameters ? "`" + m.GenericParameters.Count : "");
    }

    static string SigOfDef(MethodDefinition m)
    {
        return m.Name + "(" + string.Join(",", m.Parameters.Select(p => p.ParameterType.FullName).ToArray()) + ")" + (m.HasGenericParameters ? "`" + m.GenericParameters.Count : "");
    }

    public MethodReference ImportMethod(MethodReference m)
    {
        if (m == null) return null;
        var gim = m as GenericInstanceMethod;
        if (gim != null)
        {
            var el = ImportMethod(gim.ElementMethod);
            var ng = new GenericInstanceMethod(el);
            foreach (var a in gim.GenericArguments) ng.GenericArguments.Add(ImportType(a));
            return ng;
        }
        var md = m as MethodDefinition;
        if (md != null && methodMap.ContainsKey(md)) return methodMap[md];

        var declaring = m.DeclaringType;
        var gi = declaring as GenericInstanceType;
        if (gi != null)
        {
            // method of a generic instance (e.g. List<MajorEmpire>::Add): rebuild on top of the imported open method
            var giNew = (GenericInstanceType)ImportType(gi);
            var open = new MethodReference(m.Name, m.ReturnType, gi.ElementType) { HasThis = m.HasThis, ExplicitThis = m.ExplicitThis, CallingConvention = m.CallingConvention };
            foreach (var p in m.Parameters) open.Parameters.Add(new ParameterDefinition(p.ParameterType));
            var openImported = dst.ImportReference(open);
            var r = new MethodReference(openImported.Name, openImported.ReturnType, giNew) { HasThis = openImported.HasThis, ExplicitThis = openImported.ExplicitThis, CallingConvention = openImported.CallingConvention };
            foreach (var p in openImported.Parameters) r.Parameters.Add(new ParameterDefinition(p.ParameterType));
            return r;
        }
        var dt = ImportType(declaring) as TypeDefinition;
        if (dt != null && !typeMap.ContainsValue(dt))   // destination (game) type
        {
            string sig = Sig(m, ImportTypeSafe);
            var found = dt.Methods.FirstOrDefault(x => SigOfDef(x) == sig);
            if (found == null) throw new Exception("method not found in destination: " + m.FullName + "  [" + sig + "]");
            Publicize(found);
            return found;
        }
        if (dt != null) throw new Exception("unmapped method on cloned type: " + m.FullName);
        return dst.ImportReference(m);
    }

    TypeReference ImportTypeSafe(TypeReference t)
    {
        try { return ImportType(t); } catch (Exception) { return t; }
    }

    // ------------------------------------------------------------------ visibility

    public void Publicize(TypeDefinition t)
    {
        if (!DoPublicize) return;
        if (typeMap.ContainsValue(t)) return;
        for (var x = t; x != null; x = x.DeclaringType)
        {
            if (x.IsNested) { if (!x.IsNestedPublic) { x.IsNestedPublic = true; PublicizedLog.Add(x.FullName); } }
            else if (!x.IsPublic) { x.IsPublic = true; PublicizedLog.Add(x.FullName); }
        }
    }

    // Accessors of [GuiBound] properties must stay non-public: GuiBoundAttribute.BindAllProperties finds them
    // with BindingFlags NonPublic|Instance|FlattenHierarchy only. Publicizing the getter makes the property
    // public, the binding silently skips it, and every GuiModalWindow keeps BackgroundGroup/MainContainer null
    // -> NullReferenceException in a window Load coroutine and a stuck main-menu loading screen. Cloned code
    // must read such properties from inside the derived window class instead (see CouncilModalWindow.ChromeBound).
    static bool IsGuiBoundAccessor(MethodDefinition m)
    {
        if ((m.SemanticsAttributes & (MethodSemanticsAttributes.Getter | MethodSemanticsAttributes.Setter)) == 0) return false;
        foreach (var p in m.DeclaringType.Properties)
        {
            if (p.GetMethod != m && p.SetMethod != m) continue;
            foreach (var a in p.CustomAttributes)
                if (a.AttributeType.Name == "GuiBoundAttribute") return true;
        }
        return false;
    }

    public void Publicize(MethodDefinition m)
    {
        if (!DoPublicize) return;
        if (IsGuiBoundAccessor(m)) { KeptNonPublicLog.Add(m.FullName); return; }
        if (typeMap.ContainsValue(m.DeclaringType)) return;
        Publicize(m.DeclaringType);
        if (!m.IsPublic) { m.IsPublic = true; PublicizedLog.Add(m.FullName); }
    }

    public void Publicize(FieldDefinition f)
    {
        if (!DoPublicize) return;
        if (typeMap.ContainsValue(f.DeclaringType)) return;
        Publicize(f.DeclaringType);
        if (!f.IsPublic) { f.IsPublic = true; PublicizedLog.Add(f.FullName); }
    }
}

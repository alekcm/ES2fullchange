// Small Cecil-based inspector used by the build/test tooling (mono, no pythonnet needed):
//   ilseek <assembly.dll> types <substring>
//   ilseek <assembly.dll> type <TypeName>
//   ilseek <assembly.dll> il <TypeName> <MethodName> [paramCount]
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

static class IlSeek
{
    static AssemblyDefinition asmDef;
    static ModuleDefinition mod;

    static IEnumerable<TypeDefinition> Walk()
    {
        foreach (var top in mod.GetTypes())
        {
            yield return top;
            foreach (var n in Nested(top)) yield return n;
        }
    }
    static IEnumerable<TypeDefinition> Nested(TypeDefinition t)
    {
        foreach (var n in t.NestedTypes) { yield return n; foreach (var m in Nested(n)) yield return m; }
    }

    static void Main(string[] argv)
    {
        var rp = new ReaderParameters { AssemblyResolver = new Res { Dir = System.IO.Path.GetDirectoryName(argv[0]) } };
        mod = ModuleDefinition.ReadModule(argv[0], rp);
        string mode = argv[1];
        if (mode == "types")
        {
            string needle = argv[2];
            foreach (var t in Walk().Where(t => t.IsPublic && t.Name.Contains(needle)).OrderBy(t => t.Name))
                Console.WriteLine(t.Name + " : " + (t.BaseType != null ? t.BaseType.Name : "-") + "  [" + string.Join(", ", t.Methods.Where(m => m.IsPublic && !m.IsSpecialName && !m.Name.Contains("<")).Select(m => m.Name).ToArray()) + "]");
        }
        else if (mode == "type")
        {
            foreach (var t in Walk().Where(t => t.Name == argv[2]))
            {
                Console.WriteLine("== " + t.FullName + " : " + (t.BaseType != null ? t.BaseType.Name : "-"));
                foreach (var f in t.Fields) Console.WriteLine("   F " + f.FieldType.Name + " " + f.Name + (f.IsPublic ? " [pub]" : f.IsFamily ? " [prot]" : " [priv]"));
                foreach (var m in t.Methods.Where(m => !m.IsSpecialName && !m.Name.Contains("<")))
                    Console.WriteLine("   M " + m.ReturnType.Name + " " + m.Name + "(" + string.Join(", ", m.Parameters.Select(p => p.ParameterType.Name).ToArray()) + ")" + (m.IsPublic ? " [pub]" : m.IsPrivate ? " [priv]" : " [other]") + (m.IsVirtual ? " [virt]" : ""));
            }
        }
        else if (mode == "il")
        {
            foreach (var t in Walk().Where(t => t.Name == argv[2]))
                foreach (var m in t.Methods.Where(m => m.Name == argv[3] && (argv.Length < 5 || m.Parameters.Count == int.Parse(argv[4]))))
                {
                    if (!m.HasBody) { Console.WriteLine("   (no body)"); continue; }
                    Console.WriteLine("   " + m.ReturnType.Name + " " + m.Name + " [" + m.Body.Instructions.Count + "]");
                    foreach (var i in m.Body.Instructions)
                        Console.WriteLine("      IL_" + i.Offset.ToString("x4") + " " + i.OpCode.Name + " " + (i.Operand == null ? "" : i.Operand.ToString()));
                }
        }
        else if (mode == "props")   // get_* properties (including special names)
        {
            foreach (var t in Walk().Where(t => t.Name == argv[2]))
                foreach (var m in t.Methods.Where(m => m.IsPublic && m.Name.StartsWith("get_")))
                    Console.WriteLine("   P " + m.ReturnType.Name + " " + m.Name.Substring(4));
        }
        else if (mode == "users")   // who references a member named "<Type>::<member>" in its IL
        {
            string needle = argv[2];
            foreach (var t in Walk())
                foreach (var m in t.Methods)
                {
                    if (!m.HasBody) continue;
                    try
                    {
                        foreach (var i in m.Body.Instructions)
                            if ((i.Operand ?? "").ToString().Contains(needle)) { Console.WriteLine(t.Name + "." + m.Name); break; }
                    }
                    catch (Exception) { }
                }
        }
    }

    class Res : BaseAssemblyResolver
    {
        public string Dir;
        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            try { return base.Resolve(name); }
            catch (Exception) { return AssemblyDefinition.ReadAssembly(System.IO.Path.Combine(Dir, name.Name + ".dll")); }
        }
    }
}

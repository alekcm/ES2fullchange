// Makes every type/member public so new code can be compiled against private game internals.
using System;
using Mono.Cecil;
class Publicize {
    static void Main(string[] a) {
        var res = new DefaultAssemblyResolver(); res.AddSearchDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(a[0])));
        var m = ModuleDefinition.ReadModule(a[0], new ReaderParameters { AssemblyResolver = res });
        foreach (var t in m.GetTypes()) {
            if (t.IsNested) t.IsNestedPublic = true; else t.IsPublic = true;
            foreach (var f in t.Fields) f.IsPublic = true;
            foreach (var me in t.Methods) { if (!me.IsRuntimeSpecialName || me.Name == ".ctor" || me.Name == ".cctor") me.IsPublic = true; }
        }
        m.Write(a[1]);
    }
}

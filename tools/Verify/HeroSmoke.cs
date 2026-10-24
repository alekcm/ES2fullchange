// Runs HeroRework.BuildTrees from the patched assembly on synthetic skill trees (no game running): checks filtering by hero kind.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

static class HeroSmoke
{
    static Assembly asm;
    static T New<T>() { return (T)FormatterServices.GetUninitializedObject(typeof(T)); }
    static Type T(string name)
    {
        var t = asm.GetType(name);
        if (t != null) return t;
        foreach (var r in asm.GetReferencedAssemblies()) { try { t = Assembly.Load(r).GetType(name); } catch (Exception) { } if (t != null) return t; }
        throw new Exception("type not found " + name);
    }
    static object Make(string type) { return FormatterServices.GetUninitializedObject(T(type)); }
    static void Set(object o, string field, object v)
    {
        var t = o.GetType();
        FieldInfo f = null;
        while (t != null && f == null) { f = t.GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly); t = t.BaseType; }
        if (f == null) throw new Exception("no field " + field + " on " + o.GetType());
        f.SetValue(o, v);
    }
    static object Desc() { return Make("Amplitude.Unity.Simulation.SimulationDescriptor"); }
    static Array Arr(string elem, int n) { return Array.CreateInstance(T(elem), n); }

    // skill with the given channel effects: s = system, h = ship, e = senate
    static object Skill(string channels, int levels)
    {
        var def = Make("HeroSkillDefinition");
        var lt = T("HeroSkillDefinition+HeroSkillLevelDefinition");
        var arr = Array.CreateInstance(lt, levels);
        for (int i = 0; i < levels; i++)
        {
            var l = Activator.CreateInstance(lt, true);
            foreach (char c in channels)
            {
                var da = Arr("Amplitude.Unity.Simulation.SimulationDescriptor", 1);
                da.SetValue(Desc(), 0);
                Set(l, c == 's' ? "systemDescriptors" : c == 'h' ? "shipDescriptors" : "senateDescriptors", da);
            }
            arr.SetValue(l, i);
        }
        Set(def, "<SkillLevels>k__BackingField", arr);
        Set(def, "<Cost>k__BackingField", 1);
        return def;
    }
    static object Slot(object skill) { var s = Make("HeroSkillTreeSkill"); Set(s, "skillDefinition", skill); return s; }
    static object Stage(int req, params object[] slots)
    {
        var st = Make("HeroSkillTreeStage"); Set(st, "RequiredLevel", req);
        var a = Arr("HeroSkillTreeSkill", slots.Length); for (int i = 0; i < slots.Length; i++) a.SetValue(slots[i], i);
        Set(st, "Skills", a); return st;
    }
    static object Tree(params object[] stages)
    {
        var t = Make("HeroSkillTreeDefinition");
        var a = Arr("HeroSkillTreeStage", stages.Length); for (int i = 0; i < stages.Length; i++) a.SetValue(stages[i], i);
        Set(t, "Stages", a); return t;
    }

    static int Count(object tree, out int stages)
    {
        var st = (Array)tree.GetType().GetField("Stages").GetValue(tree);
        stages = st.Length; int n = 0;
        foreach (var s in st) n += ((Array)s.GetType().GetField("Skills").GetValue(s)).Length;
        return n;
    }

    static int Main(string[] a)
    {
        asm = Assembly.LoadFrom(a[0]);
        var rework = asm.GetType("HeroRework");
        var build = rework.GetMethod("BuildTrees", BindingFlags.Static | BindingFlags.NonPublic);
        var kindType = asm.GetType("HeroKind");
        // tree: stage0: [system, ship], stage4: [system+ship, senate], stage8: [ship], stage12 (ultimate): [system]
        var tree = Tree(
            Stage(0, Slot(Skill("s", 2)), Slot(Skill("h", 2))),
            Stage(4, Slot(Skill("sh", 2)), Slot(Skill("e", 2))),
            Stage(8, Slot(Skill("h", 1))),
            Stage(12, Slot(Skill("s", 1))));
        var trees = Arr("HeroSkillTreeDefinition", 1); trees.SetValue(tree, 0);
        int bad = 0;
        string[] names = { "Admiral", "Governor", "Senator" };
        int[] expectSkills = { 3, 3, 1 };   // admiral: ship, system+ship, ship ; governor: system, system+ship, system (ultimate) ; senator: senate only
        for (int k = 0; k < 3; k++)
        {
            var kind = Enum.ToObject(kindType, k);
            var res = (Array)build.Invoke(null, new object[] { trees, kind });
            int stages; int n = Count(res.GetValue(0), out stages);
            var st = (Array)res.GetValue(0).GetType().GetField("Stages").GetValue(res.GetValue(0));
            var req = new List<int>(); foreach (var s in st) req.Add((int)s.GetType().GetField("RequiredLevel").GetValue(s));
            Console.WriteLine(names[k] + ": skills=" + n + " stages=" + stages + " thresholds=" + string.Join(",", req.ConvertAll(x => x.ToString()).ToArray()));
            if (n != expectSkills[k]) { Console.WriteLine("  UNEXPECTED skill count, wanted " + expectSkills[k]); bad++; }
        }
        // the original must be untouched
        int os; int on = Count(tree, out os);
        if (on != 6 || os != 4) { Console.WriteLine("original tree was modified"); bad++; }
        Console.WriteLine(bad == 0 ? "HERO SMOKE OK" : "HERO SMOKE FAILED");
        return bad;
    }
}

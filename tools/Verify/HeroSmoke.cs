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

    // scaled copies of real SimulationDescriptor / modifier objects
    static int ScalingSmoke(Type rework)
    {
        int bad = 0;
        var helper = asm.GetType("CustomHackingHelper");
        var make = helper.GetMethod("MakeModifier"); var create = helper.GetMethod("CreateAndRegisterDescriptor");
        var ps = make.GetParameters();
        var opType = ps[1].ParameterType;
        object add = Enum.ToObject(opType, 1), mul = Enum.ToObject(opType, 3), force = Enum.ToObject(opType, 0);
        object m1 = make.Invoke(null, new object[] { "SystemProduction", add, 10f, "ClassStarSystem" });
        object m2 = make.Invoke(null, new object[] { "SystemFood", mul, 1.3f, "ClassStarSystem" });
        object m3 = make.Invoke(null, new object[] { "SystemApproval", force, 5f, "ClassStarSystem" });
        var modsType = ps[1].ParameterType; // placeholder to keep the compiler quiet
        var cps = create.GetParameters();
        var arrType = cps[cps.Length - 1].ParameterType;
        var arr = Array.CreateInstance(arrType.GetElementType(), 3);
        arr.SetValue(m1, 0); arr.SetValue(m2, 1); arr.SetValue(m3, 2);
        object src = create.Invoke(null, new object[] { null, "SmokeSkill", "HeroSkillSystem", null, arr });
        var scaled = rework.GetMethod("ScaledCopy", BindingFlags.Static | BindingFlags.NonPublic);
        object copy = scaled.Invoke(null, new object[] { src, 0.5f, 500 });
        if (copy == null) { Console.WriteLine("scaled copy is null"); return 1; }
        var mods = (Array)copy.GetType().GetProperty("SimulationModifierDescriptors").GetValue(copy, null);
        var vals = new List<float>(); var ids = new List<int>();
        foreach (var m in mods)
        {
            vals.Add((float)m.GetType().GetProperty("ValueAsFloat").GetValue(m, null));
            Type t = m.GetType(); while (t.GetField("<ModifierId>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) == null) t = t.BaseType;
            ids.Add((int)t.GetField("<ModifierId>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(m));
        }
        var srcMods = (Array)src.GetType().GetProperty("SimulationModifierDescriptors").GetValue(src, null);
        float srcFirst = (float)srcMods.GetValue(0).GetType().GetProperty("ValueAsFloat").GetValue(srcMods.GetValue(0), null);
        Console.WriteLine("scaled copy: modifiers=" + mods.Length + " values=" + string.Join(",", vals.ConvertAll(x => x.ToString()).ToArray()) + " ids=" + string.Join(",", ids.ConvertAll(x => x.ToString()).ToArray()));
        if (mods.Length != 2) { Console.WriteLine("  expected 2 scaled modifiers (force skipped)"); bad++; }
        else if (Math.Abs(vals[0] - 5f) > 1e-4f || Math.Abs(vals[1] - 1.15f) > 1e-4f) { Console.WriteLine("  wrong scaled values"); bad++; }
        if (srcFirst != 10f) { Console.WriteLine("  source modifier was mutated"); bad++; }
        if (ids.Count == 2 && ids[0] == ids[1]) { Console.WriteLine("  duplicate modifier ids"); bad++; }
        foreach (var sm in srcMods) { Type t = sm.GetType(); while (t.GetField("<ModifierId>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) == null) t = t.BaseType;
            int id = (int)t.GetField("<ModifierId>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sm); if (ids.Contains(id)) { Console.WriteLine("  copy shares an id with the source"); bad++; } }
        object again = scaled.Invoke(null, new object[] { src, 0.5f, 500 });
        if (!ReferenceEquals(copy, again)) { Console.WriteLine("  cache miss"); bad++; }
        string type = copy.GetType().GetProperty("Type").GetValue(copy, null).ToString();
        if (type != "HeroSkillSystem") { Console.WriteLine("  copy type " + type); bad++; }
        return bad;
    }

    // level cap rule: a clone with LevelMax, the original is untouched
    static int CapSmoke(Type rework)
    {
        var ruleType = asm.GetType("LevelUpRuleDefinition");
        object rule = Activator.CreateInstance(ruleType, true);
        ruleType.GetProperty("LevelMax").SetValue(rule, 60, null);
        var clone = rework.GetMethod("CloneObject", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { rule });
        ruleType.GetProperty("LevelMax").SetValue(clone, 8, null);
        int a = (int)ruleType.GetProperty("LevelMax").GetValue(rule, null), b = (int)ruleType.GetProperty("LevelMax").GetValue(clone, null);
        bool canAt7 = (bool)ruleType.GetMethod("CanLevelUp").Invoke(clone, new object[] { 7 }), canAt8 = (bool)ruleType.GetMethod("CanLevelUp").Invoke(clone, new object[] { 8 });
        Console.WriteLine("cap clone: original=" + a + " clone=" + b + " canLevelUp(7)=" + canAt7 + " canLevelUp(8)=" + canAt8);
        int bad = 0;
        if (a != 60 || b != 8 || ReferenceEquals(rule, clone)) { Console.WriteLine("  clone is not independent"); bad++; }
        if (!canAt7 || canAt8) { Console.WriteLine("  cap does not stop level-ups at the cap"); bad++; }
        return bad;
    }


    // ---- stage 4: content generation on a synthetic vanilla database
    static object Db(string type)
    {
        var databases = T("Amplitude.Unity.Framework.Databases");
        var m = databases.GetMethod("GetDatabase").MakeGenericMethod(T(type));
        return m.Invoke(null, new object[] { true });
    }
    static void DbAdd(object db, object item) { db.GetType().GetMethod("Add").Invoke(db, new object[] { item }); }
    static bool DbHas(object db, string name) { return (bool)db.GetType().GetMethod("ContainsKey").Invoke(db, new object[] { Activator.CreateInstance(T("Amplitude.StaticString"), new object[] { name }) }); }
    static object DbGet(object db, string name)
    {
        var args = new object[] { Activator.CreateInstance(T("Amplitude.StaticString"), new object[] { name }), null };
        db.GetType().GetMethod("TryGetValue").Invoke(db, args);
        return args[1];
    }

    static object MakeDesc(string name, string type, string prop, int op, float value)
    {
        var helper = asm.GetType("CustomHackingHelper");
        var make = helper.GetMethod("MakeModifier"); var create = helper.GetMethod("CreateAndRegisterDescriptor");
        var opType = make.GetParameters()[1].ParameterType;
        object m = make.Invoke(null, new object[] { prop, Enum.ToObject(opType, op), value, "ClassX" });
        var cps = create.GetParameters();
        var arr = Array.CreateInstance(cps[cps.Length - 1].ParameterType.GetElementType(), 1);
        arr.SetValue(m, 0);
        return create.Invoke(null, new object[] { null, name, type, null, arr });
    }

    static object SynthSkill(string name, int levels, string channel, string prop, int op, float v1, float v2)
    {
        var def = Make("HeroSkillDefinition");
        var lt = T("HeroSkillDefinition+HeroSkillLevelDefinition");
        var arr = Array.CreateInstance(lt, levels);
        for (int i = 0; i < levels; i++)
        {
            var l = Activator.CreateInstance(lt, true);
            string field = channel == "system" ? "systemDescriptors" : channel == "ship" ? "shipDescriptors" : "senateDescriptors";
            string type = channel == "system" ? "HeroSkillSystem" : channel == "ship" ? "HeroSkillShip" : "HeroSkillSenator";
            var da = Arr("Amplitude.Unity.Simulation.SimulationDescriptor", 1);
            da.SetValue(MakeDesc("D_" + name + "_" + i, type, prop, op, i == 0 ? v1 : v2), 0);
            Set(l, field, da);
            var hero = Arr("Amplitude.Unity.Simulation.SimulationDescriptor", i == levels - 1 ? 1 : 0);
            if (i == levels - 1) hero.SetValue(MakeDesc("HeroSkillComplete", "HeroSkill", "CompleteSkill", 1, 1f), 0);
            Set(l, "heroDescriptors", hero);
            arr.SetValue(l, i);
        }
        def.GetType().GetProperty("XmlSerializableName").SetValue(def, name, null);
        def.GetType().GetProperty("SkillLevels").SetValue(def, arr, null);
        return def;
    }

    static Array PropArr(object skill) { return (Array)skill.GetType().GetProperty("SkillLevels").GetValue(skill, null); }

    static string ModsOf(object skill, int level, string field)
    {
        var l = PropArr(skill).GetValue(level);
        var descs = (Array)l.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(l);
        var sb = new System.Text.StringBuilder();
        foreach (var d in descs)
            foreach (var m in (Array)d.GetType().GetProperty("SimulationModifierDescriptors").GetValue(d, null))
                sb.Append(m.GetType().GetProperty("TargetPropertyName").GetValue(m, null)).Append('=').Append(m.GetType().GetProperty("ValueAsFloat").GetValue(m, null)).Append(' ');
        return sb.ToString();
    }

    static int ContentSmoke(Type rework)
    {
        int bad = 0;
        asm.GetType("CouncilGame").GetField("LanguageOverride").SetValue(null, 1);   // the Unity language call cannot run outside the game
        var skillDb = Db("HeroSkillDefinition"); var treeDb = Db("HeroSkillTreeDefinition"); var guiDb = Db("Amplitude.Unity.Gui.GuiElement");
        object[] skills =
        {
            SynthSkill("SkSys", 2, "system", "SystemProduction", 1, 5f, 10f),
            SynthSkill("SkShip", 2, "ship", "ShipDamage", 1, 0.1f, 0.2f),
            SynthSkill("SkSen", 1, "senate", "Happiness", 1, 5f, 5f),
            SynthSkill("SkUlt", 1, "system", "SystemResearch", 1, 40f, 40f)
        };
        string[] names = { "SkSys", "SkShip", "SkSen", "SkUlt" };
        for (int i = 0; i < skills.Length; i++)
        {
            DbAdd(skillDb, skills[i]);
            var gui = Activator.CreateInstance(T("Amplitude.Unity.Gui.GuiElement"));
            gui.GetType().GetProperty("XmlSerializableName").SetValue(gui, names[i], null);
            gui.GetType().GetProperty("Title").SetValue(gui, "%T", null);
            DbAdd(guiDb, gui);
        }
        var gen = Activator.CreateInstance(T("Amplitude.Unity.Gui.GuiElement"));
        gen.GetType().GetProperty("XmlSerializableName").SetValue(gen, "HeroSkillTreeGeneric", null);
        DbAdd(guiDb, gen);
        var tree = Tree(
            Stage(0, Slot(skills[0]), Slot(skills[1])),
            Stage(4, Slot(skills[2])),
            Stage(12, Slot(skills[3])));
        tree.GetType().GetProperty("XmlSerializableName").SetValue(tree, "HeroSkillTreeSophons", null);
        DbAdd(treeDb, tree);

        rework.GetMethod("EnsureContent").Invoke(null, null);
        if (!DbHas(skillDb, "HRContentEnd")) { Console.WriteLine("content: sentinel missing (generation failed)"); return bad + 1; }
        // every written tree: the skills are in the database, the GUI elements follow
        var design = asm.GetType("HeroDesign");
        var allKeys = design.GetMethod("AllKeys");
        var build = design.GetMethod("Build");
        int treesChecked = 0, skillsChecked = 0;
        foreach (char role in new[] { 'A', 'G', 'S', 'D' })
            foreach (string key in (System.Collections.IEnumerable)allKeys.Invoke(null, new object[] { role }))
            {
                object dt = build.Invoke(null, new object[] { key, role });
                if (dt == null) { Console.WriteLine("content: no design for " + key + "|" + role); bad++; continue; }
                treesChecked++;
                string treeName = (string)dt.GetType().GetField("TreeName").GetValue(dt);
                if (!DbHas(guiDb, treeName)) { Console.WriteLine("content: no gui for tree " + treeName); bad++; }
                foreach (object stg in (System.Collections.IEnumerable)dt.GetType().GetField("Stages").GetValue(dt))
                    foreach (object sk in (System.Collections.IEnumerable)stg.GetType().GetField("Skills").GetValue(stg))
                    {
                        string n = (string)sk.GetType().GetField("Name").GetValue(sk);
                        skillsChecked++;
                        if (!DbHas(skillDb, n)) { Console.WriteLine("content: missing skill " + n); bad++; }
                        if (!DbHas(guiDb, n)) { Console.WriteLine("content: no gui for skill " + n); bad++; }
                    }
            }
        Console.WriteLine("written trees: " + treesChecked + ", skills: " + skillsChecked);
        if (treesChecked < 80 || skillsChecked < 400) { Console.WriteLine("  too few written trees"); bad++; }

        // modifiers and levels
        var checks = new object[][]
        {
            new object[] { "HRParty01_1@A", 0, "shipDescriptors", "ShipHullPlatingAbsorptionPercent=0.02", 5 },
            new object[] { "HRParty01_1@A", 4, "shipDescriptors", "ShipHullPlatingAbsorptionPercent=0.1", 5 },
            new object[] { "HRParty01_2@A", 1, "shipDescriptors", "FlatHealthRegeneration=40", 5 },
            new object[] { "HRParty03_1@A", 0, "shipDescriptors", "GarrisonMoneyUpkeep=-0.03", 5 },
            new object[] { "HRParty01_1@G", 0, "systemDescriptors", "SystemProduction", 5 },
            new object[] { "HRParty01_1@S", 0, "senateDescriptors", "", 3 },
            new object[] { "HRParty01_1@D", 4, "senateDescriptors", "", 5 },
        };
        foreach (object[] c in checks)
        {
            object def = DbGet(skillDb, (string)c[0]);
            int lv = PropArr(def).Length;
            string mods = ModsOf(def, (int)c[1], (string)c[2]);
            Console.WriteLine(c[0] + " levels=" + lv + " L" + ((int)c[1] + 1) + " " + c[2] + "[" + mods + "]");
            if (lv != (int)c[4]) { Console.WriteLine("  wrong level count"); bad++; }
            if (!mods.Contains((string)c[3])) { Console.WriteLine("  missing modifier " + c[3]); bad++; }
        }
        // the ultimate of a tree is a single level, complete descriptor only there
        object ult = DbGet(skillDb, "HRRaceSophons_5@A");
        if (PropArr(ult).Length != 1) { Console.WriteLine("  ultimate has more than one level"); bad++; }
        // special ultimates: rewards per destroyed CP, captives, triumph duration
        var rewardType = rework.GetNestedType("SpecialReward");
        var addReward = rework.GetMethod("AddSkillReward");
        var findSkill = design.GetMethod("FindSkill");
        Func<string, int, int, object> reward = (skillName, level, cp) =>
        {
            object r = Activator.CreateInstance(rewardType);
            addReward.Invoke(null, new object[] { r, findSkill.Invoke(null, new object[] { skillName }), level, cp });
            return r;
        };
        object rs = reward("HRRaceSophons_5@A", 1, 7), rc = reward("HRRaceCravers_5@A", 1, 7), rt = reward("HRRaceTerrans_5@A", 1, 25), rn = reward("HRParty01_1@A", 3, 7);
        Func<object, string, int> field = (o, f) => (int)rewardType.GetField(f).GetValue(o);
        if (field(rs, "Science") != 70) { Console.WriteLine("  Sophons ultimate: science " + field(rs, "Science")); bad++; }
        if (field(rc, "Prisoners") != 1) { Console.WriteLine("  Cravers ultimate: prisoners " + field(rc, "Prisoners")); bad++; }
        if (field(rt, "Happiness") != 10) { Console.WriteLine("  Terrans ultimate: happiness " + field(rt, "Happiness")); bad++; }
        if ((bool)rewardType.GetProperty("Any").GetValue(rn, null)) { Console.WriteLine("  an ordinary skill gives a special reward"); bad++; }
        if ((int)rework.GetMethod("EuphoriaTurns").Invoke(null, new object[] { 25 }) != 7 || (int)rework.GetMethod("EuphoriaTurns").Invoke(null, new object[] { 0 }) != 5 || (int)rework.GetMethod("EuphoriaTurns").Invoke(null, new object[] { 100000 }) != 40) { Console.WriteLine("  triumph duration"); bad++; }
        // state round trip (the triumph survives a save)
        var ser = rework.GetMethod("Serialize"); var deser = rework.GetMethod("Deserialize");
        deser.Invoke(null, new object[] { ";~2:50:10,3:44:12" });
        string again = (string)ser.Invoke(null, null);
        if (!again.EndsWith("~2:50:10,3:44:12")) { Console.WriteLine("  triumph state round trip: " + again); bad++; }
        deser.Invoke(null, new object[] { ";" });
        if (((string)ser.Invoke(null, null)).EndsWith(",") || ((string)ser.Invoke(null, null)).Contains("2:50")) { Console.WriteLine("  triumph state not cleared"); bad++; }
        // votes of the Council are read by name (the passive votes live in the senator CLASS tree)
        var votesOf = design.GetMethod("VotesOfSkill");
        object voteSk = null; string voteName = null;
        foreach (string cand in new[] { "HRClassCorporate_5@S", "HRClassCorporate_6@S", "HRClassCorporate_7@S" })
            if (voteSk == null && (int)votesOf.Invoke(null, new object[] { cand, 1 }) > 0) { voteSk = DbGet(skillDb, cand); voteName = cand; }
        if (voteSk == null) { Console.WriteLine("  no votes skill found in the senator class tree"); bad++; }
        else Console.WriteLine("votes skill " + voteName + ": " + votesOf.Invoke(null, new object[] { voteName, 1 }) + " / " + votesOf.Invoke(null, new object[] { voteName, 3 }));
        // the party trees carry no votes any more
        foreach (string cand in new[] { "HRParty03_2@S", "HRParty05_2@S", "HRRaceVenetians_2@S" })
            if ((int)votesOf.Invoke(null, new object[] { cand, 1 }) > 0) { Console.WriteLine("  party/race tree still gives votes: " + cand); bad++; }
        // idempotent
        int before = (int)skillDb.GetType().GetMethod("GetValues").Invoke(skillDb, null).GetType().GetProperty("Length").GetValue(skillDb.GetType().GetMethod("GetValues").Invoke(skillDb, null), null);
        rework.GetMethod("EnsureContent").Invoke(null, null);
        int after = (int)skillDb.GetType().GetMethod("GetValues").Invoke(skillDb, null).GetType().GetProperty("Length").GetValue(skillDb.GetType().GetMethod("GetValues").Invoke(skillDb, null), null);
        if (before != after) { Console.WriteLine("  EnsureContent is not idempotent: " + before + " -> " + after); bad++; }
        Console.WriteLine("content skills registered: " + before);
        return bad;
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
        bad += ScalingSmoke(rework);
        bad += CapSmoke(rework);
        try { bad += ContentSmoke(rework); } catch (Exception e) { Console.WriteLine("content smoke crashed: " + e); bad++; }
        Console.WriteLine(bad == 0 ? "HERO SMOKE OK" : "HERO SMOKE FAILED");
        return bad;
    }
}

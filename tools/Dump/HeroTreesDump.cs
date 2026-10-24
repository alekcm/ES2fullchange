// Dumps every hero skill tree of the game (vanilla data of ES2) and of the hero rework to Markdown.
//   usage: mono HeroTreesDump.exe <es2-data-dir> <out-vanilla.md> <out-mod.md>
// The data dir holds the XML of Endless Space 2 (see tools/fetch_es2_data.sh). The role versions of the vanilla skills are computed
// with the same code as the mod (HeroFlavors.Classify / TargetsOf / ValueOf, copied from HeroContent.RegisterVariant).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

static class HeroTreesDump
{
    sealed class Mod { public string Prop, Op, Value, Path, Left, BinOp, Right; public bool Binary, Hidden; }
    sealed class Level { public string Name; public List<string> Hero = new List<string>(), System = new List<string>(), Ship = new List<string>(), Senate = new List<string>(); public List<string> Masteries = new List<string>(); }
    sealed class Skill { public string Name; public string Tags; public List<Level> Levels = new List<Level>(); }
    sealed class Slot { public string Skill; public List<string> Required = new List<string>(); public List<KeyValuePair<string, string>> Alternatives = new List<KeyValuePair<string, string>>(); }
    sealed class Stage { public int Level; public List<Slot> Slots = new List<Slot>(); }
    sealed class Tree { public string Name; public List<Stage> Stages = new List<Stage>(); }
    sealed class Hero { public string Name, Affinity, Class, Politics, Design; public bool Hidden; public List<string> Skills = new List<string>(), Trees = new List<string>(); }

    static Dictionary<string, string> ru = new Dictionary<string, string>(), en = new Dictionary<string, string>();
    static Dictionary<string, string[]> gui = new Dictionary<string, string[]>();
    static Dictionary<string, List<Mod>> desc = new Dictionary<string, List<Mod>>();
    static Dictionary<string, Skill> skills = new Dictionary<string, Skill>();
    static List<string> skillOrder = new List<string>();
    static List<Tree> trees = new List<Tree>();
    static List<Hero> heroes = new List<Hero>();
    static CultureInfo inv = CultureInfo.InvariantCulture;

    static IEnumerable<XElement> Load(string dir, string file, string element)
    {
        string p = Path.Combine(dir, file);
        if (!File.Exists(p)) return new XElement[0];
        return XDocument.Load(p).Descendants(element);
    }

    static string Clean(string s)
    {
        if (s == null) return "";
        s = Regex.Replace(s, @"\[(\w+?)(Colored)?\]", "$1");
        s = s.Replace("\r", " ").Replace("\n", " ").Replace("|", "/");
        return Regex.Replace(s, @"\s+", " ").Trim();
    }

    static string Loc(Dictionary<string, string> d, string key)
    {
        if (key == null) return null;
        string v;
        return d.TryGetValue(key, out v) ? Clean(v) : null;
    }

    static string TitleOf(string name, bool russian)
    {
        string[] g;
        if (!gui.TryGetValue(name, out g)) return null;
        return Loc(russian ? ru : en, g[0]);
    }

    static string DescOf(string name, bool russian)
    {
        string[] g;
        if (!gui.TryGetValue(name, out g)) return null;
        return Loc(russian ? ru : en, g[1]);
    }

    static string Both(string name)
    {
        string a = TitleOf(name, true), b = TitleOf(name, false);
        if (a == null && b == null) return "";
        return a + (b != null && b != a ? " / " + b : "");
    }

    static string Short(string path)
    {
        if (string.IsNullOrEmpty(path)) return "";
        path = path.Replace("../ClassEmpire", "Empire").Replace("./", "").Replace("Class", "");
        return " @" + path;
    }

    static string Num(string v)
    {
        float f;
        if (!float.TryParse(v, NumberStyles.Float, inv, out f)) return v;
        return f.ToString("0.###", inv);
    }

    static string Fmt(Mod m)
    {
        string path = Short(m.Path);
        string hidden = m.Hidden ? " (hidden)" : "";
        if (m.Binary) return m.Prop + " " + m.Op + " (" + m.Left + " " + m.BinOp + " " + m.Right + ")" + path + hidden;
        float f;
        bool ok = float.TryParse(m.Value, NumberStyles.Float, inv, out f);
        switch (m.Op)
        {
            case "Addition": return m.Prop + " " + (ok && f >= 0 ? "+" : "") + Num(m.Value) + path + hidden;
            case "Subtraction": return m.Prop + " -" + Num(m.Value) + path + hidden;
            case "Multiplication": return m.Prop + " x" + Num(m.Value) + path + hidden;
            case "Percent": return m.Prop + " " + (ok ? (f >= 0 ? "+" : "") + Math.Round(f * 100, 1).ToString(inv) + "%" : m.Value) + path + hidden;
            default: return m.Prop + " " + m.Op + " " + m.Value + path + hidden;
        }
    }

    static List<string> EffectsOf(List<string> descriptors)
    {
        var list = new List<string>();
        foreach (string d in descriptors)
        {
            List<Mod> mods;
            if (d == "HeroSkillComplete") continue;
            if (!desc.TryGetValue(d, out mods)) { list.Add(d + " (descriptor not found)"); continue; }
            foreach (Mod m in mods) if (m.Prop != "CompleteSkill") list.Add(Fmt(m));
        }
        return list;
    }

    static int OpNumber(string op)
    {
        switch (op) { case "Addition": return 1; case "Subtraction": return 2; case "Multiplication": return 3; case "Percent": return 6; }
        return 0;
    }

    // ---------------------------------------------------------------- loading

    static void LoadAll(string dir)
    {
        foreach (string f in new[] { "loc_russian_Locales.xml", "loc_russian_Assets_Locales.xml", "loc_russian_Update4_Locales.xml" })
            foreach (var e in Load(dir, f, "LocalizationPair")) ru[(string)e.Attribute("Name")] = e.Value;
        foreach (string f in new[] { "loc_english_Locales.xml", "loc_english_Assets_Locales.xml", "loc_english_Update4_Locales.xml" })
            foreach (var e in Load(dir, f, "LocalizationPair")) en[(string)e.Attribute("Name")] = e.Value;
        foreach (string f in new[] { "GuiElements[HeroSkills].xml", "GuiElements[Heroes].xml", "GuiElements[Heroes_DLC1].xml", "GuiElements[HeroPolitics].xml", "GuiElements[HeroAffinities].xml", "GuiElements[HeroClasses].xml", "GuiElements[HeroMasteries].xml" })
            foreach (var e in Load(dir, f, "ExtendedGuiElement").Concat(Load(dir, f, "GuiElement")).Concat(Load(dir, f, "HeroGuiElement")))
                gui[(string)e.Attribute("Name")] = new[] { (string)e.Element("Title"), (string)e.Element("Description") };

        foreach (var d in Load(dir, "SimulationDescriptors[HeroSkill].xml", "SimulationDescriptor"))
        {
            var list = new List<Mod>();
            foreach (var m in d.Elements())
            {
                if (m.Name.LocalName != "Modifier" && m.Name.LocalName != "BinaryModifier") continue;
                list.Add(new Mod
                {
                    Prop = (string)m.Attribute("TargetProperty"), Op = (string)m.Attribute("Operation"), Value = (string)m.Attribute("Value"), Path = (string)m.Attribute("Path"),
                    Binary = m.Name.LocalName == "BinaryModifier", Left = (string)m.Attribute("Left"), BinOp = (string)m.Attribute("BinaryOperation"), Right = (string)m.Attribute("Right"),
                    Hidden = (string)m.Attribute("TooltipHidden") == "true"
                });
            }
            desc[(string)d.Attribute("Name")] = list;
        }

        foreach (string f in new[] { "HeroSkillDefinitions.xml", "HeroSkillDefinitions[DLC1].xml", "HeroSkillDefinitions[Update3].xml" })
            foreach (var s in Load(dir, f, "HeroSkillDefinition"))
            {
                var sk = new Skill { Name = (string)s.Attribute("Name"), Tags = (string)s.Element("Tags") };
                foreach (var l in s.Elements("SkillLevel"))
                {
                    var lv = new Level { Name = (string)l.Attribute("Name") };
                    foreach (var r in l.Elements())
                    {
                        string n = (string)r.Attribute("Name");
                        switch (r.Name.LocalName)
                        {
                            case "HeroSimulationDescriptorReference": lv.Hero.Add(n); break;
                            case "SystemSimulationDescriptorReference": lv.System.Add(n); break;
                            case "ShipSimulationDescriptorReference": lv.Ship.Add(n); break;
                            case "SenateSimulationDescriptorReference": lv.Senate.Add(n); break;
                            case "MasteryLevel": lv.Masteries.Add(((string)r.Attribute("Class")).Replace("HeroMastery", "") + " x" + (string)r.Attribute("Levels")); break;
                        }
                    }
                    sk.Levels.Add(lv);
                }
                skills[sk.Name] = sk; skillOrder.Add(sk.Name);
            }

        foreach (var t in Load(dir, "HeroSkillTreeDefinitions.xml", "HeroSkillTreeDefinition"))
        {
            var tree = new Tree { Name = (string)t.Attribute("Name") };
            foreach (var st in t.Elements("Stage"))
            {
                var stage = new Stage { Level = (int)st.Attribute("Level") };
                foreach (var sl in st.Elements("Skill"))
                {
                    var slot = new Slot { Skill = (string)sl.Element("SkillDefinition").Attribute("Name") };
                    foreach (var rq in sl.Elements("RequiredSkill")) slot.Required.Add((string)rq.Attribute("Name"));
                    foreach (var al in sl.Elements("Alternative"))
                        slot.Alternatives.Add(new KeyValuePair<string, string>((string)al.Element("SkillDefinition").Attribute("Name"), (string)al.Element("PathPrerequisite")));
                    stage.Slots.Add(slot);
                }
                tree.Stages.Add(stage);
            }
            trees.Add(tree);
        }

        foreach (string f in new[] { "HeroDefinitions.xml", "HeroDefinitions[DLC1].xml" })
            foreach (var h in Load(dir, f, "HeroDefinition"))
            {
                var hero = new Hero
                {
                    Name = (string)h.Attribute("Name"), Hidden = (string)h.Attribute("Hidden") == "true",
                    Affinity = (string)h.Element("Affinity").Attribute("Name"), Class = (string)h.Element("Class").Attribute("Name"),
                    Politics = (string)h.Element("Politics").Attribute("Name"), Design = (string)h.Element("ShipDesign").Attribute("Name")
                };
                foreach (var s in h.Elements("Skill")) hero.Skills.Add((string)s.Attribute("Name"));
                foreach (var s in h.Elements("SkillTree")) hero.Trees.Add((string)s.Attribute("Name"));
                heroes.Add(hero);
            }
    }

    // ---------------------------------------------------------------- role versions (copy of HeroContent.RegisterVariant)

    sealed class Variant { public string Title, TitleEn; public List<List<string>> Levels = new List<List<string>>(); }

    static void Accumulate(List<string> descriptors, HeroChannel channel, Dictionary<int, float> acc)
    {
        foreach (string d in descriptors)
        {
            List<Mod> mods;
            if (!desc.TryGetValue(d, out mods)) continue;
            foreach (Mod m in mods)
            {
                float v;
                if (m.Binary || !float.TryParse(m.Value, NumberStyles.Float, inv, out v)) continue;
                Flavor flavor; float mag;
                if (!HeroFlavors.Classify(m.Prop, OpNumber(m.Op), v, channel, out flavor, out mag)) continue;
                float old; acc.TryGetValue((int)flavor, out old); acc[(int)flavor] = old + mag;
            }
        }
    }

    static List<Flavor> TopFlavors(Dictionary<int, float> acc, int max)
    {
        var result = new List<Flavor>(); var used = new HashSet<int>(); float best0 = -1f;
        for (int round = 0; round < max; round++)
        {
            int bestKey = -1; float best = 0f;
            foreach (var kv in acc) if (!used.Contains(kv.Key) && kv.Value > best) { best = kv.Value; bestKey = kv.Key; }
            if (bestKey < 0) break;
            if (best0 < 0f) best0 = best;
            if (best < best0 * 0.4f) break;
            used.Add(bestKey); result.Add((Flavor)bestKey);
        }
        return result;
    }

    static List<string> TargetsText(Dictionary<int, float> mags, HeroChannel channel, float power)
    {
        var list = new List<string>();
        foreach (var kv in mags.OrderBy(k => k.Key))
        {
            foreach (FlavorTarget t in HeroFlavors.TargetsOf((Flavor)kv.Key, channel))
            {
                float v = HeroFlavors.ValueOf(t, kv.Value, power);
                if (v == 0f) continue;
                string val = t.Op == 6 ? (v >= 0 ? "+" : "") + Math.Round(v * 100, 1).ToString(inv) + "%" : t.Op == 3 ? "x" + v.ToString(inv) : (v >= 0 ? "+" : "") + v.ToString(inv);
                list.Add(t.Prop + " " + val + (string.IsNullOrEmpty(t.Path) ? "" : " @" + t.Path.Replace("../ClassEmpire", "Empire").Replace("Class", "").Replace("./", "")) + "  [" + (Flavor)kv.Key + "]");
            }
            if ((Flavor)kv.Key == Flavor.Votes && channel == HeroChannel.Senate) list.Add("(Council votes: read by the Council, see HERO_TREES_MOD.md)");
        }
        return list;
    }

    static Variant MakeVariant(Skill src, HeroKind kind, bool dictator)
    {
        HeroChannel channel = HeroFlavors.ChannelOf(kind);
        float power = HeroFlavors.PowerOf(dictator);
        int n = src.Levels.Count;
        int count = n == 0 ? 0 : (kind == HeroKind.Senator && !dictator ? 1 : (dictator && n == 2 ? 3 : n));
        var v = new Variant();
        var finalFlavors = new Dictionary<int, float>();
        for (int j = 0; j < count; j++)
        {
            int sj = (kind == HeroKind.Senator && !dictator) ? n - 1 : Math.Min(j, n - 1);
            Level s = src.Levels[sj];
            float levelScale = (dictator && j >= n) ? 1.5f : 1f;
            float factor = power * levelScale;
            var all = new Dictionary<int, float>(); var converted = new Dictionary<int, float>();
            List<string> own = channel == HeroChannel.System ? s.System : channel == HeroChannel.Senate ? s.Senate : s.Ship;
            Accumulate(own, channel, all);
            if (channel != HeroChannel.Ship) Accumulate(s.Ship, HeroChannel.Ship, converted);
            if (channel != HeroChannel.System) Accumulate(s.System, HeroChannel.System, converted);
            if (channel != HeroChannel.Senate) Accumulate(s.Senate, HeroChannel.Senate, converted);
            if (converted.Count == 0 && all.Count == 0)
            {
                Flavor fallback = Flavor.Production;
                string mastery = s.Masteries.Count > 0 ? "HeroMastery" + s.Masteries[0].Split(' ')[0] : null;
                fallback = HeroFlavors.FlavorOfMastery(mastery, fallback);
                converted[(int)fallback] = (j + 1f) / count;
            }
            foreach (var kv in converted) { float old; all.TryGetValue(kv.Key, out old); all[kv.Key] = old + kv.Value; }
            var convScaled = new Dictionary<int, float>();
            foreach (var kv in converted) convScaled[kv.Key] = kv.Value * levelScale;
            var eff = new List<string>();
            foreach (string e in EffectsOf(own)) eff.Add("kept: " + e + (factor > 1.001f ? "  (x" + factor.ToString(inv) + " for the dictator)" : ""));
            foreach (string e in TargetsText(convScaled, channel, power)) eff.Add("converted: " + e);
            if (s.Hero.Any(h => h != "HeroSkillComplete")) foreach (string e in EffectsOf(s.Hero)) eff.Add("hero: " + e);
            v.Levels.Add(eff);
            if (j == count - 1) finalFlavors = all;
        }
        var top = TopFlavors(finalFlavors, 2);
        v.Title = top.Count == 0 ? "Навык" : HeroFlavors.SkillTitle(top.ToArray(), channel, true);
        v.TitleEn = top.Count == 0 ? "Skill" : HeroFlavors.SkillTitle(top.ToArray(), channel, false);
        return v;
    }

    static readonly HeroKind[] roleKinds = { HeroKind.Admiral, HeroKind.Governor, HeroKind.Senator, HeroKind.Senator };
    static readonly bool[] roleDict = { false, false, false, true };
    static readonly string[] roleNames = { "Адмирал (A)", "Губернатор (G)", "Сенатор (S)", "Диктатор (D)" };

    static int Scale(int required, HeroKind kind, bool dictator)
    {
        if (dictator) return (required * 3 + 1) / 2;
        if (kind == HeroKind.Senator) return required / 2;
        return required;
    }

    // ---------------------------------------------------------------- vanilla document

    static string KindOfClass(string cls)
    {
        HeroKind k;
        return HeroKinds.TryKindOfClass(cls, out k) ? HeroKinds.Name(k, true) : "?";
    }

    static void WriteSkill(StringBuilder sb, string name, string indent)
    {
        Skill sk;
        if (!skills.TryGetValue(name, out sk)) { sb.AppendLine(indent + "- `" + name + "` (definition not found)"); return; }
        sb.AppendLine(indent + "- **`" + name + "`** " + (Both(name).Length > 0 ? "- " + Both(name) : "") + (sk.Tags != null ? "  (tags: " + sk.Tags + ")" : ""));
        string d = DescOf(name, true);
        if (!string.IsNullOrEmpty(d)) sb.AppendLine(indent + "  - описание: " + d);
        for (int i = 0; i < sk.Levels.Count; i++)
        {
            Level l = sk.Levels[i];
            var parts = new List<string>();
            var sy = EffectsOf(l.System); if (sy.Count > 0) parts.Add("система: " + string.Join("; ", sy));
            var sh = EffectsOf(l.Ship); if (sh.Count > 0) parts.Add("флот: " + string.Join("; ", sh));
            var se = EffectsOf(l.Senate); if (se.Count > 0) parts.Add("сенат: " + string.Join("; ", se));
            var he = EffectsOf(l.Hero); if (he.Count > 0) parts.Add("герой: " + string.Join("; ", he));
            sb.AppendLine(indent + "  - ур." + (i + 1) + (l.Masteries.Count > 0 ? " [мастерство " + string.Join(", ", l.Masteries) + "]" : "") + ": " + (parts.Count > 0 ? string.Join(" | ", parts) : "(нет эффектов)"));
        }
    }

    static void WriteVariants(StringBuilder sb, string name, string indent)
    {
        Skill sk;
        if (!skills.TryGetValue(name, out sk) || sk.Levels.Count == 0) return;
        for (int r = 0; r < 4; r++)
        {
            Variant v = MakeVariant(sk, roleKinds[r], roleDict[r]);
            sb.AppendLine(indent + "  - роль " + roleNames[r] + ": «" + v.Title + "» / " + v.TitleEn);
            for (int l = 0; l < v.Levels.Count; l++) sb.AppendLine(indent + "    - ур." + (l + 1) + ": " + (v.Levels[l].Count > 0 ? string.Join("; ", v.Levels[l]) : "(нет)"));
        }
    }

    static void WriteVanilla(string path)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Деревья навыков героев - ванильные данные Endless Space 2 и их роли в моде");
        sb.AppendLine();
        sb.AppendLine("Сгенерировано `tools/Dump/HeroTreesDump.cs` из XML ES2 (репозиторий Endless-Studio, более старая версия игры - в вашей сборке цифры могут отличаться).");
        sb.AppendLine("Названия и описания - из русской локализации. Эффекты - технические имена свойств игры: `Prop +N`, `Prop +N%`, `Prop xN`, `@Путь` - к чему применяется.");
        sb.AppendLine("Роль героя в моде: A - адмирал, G - губернатор, S - сенатор, D - диктатор. «kept» - ванильный эффект своего канала сохранён, «converted» - эффект чужого канала пересчитан в эффект роли (по таблице флейворов `HeroSkillData.cs`).");
        sb.AppendLine("Порог этапа в очках навыков: ваниль 0/4/8/12; адмирал и губернатор так же; сенатор /2 (0/2/4/6); диктатор x1.5 (0/6/12/18). У сенатора один уровень навыка (сильный), у диктатора три (если у ванильного два).");
        sb.AppendLine();
        sb.AppendLine("Какие деревья получает герой (мод): общее дерево («Universal») заменено деревом партии (`HRTreeParty0N@код`, см. HERO_TREES_MOD.md); расовое и классовое деревья - те же слоты, но с навыками роли; диктатор вместо классового дерева получает дерево фракции (`HRTreeDict<Фракция>`).");
        sb.AppendLine();

        sb.AppendLine("## 1. Герои (" + heroes.Count + ")");
        sb.AppendLine();
        sb.AppendLine("| Герой | Имя | Класс -> роль | Фракция (аффинити) | Политика -> партия | Деревья | Личные навыки | Скрыт |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|");
        foreach (Hero h in heroes)
        {
            int party = HeroFlavors.PartyIndex(h.Politics);
            string partyName = party > 0 ? party + " " + HeroFlavors.PartyName(party, true) : "?";
            sb.AppendLine("| `" + h.Name + "` | " + Both(h.Name) + " | " + h.Class.Replace("HeroClass", "") + " -> " + KindOfClass(h.Class) + " | " + h.Affinity.Replace("HeroAffinity", "") + " (" + HeroFlavors.FactionKey(h.Affinity) + ") | " +
                          h.Politics.Replace("HeroPolitics", "") + " -> " + partyName + " | " + string.Join(", ", h.Trees.Select(t => t.Replace("HeroSkillTree", ""))) + " | " + string.Join(", ", h.Skills.Select(s => "`" + s + "`")) + " | " + (h.Hidden ? "да" : "") + " |");
        }
        sb.AppendLine();
        sb.AppendLine("Партии (`Politics`): " + string.Join(", ", Enumerable.Range(1, 6).Select(i => i + " = " + HeroFlavors.PartyName(i, true) + " (" + HeroFlavors.PartyName(i, false) + ")")) + ".");
        sb.AppendLine();

        sb.AppendLine("## 2. Личные навыки героев (`<Skill>` в определении героя, у каждого героя свои два)");
        sb.AppendLine();
        sb.AppendLine("Личные навыки остаются у героя всегда, но в моде их эффекты фильтруются по каналу роли (адмирал - флот, губернатор - система, сенатор/диктатор - сенат); чужие каналы отбрасываются без пересчёта.");
        sb.AppendLine();
        var seenSkills = new HashSet<string>();
        foreach (Hero h in heroes)
        {
            sb.AppendLine("### " + h.Name + " - " + Both(h.Name) + " (" + KindOfClass(h.Class) + ")");
            foreach (string s in h.Skills) if (seenSkills.Add(s) || true) WriteSkill(sb, s, "");
            sb.AppendLine();
        }

        sb.AppendLine("## 3. Деревья (" + trees.Count + ")");
        sb.AppendLine();
        foreach (Tree t in trees)
        {
            var users = heroes.Where(h => h.Trees.Contains(t.Name)).ToList();
            sb.AppendLine("### " + t.Name + " - " + Both(t.Name));
            string td = DescOf(t.Name, true);
            if (!string.IsNullOrEmpty(td)) sb.AppendLine(td);
            sb.AppendLine();
            sb.AppendLine("Героев с этим деревом: " + users.Count + (users.Count > 0 && users.Count <= 20 ? " (" + string.Join(", ", users.Select(u => u.Name)) + ")" : ""));
            sb.AppendLine();
            foreach (Stage st in t.Stages)
            {
                sb.AppendLine("- **Этап, порог " + st.Level + " очк.** (в моде: A/G " + st.Level + ", S " + Scale(st.Level, HeroKind.Senator, false) + ", D " + Scale(st.Level, HeroKind.Senator, true) + ")");
                foreach (Slot sl in st.Slots)
                {
                    WriteSkill(sb, sl.Skill, "  ");
                    WriteVariants(sb, sl.Skill, "  ");
                    if (sl.Required.Count > 0) sb.AppendLine("    - требует навыки: " + string.Join(", ", sl.Required));
                    foreach (var al in sl.Alternatives)
                    {
                        sb.AppendLine("    - **альтернатива** (условие: " + al.Value + "):");
                        WriteSkill(sb, al.Key, "      ");
                        WriteVariants(sb, al.Key, "      ");
                    }
                }
            }
            sb.AppendLine();
        }
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }

    // ---------------------------------------------------------------- mod document

    static void WritePlan(StringBuilder sb, string treeName, string idPrefix, Flavor[] theme, HeroKind kind, bool dictator, bool authority, bool faction, string code)
    {
        HeroChannel channel = HeroFlavors.ChannelOf(kind);
        float power = HeroFlavors.PowerOf(dictator);
        var plan = HeroFlavors.PlanTree(theme, kind, dictator, authority);
        sb.AppendLine("##### `" + treeName + "` - роль " + (dictator ? "диктатор" : HeroKinds.Name(kind, true)) + ", канал " + channel + ", мощность x" + power.ToString(inv));
        foreach (PlannedStage st in plan)
        {
            sb.AppendLine("- **Порог " + st.RequiredLevel + " очк.**");
            foreach (PlannedSkill ps in st.Skills)
            {
                string name = idPrefix + ps.Id + (faction ? "" : "@" + code);
                sb.AppendLine("  - **`" + name + "`** «" + HeroFlavors.SkillTitle(ps.Flavors, channel, true) + "» / " + HeroFlavors.SkillTitle(ps.Flavors, channel, false) + "  [" + string.Join("+", ps.Flavors.Select(f => f.ToString())) + "]");
                for (int l = 0; l < ps.Magnitudes.Length; l++)
                {
                    var mags = new Dictionary<int, float>();
                    for (int f = 0; f < ps.Flavors.Length; f++) { float old; mags.TryGetValue((int)ps.Flavors[f], out old); mags[(int)ps.Flavors[f]] = old + ps.Magnitudes[l][f]; }
                    var eff = TargetsText(mags, channel, power);
                    float[] vm = HeroFlavors.VoteMagnitudes(name);
                    string votes = "";
                    if (vm != null && l < vm.Length) votes = "; голоса Совета: +" + Math.Round(vm[l] * HeroFlavors.VotesFlatPerMagnitude) + " (+" + HeroFlavors.Round(vm[l] * HeroFlavors.VotesPerFourSystems) + " за каждые 4 системы)";
                    sb.AppendLine("    - ур." + (l + 1) + " (магнитуда " + string.Join("/", ps.Magnitudes[l].Select(x => (x * power).ToString("0.###", inv))) + "): " + string.Join("; ", eff) + votes);
                }
            }
        }
    }

    static void WriteMod(string path)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Деревья навыков, созданные модом (партии и диктаторы)");
        sb.AppendLine();
        sb.AppendLine("Сгенерировано `tools/Dump/HeroTreesDump.cs` из кода мода (`HeroSkillData.cs`: `PlanTree`, таблицы флейворов). Значения - ровно то, что регистрирует игра.");
        sb.AppendLine();
        sb.AppendLine("Устройство любого такого дерева: 7 навыков, 4 этапа. Этап 0: A, B. Этап 1: C, A+B. Этап 2: B+C, A+C. Этап 3 (ультимейт): A+B+C (у диктатора +счастье и людские резервы). A, B, C - три флейвора темы. Размер: этап x (1.0 / 1.5 / 2.0 / 2.5), один флейвор x1, два x0.7 каждый, три x0.55, пять x0.45. Адмирал/губернатор: 2 уровня (0.5, 1.0 от размера), сенатор: 1 уровень (1.0), диктатор: 3 уровня (0.5, 1.0, 1.5), ультимейт диктатора x1.5; сила диктатора x1.6.");
        sb.AppendLine();
        sb.AppendLine("Флейвор -> эффект (таблицы `ShipTargets` / `SystemTargets` / `SenateTargets`): значение = опорное число x магнитуда x сила. Флейвор `Votes` в сенате не имеет модификатора: голоса читает Совет.");
        sb.AppendLine();
        sb.AppendLine("Порог этапа (очки навыков): A/G 0/4/8/12; сенатор 0/2/4/6; диктатор 0/6/12/18.");
        sb.AppendLine();

        sb.AppendLine("## Опорные эффекты флейворов (магнитуда 1, сила 1)");
        sb.AppendLine();
        sb.AppendLine("| Флейвор | Адмирал (флот) | Губернатор (система) | Сенатор (вся империя) |");
        sb.AppendLine("|---|---|---|---|");
        foreach (Flavor f in Enum.GetValues(typeof(Flavor)))
        {
            Func<HeroChannel, string> cell = ch =>
            {
                var ts = HeroFlavors.TargetsOf(f, ch);
                if (ts.Length == 0) return "- (голоса Совета)";
                return string.Join("<br>", ts.Select(t => t.Prop + " " + (t.Op == 6 ? (t.Ref >= 0 ? "+" : "") + Math.Round(t.Ref * 100, 1).ToString(inv) + "%" : (t.Ref >= 0 ? "+" : "") + t.Ref.ToString(inv)) + (string.IsNullOrEmpty(t.Path) ? "" : " @" + t.Path.Replace("../ClassEmpire", "Empire").Replace("Class", "").Replace("./", ""))));
            };
            sb.AppendLine("| " + f + " (" + HeroFlavors.Noun(f, HeroChannel.Senate, true) + ") | " + cell(HeroChannel.Ship) + " | " + cell(HeroChannel.System) + " | " + cell(HeroChannel.Senate) + " |");
        }
        sb.AppendLine();

        sb.AppendLine("## Партии (6 x 4 роли = 24 дерева)");
        sb.AppendLine();
        string[] codes = { "A", "G", "S", "D" };
        for (int p = 1; p <= 6; p++)
        {
            Flavor[] theme = HeroFlavors.PartyTheme(p);
            sb.AppendLine("### Партия " + p + ": " + HeroFlavors.PartyName(p, true) + " / " + HeroFlavors.PartyName(p, false) + " - тема: " + string.Join(", ", theme.Select(f => f.ToString())));
            sb.AppendLine();
            for (int r = 0; r < 4; r++)
            {
                WritePlan(sb, "HRTreeParty" + p.ToString("00") + "@" + codes[r], "HRParty" + p.ToString("00"), theme, roleKinds[r], roleDict[r], false, false, codes[r]);
                sb.AppendLine();
            }
        }

        sb.AppendLine("## Деревья диктаторов фракций (" + HeroFlavors.FactionKeys.Length + ")");
        sb.AppendLine();
        foreach (string key in HeroFlavors.FactionKeys)
        {
            Flavor[] theme = HeroFlavors.FactionTheme(key);
            sb.AppendLine("### " + HeroFlavors.FactionName(key, true) + " / " + HeroFlavors.FactionName(key, false) + " (`HRTreeDict" + key + "`) - тема: " + string.Join(", ", theme.Select(f => f.ToString())));
            sb.AppendLine();
            WritePlan(sb, "HRTreeDict" + key, "HRDict" + key, theme, HeroKind.Senator, true, true, true, "D");
            sb.AppendLine();
        }
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }

    static int Main(string[] args)
    {
        if (args.Length < 3) { Console.WriteLine("usage: HeroTreesDump <es2-data-dir> <out-vanilla.md> <out-mod.md>"); return 1; }
        LoadAll(args[0]);
        Console.WriteLine("heroes " + heroes.Count + ", trees " + trees.Count + ", skills " + skills.Count + ", descriptors " + desc.Count + ", ru " + ru.Count + ", en " + en.Count);
        WriteVanilla(args[1]);
        WriteMod(args[2]);
        return 0;
    }
}

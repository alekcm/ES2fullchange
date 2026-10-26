// Battle rework / phase 2 - the ship template catalogue, game glue (the maths and the wording are in
// TemplateCore; the design and the vanilla data facts are in docs/BATTLES_REWORK.md §5).
//
// What this does with the game's own data:
//  * every hull gets one vanilla ShipDesignTemplateDefinition ("<Hull>_<Role>"), whose weapon slots list several
//    families with weights - so "which template" is really "which weapon family", and that is the variant;
//  * we split each vanilla template into one variant template per weapon family that its weapon slots can take,
//    name/describe the variant from the properties of the modules the family actually consists of (TemplateCore),
//    and register the result in the same ShipDesignTemplateDefinition database, so the game's own
//    ShipDesign.AutoDesign / FillShipDesignFromTemplate machinery keeps working and improves them with research;
//  * the player's choice (which variant a design was built from) is remembered per design and travels in the
//    save (BattleCore.Designs), which is what the fleet-hover overlay and the refit order will read.
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Gui;
using Amplitude.Unity.Simulation;
using Amplitude.Unity.Simulation.SimulationModifierDescriptors;

public static class BattleTemplates
{
    // ------------------------------------------------------------------ catalogue

    static readonly List<BattleTemplate> catalog = new List<BattleTemplate>();
    static readonly Dictionary<string, List<int>> byHull = new Dictionary<string, List<int>>(StringComparer.Ordinal);
    static readonly Dictionary<string, TemplateModuleInfo> familyInfo = new Dictionary<string, TemplateModuleInfo>(StringComparer.Ordinal);
    static readonly Dictionary<string, ShipDesignTemplateDefinition> registered = new Dictionary<string, ShipDesignTemplateDefinition>(StringComparer.Ordinal);
    static bool built;
    static string status = "";

    public static int Count { get { return catalog.Count; } }
    public static string Status { get { return status; } }

    public static List<BattleTemplate> Catalog { get { return catalog; } }

    /// <summary>Build the catalogue from the live databases (idempotent; safe to call every turn).</summary>
    public static void BuildCatalog()
    {
        TemplateCore.SetLanguageProvider(CouncilGame.IsRussian);
        if (built) return;
        try
        {
            IDatabase<ShipDesignTemplateDefinition> tplDb = Databases.GetDatabase<ShipDesignTemplateDefinition>(false);
            IDatabase<HullDefinition> hullDb = Databases.GetDatabase<HullDefinition>(false);
            IDatabase<ModuleDefinition> modDb = Databases.GetDatabase<ModuleDefinition>(false);
            if (tplDb == null || hullDb == null || modDb == null) { status = "no databases"; return; }

            // 1. the weapon families the game's own templates use, and what their modules can do
            var families = new Dictionary<string, List<string>>(StringComparer.Ordinal);     // family -> module names
            ModuleDefinition[] all = modDb.GetValues();
            if (all != null)
                for (int i = 0; i < all.Length; i++)
                {
                    var mod = all[i];
                    if (mod == null || mod.Category != ModuleCategory.Weapon || StaticString.IsNullOrEmpty(mod.Family)) continue;
                    if (mod.AIBattleSituations == null || mod.AIBattleSituations.Length == 0) continue;   // not a real gun (fighters, AOE...)
                    List<string> list;
                    string fam = mod.Family.ToString();
                    if (!families.TryGetValue(fam, out list)) { list = new List<string>(); families[fam] = list; }
                    list.Add(mod.Name.ToString());
                }
            foreach (var kv in families)
            {
                TemplateModuleInfo info = ReadFamily(modDb, kv.Value);
                if (info != null) familyInfo[kv.Key] = info;
            }
            if (familyInfo.Count == 0) { status = "no weapon families"; return; }

            // 2. one variant per (vanilla template, weapon family)
            ShipDesignTemplateDefinition[] templates = tplDb.GetValues();
            for (int i = 0; templates != null && i < templates.Length; i++)
            {
                var parent = templates[i];
                if (parent == null || parent.TemplateSlotBindings == null) continue;
                string hull = parent.HullName.ToString();
                string role = parent.ShipRoleName.ToString();
                if (string.IsNullOrEmpty(hull)) continue;
                foreach (var fam in WeaponFamiliesOf(parent, familyInfo))
                {
                    TemplateModuleInfo info = familyInfo[fam];
                    var t = new BattleTemplate
                    {
                        Id = TemplateCore.TemplateId(hull, role, fam),
                        ParentId = parent.Name.ToString(),
                        Hull = hull,
                        Role = role,
                        Family = fam,
                        Info = info,
                        Weight = info.BestLevel
                    };
                    t.Variant = VariantOf(info, hull);
                    t.Description = TemplateCore.Describe(hull, RoleLabel(role), info, CouncilGame.IsRussian());
                    List<int> bucket;
                    string key = hull;
                    if (!byHull.TryGetValue(key, out bucket)) { bucket = new List<int>(); byHull[key] = bucket; }
                    t.VariantIndex = bucket.Count;
                    catalog.Add(t);
                    bucket.Add(catalog.Count - 1);
                }
            }
            built = true;
            status = catalog.Count + " variants / " + byHull.Count + " hulls / " + familyInfo.Count + " families";
        }
        catch (Exception e) { status = "build failed: " + e.GetType().Name + ": " + e.Message; Fail("BuildCatalog", e); }
    }

    /// <summary>The weapon families a vanilla template's weapon slots refer to (in the template's own order).</summary>
    static List<string> WeaponFamiliesOf(ShipDesignTemplateDefinition parent, Dictionary<string, TemplateModuleInfo> known)
    {
        var order = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        ShipDesignTemplateDefinition.TemplateSlotBinding[] bindings = parent.TemplateSlotBindings;
        for (int b = 0; b < bindings.Length; b++)
        {
            ShipDesignTemplateDefinition.TemplateModule[] mods = bindings[b].TemplateModules;
            if (mods == null) continue;
            for (int m = 0; m < mods.Length; m++)
            {
                if (mods[m] == null || StaticString.IsNullOrEmpty(mods[m].Family)) continue;
                string fam = mods[m].Family.ToString();
                if (!known.ContainsKey(fam) || seen.Contains(fam)) continue;
                seen.Add(fam);
                order.Add(fam);
            }
        }
        return order;
    }

    static TemplateVariant VariantOf(TemplateModuleInfo info, string hull)
    {
        string key = TemplateCore.Classify(info);
        var v = new TemplateVariant
        {
            Key = key,
            NameRu = TemplateCore.NameRu(key, hull),
            NameEn = TemplateCore.NameEn(key, hull),
            ProfileRu = TemplateCore.Profile(info),
            ArmorRu = TemplateCore.ArmorRu(info),
            DamageRu = TemplateCore.DamageKindRu(info)
        };
        return v;
    }

    /// <summary>The role's own name, as the game labels it (the localisation of ShipRoleDefinition), fallback: the bare word.</summary>
    static string RoleLabel(string roleName)
    {
        if (string.IsNullOrEmpty(roleName)) return null;
        try
        {
            IDatabase<ShipRoleDefinition> db = Databases.GetDatabase<ShipRoleDefinition>(false);
            ShipRoleDefinition role;
            if (db != null && db.TryGetValue(new StaticString(roleName), out role) && role != null)
            {
                string localized = Localized(role.Name);
                if (!string.IsNullOrEmpty(localized)) return localized.ToLowerInvariant();
            }
        }
        catch (Exception) { }
        string bare = roleName.StartsWith("ShipRole") ? roleName.Substring(8) : roleName;
        return char.ToLowerInvariant(bare[0]) + bare.Substring(1);
    }

    static string Localized(StaticString key)
    {
        try
        {
            string text;
            return CustomHackingHelper.TryLocalizeCustomKey(key.ToString(), out text) ? text : null;
        }
        catch (Exception) { return null; }
    }

    /// <summary>Turn the family's best known module into the facts TemplateCore classifies on. Everything comes
    /// from the game data: the AI range profile, the armour preferences, the flak slave module and the descriptors.</summary>
    static TemplateModuleInfo ReadFamily(IDatabase<ModuleDefinition> modDb, List<string> moduleNames)
    {
        var info = new TemplateModuleInfo();
        ModuleDefinition best = null;
        for (int i = 0; i < moduleNames.Count; i++)
        {
            ModuleDefinition mod;
            if (!modDb.TryGetValue(new StaticString(moduleNames[i]), out mod) || mod == null) continue;
            if (best == null || mod.Level > best.Level) best = mod;
        }
        if (best == null) return null;
        info.Family = StaticString.IsNullOrEmpty(best.Family) ? null : best.Family.ToString();
        info.IsStrategic = info.Family != null && info.Family.IndexOf("Strategic", StringComparison.Ordinal) >= 0;
        info.BestLevel = best.Level;
        ShipDesignTemplateDefinition.TemplateSlotBinding dummy = null;     // keeps the compiler honest about the type above
        if (dummy != null) return null;

        ShipDesignTemplateDefinition.TemplateModule[] nothing = null;
        if (nothing != null) return null;

        AIBattleSituation[] situations = best.AIBattleSituations;
        for (int s = 0; situations != null && s < situations.Length; s++)
        {
            AIBattleSituation sIt = situations[s];
            if (sIt == null || sIt.Situation == null) continue;
            bool against = sIt.Mode == AIBattleSituation.SituationMode.Against;
            switch (sIt.Situation)
            {
                case "Short": if (!against) info.RangeShort = sIt.Value; break;
                case "Medium": if (!against) info.RangeMedium = sIt.Value; break;
                case "Long": if (!against) info.RangeLong = sIt.Value; break;
                case "EnergyDefense": info.VsEnergyArmor = sIt.Value; break;
                case "ProjectileDefense": info.VsProjectileArmor = sIt.Value; break;
                case "Kinetic": if (against) info.VsKinetic = sIt.Value; break;
                case "Missile": if (against) info.VsMissile = sIt.Value; break;
                case "Laser": if (against) info.VsLaser = sIt.Value; break;
                case "Beam": if (against) info.VsBeam = sIt.Value; break;
                case "Fighter":
                case "Bomber": if (against) info.VsFighter = Math.Max(info.VsFighter, sIt.Value); break;
            }
        }
        info.WeaponType = WeaponTypeOf(best);
        info.HasFlak = HasFlakSlave(best);
        ReadModuleNumbers(best, info);
        return info;
    }

    /// <summary>The type of damage the module deals: the game keeps it on the WeaponModule itself.</summary>
    static string WeaponTypeOf(ModuleDefinition mod)
    {
        try
        {
            var prop = mod.GetType().GetProperty("Type");
            if (prop != null)
            {
                object v = prop.GetValue(mod, null);
                if (v != null) return v.ToString();
            }
        }
        catch (Exception) { }
        return null;
    }

    /// <summary>The flak slave module is what makes a gun a barrage screen in the game's own terms.</summary>
    static bool HasFlakSlave(ModuleDefinition mod)
    {
        try
        {
            ModuleDefinition.SlaveModuleDefinition[] slaves = mod.SlaveModuleDefinitions;
            for (int i = 0; slaves != null && i < slaves.Length; i++)
            {
                if (slaves[i] == null) continue;
                string name = slaves[i].Name == null ? null : slaves[i].Name.ToString();
                if (name != null && name.IndexOf("Flack", StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (name != null && name.IndexOf("Flak", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
        }
        catch (Exception) { }
        return false;
    }

    /// <summary>DamagePerSecond and the range penalties, read out of the module's own descriptors (the same walk
    /// HeroContent uses for the hero-flavour detection: SimulationDescriptor.SimulationModifierDescriptors).</summary>
    static void ReadModuleNumbers(ModuleDefinition mod, TemplateModuleInfo info)
    {
        SimulationDescriptor[] descs = mod.Descriptors;
        for (int d = 0; descs != null && d < descs.Length; d++)
        {
            SimulationDescriptor desc = descs[d];
            if (desc == null || desc.SimulationModifierDescriptors == null) continue;
            SimulationModifierDescriptor[] mods = desc.SimulationModifierDescriptors;
            for (int m = 0; m < mods.Length; m++)
            {
                var single = mods[m] as SingleSimulationModifierDescriptor;
                if (single == null || !single.IsValueFloat) continue;
                string prop = single.TargetPropertyName == null ? null : single.TargetPropertyName.ToString();
                if (prop == null) continue;
                switch (prop)
                {
                    case "DamagePerSecond": info.Dps += single.ValueAsFloat; break;
                    case "LongRangePenalty": info.LongPenalty = Math.Max(info.LongPenalty, single.ValueAsFloat); break;
                    case "ShortRangePenalty": info.ShortPenalty = Math.Max(info.ShortPenalty, single.ValueAsFloat); break;
                }
            }
        }
    }

    // ------------------------------------------------------------------ the variants of a hull

    public static List<int> VariantsOf(string hull)
    {
        BuildCatalog();
        List<int> bucket;
        return hull != null && byHull.TryGetValue(hull, out bucket) ? bucket : new List<int>();
    }

    public static BattleTemplate ById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        for (int i = 0; i < catalog.Count; i++)
            if (catalog[i].Id == id) return catalog[i];
        return null;
    }

    /// <summary>The template of a hull whose weapon family the design actually carries (the binding the overlay and
    /// the refit order need: a design was built from a variant, and the variant is what we show).</summary>
    public static BattleTemplate Match(ShipDesign design)
    {
        if (design == null) return null;
        ulong id = IdOf(design);
        BattleTemplate bound = BoundOf(design);
        if (bound != null) return bound;
        BuildCatalog();
        string hull = design.Hull != null ? design.Hull.Name.ToString() : null;
        string family = PrimaryWeaponFamily(design);
        List<int> bucket = VariantsOf(hull);
        for (int i = 0; i < bucket.Count; i++)
        {
            var t = catalog[bucket[i]];
            if (family != null && t.Family == family) return t;
        }
        return bucket.Count > 0 ? catalog[bucket[0]] : null;
    }

    /// <summary>The variants of a hull, in catalogue order (the cycle the player clicks through).</summary>
    public static List<BattleTemplate> Variants(string hull)
    {
        var list = new List<BattleTemplate>();
        List<int> bucket = VariantsOf(hull);
        for (int i = 0; i < bucket.Count; i++) list.Add(catalog[bucket[i]]);
        return list;
    }

    /// <summary>The variant the player last chose for this hull (empty = the vanilla auto-design).</summary>
    public static string ChoiceForHull(string hull)
    {
        if (string.IsNullOrEmpty(hull)) return null;
        string v;
        return BattleRework.Core.HullVariantChoice.TryGetValue(hull, out v) ? v : null;
    }

    public static void SetChoiceForHull(string hull, string templateId)
    {
        if (string.IsNullOrEmpty(hull)) return;
        if (string.IsNullOrEmpty(templateId)) BattleRework.Core.HullVariantChoice.Remove(hull);
        else BattleRework.Core.HullVariantChoice[hull] = templateId;
    }

    // ------------------------------------------------------------------ refitting an existing ship (docs §8)

    /// <summary>Refit one ship to a template variant: the shared design is re-laid-out (that is how the game works -
    /// a design belongs to the empire, not to a ship), and the ship is re-applied through the game's own
    /// DepartmentOfDefense path so that the simulation entity is rebuilt the way a vanilla refit does it.
    /// <paramref name="rememberForHull"/> false = only this ship's design, true = also the default for the hull
    /// (what the player's Auto-design button does). Returns the human result line ("" = nothing to do).</summary>
    public static string RefitShip(Ship ship, BattleTemplate variant, bool rememberForHull)
    {
        try
        {
            if (ship == null || variant == null) return "";
            MajorEmpire player = CouncilGame.LocalEmpire() as MajorEmpire;
            var defense = player != null ? player.GetAgency<DepartmentOfDefense>() : null;
            if (defense == null) return CouncilGame.Tr("нет департамента обороны", "no DepartmentOfDefense");
            ShipDesign design = ship.ShipDesign;
            if (design == null)
            {
                // a ship without a design of its own (vanilla "default" hull): take the empire's design for that hull
                design = BestDesignForHull(defense, variant.Hull);
                if (design == null) return CouncilGame.Tr("нет проекта этого корпуса", "no design of this hull");
            }
            int bound = SetChoice(design, variant.Id, rememberForHull) ? 1 : 0;
            int shared = ShipsUsingDesign(design);
            try { DepartmentOfDefense.ApplyShipDesignToShip(ship, design); }
            catch (Exception e)
            {
                Fail("ApplyShipDesignToShip", e);            // the playtest needs to know which of the two paths the game allows
                try { ship.RetrofitShipDesign(design); }
                catch (Exception e2) { return CouncilGame.Tr("рефит не удался: ", "refit failed: ") + e2.GetType().Name; }
            }
            string what = SafeName(design);
            return what + (CouncilGame.IsRussian() ? ": слотов обновлено " : ": slots refreshed ") + bound
                   + (shared > 1 ? (CouncilGame.IsRussian() ? ", кораблей на этом проекте: " : ", ships on this design: ") + shared : "")
                   + ")";
        }
        catch (Exception e) { Fail("RefitShip", e); return ""; }
    }

    /// <summary>The empire's design that best matches a hull (for ships that have no design of their own, or when
    /// the refit menu needs a candidate to edit).</summary>
    public static ShipDesign BestDesignForHull(DepartmentOfDefense defense, string hull)
    {
        try
        {
            if (defense == null || defense.AvailableShipDesigns == null || string.IsNullOrEmpty(hull)) return null;
            ShipDesign best = null;
            foreach (ShipDesign d in defense.AvailableShipDesigns)
            {
                if (d == null || d.Hull == null) continue;
                if (d.Hull.Name.ToString() != hull) continue;
                if (best == null) { best = d; continue; }
                bool ok = false;
                try { ok = defense.IsShipDesignUpToDate(d) && !defense.IsShipDesignUpToDate(best); } catch (Exception) { }
                if (ok) best = d;
            }
            return best;
        }
        catch (Exception) { return null; }
    }

    /// <summary>How many ships of the player share this design - a refit touches all of them, the player must know.</summary>
    public static int ShipsUsingDesign(ShipDesign design)
    {
        try
        {
            if (design == null) return 0;
            MajorEmpire player = CouncilGame.LocalEmpire() as MajorEmpire;
            var defense = player != null ? player.GetAgency<DepartmentOfDefense>() : null;
            if (defense == null || defense.Fleets == null) return 0;
            int n = 0;
            foreach (Fleet f in defense.Fleets)
            {
                if (f == null || f.Ships == null) continue;
                for (int i = 0; i < f.Ships.Count; i++)
                    if (f.Ships[i] != null && f.Ships[i].ShipDesign == design) n++;
            }
            return n;
        }
        catch (Exception) { return 0; }
    }

    /// <summary>The design's binding from the save, if there is one.</summary>
    public static BattleTemplate BoundOf(ShipDesign design)
    {
        BattleTemplateBinding b = BindingOf(design);
        if (b == null) return null;
        BattleTemplate t = ById(b.TemplateId);
        if (t != null) return t;
        // the catalogue has no such entry (a hull added by a DLC, a renamed family): still show what was stored
        return new BattleTemplate { Id = b.TemplateId, Hull = b.Hull, Role = b.Role, Family = b.WeaponFamily };
    }

    static BattleTemplateBinding BindingOf(ShipDesign design)
    {
        try
        {
            if (design == null) return null;
            ulong id = IdOf(design);
            BattleTemplateBinding b;
            if (id == 0 || !BattleRework.Core.Designs.TryGetValue(id, out b) || b == null || string.IsNullOrEmpty(b.TemplateId)) return null;
            return b;
        }
        catch (Exception) { return null; }
    }

    /// <summary>The weapon family the design is built around: the most common one among its slots.</summary>
    public static string PrimaryWeaponFamily(ShipDesign design)
    {
        try
        {
            if (design == null || design.Layout == null || design.Layout.Slots == null) return null;
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            List<Slot> slots = design.Layout.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                Slot slot = slots[i];
                ModuleDefinition mod = slot != null ? slot.ModuleDefinition : null;
                if (mod == null || StaticString.IsNullOrEmpty(mod.Family) || mod.Category != ModuleCategory.Weapon) continue;
                string fam = mod.Family.ToString();
                int old;
                counts.TryGetValue(fam, out old);
                counts[fam] = old + 1;
            }
            string best = null;
            int bestCount = 0;
            foreach (var kv in counts)
                if (kv.Value > bestCount) { bestCount = kv.Value; best = kv.Key; }
            return best;
        }
        catch (Exception) { return null; }
    }

    // ------------------------------------------------------------------ applying a variant to a design

    /// <summary>Fill the empty slots of the design from the variant: the game's own rule (best available module of
    /// the family by weight x level, only slots that can take it, only what the empire has researched), applied to
    /// the variant's weapon family - which is the one difference to the vanilla template. Returns the number of
    /// slots bound. Everything else in ShipDesign.AutoDesign (upgrades, pattern) is left to the game.</summary>
    public static int FillFromVariant(ShipDesign design, BattleTemplate variant)
    {
        int bound = 0;
        try
        {
            if (design == null || variant == null || design.Layout == null || design.Layout.Slots == null) return 0;
            ShipDesignTemplateDefinition parent = ParentTemplate(variant.ParentId);
            if (parent == null) return 0;
            PrerequisiteContext shipContext = design.Ship != null ? design.Ship.PrerequisiteContext : design.SlotPrerequisitesContext;
            List<Slot> slots = design.Layout.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                Slot slot = slots[i];
                if (slot == null || slot.Definition == null) continue;
                if (slot.ModuleDefinition != null) continue;                       // never touch what the player placed
                SlotDefinition def = slot.Definition;
                ShipDesignTemplateDefinition.TemplateSlotBinding binding = parent.FindSlotBinding(
                    slot.SectionDefinition != null ? slot.SectionDefinition.Name : def.Name, def.Name);
                if (binding == null) continue;
                ModuleDefinition pick = PickModule(def, binding, variant.Family, shipContext);
                if (pick == null) continue;
                slot.BindModule(pick, false, true);
                bound++;
            }
            if (bound > 0) design.Refresh();
        }
        catch (Exception e) { Fail("FillFromVariant", e); }
        return bound;
    }

    static ShipDesignTemplateDefinition ParentTemplate(string name)
    {
        try
        {
            IDatabase<ShipDesignTemplateDefinition> db = Databases.GetDatabase<ShipDesignTemplateDefinition>(false);
            if (db == null || string.IsNullOrEmpty(name)) return null;
            ShipDesignTemplateDefinition def;
            return db.TryGetValue(new StaticString(name), out def) ? def : null;
        }
        catch (Exception) { return null; }
    }

    /// <summary>The vanilla "best available module of the family", restricted to one family: same weight x level
    /// comparison (the template's Weight times the module Level), same research/slot checks.</summary>
    static ModuleDefinition PickModule(SlotDefinition slot, ShipDesignTemplateDefinition.TemplateSlotBinding binding, string family, PrerequisiteContext context)
    {
        ModuleDefinition best = null;
        float bestScore = float.MinValue;
        ShipDesignTemplateDefinition.TemplateModule[] candidates = binding.TemplateModules;
        for (int c = 0; candidates != null && c < candidates.Length; c++)
        {
            if (candidates[c] == null || StaticString.IsNullOrEmpty(candidates[c].Family)) continue;
            string fam = candidates[c].Family.ToString();
            if (family != null && fam != family) continue;
            ModuleDefinition mod = BestResearchedModuleOfFamily(fam, context, slot);
            if (mod == null) continue;
            float score = candidates[c].Weight * mod.Level;
            if (score > bestScore) { bestScore = score; best = mod; }
        }
        return best;
    }

    /// <summary>ShipDesign::TryGetBestAvailableModuleOfFamily, reimplemented because it is not public: the highest
    /// level module of the family whose prerequisites the ship passes and whose slot accepts it.</summary>
    static ModuleDefinition BestResearchedModuleOfFamily(string family, PrerequisiteContext context, SlotDefinition slot)
    {
        try
        {
            IDatabase<ModuleDefinition> db = Databases.GetDatabase<ModuleDefinition>(false);
            if (db == null) return null;
            ModuleDefinition best = null;
            ModuleDefinition[] all = db.GetValues();
            StaticString wanted = new StaticString(family);
            for (int i = 0; all != null && i < all.Length; i++)
            {
                var mod = all[i];
                if (mod == null || mod.Family != wanted) continue;
                if (best != null && mod.Level <= best.Level) continue;
                if (context != null && !PrerequisiteHelper.CheckPrerequisitesAndMetaPrerequisites(context, mod, null, null, new string[0]))
                    continue;
                if (slot != null && !slot.CanModuleBeBound(mod, context)) continue;
                best = mod;
            }
            return best;
        }
        catch (Exception) { return null; }
    }

    // ------------------------------------------------------------------ the player's choice

    /// <summary>The variant a design was built from (empty = "vanilla auto-design"). Written by the design-window
    /// button (Patcher: ShipDesignModalWindow.OnAutoDesignCb) and read by the overlay / the refit order.</summary>
    public static string ChoiceOf(ShipDesign design)
    {
        return BoundOf(design) != null ? BoundOf(design).Id : null;
    }

    /// <summary>Apply a variant to a design: fill the empty slots from it and remember the binding (save-visible).</summary>
    public static bool SetChoice(ShipDesign design, string templateId)
    {
        return SetChoice(design, templateId, true);
    }

    /// <summary>Bind a design to a template. <paramref name="rememberForHull"/> also makes the choice the default
    /// for the whole hull class (that is what the game's Auto-design button replays), which is what the player
    /// pressing a "use this template" button means; the refit order passes false to bind one ship only.</summary>
    public static bool SetChoice(ShipDesign design, string templateId, bool rememberForHull)
    {
        try
        {
            if (design == null) return false;
            ulong id = IdOf(design);
            if (string.IsNullOrEmpty(templateId))
            {
                if (id != 0) BattleRework.Core.Designs.Remove(id);
                return true;
            }
            BattleTemplate variant = ById(templateId);
            if (variant == null) return false;
            if (rememberForHull) SetChoiceForHull(variant.Hull, variant.Id);
            int bound = FillFromVariant(design, variant);
            BattleRework.Core.Designs[id] = new BattleTemplateBinding
            {
                TemplateId = variant.Id,
                Hull = variant.Hull,
                Role = variant.Role,
                WeaponFamily = variant.Family,
                DesignName = SafeName(design)
            };
            return bound >= 0;
        }
        catch (Exception e) { Fail("SetChoice", e); return false; }
    }

    /// <summary>The design-window button: cycle the hull's variants and fill the empty slots from the new one.
    /// Returns the text to show the player (the variant name + description), or null for "nothing to cycle".</summary>
    public static string CycleDesignVariant(ShipDesign design)
    {
        try
        {
            if (design == null || design.Hull == null) return null;
            string hull = design.Hull.Name.ToString();
            List<int> bucket = VariantsOf(hull);
            if (bucket.Count == 0) return null;
            string current = ChoiceOf(design);
            int at = -1;
            for (int i = 0; i < bucket.Count; i++)
                if (catalog[bucket[i]].Id == current) { at = i; break; }
            BattleTemplate next = catalog[bucket[(at + 1) % bucket.Count]];
            if (at < 0)
            {
                // first press on a design that was not built from a template: keep its weapon family if we know it
                string family = PrimaryWeaponFamily(design);
                if (family != null)
                    for (int i = 0; i < bucket.Count; i++)
                        if (catalog[bucket[i]].Family == family) { next = catalog[bucket[i]]; break; }
            }
            SetChoice(design, next.Id);
            return (next.Variant != null ? (CouncilGame.IsRussian() ? next.Variant.NameRu : next.Variant.NameEn) : next.Family)
                   + "\n" + next.Description;
        }
        catch (Exception e) { Fail("CycleDesignVariant", e); return null; }
    }

    /// <summary>Hook of ShipDesignModalWindow.OnAutoDesignCb (inserted at its start): the game's own "Auto design"
    /// button first lays the hull out from the variant the player chose for this hull, then the vanilla body runs
    /// ShipDesign.AutoDesign() and finishes the rest of the slots the way the game always did. Nothing happens when
    /// the design's hull has no variant chosen, so the button keeps its vanilla behaviour.</summary>
    public static void AutoDesignCb(ShipDesignModalWindow window)
    {
        try
        {
            if (window == null) return;
            var panel = Field(window, "shipDesignEditionPanel");
            var gui = panel != null ? Call(panel, "get_GuiShipDesign") : null;
            object designObject = gui != null ? GetProp(gui, "ShipDesign") : null;
            var design = designObject as ShipDesign;
            if (design == null) return;
            string hull = design.Hull != null ? design.Hull.Name.ToString() : null;
            string choice = ChoiceForHull(hull);
            if (string.IsNullOrEmpty(choice)) return;                 // vanilla auto-design
            BattleTemplate variant = ById(choice);
            if (variant == null) return;
            FillFromVariant(design, variant);
            SetChoice(design, variant.Id);                             // remember it on the design (overlay/refit read this)
            RefreshSlots(gui);
        }
        catch (Exception e) { Fail("AutoDesignCb", e); }
    }

    /// <summary>Hook of ShipDesignModalWindow.RefreshButtons: the game's own Auto-design button gets the name of the
    /// template the hull is set to (and its tooltip the variant's short description), so the player sees what the
    /// button will do before he presses it. Without a chosen template the vanilla label is restored.</summary>
    public static void SyncDesignWindow(ShipDesignModalWindow window)
    {
        try
        {
            BuildCatalog();
            if (window == null) return;
            var button = Field(window, "AutoDesignButton") as AgeControlButton;
            if (button == null) return;
            var panel = Field(window, "shipDesignEditionPanel");
            var gui = panel != null ? Call(panel, "get_GuiShipDesign") : null;
            object designObject = gui != null ? GetProp(gui, "ShipDesign") : null;
            var design = designObject as ShipDesign;
            string hull = design != null && design.Hull != null ? design.Hull.Name.ToString() : null;
            BattleTemplate variant = ById(hull != null ? ChoiceForHull(hull) : null);
            SetControlLabel(button, variant == null ? null
                : (CouncilGame.Tr("Автодизайн: ", "Auto design: ") + (CouncilGame.IsRussian() ? variant.Variant.NameRu : variant.Variant.NameEn)));
            var tooltip = Field(window, "AutoDesignButtonTooltip") as AgeTooltip;
            if (tooltip != null) tooltip.Content = variant == null ? string.Empty : variant.Description;
        }
        catch (Exception e) { Fail("SyncDesignWindow", e); }
    }

    static readonly Dictionary<object, string> originalLabels = new Dictionary<object, string>();

    /// <summary>Writes over the label of a vanilla button (and puts the text back when there is nothing to show),
    /// the same two steps CouncilUiNative uses: auto-localization off, then the label text.</summary>
    static void SetControlLabel(AgeControlButton button, string text)
    {
        try
        {
            var label = ((Component)button).GetComponentInChildren<AgePrimitiveLabel>();
            if (label == null) return;
            object key = button;
            string stored;
            if (!originalLabels.TryGetValue(key, out stored)) { originalLabels[key] = label.Text; stored = label.Text; }
            if (!label.DisableAutoLocalize) label.DisableAutoLocalize = true;
            label.Text = text ?? stored ?? string.Empty;
        }
        catch (Exception) { }
    }

    static void RefreshSlots(object gui)
    {
        try
        {
            var slots = GetProp(gui, "GuiSlots");
            if (slots == null) return;
            var list = slots as System.Collections.IList;
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                var gslot = list[i] as GuiSlot;
                if (gslot != null) gslot.ForceRefreshModule();
            }
        }
        catch (Exception) { }
    }

    static object Field(object target, string name)
    {
        if (target == null) return null;
        var f = target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        return f != null ? f.GetValue(target) : null;
    }

    static object Call(object target, string name)
    {
        if (target == null) return null;
        var m = target.GetType().GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        return m != null ? m.Invoke(target, null) : null;
    }

    static object GetProp(object target, string name)
    {
        return target == null ? null : Call(target, "get_" + name);
    }

    /// <summary>The catalogue line for one design (used by the debug block now, by the fleet-hover overlay and the
    /// refit menu later): "Корпус — малый атакующий. Роль — снайпер: ...".</summary>
    public static string DescribeDesign(ShipDesign design)
    {
        BattleTemplate t = Match(design);
        if (t == null) return null;
        string name = t.Variant != null ? (CouncilGame.IsRussian() ? t.Variant.NameRu : t.Variant.NameEn) : t.Family;
        return name + "\n" + t.Description;
    }

    /// <summary>What the fleets of an empire are built from, grouped by template - the text of the hover overlay
    /// (docs §6) and of the refit menu (docs §8). One line per template.</summary>
    public static string SummarizeFleet(Fleet fleet, bool includeOwn)
    {
        try
        {
            if (fleet == null || fleet.Ships == null) return "";
            var perTemplate = new Dictionary<string, int>(StringComparer.Ordinal);
            var shipNames = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 0; i < fleet.Ships.Count; i++)
            {
                Ship ship = fleet.Ships[i];
                if (ship == null) continue;
                ShipDesign design = ship.ShipDesign;
                BattleTemplate t = design != null ? Match(design) : null;
                string id = t != null && !string.IsNullOrEmpty(t.Id) ? t.Id : (design != null ? design.Name.ToString() : "?");
                int old;
                perTemplate.TryGetValue(id, out old);
                perTemplate[id] = old + 1;
                if (!shipNames.ContainsKey(id) && t != null) shipNames[id] = t.Description;
            }
            var sb = new System.Text.StringBuilder();
            foreach (var kv in perTemplate)
            {
                BattleTemplate t = ById(kv.Key);
                string label = t != null && t.Variant != null
                    ? (CouncilGame.IsRussian() ? t.Variant.NameRu : t.Variant.NameEn)
                    : kv.Key;
                sb.Append(kv.Value).Append(" x ").Append(label);
            string description;
                if (includeOwn && shipNames.TryGetValue(kv.Key, out description) && !string.IsNullOrEmpty(description))
                    sb.Append("\n").Append(description);
                sb.Append("\n");
            }
            return sb.ToString().TrimEnd();
        }
        catch (Exception) { return ""; }
    }

    // ------------------------------------------------------------------ native tooltip injection (docs §6)
    // The game's own tooltip of a fleet line (FleetLine.FleetTooltip, an AgeTooltip filled in FleetLine.Refresh)
    // is the place the fleet composition belongs: it is shown when hovering the fleet icon/row on the galaxy map
    // AND in the system panels. We append the template summary to its Content after the vanilla bind - no new
    // widgets, no separate labels: font, layout and show/hide logic stay 100% the game's.

    /// <summary>One line per template: what this garrison (fleet/hangar) is built from. "" when nothing is bound.</summary>
    public static string GarrisonSummary(Garrison garrison, bool withDescriptions)
    {
        try
        {
            if (garrison == null || garrison.Ships == null) return "";
            var perTemplate = new Dictionary<string, int>(StringComparer.Ordinal);
            var names = new Dictionary<string, string>(StringComparer.Ordinal);
            int unbound = 0;
            for (int i = 0; i < garrison.Ships.Count; i++)
            {
                Ship ship = garrison.Ships[i];
                ShipDesign design = ship != null ? ship.ShipDesign : null;
                if (design == null) continue;
                BattleTemplate t = Match(design);
                if (t == null || string.IsNullOrEmpty(t.Id))
                {
                    unbound++;
                    string dn = DesignName(design);
                    if (!string.IsNullOrEmpty(dn)) { Inc(perTemplate, dn); names[dn] = ""; }
                    continue;
                }
                Inc(perTemplate, t.Id);
                names[t.Id] = withDescriptions ? t.Description : null;
            }
            if (perTemplate.Count == 0) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var kv in perTemplate)
            {
                if (sb.Length > 0) sb.Append("\n");
                BattleTemplate t = ById(kv.Key);
                string label = t != null && t.Variant != null
                    ? (CouncilGame.IsRussian() ? t.Variant.NameRu : t.Variant.NameEn)
                    : kv.Key;
                sb.Append(kv.Value).Append("× ").Append(label);
                if (t != null && t.Variant != null && t.Family != null)
                    sb.Append(" [").Append(TemplateCore.FamilyLabel(t.Family)).Append("]");
                string desc = t != null ? names[kv.Key] : null;
                if (!string.IsNullOrEmpty(desc)) sb.Append("\n  ").Append(desc.Replace("\n", "\n  "));
            }
            if (unbound > 0 && perTemplate.Count == unbound)
                sb.Append("\n").Append(CouncilGame.Tr("шаблоны не заданы — нажмите «Автодизайн» в конструкторе",
                                                       "no templates chosen - press Auto design in the design window"));
            return sb.ToString();
        }
        catch (Exception) { return ""; }
    }

    static void Inc(Dictionary<string, int> d, string key)
    {
        int old; d.TryGetValue(key, out old); d[key] = old + 1;
    }

    /// <summary>The line for one ship (God-Mod refit rows and previews): design name + its template variant.</summary>
    public static string ShipLine(Ship ship)
    {
        try
        {
            if (ship == null) return "";
            ShipDesign design = ship.ShipDesign;
            BattleTemplate t = design != null ? Match(design) : null;
            string name = design != null ? DesignName(design) : "";
            if (string.IsNullOrEmpty(name)) name = "?";
            if (t == null || t.Variant == null) return name + " — " + CouncilGame.Tr("без шаблона", "no template");
            return name + " — " + (CouncilGame.IsRussian() ? t.Variant.NameRu : t.Variant.NameEn);
        }
        catch (Exception) { return ""; }
    }

    /// <summary>Player-facing design name: the localized one when it exists (raw internal names like
    /// "ShipDesign#416" must never reach a tooltip).</summary>
    public static string DesignName(ShipDesign design)
    {
        try
        {
            if (design == null) return "";
            string n = null;
            var prop = design.GetType().GetProperty("LocalizedName");
            if (prop != null) n = prop.GetValue(design, null) as string;
            if (string.IsNullOrEmpty(n)) n = design.UserName;
            if (string.IsNullOrEmpty(n) || n.StartsWith("ShipDesign"))
                n = SafeName(design);
            if (string.IsNullOrEmpty(n) || n.StartsWith("ShipDesign")) return "";   // nothing presentable
            return n;
        }
        catch (Exception) { return ""; }
    }

    /// <summary>Hook of FleetLine.Refresh (post-call): appends the composition line to the line's own AgeTooltip.
    /// Idempotent: the marker is checked, so a vanilla rebind simply gets the line re-appended on the next refresh.</summary>
    public static void RefreshFleetLineTooltip(FleetLine line)
    {
        try
        {
            if (line == null) return;
            var tooltip = line.FleetTooltip;
            if (tooltip == null) return;
            var garrison = line.GuiGarrison != null ? line.GuiGarrison.Garrison : null;
            // plain hover: counts only ("2× Крейсер [ракеты]"); Alt: + the variants' descriptions, as before
            string summary = GarrisonSummary(garrison, IsAltDown());
            if (string.IsNullOrEmpty(summary)) return;
            const string marker = "\u2550\u2550\u2550";    // ═ - our blocks are marked, vanilla text never contains this
            string content = tooltip.Content;
            if (content != null && content.IndexOf(marker, StringComparison.Ordinal) >= 0) return;
            string header = CouncilGame.IsRussian() ? "═══ Шаблоны ═══" : "═══ Templates ═══";
            tooltip.Content = (string.IsNullOrEmpty(content) ? "" : content + "\n") + header + "\n" + summary;
        }
        catch (Exception) { }
    }

    /// <summary>Hook of ShipItem.Refresh (post-call): the short help line in the game's own tooltip of a single
    /// ship card (docs §6). Without Alt: attack / level / hull points / movement / manpower + ONE sentence that
    /// describes the ship by what it actually is - the game's own range-accuracy numbers, not the template's
    /// nominal band. With Alt: the same plus the full catalogue entry of the design (the way the fleet block
    /// did it before the player asked for less on plain hover).</summary>
    public static void RefreshShipItemTooltip(ShipItem item)
    {
        try
        {
            if (item == null) return;
            var gui = item.GuiShip;
            if (gui == null || gui.Ship == null) return;
            var at = item.AgeTransform;
            var tooltip = at != null ? at.AgeTooltip : null;
            if (tooltip == null) return;
            string cur = tooltip.Content ?? "";
            if (cur.IndexOf('\u2550') >= 0) return;                 // this bind was already extended
            var sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(cur)) sb.Append(cur).Append('\n');
            string stats = ShipStatsLine(gui);
            if (!string.IsNullOrEmpty(stats)) sb.Append(stats).Append('\n');
            string profile = ShipProfileSentence(gui);
            if (!string.IsNullOrEmpty(profile)) sb.Append(profile).Append('\n');
            if (IsAltDown())
            {
                string full = ShipDesignEntry(gui.Ship);
                if (!string.IsNullOrEmpty(full)) sb.Append(full).Append('\n');
            }
            if (sb.Length == 0) return;
            sb.Append("\u2550");                                     // marker: the extension exists even if all lines were empty
            tooltip.Content = sb.ToString();
        }
        catch (System.Exception) { }
    }

    static string ShipStatsLine(GuiShip gui)
    {
        try
        {
            bool ru = CouncilGame.IsRussian();
            var sb = new System.Text.StringBuilder();
            AppendStat(sb, ru ? "атака " : "atk ", Stat(gui, "ShipStatOffensiveMilitaryPowerProperty"));
            AppendStat(sb, ru ? "защита " : "def ", Stat(gui, "ShipStatDefensiveMilitaryPowerProperty"));
            string level = Localized(gui.Level);
            if (string.IsNullOrEmpty(level)) level = (string)gui.Level;
            AppendStat(sb, "", level);
            AppendStat(sb, ru ? "прочность " : "hull ", Stat(gui, "ShipStatHealthProperty"));
            AppendStat(sb, ru ? "ход " : "move ", Stat(gui, "ShipStatMovementProperty"));
            AppendStat(sb, ru ? "люд. ресурсы " : "manpower ", Stat(gui, "ShipStatManpowerProperty"));
            return sb.ToString();
        }
        catch (System.Exception) { return ""; }
    }
    static void AppendStat(System.Text.StringBuilder sb, string label, string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        if (sb.Length > 0) sb.Append(" \u00b7 ");
        sb.Append(label).Append(value);
    }

    static string Stat(GuiShip gui, string statName)
    {
        try
        {
            var key = new StaticString(statName);
            string v = gui.GetStatValue(key, false);
            return string.IsNullOrEmpty(v) || v == "0" ? v : v;
        }
        catch (System.Exception) { return null; }
    }

    /// <summary>Range profile from the game's own efficiency dictionary (the same numbers the card shows as
    /// "ОБЩАЯ МЕТКОСТЬ 10/50/95"), plus flak/armor traits from the design's modules. THIS is what decides the
    /// one-liner - a ship with 0.1/0.5/0.95 is a brawler no matter which template it was built from.</summary>
    static string ShipProfileSentence(GuiShip gui)
    {
        float l = 0f, m = 0f, s = 0f;
        bool seen = false;
        try
        {
            var eff = gui.GetRangeEfficiencies();
            if (eff != null && eff.Count > 0)
            {
                int idx = 0;
                foreach (var kv in eff)
                {
                    string k = ((string)kv.Key ?? "").ToLowerInvariant();
                    float v = kv.Value;
                    if (k.IndexOf("short") >= 0 || k.IndexOf("close") >= 0 || k.IndexOf("near") >= 0) s = v;
                    else if (k.IndexOf("long") >= 0 || k.IndexOf("far") >= 0) l = v;
                    else if (k.IndexOf("medi") >= 0 || k.IndexOf("mid") >= 0) m = v;
                    else if (eff.Count == 3) { if (idx == 0) l = v; else if (idx == 1) m = v; else s = v; }
                    idx++;
                }
                seen = true;
            }
        }
        catch (System.Exception) { }
        if (!seen) return "";
        bool flak = false;
        string armor = null;
        try
        {
            ShipDesign design = gui.ShipDesign;
            var slots = design != null && design.Layout != null ? design.Layout.Slots : null;
            for (int i = 0; slots != null && i < slots.Count; i++)
            {
                ModuleDefinition mod = slots[i] != null ? slots[i].ModuleDefinition : null;
                if (mod == null) continue;
                string n = mod.Name.ToString();
                if (n.IndexOf("Flack", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("Flak", StringComparison.OrdinalIgnoreCase) >= 0)
                    flak = true;
            }
            string fam = design != null ? PrimaryWeaponFamily(design) : null;
            TemplateModuleInfo info;
            if (fam != null && familyInfo.TryGetValue(fam, out info) && info != null)
            {
                if (info.VsEnergyArmor - info.VsProjectileArmor >= 0.2f) armor = "energy";
                else if (info.VsProjectileArmor - info.VsEnergyArmor >= 0.2f) armor = "projectile";
            }
        }
        catch (System.Exception) { }
        return TemplateCore.ShortProfileText(l, m, s, flak, armor, CouncilGame.IsRussian());
    }

    /// <summary>The full entry (Alt-mode): variant + catalogue description, exactly the text the design window's
    /// button tooltip carries, plus what the design is bound to when no variant exists.</summary>
    static string ShipDesignEntry(Ship ship)
    {
        try
        {
            ShipDesign design = ship != null ? ship.ShipDesign : null;
            if (design == null) return "";
            BattleTemplate t = Match(design);
            if (t == null) return CouncilGame.Tr("проект без шаблона", "design without a template");
            string name = t.Variant != null ? (CouncilGame.IsRussian() ? t.Variant.NameRu : t.Variant.NameEn) : t.Family;
            return name + (string.IsNullOrEmpty(t.Description) ? "" : "\n" + t.Description);
        }
        catch (System.Exception) { return ""; }
    }

    static string Localized(string key)
    {
        try
        {
            if (string.IsNullOrEmpty(key)) return null;
            var svc = Amplitude.Unity.Framework.Services.GetService<Amplitude.Unity.Localization.ILocalizationService>();
            if (svc == null) return null;
            string s = svc.Localize(key, null, null);
            return string.IsNullOrEmpty(s) || s == key ? null : s;
        }
        catch (System.Exception) { return null; }
    }

    static bool IsAltDown()
    {
        try { return UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt) || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightAlt); }
        catch (System.Exception) { return false; }
    }

    /// <summary>Same line for the battle report window's fleet panel tooltip (docs §7).</summary>
    public static string FleetSummaryOf(EncounterGroup group)
    {
        try
        {
            if (group == null) return "";
            var perTemplate = new Dictionary<string, int>(StringComparer.Ordinal);
            var flotillas = group.Flotillas;
            for (int f = 0; flotillas != null && f < flotillas.Length; f++)
            {
                var ships = flotillas[f] != null ? flotillas[f].Ships : null;
                for (int s = 0; ships != null && s < ships.Length; s++)
                {
                    EncounterShip ship = ships[s];
                    if (ship == null) continue;
                    ShipDesign design = null;
                    try { design = ship.ShipDesign; } catch (Exception) { }
                    BattleTemplate t = design != null ? Match(design) : null;
                    string key = t != null && !string.IsNullOrEmpty(t.Id) ? t.Id : (design != null ? DesignName(design) : "");
                    if (string.IsNullOrEmpty(key)) continue;
                    Inc(perTemplate, key);
                }
            }
            if (perTemplate.Count == 0) return "";
            var sb = new System.Text.StringBuilder();
            foreach (var kv in perTemplate)
            {
                if (sb.Length > 0) sb.Append("\n");
                BattleTemplate t = ById(kv.Key);
                string label = t != null && t.Variant != null
                    ? (CouncilGame.IsRussian() ? t.Variant.NameRu : t.Variant.NameEn) : kv.Key;
                sb.Append(kv.Value).Append("\u00d7 ").Append(label);
            }
            return sb.ToString();
        }
        catch (Exception) { return ""; }
    }

    // ------------------------------------------------------------------ registration (the game's auto-design / the AI)

    /// <summary>Register the variant templates in the game's ShipDesignTemplateDefinition database, so the vanilla
    /// FillShipDesignFromTemplate (used by ShipDesign.AutoDesign and by the AI's design generator) can pick them:
    /// a variant is the parent template with every weapon binding reduced to the variant's family. Idempotent.</summary>
    public static int RegisterVariantTemplates()
    {
        int added = 0;
        try
        {
            BuildCatalog();
            IDatabase<ShipDesignTemplateDefinition> db = Databases.GetDatabase<ShipDesignTemplateDefinition>(false);
            if (db == null) return 0;
            for (int i = 0; i < catalog.Count; i++)
            {
                BattleTemplate t = catalog[i];
                if (registered.ContainsKey(t.Id) || db.ContainsKey(new StaticString(t.Id))) continue;
                ShipDesignTemplateDefinition parent = ParentTemplate(t.ParentId);
                if (parent == null) continue;
                var def = new ShipDesignTemplateDefinition();
                def.XmlSerializableName = t.Id;
                def.XmlSerializableHullName = t.Hull;
                def.XmlSerializableShipRoleName = t.Role;
                def.TemplateSlotBindings = BuildBindings(parent, t.Family);
                db.Add(def);
                registered[t.Id] = def;
                added++;
            }
        }
        catch (Exception e) { Fail("RegisterVariantTemplates", e); }
        return added;
    }

    static ShipDesignTemplateDefinition.TemplateSlotBinding[] BuildBindings(ShipDesignTemplateDefinition parent, string family)
    {
        var list = new List<ShipDesignTemplateDefinition.TemplateSlotBinding>();
        ShipDesignTemplateDefinition.TemplateSlotBinding[] bindings = parent.TemplateSlotBindings;
        for (int b = 0; bindings != null && b < bindings.Length; b++)
        {
            ShipDesignTemplateDefinition.TemplateSlotBinding src = bindings[b];
            if (src == null) continue;
            ShipDesignTemplateDefinition.TemplateModule[] mods = src.TemplateModules;
            bool isWeaponSlot = src.SlotName != null && src.SlotName.ToString().IndexOf("Weapon", StringComparison.Ordinal) >= 0;
            var keep = new List<ShipDesignTemplateDefinition.TemplateModule>();
            for (int m = 0; mods != null && m < mods.Length; m++)
            {
                if (mods[m] == null || StaticString.IsNullOrEmpty(mods[m].Family)) continue;
                string fam = mods[m].Family.ToString();
                if (isWeaponSlot && fam != family) continue;                     // the only difference to the parent
                if (!isWeaponSlot || family != null) keep.Add(mods[m]);
            }
            if (keep.Count == 0 && isWeaponSlot) continue;                       // this hull cannot take that family here
            var dst = new ShipDesignTemplateDefinition.TemplateSlotBinding();
            dst.XmlSerializableSlotName = src.SlotName.ToString();
            dst.TemplateModules = keep.ToArray();
            list.Add(dst);
        }
        return list.ToArray();
    }

    // ------------------------------------------------------------------ helpers

    static ulong IdOf(ShipDesign design)
    {
        try { return design == null ? 0UL : design.GUID.ToUInt64Safe(); }
        catch (Exception) { return 0UL; }
    }

    static string SafeName(ShipDesign design)
    {
        try { return string.IsNullOrEmpty(design.UserName) ? design.Name.ToString() : design.UserName; }
        catch (Exception) { return ""; }
    }

    static void Fail(string where, Exception e)
    {
        try { Debug.LogWarning("[BattleTemplates] " + where + ": " + e.GetType().Name + ": " + e.Message); }
        catch (Exception) { }
    }
}

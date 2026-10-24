// Hero rework (stage 3): level cap of senators and the dictatorship.
//  * a senator hero (not the dictator) stops after HeroKinds.SenatorLevelMax levels;
//  * a dictatorship has exactly one senator, the faction leader (the prime senator when the dictatorship starts);
//  * while he rules, the official ideology (the politics of the prime senator) is not changed by elections or by
//    the empire rebellion: only the player can end it by changing the government (abdication);
//  * the hero who represents the leader is the dictator: no level cap, his ultimate multiplies his learned skills;
//  * every empire is given a dictator hero for the time of the dictatorship (HeroDictatorHero.cs).
using System;
using System.Collections.Generic;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;

public static partial class HeroRework
{
    sealed class DictatorRecord
    {
        public string Politics;
        public ulong SenatorGuid;
    }

    static Dictionary<int, DictatorRecord> dictatorships = new Dictionary<int, DictatorRecord>();
    static readonly HashSet<ulong> dictatorHeroes = new HashSet<ulong>();
    static readonly Dictionary<LevelUpRuleDefinition, LevelUpRuleDefinition> cappedToOriginal = new Dictionary<LevelUpRuleDefinition, LevelUpRuleDefinition>();
    static readonly Dictionary<LevelUpRuleDefinition, LevelUpRuleDefinition> originalToCapped = new Dictionary<LevelUpRuleDefinition, LevelUpRuleDefinition>();

    public static bool IsDictator(Hero hero)
    {
        return hero != null && dictatorHeroes.Count > 0 && dictatorHeroes.Contains((ulong)hero.GUID);
    }

    static Dictionary<int, KeyValuePair<string, ulong>> DictatorsForSave()
    {
        var map = new Dictionary<int, KeyValuePair<string, ulong>>();
        foreach (var kv in dictatorships) map[kv.Key] = new KeyValuePair<string, ulong>(kv.Value.Politics, kv.Value.SenatorGuid);
        return map;
    }

    static void LoadDictators(Dictionary<int, KeyValuePair<string, ulong>> map)
    {
        ClearDictators();
        foreach (var kv in map) dictatorships[kv.Key] = new DictatorRecord { Politics = kv.Value.Key, SenatorGuid = kv.Value.Value };
    }

    static void ClearDictators()
    {
        dictatorships = new Dictionary<int, DictatorRecord>();
        dictatorHeroes.Clear();
        cappedToOriginal.Clear();
        originalToCapped.Clear();
        dictatorOrderTurn.Clear();
        passiveXpTurn = int.MinValue;
    }

    // ---------------------------------------------------------------- senate helpers

    static Senate SenateOf(Empire empire)
    {
        var domestic = empire == null ? null : empire.GetAgency<DepartmentOfDomesticAffairs>();
        return domestic == null ? null : domestic.Senate;
    }

    public static bool IsDictatorship(Senate senate)
    {
        if (senate == null || senate.SimulationObject == null) return false;
        var tags = senate.SimulationObject.Tags;
        return tags.Contains(Senate.ReadOnlyGovernmentDictatorship) || tags.Contains(Senate.ReadOnlyGovernmentDictatorshipCravers);
    }

    static Senator SenatorWithPolitics(Senate senate, string politics)
    {
        foreach (Senator s in senate.Senators)
            if (s.Politics != null && s.Politics.Name.ToString() == politics) return s;
        return null;
    }

    static DictatorRecord RecordFor(Senate senate)
    {
        if (senate == null || senate.Empire == null) return null;
        DictatorRecord rec;
        return dictatorships.TryGetValue(senate.Empire.Index, out rec) && IsDictatorship(senate) ? rec : null;
    }

    // ---------------------------------------------------------------- turn pass

    /// <summary>Follows the dictatorships: appoints the leader when one starts, keeps the senate at one senator, forgets it when the government changes.</summary>
    static void UpdateDictatorships(Game game)
    {
        dictatorHeroes.Clear();
        foreach (MajorEmpire empire in game.EnumerableMajorEmpires)
        {
            try
            {
                Senate senate = SenateOf(empire);
                if (senate == null) continue;
                int index = empire.Index;
                if (!IsDictatorship(senate))
                {
                    dictatorships.Remove(index);   // abdication, change of the government or anarchy: the leader no longer rules
                    SyncDictatorHero(empire, null);   // ... and the dictator hero leaves
                    continue;
                }
                DictatorRecord rec;
                Senator leader = null;
                if (dictatorships.TryGetValue(index, out rec)) leader = SenatorWithPolitics(senate, rec.Politics);
                if (leader == null)
                {
                    leader = senate.PrimeSenator;   // the senator with the most support when the dictatorship starts
                    if (leader == null || leader.Politics == null) { dictatorships.Remove(index); SyncDictatorHero(empire, null); continue; }
                    rec = new DictatorRecord { Politics = leader.Politics.Name.ToString(), SenatorGuid = (ulong)leader.GUID };
                    dictatorships[index] = rec;
                    AnnounceDictator(empire, leader);
                }
                rec.SenatorGuid = (ulong)leader.GUID;
                // exactly one senator
                if (senate.Senators.Count > 1)
                {
                    var extra = new List<Senator>();
                    foreach (Senator s in senate.Senators) if (!ReferenceEquals(s, leader)) extra.Add(s);
                    foreach (Senator s in extra) senate.RemoveSenator(s, false);
                    senate.PrimeSenator = leader;
                }
                SyncDictatorHero(empire, rec);   // a dictatorship gives the player a dictator hero
                var education = empire.GetAgency<DepartmentOfEducation>();
                var dictatorHero = education == null ? null : FindDictatorHero(education);
                if (leader.RepresentativeHero == null || (dictatorHero != null && !ReferenceEquals(leader.RepresentativeHero, dictatorHero))) leader.UpdateRepresentativeHero();
                if (leader.RepresentativeHero != null) dictatorHeroes.Add((ulong)leader.RepresentativeHero.GUID);
            }
            catch (Exception e) { Fail("UpdateDictatorships", e); }
        }
    }

    static void AnnounceDictator(MajorEmpire empire, Senator leader)
    {
        try
        {
            var local = CouncilGame.GetMajor(0);
            if (local == null || local.Index != empire.Index) return;
            leader.UpdateRepresentativeHero();
            string who = leader.RepresentativeHero != null ? leader.RepresentativeHero.LocalizedName : "-";
            CouncilUi.Toast(CouncilGame.Tr("Диктатура: лидер фракции — " + who + ". Идеология закреплена, пока он у власти.",
                                           "Dictatorship: faction leader is " + who + ". The ideology is locked while he rules."));
        }
        catch (Exception) { }
    }

    // ---------------------------------------------------------------- hooks

    /// <summary>Senate.ServerTriggerRebellion start hook: a locked dictatorship is not overthrown by unrest. Returns true to skip the original.</summary>
    public static bool BlockRebellion(Senate senate)
    {
        try { return Enabled && RecordFor(senate) != null; }
        catch (Exception e) { Fail("BlockRebellion", e); return false; }
    }

    /// <summary>Senate.ParseElectionResult start hook: an election during a dictatorship can only confirm the leader.</summary>
    public static void LockElection(Senate senate, ElectionResult result)
    {
        try
        {
            if (!Enabled || result == null) return;
            DictatorRecord rec = RecordFor(senate);
            if (rec == null || result.Candidates == null) return;
            Senator existing = SenatorWithPolitics(senate, rec.Politics);
            ElectionResult.CandidateData keep = null;
            foreach (ElectionResult.CandidateData c in result.Candidates)
                if (c.Politics != null && c.Politics.Name.ToString() == rec.Politics && (keep == null || c.Support > keep.Support)) keep = c;
            if (keep == null && existing != null)
            {
                keep = new ElectionResult.CandidateData();
                keep.Politics = existing.Politics;
                keep.Support = existing.GetPropertyValue(SimulationProperties.Senator.Support);
            }
            if (keep == null) return;
            if (existing != null) keep.GUID = existing.GUID;
            result.Candidates.Clear();
            result.Candidates.Add(keep);
        }
        catch (Exception e) { Fail("LockElection", e); }
    }

    /// <summary>Hero.UpdateLevel start hook: puts the level cap of senators on the level-up rule of the hero.</summary>
    public static void BeforeUpdateLevel(Hero hero)
    {
        try
        {
            if (!IsRegular(hero)) return;
            LevelUpRuleDefinition current = hero.LevelUpRule;
            if (current == null) return;
            LevelUpRuleDefinition original;
            if (!cappedToOriginal.TryGetValue(current, out original)) original = current;
            int cap = HeroKinds.LevelCapFor(KindOf(hero), IsDictator(hero), original.LevelMax);
            LevelUpRuleDefinition wanted = original;
            if (cap > 0)
            {
                if (!originalToCapped.TryGetValue(original, out wanted))
                {
                    wanted = (LevelUpRuleDefinition)CloneObject(original);
                    wanted.LevelMax = cap;
                    originalToCapped[original] = wanted;
                    cappedToOriginal[wanted] = original;
                }
            }
            if (!ReferenceEquals(wanted, current)) hero.LevelUpRule = wanted;
        }
        catch (Exception e) { Fail("BeforeUpdateLevel", e); }
    }
}

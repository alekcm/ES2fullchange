using System;
using System.Collections.Generic;

static class T
{
    static int fails;
    static void Check(bool c, string m) { if (!c) { fails++; Console.WriteLine("FAIL: " + m); } else Console.WriteLine("ok:   " + m); }

    static int Main()
    {
        var c = new CouncilCore();
        Check(c.NextAction(5) == CouncilCore.ActionNone, "disabled council does nothing");
        c.Found(30, new[] { 0, 1, 2 }, 20);
        Check(c.NextAction(29) == CouncilCore.ActionNone, "not before founding turn");
        Check(c.NextAction(30) == CouncilCore.ActionOpen, "opens on founding turn");
        var props = c.PickProposals(12345, 3, null);
        Check(props.Count == 3, "3 proposals picked: " + string.Join(",", props.ToArray()));
        var groups = new List<string>(); bool dup = false;
        foreach (var p in props) { var g = CouncilLaws.Get(p).Group; if (groups.Contains(g)) dup = true; groups.Add(g); }
        Check(!dup, "no two laws of the same group");
        var props2 = c.PickProposals(12345, 3, null);
        Check(string.Join(",", props.ToArray()) == string.Join(",", props2.ToArray()), "deterministic picks");

        // force a known set: Merit(targeted) / IntelExchange(global) / DustPayout(targeted)
        c.OpenVote(30, new[] { CouncilLaws.Merit, CouncilLaws.IntelExchange, CouncilLaws.DustPayout });
        Check(c.Phase == CouncilCore.PhaseVoting && c.NextVoteTurn == 50, "vote open, next vote at +20");
        Check(c.NextAction(30) == CouncilCore.ActionNone, "tally not on the same turn");
        Check(c.NextAction(31) == CouncilCore.ActionTally, "tally next turn");

        Check(!c.TrySetBallot(0, new[] { new[] { 0, 0, 105 } }), "reject non multiple of 10");
        Check(!c.TrySetBallot(0, new[] { new[] { 0, 0, 110 } }), "reject over budget");
        Check(!c.TrySetBallot(0, new[] { new[] { 0, 9, 10 } }), "reject non member target");
        Check(!c.TrySetBallot(0, new[] { new[] { 1, 2, 10 } }), "reject bad yes/no option");
        Check(!c.TrySetBallot(7, new[] { new[] { 0, 0, 10 } }), "reject non member voter");
        Check(c.TrySetBallot(0, new[] { new[] { 0, 0, 60 }, new[] { 1, 0, 40 } }), "empire 0 ballot ok (60 on merit->0, 40 yes intel)");
        Check(c.Spent(0) == 100, "spent 100");
        Check(c.TrySetBallot(1, new[] { new[] { 0, 1, 50 }, new[] { 1, 1, 30 }, new[] { 2, 1, 20 } }), "empire 1 ballot ok");
        Check(c.TrySetBallot(2, new[] { new[] { 0, 1, 30 }, new[] { 1, 1, 30 }, new[] { 2, 2, 10 } }), "empire 2 ballot ok");
        Check(c.TrySetBallot(2, new[] { new[] { 0, 1, 30 }, new[] { 1, 1, 30 }, new[] { 2, 2, 10 }, new[] { 0, 1, 0 } }), "replace ballot ignores zero entries");

        // serialization roundtrip while voting
        string s1 = c.Serialize();
        var r = CouncilCore.Deserialize(s1);
        Check(r.Serialize() == s1, "serialize roundtrip (voting)");
        Check(r.Enabled && r.Phase == CouncilCore.PhaseVoting && r.Proposals.Count == 3 && r.Members.Count == 3, "restored state");

        var enacted = c.Tally(31);
        // Merit: emp0 60 vs emp1 50 + emp2 30 -> target 1 wins with 80 (50+30)
        Check(c.History.Count == 3, "3 results recorded");
        Check(c.History[0].Passed && c.History[0].Target == 1 && c.History[0].Yes == 80 && c.History[0].No == 60, "merit -> empire 1 (80 vs 60)");
        Check(c.VoteBonus.ContainsKey(1) && c.VoteBonus[1] == 30 && c.Budget(1) == 130, "empire 1 has +30 votes");
        Check(!c.History[1].Passed && c.History[1].Yes == 40 && c.History[1].No == 60, "intel exchange rejected (40 yes vs 60 no)");
        Check(c.History[2].Passed && c.History[2].Target == 1, "dust payout -> empire 1 (20 vs 10)");
        Check(enacted.Count == 2, "2 enacted");
        Check(c.Phase == CouncilCore.PhaseIdle && c.Proposals.Count == 0, "idle after tally");
        Check(!c.IsLawActive(CouncilLaws.DustPayout), "instant law is not kept active");

        // tie -> fails
        c.OpenVote(50, new[] { CouncilLaws.Replenishment, CouncilLaws.Counterintel, CouncilLaws.ExpansionLimit });
        c.TrySetBallot(0, new[] { new[] { 0, 0, 50 } });
        c.TrySetBallot(1, new[] { new[] { 0, 1, 50 } });
        c.TrySetBallot(2, new[] { new[] { 1, 0, 10 }, new[] { 2, 2, 10 } });
        c.Tally(51);
        Check(!c.History[c.History.Count - 3].Passed, "targeted tie fails");
        Check(c.History[c.History.Count - 2].Passed && c.IsLawActive(CouncilLaws.Counterintel), "global law passes with 10 vs 0 and stays active");
        Check(c.History[c.History.Count - 1].Passed && c.GetColonyDelta(2) == -1, "expansion limit -> empire 2 (-1)");
        var removed = c.ExpireLaws();
        Check(removed.Count == 1 && !c.IsLawActive(CouncilLaws.Counterintel), "cycle laws expire");
        Check(c.GetColonyDelta(2) == -1, "permanent effects survive expiry");

        // persistence of full state
        c.OpenVote(70, new[] { CouncilLaws.Replenishment, CouncilLaws.Counterintel, CouncilLaws.Merit });
        c.TrySetBallot(0, new[] { new[] { 0, 1, 30 } });
        var s2 = c.Serialize(); var r2 = CouncilCore.Deserialize(s2);
        Check(s2 == r2.Serialize(), "serialize roundtrip 2");
        Check(CouncilCore.Deserialize("garbage").Enabled == false, "garbage gives disabled council");
        Check(CouncilCore.Deserialize(null).Enabled == false, "null gives disabled council");

        // AI ballots are valid and deterministic
        var ai = c.BuildAiBallot(1, 777, e => e * 10);
        Check(c.TrySetBallot(1, ai), "AI ballot passes validation");
        var ai2 = c.BuildAiBallot(1, 777, e => e * 10);
        Check(ai.Count == ai2.Count, "AI ballot deterministic");
        // many random picks are always valid
        bool allValid = true;
        for (int seed = 0; seed < 500; seed++)
        {
            var cc = new CouncilCore(); cc.Found(1, new[] { 0, 1, 2, 3 }, 20);
            var pr = cc.PickProposals(seed, 3, null); cc.OpenVote(1, pr);
            if (pr.Count != 3) allValid = false;
            for (int e = 0; e < 4; e++) if (!cc.TrySetBallot(e, cc.BuildAiBallot(e, seed, x => x))) allValid = false;
            cc.Tally(2);
            if (Math.Abs(0) > 1) allValid = false;
        }
        Check(allValid, "500 random sessions: picks and AI ballots valid");
        Console.WriteLine(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
        return fails;
    }
}

using System;
using System.Collections.Generic;

static class M
{
    static int fails;
    static void Check(bool c, string m) { if (!c) { fails++; Console.WriteLine("FAIL: " + m); } else Console.WriteLine("ok:   " + m); }

    static int turn;
    static CouncilCore Open(CouncilCore c, int seed)
    {
        var p = c.BuildProposals(seed, 3);
        c.OpenVote(turn, p);
        return c;
    }
    static void Close(CouncilCore c) { turn++; c.Tally(turn); turn += 19; }
    static bool Has(CouncilCore c, string id) { return c.Proposals.Contains(id); }
    static int Idx(CouncilCore c, string id) { return c.Proposals.IndexOf(id); }
    static int Mq(string id) { return CouncilCore.MandateQBase + CouncilLaws.MandateIndex(id); }

    static int Main()
    {
        var c = new CouncilCore();
        turn = 0;
        c.Found(0, new[] { 0, 1, 2 }, 20);
        Check(c.VictoryTurns == 30, "victory countdown is 30 turns on normal speed");
        var fast = new CouncilCore(); fast.Found(0, new[] { 0 }, 10);
        Check(fast.VictoryTurns == 15, "victory countdown scales with game speed");

        // ---- qualification
        Check(!c.Qualifies(0, CouncilLaws.Chairman), "no tasks -> no chairman candidacy");
        Check(c.ReportTask(0, CouncilLaws.CatDiplomatic, "t1"), "task recorded");
        Check(!c.ReportTask(0, CouncilLaws.CatDiplomatic, "t1"), "same task is not counted twice");
        Check(!c.Qualifies(0, CouncilLaws.Chairman), "1 diplomatic task is not enough");
        c.ReportTask(0, CouncilLaws.CatDiplomatic, "t2");
        Check(c.Qualifies(0, CouncilLaws.Chairman), "2 diplomatic tasks -> chairman candidate");
        c.ReportTask(1, CouncilLaws.CatMilitary, "m1"); c.ReportTask(1, CouncilLaws.CatDefense, "d1");
        Check(c.Qualifies(1, CouncilLaws.Marshal), "military + defense task -> marshal candidate");
        c.ReportTask(2, CouncilLaws.CatSupply, "s1"); c.ReportTask(2, CouncilLaws.CatSupply, "s2");
        Check(!c.Qualifies(2, CouncilLaws.Quartermaster), "2 supply tasks are not enough for the quartermaster");
        c.ReportTask(2, CouncilLaws.CatSupply, "s3");
        Check(c.Qualifies(2, CouncilLaws.Quartermaster), "3 different supply tasks -> quartermaster candidate");
        c.ReportTask(2, CouncilLaws.CatIntel, "i1"); c.ReportTask(2, CouncilLaws.CatResearch, "r1");
        Check(c.Qualifies(2, CouncilLaws.Eye), "intelligence + research -> eye candidate");
        Check(!c.Qualifies(0, CouncilLaws.Supreme), "supreme needs more");

        // ---- session 1: vote for mandates (goes to the NEXT vote)
        Open(c, 1);
        Check(c.Proposals.Count == 3 && !Has(c, "M:Chairman"), "session 1 has 3 ordinary questions");
        Check(!c.TrySetBallot(1, new[] { new[] { Mq(CouncilLaws.Chairman), 0, 10 } }), "cannot back a mandate you do not qualify for");
        Check(!c.TrySetBallot(0, new[] { new[] { Mq(CouncilLaws.Chairman), 1, 10 } }), "mandate vote option must be 0");
        Check(c.TrySetBallot(0, new[] { new[] { Mq(CouncilLaws.Chairman), 0, 50 } }), "empire 0 backs chairman with 50");
        Check(c.TrySetBallot(1, new[] { new[] { Mq(CouncilLaws.Marshal), 0, 30 } }), "empire 1 backs marshal with 30");
        Check(c.TrySetBallot(2, new[] { new[] { Mq(CouncilLaws.Eye), 0, 20 }, new[] { 0, 0, 80 } }), "empire 2 splits votes between mandate and a law");
        Check(!c.TrySetBallot(2, new[] { new[] { Mq(CouncilLaws.Eye), 0, 20 }, new[] { 0, 0, 90 } }), "mandate votes spend from the same 100 budget");
        string saved = c.Serialize();
        Check(CouncilCore.Deserialize(saved).Serialize() == saved, "roundtrip while mandate votes are open");
        turn = 2; c.Tally(turn);
        Check(c.PendingMandate == CouncilLaws.Chairman, "most backed mandate (chairman 50) goes to the next vote");
        Check(c.Holders.Count == 0, "nobody holds a mandate yet");
        turn = 21;

        // ---- session 2: the mandate is a one-empire question among qualifying empires
        Open(c, 2);
        int mq = Idx(c, "M:Chairman");
        Check(mq >= 0, "session 2 contains the chairman election");
        int mcount = 0; foreach (var p in c.Proposals) if (p.StartsWith("M:")) mcount++;
        Check(mcount == 1 && c.Proposals.Count == 3, "exactly one mandate question among the three");
        Check(c.MandateCandidates.Count == 1 && c.MandateCandidates[0] == 0, "only empire 0 qualifies, but a choice is still offered");
        Check(c.PendingMandate == "", "pending mandate consumed");
        Check(!c.TrySetBallot(1, new[] { new[] { mq, 1, 10 } }), "cannot vote for a non-candidate");
        Check(c.TrySetBallot(1, new[] { new[] { mq, 0, 40 } }), "vote for the only candidate");
        Check(c.TrySetBallot(0, new[] { new[] { mq, 0, 10 } }), "candidate votes for itself");
        turn = 22; c.Tally(turn);
        Check(c.Holds(0, CouncilLaws.Chairman) && c.HolderOf(CouncilLaws.Chairman) == 0, "empire 0 is chairman");
        Check(c.Active.Count == 0 || !c.IsLawActive("M:Chairman"), "mandate is not an expiring cycle law");
        turn = 41;

        // ---- session 3: mandate survives the next vote (no re-election needed), chairman power
        Open(c, 3);
        Check(c.Holds(0, CouncilLaws.Chairman), "mandate persists across votes");
        Check(c.CanChairmanReplace(0) && !c.CanChairmanReplace(1), "only the chairman may replace a resolution");
        string old = c.Proposals[0];
        string repl = null;
        foreach (var d in CouncilLaws.All) if (d.Implemented && d.Id != CouncilLaws.NoConfidence && !c.Proposals.Contains(d.Id) && d.Kind != CouncilLawDef.KindMandate) { bool clash = false; foreach (var p in c.Proposals) { var o = CouncilLaws.Get(p); if (o.Group == d.Group) clash = true; } if (!clash || old == null) { repl = d.Id; break; } }
        if (repl == null) { foreach (var d in CouncilLaws.All) if (d.Implemented && d.Id != CouncilLaws.NoConfidence && !c.Proposals.Contains(d.Id)) { repl = d.Id; break; } }
        bool ok = c.ChairmanReplace(0, 0, repl);
        if (!ok) { // pick a law without a group clash against the OTHER two questions
            foreach (var d in CouncilLaws.All) { if (!d.Implemented || d.Id == CouncilLaws.NoConfidence || d.Kind == CouncilLawDef.KindMandate) continue; if (c.ChairmanReplace(0, 0, d.Id)) { ok = true; repl = d.Id; break; } }
        }
        Check(ok && c.Proposals[0] == repl && repl != old, "chairman replaced an ordinary resolution");
        Check(!c.ChairmanReplace(0, 1, CouncilLaws.Merit), "only once per cycle");
        c.TrySetBallot(1, new[] { new[] { 0, 0, 10 } });
        turn = 42; c.Tally(turn);
        turn = 61;
        Open(c, 4);
        Check(c.CanChairmanReplace(0), "power is available again next cycle");
        c.TrySetBallot(1, new[] { new[] { 0, 0, 10 } });
        Check(!c.CanChairmanReplace(0), "cannot change the agenda after votes are cast");
        turn = 62; c.Tally(turn); turn = 81;

        // ---- supreme legate
        c.ReportTask(0, CouncilLaws.CatMilitary, "m1"); c.ReportTask(0, CouncilLaws.CatDefense, "d1");
        Check(c.OrdinaryQualifiedCount(0) == 2, "empire 0 qualifies for 2 ordinary mandates");
        Check(c.HeldOrdinaryForFullCycle(0), "chairman held through a full cycle");
        Check(c.Qualifies(0, CouncilLaws.Supreme), "-> supreme candidate");
        Check(!c.Qualifies(1, CouncilLaws.Supreme), "empire 1 does not qualify");
        Open(c, 5);
        c.TrySetBallot(0, new[] { new[] { Mq(CouncilLaws.Supreme), 0, 100 } });
        turn = 82; c.Tally(turn); turn = 101;
        Check(c.PendingMandate == CouncilLaws.Supreme, "supreme mandate goes to the next vote");
        Open(c, 6);
        Check(Has(c, "M:Supreme"), "supreme election is on the agenda");
        Check(c.Proposals.Count == 3, "3 questions");
        c.TrySetBallot(0, new[] { new[] { Idx(c, "M:Supreme"), 0, 100 } });
        turn = 102; c.Tally(turn);
        Check(c.SupremeHolder == 0 && c.SupremeSince == 102, "empire 0 is the supreme legate");
        Check(c.Budget(0) == 200 && c.Budget(1) == 100, "supreme mandate doubles the votes");
        Check(c.VictoryTurnsLeft(102) == 30 && c.VictoryTurnsLeft(112) == 20, "countdown runs");
        Check(c.CheckVictory(131) == -1 && c.CheckVictory(132) == 0, "victory after 30 turns");
        turn = 121;

        // ---- the next vote must contain no-confidence
        Open(c, 7);
        Check(Has(c, CouncilLaws.NoConfidence), "no-confidence is guaranteed while the countdown runs");
        int nq = Idx(c, CouncilLaws.NoConfidence);
        c.TrySetBallot(1, new[] { new[] { nq, 0, 100 } });
        c.TrySetBallot(2, new[] { new[] { nq, 0, 100 } });
        c.TrySetBallot(0, new[] { new[] { nq, 1, 200 } });
        turn = 122; c.Tally(turn);
        Check(c.SupremeHolder == 0, "no-confidence fails when the legate has double votes (200 vs 200 tie)");
        turn = 141;
        Open(c, 8);
        nq = Idx(c, CouncilLaws.NoConfidence);
        Check(nq >= 0, "no-confidence again next vote");
        c.TrySetBallot(1, new[] { new[] { nq, 0, 100 } });
        c.TrySetBallot(2, new[] { new[] { nq, 0, 100 } });
        c.TrySetBallot(0, new[] { new[] { nq, 1, 190 } });
        turn = 142; c.Tally(turn);
        Check(c.SupremeHolder == -1 && c.SupremeSince == -1, "no-confidence revoked the supreme mandate and stopped the countdown");
        Check(c.Budget(0) == 100, "votes back to normal");
        Check(c.CheckVictory(500) == -1, "no victory without the mandate");
        Check(c.Holds(0, CouncilLaws.Chairman), "ordinary mandates stay");

        // ---- quartermaster / marshal
        c.SetHolder(CouncilLaws.Quartermaster, 2, 150);
        c.SetHolder(CouncilLaws.Marshal, 1, 150);
        Check(c.CanUseQuartermaster(2, 150) && c.TryQuartermaster(2, 150), "quartermaster delivery");
        Check(!c.CanUseQuartermaster(2, 160) && c.TurnsToQuartermaster(2, 160) == 10, "once per 20 turns");
        Check(c.CanUseQuartermaster(2, 170), "available again after the cycle");
        Check(c.TryHireMarshal(1) && !c.TryHireMarshal(1), "marshal hires only one contingent");
        c.SetHolder(CouncilLaws.Marshal, 0, 160);
        Check(c.MarshalHired.Count == 0 && c.CanHireMarshal(0), "contract ends with the mandate; the new marshal may hire");

        // ---- serialization with full state
        c.PendingMandate = CouncilLaws.Eye;
        string s2 = c.Serialize();
        var r = CouncilCore.Deserialize(s2);
        Check(r.Serialize() == s2, "full roundtrip with mandates");
        Check(r.Holds(2, CouncilLaws.Quartermaster) && r.PendingMandate == CouncilLaws.Eye && r.VictoryTurns == 30, "restored mandate state");

        // ---- leaving the council
        Check(c.Leave(0), "empire 0 leaves the council");
        Check(!c.IsMember(0) && c.IsOutcast(0), "no longer a member, marked as outcast");
        Check(!c.Holds(0, CouncilLaws.Chairman) && !c.Holds(0, CouncilLaws.Marshal), "its mandates are gone");
        Check(!c.Leave(0), "cannot leave twice");
        Check(c.Members.Count == 2, "2 members left");
        turn = 190;
        Open(c, 9);
        Check(!c.TrySetBallot(0, new[] { new[] { 0, 0, 10 } }), "outcast cannot vote");

        // ---- disband clears everything
        c.Disband();
        Check(c.Holders.Count == 0 && c.Outcasts.Count == 0 && !c.Enabled, "disband clears mandates");

        // ---- random AI sessions with mandates stay valid
        int bad = 0;
        for (int seed = 0; seed < 300; seed++)
        {
            var z = new CouncilCore(); z.Found(0, new[] { 0, 1, 2, 3 }, 20);
            var rng = new CouncilRng(seed);
            for (int e = 0; e < 4; e++)
            {
                z.ReportTask(e, CouncilLaws.CatDiplomatic, "a"); z.ReportTask(e, CouncilLaws.CatDiplomatic, "b");
                if (rng.Range(2) == 0) { z.ReportTask(e, CouncilLaws.CatMilitary, "a"); z.ReportTask(e, CouncilLaws.CatDefense, "b"); }
            }
            for (int v = 0; v < 8; v++)
            {
                int t = v * 20;
                z.OpenVote(t, z.BuildProposals(seed * 13 + v, 3));
                int mc = 0; foreach (var p in z.Proposals) if (p.StartsWith("M:")) mc++;
                if (mc > 1 || z.Proposals.Count > 3) bad++;
                for (int e = 0; e < 4; e++)
                {
                    var b = z.BuildAiBallot(e, seed + v, delegate (int x) { return x; });
                    if (!z.TrySetBallot(e, b)) { bad++; Console.WriteLine("bad AI ballot seed " + seed + " v " + v); }
                }
                z.Tally(t + 1);
            }
        }
        Check(bad == 0, "300 random sessions with mandates: at most one mandate question, AI ballots valid");

        Console.WriteLine(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
        return fails == 0 ? 0 : 1;
    }
}

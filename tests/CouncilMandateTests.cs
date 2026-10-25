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
        Check(!c.Qualifies(0, CouncilLaws.Chairman), "no achievements -> no chairman candidacy");
        Check(c.ReportTask(0, CouncilLaws.CatMastery, "H1"), "achievement recorded");
        Check(!c.ReportTask(0, CouncilLaws.CatMastery, "H1"), "the same hero is not counted twice");
        Check(c.HasTask(0, CouncilLaws.CatMastery, "H1") && !c.HasTask(0, CouncilLaws.CatMastery, "H2"), "HasTask");
        for (int h = 2; h <= 4; h++) c.ReportTask(0, CouncilLaws.CatMastery, "H" + h);
        Check(!c.Qualifies(0, CouncilLaws.Chairman), "4 heroes of the Mastery rank are not enough");
        c.ReportTask(0, CouncilLaws.CatMastery, "H5");
        Check(c.Qualifies(0, CouncilLaws.Chairman), "5 heroes of the Mastery rank -> chairman candidate");
        Check(!c.Qualifies(1, CouncilLaws.Marshal), "no captured capital -> no marshal candidacy");
        c.ReportTask(1, CouncilLaws.CatCapital, "any");
        Check(c.Qualifies(1, CouncilLaws.Marshal), "captured capital -> marshal candidate");
        Check(!c.Qualifies(2, CouncilLaws.Quartermaster), "no level-4 system -> no quartermaster candidacy");
        c.ReportTask(2, CouncilLaws.CatSystemLevel, "L4");
        Check(c.Qualifies(2, CouncilLaws.Quartermaster), "level-4 system -> quartermaster candidate");
        Check(!c.Qualifies(2, CouncilLaws.Eye), "unexplored galaxy -> no eye candidacy");
        c.ReportTask(2, CouncilLaws.CatExplore, "75");
        Check(c.Qualifies(2, CouncilLaws.Eye), "75% explored -> eye candidate");
        Check(!c.Qualifies(1, CouncilLaws.Chairman) && !c.Qualifies(0, CouncilLaws.Eye), "conditions are per empire and per mandate");
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
        c.ReportTask(0, CouncilLaws.CatCapital, "any");
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
        Check(c.Budget(0) == 250 && c.Budget(1) == 100, "supreme mandate: 100 + 50 (chairman) + 100, nothing is doubled");
        Check(c.VictoryTurnsLeft(102) == 30 && c.VictoryTurnsLeft(112) == 20, "countdown runs");
        Check(c.CheckVictory(131) == -1 && c.CheckVictory(132) == 0, "victory after 30 turns");
        turn = 121;

        // ---- the next vote must contain the confirmation of the mandate ("yes" keeps it: 51%, 67% for an allied holder)
        Check(c.KeepPercentNeeded == 51 && c.MandateKept(51, 49) && !c.MandateKept(50, 50) && c.MandateKept(0, 0) && !c.MandateKept(0, 10), "solo: 51% to keep");
        c.SupremeInAlliance = true;
        Check(c.KeepPercentNeeded == 67 && c.MandateKept(67, 33) && !c.MandateKept(66, 34) && !c.MandateKept(400, 200) && c.MandateKept(410, 200), "alliance: 67% to keep (the others need 34% to remove)");
        c.SupremeInAlliance = false;
        Open(c, 7);
        Check(Has(c, CouncilLaws.NoConfidence), "the confirmation is guaranteed while the countdown runs");
        int nq = Idx(c, CouncilLaws.NoConfidence);
        c.TrySetBallot(1, new[] { new[] { nq, 1, 100 } });
        c.TrySetBallot(2, new[] { new[] { nq, 1, 100 } });
        c.TrySetBallot(0, new[] { new[] { nq, 0, 250 } });
        turn = 122; c.Tally(turn);
        Check(c.SupremeHolder == 0 && c.History[c.History.Count - 3 + nq].Passed, "solo: 250 keep vs 200 remove = 55% keeps the legate");
        turn = 141;
        c.SupremeInAlliance = true;
        Open(c, 8);
        nq = Idx(c, CouncilLaws.NoConfidence);
        c.TrySetBallot(1, new[] { new[] { nq, 1, 100 } });
        c.TrySetBallot(0, new[] { new[] { nq, 0, 250 } });
        turn = 142; c.Tally(turn);
        Check(c.SupremeHolder == 0, "alliance: 250 keep vs 100 remove = 71% keeps the legate");
        turn = 161;
        Open(c, 9);
        nq = Idx(c, CouncilLaws.NoConfidence);
        Check(nq >= 0, "the confirmation again next vote");
        c.TrySetBallot(1, new[] { new[] { nq, 1, 100 } });
        c.TrySetBallot(2, new[] { new[] { nq, 1, 100 } });
        c.TrySetBallot(0, new[] { new[] { nq, 0, 250 } });
        turn = 162; c.Tally(turn);
        Check(c.SupremeHolder == -1 && c.SupremeSince == -1, "alliance: 55% is not enough, the mandate is revoked and the countdown stops");
        c.SupremeInAlliance = false;
        Check(c.Budget(0) == 150, "votes back to normal (the chairman mandate stays: +50)");
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
                for (int h = 0; h < 5; h++) z.ReportTask(e, CouncilLaws.CatMastery, "h" + h);
                if (rng.Range(2) == 0) z.ReportTask(e, CouncilLaws.CatCapital, "any");
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

        {
            var q = new CouncilCore(); q.Found(0, new[] { 0, 1 }, 20);
            q.SetHolder(CouncilLaws.Marshal, 1, 0);
            Check(q.SquadNeeded("M", 1) && !q.SquadNeeded("M", 0), "marshal squad needed only for the holder");
            q.AddSquadShips("M", 1, new ulong[] { 5, 6, 7 });
            q.AddSquadShips("E", 0, new ulong[] { 6, 8 });
            Check(q.FindSquad("E", 0).Ships.Count == 1 && q.IsSquadShip(8) && !q.IsSquadShip(9), "ship belongs to one squad only");
            Check(q.IsGiftSquadShip(8) && !q.IsGiftSquadShip(5) && !q.IsGiftSquadShip(9), "law squads are free of upkeep, the hired Marshal contingent is not");
            var rs = CouncilCore.Deserialize(q.Serialize());
            Check(rs.Squads.Count == 2 && rs.FindSquad("M", 1).Ships.Count == 3 && rs.IsSquadShip(7), "squads survive serialization");
            Check(q.ExpiredSquads().Count == 1 && q.ExpiredSquads()[0].Tag == "E", "escort squad expires without the law");
            q.RevokeMandate(CouncilLaws.Marshal);
            Check(q.ExpiredSquads().Count == 2, "marshal squad expires with the mandate");
            q.Leave(1);
            Check(!q.SquadNeeded("M", 1), "leaving the council ends squads");
        }

        {
            var q = new CouncilCore(); q.Found(0, new[] { 0, 1, 2 }, 20);
            var idx = CouncilCore.PickPresenceIndices(40, 3, 123);
            var idx2 = CouncilCore.PickPresenceIndices(40, 3, 123);
            bool same = idx.Count == idx2.Count; for (int i = 0; same && i < idx.Count; i++) same = idx[i] == idx2[i];
            Check(idx.Count == 6 && same, "presence: 6 marks for 3 members, deterministic");
            Check(CouncilCore.PickPresenceIndices(3, 3, 1).Count == 3, "presence: never more marks than systems");
            q.SetPresenceMarks(new ulong[] { 10, 11, 12, 13, 14, 15, 16, 17 });
            q.ApplyPresenceBonus(delegate (ulong g) { return g < 15 ? 0 : (g < 17 ? 1 : -1); });
            Check(q.PresenceBonusOf(0) == 50 && q.PresenceBonusOf(1) == 20 && q.PresenceBonusOf(2) == 0, "presence: +10 per held system, capped at +50");
            Check(q.Budget(0) == 150 && q.Budget(1) == 120, "presence: bonus is part of the vote budget");
            Check(q.PresenceMarks.Count == 0, "presence: marks are consumed when the session opens");
            q.SetPresenceMarks(new ulong[] { 5, 6 });
            var rp = CouncilCore.Deserialize(q.Serialize());
            Check(rp.PresenceMarks.Count == 2 && rp.PresenceBonusOf(0) == 50 && rp.Budget(1) == 120, "presence survives serialization");
            q.RemoveMember(0);
            Check(q.PresenceBonusOf(0) == 0, "leaving the council drops the presence bonus");
        }
        // ---- sources of votes: system levels, Republic, mandates, wars
        {
            Check(CouncilCore.SystemLevelVotes(1) == 0 && CouncilCore.SystemLevelVotes(2) == 5 && CouncilCore.SystemLevelVotes(3) == 10 && CouncilCore.SystemLevelVotes(4) == 20,
                  "system level votes 0 / 5 / 10 / 20, not cumulative");
            Check(CouncilCore.TechStageVotes(1) == 0 && CouncilCore.TechStageVotes(2) == 0 && CouncilCore.TechStageVotes(3) == 10 && CouncilCore.TechStageVotes(4) == 20 && CouncilCore.TechStageVotes(5) == 30 && CouncilCore.TechStageVotes(6) == 30,
                  "technology stage votes 3/4/5 -> 10/20/30");
            Check(CouncilCore.RepublicVotes == 40 && CouncilCore.MandateVotes == 50 && CouncilCore.SupremeMandateVotes == 100 && CouncilCore.WarPenaltyVotes == 20, "vote constants");
            var v = new CouncilCore(); v.Found(0, new[] { 0, 1, 2 }, 20);
            var game = new Dictionary<int, int>(); game[0] = 5 + 10 + 20 + CouncilCore.RepublicVotes; game[1] = 0; game[2] = 20;
            v.ApplySessionVotes(game, 10);
            Check(v.Budget(0) == 175 && v.Budget(1) == 100 && v.Budget(2) == 120, "session votes are part of the budget");
            v.RecordWarDeclared(1, 10);
            v.ApplySessionVotes(game, 12);
            Check(v.RecentWars(1, 12) == 1 && v.SessionVotesOf(1) == -20 && v.Budget(1) == 80, "a declared war costs 20 votes");
            v.RecordWarDeclared(1, 12); v.RecordWarDeclared(1, 12);
            v.ApplySessionVotes(game, 13);
            Check(v.Budget(1) == 100 - 60 && v.WarPenaltyOf(1, 13) == 60, "wars stack");
            v.ApplySessionVotes(game, 31);
            Check(v.RecentWars(1, 31) == 2 && v.Budget(1) == 60, "the penalty of the first war ends after 20 turns (2 wars of turn 12 remain)");
            v.ApplySessionVotes(game, 40);
            Check(v.RecentWars(1, 40) == 0 && v.Budget(1) == 100, "all penalties are over after 20 turns");
            var huge = new Dictionary<int, int>();
            for (int w = 0; w < 10; w++) v.RecordWarDeclared(2, 50);
            v.ApplySessionVotes(huge, 50);
            Check(v.Budget(2) == 0, "the budget never goes below zero");
            // mandates: +50 each, supreme: x2 and +100
            var m = new CouncilCore(); m.Found(0, new[] { 0, 1 }, 20);
            m.SetHolder(CouncilLaws.Chairman, 0, 0); m.SetHolder(CouncilLaws.Eye, 0, 0);
            Check(m.MandateVotesOf(0) == 100 && m.Budget(0) == 200 && m.Budget(1) == 100, "every mandate gives +50 votes");
            m.SetHolder(CouncilLaws.Supreme, 0, 5);
            Check(m.MandateVotesOf(0) == 100 && m.Budget(0) == 300, "supreme: 100 + 100 + 100, nothing is doubled");
            m.ApplySessionVotes(game, 10);
            var rm = CouncilCore.Deserialize(m.Serialize());
            Check(rm.Budget(0) == m.Budget(0) && rm.SessionVotesOf(0) == m.SessionVotesOf(0), "session votes survive serialization");
            m.RecordWarDeclared(1, 7);
            rm = CouncilCore.Deserialize(m.Serialize());
            Check(rm.RecentWars(1, 8) == 1, "declared wars survive serialization");
            Check(m.BudgetText(0, 10, true).Length > 0 && m.BudgetText(1, 8, false).Contains("war"), "budget explanation");
            // older saves (without the vote and purchase fields) still load
            string oldState = m.Serialize();
            for (int strip = 0; strip < 4; strip++) { int cut = oldState.LastIndexOf('|'); if (cut < 0) break; oldState = oldState.Substring(0, cut); }
            var ro = CouncilCore.Deserialize(oldState);
            Check(ro.Enabled && ro.Holds(0, CouncilLaws.Supreme) && ro.SessionVotesOf(0) == 0, "state without vote fields loads");
            Check(ro.BoughtVotesOf(0) == 0 && !ro.IsTerrorFleet(0, 1ul), "state without purchase fields loads");
        }
        // ---- purchases of votes during a session (the senator class line)
        {
            var b = new CouncilCore(); b.Found(0, new[] { 0, 1 }, 20);
            Check(b.BoughtVotesOf(0) == 0 && b.Budget(0) == 100, "no purchases, plain budget");
            Check(!b.CanBuyVotes(0, 0, 4), "cannot buy while no vote is open");
            b.OpenVote(10, new List<string> { "ResearchDevUp" });
            Check(b.CanBuyVotes(0, 0, 4) && b.CanBuyVotes(0, 2, 1) && !b.CanBuyVotes(0, 2, 0) && !b.CanBuyVotes(2, 0, 4), "buy gates: voting phase, membership, limit");
            Check(b.TryBuyVotes(0, 0, 4) && b.TryBuyVotes(0, 2, 1) && !b.TryBuyVotes(0, 2, 1), "stacks register up to the limit");
            Check(b.BoughtStacks(0, 0) == 1 && b.BoughtStacks(0, 2) == 1 && b.BoughtVotesOf(0) == 20 && b.Budget(0) == 120, "10 votes per purchase enter the budget");
            Check(!b.CanBuyVotes(0, 2, 1), "civic limit of one reached");
            // terror: the stack and the immobilised fleet register together
            Check(b.TryBuyTerror(0, 4, 77ul) && b.IsTerrorFleet(0, 77ul) && b.BoughtStacks(0, 3) == 1, "a terror purchase freezes its fleet");
            Check(!b.TryBuyTerror(0, 4, 77ul), "the same fleet cannot be frozen twice");
            Check(b.TryBuyTerror(0, 2, 78ul) && !b.TryBuyTerror(0, 2, 79ul), "the limit of terror purchases holds");
            Check(b.BoughtVotesOf(0) == 40 && b.Budget(0) == 140, "four purchases = 40 votes");
            // serialization round trip
            var rb = CouncilCore.Deserialize(b.Serialize());
            Check(rb.BoughtStacks(0, 0) == 1 && rb.BoughtStacks(0, 2) == 1 && rb.BoughtStacks(0, 3) == 2 && rb.BoughtVotesOf(0) == 40, "purchases survive serialization");
            Check(rb.IsTerrorFleet(0, 77ul) && rb.IsTerrorFleet(0, 78ul) && !rb.IsTerrorFleet(0, 79ul), "frozen fleets survive serialization");
            Check(rb.Budget(0) == 140, "the budget of a loaded state counts the purchases");
            Check(rb.BudgetText(0, 10, true).Contains("куплено"), "the budget explanation mentions the purchases");
            // a new session wipes everything (the penalties end when it opens)
            b.OpenVote(30, new List<string> { "ResearchDevUp" });
            Check(b.BoughtVotesOf(0) == 0 && !b.IsTerrorFleet(0, 77ul) && b.Budget(0) == 100, "a new session clears the purchases and the frozen fleets");
            // leaving the council drops the state too
            b.TryBuyVotes(1, 1, 4);
            b.RemoveMember(1);
            Check(b.BoughtVotesOf(1) == 0, "leaving the council drops the purchases");
        }
        // ---- mandate powers of an AI empire
        {
            var a = new CouncilCore();
            turn = 400;
            a.Found(0, new[] { 0, 1, 2 }, 20);
            a.SetHolder(CouncilLaws.Chairman, 1, turn);
            Open(a, 7);
            Check(a.CanChairmanReplace(1), "AI chairman may replace a question before anybody voted");
            var before = new List<string>(a.Proposals);
            Check(a.TryAiChairman(1, 99), "AI chairman replaces a question");
            int changed = 0; for (int i = 0; i < before.Count; i++) if (before[i] != a.Proposals[i]) changed++;
            Check(changed == 1 && !a.CanChairmanReplace(1) && !a.TryAiChairman(1, 99), "exactly one question changed, once per vote");
            bool harmless = true;
            for (int i = 0; i < a.Proposals.Count; i++) { var d = CouncilLaws.Get(a.Proposals[i]); if (d == null) harmless = false; }
            Check(harmless, "all proposals are known laws");
            Check(!new CouncilCore().TryAiChairman(0, 1), "no council, no replacement");
            Check(CouncilCore.AiCanAffordMarshal(1000f, 200f, 2) && !CouncilCore.AiCanAffordMarshal(1000f, 10f, 2) && !CouncilCore.AiCanAffordMarshal(50f, 500f, 2), "AI hires the Marshal only with a stock and a positive net income");
            Check(CouncilCore.MarshalShips(1) == 3 && CouncilCore.MarshalShips(20) == 8, "contingent size");
        }
        Console.WriteLine(fails == 0 ? "ALL PASSED" : (fails + " FAILED"));
        return fails == 0 ? 0 : 1;
    }
}

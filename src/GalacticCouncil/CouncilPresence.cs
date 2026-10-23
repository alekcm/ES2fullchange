// Strategic Presence Program: systems are marked at the end of the vote that enacts the law; at the next session
// every member gets extra votes for the marked systems it holds (pure logic, no game types).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public sealed partial class CouncilCore
{
    public const int PresenceStep = 10;      // votes per held marked system
    public const int PresenceMaxBonus = 50;  // cap per empire

    public List<ulong> PresenceMarks = new List<ulong>();                       // systems marked for the next session
    public Dictionary<int, int> PresenceBonus = new Dictionary<int, int>();     // extra votes of the open session

    public int PresenceBonusOf(int emp) { int v; return PresenceBonus.TryGetValue(emp, out v) ? v : 0; }

    /// <summary>Called when the law is enacted: these systems count at the next session.</summary>
    public void SetPresenceMarks(IList<ulong> marks)
    {
        PresenceMarks.Clear();
        for (int i = 0; i < marks.Count; i++) if (!PresenceMarks.Contains(marks[i])) PresenceMarks.Add(marks[i]);
    }

    /// <summary>Called when a session opens: converts held marked systems into extra votes and consumes the marks.</summary>
    public void ApplyPresenceBonus(Func<ulong, int> ownerOf)
    {
        PresenceBonus.Clear();
        if (PresenceMarks.Count == 0) return;
        var count = new Dictionary<int, int>();
        for (int i = 0; i < PresenceMarks.Count; i++)
        {
            int o = ownerOf(PresenceMarks[i]);
            if (o < 0 || !IsMember(o)) continue;
            int c; count.TryGetValue(o, out c); count[o] = c + 1;
        }
        foreach (var kv in count) PresenceBonus[kv.Key] = Math.Min(PresenceMaxBonus, kv.Value * PresenceStep);
        PresenceMarks.Clear();
    }

    /// <summary>Deterministic choice of how many / which systems to mark (indices into a candidate list of the given size).</summary>
    public static List<int> PickPresenceIndices(int candidates, int memberCount, int seed)
    {
        var res = new List<int>();
        if (candidates <= 0) return res;
        int want = Math.Min(12, Math.Max(5, memberCount * 2));
        want = Math.Min(want, candidates);
        var rng = new CouncilRng(seed);
        var pool = new List<int>();
        for (int i = 0; i < candidates; i++) pool.Add(i);
        while (res.Count < want && pool.Count > 0)
        {
            int k = rng.Range(pool.Count);
            res.Add(pool[k]);
            pool.RemoveAt(k);
        }
        res.Sort();
        return res;
    }

    void ResetPresence() { PresenceMarks.Clear(); PresenceBonus.Clear(); }

    void AppendPresenceState(StringBuilder sb)
    {
        var a = new string[PresenceMarks.Count];
        for (int i = 0; i < a.Length; i++) a[i] = PresenceMarks[i].ToString(CultureInfo.InvariantCulture);
        sb.Append('|').Append(string.Join(",", a)).Append('|');
        var parts = new List<string>();
        foreach (var kv in PresenceBonus) parts.Add(kv.Key.ToString(CultureInfo.InvariantCulture) + ":" + kv.Value.ToString(CultureInfo.InvariantCulture));
        sb.Append(string.Join(";", parts.ToArray()));
    }

    void ReadPresenceState(string marks, string bonus)
    {
        PresenceMarks.Clear(); PresenceBonus.Clear();
        foreach (var x in SplitStr(marks, ',')) PresenceMarks.Add(ulong.Parse(x, CultureInfo.InvariantCulture));
        foreach (var rec in SplitStr(bonus, ';'))
        {
            var t = rec.Split(':');
            PresenceBonus[int.Parse(t[0], CultureInfo.InvariantCulture)] = int.Parse(t[1], CultureInfo.InvariantCulture);
        }
    }
}

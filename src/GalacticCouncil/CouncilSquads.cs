// Council service squads: logic only (no game dependencies). The game glue spawns/recalls the ships.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public sealed class CouncilSquad
{
    public string Tag;                 // M marshal contingent, E escort squadron, A academy squadron, S covert group
    public int Emp;
    public List<ulong> Ships = new List<ulong>();
}

public sealed partial class CouncilCore
{
    public const string TagMarshal = "M", TagEscort = "E", TagAcademy = "A", TagStealth = "S";
    public const string LawEscort = "EscortSquads", LawAcademySquad = "AcademySquad", LawStealth = "StealthOps";

    public List<CouncilSquad> Squads = new List<CouncilSquad>();

    /// <summary>Should the squad still exist? (the contract/law is in force and the empire is still in the council)</summary>
    public bool SquadNeeded(string tag, int emp)
    {
        if (!Enabled || !IsMember(emp)) return false;
        switch (tag)
        {
            case TagMarshal: return Holds(emp, CouncilLaws.Marshal);
            case TagEscort: return IsLawActive(LawEscort);
            case TagAcademy: return IsLawActiveFor(LawAcademySquad, emp);
            case TagStealth: return IsLawActiveFor(LawStealth, emp);
        }
        return false;
    }

    public bool IsSquadShip(ulong guid)
    {
        for (int i = 0; i < Squads.Count; i++) if (Squads[i].Ships.Contains(guid)) return true;
        return false;
    }

    public CouncilSquad FindSquad(string tag, int emp)
    {
        for (int i = 0; i < Squads.Count; i++) if (Squads[i].Tag == tag && Squads[i].Emp == emp) return Squads[i];
        return null;
    }

    /// <summary>Records freshly found ships of a squad (merges into an existing record of the same tag/empire).</summary>
    public void AddSquadShips(string tag, int emp, IEnumerable<ulong> ships)
    {
        var s = FindSquad(tag, emp);
        if (s == null) { s = new CouncilSquad { Tag = tag, Emp = emp }; Squads.Add(s); }
        foreach (var g in ships) if (!s.Ships.Contains(g) && !IsSquadShip(g)) s.Ships.Add(g);
    }

    public void RemoveSquad(CouncilSquad s) { Squads.Remove(s); }

    /// <summary>Squads whose reason to exist is gone.</summary>
    public List<CouncilSquad> ExpiredSquads()
    {
        var l = new List<CouncilSquad>();
        for (int i = 0; i < Squads.Count; i++) if (!SquadNeeded(Squads[i].Tag, Squads[i].Emp)) l.Add(Squads[i]);
        return l;
    }

    void AppendSquadState(StringBuilder sb)
    {
        var parts = new List<string>();
        for (int i = 0; i < Squads.Count; i++)
        {
            var g = new string[Squads[i].Ships.Count];
            for (int k = 0; k < g.Length; k++) g[k] = Squads[i].Ships[k].ToString(CultureInfo.InvariantCulture);
            parts.Add(Squads[i].Tag + "~" + Squads[i].Emp.ToString(CultureInfo.InvariantCulture) + "~" + string.Join(",", g));
        }
        sb.Append(string.Join(";", parts.ToArray()));
    }

    void ReadSquadState(string s)
    {
        Squads.Clear();
        foreach (var rec in SplitStr(s, ';'))
        {
            var t = rec.Split('~');
            if (t.Length < 3) continue;
            var sq = new CouncilSquad { Tag = t[0], Emp = int.Parse(t[1], CultureInfo.InvariantCulture) };
            foreach (var g in SplitStr(t[2], ',')) sq.Ships.Add(ulong.Parse(g, CultureInfo.InvariantCulture));
            Squads.Add(sq);
        }
    }
}

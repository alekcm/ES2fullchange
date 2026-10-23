using System;
using System.Collections.Generic;
using System.IO;
using Amplitude;
using Amplitude.Unity.Game.Orders;

/// <summary>Player order: replaces the whole council ballot of the issuing empire.</summary>
public class OrderCouncilVote : Order
{
    public static readonly StaticString AuthenticationPath = "DepartmentOfTheTreasury/OrderCouncilVote";

    public List<int[]> Entries = new List<int[]>();

    public OrderCouncilVote(int empireIndex, List<int[]> entries) : base(empireIndex)
    {
        Entries = entries ?? new List<int[]>();
    }

    public override StaticString Path { get { return AuthenticationPath; } }

    public override void Pack(BinaryWriter writer)
    {
        base.Pack(writer);
        writer.Write(Entries.Count);
        for (int i = 0; i < Entries.Count; i++)
        {
            writer.Write(Entries[i][0]);
            writer.Write(Entries[i][1]);
            writer.Write(Entries[i][2]);
        }
    }

    public override void Unpack(BinaryReader reader)
    {
        base.Unpack(reader);
        int n = reader.ReadInt32();
        Entries = new List<int[]>();
        for (int i = 0; i < n && i < 64; i++)
        {
            int q = reader.ReadInt32(), o = reader.ReadInt32(), v = reader.ReadInt32();
            Entries.Add(new[] { q, o, v });
        }
    }
}

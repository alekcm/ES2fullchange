using System;
using System.Collections.Generic;
using System.IO;
using Amplitude;
using Amplitude.Unity.Game.Orders;

/// <summary>
/// Player order of the Galactic Council. Kind selects what it does:
/// Ballot (replaces the whole ballot), ChairmanReplace, MarshalHire, QuartermasterRequest, ReportTask (unused), Leave.
/// Orders are deserialized without running field initializers, so Unpack assigns every field.
/// </summary>
public class OrderCouncilVote : Order
{
    public static readonly StaticString AuthenticationPath = "DepartmentOfTheTreasury/OrderCouncilVote";

    public const int KindBallot = 0;
    public const int KindChairmanReplace = 1;     // A = question index, B = index of the new law in CouncilLaws.All
    public const int KindMarshalHire = 2;
    public const int KindQuartermaster = 3;
    public const int KindReportTask = 4;          // unused (the manual assignment stub was removed; conditions are read from the game)
    public const int KindLeave = 5;
    public const int KindDebug = 6;               // God-Mod menu: A = CouncilGame.Dbg* action, B = parameter (every client runs it, so the game stays in sync)

    public List<int[]> Entries = new List<int[]>();
    public int Kind;
    public int A;
    public int B;

    public OrderCouncilVote(int empireIndex, List<int[]> entries) : base(empireIndex)
    {
        Entries = entries ?? new List<int[]>();
        Kind = KindBallot;
    }

    public OrderCouncilVote(int empireIndex, int kind, int a, int b) : base(empireIndex)
    {
        Entries = new List<int[]>();
        Kind = kind; A = a; B = b;
    }

    public override StaticString Path { get { return AuthenticationPath; } }

    public override void Pack(BinaryWriter writer)
    {
        base.Pack(writer);
        writer.Write(Kind);
        writer.Write(A);
        writer.Write(B);
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
        Kind = reader.ReadInt32();
        A = reader.ReadInt32();
        B = reader.ReadInt32();
        int n = reader.ReadInt32();
        Entries = new List<int[]>();
        for (int i = 0; i < n && i < 64; i++)
        {
            int q = reader.ReadInt32(), o = reader.ReadInt32(), v = reader.ReadInt32();
            Entries.Add(new[] { q, o, v });
        }
    }
}

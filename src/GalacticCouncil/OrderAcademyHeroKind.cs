using System.IO;
using Amplitude;
using Amplitude.Unity.Game.Orders;

/// <summary>
/// Player order of the Academy offer rework ("variant 2b"). The player answered the "what kind of hero"
/// question about the pending hero choice of one gauge; the processor reshapes that vanilla trio into two
/// candidates of the requested kind on every machine (deterministic pick), returns the dropped trio heroes
/// to the Academy pool and raises EventHeroUnlockGaugeFilled again, so the recruitment notification redraws.
/// Kind is (int)HeroKind: 0 admiral, 1 governor, 2 senator. AI empires never send this order.
/// Orders are deserialized without running field initializers, so Unpack assigns every field.
/// </summary>
public class OrderAcademyHeroKind : Order
{
    public static readonly StaticString AuthenticationPath = "DepartmentOfEducation/OrderAcademyHeroKind";

    public int GaugeIndex;      // HeroChoice.GaugeIndex of the pending choice to reshape
    public int Kind;            // (int)HeroKind of the wanted candidates

    public OrderAcademyHeroKind(int empireIndex, int gaugeIndex, int kind) : base(empireIndex)
    {
        GaugeIndex = gaugeIndex;
        Kind = kind;
    }

    public override StaticString Path { get { return AuthenticationPath; } }

    public override void Pack(BinaryWriter writer)
    {
        base.Pack(writer);
        writer.Write(GaugeIndex);
        writer.Write(Kind);
    }

    public override void Unpack(BinaryReader reader)
    {
        base.Unpack(reader);
        GaugeIndex = reader.ReadInt32();
        Kind = reader.ReadInt32();
    }
}

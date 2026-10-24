// Mandate powers of AI empires. A human presses the buttons of the Council window (orders posted by his client); for an AI the same actions
// are run here, on every machine at the same point of the turn, so the Council state stays identical everywhere. Only the host posts the
// orders that create ships (SpawnSquad checks it).
using System;
using System.Collections.Generic;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;

public static partial class CouncilGame
{
    static void AiMandateActions(Game game, int turn)
    {
        foreach (MajorEmpire e in game.EnumerableMajorEmpires)
        {
            if (!e.IsControlledByAI || e.HasBeenEliminated || !Core.IsMember(e.Index)) continue;
            int idx = e.Index;
            try
            {
                if (Core.CanChairmanReplace(idx))
                {
                    int seed = unchecked(Core.VoteOpenTurn * 31 + Core.VoteIndex * 977 + 5);
                    Core.TryAiChairman(idx, seed);
                }
            }
            catch (Exception ex) { Fail("AI Chairman " + idx, ex); }
            try { if (Core.TryQuartermaster(idx, turn)) GrantSupply(idx); }
            catch (Exception ex) { Fail("AI Quartermaster " + idx, ex); }
            try
            {
                if (Core.CanHireMarshal(idx) && AiCanAffordMarshal(e) && Core.TryHireMarshal(idx)) SpawnContingent(idx);
            }
            catch (Exception ex) { Fail("AI Marshal " + idx, ex); }
        }
    }

    static bool AiCanAffordMarshal(MajorEmpire e)
    {
        var treasury = e.GetAgency<DepartmentOfTheTreasury>();
        if (treasury == null) return false;
        float stock, net;
        if (!treasury.TryGetResourceStockAndNet(e.SimulationObject, DepartmentOfTheTreasury.Resources.EmpireMoney, out stock, out net)) return false;
        return CouncilCore.AiCanAffordMarshal(stock, net, GameStage(GetGame(), e));
    }
}

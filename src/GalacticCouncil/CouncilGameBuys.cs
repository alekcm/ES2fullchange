// The senator class line in action: buying extra votes while a Council vote is open (see HeroDesign "BribeVotes").
//   limits   - levels of the four T1 skills of ClassCorporate summed over the senators that represent the
//              empire's parties (+2 per method for the held class ultimate);
//   price    - base x game stage x (1 + 0.5 x purchases already made): inflation inside and outside the session;
//   payment  - Dust (bribe) or Influence (pressure) through the treasury; the civic purchase takes happiness of
//              every system and the terror purchase immobilises a fleet of 4+ command points, both until the
//              NEXT session opens (RefreshBuyPenalties applies and removes the descriptors);
//   orders   - OrderCouncilVote.KindBuyVotes (validated by the preprocessor, applied once by the processor),
//              so multiplayer and saved games stay consistent.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Amplitude;
using Amplitude.Unity.Framework;
using Amplitude.Unity.Game;
using Amplitude.Unity.Simulation;

public static partial class CouncilGame
{
    // ---------------------------------------------------------------- limits (skills of the representing senators)

    public static int BuyLimitOf(Empire e, int method) { return HeroRework.BuyLimitOf(e, method); }

    // ---------------------------------------------------------------- price of the next purchase

    /// <summary>Price of the next stack of a method: the inflation is the game stage and the purchases already made.</summary>
    public static int BuyPriceOf(Empire e, int method)
    {
        if (e == null) return 0;
        int[] v = Core.BuysOf(e.Index);
        return CouncilCore.BuyStackCost(method, v[method] + 1, GameStage(GetGame(), e));
    }

    // ---------------------------------------------------------------- resources (Dust and Influence)

    static StaticString BuyResource(int method)
    {
        return method == 0 ? DepartmentOfTheTreasury.Resources.EmpireMoney
                           : DepartmentOfTheTreasury.Resources.EmpireEmpirePoint;
    }

    static bool CanAffordBuy(Empire e, int method, int price)
    {
        if (e == null || price <= 0) return price <= 0;
        var treasury = e.GetAgency<DepartmentOfTheTreasury>();
        if (treasury == null || e.SimulationObject == null) return false;
        float stock, net;
        if (!treasury.TryGetResourceStockAndNet(e.SimulationObject, BuyResource(method), out stock, out net)) return false;
        return stock >= price;
    }

    /// <summary>Collects the price; false when the empire cannot pay (nothing is taken then).</summary>
    static bool TryPayBuy(Empire e, int method, int price)
    {
        if (price <= 0) return true;
        if (!CanAffordBuy(e, method, price)) return false;
        var treasury = e.GetAgency<DepartmentOfTheTreasury>();
        if (treasury == null || e.SimulationObject == null) return false;
        return treasury.TryTransferResources(e.SimulationObject, BuyResource(method), -price, false);
    }

    public static string BuyUnitName(int method, bool ru)
    {
        return method == 0 ? Tr("Пыли", "Dust") : Tr("Влияния", "Influence");
    }

    // ---------------------------------------------------------------- fleets (the terror purchase)

    public sealed class CouncilBuyFleet
    {
        public ulong Guid;
        public string Name;
        public float Cp;
        public bool Frozen;
    }

    /// <summary>Fleets of the empire the player can hold "in fear" (frozen ones marked).</summary>
    public static List<CouncilBuyFleet> BuyFleetsOf(Empire e)
    {
        var res = new List<CouncilBuyFleet>();
        var defense = e == null ? null : e.GetAgency<DepartmentOfDefense>();
        if (defense == null || defense.Fleets == null) return res;
        foreach (Fleet f in defense.Fleets)
        {
            if (f == null || f.SimulationObject == null) continue;
            float cp;
            try { cp = f.CommandPoints; } catch (Exception) { continue; }
            string name;
            try { name = string.IsNullOrEmpty(f.LocalizedName) ? ("#" + (ulong)f.GUID) : f.LocalizedName; } catch (Exception) { name = "#" + (ulong)f.GUID; }
            res.Add(new CouncilBuyFleet { Guid = (ulong)f.GUID, Name = name, Cp = cp, Frozen = Core.IsTerrorFleet(e.Index, (ulong)f.GUID) });
        }
        return res;
    }

    static Fleet FindFleet(Empire e, ulong guid)
    {
        var defense = e == null ? null : e.GetAgency<DepartmentOfDefense>();
        if (defense == null || defense.Fleets == null) return null;
        foreach (Fleet f in defense.Fleets)
            if (f != null && (ulong)f.GUID == guid) return f;
        return null;
    }

    // ---------------------------------------------------------------- the order (UI entry, preprocessor, processor)

    /// <summary>UI entry: posts the purchase order (the server validates it and applies it once).</summary>
    public static bool CastBuy(Empire empire, int method, ulong fleet)
    {
        try
        {
            if (empire == null) return false;
            var pc = empire.PlayerControllers;
            if (pc == null || pc.Client == null) return false;
            pc.Client.PostOrder(new OrderCouncilVote(empire.Index, OrderCouncilVote.KindBuyVotes, method, 0, fleet));
            return true;
        }
        catch (Exception e) { Fail("CastBuy", e); return false; }
    }

    public static bool BuyPreprocessor(OrderCouncilVote o)
    {
        if (o == null || o.Kind != OrderCouncilVote.KindBuyVotes) return false;
        int method = o.A;
        if (method < 0 || method >= CouncilCore.BuyMethodCount) return false;
        if (!Core.Enabled || Core.Phase != CouncilCore.PhaseVoting) return false;
        var e = GetMajor(o.EmpireIndex);
        if (e == null || !Core.IsMember(e.Index)) return false;
        int limit = BuyLimitOf(e, method);
        if (!Core.CanBuyVotes(e.Index, method, limit)) return false;
        int price = BuyPriceOf(e, method);
        if (!CanAffordBuy(e, method, price)) return false;
        if (method == 3)
        {
            Fleet f = FindFleet(e, o.Fleet);
            if (f == null || f.CommandPoints < CouncilCore.TerrorFleetCp) return false;
            if (Core.IsTerrorFleet(e.Index, o.Fleet)) return false;
        }
        return true;
    }

    public static void BuyProcess(OrderCouncilVote o)
    {
        int method = o.A;
        var e = GetMajor(o.EmpireIndex);
        if (e == null) return;
        int limit = BuyLimitOf(e, method);
        int price = BuyPriceOf(e, method);
        if (!Core.CanBuyVotes(e.Index, method, limit)) return;
        if (method == 3)
        {
            if (!Core.TryBuyTerror(e.Index, limit, o.Fleet)) return;
        }
        else
        {
            if (!TryPayBuy(e, method, price)) return;
            if (!Core.TryBuyVotes(e.Index, method, limit))
            {
                // the payment is already made - give it back if the stack did not register
                if (method == 0 || method == 1) TryPayBuy(e, method, -price);
                return;
            }
        }
        RefreshBuyPenalties(GetGame());
        try { CouncilNative.RebuildIfVisible(); } catch (Exception) { }
        if (e.Index == LocalIndex())
        {
            string what = method == 0 ? Tr("Подкуп избирателей", "Bribing the electorate")
                        : method == 1 ? Tr("Задавить авторитетом", "Crushing authority")
                        : method == 2 ? Tr("Принудительное участие", "Compulsory turnout")
                        : Tr("Держать в страхе", "Keeping them in fear");
            string pay = method == 0 || method == 1 ? " — " + price + " " + BuyUnitName(method, true) : "";
            CouncilUi.Toast(what + ": " + Tr("куплено 10 голосов", "10 votes bought") + pay);
        }
    }

    // ---------------------------------------------------------------- penalties: civic happiness and frozen fleets

    static readonly List<SimulationDescriptor> buyDescriptors = new List<SimulationDescriptor>();
    static readonly List<Amplitude.Unity.Simulation.SimulationObjectWrapper> buyTargets = new List<Amplitude.Unity.Simulation.SimulationObjectWrapper>();
    static string buySignature;
    static int buyDescriptorSeq;

    static SimulationDescriptor MakeCivicDescriptor(int unhappiness)
    {
        var mod = CustomHackingHelper.MakeModifier("Happiness", SimulationModifierDescriptor.ModifierOperation.Addition, -unhappiness, "ClassStarSystem");
        return CustomHackingHelper.CreateAndRegisterDescriptor(null, "CouncilCivic_" + (++buyDescriptorSeq), "CouncilCivic", null, new[] { mod });
    }

    static SimulationDescriptor MakeTerrorDescriptor()
    {
        var mod = CustomHackingHelper.MakeModifier("ShipMaximumMovement", SimulationModifierDescriptor.ModifierOperation.Addition, -999f, "");
        return CustomHackingHelper.CreateAndRegisterDescriptor(null, "CouncilTerror_" + (++buyDescriptorSeq), "CouncilTerror", null, new[] { mod });
    }

    static void ClearBuyPenalties()
    {
        for (int i = 0; i < buyDescriptors.Count; i++)
        {
            try { buyTargets[i].SimulationObject.RemoveDescriptor(buyDescriptors[i]); buyTargets[i].Refresh(false); } catch (Exception) { }
        }
        buyDescriptors.Clear();
        buyTargets.Clear();
        buySignature = "";
    }

    /// <summary>Applies / removes the happiness and fleet descriptors of the purchases; called every turn and after a purchase.
    /// The signature guards the work: nothing happens while the wanted state is already applied.</summary>
    public static void RefreshBuyPenalties(Game game)
    {
        try
        {
            if (game == null) return;
            var systems = new List<ColonizedStarSystem>();  // systems with a happiness penalty
            var strength = new List<int>();
            var fleets = new List<Fleet>();                 // fleets held in fear
            var sb = new StringBuilder();
            foreach (MajorEmpire e in game.EnumerableMajorEmpires)
            {
                if (e == null || e.HasBeenEliminated) continue;
                int[] v = Core.BuysOf(e.Index);
                if (v[2] > 0)
                {
                    var interior = e.GetAgency<DepartmentOfTheInterior>();
                    if (interior == null || interior.ColonizedStarSystems == null) continue;
                    foreach (ColonizedStarSystem cs in interior.ColonizedStarSystems)
                    {
                        if (cs == null || cs.SimulationObject == null) continue;
                        systems.Add(cs); strength.Add(v[2] * CouncilCore.CivicHappinessPerStack);
                        sb.Append(cs.GUID).Append(':').Append(v[2]).Append(',');
                    }
                }
                List<ulong> frozen;
                if (Core.TerrorFleets.TryGetValue(e.Index, out frozen) && frozen != null)
                    for (int i = 0; i < frozen.Count; i++)
                    {
                        Fleet f = FindFleet(e, frozen[i]);
                        if (f == null || f.SimulationObject == null) { sb.Append(frozen[i]).Append(":gone,"); continue; }
                        fleets.Add(f);
                        sb.Append(frozen[i]).Append(":ice,");
                    }
            }
            string sig = sb.ToString();
            if (sig == buySignature) return;
            ClearBuyPenalties();
            buySignature = sig;
            var byStrength = new Dictionary<int, SimulationDescriptor>();
            for (int i = 0; i < systems.Count; i++)
            {
                SimulationDescriptor d;
                if (!byStrength.TryGetValue(strength[i], out d)) byStrength[strength[i]] = d = MakeCivicDescriptor(strength[i]);
                systems[i].SimulationObject.AddDescriptor(d);
                buyDescriptors.Add(d); buyTargets.Add(systems[i]);
            }
            for (int i = 0; i < fleets.Count; i++)
            {
                SimulationDescriptor d = MakeTerrorDescriptor();
                fleets[i].SimulationObject.AddDescriptor(d);
                buyDescriptors.Add(d); buyTargets.Add(fleets[i]);
            }
            for (int i = 0; i < buyTargets.Count; i++)
            {
                try { buyTargets[i].Refresh(false); } catch (Exception) { }
            }
        }
        catch (Exception e) { Fail("RefreshBuyPenalties", e); }
    }
}

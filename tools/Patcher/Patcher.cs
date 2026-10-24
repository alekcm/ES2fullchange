// Galactic Council patcher.
//   Patcher.exe <baseAssembly> <councilDll> <outAssembly> <managedDir>
// Clones the Council types into Assembly-CSharp and installs the hooks (see docs/GALACTIC_COUNCIL.md).
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

public static class Patcher
{
    static Cloner cl;
    static ModuleDefinition mod;

    public static int Main(string[] args)
    {
        if (args.Length < 4) { Console.Error.WriteLine("usage: Patcher base.dll council.dll out.dll managedDir"); return 2; }
        try { Run(args[0], args[1], args[2], args[3]); return 0; }
        catch (Exception e) { Console.Error.WriteLine("PATCH FAILED: " + e); return 1; }
    }

    static void Run(string basePath, string councilPath, string outPath, string managedDir)
    {
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(managedDir);
        var asm = AssemblyDefinition.ReadAssembly(basePath, new ReaderParameters { AssemblyResolver = resolver });
        mod = asm.MainModule;
        int selfRefsBefore = CountSelfReferences();
        if (mod.GetType("CouncilGame") != null) throw new Exception("base assembly already contains the Council; start from the baseline assembly");

        RepairHackingModes();

        var council = AssemblyDefinition.ReadAssembly(councilPath, new ReaderParameters { AssemblyResolver = resolver });
        cl = new Cloner(council.MainModule, mod);
        cl.CloneAll();
        Console.WriteLine("cloned types: " + cl.NewTypes.Count);

        var game = cl.Mapped("CouncilGame");
        var ui = cl.Mapped("CouncilUi");
        var voteOrder = cl.Mapped("OrderCouncilVote");
        if (game == null || ui == null || voteOrder == null) throw new Exception("council types missing");

        HookClientBeginTurn(game);
        HookSerialize(game);
        HookTreasury(game, voteOrder);
        HookBackdoor(game);
        HookSleepers(game);
        HookAcademyWindow(ui);
        HookMarket(game);
        HookLoot(game);
        HookShipBlocks(game);
        HookProductionCost(game);
        HookBattleLaws(game);
        HookCompany(game);
        HookDecolonize(game);
        HookWarCreated(game);
        HookBeforeTurnEnd(game);
        HookVictoryContext(game);
        HookDebugTabs(cl.Mapped("CouncilDebug"));
        var hero = cl.Mapped("HeroRework");
        if (hero == null) throw new Exception("HeroRework missing");
        var academyOrder = cl.Mapped("OrderAcademyHeroKind");
        if (academyOrder == null) throw new Exception("OrderAcademyHeroKind missing");
        HookHeroRework(hero);
        HookEducation(hero, academyOrder);
        HookScience(hero);
        HideHackingNotice();
        DisableHackingEndTurnWarning();

        Console.WriteLine("publicized members: " + cl.PublicizedLog.Count);
        foreach (var s in cl.PublicizedLog.OrderBy(x => x)) Console.WriteLine("  + " + s);

        CheckSelfReferences(selfRefsBefore);
        CheckBranches();
        asm.Write(outPath);
        Console.WriteLine("written " + outPath);
    }

    // The baseline (hacking fixes commit 6ae43ff) contains CustomHackingHelper.GetValidModesForTarget with a broken
    // branch: `brfalse.s` at IL_0075 has an unresolved target (the real target, the final `ldloc.0; ret`, is more than
    // 127 bytes away). The JIT throws InvalidProgramException, which made the hack target un-pickable in the UI
    // (a system could only be chosen as a route node). Point the branch at the final `ldloc.0` and use the long form.
    // The popup "hacking operations are available / pirates and minor factions can hack" (vanilla NotificationMinorAndPirateHackingEnabled, raised once
    // by HackingManager.ClientBeginTurn) is not wanted: Bind returns false, so GuiNotificationManager.RecordEventForEmpire drops it. The event itself
    // is still raised (the AI listens to it).
    static void HideHackingNotice()
    {
        var m = Method("NotificationMinorAndPirateHackingEnabled", "Bind");
        var il = m.Body.GetILProcessor();
        var first = m.Body.Instructions[0];
        il.InsertBefore(first, il.Create(OpCodes.Ldc_I4_0));
        il.InsertBefore(first, il.Create(OpCodes.Ret));
        Console.WriteLine("hook: NotificationMinorAndPirateHackingEnabled.Bind returns false (popup hidden)");
    }

    // End-turn warning "hacking operations are available": DepartmentOfIntelligence.EndTurnValidator raises EventHackingOperationLeftUnassigned and
    // blocks the first press of "end turn" while a hacking slot is free and a target exists. It returns true (turn may end) at once now.
    static void DisableHackingEndTurnWarning()
    {
        var m = Method("DepartmentOfIntelligence", "EndTurnValidator");
        var il = m.Body.GetILProcessor();
        var first = m.Body.Instructions[0];
        il.InsertBefore(first, il.Create(OpCodes.Ldc_I4_1));
        il.InsertBefore(first, il.Create(OpCodes.Ret));
        Console.WriteLine("hook: DepartmentOfIntelligence.EndTurnValidator always lets the turn end (no unassigned-hacking warning)");
    }

    static void RepairHackingModes()
    {
        var m = Method("CustomHackingHelper", "GetValidModesForTarget");
        var ins = m.Body.Instructions;
        var last = ins[ins.Count - 1];
        var tail = ins[ins.Count - 2];
        if (last.OpCode != OpCodes.Ret || tail.OpCode != OpCodes.Ldloc_0) throw new Exception("GetValidModesForTarget: unexpected tail");
        int fixedCount = 0;
        foreach (var i in ins)
        {
            bool isBr = i.OpCode.OperandType == OperandType.ShortInlineBrTarget || i.OpCode.OperandType == OperandType.InlineBrTarget;
            if (!isBr || i.Operand != null) continue;
            if (i.OpCode == OpCodes.Brfalse_S || i.OpCode == OpCodes.Brfalse) i.OpCode = OpCodes.Brfalse;
            else throw new Exception("GetValidModesForTarget: unexpected broken branch " + i.OpCode);
            i.Operand = tail;
            fixedCount++;
        }
        LengthenBranches(m);
        Console.WriteLine("repair: GetValidModesForTarget broken branches fixed: " + fixedCount);
    }

    // Every branch/switch operand must resolve to an instruction of the same method (Mono 6 JIT tolerates some broken
    // branches that the game's older Mono rejects with InvalidProgramException).
    static void CheckBranches()
    {
        int bad = 0;
        Action<TypeDefinition> walk = null;
        walk = t =>
        {
            foreach (var md in t.Methods)
            {
                if (!md.HasBody) continue;
                var set = new HashSet<Instruction>(md.Body.Instructions);
                foreach (var i in md.Body.Instructions)
                {
                    var ot = i.OpCode.OperandType;
                    if (ot == OperandType.ShortInlineBrTarget || ot == OperandType.InlineBrTarget)
                    {
                        var target = i.Operand as Instruction;
                        if (target == null || !set.Contains(target)) { bad++; Console.WriteLine("BAD BRANCH " + t.FullName + "::" + md.Name + " IL_" + i.Offset.ToString("x4")); }
                    }
                    else if (ot == OperandType.InlineSwitch)
                    {
                        foreach (var x in (Instruction[])i.Operand)
                            if (x == null || !set.Contains(x)) { bad++; Console.WriteLine("BAD SWITCH " + t.FullName + "::" + md.Name); }
                    }
                }
            }
            foreach (var n in t.NestedTypes) walk(n);
        };
        foreach (var t in mod.Types) walk(t);
        if (bad > 0) throw new Exception("broken branches: " + bad);
        Console.WriteLine("branch check: ok");
    }

    static MethodDefinition Method(string type, string name, int paramCount = -1)
    {
        var t = mod.GetType(type);
        if (t == null) throw new Exception("type not found: " + type);
        var m = t.Methods.FirstOrDefault(x => x.Name == name && (paramCount < 0 || x.Parameters.Count == paramCount));
        if (m == null) throw new Exception("method not found: " + type + "::" + name);
        return m;
    }

    static MethodDefinition GameMethod(TypeDefinition t, string name)
    {
        var m = t.Methods.FirstOrDefault(x => x.Name == name);
        if (m == null) throw new Exception("council method not found: " + name);
        return m;
    }

    // ------------------------------------------------------------------ hooks

    // AcademyRequestsManager.ClientBeginTurn() => CouncilGame.ClientBeginTurn(this)
    static void HookClientBeginTurn(TypeDefinition game)
    {
        var m = Method("AcademyRequestsManager", "ClientBeginTurn");
        var il = m.Body.GetILProcessor();
        m.Body.Instructions.Clear();
        m.Body.ExceptionHandlers.Clear();
        m.Body.Variables.Clear();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, GameMethod(game, "ClientBeginTurn"));
        il.Emit(OpCodes.Ret);
        Console.WriteLine("hook: AcademyRequestsManager.ClientBeginTurn replaced");
    }

    // AcademyRequestsManager.SerializeAttributes(...) : append CouncilGame.SerializeAttributes(serializer, versionProvider)
    static void HookSerialize(TypeDefinition game)
    {
        var m = Method("AcademyRequestsManager", "SerializeAttributes");
        var ins = m.Body.Instructions;
        var ret = ins.Last();
        if (ret.OpCode != OpCodes.Ret) throw new Exception("unexpected end of SerializeAttributes");
        var il = m.Body.GetILProcessor();
        il.InsertBefore(ret, il.Create(OpCodes.Ldarg_1));
        il.InsertBefore(ret, il.Create(OpCodes.Ldarg_2));
        il.InsertBefore(ret, il.Create(OpCodes.Call, GameMethod(game, "SerializeAttributes")));
        Console.WriteLine("hook: AcademyRequestsManager.SerializeAttributes extended");
    }

    // DepartmentOfTheTreasury: [OrderProcessor(typeof(OrderCouncilVote), "CouncilVote")] CouncilVotePreprocessor / CouncilVoteProcessor
    static void HookTreasury(TypeDefinition game, TypeDefinition voteOrder)
    {
        var treasury = mod.GetType("DepartmentOfTheTreasury");
        var template = treasury.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "OrderProcessorAttribute");
        if (template == null) throw new Exception("no OrderProcessor template attribute");
        var proto = treasury.Methods.First(x => x.Name == "TransferResourcePreprocessor");
        var protoProc = treasury.Methods.First(x => x.Name == "TransferResourceProcessor");

        var pre = new MethodDefinition("CouncilVotePreprocessor", proto.Attributes, proto.ReturnType);
        pre.Parameters.Add(new ParameterDefinition("order", ParameterAttributes.None, voteOrder));
        var il = pre.Body.GetILProcessor();
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Call, GameMethod(game, "VotePreprocessor"));
        il.Emit(OpCodes.Ret);
        treasury.Methods.Add(pre);

        var proc = new MethodDefinition("CouncilVoteProcessor", protoProc.Attributes, protoProc.ReturnType);
        proc.Parameters.Add(new ParameterDefinition("order", ParameterAttributes.None, voteOrder));
        il = proc.Body.GetILProcessor();
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Call, GameMethod(game, "VoteProcessor"));
        il.Emit(OpCodes.Ret);
        treasury.Methods.Add(proc);

        var attr = new CustomAttribute(template.Constructor);
        attr.ConstructorArguments.Add(new CustomAttributeArgument(template.ConstructorArguments[0].Type, voteOrder));
        attr.ConstructorArguments.Add(new CustomAttributeArgument(template.ConstructorArguments[1].Type, "CouncilVote"));
        treasury.CustomAttributes.Add(attr);
        Console.WriteLine("hook: DepartmentOfTheTreasury order processor CouncilVote added");
    }

    // DepartmentOfEducation: [OrderProcessor(typeof(OrderAcademyHeroKind), "AcademyHeroKind")] AcademyHeroKindPreprocessor / AcademyHeroKindProcessor
    // (the "variant 2b" Academy offer: the pending vanilla trio becomes a pair of the picked hero kind)
    static void HookEducation(TypeDefinition hero, TypeDefinition academyOrder)
    {
        var edu = mod.GetType("DepartmentOfEducation");
        var template = edu.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "OrderProcessorAttribute");
        if (template == null) throw new Exception("no OrderProcessor template attribute on DepartmentOfEducation");
        var proto = mod.GetType("DepartmentOfTheTreasury").Methods.First(x => x.Name == "TransferResourcePreprocessor");
        var protoProc = mod.GetType("DepartmentOfTheTreasury").Methods.First(x => x.Name == "TransferResourceProcessor");

        var pre = new MethodDefinition("AcademyHeroKindPreprocessor", proto.Attributes, proto.ReturnType);
        pre.Parameters.Add(new ParameterDefinition("order", ParameterAttributes.None, academyOrder));
        var il = pre.Body.GetILProcessor();
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Call, GameMethod(hero, "AcademyHeroKindPreprocessor"));
        il.Emit(OpCodes.Ret);
        edu.Methods.Add(pre);

        var proc = new MethodDefinition("AcademyHeroKindProcessor", protoProc.Attributes, protoProc.ReturnType);
        proc.Parameters.Add(new ParameterDefinition("order", ParameterAttributes.None, academyOrder));
        il = proc.Body.GetILProcessor();
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Call, GameMethod(hero, "AcademyHeroKindProcessor"));
        il.Emit(OpCodes.Ret);
        edu.Methods.Add(proc);

        var attr = new CustomAttribute(template.Constructor);
        attr.ConstructorArguments.Add(new CustomAttributeArgument(template.ConstructorArguments[0].Type, academyOrder));
        attr.ConstructorArguments.Add(new CustomAttributeArgument(template.ConstructorArguments[1].Type, "AcademyHeroKind"));
        edu.CustomAttributes.Add(attr);
        Console.WriteLine("hook: DepartmentOfEducation order processor AcademyHeroKind added");
    }

    // DepartmentOfScience.UnlockTechnology ends -> HeroRework.OnTechnologyUnlocked(department):
    // a freshly researched cloaking/detection technology raises the mirrored Seeker skills of the empire's heroes.
    static void HookScience(TypeDefinition hero)
    {
        var dos = mod.GetType("DepartmentOfScience");
        var hook = GameMethod(hero, "OnTechnologyUnlocked");
        int n = 0;
        foreach (var m in dos.Methods)
        {
            if (m.Name != "UnlockTechnology" || !m.HasBody || m.IsStatic) continue;
            n += WrapReturns(m, hook, new[] { OpCodes.Ldarg_0 });
        }
        if (n == 0) throw new Exception("DepartmentOfScience.UnlockTechnology was not hooked");
        Console.WriteLine("hook: DepartmentOfScience.UnlockTechnology ends call OnTechnologyUnlocked: " + n);
    }

    // GameClient.SpawnHackingOperationBackdoorProcessor: stop before doing anything when the Counterintel law is active
    static void HookBackdoor(TypeDefinition game)
    {
        var it = mod.GetType("GameClient").NestedTypes.First(t => t.Name.StartsWith("<SpawnHackingOperationBackdoorProcessor>"));
        var m = it.Methods.First(x => x.Name == "MoveNext");
        var ins = m.Body.Instructions;
        // find the "brtrue" that dispatches on $PC, the first instruction after it is the start of the first-run path
        Instruction brtrue = null;
        for (int i = 0; i < 8 && i < ins.Count; i++) if (ins[i].OpCode == OpCodes.Brtrue) { brtrue = ins[i]; break; }
        if (brtrue == null) throw new Exception("PC dispatch not found in " + it.FullName);
        var next = brtrue.Next;
        var il = m.Body.GetILProcessor();
        il.InsertBefore(next, il.Create(OpCodes.Call, GameMethod(game, "BlocksNewBackdoor")));
        il.InsertBefore(next, il.Create(OpCodes.Brfalse, next));
        il.InsertBefore(next, il.Create(OpCodes.Ldc_I4_0));
        il.InsertBefore(next, il.Create(OpCodes.Ret));
        Console.WriteLine("hook: backdoor spawn blocked by Counterintel law");
    }

    // sleeper-count call sites: CustomHackingHelper.GetHackerSleeperCountOnSystem -> CouncilGame.SleepersForHack
    static void HookSleepers(TypeDefinition game)
    {
        var wrapper = GameMethod(game, "SleepersForHack");
        int n = 0;
        foreach (var spec in new[] { "OverhaulOutcomeDefinition::CheckHackingEmpireAndTarget", "CustomHackingHelper::BuildPromptElement" })
        {
            var parts = spec.Split(new[] { "::" }, StringSplitOptions.None);
            var m = Method(parts[0], parts[1]);
            foreach (var i in m.Body.Instructions)
            {
                var mr = i.Operand as MethodReference;
                if (i.OpCode == OpCodes.Call && mr != null && mr.Name == "GetHackerSleeperCountOnSystem") { i.Operand = wrapper; n++; }
            }
        }
        if (n != 2) throw new Exception("expected 2 sleeper call sites, patched " + n);
        Console.WriteLine("hook: sleeper count call sites patched: " + n);
    }

    // AcademyModalWindow.OnBeginShow/OnBeginHide -> show/hide the launcher button
    static void HookAcademyWindow(TypeDefinition ui)
    {
        foreach (var pair in new[] { new[] { "OnBeginShow", "OnAcademyWindowShown" }, new[] { "OnBeginHide", "OnAcademyWindowHidden" } })
        {
            var m = Method("AcademyModalWindow", pair[0]);
            var il = m.Body.GetILProcessor();
            il.InsertBefore(m.Body.Instructions[0], il.Create(OpCodes.Call, GameMethod(ui, pair[1])));
        }
        Console.WriteLine("hook: AcademyModalWindow show/hide");
    }

    // Rewrites every `ret` of m in place into `<push extra args> call hook; ret`. The original ret instruction object is
    // reused (opcode/operand rewritten) so that branches targeting it stay valid.
    // Cecil without Rocks: turn short branch forms into long ones so that growing the method cannot overflow them.
    static void LengthenBranches(MethodDefinition m)
    {
        foreach (var i in m.Body.Instructions)
        {
            if (i.OpCode == OpCodes.Br_S) i.OpCode = OpCodes.Br;
            else if (i.OpCode == OpCodes.Brfalse_S) i.OpCode = OpCodes.Brfalse;
            else if (i.OpCode == OpCodes.Brtrue_S) i.OpCode = OpCodes.Brtrue;
            else if (i.OpCode == OpCodes.Beq_S) i.OpCode = OpCodes.Beq;
            else if (i.OpCode == OpCodes.Bge_S) i.OpCode = OpCodes.Bge;
            else if (i.OpCode == OpCodes.Bgt_S) i.OpCode = OpCodes.Bgt;
            else if (i.OpCode == OpCodes.Ble_S) i.OpCode = OpCodes.Ble;
            else if (i.OpCode == OpCodes.Blt_S) i.OpCode = OpCodes.Blt;
            else if (i.OpCode == OpCodes.Bne_Un_S) i.OpCode = OpCodes.Bne_Un;
            else if (i.OpCode == OpCodes.Bge_Un_S) i.OpCode = OpCodes.Bge_Un;
            else if (i.OpCode == OpCodes.Bgt_Un_S) i.OpCode = OpCodes.Bgt_Un;
            else if (i.OpCode == OpCodes.Ble_Un_S) i.OpCode = OpCodes.Ble_Un;
            else if (i.OpCode == OpCodes.Blt_Un_S) i.OpCode = OpCodes.Blt_Un;
            else if (i.OpCode == OpCodes.Leave_S) i.OpCode = OpCodes.Leave;
        }
    }

    static int WrapReturns(MethodDefinition m, MethodDefinition hook, OpCode[] extraLoads)
    {
        LengthenBranches(m);
        var il = m.Body.GetILProcessor();
        var rets = m.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
        foreach (var r in rets)
        {
            var seq = new List<Instruction>();
            for (int k = 1; k < extraLoads.Length; k++) seq.Add(il.Create(extraLoads[k]));
            seq.Add(il.Create(OpCodes.Call, hook));
            seq.Add(il.Create(OpCodes.Ret));
            r.OpCode = extraLoads[0];
            r.Operand = null;
            var at = r;
            foreach (var x in seq) { il.InsertAfter(at, x); at = x; }
        }
        return rets.Count;
    }

    // H1: Tradable.GetValue / TradableResource.GetValue(type, qty)    => AdjustTradableValue(value, this, type)
    // H2: Marketplace ITradingManagementService.GetTradablePrice(...) => AdjustTradePrice(price, type, empire)
    static void HookMarket(TypeDefinition game)
    {
        var adjValue = GameMethod(game, "AdjustTradableValue");
        int n = 0;
        foreach (var tn in new[] { "Tradable", "TradableResource" })
        {
            var m = Method(tn, "GetValue", 2);
            n += WrapReturns(m, adjValue, new[] { OpCodes.Ldarg_0, OpCodes.Ldarg_1 });
        }
        Console.WriteLine("hook: tradable value returns wrapped: " + n);

        var mp = mod.GetType("Marketplace");
        var gp = mp.Methods.Where(x => x.Name.EndsWith("GetTradablePrice") && x.Parameters.Count == 4).ToList();
        if (gp.Count != 1) throw new Exception("expected 1 GetTradablePrice, found " + gp.Count);
        int k = WrapReturns(gp[0], GameMethod(game, "AdjustTradePrice"), new[] { OpCodes.Ldarg_1, OpCodes.Ldarg_3 });
        Console.WriteLine("hook: GetTradablePrice returns wrapped: " + k);
    }

    // Scrap/sell preprocessors refuse orders that touch ships of a council squad.
    static void HookShipBlocks(TypeDefinition game)
    {
        var block = GameMethod(game, "BlocksShips");
        foreach (var spec in new[] { new[] { "DepartmentOfDefense", "ScrapShipsPreprocessor", "OrderScrapShips" },
                                     new[] { "DepartmentOfCommerce", "SelloutTradableShipsPreprocessor", "OrderSelloutTradableShips" } })
        {
            var m = Method(spec[0], spec[1]);
            var getter = mod.GetType(spec[2]).Methods.First(x => x.Name == "get_ShipsGUID");
            var il = m.Body.GetILProcessor();
            var first = m.Body.Instructions[0];
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, il.Create(OpCodes.Callvirt, getter));
            il.InsertBefore(first, il.Create(OpCodes.Call, block));
            il.InsertBefore(first, il.Create(OpCodes.Brfalse, first));
            il.InsertBefore(first, il.Create(OpCodes.Ldc_I4_0));
            il.InsertBefore(first, il.Create(OpCodes.Ret));
            Console.WriteLine("hook: " + spec[1] + " blocks council ships");
        }
    }

    // H4: DepartmentOfTheTreasury.GetProductionCostWithBonus(context, provider, cost, float) - the sink all cost overloads call
    static void HookProductionCost(TypeDefinition game)
    {
        var t = mod.GetType("DepartmentOfTheTreasury");
        var m = t.Methods.Where(x => x.Name == "GetProductionCostWithBonus" && x.Parameters.Count == 4 && x.Parameters[3].ParameterType.Name == "Single").ToList();
        if (m.Count != 1 || !m[0].IsStatic) throw new Exception("GetProductionCostWithBonus(float) not found/static");
        int n = WrapReturns(m[0], GameMethod(game, "AdjustProductionCost"), new[] { OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2 });
        Console.WriteLine("hook: production cost returns wrapped: " + n);
    }


    // WrapReturns with arbitrary argument slots (0 = this, n = parameter n-1); the first slot reuses the original ret.
    static int WrapReturnsSlots(MethodDefinition m, MethodDefinition hook, int[] slots)
    {
        LengthenBranches(m);
        var il = m.Body.GetILProcessor();
        var rets = m.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
        Func<int, Instruction> load = s => s == 0 ? il.Create(OpCodes.Ldarg_0) : il.Create(OpCodes.Ldarg, m.Parameters[s - 1]);
        foreach (var r in rets)
        {
            var first = load(slots[0]);
            var seq = new List<Instruction>();
            for (int k = 1; k < slots.Length; k++) seq.Add(load(slots[k]));
            seq.Add(il.Create(OpCodes.Call, hook));
            seq.Add(il.Create(OpCodes.Ret));
            r.OpCode = first.OpCode; r.Operand = first.Operand;
            var at = r;
            foreach (var x in seq) { il.InsertAfter(at, x); at = x; }
        }
        return rets.Count;
    }

    // Simplified battle damage (-20% law), ground battle modifiers (Fortification Inspection / Capital Reinforcement)
    static void HookBattleLaws(TypeDefinition game)
    {
        var m = Method("ServerEncounterState_RunSimpleBattle", "ComputeSimplifiedEffectiveDamagePercentage");
        int n = WrapReturnsSlots(m, GameMethod(game, "AdjustSimplifiedDamage"), new[] { 1, 4, 7, 8 });
        Console.WriteLine("hook: simplified battle damage returns wrapped: " + n);
        var g = Method("ServerGroundBattle", "GetColonizedStarSystemOpponentPropertyValue");
        n = WrapReturns(g, GameMethod(game, "AdjustOpponentProp"), new[] { OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 });
        Console.WriteLine("hook: ground battle opponent property returns wrapped: " + n);
    }

    // Antitrust Oversight: DepartmentOfCommerce.GetTradingCompanyIncomeMultiplier(company)
    static void HookCompany(TypeDefinition game)
    {
        var m = Method("DepartmentOfCommerce", "GetTradingCompanyIncomeMultiplier");
        int n = WrapReturns(m, GameMethod(game, "AdjustCompanyIncome"), new[] { OpCodes.Ldarg_1 });
        Console.WriteLine("hook: trading company income multiplier returns wrapped: " + n);
    }

    // Evacuation compensation: DecolonizeProcessor calls RemoveColonizedStarSystemAndPlanets; call OnDecolonize(order, system) first.
    static void HookDecolonize(TypeDefinition game)
    {
        var it = mod.GetType("DepartmentOfTheInterior").NestedTypes.First(t => t.Name == "<DecolonizeProcessor>c__Iterator1B");
        var mv = it.Methods.First(x => x.Name == "MoveNext");
        var orderField = it.Fields.First(f => f.Name == "order");
        var calls = mv.Body.Instructions.Where(i => i.OpCode.Code == Code.Call && ((MethodReference)i.Operand).Name == "RemoveColonizedStarSystemAndPlanets").ToList();
        if (calls.Count != 1) throw new Exception("expected 1 RemoveColonizedStarSystemAndPlanets call, found " + calls.Count);
        var call = calls[0];
        var sysType = ((MethodReference)call.Operand).Parameters[0].ParameterType;
        var intType = ((MethodReference)call.Operand).DeclaringType;
        var vSys = new VariableDefinition(sysType);
        var vInt = new VariableDefinition(intType);
        mv.Body.Variables.Add(vSys);
        mv.Body.Variables.Add(vInt);
        mv.Body.InitLocals = true;
        var il = mv.Body.GetILProcessor();
        il.InsertBefore(call, il.Create(OpCodes.Stloc, vSys));
        il.InsertBefore(call, il.Create(OpCodes.Stloc, vInt));
        il.InsertBefore(call, il.Create(OpCodes.Ldarg_0));
        il.InsertBefore(call, il.Create(OpCodes.Ldfld, orderField));
        il.InsertBefore(call, il.Create(OpCodes.Ldloc, vSys));
        il.InsertBefore(call, il.Create(OpCodes.Call, GameMethod(game, "OnDecolonize")));
        il.InsertBefore(call, il.Create(OpCodes.Ldloc, vInt));
        il.InsertBefore(call, il.Create(OpCodes.Ldloc, vSys));
        Console.WriteLine("hook: DecolonizeProcessor calls OnDecolonize before removing the system");
    }

    // GameClient.CreateWarInstanceProcessor: right after CreateWarInstance(...) returned the new WarInstance => CouncilGame.OnWarCreated(war)
    // Council squads spawned during a turn must be known (and their upkeep waived) before the end-of-turn bailiff check.
    // Both targets are iterator coroutines: the call is inserted at the start of state 0 of the compiler-generated MoveNext.
    static void HookBeforeTurnEnd(TypeDefinition game)
    {
        var hook = GameMethod(game, "BeforeTurnEnd");
        var treasury = mod.GetType("DepartmentOfTheTreasury");
        foreach (var name in new[] { "GameServerState_ComputeBailiff", "GameClientState_Turn_End_CollectResources" })
        {
            var its = treasury.NestedTypes.Where(t => t.Name.StartsWith("<" + name + ">")).ToList();
            if (its.Count != 1) throw new Exception("iterator of " + name + " not found/ambiguous: " + its.Count);
            var mn = its[0].Methods.First(x => x.Name == "MoveNext");
            var ins = mn.Body.Instructions;
            Instruction first = null;
            for (int i = 0; i < ins.Count && first == null; i++)
            {
                if (ins[i].OpCode == OpCodes.Switch) first = ((Instruction[])ins[i].Operand)[0];
                else if (ins[i].OpCode == OpCodes.Brtrue || ins[i].OpCode == OpCodes.Brtrue_S) first = ins[i + 1];
            }
            if (first == null) throw new Exception("state 0 of " + name + " not found");
            var call = Instruction.Create(OpCodes.Call, hook);
            var il = mn.Body.GetILProcessor();
            il.InsertBefore(first, call);
            // branches that targeted the old first instruction must now enter through the hook call
            foreach (var x in ins)
            {
                if (x == call) continue;
                if (x.Operand == first) x.Operand = call;
                else if (x.Operand is Instruction[])
                {
                    var arr = (Instruction[])x.Operand;
                    for (int k = 0; k < arr.Length; k++) if (arr[k] == first) arr[k] = call;
                }
            }
            Console.WriteLine("hook: " + its[0].Name + ".MoveNext state 0 calls BeforeTurnEnd");
        }
    }

    static void HookWarCreated(TypeDefinition game)
    {
        var it = mod.GetType("GameClient").NestedTypes.First(t => t.Name == "<CreateWarInstanceProcessor>c__Iterator29");
        var mv = it.Methods.First(x => x.Name == "MoveNext");
        var calls = mv.Body.Instructions.Where(i => (i.OpCode.Code == Code.Callvirt || i.OpCode.Code == Code.Call) && ((MethodReference)i.Operand).Name == "CreateWarInstance").ToList();
        if (calls.Count != 1) throw new Exception("expected 1 CreateWarInstance call, found " + calls.Count);
        var store = calls[0].Next;
        VariableDefinition v;
        switch (store.OpCode.Code)
        {
            case Code.Stloc_0: v = mv.Body.Variables[0]; break;
            case Code.Stloc_1: v = mv.Body.Variables[1]; break;
            case Code.Stloc_2: v = mv.Body.Variables[2]; break;
            case Code.Stloc_3: v = mv.Body.Variables[3]; break;
            case Code.Stloc: case Code.Stloc_S: v = (VariableDefinition)store.Operand; break;
            default: throw new Exception("CreateWarInstance result is not stored in a local: " + store.OpCode);
        }
        var il = mv.Body.GetILProcessor();
        var load = il.Create(OpCodes.Ldloc, v);
        il.InsertAfter(store, load);
        il.InsertAfter(load, il.Create(OpCodes.Call, GameMethod(game, "OnWarCreated")));
        Console.WriteLine("hook: CreateWarInstanceProcessor calls OnWarCreated");
    }

    // God-Mod menu: Council and hero test tools are appended to the end of two existing tabs (static (Game, MajorEmpire) methods)
    static void HookDebugTabs(TypeDefinition dbg)
    {
        if (dbg == null) throw new Exception("CouncilDebug missing");
        foreach (var spec in new[] { new[] { "DrawTabTechLawsHeroes", "DrawHeroTools" }, new[] { "DrawTabDiplomacyAndEmpires", "DrawCouncilTools" } })
        {
            var m = Method("CustomHackingHelper", spec[0]);
            if (!m.IsStatic || m.Parameters.Count != 2) throw new Exception("unexpected signature of " + spec[0]);
            var target = GameMethod(dbg, spec[1]);
            LengthenBranches(m);
            var il = m.Body.GetILProcessor();
            var rets = m.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
            foreach (var r in rets)
            {
                r.OpCode = OpCodes.Ldarg_0; r.Operand = null;
                var seq = new List<Instruction> { il.Create(OpCodes.Ldarg_1), il.Create(OpCodes.Call, target), il.Create(OpCodes.Ret) };
                var at = r;
                foreach (var x in seq) { il.InsertAfter(at, x); at = x; }
            }
            Console.WriteLine("hook: " + spec[0] + " ends with " + spec[1] + " (returns wrapped: " + rets.Count + ")");
        }
    }

    // VictoryManager.SetupInterpreterContextForContender(contender) end => AdjustVictoryContext(this, contender.Empires)
    static void HookVictoryContext(TypeDefinition game)
    {
        var vm = mod.GetType("VictoryManager");
        var m = vm.Methods.First(x => x.Name == "SetupInterpreterContextForContender");
        var contender = vm.NestedTypes.First(t => t.Name == "VictoryContender");
        var empires = contender.Fields.First(f => f.Name == "Empires");
        LengthenBranches(m);
        var il = m.Body.GetILProcessor();
        var rets = m.Body.Instructions.Where(i => i.OpCode == OpCodes.Ret).ToList();
        foreach (var r in rets)
        {
            // rewrite the ret in place (branches may target it), append the call after it
            r.OpCode = OpCodes.Ldarg_0; r.Operand = null;
            var seq = new List<Instruction> { il.Create(OpCodes.Ldarg_1), il.Create(OpCodes.Ldfld, empires), il.Create(OpCodes.Call, GameMethod(game, "AdjustVictoryContext")), il.Create(OpCodes.Ret) };
            var at = r;
            foreach (var x in seq) { il.InsertAfter(at, x); at = x; }
        }
        Console.WriteLine("hook: victory interpreter context returns wrapped: " + rets.Count);
    }

    // Hero rework: effect channels per hero kind, assignment rules, senate representative, skill trees, window title
    static void HookHeroRework(TypeDefinition hero)
    {
        int n = 0;
        foreach (var spec in new[] {
            new[] { "GetShipSkillDescriptors", "FilterShip" },
            new[] { "GetOnOrbitSkillDescriptors", "FilterShip" },
            new[] { "GetOnOwnedOrbitSkillDescriptors", "FilterShip" },
            new[] { "GetOnHostileOrbitSkillDescriptors", "FilterShip" },
            new[] { "GetOnNeutralOrbitSkillDescriptors", "FilterShip" },
            new[] { "GetOnFriendlyOrbitSkillDescriptors", "FilterShip" },
            new[] { "GetSystemSkillDescriptors", "FilterSystem" },
            new[] { "GetSenateSkillDescriptors", "FilterSenate" } })
            n += WrapReturns(Method("Hero", spec[0]), GameMethod(hero, spec[1]), new[] { OpCodes.Ldarg_0 });
        Console.WriteLine("hook: hero skill descriptor returns wrapped: " + n);

        n = WrapReturns(Method("Hero", "get_SkillTrees"), GameMethod(hero, "TreesFor"), new[] { OpCodes.Ldarg_0 });
        Console.WriteLine("hook: Hero.get_SkillTrees returns wrapped: " + n);
        n = WrapReturns(Method("GuiHero", "get_Title"), GameMethod(hero, "TitleWithKind"), new[] { OpCodes.Ldarg_0 });
        Console.WriteLine("hook: GuiHero.get_Title returns wrapped: " + n);

        // assignment rules: if (!HeroRework.AllowAssign(hero, ref info, toFleet)) return false;
        var allow = GameMethod(hero, "AllowAssign");
        foreach (var spec in new[] { new object[] { "CheckHeroAssignableToFleet", true }, new object[] { "CheckHeroAssignableToSystem", false } })
        {
            var m = Method("CustomHackingHelper", (string)spec[0]);
            var il = m.Body.GetILProcessor();
            var first = m.Body.Instructions[0];
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, il.Create((bool)spec[1] ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0));
            il.InsertBefore(first, il.Create(OpCodes.Call, allow));
            il.InsertBefore(first, il.Create(OpCodes.Brtrue, first));
            il.InsertBefore(first, il.Create(OpCodes.Ldc_I4_0));
            il.InsertBefore(first, il.Create(OpCodes.Ret));
            Console.WriteLine("hook: " + spec[0] + " checks the hero kind");
        }

        n = WrapReturns(Method("Hero", "ComputeAvailableSkills"), GameMethod(hero, "AfterComputeAvailable"), new[] { OpCodes.Ldarg_0 });
        Console.WriteLine("hook: Hero.ComputeAvailableSkills ends call AfterComputeAvailable: " + n);

        // special ultimates of the admirals: GameClient.ApplyDamage(encounter, damage, attacker, ref xp, out surviving, out damaged, out destroyed) end
        // => HeroRework.OnShipsDestroyed(encounter, attacker, ref destroyed)
        n = WrapReturnsSlots(Method("GameClient", "ApplyDamage", 7), GameMethod(hero, "OnShipsDestroyed"), new[] { 1, 3, 7 });
        Console.WriteLine("hook: GameClient.ApplyDamage (simplified battles) ends call OnShipsDestroyed: " + n);

        // stage 3: Hero.UpdateLevel(bool) start => HeroRework.BeforeUpdateLevel(this)
        {
            var m = Method("Hero", "UpdateLevel", 1);
            var il = m.Body.GetILProcessor();
            var first = m.Body.Instructions[0];
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, il.Create(OpCodes.Call, GameMethod(hero, "BeforeUpdateLevel")));
            Console.WriteLine("hook: Hero.UpdateLevel caps the level of senators");
        }
        // Senate.ServerTriggerRebellion start: if (HeroRework.BlockRebellion(this)) return;
        {
            var m = Method("Senate", "ServerTriggerRebellion");
            var il = m.Body.GetILProcessor();
            var first = m.Body.Instructions[0];
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, il.Create(OpCodes.Call, GameMethod(hero, "BlockRebellion")));
            il.InsertBefore(first, il.Create(OpCodes.Brfalse, first));
            il.InsertBefore(first, il.Create(OpCodes.Ret));
            Console.WriteLine("hook: Senate.ServerTriggerRebellion blocked while a dictator rules");
        }
        // Senate.ParseElectionResult start => HeroRework.LockElection(this, electionResult)
        {
            var m = Method("Senate", "ParseElectionResult");
            var il = m.Body.GetILProcessor();
            var first = m.Body.Instructions[0];
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_1));
            il.InsertBefore(first, il.Create(OpCodes.Call, GameMethod(hero, "LockElection")));
            Console.WriteLine("hook: Senate.ParseElectionResult keeps the ideology of the dictator");
        }

        // unspent skill points: a hero who took his ultimate has nothing to spend them on => Hero.get_AvailableSkillPoints returns 0
        n = WrapReturns(Method("Hero", "get_AvailableSkillPoints"), GameMethod(hero, "AdjustAvailablePoints"), new[] { OpCodes.Ldarg_0 });
        Console.WriteLine("hook: Hero.get_AvailableSkillPoints returns wrapped: " + n);

        // dictator hero definitions exist before a save is read: DepartmentOfEducation.SerializeElements start => HeroRework.EnsureDictatorDefinitions()
        {
            var m = Method("DepartmentOfEducation", "SerializeElements");
            var il = m.Body.GetILProcessor();
            var first = m.Body.Instructions[0];
            il.InsertBefore(first, il.Create(OpCodes.Call, GameMethod(hero, "EnsureDictatorDefinitions")));
            Console.WriteLine("hook: DepartmentOfEducation.SerializeElements registers the dictator hero definitions first");
        }

        // stage 4: HeroSkill.Load start => HeroRework.EnsureContent() (skills of the rework exist before a save looks them up)
        {
            var m = Method("HeroSkill", "Load");
            var il = m.Body.GetILProcessor();
            var first = m.Body.Instructions[0];
            il.InsertBefore(first, il.Create(OpCodes.Call, GameMethod(hero, "EnsureContent")));
            Console.WriteLine("hook: HeroSkill.Load registers the skills of the rework first");
        }

        // Senator.UpdateRepresentativeHero: if (HeroRework.UpdateRepresentative(this)) return;
        {
            var m = Method("Senator", "UpdateRepresentativeHero");
            var il = m.Body.GetILProcessor();
            var first = m.Body.Instructions[0];
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, il.Create(OpCodes.Call, GameMethod(hero, "UpdateRepresentative")));
            il.InsertBefore(first, il.Create(OpCodes.Brfalse, first));
            il.InsertBefore(first, il.Create(OpCodes.Ret));
            Console.WriteLine("hook: Senator.UpdateRepresentativeHero restricted to senator heroes");
        }

        // variant 2b: HeroRecruitmentNotificationWindow.Refresh start => HeroRework.OnRecruitNoteRefresh(this)
        // (a fresh vanilla trio makes the local player first pick the kind of hero)
        {
            var m = Method("HeroRecruitmentNotificationWindow", "Refresh");
            var il = m.Body.GetILProcessor();
            var first = m.Body.Instructions[0];
            il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
            il.InsertBefore(first, il.Create(OpCodes.Call, GameMethod(hero, "OnRecruitNoteRefresh")));
            Console.WriteLine("hook: HeroRecruitmentNotificationWindow.Refresh asks the hero kind");
        }
    }

    // H3: GroundBattleOutcomeLootDefinition.FillLoot(battle, loot, populations) => ...; AdjustLoot(battle, loot)
    static void HookLoot(TypeDefinition game)
    {
        var m = Method("GroundBattleOutcomeLootDefinition", "FillLoot");
        int n = WrapReturns(m, GameMethod(game, "AdjustLoot"), new[] { OpCodes.Ldarg_1, OpCodes.Ldarg_2 });
        Console.WriteLine("hook: FillLoot returns wrapped: " + n);
    }

    // The baseline assembly already carries some TypeRefs that point at "Assembly-CSharp" itself (this is known to load
    // fine in the game); the patch must not add new ones.
    static int CountSelfReferences()
    {
        string self = mod.Assembly.Name.Name;
        return mod.GetTypeReferences().Count(t => t.Scope is AssemblyNameReference && ((AssemblyNameReference)t.Scope).Name == self);
    }

    static void CheckSelfReferences(int before)
    {
        int now = CountSelfReferences();
        Console.WriteLine("self type references: before=" + before + " after=" + now);
        if (now > before) throw new Exception("patch introduced new self references");
    }
}

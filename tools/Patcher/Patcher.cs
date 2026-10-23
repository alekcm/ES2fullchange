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

        Console.WriteLine("publicized members: " + cl.PublicizedLog.Count);
        foreach (var s in cl.PublicizedLog.OrderBy(x => x)) Console.WriteLine("  + " + s);

        CheckSelfReferences(selfRefsBefore);
        asm.Write(outPath);
        Console.WriteLine("written " + outPath);
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

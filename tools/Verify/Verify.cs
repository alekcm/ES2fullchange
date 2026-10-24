// Loads the patched assembly in Mono and force-JITs every Council method and every hooked game method.
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

public static class Verify
{
    static int ok, fail;
    static void Prep(MethodBase m)
    {
        if (m.IsAbstract || m.ContainsGenericParameters) return;
        try { RuntimeHelpers.PrepareMethod(m.MethodHandle); ok++; }
        catch (Exception e) { fail++; Console.WriteLine("JIT FAIL " + m.DeclaringType + "::" + m.Name + " : " + e.GetType().Name + " " + e.Message); }
    }
    static void PrepType(Type t)
    {
        const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        foreach (var m in t.GetMethods(F)) Prep(m);
        foreach (var c in t.GetConstructors(F)) Prep(c);
        foreach (var n in t.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)) PrepType(n);
    }
    public static int Main(string[] a)
    {
        var asm = Assembly.LoadFrom(a[0]);
        Type[] types;
        try { types = asm.GetTypes(); }
        catch (ReflectionTypeLoadException e) { foreach (var x in e.LoaderExceptions) Console.WriteLine("LOAD " + x.Message); return 1; }
        string[] names = { "CouncilGame", "CouncilUi", "CouncilUiBehaviour", "OrderCouncilVote", "CouncilCore", "CouncilLaws" };
        foreach (var t in types) if (t.Name.StartsWith("Council") || t.Name == "OrderCouncilVote" || t.Name.StartsWith("Hero") && (t.Name == "HeroRework" || t.Name == "HeroKinds")) PrepType(t);
        foreach (var n in new[] { "AcademyRequestsManager", "AcademyModalWindow", "DepartmentOfTheTreasury", "OverhaulOutcomeDefinition", "CustomHackingHelper", "Tradable", "TradableResource", "Marketplace", "GroundBattleOutcomeLootDefinition", "DepartmentOfDefense", "DepartmentOfCommerce", "ServerEncounterState_RunSimpleBattle", "ServerGroundBattle", "DepartmentOfTheInterior", "VictoryManager", "Hero", "GuiHero", "Senator" })
            PrepType(asm.GetType(n));
        PrepType(asm.GetType("GameClient").GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public).First(t => t.Name.StartsWith("<SpawnHackingOperationBackdoorProcessor>")));
        Console.WriteLine("jitted ok=" + ok + " fail=" + fail);
        return fail == 0 ? 0 : 1;
    }
}

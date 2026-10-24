// Pure logic of the "variant 2b" Academy offer - no game state, so the tests can exercise it without the
// game assemblies (HeroRework itself carries static fields of game types and cannot be loaded there).
using System;
using System.Collections.Generic;

public static class HeroAcademy
{
    /// <summary>Picks the offered pair out of the candidates: sorting makes the pool order irrelevant
    /// (the Academy pool list may be ordered differently in memory), then the game PRNG shuffles and the
    /// first two names are offered. The same candidates and seed give the same pair on every machine.
    /// One candidate gives a "pair" of one, an empty list gives nothing.</summary>
    public static List<string> PickPair(List<string> candidates, int seed)
    {
        var list = new List<string>(candidates);
        list.Sort(StringComparer.Ordinal);
        var rnd = new Amplitude.Utilities.Random(seed);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            var t = list[i]; list[i] = list[j]; list[j] = t;
        }
        return list.GetRange(0, Math.Min(2, list.Count));
    }

    /// <summary>Stable (machine independent) string hash: heroes of an unknown (modded) class are spread
    /// over the kinds by name, so the kind of a name never differs between the machines of a session.</summary>
    public static ulong StableHash(string text)
    {
        ulong h = 14695981039346656037UL;
        if (text == null) return h;
        for (int i = 0; i < text.Length; i++) { h ^= (ulong)text[i]; h *= 1099511628211UL; }
        return h;
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;

namespace Slafight_Plugin_EXILED.Patches;

[HarmonyPatch(typeof(Scp330Candies), nameof(Scp330Candies.Candies), MethodType.Getter)]
public static class CandyPatch
{
    private static ICandy[] DetectedCandies;
    
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> _)
    {
        // ここで一度だけ Exiled 側のキャンディを全部集める
        DetectedCandies = FixCandies();

        return new CodeInstruction[]
        {
            new (OpCodes.Ldsfld, AccessTools.Field(typeof(CandyPatch), nameof(DetectedCandies))),
            new (OpCodes.Ret),
        };
    }

    private static ICandy[] FixCandies()
    {
        var asm = typeof(Scp330Candies).Assembly;

        // Kind ごとに候補を集める
        var normals = new Dictionary<CandyKindID, ICandy>();
        var haunteds = new Dictionary<CandyKindID, ICandy>();

        foreach (Type t in asm.GetTypes())
        {
            if (!typeof(ICandy).IsAssignableFrom(t) || t.IsInterface || t.IsAbstract)
                continue;

            try
            {
                if (Activator.CreateInstance(t) is not ICandy candy)
                    continue;

                string typeName = t.FullName ?? t.Name;
                bool isHaunted = typeName.Contains("Haunted", StringComparison.OrdinalIgnoreCase);

                if (isHaunted)
                {
                    haunteds[candy.Kind] = candy;
                    Log.Info($"[CandyPatch] Haunted candidate: {candy.Kind} ({typeName})");
                }
                else
                {
                    normals[candy.Kind] = candy;
                    Log.Info($"[CandyPatch] Normal candidate: {candy.Kind} ({typeName})");
                }
            }
            catch (Exception e)
            {
                Log.Warn($"[CandyPatch] Failed to create {t.FullName}: {e}");
            }
        }

        // Kind ごとに「通常があれば通常」「なければ Haunted」を採用
        var result = new List<ICandy>();

        // 通常キャンディーが存在する Kind は通常だけ
        foreach (var kv in normals)
        {
            result.Add(kv.Value);
            Log.Info($"[CandyPatch] Use normal candy: {kv.Key} ({kv.Value.GetType().FullName})");
        }

        // 通常が存在せず Haunted だけの Kind は Haunted を採用（茶色など）
        foreach (var kv in haunteds)
        {
            if (normals.ContainsKey(kv.Key))
                continue; // その色は既に通常を使っているのでスキップ

            result.Add(kv.Value);
            Log.Info($"[CandyPatch] Use haunted-only candy: {kv.Key} ({kv.Value.GetType().FullName})");
        }

        return result.ToArray();
    }

}
using System;
using System.Collections.Generic;
using Exiled.API.Features;
using MEC;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Slafight_Plugin_EXILED.MapExtensions;

public static class TrainComing
{
    public static IEnumerator<float> SpawnTrainAndAnim(Vector3 startPos, Vector3 checkpointPos, Vector3 endPos)
    {
        for (;;)
        {
            if (Round.IsLobby)
            {
                Log.Info("[Train] Lobby, stop.");
                yield break;
            }

            if (!ObjectSpawner.TrySpawnSchematic("STrain", startPos, out var train))
            {
                Log.Error("[Train] Failed to spawn STrain.");
                yield break;
            }

            Log.Info("[Train] Spawned STrain at " + startPos);

            Timing.RunCoroutine(Anim(train, startPos, checkpointPos, 3.015f));
            yield return Timing.WaitForSeconds(35f); // 修正

            Timing.RunCoroutine(Anim(train, checkpointPos, endPos, 1.765f));
            yield return Timing.WaitForSeconds(2f); // 念のため少し待つ

            train.Destroy();
            Log.Info("[Train] Destroyed STrain, scheduling next.");

            yield return Timing.WaitForSeconds(50f * Random.Range(0f, 5.000001f));
        }
    }
    
    private static IEnumerator<float> Anim(SchematicObject schem, Vector3 startpos, Vector3 endpos, float duration)
    {
        if (schem is null || duration <= 0f)
            yield break;

        float elapsedTime = 0f;
        Vector3 startPos = startpos;
        Vector3 endPos = endpos;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            schem.transform.position = Vector3.Lerp(startPos, endPos, progress);
            yield return 0f;
        }

        schem.transform.position = endPos;
    }
}
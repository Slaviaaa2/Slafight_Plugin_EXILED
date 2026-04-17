using System.Collections.Generic;
using Exiled.API.Features;
using MEC;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using UnityEngine;
using Utils.NonAllocLINQ;

namespace Slafight_Plugin_EXILED.CustomMaps.Entities;

public static class Scp513
{
    public static void Register()
    {
        Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingPlayers;
    }

    public static void Unregister()
    {
        Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingPlayers;
    }

    private static readonly List<Player> StalkingTargets = [];
    private static CoroutineHandle Scp513CoroutineHandle;

    private static void OnWaitingPlayers()
    {
        StalkingTargets.Clear();
        Timing.KillCoroutines(Scp513CoroutineHandle);
        Scp513CoroutineHandle = Timing.RunCoroutine(Scp513Coroutine());
    }

    public static void AddTarget(Player? player)
    {
        if (player == null) return;
        StalkingTargets.AddIfNotContains(player);
    }

    private static IEnumerator<float> Scp513Coroutine()
    {
        List<SchematicObject> instances = [];

        while (true)
        {
            if (RoundSummary.SummaryActive)
                yield break;

            // 既存インスタンス破棄
            instances.ForEach(instance =>
            {
                if (instance != null)
                    instance.Destroy();
            });
            instances.Clear();
            yield return Timing.WaitForSeconds(Random.Range(8f, 15f));
            foreach (var player in StalkingTargets.ToArray())
            {
                if (player == null || !player.IsConnected || !player.IsAlive)
                    continue;

                // プレイヤーの前方10m
                Vector3 spawnPos = player.Position + player.Transform.forward * 7.5f;

                // プレイヤーの方を向く（Y回転のみ）
                Vector3 lookDir = player.Position - spawnPos;
                lookDir.y = 0f;

                if (lookDir.sqrMagnitude < 0.001f)
                    lookDir = new Vector3(player.Transform.forward.x, 0f, player.Transform.forward.z);

                Quaternion playerLookRotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);

                var obj = ObjectSpawner.SpawnSchematic("SCP513", spawnPos, playerLookRotation);
                if (obj == null)
                    continue;

                obj.transform.SetParent(player.Transform, true);
                instances.Add(obj);
            }

            yield return Timing.WaitForSeconds(Random.Range(2f, 7f));
        }
    }
}
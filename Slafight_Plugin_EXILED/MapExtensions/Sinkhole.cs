using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Hazards;
using MEC;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED.MapExtensions;

public class Sinkhole
{
    public Sinkhole()
    {
        Exiled.Events.Handlers.Server.RoundStarted += RoundStartHole;
    }

    ~Sinkhole()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= RoundStartHole;
    }
    
    private readonly List<Vector3> Sinkholes = new();
    private readonly List<Player> JoiningPlayers = new();
    private CoroutineHandle _sinkholeHandle;

    private readonly Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio 
        = EventHandler.CreateAndPlayAudio;
    
    private void RoundStartHole()
    {
        Sinkholes.Clear();
        JoiningPlayers.Clear();

        if (_sinkholeHandle.IsRunning)
            Timing.KillCoroutines(_sinkholeHandle);

        _sinkholeHandle = Timing.RunCoroutine(SinkholesCoroutine());
    }

    private IEnumerator<float> SinkholesCoroutine()
    {
        for (;;)
        {
            if (Round.IsLobby || Round.IsEnded)
                yield break;

            Sinkholes.Clear();
            foreach (var hazard in Hazard.List)
            {
                if (hazard.Type == HazardType.Sinkhole)
                    Sinkholes.Add(hazard.Position);
            }

            foreach (var player in Player.List)
            {
                // プレイヤー生存・接続チェック
                if (player == null || !player.IsConnected || !player.IsAlive)
                    continue;

                if (player.GetTeam() == CTeam.SCPs)
                    continue;

                foreach (var sinkhole in Sinkholes)
                {
                    float distance = Vector3.Distance(player.Position, sinkhole);
                    if (distance <= 1.5f)
                    {
                        if (!JoiningPlayers.Contains(player))
                        {
                            CreateAndPlayAudio("SinkholeFall.ogg", "Sinkhole", player.Position, true, null, false, 10, 0);
                            JoiningPlayers.Add(player);
                            player.IsGodModeEnabled = true;

                            Timing.RunCoroutine(PocketJoinAnim(player, sinkhole));

                            Timing.CallDelayed(3.1f, () =>
                            {
                                if (player == null || !player.IsConnected)
                                    return;

                                player.EnableEffect(EffectType.PocketCorroding);
                                JoiningPlayers.Remove(player);

                                Timing.CallDelayed(0.15f, () =>
                                {
                                    if (player == null || !player.IsConnected)
                                        return;

                                    player.IsGodModeEnabled = false;
                                });
                            });
                        }
                    }
                }
            }

            yield return Timing.WaitForSeconds(3f);
        }
    }
    
    private IEnumerator<float> PocketJoinAnim(Player player, Vector3 sinkholePos)
    {
        float elapsedTime = 0f;
        const float totalDuration = 3f;

        Vector3 startPos = player.Position;
        Vector3 endPos = sinkholePos + new Vector3(0f, -1.05f, 0f);

        while (elapsedTime < totalDuration)
        {
            if (Round.IsLobby || Round.IsEnded)
                yield break;

            if (player == null || !player.IsConnected || !player.IsAlive)
                yield break;

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / totalDuration;
            player.Position = Vector3.Lerp(startPos, endPos, progress);

            yield return 0f;
        }
    }
}

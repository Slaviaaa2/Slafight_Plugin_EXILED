using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Hazards;
using MEC;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

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
    
    private List<Vector3> Sinkholes = new List<Vector3>();
    private List<Player> JoiningPlayers = new List<Player>();
    
    private void RoundStartHole()
    {
        Sinkholes.Clear();
        JoiningPlayers.Clear();
        Timing.RunCoroutine(SinkholesCoroutine());
    }

    private IEnumerator<float> SinkholesCoroutine()
    {
        for (;;)
        {
            Sinkholes.Clear();
            foreach (var sinkholes in Hazard.List)
            {
                if (sinkholes.Type == HazardType.Sinkhole)
                {
                    Sinkholes.Add(sinkholes.Position);
                    //Log.Debug($"Sinkhole検出: {sinkholes.Position}");
                }
            }

            //Log.Debug($"Sinkholes数: {Sinkholes.Count}");

            if (Round.IsLobby) yield break;

            foreach (Player player in Player.List)
            {
                if (player.GetTeam() == CTeam.SCPs) continue; // SCP除外

                foreach (var sinkhole in Sinkholes)
                {
                    // 修正1: 距離閾値を2.5fに拡大
                    float distance = Vector3.Distance(player.Position, sinkhole);
                    if (distance <= 1.5f)
                    {
                        //Log.Debug($"プレイヤー {player.Nickname} がSinkholeに接近: {distance:F2}m");
                    
                        if (!JoiningPlayers.Contains(player))
                        {
                            JoiningPlayers.Add(player);
                            player.IsGodModeEnabled = true;
                            Timing.RunCoroutine(PocketJoinAnim(player, sinkhole));
                            Timing.CallDelayed(3.1f, () =>
                            {
                                player.EnableEffect(EffectType.PocketCorroding);
                                JoiningPlayers.Remove(player);
                                Timing.CallDelayed(0.15f, () =>
                                {
                                    player.IsGodModeEnabled = false;
                                });
                            });
                        }
                    }
                }
            }
            yield return Timing.WaitForSeconds(3f); // 修正2: 更新頻度向上
        }
    }
    
    private IEnumerator<float> PocketJoinAnim(Player player,Vector3 sinkholePos)
    {
        float elapsedTime = 0f;
        float totalDuration = 3f;
        Vector3 startPos = new Vector3(player.Position.x, player.Position.y, player.Position.z);
        Vector3 endPos = sinkholePos + new Vector3(0f,-1.05f,0f);
        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / totalDuration;
            player.Position = Vector3.Lerp(startPos, endPos, progress);
            yield return 0f;
        }
    }
}
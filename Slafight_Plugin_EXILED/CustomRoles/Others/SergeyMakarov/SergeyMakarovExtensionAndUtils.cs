using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED.CustomRoles.Others.SergeyMakarov;

public static class SergeyMakarovExtensionAndUtils
{
    private static readonly Action<string, string, Vector3, bool, Transform, bool, float, float> CreateAndPlayAudio
        = EventHandler.CreateAndPlayAudio;
    public static bool IsSergeyMarkov(this Player player)
    {
        if (player == null) return false;
        return player.GetCustomRole() == CRoleTypeId.SergeyMakarov || player.GetCustomRole() == CRoleTypeId.SergeyMakarovAwaken;
    }

    public static IEnumerator<float> AwakenScene(Player player)
    {
        if (!player.IsSergeyMarkov() || !Round.InProgress) yield break;
        var ragdoll = Ragdoll.CreateAndSpawn(RoleTypeId.Scientist, player.CustomName, "生命活動が停止しているように見える・・・",
            player.Position + Vector3.up * 0.5f);
        player.IsGodModeEnabled = true;
        player.Health = 0f;
        player.ClearInventory();
        player.EnableEffect(EffectType.SinkHole, 99);
        player.EnableEffect(EffectType.Fade, 200);
        player.Handcuff();
        CreateAndPlayAudio("DeathBell.ogg", "SergeyDeath", player.Position, true, null, false, 10, 0);
        player.Broadcast(999, "\n<size=120>死亡した</size>");
        player.ShowHint("「薄汚い裏切り者共が... 二度も私を殺すとは...」");
        yield return Timing.WaitForSeconds(3f);
        if (!player.IsSergeyMarkov() || !Round.InProgress) yield break;
        player.ShowHint("<color=yellow>「ここで終わるわけにはいかんのだ... 復讐を果たさねば！」</color>");
        yield return Timing.WaitForSeconds(3f);
        if (!player.IsSergeyMarkov() || !Round.InProgress) yield break;
        player.ShowHint("<color=red>「能無し共も！裏切り者共も！私の邪魔をするものを滅ぼさねばならぬのだ！」</color>");
        yield return Timing.WaitForSeconds(5f);
        if (!player.IsSergeyMarkov() || !Round.InProgress) yield break;
        player.ShowHint("<color=red><b>「サイト-02は私の場所だ！私の牙城に踏み込んだことを後悔するがいい...!」</b></color>");
        yield return Timing.WaitForSeconds(8f);
        if (!player.IsSergeyMarkov() || !Round.InProgress) yield break;
        CreateAndPlayAudio("SpawnBell.ogg", "SergeySpawn", player.Position, true, null, false, 10, 0);
        foreach (var room in Room.List)
        {
            room.Blackout(5);
        }
        player.ClearBroadcasts();
        player.SetRole(CRoleTypeId.SergeyMakarovAwaken);
        player.IsGodModeEnabled = false;
        ragdoll.Destroy();
        yield return Timing.WaitForSeconds(5f);
        if (!player.IsSergeyMarkov() || !Round.InProgress) yield break;
        Exiled.API.Features.Cassie.MessageTranslated("Attention, All Insurgency Agent. Detected Unknown It in Scp 1 0 6 Chamber. Please Terminate $PITCH_0.5 .g1 .g7 .g4 .g4 .g6 .g2 .g1","警告：SCP-106収容房に不明な霊的実体を検出...[ノイズ]",true);
    }
}
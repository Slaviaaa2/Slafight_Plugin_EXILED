using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class DummyRoad : CItemKeycard
{
    public override string DisplayName => "Dummy Road Spawner";
    public override string Description => "What the Fuck?";

    protected override string UniqueKey => "DummyRoad";
    protected override ItemType BaseItem => ItemType.KeycardCustomTaskForce;

    protected override string KeycardLabel => "Dummy Road Spawn Device";
    protected override Color32? KeycardLabelColor => new Color32(0, 0, 0, 255);
    protected override string KeycardName => "Dummy Lord";
    protected override Color32? TintColor => new Color32(0, 0, 0, 255);
    protected override Color32? KeycardPermissionsColor => new Color32(255, 255, 255, 255);
    protected override KeycardPermissions Permissions => KeycardPermissions.None;
    protected override byte Rank => 1;
    protected override string SerialNumber => "555555555555";

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => Color.black;

    /// <summary>
    /// 投げる (Drop=Throw 含む通常 Drop) でアイテムを消費して
    /// SCP3005 schematic を発射、軌道上に NPC を撒く。
    /// </summary>
    protected override void OnDropping(DroppingItemEventArgs ev)
    {
        ev.IsAllowed = false;
        ev.Item.Destroy();

        try
        {
            var schem = ObjectSpawner.SpawnSchematic("SCP3005", ev.Player.Position, ev.Player.CameraTransform.forward);
            Timing.RunCoroutine(MissileCoroutine(schem, ev.Player));
        }
        catch (Exception ex)
        {
            Log.Error($"[DummyRoad] Schematic spawn failed: {ex}");
        }
    }

    private static IEnumerator<float> MissileCoroutine(SchematicObject schem, Player pushPlayer)
    {
        if (schem == null || schem.transform == null)
        {
            Log.Warn("[DummyRoad] MissileCoroutine aborted: schem null at start.");
            yield break;
        }

        const float totalDuration = 0.5f;
        var elapsed = 0f;
        var startPos = schem.transform.position;
        var forward = pushPlayer != null ? pushPlayer.CameraTransform.forward.normalized : Vector3.forward;
        var endPos = startPos + forward * 10f;

        var i = 0;
        while (elapsed < totalDuration)
        {
            if (Round.IsLobby || Round.IsEnded) yield break;
            if (schem == null || schem.transform == null) yield break;
            if (pushPlayer != null && !pushPlayer.IsConnected) yield break;

            try
            {
                Npc.Spawn("DummyRoad No. " + i, RoleTypeId.ClassD, schem.transform.position);
                i++;
            }
            catch (Exception ex)
            {
                Log.Error($"[DummyRoad] NPC spawn failed: {ex}");
            }

            elapsed += Time.deltaTime;
            schem.transform.position = Vector3.Lerp(startPos, endPos, elapsed / totalDuration);
            yield return 0f;
        }

        try { schem?.Destroy(); }
        catch (Exception ex) { Log.Error($"[DummyRoad] Error destroying schem: {ex}"); }
    }
}

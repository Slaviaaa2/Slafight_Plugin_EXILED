using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp096Anger : CRole
{
    public override void SpawnRole(Player player, RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player.Role.Set(RoleTypeId.Scp096);
        player.UniqueRole = "Scp096_Anger";
        player.CustomInfo = "<color=#C50000>SCP-096: ANGER</color>";
        player.MaxArtificialHealth = 1000;
        player.MaxHealth = 5000;
        player.Health = 5000;
        StatusEffectBase? movement = player.GetEffect(EffectType.MovementBoost);
        movement.Intensity = 50;
        player.ShowHint(
            "<color=red>SCP-096: ANGER</color>\nSCP-096の怒りと悲しみが頂点に達し、その化身へと変貌して大いなる力を手に入れた。\n<color=red>とにかく破壊しまくれ！！！！！</color>",
            10);
        player.Transform.eulerAngles = new Vector3(0, -90, 0);
        Log.Debug("Scp096: Anger was Spawned!");
        Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.ShyguyPosition = player.Position;
        Slafight_Plugin_EXILED.Plugin.Singleton.SpecialEventsHandler.StartAnger();
    }
}
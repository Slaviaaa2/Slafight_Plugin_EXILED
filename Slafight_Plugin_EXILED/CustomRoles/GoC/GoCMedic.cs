using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomItems.exiledApiItems;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.CustomRoles.GoC;

public class GoCMedic : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.GoCMedic;
    protected override CTeam Team { get; set; } = CTeam.GoC;
    protected override string UniqueRoleKey { get; set; } = "GoCMedic";

    public override void SpawnRole(Player? player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player!.Role.Set(RoleTypeId.NtfSpecialist);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        player.AddItem(ItemType.GunE11SR);
        player.AddItem(ItemType.KeycardMTFOperative);
        player.TryAddCustomItem<AdvancedMedkit>();
        player.TryAddCustomItem<AdvancedMedkit>();
        player.TryAddCustomItem<AdvancedMedkit>();
        player.AddItem(ItemType.SCP500);
        player.AddItem(ItemType.Radio);
        player.TryAddCustomItem<ArmorInfantry>();
            
        player.AddAmmo(AmmoType.Nato556,140);

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Hammer Down Infantry");
        player.SetCustomInfo("Global Occult Collision: Broken Dagger Medic");
        Timing.RunCoroutine(Coroutine(player));
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<size=24><color=#0000c8>GoC: Broken Dagger 医療スペシャリスト</color>\n負傷した部隊の治療等を行う\nPassive: VERITAS\n遠くにいる敵等を認識できる",10f);
        });
    }
    
    private IEnumerator<float> Coroutine(Player player)
    {
        while (true)
        {
            if (!Check(player)) yield break;
            if (!player.IsEffectActive<Scp1344>())
            {
                player.EnableEffect(EffectType.Scp1344, 1);
            }
            yield return Timing.WaitForSeconds(3f);
        }
    }
}
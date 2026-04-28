using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.CustomItems.exiledApiItems;
using Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.CustomRoles.GoC;

public class GoCDeputy : CRole
{
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.GoCDeputy;
    protected override CTeam Team { get; set; } = CTeam.GoC;
    protected override string UniqueRoleKey { get; set; } = "GoCDeputy";

    public override void SpawnRole(Player? player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player!.Role.Set(RoleTypeId.NtfSergeant);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;
        player.ClearInventory();
        player.AddItem(ItemType.GunCom45);
        player.AddItem(ItemType.GunE11SR);
        player.AddItem(ItemType.KeycardMTFOperative);
        player.TryAddCustomItem<FlashBangE>();
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.GrenadeHE);
        player.AddItem(ItemType.Radio);
        
        CItem.Get<ArmorInfantry>()?.Give(player);
            
        player.AddAmmo(AmmoType.Nato556,140);

        //PlayerExtensions.OverrideRoleName(player,$"{player.GroupName}","Hammer Down Infantry");
        player.SetCustomInfo("Global Occult Collision: Broken Dagger Deputy");
        Timing.RunCoroutine(Coroutine(player));
        Timing.CallDelayed(0.05f, () =>
        {
            player.ShowHint("<size=24><color=#0000c8>GoC: Broken Dagger 副官</color>\n部隊の任務遂行を補助する\nPassive: VERITAS\n遠くにいる敵等を認識できる",10f);
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
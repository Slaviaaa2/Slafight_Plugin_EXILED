using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp173;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.Abilities;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using EventHandler = Slafight_Plugin_EXILED.MainHandlers.EventHandler;

namespace Slafight_Plugin_EXILED.CustomRoles.SCPs;

public class Scp173Role : CRole
{
    protected override string RoleName { get; set; } = "SCP-173";
    protected override CRoleTypeId CRoleTypeId { get; set; } = CRoleTypeId.Scp173;
    protected override CTeam Team { get; set; } = CTeam.SCPs;
    protected override string UniqueRoleKey { get; set; } = "Scp173";

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Scp173.Blinking += OnBlinking;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Scp173.Blinking -= OnBlinking;
        base.UnregisterEvents();
    }
    
    public override void SpawnRole(Player? player,RoleSpawnFlags roleSpawnFlags = RoleSpawnFlags.All)
    {
        base.SpawnRole(player, roleSpawnFlags);
        player!.Role.Set(RoleTypeId.Scp173);
        player.UniqueRole = UniqueRoleKey;
        player.MaxHealth = 4500f;
        player.Health = player.MaxHealth;
        player.MaxHumeShield = 1500f;
        player.HumeShield = player.MaxHumeShield;
        player.ClearInventory();
        player.SetCustomInfo("SCP-173");
        
        player.EnableEffect(EffectType.Slowness, 95, 60f);
        
        player.AddAbility(new TeleportRandomAbility(player));
        player.AddAbility(new PlaceTantrumAbility(player));
        Timing.CallDelayed(0.05f, () =>
        {
            player.Position = EventHandler.Scp173SpawnPoint;
            player.ShowHint("<size=24><color=red>SCP-173</color>\n相手が瞬きしたときに超高速で移動し、首をへし折る。\nメインヴィランアビリティでランダムな場所にテレポートできる。\n汚物作戦アビリティで周囲5m以内に汚物を生成できる。",10f);
        });
    }

    private void OnBlinking(BlinkingEventArgs ev)
    {
        if (!Check(ev.Player)) return;
        if (ev.Targets.Count >= 3)
        {
            ev.Scp173.BlinkReady = false;
        }
    }
    
    protected override void OnDying(DyingEventArgs ev)
    {
        CassieHelper.AnnounceTermination(ev, "SCP 1 7 3", $"<color={CustomTeamUtils.GetTeamColor(Team)}>{RoleName}</color>", true);
        base.OnDying(ev);
    }
}
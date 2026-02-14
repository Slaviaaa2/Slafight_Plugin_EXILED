using System.Linq;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using PlayerStatsSystem;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using Player = Exiled.API.Features.Player;

namespace Slafight_Plugin_EXILED.CustomItems.newEve;

public class NeutralizeGrenade : CItem
{
    protected override ItemType ItemType => ItemType.GrenadeHE;
    protected override void OnObtained(Player player)
    {
        player.ShowHint("<size=20>あなたはNeutralize Grenadeを拾いました！</size>\n反ミーム存在及びその影響を受けた者たちにのみ有効な、弱化兵器。");
        base.OnObtained(player);
    }

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Map.ExplodingGrenade += OnGrenading;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Map.ExplodingGrenade -= OnGrenading;
        base.UnregisterEvents();
    }

    private void OnGrenading(ExplodingGrenadeEventArgs ev)
    {
        if (!CheckSerial(ev.Projectile.Serial, this)) return;
        var fifthists = ev.TargetsToAffect.Where(player => player.IsFifthist()).ToList();
        ev.TargetsToAffect.Clear();
        foreach (var fifthist in fifthists)
        {
            fifthist.EnableEffect(EffectType.SinkHole, 30, 10f);
        }
    }
}
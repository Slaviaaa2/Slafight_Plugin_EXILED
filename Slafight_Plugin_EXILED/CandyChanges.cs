using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Hazards;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.Events.EventArgs.Scp330;
using InventorySystem.Items;
using InventorySystem.Items.Usables.Scp330;
using MEC;
using PlayerRoles;
using UnityEngine;
using VoiceChat;

namespace Slafight_Plugin_EXILED;

public class CandyChanges
{
    public CandyChanges()
    {
        Exiled.Events.Handlers.Scp330.InteractingScp330 += CandyRoll;
        Exiled.Events.Handlers.Scp330.EatenScp330 += SpecialCandiesEffect;
    }

    ~CandyChanges()
    {
        Exiled.Events.Handlers.Scp330.InteractingScp330 -= CandyRoll;
        Exiled.Events.Handlers.Scp330.EatenScp330 -= SpecialCandiesEffect;
    }

    public void CandyRoll(InteractingScp330EventArgs ev)
    {
        float random = UnityEngine.Random.Range(0f, 1f);
        var before = ev.Candy;

        List<CandyKindID> rareCandies = new()
        {
            CandyKindID.Black,
            CandyKindID.Brown,
            CandyKindID.Gray,
            CandyKindID.Orange,
            CandyKindID.White,
            CandyKindID.Evil
        };
        
        List<CandyKindID> normalCandies = new()
        {
            CandyKindID.Red,
            CandyKindID.Blue,
            CandyKindID.Green,
            CandyKindID.Purple,
            CandyKindID.Rainbow,
            CandyKindID.Yellow
        };

        if (random <= 0.1f)
        {
            ev.Candy = CandyKindID.Pink;
        }
        else if (random <= 0.22f)
        {
            ev.Candy = rareCandies.RandomItem();
        }
        else
        {
            ev.Candy = normalCandies.RandomItem();
        }

        Log.Debug($"RollPlayer: {ev.Player}, CandyRoll: {random}, Before: {before}, After: {ev.Candy}");
    }

    public void SpecialCandiesEffect(EatenScp330EventArgs ev)
    {
        if (Plugin.Singleton.Config.Season != 1) // !IsHalloween
        {
            Log.Debug($"[Candy Eaten] Player: {ev.Player}, Candy: {ev.Candy}");
            if (ev.Player == null) return;
            if (ev.Candy.Kind == CandyKindID.Black)
            {
                // Goodbye. MASARU Black...
                ev.Player?.ShowHint("<size=24>この世全ての混沌を混ぜて煮詰めた匂いがする...</size>");
            }
            else if (ev.Candy.Kind == CandyKindID.Brown)
            {
                TantrumHazard.PlaceTantrum(ev.Player.Position);
            }
            else if (ev.Candy.Kind == CandyKindID.Gray)
            {
                ev.Player?.EnableEffect(EffectType.Slowness, 35,60f);
                ev.Player?.CustomHumeShieldStat.MaxValue = 10000f;
                ev.Player?.CustomHumeShieldStat.CurValue = 10000f;
                ev.Player?.CustomHumeShieldStat.ShieldRegenerationMultiplier = 0f;
                ev.Player?.ShowHint("<size=24>埃っぽく鉄臭い匂いが鼻を刺す...</size>");
                Timing.CallDelayed(60f, () =>
                {
                    ev.Player?.CustomHumeShieldStat.MaxValue = 0f;
                    ev.Player?.CustomHumeShieldStat.CurValue = 0f;
                });
            }
            else if (ev.Candy.Kind == CandyKindID.Orange)
            {
                ev.Player?.ShowHint("<size=24>眩しいほどに爽やかなオレンジの匂いがする...</size>");
                foreach (Player player in Player.List)
                {
                    if (player == null || ev.Player == null) continue;
                    if (player != ev.Player)
                    {
                        if (Vector3.Distance(player.Position, ev.Player.Position) <= 3.25f)
                        {
                            player.Explode(ProjectileType.Flashbang,ev.Player);
                        }
                    }
                }
            }
            else if (ev.Candy.Kind == CandyKindID.White)
            {
                ev.Player?.EnableEffect(EffectType.Ghostly, 40f);
                ev.Player?.EnableEffect(EffectType.MovementBoost, 20, 40f);
                ev.Player?.ShowHint("<size=24>透き通るようなミルクの香りがする...</size>");
            }
            else if (ev.Candy.Kind == CandyKindID.Evil)
            {
                ev.Player?.Role.Set(RoleTypeId.Scp0492,RoleSpawnFlags.None);
                ev.Player?.EnableEffect(EffectType.Scp207, 2);
                ev.Player?.VoiceChannel = VoiceChatChannel.Proximity;
                ev.Player?.ShowHint("<size=24>冒涜的な匂いに気が狂いそうになる...</size>");
            }
        }
    }
}
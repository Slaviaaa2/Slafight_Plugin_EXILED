using Exiled.API.Enums;
using Exiled.Events.EventArgs.Scp914;
using PlayerRoles;
using Scp914;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.ProximityChat;
using UnityEngine;
using Random = System.Random;

namespace Slafight_Plugin_EXILED;

public class Scp914Changes
{
    public Scp914Changes()
    {
        
    }

    ~Scp914Changes()
    {
        
    }

    private Random random = new Random();
    private void Human(UpgradingPlayerEventArgs ev)
    {
        if (ev.KnobSetting == Scp914KnobSetting.VeryFine)
        {
            var _value = random.Next(0, 4);
            if (_value == 0)
            {
                ev.Player?.Role.Set(RoleTypeId.Scp0492,RoleSpawnFlags.None);
                ev.Player?.EnableEffect(EffectType.Scp207, 4);
                ev.Player?.UniqueRole = "Zombified";
                ev.Player?.SetCustomInfo("<color=#C50000>Zombified Subject</color>");
                ev.Player?.SetScale(new Vector3((UnityEngine.Random.Range(0.01f, 1.08f)), (UnityEngine.Random.Range(0.01f, 1.08f)), (UnityEngine.Random.Range(0.01f, 1.08f))));
                if (!Handler.CanUsePlayers.Contains(ev.Player))
                {
                    Handler.CanUsePlayers.Add(ev.Player);
                }
                if (!Handler.ActivatedPlayers.Contains(ev.Player))
                {
                    Handler.ActivatedPlayers.Add(ev.Player);
                }
                ev.Player?.ShowHint("<size=24>体が魔改造されていく・・・！</size>");
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using UnityEngine;

namespace Slafight_Plugin_EXILED.MapExtensions;

public class Scp012_033
{
    private CoroutineHandle _coroutineHandle;
    public Dictionary<Player, bool> Effecteds { get; } = new();

    private void ThetaPrimeEffect(UsingRadioBatteryEventArgs ev)
    {
        if (Effecteds.TryGetValue(ev.Player, out var effected) && effected)
        {
            ev.IsAllowed = false;
            ev.Radio.IsEnabled = false;
            ev.Player?.ShowHint("...?", 1.5f);
        }
    }

    private IEnumerator<float> ThetaPrimeCoroutine()
    {
        var scp012Pos = CustomMap.Scp012_t.Position;
        while (true)  // for(;;)より明示的
        {
            var alivePlayers = Player.List.Where(p => p.IsConnected && !p.IsHost && !p.IsNPC).ToList();
            foreach (var player in alivePlayers)
            {
                Effecteds[player] = Vector3.Distance(scp012Pos, player.Position) <= 5.5f;
            }
        
            // 切断/死亡Playerをクリーンアップ（任意、メモリ節約）
            Effecteds.Where(kvp => !kvp.Key.IsConnected).ToList().ForEach(kvp => Effecteds.Remove(kvp.Key));
        
            yield return Timing.WaitForSeconds(0.5f);
        }
    }

    private void OnLeft(LeftEventArgs ev) => Effecteds.Remove(ev.Player);
    public Scp012_033()
    {
        Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;  // 名前重複避け
        Exiled.Events.Handlers.Player.Left += OnLeft;
        Exiled.Events.Handlers.Player.UsingRadioBattery += ThetaPrimeEffect;
    }
    ~Scp012_033()
    {
        Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
        Exiled.Events.Handlers.Player.Left -= OnLeft;
        Exiled.Events.Handlers.Player.UsingRadioBattery -= ThetaPrimeEffect;
        Timing.KillCoroutines(_coroutineHandle);
    }
    private void OnRoundStarted()
    {
        Effecteds.Clear();
        _coroutineHandle = Timing.RunCoroutine(ThetaPrimeCoroutine());
    }
}
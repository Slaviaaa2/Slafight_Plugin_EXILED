using System;
using System.Collections.Generic;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.Events.EventArgs.Player;
using MEC;
using Slafight_Plugin_EXILED.API.Features;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class Toolbox : CItem
{
    public override string DisplayName { get; } = "Toolbox";
    public override string Description { get; } = "様々な作業を行うことができる便利な工具箱。\nTキーで使う機能を切り替えられる。";
    protected override string UniqueKey { get; } = "Toolbox";
    protected override ItemType BaseItem { get; } = ItemType.Coin;
    
    public enum UtilType
    {
        Work,MaintenanceLock
    }
    public struct ToolboxStats
    {
        public UtilType SelectedUtilType;
        public float AwaitingCooldownDuration;
        public bool IsAwaitingCooldown => AwaitingCooldownDuration > 0;
    }
    private static readonly Dictionary<Player, ToolboxStats> ToolboxStatsMap = [];

    public override void RegisterEvents()
    {
        Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
        Exiled.Events.Handlers.Player.InteractingDoor += OnInteractingDoor;
        Exiled.Events.Handlers.Player.UnlockingGenerator += OnInteractingGenerator;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
        Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractingDoor;
        Exiled.Events.Handlers.Player.UnlockingGenerator -= OnInteractingGenerator;
        base.UnregisterEvents();
    }

    protected override void OnWaitingForPlayers()
    {
        ToolboxStatsMap.Clear();
    }

    private static void OnPlayerLeft(LeftEventArgs ev)
    {
        if (ev?.Player == null) return;
        ToolboxStatsMap.Remove(ev.Player);
    }

    protected override void OnAcquired(ItemAddedEventArgs ev, bool displayMessage)
    {
        ToolboxStatsMap.TryAdd(ev.Player, new ToolboxStats { SelectedUtilType = UtilType.Work, AwaitingCooldownDuration = 0f });
        base.OnAcquired(ev, displayMessage);
    }

    protected override void OnDropping(DroppingItemEventArgs ev)
    {
        if (!ev.IsThrown) return;
        ev.IsAllowed = false;
        if (!ToolboxStatsMap.TryGetValue(ev.Player, out var stats)) return;

        stats.SelectedUtilType = stats.SelectedUtilType switch
        {
            UtilType.Work => UtilType.MaintenanceLock,
            UtilType.MaintenanceLock => UtilType.Work,
            _ => throw new ArgumentOutOfRangeException()
        };
        ToolboxStatsMap[ev.Player] = stats;

        ev.Player.ShowHint(
            $"<size=24>現在選択されている機能：{GetTranslatedText(stats.SelectedUtilType)}</size>\n" +
            $"<size=22>{GetTranslatedText(stats.SelectedUtilType, true)}\n{TryGetExternalCooldownText(ev.Player)}</size>",
            4f);
    }

    private void OnInteractingDoor(InteractingDoorEventArgs ev)
    {
        if (!CheckHeld(ev.Player)) return;
        if (!ToolboxStatsMap.TryGetValue(ev.Player, out var stats)) return;
        if (stats.IsAwaitingCooldown) return;
        if (stats.SelectedUtilType is UtilType.Work)
        {
            if (ev.Door is not BreakableDoor breakableDoor) return;
            if (!breakableDoor.IsBreakable) return;
            if (breakableDoor.IsDestroyed)
            {
                breakableDoor.Repair();
            }
            else
            {
                breakableDoor.Break();
            }
        }
        else
        {
            if (ev.Door.IsLocked) return;
            ev.Door.Lock(30f, DoorLockType.Regular079);
        }

        Timing.RunCoroutine(CooldownCoroutine(ev.Player));
    }

    private void OnInteractingGenerator(UnlockingGeneratorEventArgs ev)
    {
        if (!CheckHeld(ev.Player)) return;
        if (!ToolboxStatsMap.TryGetValue(ev.Player, out var stats)) return;
        if (stats.SelectedUtilType is not UtilType.Work) return;
        if (stats.IsAwaitingCooldown) return;

        // Engineer.cs と同じ「強制解錠」パターン
        ev.IsAllowed = true;
        ev.Generator.State = GeneratorState.Unlocked;
        ev.Generator.IsUnlocked = true;

        Timing.RunCoroutine(CooldownCoroutine(ev.Player));
    }

    private static IEnumerator<float> CooldownCoroutine(Player player, float cooldownTime = 60f)
    {
        if (!ToolboxStatsMap.TryGetValue(player, out var stats)) yield break;

        // 開始時にクールダウン時間を実際に載せる (旧実装ではここが抜けてて絶対 0 のまま抜けていた)
        stats.AwaitingCooldownDuration = cooldownTime;
        ToolboxStatsMap[player] = stats;

        while (true)
        {
            if (player is null || Round.IsLobby || ToolboxStatsMap.IsEmpty()) yield break;
            if (!ToolboxStatsMap.TryGetValue(player, out stats)) yield break;
            if (stats.AwaitingCooldownDuration <= 0f) yield break;

            stats.AwaitingCooldownDuration -= 1f;
            ToolboxStatsMap[player] = stats;
            yield return Timing.WaitForSeconds(1f);
        }
    }

    private static string GetTranslatedText(UtilType utilType, bool isDesc = false)
    {
        string result;
        if (!isDesc)
        {
            result = utilType switch
            {
                UtilType.Work => "作業",
                UtilType.MaintenanceLock => "メンテナンスロック",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else
        {
            result = utilType switch
            {
                UtilType.Work => "ドアに向かってインタラクトすることで破壊・修理を行うことができる。\n発電機に対してインタラクトした場合は強制的に開けることができる。",
                UtilType.MaintenanceLock => "ドアに向かってインタラクトすることでドアをメンテナンスモードにでき、一定時間閉じた状態でロックできる。",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        return result;
    }

    private static string TryGetExternalCooldownText(Player player)
    {
        if (!ToolboxStatsMap.TryGetValue(player, out var stats)) return string.Empty;
        return stats.IsAwaitingCooldown ? $"(クールダウン中：{stats.AwaitingCooldownDuration}秒)" : string.Empty;
    }
}
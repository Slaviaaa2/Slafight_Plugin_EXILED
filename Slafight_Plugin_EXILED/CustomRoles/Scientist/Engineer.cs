using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Warhead;
using HintServiceMeow.Core.Enum;
using MEC;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;
using Random = System.Random;

namespace Slafight_Plugin_EXILED.CustomRoles.Scientist;

[CRoleAutoRegisterIgnore]
public class Engineer : CRole
{
    private class EngineerState
    {
        public TaskType Task = TaskType.None;
        public int Level = 0;
        public int Exp = 0;
        public CoroutineHandle HudRoutine;
    }

    private readonly Dictionary<int, EngineerState> _states = new();
    private readonly Random _rng = new();

    public override void RegisterEvents()
    {
        Log.Info("[Engineer] RegisterEvents");
        Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUp;
        Exiled.Events.Handlers.Map.GeneratorActivating += OnGenerator;
        Exiled.Events.Handlers.Warhead.ChangingLeverStatus += OnLeverChanging;
        Exiled.Events.Handlers.Player.InteractingDoor += OnInteractedDoor;
        Exiled.Events.Handlers.Player.IntercomSpeaking += OnIntercom;
        Exiled.Events.Handlers.Player.ChangingRole += OnChangingRole;
        Exiled.Events.Handlers.Player.DroppingItem += OnDropping;
        Exiled.Events.Handlers.Player.UnlockingGenerator += OnInteractingGenerator;
        base.RegisterEvents();
    }

    public override void UnregisterEvents()
    {
        Log.Info("[Engineer] UnregisterEvents");
        Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUp;
        Exiled.Events.Handlers.Map.GeneratorActivating -= OnGenerator;
        Exiled.Events.Handlers.Warhead.ChangingLeverStatus -= OnLeverChanging;
        Exiled.Events.Handlers.Player.InteractingDoor -= OnInteractedDoor;
        Exiled.Events.Handlers.Player.IntercomSpeaking -= OnIntercom;
        Exiled.Events.Handlers.Player.ChangingRole -= OnChangingRole;
        Exiled.Events.Handlers.Player.DroppingItem -= OnDropping;
        Exiled.Events.Handlers.Player.UnlockingGenerator -= OnInteractingGenerator;
        base.UnregisterEvents();
    }

    public override void SpawnRole(Player player, RoleSpawnFlags flags = RoleSpawnFlags.All)
    {
        Cleanup(player);

        player.Role.Set(RoleTypeId.Scientist);
        base.SpawnRole(player, flags);

        player.UniqueRole = "Engineer";
        player.MaxHealth = 100;
        player.Health = player.MaxHealth;

        var state = new EngineerState();
        _states[player.Id] = state;

        AssignNewTask(player, state);

        player.ClearInventory();
        player.AddItem(ItemType.KeycardResearchCoordinator);
        player.AddItem(ItemType.Medkit);
        player.AddItem(ItemType.Medkit);

        var room = Room.Get(RoomType.HczTestRoom);
        var pos = room != null ? room.WorldPosition(new Vector3(0f, 1f, 0f)) : player.Position;
        player.Position = pos;

        player.CustomInfo = "Engineer";
        player.InfoArea |= PlayerInfoArea.Nickname;
        player.InfoArea &= ~PlayerInfoArea.Role;

        Timing.CallDelayed(0.1f, () =>
        {
            player.ShowHint("<color=#00ffff>エンジニア</color>\nタスク達成で権限アップグレード。\n発電機に権限無視してアクセスできる", 8f);

            if (state.HudRoutine != default)
            {
                Timing.KillCoroutines(state.HudRoutine);
                state.HudRoutine = default;
            }

            state.HudRoutine = Timing.RunCoroutine(HudLoop(player));
        });
    }

    private EngineerState? GetState(Player player)
    {
        if (_states.TryGetValue(player.Id, out var st))
            return st;

        return null;
    }

    private void Cleanup(Player player)
    {
        if (_states.TryGetValue(player.Id, out var st))
        {
            if (st.HudRoutine != default)
            {
                Timing.KillCoroutines(st.HudRoutine);
                st.HudRoutine = default;
            }
        }

        _states.Remove(player.Id);
    }

    private void SyncTaskHud(Player player, EngineerState st)
    {
        string text;
        if (st.Level < 5 && st.Task == TaskType.None)
            text = "";
        else
            text = $"タスク：{st.Task}\nLv.{st.Level} EXP:{st.Exp}";

        Plugin.Singleton.PlayerHUD.HintSync(SyncType.PHUD_Specific, text, player);
    }

    private IEnumerator<float> HudLoop(Player player)
    {
        while (player.IsConnected)
        {
            var st = GetState(player);
            if (st == null)
                break;

            SyncTaskHud(player, st);
            yield return Timing.WaitForSeconds(1f);
        }
    }

    private void AssignNewTask(Player player, EngineerState st)
    {
        if (st.Level >= 5)
        {
            st.Task = TaskType.None;
            SyncTaskHud(player, st);
            return;
        }

        var candidates = new List<TaskType>();

        if (Exiled.API.Features.Generator.List.Any(g => !g.IsEngaged))
            candidates.Add(TaskType.GeneratorTask);

        candidates.Add(TaskType.CollectScpItems);
        candidates.Add(TaskType.CollectKeycard);
        candidates.Add(TaskType.CloseKeycardDoor);
        candidates.Add(TaskType.MaintenanceIntercom);

        if (!Warhead.IsKeycardActivated)
            candidates.Add(TaskType.SetupWarhead);

        if (candidates.Count == 0)
        {
            st.Task = TaskType.None;
        }
        else
        {
            var choice = candidates[_rng.Next(candidates.Count)];
            st.Task = choice;
        }

        SyncTaskHud(player, st);
    }

    private void AddExp(Player player, int amount)
    {
        var st = GetState(player);
        if (st == null) return;

        st.Exp += amount;

        while (true)
        {
            int need = st.Level switch
            {
                0 => 10,
                1 => 20,
                2 => 30,
                3 => 40,
                4 => 50,
                _ => int.MaxValue
            };

            if (st.Exp < need || st.Level >= 5)
                break;

            st.Exp -= need;   // ★必要分だけ消費して超過分を残す
            st.Level++;
            OnLevelUp(player, st);
        }

        AssignNewTask(player, st);
    }

    private void OnLevelUp(Player player, EngineerState st)
    {
        switch (st.Level)
        {
            case 1:
                player.ShowHint("Level 1に到達！無線が支給されました！", 5f);
                player.GiveOrDrop(ItemType.Radio);
                break;
            case 2:
                player.ShowHint("Level 2に到達！軽アーマーが支給されました！", 5f);
                player.GiveOrDrop(ItemType.ArmorLight);
                break;
            case 3:
                player.ShowHint("Level 3に到達！収容エンジニアに昇格しました！", 5f);
                RemoveKeycards(player);
                player.GiveOrDrop(ItemType.KeycardContainmentEngineer);
                break;
            case 4:
                player.ShowHint("Level 4に到達！施設管理者に昇格しました！", 5f);
                RemoveKeycards(player);
                player.GiveOrDrop(ItemType.KeycardFacilityManager);
                break;
            case 5:
                player.ShowHint("最大レベル到達！\n最高アクセス権限及びOMEGA WARHEADアクセスパス付与！", 8f);
                RemoveKeycards(player);
                player.GiveOrDrop(ItemType.KeycardO5);
                player.GiveOrDrop(2005);
                break;
        }
    }

    private void RemoveKeycards(Player player)
    {
        foreach (var item in player.Items.ToList())
        {
            if (item.IsKeycard)
                item.Destroy();
        }
    }

    // ========= イベント =========

    private void OnDropping(DroppingItemEventArgs ev)
    {
        if (!ev.IsAllowed) return;
        if (ev.Player.GetCustomRole() != CRoleTypeId.Engineer) return;
        if (GetState(ev.Player) is { Level: < 5 })
        {
            if (ev.Item.Type == ItemType.KeycardResearchCoordinator ||
                ev.Item.Type == ItemType.KeycardContainmentEngineer || ev.Item.Type == ItemType.KeycardFacilityManager ||
                ev.Item.Type == ItemType.KeycardO5)
            {
                ev.IsAllowed = false;
            }
            else if (CustomItem.TryGet(ev.Item, out var item))
            {
                if (item.Id == 2005)
                {
                    ev.IsAllowed = false;
                }
            }
        }
    }

    private void OnPickingUp(PickingUpItemEventArgs ev)
    {
        if (!ev.IsAllowed) return;
        if (ev.Player.GetCustomRole() != CRoleTypeId.Engineer) return;

        var st = GetState(ev.Player);
        if (st == null) return;

        if (ev.Pickup.Category == ItemCategory.SCPItem && st.Task == TaskType.CollectScpItems)
        {
            ev.IsAllowed = false;
            ev.Pickup.Destroy();
            AddExp(ev.Player, 8);
            return;
        }

        if (ev.Pickup.Type.IsKeycard())
        {
            if (st.Level >= 5)
                return;

            if (st.Task != TaskType.CollectKeycard)
            {
                ev.IsAllowed = false;
                ev.Player.ShowHint("Level 5未満はタスク以外でキーカードを拾えません！", 3f);
                return;
            }

            if (ev.Pickup.PreviousOwner == null || ev.Pickup.PreviousOwner != ev.Player)
            {
                ev.IsAllowed = false;
                ev.Pickup.Destroy();
                AddExp(ev.Player, 5);
            }
        }
    }

    private void OnGenerator(GeneratorActivatingEventArgs ev)
    {
        if (!ev.IsAllowed) return;
        var p = ev.Generator.LastActivator;
        if (p == null || p.GetCustomRole() != CRoleTypeId.Engineer) return;

        var st = GetState(p);
        if (st == null) return;

        if (st.Task == TaskType.GeneratorTask)
            AddExp(p, 30);
    }

    private void OnInteractingGenerator(UnlockingGeneratorEventArgs ev)
    {
        if (ev.Player.GetCustomRole() != CRoleTypeId.Engineer) return;
        ev.IsAllowed = true;
        ev.Generator.State = GeneratorState.Unlocked;
        ev.Generator.IsUnlocked = true;
    }

    private void OnLeverChanging(ChangingLeverStatusEventArgs ev)
    {
        if (!ev.IsAllowed) return;
        if (ev.Player.GetCustomRole() != CRoleTypeId.Engineer) return;

        var st = GetState(ev.Player);
        if (st == null) return;

        if (!ev.CurrentState && st.Task == TaskType.SetupWarhead)
            AddExp(ev.Player, 20);
    }

    private void OnInteractedDoor(InteractingDoorEventArgs ev)
    {
        if (!ev.IsAllowed) return;
        if (!ev.Door.IsKeycardDoor) return;
        if (ev.Player.GetCustomRole() != CRoleTypeId.Engineer) return;

        var st = GetState(ev.Player);
        if (st == null) return;

        if (st.Task == TaskType.CloseKeycardDoor &&
            ev.Door.IsOpen &&
            ev.Player.HasPermission(ev.Door.KeycardPermissions, true))
        {
            AddExp(ev.Player, 10);
        }
    }

    private void OnIntercom(IntercomSpeakingEventArgs ev)
    {
        if (!ev.IsAllowed) return;
        if (ev.Player.GetCustomRole() != CRoleTypeId.Engineer) return;

        var st = GetState(ev.Player);
        if (st == null) return;

        if (st.Task == TaskType.MaintenanceIntercom)
            AddExp(ev.Player, 20);
    }

    private void OnChangingRole(ChangingRoleEventArgs ev)
    {
        if (ev.Player == null) return;
        Cleanup(ev.Player);
    }
}
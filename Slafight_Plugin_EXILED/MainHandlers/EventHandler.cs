using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Warhead;
using MapGeneration;
using MEC;
using PlayerRoles;
using ProjectMER.Events.Arguments;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using Slafight_Plugin_EXILED.SpecialEvents;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Log = Exiled.API.Features.Log;
using Player = Exiled.API.Features.Player;
using PlayerHandler = Exiled.Events.Handlers.Player;
using Room = Exiled.API.Features.Room;
using ServerHandler = Exiled.Events.Handlers.Server;
using Warhead = Exiled.API.Features.Warhead;
using WarheadHandler = Exiled.Events.Handlers.Warhead;
using MapHandler = Exiled.Events.Handlers.Map;

namespace Slafight_Plugin_EXILED.MainHandlers;

public class EventHandler
{
    public EventHandler()
    {
        PlayerHandler.Verified += OnVerified;
        PlayerHandler.Left += OnLeft;
        ServerHandler.RestartingRound += OnRoundRestarted; // FIX: 正しいイベントに登録
        ServerHandler.RoundStarted += OnRoundStarted;
        ServerHandler.ReloadedPlugins += OnPluginLoad;

        MapHandler.Decontaminating += DeconCancell;

        PlayerHandler.ChangingRole += OnChangingRole;
        PlayerHandler.FlippingCoin += PositionGet;
        PlayerHandler.InteractingDoor += DoorGet;
        PlayerHandler.UsedItem += OnUsed;

        WarheadHandler.Starting += AlphaWarheadLock;
        WarheadHandler.DeadmanSwitchInitiating += DeadmanCancell;

        ProjectMER.Events.Handlers.Schematic.SchematicSpawned += SetupSpawnPoints;
    }

    ~EventHandler()
    {
        PlayerHandler.Verified -= OnVerified;
        PlayerHandler.Left -= OnLeft;
        ServerHandler.RestartingRound -= OnRoundRestarted; // FIX: デストラクタも正しく解除
        ServerHandler.RoundStarted -= OnRoundStarted;
        ServerHandler.ReloadedPlugins -= OnPluginLoad;

        MapHandler.Decontaminating -= DeconCancell;

        PlayerHandler.ChangingRole -= OnChangingRole;
        PlayerHandler.FlippingCoin -= PositionGet;
        PlayerHandler.InteractingDoor -= DoorGet;
        PlayerHandler.UsedItem -= OnUsed;

        WarheadHandler.Starting -= AlphaWarheadLock;
        WarheadHandler.DeadmanSwitchInitiating -= DeadmanCancell;

        ProjectMER.Events.Handlers.Schematic.SchematicSpawned -= SetupSpawnPoints;
    }

    private readonly Config cfg = Plugin.Singleton.Config;

    public bool DeadmanDisable = false;
    public bool SkeletonSpawned = false;
    public float SpawnRoll = 1;
    public float Funny = 0;

    public bool SpecialWarhead = false;
    public static int WarheadID = 0;

    public bool DeconCancellFlag = false;
    public bool CryFuckEnabled = false;
    public bool CryFuckSpawned = false;

    private bool GateDoorLocked = false;
    public bool IsScpAutoSpawnLocked = false;
    private bool _pluginLoaded = false;

    // FIX: Playerオブジェクトが安全に使用可能かを確認するヘルパー
    private static bool IsPlayerValid(Player p)
    {
        try
        {
            return p != null && p.IsConnected && p.ReferenceHub != null;
        }
        catch
        {
            return false;
        }
    }

    private void OnPluginLoad()
    {
        Log.Info("OnPluginLoad is successfully called!");
        if (!_pluginLoaded)
        {
            _pluginLoaded = true;
        }
    }

    private void OnVerified(VerifiedEventArgs ev)
    {
        if (ev?.Player == null) return; // FIX: nullガード
        SpecificFlagsManager.InitPlayerFlags(ev.Player);
        ev.Player.Broadcast(6, "\n<size=28><color=#008cff>シャープ鯖</color>へようこそ！\\n本サーバーはRP鯖です。RPを念頭に置いておく以外の制約は無いので自由に楽しんでください！</size>", Broadcast.BroadcastFlags.Normal, true);
        if (Round.InProgress) return;
        Timing.CallDelayed(0.05f, () =>
        {
            if (!IsPlayerValid(ev.Player)) return; // FIX: 遅延後の生存チェック
            string tips = Tips.GetRandomTip();
            ev.Player.ShowHint(
                "\n\n\n\n\n\n\n<size=32>次のイベント：" + Plugin.Singleton.SpecialEventsHandler.LocalizedEventName + "</size>" +
                $"\n\n<size=28>Tips: {tips}</size>",
                5555f);
        });
    }

    private void OnLeft(LeftEventArgs ev)
    {
        if (ev?.Player == null) return; // FIX: nullガード
        if (ev.Player.GetTeam() != CTeam.SCPs) return;
        if (Round.ElapsedTime.TotalSeconds > 179) return;

        // FIX: 離脱したプレイヤー自身を除外してSCP生存数を数える（元のコードは自分自身が含まれる可能性があった）
        int scpAlive = Player.List.Count(p => p != ev.Player && p.IsAlive && p.GetTeam() == CTeam.SCPs);
        if (scpAlive >= 1) return;

        var candidate = Player.List.FirstOrDefault(p => !p.IsAlive);
        if (candidate == null) return;

        var roleInfo = ev.Player.GetRoleInfo();

        if (roleInfo.Custom == CRoleTypeId.None)
        {
            candidate.SetRole(roleInfo.Vanilla);
        }
        else
        {
            Debug.Assert(roleInfo.Custom != null, "roleInfo.Custom != null");
            candidate.SetRole((CRoleTypeId)roleInfo.Custom);
        }

        candidate.ShowHint("※SCPプレイヤーが切断したため代わりにスポーンしました");
    }

    public static void SyncSpecialEvent()
    {
        Timing.CallDelayed(0.05f, () =>
        {
            if (Round.InProgress) return;
            foreach (var player in Player.List)
            {
                if (!IsPlayerValid(player)) continue; // FIX: 無効プレイヤーをスキップ
                var tips = Tips.GetRandomTip();
                player.ShowHint(
                    "\n\n\n\n\n\n\n<size=32>次のイベント：" + Plugin.Singleton.SpecialEventsHandler.LocalizedEventName + "</size>" +
                    $"\n\n<size=28>Tips: {tips}</size>",
                    5555f);
            }
        });
    }

    private void OnRoundRestarted()
    {
        Timing.CallDelayed(0.1f, () =>
        {
            DeadmanDisable = false;
            SkeletonSpawned = false;
            DeconCancellFlag = false;
            SpecialWarhead = false;
            WarheadID = 0;
            CryFuckEnabled = false;
            CryFuckSpawned = false;
            GateDoorLocked = false;
            IsScpAutoSpawnLocked = false;
        });
    }

    public static Vector3 Scp173SpawnPoint = Vector3.zero;

    private void SetupSpawnPoints(SchematicSpawnedEventArgs ev)
    {
        if (ev?.Schematic == null)
            return;

        var schematic = ev.Schematic;

        if (schematic.Name != "Scp173SpawnPoint")
            return;

        try
        {
            Scp173SpawnPoint = schematic.Position;
        }
        catch (Exception e)
        {
            Log.Error($"[SetupSpawnPoints] Failed to get position for {schematic.Name}: {e}");
            return;
        }

        // 座標だけ使うダミーなので安全に破棄
        ev.DestroySafe(0.05f); // SchematicHelpers 拡張
    }

    private void OnRoundStarted()
    {
        SpecificFlagsManager.ClearAll();

        foreach (var player in Player.List.ToList().Where(IsPlayerValid))
        {
            player.ShowHint("");
            SpecificFlagsManager.InitPlayerFlags(player);
        }

        foreach (Door door in Door.List)
        {
            if (SpecialEventsHandler.Instance.NowEvent == SpecialEventType.NuclearAttack) break;
            if (door.Type == DoorType.GateA || door.Type == DoorType.GateB)
                door.Lock(120f, DoorLockType.AdminCommand);
        }

        Timing.CallDelayed(1f, () =>
        {
            foreach (var pickup in Pickup.List)
            {
                if (pickup == null) return;
                pickup.UnSpawn();
                Timing.CallDelayed(0.1f, () => pickup.Spawn());
            }
            List<SpecialEventType> notallowed =
            [
                SpecialEventType.OperationBlackout,
                SpecialEventType.Scp1509BattleField,
                SpecialEventType.FacilityTermination,
                SpecialEventType.SergeyMakarovReturns,
            ];
            if (!notallowed.Contains(Plugin.Singleton.SpecialEventsHandler.NowEvent))
            {
                if (Plugin.Singleton.SpecialEventsHandler.NowEvent == SpecialEventType.OmegaWarhead)
                {
                    Exiled.API.Features.Cassie.MessageTranslated($"Emergency , emergency , A large containment breach is currently started within the site. All personnel must immediately begin evacuation .",
                        "緊急、緊急、現在大規模な収容違反がサイト内で発生しています。全職員は警備隊の指示に従い、避難を開始してください。",true);
                }
                else
                {
                    Exiled.API.Features.Cassie.MessageTranslated($"Attention, All personnel . Detected containment breach is currently started within the site. All personnel must immediately begin evacuation .",
                        "全職員へ通達。収容違反の発生を確認しました。全職員は警備隊の指示に従い、避難を開始してください。",true);
                }
                foreach (Room room in Room.List)
                {
                    room.RoomLightController.ServerFlickerLights(3f);
                }
            }

            Timing.CallDelayed(5f, () =>
            {
                if (!Round.InProgress) return;

                foreach (Door door in Door.List)
                {
                    if (door.Type == DoorType.Scp173Gate)
                    {
                        door.Unlock();
                        door.IsOpen = true;
                    }
                }
            });
        });
    }

    public void OnChangingRole(ChangingRoleEventArgs ev)
    {
        if (ev?.Player == null) return; // FIX: 即時nullチェック（遅延前に確認）

        Timing.CallDelayed(1.05f, () =>
        {
            // FIX: 遅延後に再度有効性チェック（この間に切断している可能性がある）
            if (!IsPlayerValid(ev.Player)) return;
            if (!Round.InProgress) return;

            RoleTypeId role = ev.Player.Role;
            var allowed = PlayerRolesUtils.GetTeam(role);

            if (allowed == Team.SCPs) return;
            if (ev.Player.HasItem(ItemType.Flashlight)) return;
            if (ev.Player.IsInventoryFull) return;
            if (ev.NewRole == RoleTypeId.Spectator) return;
            if (ev.Player.Inventory == null) return;

            Log.Debug("Giving Flashlight to " + ev.Player.Nickname);
            ev.Player.GiveOrDrop(ItemType.Flashlight);
        });
    }

    public static void CreateAndPlayAudio(string fileName, string audioPlayerName, Vector3 position, bool destroyOnEnd = false, Transform parent = null, bool isSpatial = false, float maxDistance = 5, float minDistance = 5)
    {
        var audioPlayer = AudioPlayer.CreateOrGet(audioPlayerName);

        if (!audioPlayer.TryGetSpeaker(audioPlayerName, out Speaker speaker))
        {
            speaker = audioPlayer.AddSpeaker(audioPlayerName, isSpatial: isSpatial, maxDistance: maxDistance, minDistance: minDistance);
        }

        if (parent)
        {
            speaker.transform.SetParent(parent);
            speaker.transform.localPosition = Vector3.zero;
            speaker.transform.localRotation = Quaternion.identity;
        }
        else
        {
            speaker.Position = position;
        }

        AudioClipStorage.LoadClip(Path.Combine(Plugin.Singleton.Config.AudioReferences, fileName), fileName);
        audioPlayer.AddClip(fileName, destroyOnEnd: destroyOnEnd);
    }

    private void AlphaWarheadLock(StartingEventArgs ev)
    {
    }

    private void DeadmanCancell(DeadmanSwitchInitiatingEventArgs ev)
    {
        if (ev == null) return; // FIX: nullガード
        if (DeadmanDisable)
            ev.IsAllowed = false;
    }

    private void DeconCancell(DecontaminatingEventArgs ev)
    {
        if (ev == null) return; // FIX: nullガード
        if (DeconCancellFlag)
        {
            ev.IsAllowed = false;
            Log.Debug("Decon Cancell called.");
        }
    }

    private void PositionGet(FlippingCoinEventArgs ev)
    {
        if (ev?.Player == null) return;
        Vector3 playerPosition = ev.Player.Position;
        if (ev.Player.UniqueRole == "Debug")
        {
            if (ev.Player.CurrentRoom != null)
            {
                Room currentRoom = ev.Player.CurrentRoom;
                Vector3 localPos = Quaternion.Inverse(currentRoom.Rotation) * (playerPosition - currentRoom.Position);
                Quaternion localRot = Quaternion.Inverse(currentRoom.Rotation);
                Vector3 localEuler = localRot.eulerAngles;
                Vector3 roomRot = currentRoom.Rotation.eulerAngles;

                ev.Player.ShowHint("X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z +
                                   "\nRoom: " + currentRoom.Type +
                                   "\nLocal: " + localPos.x + "," + localPos.y + "," + localPos.z +
                                   "\nLocalRot: " + localEuler.x + "," + localEuler.y + "," + localEuler.z +
                                   "\nRoomRot: " + roomRot.x + "," + roomRot.y + "," + roomRot.z, 5);

                Log.Debug("Position Get: " + "X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z);
                Log.Debug(" Room: " + currentRoom.Type);
                Log.Debug(" LocalPos: X:" + localPos.x + " Y:" + localPos.y + " Z:" + localPos.z);
                Log.Debug(" LocalRot: X:" + localEuler.x + " Y:" + localEuler.y + " Z:" + localEuler.z);
                Log.Debug(" RoomRot: X:" + roomRot.x + " Y:" + roomRot.y + " Z:" + roomRot.z);
            }
            else
            {
                ev.Player.ShowHint("X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z, 5);
                Log.Debug("Position Get: " + "X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z);
            }
        }
    }

    private void DoorGet(InteractingDoorEventArgs ev)
    {
        if (ev?.Player == null || ev.Door == null) return;
        if (ev.Player.UniqueRole == "Debug")
        {
            Room doorRoom = ev.Door.Room;
            if (doorRoom == null) return;
            Vector3 doorLocalPos = Quaternion.Inverse(doorRoom.Rotation) * (ev.Player.Position - doorRoom.Position);
            Quaternion doorLocalRot = Quaternion.Inverse(doorRoom.Rotation);
            Vector3 doorLocalEuler = doorLocalRot.eulerAngles;
            Vector3 doorRoomRot = doorRoom.Rotation.eulerAngles;

            ev.Player.ShowHint("DoorType:" + ev.Door.Type + "\nName & Room: " + ev.Door.Name + ", " + doorRoom.Type +
                               "\nLocal: " + doorLocalPos.x + "," + doorLocalPos.y + "," + doorLocalPos.z +
                               "\nLocalRot: " + doorLocalEuler.x + "," + doorLocalEuler.y + "," + doorLocalEuler.z +
                               "\nRoomRot: " + doorRoomRot.x + "," + doorRoomRot.y + "," + doorRoomRot.z, 5);

            Log.Debug("Door Get: " + ev.Door.Type);
            Log.Debug(" Name & Room: " + ev.Door.Name + ", " + doorRoom.Type);
            Log.Debug(" LocalPos: X:" + doorLocalPos.x + " Y:" + doorLocalPos.y + " Z:" + doorLocalPos.z);
            Log.Debug(" LocalRot: X:" + doorLocalEuler.x + " Y:" + doorLocalEuler.y + " Z:" + doorLocalEuler.z);
            Log.Debug(" RoomRot: X:" + doorRoomRot.x + " Y:" + doorRoomRot.y + " Z:" + doorRoomRot.z);
        }
        else
        {
            if (ev.Door.Type == DoorType.GateA || ev.Door.Type == DoorType.GateB)
            {
                if (ev.Door.IsLocked && Plugin.Singleton.SpecialEventsHandler.NowEvent == SpecialEventType.None)
                {
                    ev.Player.ShowHint("収容違反への対応として暫くロックされているようだ・・・");
                }
            }
        }
    }

    private void OnUsed(UsedItemEventArgs ev)
    {
        if (ev?.Player == null || ev.Item == null) return; // FIX: nullガード
        if (SpecificFlagsManager.HasFlag(ev.Player, SpecificFlagType.AntiMemeEffectDisabled))
        {
            if (ev.Item.Type == ItemType.SCP500 && !ev.Item.IsCustomItem(out _))
            {
                if (SpecificFlagsManager.HasFlag(ev.Player, SpecificFlagType.Scp207Level4))
                {
                    ev.Player.EnableEffect(EffectType.Scp207, 4);
                    ev.Player.EnableEffect(EffectType.Invigorated, 60);
                }
            }
        }
    }
}
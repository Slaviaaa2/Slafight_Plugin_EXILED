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

    // FIX: ラウンドごとのPIDを保持し、古い遅延コルーチンがラウンドをまたがないようにする
    private int _currentRoundPID = 0;

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

    public void SyncSpecialEvent()
    {
        Timing.CallDelayed(0.05f, () =>
        {
            if (Round.InProgress) return;
            foreach (var player in Player.List)
            {
                if (!IsPlayerValid(player)) continue; // FIX: 無効プレイヤーをスキップ
                string tips = Tips.GetRandomTip();
                player.ShowHint(
                    "\n\n\n\n\n\n\n<size=32>次のイベント：" + Plugin.Singleton.SpecialEventsHandler.LocalizedEventName + "</size>" +
                    $"\n\n<size=28>Tips: {tips}</size>",
                    5555f);
            }
        });
    }

    public void OnRoundRestarted()
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
            _currentRoundPID = 0; // FIX: PIDリセット
        });
    }

    public static Vector3 Scp173SpawnPoint = Vector3.zero;

    public void SetupSpawnPoints(SchematicSpawnedEventArgs ev)
    {
        if (ev?.Schematic == null) return; // FIX: nullガード
        if (ev.Schematic.Name == "Scp173SpawnPoint")
        {
            Scp173SpawnPoint = ev.Schematic.Position;
            ev.Schematic.Destroy();
        }
    }

    public void OnRoundStarted()
    {
        SpecificFlagsManager.ClearAll();

        // FIX: ラウンドPIDをキャプチャして遅延コルーチンのラウンド跨ぎを防止
        _currentRoundPID = Plugin.Singleton.SpecialEventsHandler.EventPID;
        int roundPID = _currentRoundPID;

        if (Plugin.Singleton.SpecialEventsHandler.NowEvent == SpecialEventType.SergeyMakarovReturns)
        {
            Log.Debug("[EventHandler] SergeyMakarovReturns active. Skipping AutoSCP.");
        }
        else
        {
            foreach (var player in Player.List.ToList().Where(p => IsPlayerValid(p)))
            {
                player.ShowHint("");
                SpecificFlagsManager.InitPlayerFlags(player);
            }
        }

        foreach (Door door in Door.List)
        {
            if (door.Type == DoorType.GateA || door.Type == DoorType.GateB)
                door.Lock(120f, DoorLockType.AdminCommand);

            if (door.Type == DoorType.PrisonDoor)
                door.Lock(DoorLockType.AdminCommand);
        }

        Timing.CallDelayed(2f, () =>
        {
            // FIX: ラウンドが変わっていたら処理しない
            if (roundPID != Plugin.Singleton.SpecialEventsHandler.EventPID) return;
            if (!Round.InProgress) return;

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
                    Exiled.API.Features.Cassie.MessageTranslated(
                        "Emergency . Emergency . Multiple scp subjects have breached containment . This is not a drill . All personnel activate emergency protocols . MtfUnit units are being scrambled . Proceed to your emergency stations immediately .",
                        "緊急事態。緊急事態。複数のSCP被験体が収容を突破しました。これは訓練ではありません。全職員は緊急プロトコルを発動してください。機動部隊ユニットを緊急展開要請中。ただちに各自の緊急ポストに向かってください。",
                        true);
                }
                else
                {
                    // ラウンドごとにランダムで日常的な収容違反放送を選ぶ
                    var breachMessages = new (string cassie, string translation)[]
                    {
                        (
                            "Attention all personnel . one or more scp subjects have breached containment . security teams please respond to the affected sectors . all Class D personnel return to your cells immediately . this is a standard containment breach . remain calm and follow protocol .",
                            "全職員へ通達。1体以上のSCP被験体が収容を突破しました。セキュリティチームは該当セクターに向かってください。Dクラス職員は全員ただちに居室に戻りなさい。これは通常の収容違反です。冷静にプロトコルに従ってください。"
                        ),
                        (
                            "Attention . this is cassie . a containment breach has been detected within the facility . security personnel report to your assigned positions . researchers please secure your laboratories . Class D subjects are to be escorted back to cells . all clear will be announced when the situation is resolved .",
                            "通達。こちらCASSIEです。施設内で収容違反が検知されました。セキュリティ職員は担当ポジションに向かってください。研究者の方々はラボを確保してください。Dクラス被験者は居室に誘導してください。状況解決後に安全確認の放送を行います。"
                        ),
                        (
                            "Site 02 standard alert . scp containment failure detected . security response teams are being deployed . non essential personnel are advised to shelter in place until further notice . this situation is being handled . thank you for your cooperation .",
                            "Site-02標準警報。SCP収容失敗を検知。セキュリティ対応チームを展開中です。非必須職員の方々は次の通達があるまで現在地で待機することをお勧めします。状況は対処中です。ご協力ありがとうございます。"
                        ),
                        (
                            "Attention all site 02 personnel . automated systems have detected an anomalous breach in the containment sector . this is a level 2 containment event . security teams stand by for orders . all other personnel maintain your current assignments and await further instructions .",
                            "Site-02全職員へ通達。自動システムが収容セクターにおける異常な収容突破を検知しました。これはレベル2収容事案です。セキュリティチームは命令を待機してください。その他の職員は現在の業務を維持し、続報をお待ちください。"
                        ),
                        (
                            "Good morning site 02 . we have a small situation today . one or more scp subjects have left their designated containment areas . security teams are already responding . all personnel please follow standard protocols . this should be resolved shortly .",
                            "Site-02の皆さん、おはようございます。本日は少し問題が発生しています。1体以上のSCP被験体が指定収容区域を離れました。セキュリティチームはすでに対応中です。全職員は標準プロトコルに従ってください。まもなく解決される見込みです。"
                        ),
                    };

                    var pick = breachMessages[new System.Random().Next(breachMessages.Length)];
                    Exiled.API.Features.Cassie.MessageTranslated(pick.cassie, pick.translation, true);
                }

                foreach (Room room in Room.List)
                {
                    if (room?.RoomLightController != null) // FIX: RoomLightController のnullチェック
                        room.RoomLightController.ServerFlickerLights(3f);
                }
            }

            Timing.CallDelayed(5f, () =>
            {
                // FIX: ラウンドチェック
                if (roundPID != Plugin.Singleton.SpecialEventsHandler.EventPID) return;
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

            // AutoSCP
            Timing.CallDelayed(3f, () =>
            {
                // FIX: ラウンドチェック + ラウンド進行確認
                if (roundPID != Plugin.Singleton.SpecialEventsHandler.EventPID) return;
                if (!Round.InProgress) return;
                if (Plugin.Singleton.SpecialEventsHandler.NowEvent == SpecialEventType.SergeyMakarovReturns) return;
                if (Plugin.Singleton.EventHandler.IsScpAutoSpawnLocked) return;

                int scpCount = Player.List.Count(p => IsPlayerValid(p) && p.Role?.Team == Team.SCPs);
                Log.Debug($"[AutoSCP] SCP count = {scpCount}, players = {Player.List.Count(p => IsPlayerValid(p))}");

                // FIX: ReferenceHub破棄済みの場合に備えてtry-catchで保護
                foreach (var p in Player.List.Where(p => IsPlayerValid(p)))
                {
                    try
                    {
                        Log.Debug($"[AutoSCP] {p.Nickname}: {p.Role?.Type} / {p.Role?.Team}");
                    }
                    catch (Exception e)
                    {
                        Log.Debug($"[AutoSCP] Failed to get player info: {e.Message}");
                    }
                }

                if (scpCount == 0 && Player.List.Count(p => IsPlayerValid(p)) > 0)
                {
                    var alivePlayer = Player.List.FirstOrDefault(p => IsPlayerValid(p) && p.IsAlive);
                    if (alivePlayer != null)
                    {
                        Log.Debug($"[AutoSCP] Forcing 173 to {alivePlayer.Nickname}");
                        alivePlayer.SetRole(RoleTypeId.Scp173);
                        alivePlayer.ShowHint("※SCPが正常に生成されなかった為、SCP-173に変更されました。");
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
        if (ev?.Player == null) return; // FIX: nullガード
        Vector3 playerPosition = ev.Player.Position;
        if (ev.Player.UniqueRole == "Debug")
        {
            if (ev.Player.CurrentRoom != null)
            {
                Room currentRoom = ev.Player.CurrentRoom;
                Vector3 localPos = currentRoom.Rotation * (playerPosition - currentRoom.Position);
                Vector3 localRot = currentRoom.Rotation.eulerAngles;

                ev.Player.ShowHint("X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z +
                                   "\nRoom: " + currentRoom.Type +
                                   "\nLocal: " + localPos.x + "," + localPos.y + "," + localPos.z +
                                   "\nRot: " + currentRoom.Rotation.eulerAngles.x + "," + currentRoom.Rotation.eulerAngles.y + "," + currentRoom.Rotation.eulerAngles.z, 5);

                Log.Debug("Position Get: " + "X:" + playerPosition.x + " Y:" + playerPosition.y + " Z:" + playerPosition.z);
                Log.Debug(" Room: " + currentRoom.Type);
                Log.Debug(" LocalPos: X:" + localPos.x + " Y:" + localPos.y + " Z:" + localPos.z);
                Log.Debug(" RoomRot: X:" + localRot.x + " Y:" + localRot.y + " Z:" + localRot.z);
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
        if (ev?.Player == null || ev.Door == null) return; // FIX: nullガード
        if (ev.Player.UniqueRole == "Debug")
        {
            Room doorRoom = ev.Door.Room;
            if (doorRoom == null) return; // FIX: doorRoom nullガード
            Vector3 doorLocalPos = doorRoom.Rotation * (ev.Player.Position - doorRoom.Position);
            Vector3 doorLocalRot = doorRoom.Rotation.eulerAngles;

            ev.Player.ShowHint("DoorType:" + ev.Door.Type + "\nName & Room: " + ev.Door.Name + ", " + doorRoom.Type +
                               "\nLocal: " + doorLocalPos.x + "," + doorLocalPos.y + "," + doorLocalPos.z +
                               "\nRot: " + doorLocalRot.x + "," + doorLocalRot.y + "," + doorLocalRot.z, 5);

            Log.Debug("Door Get: " + ev.Door.Type);
            Log.Debug(" Name & Room: " + ev.Door.Name + ", " + doorRoom.Type);
            Log.Debug(" LocalPos: X:" + doorLocalPos.x + " Y:" + doorLocalPos.y + " Z:" + doorLocalPos.z);
            Log.Debug(" RoomRot: X:" + doorLocalRot.x + " Y:" + doorLocalRot.y + " Z:" + doorLocalRot.z);
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
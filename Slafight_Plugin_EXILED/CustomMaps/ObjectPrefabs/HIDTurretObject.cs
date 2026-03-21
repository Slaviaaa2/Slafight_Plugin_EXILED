using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;
using NetworkManagerUtils.Dummies;
using PlayerRoles;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomMaps.ObjectPrefabs;

public class HIDTurretObject : ObjectPrefab
{
    // 合計射程（Turret基準）
    [Header("Turret Settings")]
    public float TotalRange { get; set; } = 8f;
    
    // main と forward の間隔
    public float NpcOffsetDistance { get; set; } = 3f;

    public override Vector3 Position
    {
        get => _schematicObject != null ? _schematicObject.Position : base.Position;
        set
        {
            if (_schematicObject != null)
                _schematicObject.Position = value;
            else
                base.Position = value;

            // Schematic 実位置を基準に同期
            var schemPos = _schematicObject != null ? _schematicObject.Position : base.Position;
            SyncNpcPositions(schemPos);
        }
    }

    public override Quaternion Rotation
    {
        get => _schematicObject != null ? _schematicObject.Rotation : base.Rotation;
        set
        {
            if (_schematicObject != null)
                _schematicObject.Rotation = value;
            else
                base.Rotation = value;
        }
    }

    public override Vector3 Scale
    {
        get => _schematicObject != null ? _schematicObject.Scale : base.Scale;
        set
        {
            if (_schematicObject != null)
                _schematicObject.Scale = value;
            else
                base.Scale = value;
        }
    }

    private SchematicObject _schematicObject;
    private CoroutineHandle _coroutineHandle;
    private CoroutineHandle _hidRefreshHandle;
    
    private Npc _dummyMain;    // 本体（ビーム始点寄り）
    private Npc _dummyRange;   // ビーム延長用
    private bool _isFiring;
    private Player _currentTarget;

    private Vector3 SchemCenter => _schematicObject != null ? _schematicObject.Position : base.Position;

    protected override void OnCreate()
    {
        // Schematic を湧かせて、その実座標を基準にする
        _schematicObject = ObjectSpawner.SpawnSchematic("HIDTurretSchem", base.Position, base.Rotation);

        var schemPos = _schematicObject.Position;
        var schemRot = _schematicObject.Rotation;

        // Npc1: 本体（Schematic の近く）
        _dummyMain = Npc.Spawn("HIDTurret_Main", RoleTypeId.Tutorial, true);
        SetupNpc(_dummyMain, schemPos, schemRot);
        
        // Npc2: とりあえず同じ位置でスポーン（後で線上に並べ直す）
        _dummyRange = Npc.Spawn("HIDTurret_Range", RoleTypeId.Tutorial, true);
        SetupNpc(_dummyRange, schemPos, schemRot);
        
        _coroutineHandle = Timing.RunCoroutine(TurretCoroutine());
        _hidRefreshHandle = Timing.RunCoroutine(HidRefreshCoroutine());
        
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        Timing.KillCoroutines(_coroutineHandle, _hidRefreshHandle);
        
        if (_isFiring)
        {
            TryInvokeDummy(_dummyMain, "Shoot->Release");
            TryInvokeDummy(_dummyRange, "Shoot->Release");
            _isFiring = false;
        }
        
        _schematicObject?.Destroy();
        _schematicObject = null;
        
        _dummyMain?.Destroy();
        _dummyRange?.Destroy();
        _dummyMain = null;
        _dummyRange = null;
        
        base.OnDestroy();
    }

    // Npc共通セットアップ
    private void SetupNpc(Npc npc, Vector3 pos, Quaternion rot)
    {
        // 仮置き
        npc.Position = pos;
        npc.Rotation = rot;
    
        // Npc.Spawn 内部の Role.Set 完了後に最終セットアップ
        Timing.CallDelayed(0.6f, () =>
        {
            if (npc?.ReferenceHub == null) return;
        
            npc.IsNoclipPermitted = true;
            npc.IsNoclipEnabled = true;
            
            npc.Position = pos;
            npc.Rotation = rot;

            npc.IsGodModeEnabled = true;
            npc.IsSpectatable = false;

            // Fade 255
            npc.EnableEffect(EffectType.Fade, 255);
        
            // 元の InfoArea 設定を維持する形でマスク
            npc.InfoArea &= PlayerInfoArea.Badge;
            npc.InfoArea &= PlayerInfoArea.CustomInfo;
            npc.InfoArea &= PlayerInfoArea.Nickname;
            npc.InfoArea &= PlayerInfoArea.Role;
            npc.InfoArea &= PlayerInfoArea.UnitName;
            npc.InfoArea &= PlayerInfoArea.PowerStatus;
        
            npc.CurrentItem = Item.Create(ItemType.MicroHID);
        });
    }
    
    // 視線を滑らかにターゲット方向（直線ビーム方向）へ寄せる
    private void AimDummyWithActions(Npc npc, Vector3 targetWorldPos)
    {
        if (npc == null) return;

        var eyePos = npc.Position + Vector3.up * 1.6f;
        var dir = (targetWorldPos - eyePos).normalized;
        if (dir.sqrMagnitude < 0.0001f) return;

        // Yaw / Pitch を算出
        float targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float targetPitch = -Mathf.Asin(dir.y) * Mathf.Rad2Deg;

        var curEuler = npc.CameraTransform.rotation.eulerAngles;
        float currentYaw = curEuler.y;
        float currentPitch = curEuler.x;

        float deltaYaw = Mathf.DeltaAngle(currentYaw, targetYaw);
        float deltaPitch = Mathf.DeltaAngle(currentPitch, targetPitch);

        const float deadZone = 0.5f;
        if (Mathf.Abs(deltaYaw) < deadZone && Mathf.Abs(deltaPitch) < deadZone)
            return;

        string yawAction = null;
        if (deltaYaw > 45f) yawAction = "CurrentHorizontal+45";
        else if (deltaYaw > 10f) yawAction = "CurrentHorizontal+10";
        else if (deltaYaw > 1f) yawAction = "CurrentHorizontal+1";
        else if (deltaYaw < -45f) yawAction = "CurrentHorizontal-45";
        else if (deltaYaw < -10f) yawAction = "CurrentHorizontal-10";
        else if (deltaYaw < -1f) yawAction = "CurrentHorizontal-1";

        string pitchAction = null;
        if (deltaPitch > 45f) pitchAction = "CurrentVertical+45";
        else if (deltaPitch > 10f) pitchAction = "CurrentVertical+10";
        else if (deltaPitch > 1f) pitchAction = "CurrentVertical+1";
        else if (deltaPitch < -45f) pitchAction = "CurrentVertical-45";
        else if (deltaPitch < -10f) pitchAction = "CurrentVertical-10";
        else if (deltaPitch < -1f) pitchAction = "CurrentVertical-1";

        if (yawAction != null)
            TryInvokeDummy(npc, yawAction);
        if (pitchAction != null)
            TryInvokeDummy(npc, pitchAction);
    }

    // Schematic を中心とした直線上に Npc を並べる
    private void SyncNpcAlongBeam(Vector3 dir)
    {
        var center = SchemCenter;

        float mainOffset = 0.5f;                     // 本体のちょい前
        float forwardOffset = mainOffset + NpcOffsetDistance;

        if (_dummyMain != null)
            _dummyMain.Position = center + dir * mainOffset;

        if (_dummyRange != null)
            _dummyRange.Position = center + dir * forwardOffset;
    }

    // 位置だけ変わったときの同期（向きは後で決める）
    private void SyncNpcPositions(Vector3 basePos)
    {
        // デフォは「Turret の forward = Rotation * forward」を使う
        var dir = (Rotation * Vector3.forward).normalized;
        if (dir.sqrMagnitude < 0.0001f)
            dir = Vector3.forward;

        SyncNpcAlongBeam(dir);
    }

    private IEnumerator<float> TurretCoroutine()
    {
        var animator = _schematicObject.AnimationController;
        
        while (true)
        {
            _currentTarget = null;
            foreach (var player in Player.List)
            {
                if (player == null || player.GetTeam() != CTeam.SCPs || !player.IsAlive) continue;
                
                if (Vector3.Distance(player.Position, SchemCenter) <= TotalRange)
                {
                    _currentTarget = player;
                    break;
                }
            }
            
            if (_currentTarget == null)
            {
                if (_isFiring)
                {
                    TryInvokeDummy(_dummyMain, "Shoot->Release");
                    TryInvokeDummy(_dummyRange, "Shoot->Release");
                    _isFiring = false;
                }
                yield return Timing.WaitForSeconds(0.1f);
                continue;
            }

            var center = SchemCenter;
            var toTarget = _currentTarget.Position - center;
            var flatDir = new Vector3(toTarget.x, 0f, toTarget.z).normalized;

            if (flatDir.sqrMagnitude < 0.0001f)
                flatDir = Vector3.forward;

            // Turret 本体もその方向を向かせる
            Rotation = Quaternion.LookRotation(flatDir, Vector3.up);

            // main / forward を「Schematic中心→ターゲット方向」の線上に並べる
            SyncNpcAlongBeam(flatDir);

            // 視線も同じ線の先を見るようにする
            var farPoint = center + flatDir * 100f;
            if (_dummyMain != null)
                AimDummyWithActions(_dummyMain, farPoint);
            if (_dummyRange != null)
                AimDummyWithActions(_dummyRange, farPoint);
            
            // HID発射（両Npc）
            float dist = Vector3.Distance(_currentTarget.Position, center);
            if (dist <= TotalRange && _currentTarget.IsAlive)
            {
                if (!_isFiring)
                {
                    TryInvokeDummy(_dummyMain, "Shoot->Hold");
                    TryInvokeDummy(_dummyRange, "Shoot->Hold");
                    _isFiring = true;
                }
            }
            else if (_isFiring)
            {
                TryInvokeDummy(_dummyMain, "Shoot->Release");
                TryInvokeDummy(_dummyRange, "Shoot->Release");
                _isFiring = false;
            }
            
            yield return Timing.WaitForSeconds(1f / 30f);
        }
    }

    private IEnumerator<float> HidRefreshCoroutine()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(120f);
            
            if (_dummyMain == null || _dummyRange == null) continue;
            
            TryInvokeDummy(_dummyMain, "Shoot->Release");
            TryInvokeDummy(_dummyRange, "Shoot->Release");
            _isFiring = false;
            
            _dummyMain.ClearInventory();
            _dummyRange.ClearInventory();
            yield return Timing.WaitForSeconds(0.5f);
            
            _dummyMain.CurrentItem = Item.Create(ItemType.MicroHID);
            _dummyRange.CurrentItem = Item.Create(ItemType.MicroHID);
        }
    }

    private void TryInvokeDummy(Npc npc, string action)
    {
        var hub = npc.ReferenceHub;
        foreach (var a in DummyActionCollector.ServerGetActions(hub))
        {
            if (a.Name.EndsWith(action)) { a.Action(); break; }
        }
    }
}

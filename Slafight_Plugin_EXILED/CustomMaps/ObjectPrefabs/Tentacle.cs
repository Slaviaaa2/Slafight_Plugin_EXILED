using System.Collections.Generic;
using Exiled.API.Features;
using MEC;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.Extensions;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomMaps.ObjectPrefabs;

public class Tentacle : ObjectPrefab
{
    public override Vector3 Position
    {
        get => _schematicObject != null ? _schematicObject.Position : base.Position;
        set
        {
            if (_schematicObject != null)
                _schematicObject.Position = value;
            else
                base.Position = value;
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

    protected override void OnCreate()
    {
        _schematicObject = ObjectSpawner.SpawnSchematic("Tentacle", base.Position, base.Rotation);
        _coroutineHandle = Timing.RunCoroutine(TentacleCoroutine());
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        _schematicObject?.Destroy();
        Timing.KillCoroutines(_coroutineHandle);
        base.OnDestroy();
    }

    private IEnumerator<float> TentacleCoroutine()
    {
        var animator = _schematicObject.AnimationController;

        // 生成アニメ
        animator.Play("spawning");
        yield return Timing.WaitForSeconds(1f);

        while (true)
        {
            // Idle に戻す
            animator.Play("idle");
            yield return Timing.WaitForSeconds(5f);

            Player targetPlayer = null;

            // 近くのプレイヤー探索
            foreach (var player in Player.List)
            {
                if (player == null || player.GetTeam() == CTeam.SCPs) continue;

                if (Vector3.Distance(player.Position, Position) <= 3f)
                {
                    targetPlayer = player;
                    break;
                }
            }

            // いなければ次ループ
            if (targetPlayer == null)
                continue;

            // 攻撃アニメ開始
            animator.Play("attacking");

            // 攻撃アニメ中、しばらくターゲットを向き続ける（60fps で ~50frame）
            const float attackWindow = 0.83f;      // 50 / 60
            const float checkInterval = 1f / 60f;  // 1 frame 相当
            float elapsed = 0f;

            while (elapsed < attackWindow)
            {
                if (targetPlayer != null && targetPlayer.IsAlive)
                {
                    var toTarget = targetPlayer.Position - Position;
                    toTarget.y = 0f; // 水平だけ向く

                    if (toTarget.sqrMagnitude > 0.001f)
                    {
                        var rot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                        Rotation = rot; // override 経由で schematic 側も回る
                    }
                }

                elapsed += checkInterval;
                yield return Timing.WaitForSeconds(checkInterval);
            }

            // 攻撃判定: 攻撃開始から ~50frame 後もまだ近くにいるか
            if (targetPlayer != null &&
                targetPlayer.IsAlive &&
                Vector3.Distance(targetPlayer.Position, Position) <= 3f)
            {
                targetPlayer.Hurt(35, "SCP-035の触手に殺された");
            }
        }
    }
}

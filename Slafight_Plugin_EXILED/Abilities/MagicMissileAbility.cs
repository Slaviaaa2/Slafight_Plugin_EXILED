using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using ProjectMER.Features;
using ProjectMER.Features.Objects;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.ProximityChat
{
    public class MagicMissileAbility : AbilityBase
    {
        // AbilityBase 抽象プロパティの実装（デフォルト値）
        protected override float DefaultCooldown => 5f;
        protected override int DefaultMaxUses => 5;

        // 完全デフォルト
        public MagicMissileAbility(Player owner)
            : base(owner) { }

        // クールダウンだけ変える
        public MagicMissileAbility(Player owner, float cooldownSeconds)
            : base(owner, cooldownSeconds, null) { }

        // 両方カスタム
        public MagicMissileAbility(Player owner, float cooldownSeconds, int maxUses)
            : base(owner, cooldownSeconds, maxUses) { }

        protected override void ExecuteAbility(Player player)
        {
            Vector3 startPos = player.Position;
            try
            {
                var schem = ObjectSpawner.SpawnSchematic("SCP3005", startPos, player.CameraTransform.forward);
                Timing.RunCoroutine(MissileCoroutine(schem, player));
            }
            catch (Exception ex)
            {
                Log.Error("MagicMissile spawn failed: " + ex.Message);
            }
        }

        private static IEnumerator<float> MissileCoroutine(SchematicObject schem, Player pushPlayer)
        {
            float elapsedTime = 0f;
            float totalDuration = 0.8f;
            Vector3 startPos = schem.transform.position;

            Vector3 cameraForward = pushPlayer.CameraTransform.forward.normalized;
            Vector3 endPos = startPos + cameraForward * 5f;

            while (elapsedTime < totalDuration)
            {
                foreach (Player player in Player.List)
                {
                    if (Vector3.Distance(schem.transform.position, player.Transform.position) <= 1f)
                    {
                        if (player != pushPlayer)
                        {
                            player.Hurt(pushPlayer, 10f, DamageType.Unknown);
                            pushPlayer.ShowHitMarker();
                        }
                    }
                }

                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / totalDuration;
                schem.transform.position = Vector3.Lerp(startPos, endPos, progress);
                yield return 0f;
            }

            schem.Destroy();
        }
    }
}
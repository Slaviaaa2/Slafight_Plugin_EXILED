using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.CustomItems.API.Features;
using Slafight_Plugin_EXILED.API.Enums;
using UnityEngine;
using Random = System.Random;

namespace Slafight_Plugin_EXILED.Extensions;

public static class StaticUtils
{
    // 【既存メソッド: そのまま残す】
    /// <summary>
    /// 指定のCTeamだけ/以外のPlayer選出リストを作成します。
    /// </summary>
    /// <param name="team">CTeamの指定。</param>
    /// <param name="elseMode">falseで指定のCTeamだけ、trueで指定のCTeam以外になります。</param>
    /// <returns></returns>
    public static List<Player> CandidatePlayersByCTeam(CTeam team, bool elseMode = false)
    {
        var allPlayers = Player.List.ToList();
        return elseMode 
            ? allPlayers.Where(p => p != null && p.GetTeam() != team).ToList()
            : allPlayers.Where(p => p != null && p.GetTeam() == team).ToList();
    }

    private static readonly Random _random = new();

    // 【新規: 比率選出メソッド】
    /// <summary>
    /// 指定のCTeam選出リストをランダムにシャッフルしてからreturnします。
    /// </summary>
    /// <param name="excludeTeam">CTeamの指定。</param>
    /// <param name="ratio">比率。1f / 3fとかで3人に一人になったり、0.5とかで百分率指定できます。</param>
    /// <param name="elseMode"><see cref="CandidatePlayersByCTeam"/>のelseModeを参照してください</param>
    /// <returns></returns>
    public static List<Player> SelectRandomPlayersByRatio(CTeam excludeTeam, float ratio, bool elseMode = false)
    {
        var allPlayers = Player.List.ToList();
        if (allPlayers.Count == 0) return [];

        var candidates = CandidatePlayersByCTeam(excludeTeam, elseMode);  // 既存メソッド利用
        int targetCount = Math.Min((int)Math.Truncate(allPlayers.Count * ratio), candidates.Count());

        return candidates.ShuffleTake(targetCount).ToList();
    }

    // 【内部拡張: 外部非公開】
    extension<T>(IEnumerable<T> source)
    {
        private IEnumerable<T> ShuffleTake(int count)
        {
            var list = source.ToList();
            int n = list.Count;
            if (count >= n) return list;

            for (int i = 0; i < count; i++)
            {
                int pos = _random.Next(i, n);
                (list[i], list[pos]) = (list[pos], list[i]);
            }
            return list.Take(count);
        }

        public IEnumerable<T> Shuffle()
        {
            var list = source.ToList();
            int n = list.Count;
            for (int i = 0; i < n - 1; i++)
            {
                int j = UnityEngine.Random.Range(i, n);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }
    }

    // Give Or Drop
    extension(Player player)
    {
        public void GiveOrDrop(ItemType itemType)
        {
            if (player.IsInventoryFull)
            {
                Pickup.CreateAndSpawn(itemType, player.Position + Vector3.up * 0.5f);
            }
            else
            {
                player.AddItem(itemType);
            }
        }

        public void GiveOrDrop(uint customId)
        {
            if (player.IsInventoryFull)
            {
                CustomItem.TrySpawn(customId, player.Position + Vector3.up * 0.5f, out _);
            }
            else
            {
                player.TryAddCustomItem(customId);
            }
        }

        public bool HasPermission(KeycardPermissions permissions, bool requireAll = false)
        {
            if (permissions == KeycardPermissions.None) return true;

            foreach (var item in player.Items.ToList())
            {
                if (!item.IsKeycard || item is not Keycard keycard) continue;
                if (requireAll)
                {
                    // 全権限必須: 全てのフラグがONかチェック
                    if ((keycard.Permissions & permissions) == permissions)
                    {
                        return true;
                    }
                }
                else
                {
                    // いずれかOK: 共通フラグがあればtrue
                    if (keycard.Permissions.HasFlagFast(permissions))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Playerを中心とした四角形範囲(XZ平面)のランダム位置をY固定で取得
        /// </summary>
        /// <param name="halfSize">半径サイズ (例: 5f → ±5m四方)</param>
        /// <param name="fixedY">固定Y座標 (デフォルト: player.Position.y)</param>
        /// <returns>ランダムVector3</returns>
        public Vector3 GetRandomSquarePosition(float halfSize, float fixedY = float.NaN)
        {
            Vector3 center = player.Position;
            float y = float.IsNaN(fixedY) ? center.y : fixedY;
        
            float randomX = UnityEngine.Random.Range(center.x - halfSize, center.x + halfSize);
            float randomZ = UnityEngine.Random.Range(center.z - halfSize, center.z + halfSize);
        
            return new Vector3(randomX, y, randomZ);
        }

        public bool IsFifthist()
        {
            return player.GetTeam() == CTeam.Fifthists || player.GetCustomRole() == CRoleTypeId.Scp3005;
        }

        public bool IsHumanitist()
        {
            return player.GetTeam() != CTeam.FoundationForces;
        }
    }

    extension(Item item)
    {
        public bool IsCustomItem(out CustomItem customItem)
        {
            var result = CustomItem.TryGet(item, out var ci);
            customItem = ci;
            return result;
        }
    }
}
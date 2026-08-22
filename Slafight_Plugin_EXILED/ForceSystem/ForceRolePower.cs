using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.ForceSystem;

/// <summary>
/// TopLead 選出で使う役職の優先度です。大きいほど上位です。
/// </summary>
/// <remarks>
/// 草案の「役職による優先」がこれにあたります。元帥のような上位役職が居るときは、
/// SubLead の貢献度を無視して優先的に TopLead へ昇格します。
///
/// カスタム役職は <see cref="CustomRole.ForceRolePower"/> で自分から名乗ります。
/// ここに表を作らないのは、役職が増えるたびに表を書き足す形へ戻さないためです。
/// バニラ役職だけは列挙型に属性を足せないので、やむを得ずここで対応させています。
/// </remarks>
public static class ForceRolePower
{
    /// <summary>
    /// このプレイヤーの役職優先度です。
    /// </summary>
    /// <remarks>
    /// カスタム役職を持っていればそちらを優先します。0 を返す (=名乗らない) カスタム役職は
    /// バニラ側の値にフォールバックするので、見た目だけ変えた役職が格下げされません。
    /// </remarks>
    public static int Of(Player player)
    {
        if (player is null) return 0;

        if (CustomRole.Of(player) is { } customRole && customRole.ForceRolePower > 0)
            return customRole.ForceRolePower;

        return Of(player.Role?.Type ?? RoleTypeId.None);
    }

    /// <summary>
    /// バニラ役職の優先度です。
    /// </summary>
    /// <remarks>
    /// 値はバニラの <c>NineTailedFoxNamingRule.GetRolePower</c> に合わせてあります
    /// (Private 1 / Specialist・Sergeant 2 / Captain 3)。
    /// クライアントが名札に出す GiveOrders/FollowOrders の判定と食い違わないようにするためです。
    /// カオス側はバニラに序列が無いので、隊長格だけを 2 として扱います。
    /// </remarks>
    public static int Of(RoleTypeId role) => role switch
    {
        RoleTypeId.NtfCaptain => 3,
        RoleTypeId.NtfSergeant or RoleTypeId.NtfSpecialist => 2,
        RoleTypeId.NtfPrivate => 1,

        RoleTypeId.ChaosRepressor => 2,
        RoleTypeId.ChaosMarauder or RoleTypeId.ChaosRifleman or RoleTypeId.ChaosConscript => 1,

        RoleTypeId.FacilityGuard => 1,

        _ => 0,
    };
}

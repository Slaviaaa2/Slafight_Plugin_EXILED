using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.ForceSystem;

/// <summary>
/// 「誰にどの隊が見えるか」を決めます。差し替え可能です。
/// </summary>
/// <remarks>
/// 部隊一覧をそのまま全部見せると索敵情報になってしまうので、
/// 表示は必ずここを通します。
///
/// <b>既定は <see cref="ForceBase.Team"/> による仕分けです。</b>
/// 隊が <see cref="CustomTeam"/> を名乗っていればそれで同じ側かを決め、
/// 名乗っていなければ隊の型で決めます。
/// <see cref="PlayerRoles.Faction"/> で括らないのは、
/// <see cref="PlayerRoles.Faction.FoundationEnemy"/> にカオスと D クラスが両方入るからです。
/// あれで括るとギャングにカオスの部隊が丸見えになります。
///
/// ゲームモードなどで見え方を変えたいときは <see cref="Rule"/> を差し替えてください。
/// </remarks>
/// <example>
/// <code>
/// // 全部隊を全員に見せる (観戦者向けモードなど)
/// ForceVisibility.Rule = (_, _) => true;
///
/// // 自分の所属だけ見せる
/// ForceVisibility.Rule = (viewer, force) => ReferenceEquals(viewer.GetForce(), force);
///
/// // 既定に戻す
/// ForceVisibility.Reset();
/// </code>
/// </example>
public static class ForceVisibility
{
    /// <summary>
    /// 見えるかどうかを決める述語です。差し替えると表示範囲が変わります。
    /// </summary>
    /// <remarks>
    /// null を入れると既定に戻ります。ラウンドをまたいでも保持されるので、
    /// ゲームモード側で差し替えたなら終了時に <see cref="Reset"/> してください。
    /// </remarks>
    public static Func<Player, ForceBase, bool> Rule
    {
        get => rule;
        set => rule = value ?? Default;
    }

    private static Func<Player, ForceBase, bool> rule = Default;

    /// <summary>
    /// 既定のルールに戻します。
    /// </summary>
    public static void Reset() => rule = Default;

    /// <summary>
    /// このプレイヤーに見える隊です。
    /// </summary>
    public static IEnumerable<ForceBase> VisibleTo(Player viewer)
    {
        if (viewer is null) return [];

        return ForceRegistry.All.Where(force => IsVisible(viewer, force));
    }

    /// <summary>
    /// この隊がこのプレイヤーに見えるかどうか。
    /// </summary>
    /// <remarks>
    /// 差し替えたルールが例外を投げても表示を巻き添えにしません。
    /// HUD は 1 秒ごとに回るので、落ちるたびにログが溢れるのも避けます。
    /// </remarks>
    public static bool IsVisible(Player viewer, ForceBase force)
    {
        if (viewer is null || force is null) return false;

        try
        {
            return rule(viewer, force);
        }
        catch
        {
            return Default(viewer, force);
        }
    }

    /// <summary>
    /// 既定のルールです。
    /// </summary>
    private static bool Default(Player viewer, ForceBase force)
    {
        ForceBase own = viewer.GetForce();

        // 自分の所属は常に見える。
        if (ReferenceEquals(own, force)) return true;

        // 部隊システムの対象外 (SCP など) には何も見せない。
        if (viewer.Role is null || !ForceKinds.IsForceTeam(viewer.Role.Team)) return false;

        // 陣営が違う隊は見せない。索敵情報になる。
        if (viewer.Role.Team.GetFaction() != force.Faction) return false;

        // 隊が CustomTeam を名乗っているなら、それが唯一の正解。
        if (force.Team is { } theirs)
        {
            CustomTeam mine = own?.Team ?? CustomTeam.Of(viewer);

            return mine is not null && mine.IsSameSide(theirs);
        }

        // 名乗っていないなら隊の型で仕分ける。
        // 機動部隊どうし・カオスどうし・ギャングどうしだけが互いに見える。
        if (own is not null)
            return own.GetType() == force.GetType();

        // まだどこにも属していない場合は、自分がこれから入りうる隊だけ見せる。
        return force.GetType() == ForceKinds.For(viewer);
    }
}

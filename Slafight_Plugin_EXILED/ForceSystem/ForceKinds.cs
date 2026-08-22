using System;
using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.ForceSystem.Forces;

namespace Slafight_Plugin_EXILED.ForceSystem;

/// <summary>
/// バニラ陣営から、その人が属する隊の種類を決めます。
/// </summary>
/// <remarks>
/// <b>ここが「陣営 → 隊の種類」を持つ唯一の場所です。</b>
/// 分隊の編成・表示の仕分け・波からの生成が同じ表を見るようにしてあります。
/// 3 箇所に散らすと、ギャングだけ分隊が組めないといった食い違いが生まれます。
///
/// カスタム役職が独自の隊に属するべきときは、波の側で
/// <c>SpawnSet.CreateForce</c> を override してください。そちらが優先されます。
/// </remarks>
public static class ForceKinds
{
    /// <summary>
    /// このプレイヤーが属する隊の種類です。対象外なら null。
    /// </summary>
    public static Type For(Player player) => For(player?.Role?.Team ?? Team.Dead);

    /// <summary>
    /// この陣営が属する隊の種類です。対象外なら null。
    /// </summary>
    /// <remarks>
    /// SCP と科学者は隊を組みません。草案が扱っているのは機動部隊・カオス・D クラスだけです。
    /// </remarks>
    public static Type For(Team team) => team switch
    {
        Team.FoundationForces => typeof(MobileTaskForce),
        Team.ChaosInsurgency => typeof(ChaosForce),
        Team.ClassD => typeof(ClassDGang),
        _ => null,
    };

    /// <summary>
    /// この陣営が部隊システムの対象かどうか。
    /// </summary>
    public static bool IsForceTeam(Team team) => For(team) is not null;

    /// <summary>
    /// この陣営に合った隊を作ります。対象外なら null。
    /// </summary>
    public static ForceBase Create(Team team) => team switch
    {
        Team.FoundationForces => new MobileTaskForce(null, null),
        Team.ChaosInsurgency => new ChaosForce(),
        Team.ClassD => new ClassDGang(),
        _ => null,
    };
}

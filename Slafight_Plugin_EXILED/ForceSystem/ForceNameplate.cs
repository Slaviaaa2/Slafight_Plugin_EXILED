using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Enums;
using Slafight_Plugin_EXILED.API.Features;

namespace Slafight_Plugin_EXILED.ForceSystem;

/// <summary>
/// 名札に部隊名と階級を出します。
/// </summary>
/// <remarks>
/// バニラは <c>UnitNamingRule.AppendName</c> が
/// <c>PlayerInfoArea.PowerStatus</c> を見て「命令を下せ / 命令に従え / 同格」を出しますが、
/// 部隊システムは独自の階級を持つので併記すると 2 種類の階級が並びます。
/// <see cref="Options"/> で <c>ShowPowerStatus</c> を落とし、こちらの表記に一本化します。
///
/// 名札そのものは <see cref="CustomInfoDisplay"/> が組み立てます。
/// ここは <c>%extrainfo%</c> の中身を返すだけで、
/// カスタム役職が自分で組んだ名札を横から潰しません。
/// </remarks>
public static class ForceNameplate
{
    /// <summary>
    /// 部隊システムに乗っている人の名札設定です。
    /// </summary>
    private static readonly CustomInfoDisplayOptions Options = new()
    {
        // バニラの階級表示をやめて、こちらの TopLead / SubLead 表記に一本化する。
        ShowPowerStatus = false,
    };

    /// <summary>
    /// 名札に差し込む行を返します。部隊システムの対象外なら null。
    /// </summary>
    /// <remarks>
    /// 色は <see cref="ServerColors"/> から選びます。名札はバニラの
    /// <c>NicknameSync.ValidateCustomInfo</c> を通るので、それ以外の色を使うと
    /// 名札全体が弾かれて何も表示されなくなります。
    /// </remarks>
    public static string Text(Player player)
    {
        // SCP や科学者は部隊システムの対象外。役職が変わった後も
        // 隊員状態は残るので、陣営を毎回見て判断する。
        if (player?.Role is null || !ForceKinds.IsForceTeam(player.Role.Team)) return null;

        if (player.GetForceMember() is not { } member) return null;

        ForceBase force = member.Force;
        string rank = member.Level.NameOf(force);

        if (force is null)
            return $"<size=20><color={ServerColors.Nickel}>{rank}</color></size>";

        string color = force.IsMainForce ? ServerColors.Yellow : ServerColors.Cyan;

        return $"<size=20><color={color}>{force.Name}</color> " +
               $"<color={ServerColors.Silver}>{force.KindName}・{rank}</color></size>";
    }

    /// <summary>
    /// この人の名札を部隊システムの管理下に置いて描き直します。
    /// </summary>
    /// <remarks>
    /// 既にカスタム役職が名札を組んでいる場合は、そちらの設定を尊重して
    /// 差し込み行だけを更新します。
    /// </remarks>
    public static void Refresh(Player player)
    {
        if (player is null) return;

        CustomInfoDisplay.EnsureTracked(player, Options);
        CustomInfoDisplay.Refresh(player);
    }
}

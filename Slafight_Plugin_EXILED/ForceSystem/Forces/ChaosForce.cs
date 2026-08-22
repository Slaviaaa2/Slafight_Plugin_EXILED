using PlayerRoles;

namespace Slafight_Plugin_EXILED.ForceSystem.Forces;

/// <summary>
/// カオス・インサージェンシーの隊です。
/// </summary>
/// <remarks>
/// 草案の派生システムどおり、標準との差分は 2 つだけです。
/// <list type="bullet">
/// <item>SCP-914 の使用が減点対象から外れる。</item>
/// <item>継続所属時間の影響が「大」から「中」に抑えられる。</item>
/// </list>
/// この 2 つは <see cref="ForceBase"/> 側の <c>virtual</c> を override して表現します。
/// </remarks>
public sealed class ChaosForce : ForceBase
{
    internal ChaosForce(string name)
    {
        Name = name;
    }

    /// <inheritdoc />
    public override string Name { get; }

    /// <summary>
    /// カオス側にバニラの部隊名はありません。
    /// </summary>
    /// <remarks>
    /// <see cref="Respawning.NamingRules.NamingRulesManager.AllNamingRules"/> は
    /// <see cref="Team.FoundationForces"/> しか持たないため、
    /// 名札には出せません。表示はこちらの HUD が受け持ちます。
    /// </remarks>
    public override byte? UnitId => null;

    /// <inheritdoc />
    public override Faction Faction => Faction.FoundationEnemy;

    /// <summary>
    /// 継続所属時間の影響は「中」に抑えられます。
    /// </summary>
    /// <remarks>草案「継続所属時間の影響が中に抑えられています」。</remarks>
    public override ForceImpact MembershipImpact => ForceImpact.Medium;

    /// <summary>
    /// SCP-914 の使用は減点対象から外れます。
    /// </summary>
    /// <remarks>草案「SCP-914 の使用が減点対象から外れ」。</remarks>
    public override ForceImpact Scp914KeycardPenalty => ForceImpact.None;
}

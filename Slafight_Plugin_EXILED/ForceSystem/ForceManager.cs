using Exiled.API.Features;
using PlayerRoles;
using Slafight_Plugin_EXILED.API.Core.Features;
using Slafight_Plugin_EXILED.API.Features;
using Slafight_Plugin_EXILED.ForceSystem.Forces;

using ServerHandlers = Exiled.Events.Handlers.Server;

namespace Slafight_Plugin_EXILED.ForceSystem;

/// <summary>
/// 部隊システムの入口です。波が出たら隊を作り、評価ループを回します。
/// </summary>
/// <remarks>
/// クラス名が名前空間 (<c>Slafight_Plugin_EXILED.ForceSystem</c>) と衝突しないよう
/// <c>ForceSystem</c> ではなく <c>ForceManager</c> にしてあります。
/// 同名だと外から <c>ForceSystem.Something</c> が曖昧になります。
///
/// <see cref="EventHandlerBase"/> を継承しているだけで
/// <c>EventHandlerRegistry</c> が生成・購読します。<c>Plugin.cs</c> には何も書きません。
/// </remarks>
public sealed class ForceManager : EventHandlerBase
{
    /// <inheritdoc />
    public override void RegisterEvents()
    {
        // 名札の %extrainfo% に部隊名と階級を差し込む。
        CustomInfoDisplay.ExtraInfoProvider = ForceNameplate.Text;

        SpawnSystem.Spawned += OnSpawned;
        ServerHandlers.RestartingRound += OnRestartingRound;
        ServerHandlers.WaitingForPlayers += OnRestartingRound;
        ServerHandlers.RoundStarted += OnRoundStarted;
    }

    /// <inheritdoc />
    public override void UnregisterEvents()
    {
        if (CustomInfoDisplay.ExtraInfoProvider == ForceNameplate.Text)
            CustomInfoDisplay.ExtraInfoProvider = null;

        SpawnSystem.Spawned -= OnSpawned;
        ServerHandlers.RestartingRound -= OnRestartingRound;
        ServerHandlers.WaitingForPlayers -= OnRestartingRound;
        ServerHandlers.RoundStarted -= OnRoundStarted;

        ForceRegistry.Reset();
    }

    /// <summary>
    /// ラウンドが始まったら評価ループを起こします。
    /// </summary>
    /// <remarks>
    /// <see cref="RoundScope"/> に載せるので、ラウンド再開で必ず止まります。
    /// 自前でコルーチンハンドルを抱えません。
    /// </remarks>
    private static void OnRoundStarted() => ForceEvaluator.Start();

    private static void OnRestartingRound() => ForceRegistry.Reset();

    /// <summary>
    /// 波が出たら、その波ぶんの隊を 1 つ作ります。
    /// </summary>
    /// <remarks>
    /// <see cref="SpawnedEventArgs.UnitId"/> は <see cref="SpawnSystem"/> が
    /// この波の全員に配った番号です。NTF の波でなければ null なので、
    /// そのときは名前だけこちらで作ります。
    /// </remarks>
    private static void OnSpawned(SpawnedEventArgs ev)
    {
        if (ev?.Players is not { Count: > 0 }) return;

        // 波が自分で名乗るならそれが最優先。名乗らなければ陣営から決める。
        ForceBase force = ev.Wave?.CreateForce(ev.UnitId, ev.UnitName)
                          ?? Create(ev.Wave?.RespawnFaction ?? Faction.Unclassified, ev.UnitId, ev.UnitName);

        if (force is null) return;

        ForceRegistry.Register(force);

        foreach (Player player in ev.Players)
        {
            if (ForceRegistry.MemberOf(player) is not { } member) continue;

            force.Add(member);
        }

        ForceEvaluator.AssignTopLead(force);
    }

    /// <summary>
    /// 陣営に合った隊を作ります。部隊システムの対象外なら null。
    /// </summary>
    /// <remarks>
    /// 派生システムの選択はここだけです。草案が「GoC や第五教会は後ほど決定」と
    /// しているので、まだ足しません。
    /// </remarks>
    private static ForceBase Create(Faction faction, byte? unitId, string unitName)
    {
        switch (faction)
        {
            case Faction.FoundationStaff:
                // NTF はバニラが採番した部隊名をそのまま名乗る。名札との食い違いを避けるため。
                return new MobileTaskForce(unitName, unitId);

            case Faction.FoundationEnemy:
                return new ChaosForce();

            default:
                return null;
        }
    }
}

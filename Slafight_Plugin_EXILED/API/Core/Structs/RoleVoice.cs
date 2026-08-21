using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Core.Features;
using Slafight_Plugin_EXILED.API.Features;

namespace Slafight_Plugin_EXILED.API.Core.Structs;

/// <summary>
/// 役職が名乗る近接ボイスの扱いです。
/// </summary>
/// <remarks>
/// <b>近接ボイスは「SCP チャットを近くの人にも聞こえるようにする」仕組みです。</b>
/// つまり <see cref="Features.CustomRole.BaseRole"/> が SCP チャットで話せる役職でなければ、
/// ここで <see cref="Toggle"/> を宣言しても発声経路そのものが無いので何も起きません。
/// 人間ベースの役職に付けると、「使えます」の案内だけ出て声が乗らない状態になります。
/// </remarks>
public readonly struct RoleProximitySettings
{
    public RoleProximitySettings(bool isAvailable, bool enabledByDefault = true)
    {
        IsAvailable = isAvailable;
        EnabledByDefault = isAvailable && enabledByDefault;
    }

    /// <summary>この役職が近接ボイスを使えるか。</summary>
    public bool IsAvailable { get; }

    /// <summary>スポーン直後から有効にするか。</summary>
    public bool EnabledByDefault { get; }

    /// <summary>使えません。</summary>
    public static RoleProximitySettings Disabled => default;

    /// <summary>切り替えで使えるようにします。</summary>
    public static RoleProximitySettings Toggle(bool enabledByDefault = true) => new(true, enabledByDefault);
}

/// <summary>
/// 役職が持つ声の経路 1 本です。null を返すと次の経路の判定に進みます。
/// </summary>
public readonly struct RoleVoiceRoute
{
    public RoleVoiceRoute(Func<VoiceRouteContext, VoiceRouteDecision?> evaluator)
    {
        Evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    /// <summary>判定そのものです。</summary>
    public Func<VoiceRouteContext, VoiceRouteDecision?> Evaluator { get; }

    /// <summary>この経路を判定します。</summary>
    public VoiceRouteDecision? Evaluate(VoiceRouteContext context) => Evaluator?.Invoke(context);

    /// <summary>
    /// 条件に合う受け手へ流します。
    /// </summary>
    public static RoleVoiceRoute ToPlayers(
        Predicate<Player> receivers,
        VoiceRouteDecision decision,
        Predicate<VoiceRouteContext> condition = null,
        bool includeSender = false)
    {
        if (receivers is null)
            throw new ArgumentNullException(nameof(receivers));

        return new RoleVoiceRoute(context =>
        {
            if (!includeSender && context.Sender.Id == context.Receiver.Id)
                return null;

            return receivers(context.Receiver) && (condition is null || condition(context))
                ? decision
                : null;
        });
    }

    /// <summary>
    /// 指定した陣営へ流します。陣営は型で指してください。
    /// </summary>
    public static RoleVoiceRoute ToTeams(
        IEnumerable<CustomTeam> receiverTeams,
        VoiceRouteDecision decision,
        Predicate<VoiceRouteContext> condition = null,
        bool includeSender = false,
        bool aliveReceiversOnly = true)
    {
        if (receiverTeams is null)
            throw new ArgumentNullException(nameof(receiverTeams));

        HashSet<CustomTeam> receiverSet = new HashSet<CustomTeam>(receiverTeams);

        return ToPlayers(
            receiver => (!aliveReceiversOnly || receiver.IsAlive) &&
                        receiverSet.Contains(CustomTeam.Of(receiver)),
            decision,
            condition,
            includeSender);
    }

    /// <summary>
    /// 全員に効かせます。前の経路で拾われなかった相手を塞ぐ用途に使います。
    /// </summary>
    public static RoleVoiceRoute All(
        VoiceRouteDecision decision,
        Predicate<VoiceRouteContext> condition = null,
        bool includeSender = true)
        => ToPlayers(_ => true, decision, condition, includeSender);
}

/// <summary>
/// 役職が名乗る声の設定一式です。経路は並び順に判定されます。
/// </summary>
/// <example>
/// <code>
/// public override RoleVoiceSettings Voice => RoleVoiceSettings.WithProximity();
/// </code>
/// </example>
public readonly struct RoleVoiceSettings
{
    private static readonly IReadOnlyList<RoleVoiceRoute> EmptyRoutes = [];

    private readonly IReadOnlyList<RoleVoiceRoute> routes;

    public RoleVoiceSettings(
        RoleProximitySettings proximity = default,
        IReadOnlyList<RoleVoiceRoute> routes = null)
    {
        Proximity = proximity;
        this.routes = routes;
    }

    /// <summary>近接ボイスの扱いです。</summary>
    public RoleProximitySettings Proximity { get; }

    /// <summary>この役職が持つ声の経路です。</summary>
    public IReadOnlyList<RoleVoiceRoute> Routes => routes ?? EmptyRoutes;

    /// <summary>何も指定しません。</summary>
    public static RoleVoiceSettings None => default;

    /// <summary>
    /// 近接ボイスだけ使えるようにします。
    /// <b><see cref="Features.CustomRole.BaseRole"/> が SCP チャットを持つ役職のときだけ意味があります</b>
    /// (<see cref="RoleProximitySettings"/> の注記を参照)。
    /// </summary>
    public static RoleVoiceSettings WithProximity(bool enabledByDefault = true)
        => new(RoleProximitySettings.Toggle(enabledByDefault));
}

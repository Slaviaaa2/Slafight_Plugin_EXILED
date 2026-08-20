using Exiled.API.Features;

namespace Slafight_Plugin_EXILED.API.Core.Interfaces;

/// <summary>
/// 特定のプレイヤーに属するものです。
/// <see cref="Features.EventHandlerBase"/> はこれを実装したハンドラを、
/// 持ち主が退出した時点で自動的に破棄します。
/// </summary>
public interface IPlayerOwn
{
    Player Player { get; }
}

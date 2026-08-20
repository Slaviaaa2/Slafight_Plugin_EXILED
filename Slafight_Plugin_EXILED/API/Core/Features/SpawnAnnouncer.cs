using Exiled.API.Features;
using Slafight_Plugin_EXILED.Extensions;

namespace Slafight_Plugin_EXILED.API.Core.Features;

/// <summary>
/// 波が出たときのアナウンスを流します。
/// </summary>
/// <remarks>
/// <b>ここには波ごとの分岐がありません。</b>
/// 何と言うか・何を流すかは波自身が
/// <see cref="SpawnSet.Announcement"/> と <see cref="SpawnSet.Theme"/> で名乗ります。
/// 旧実装は同じ処理を 24 分岐の switch 2 つで書いていました。
/// 波を足すときにこのファイルを触る必要はありません。
///
/// このクラスはどこからも登録されていません。<see cref="EventHandlerBase"/> を
/// 継承しているだけで <see cref="EventHandlerRegistry"/> が購読させます。
/// </remarks>
public sealed class SpawnAnnouncer : EventHandlerBase
{
    /// <inheritdoc />
    public override void RegisterEvents()
    {
        SpawnSystem.Spawned += OnSpawned;
    }

    /// <inheritdoc />
    public override void UnregisterEvents()
    {
        SpawnSystem.Spawned -= OnSpawned;
    }

    private static void OnSpawned(object sender, SpawnedEventArgs ev)
    {
        (string cassie, string subtitle) = ev.Wave.Announcement(ev.SpawnCount);

        if (cassie is { Length: > 0 })
            CassieExtensions.CassieTranslated(cassie, subtitle ?? string.Empty, false);

        if (ev.Wave.Theme is { Length: > 0 } theme)
            Log.Debug($"[Slafight] ウェーブ '{ev.Wave.Name}' のテーマ: {theme}");
    }
}

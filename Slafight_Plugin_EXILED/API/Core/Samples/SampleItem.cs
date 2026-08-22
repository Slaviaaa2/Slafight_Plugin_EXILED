using Exiled.API.Features;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.API.Core.Samples;

/// <summary>
/// カスタムアイテムの書き方の見本です。
///
/// 見どころは <see cref="charges"/> が<b>ただのフィールド</b>であることです。
/// 追跡キーはシリアルで、それはインベントリのアイテムと地面のピックアップで共通なので、
/// 落として拾い直しても同じインスタンスが付いてきます。
/// シリアルをキーにした static 辞書を用意する必要はありません。
/// </summary>
public sealed class SampleItem : CustomItem
{
    /// <summary>
    /// per-item 状態。static 辞書は要りません。
    /// </summary>
    private int charges = 3;

    public override ItemType BaseType => ItemType.Medkit;

    public override string Name => "Sample Medkit";

    public override string Description => $"動作確認用。残り {charges} 回。";

    protected override void OnPickedUp(Player player)
    {
        Log.Debug($"[Sample] {player?.Nickname} が {Name} を拾いました (残り {charges})。");
    }

    /// <summary>
    /// 使用モーションの完了時に、バニラの効果を差し替えます。
    /// </summary>
    protected override void OnUsed()
    {
        charges--;

        if (charges > 0)
        {
            Owner.ShowHint($"{Name}: 残り {charges} 回", 3f);

            return;
        }

        Owner.ShowHint($"{Name} を使い切りました。", 3f);
        Destroy();
    }
}

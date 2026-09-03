#nullable enable
namespace Slafight_Plugin_EXILED.API.Core.Structs;

/// <summary>
/// CustomTeam / CustomRole が名乗る定型文通信の上書きです。
/// null の項目は下位の設定（バニラ陣営 → CustomTeam → CustomRole）を引き継ぎます。
/// </summary>
public readonly struct CommunicationPolicy
{
    public CommunicationPolicy(
        bool? isAvailable = null,
        string? prefix = null,
        string? proximityPrefix = null,
        string? radioPrefix = null,
        string? proximityLabel = null,
        string? radioLabel = null,
        float? proximityRange = null,
        bool? isRadioAvailable = null)
    {
        IsAvailable = isAvailable;
        Prefix = prefix;
        ProximityPrefix = proximityPrefix;
        RadioPrefix = radioPrefix;
        ProximityLabel = proximityLabel;
        RadioLabel = radioLabel;
        ProximityRange = proximityRange;
        IsRadioAvailable = isRadioAvailable;
    }

    /// <summary>通信機能を利用できるか。null なら下位設定を引き継ぎます。</summary>
    public bool? IsAvailable { get; }

    /// <summary>
    /// 発言者名の前へ付ける陣営表記。null なら引き継ぎ、空文字なら非表示にします。
    /// </summary>
    public string? Prefix { get; }

    /// <summary>近接で届いた行だけに使う陣営表記。null なら <see cref="Prefix"/> を使います。</summary>
    public string? ProximityPrefix { get; }

    /// <summary>Radioで届いた行だけに使う陣営表記。null なら <see cref="Prefix"/> を使います。</summary>
    public string? RadioPrefix { get; }

    /// <summary>近接経路の先頭に出す表示。null なら引き継ぎます。</summary>
    public string? ProximityLabel { get; }

    /// <summary>Radio経路の先頭に出す表示。null なら引き継ぎます。</summary>
    public string? RadioLabel { get; }

    /// <summary>近接通信の距離。null なら引き継ぎます。</summary>
    public float? ProximityRange { get; }

    /// <summary>使用可能なRadioを所持しているとき無線経路も使うか。null なら引き継ぎます。</summary>
    public bool? IsRadioAvailable { get; }

    /// <summary>可否を引き継ぎます。prefix を指定すれば表記だけ上書きします。</summary>
    public static CommunicationPolicy Inherit(
        string? prefix = null,
        string? proximityPrefix = null,
        string? radioPrefix = null,
        string? proximityLabel = null,
        string? radioLabel = null,
        float? proximityRange = null,
        bool? isRadioAvailable = null)
        => new(
            null,
            prefix,
            proximityPrefix,
            radioPrefix,
            proximityLabel,
            radioLabel,
            proximityRange,
            isRadioAvailable);

    /// <summary>この層で通信を許可します。</summary>
    public static CommunicationPolicy Enabled(
        string? prefix = null,
        string? proximityPrefix = null,
        string? radioPrefix = null,
        string? proximityLabel = null,
        string? radioLabel = null,
        float? proximityRange = null,
        bool? isRadioAvailable = null)
        => new(
            true,
            prefix,
            proximityPrefix,
            radioPrefix,
            proximityLabel,
            radioLabel,
            proximityRange,
            isRadioAvailable);

    /// <summary>この層で通信を禁止します。</summary>
    public static CommunicationPolicy Disabled(string? prefix = null) => new(false, prefix);
}

/// <summary>プレイヤーについて全上書きを解決した、実際に使用する通信設定です。</summary>
public readonly struct ResolvedCommunicationPolicy
{
    public ResolvedCommunicationPolicy(
        bool isAvailable,
        string? prefix,
        string? proximityPrefix = null,
        string? radioPrefix = null,
        string? proximityLabel = "近接",
        string? radioLabel = "通信",
        float proximityRange = 8f,
        bool isRadioAvailable = true)
    {
        IsAvailable = isAvailable;
        Prefix = prefix ?? string.Empty;
        ProximityPrefix = proximityPrefix ?? Prefix;
        RadioPrefix = radioPrefix ?? Prefix;
        ProximityLabel = proximityLabel ?? "近接";
        RadioLabel = radioLabel ?? "通信";
        ProximityRange = proximityRange;
        IsRadioAvailable = isRadioAvailable;
    }

    public bool IsAvailable { get; }

    public string Prefix { get; }

    public string ProximityPrefix { get; }

    public string RadioPrefix { get; }

    public string ProximityLabel { get; }

    public string RadioLabel { get; }

    public float ProximityRange { get; }

    public bool IsRadioAvailable { get; }

    internal ResolvedCommunicationPolicy Apply(CommunicationPolicy policy)
        => new(
            policy.IsAvailable ?? IsAvailable,
            policy.Prefix ?? Prefix,
            policy.ProximityPrefix ?? policy.Prefix ?? ProximityPrefix,
            policy.RadioPrefix ?? policy.Prefix ?? RadioPrefix,
            policy.ProximityLabel ?? ProximityLabel,
            policy.RadioLabel ?? RadioLabel,
            policy.ProximityRange ?? ProximityRange,
            policy.IsRadioAvailable ?? IsRadioAvailable);
}

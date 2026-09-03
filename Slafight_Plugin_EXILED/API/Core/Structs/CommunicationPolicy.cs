#nullable enable
namespace Slafight_Plugin_EXILED.API.Core.Structs;

/// <summary>
/// CustomTeam / CustomRole が名乗る定型文通信の上書きです。
/// null の項目は下位の設定（バニラ陣営 → CustomTeam → CustomRole）を引き継ぎます。
/// </summary>
public readonly struct CommunicationPolicy
{
    public CommunicationPolicy(bool? isAvailable = null, string? prefix = null)
    {
        IsAvailable = isAvailable;
        Prefix = prefix;
    }

    /// <summary>通信機能を利用できるか。null なら下位設定を引き継ぎます。</summary>
    public bool? IsAvailable { get; }

    /// <summary>
    /// 発言者名の前へ付ける陣営表記。null なら引き継ぎ、空文字なら非表示にします。
    /// </summary>
    public string? Prefix { get; }

    /// <summary>可否を引き継ぎます。prefix を指定すれば表記だけ上書きします。</summary>
    public static CommunicationPolicy Inherit(string? prefix = null) => new(null, prefix);

    /// <summary>この層で通信を許可します。</summary>
    public static CommunicationPolicy Enabled(string? prefix = null) => new(true, prefix);

    /// <summary>この層で通信を禁止します。</summary>
    public static CommunicationPolicy Disabled(string? prefix = null) => new(false, prefix);
}

/// <summary>プレイヤーについて全上書きを解決した、実際に使用する通信設定です。</summary>
public readonly struct ResolvedCommunicationPolicy
{
    public ResolvedCommunicationPolicy(bool isAvailable, string? prefix)
    {
        IsAvailable = isAvailable;
        Prefix = prefix ?? string.Empty;
    }

    public bool IsAvailable { get; }

    public string Prefix { get; }

    internal ResolvedCommunicationPolicy Apply(CommunicationPolicy policy)
        => new(
            policy.IsAvailable ?? IsAvailable,
            policy.Prefix ?? Prefix);
}

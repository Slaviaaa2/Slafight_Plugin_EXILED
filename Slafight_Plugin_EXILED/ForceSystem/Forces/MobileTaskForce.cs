using PlayerRoles;

namespace Slafight_Plugin_EXILED.ForceSystem.Forces;

/// <summary>
/// 機動部隊の隊です。部隊システムの標準ルールがそのまま適用されます。
/// </summary>
/// <remarks>
/// 草案が基準として書いている振る舞いはすべて <see cref="ForceBase"/> の既定値です。
/// ここで override するものはありません。派生システムとの差分を読みたいときは
/// <see cref="ChaosForce"/> と <see cref="ClassDGang"/> を見てください。
/// </remarks>
public sealed class MobileTaskForce : ForceBase
{
    private readonly string vanillaName;

    internal MobileTaskForce(string vanillaName, byte? unitId)
    {
        this.vanillaName = vanillaName;
        UnitId = unitId;
    }

    /// <inheritdoc />
    public override byte? UnitId { get; }

    /// <summary>
    /// 本隊はバニラが採番した部隊名をそのまま使います。
    /// </summary>
    /// <remarks>
    /// 名札に出る <c>(ALPHA-01)</c> と食い違わせないためです。
    /// 分隊にはバニラの番号が無いので、既定どおり新しい NATO 名を取ります。
    /// </remarks>
    protected override string BuildMainName() => ForceNaming.Adopt(vanillaName);

    /// <inheritdoc />
    public override Faction Faction => Faction.FoundationStaff;
}

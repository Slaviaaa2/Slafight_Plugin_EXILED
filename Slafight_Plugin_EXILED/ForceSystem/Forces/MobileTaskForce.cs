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
    internal MobileTaskForce(string name, byte? unitId)
    {
        Name = name;
        UnitId = unitId;
    }

    /// <inheritdoc />
    public override string Name { get; }

    /// <inheritdoc />
    public override byte? UnitId { get; }

    /// <inheritdoc />
    public override Faction Faction => Faction.FoundationStaff;
}

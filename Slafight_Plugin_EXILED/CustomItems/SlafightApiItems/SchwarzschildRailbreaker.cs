#nullable enable
using System.Collections.Generic;
using Slafight_Plugin_EXILED.API.Features;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class SchwarzschildRailbreaker : CItemHybrid
{
    public override string DisplayName => "シュバルツシルト・レイルブレイカー";
    public override string Description => "クエィサァーとレールガンを切り替えられる複合武器";
    protected override string UniqueKey => "SchwarzschildRailbreaker";

    protected override List<CItemHybridMode> BuildSubModes()
        => [new(new SchwarzschildQuasar(), "クエィサァー"), new(new GunGoCRailgunFull(), "超電磁砲")];
}
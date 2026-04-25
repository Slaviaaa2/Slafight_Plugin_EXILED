using Exiled.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class MasterCard : CItemKeycard
{
    public override string DisplayName => "MasterCard";
    public override string Description => "ただのMasterCard。何にも使えない";

    protected override string UniqueKey => "MasterCard";

    protected override string KeycardLabel => "MasterCard";
    protected override Color32? KeycardLabelColor => new Color32(255, 255, 255, 255);

    protected override string KeycardName => "MasterCard";
    protected override Color32? TintColor => new Color32(0, 56, 170, 255);
    protected override Color32? KeycardPermissionsColor => new Color32(0, 0, 0, 255);

    protected override KeycardPermissions Permissions => KeycardPermissions.None;

    protected override string SerialNumber => "1234567890";

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => Color.blue;
}

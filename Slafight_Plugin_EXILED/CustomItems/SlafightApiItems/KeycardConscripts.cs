using Exiled.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class KeycardConscripts : CItemKeycard
{
    public override string DisplayName => "カオス 破壊カード";
    public override string Description =>
        "新入りや潜入工作員が持つ、アクセスデバイスの代わりとなるカード。";

    protected override string UniqueKey => "KeycardConscripts";

    protected override string KeycardLabel => "BREAKING CARD";
    protected override Color32? KeycardLabelColor => new Color32(255, 255, 255, 255);

    protected override string KeycardName => "Chaos Conscript";
    protected override Color32? TintColor => new Color32(0, 68, 0, 255);
    protected override Color32? KeycardPermissionsColor => new Color32(0, 0, 0, 255);

    protected override KeycardPermissions Permissions =>
        KeycardPermissions.ContainmentLevelOne |
        KeycardPermissions.ContainmentLevelTwo |
        KeycardPermissions.ArmoryLevelOne |
        KeycardPermissions.ArmoryLevelTwo |
        KeycardPermissions.Intercom |
        KeycardPermissions.Checkpoints |
        KeycardPermissions.ExitGates;

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => Color.green;
}

using Exiled.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class KeycardSecurityChief : CItemKeycard
{
    public override string DisplayName => "警備長キーカード";
    public override string Description => "警備隊を指揮したりする警備長が持つキーカード。";

    protected override string UniqueKey => "KeycardSecurityChief";
    protected override ItemType BaseItem => ItemType.KeycardCustomMetalCase;

    protected override string KeycardLabel => "警備主任キーカード";
    protected override Color32? KeycardLabelColor => new Color32(255, 255, 255, 255);

    protected override string KeycardName => "Chf. Security";
    protected override Color32? TintColor => new Color32(68, 68, 68, 255);
    protected override Color32? KeycardPermissionsColor => new Color32(0, 0, 0, 255);

    protected override KeycardPermissions Permissions =>
        KeycardPermissions.ContainmentLevelOne |
        KeycardPermissions.ArmoryLevelOne |
        KeycardPermissions.ArmoryLevelTwo |
        KeycardPermissions.Intercom |
        KeycardPermissions.Checkpoints |
        KeycardPermissions.ExitGates;

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => Color.green;
}

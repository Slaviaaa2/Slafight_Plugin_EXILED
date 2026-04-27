using Exiled.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class KeycardFifthist : CItemKeycard
{
    public override string DisplayName => "第五教会 第五デバイス";
    public override string Description =>
        "第五教会が目的を達成するために作られたアクセスデバイス。\n扉やゲートを第五することで施設に侵入する。";

    protected override string UniqueKey => "KeycardFifthist";
    protected override ItemType BaseItem => ItemType.KeycardCustomTaskForce;

    protected override string KeycardLabel => "第五教会 第五デバイス";
    protected override Color32? KeycardLabelColor => new Color32(255, 0, 250, 255);

    protected override string KeycardName => "Rsc. Fifth";
    protected override Color32? TintColor => new Color32(255, 0, 250, 255);
    protected override Color32? KeycardPermissionsColor => new Color32(255, 255, 255, 255);

    protected override KeycardPermissions Permissions =>
        KeycardPermissions.ContainmentLevelOne |
        KeycardPermissions.ContainmentLevelTwo |
        KeycardPermissions.ContainmentLevelThree |
        KeycardPermissions.ArmoryLevelOne |
        KeycardPermissions.ArmoryLevelTwo |
        KeycardPermissions.ArmoryLevelThree |
        KeycardPermissions.Checkpoints |
        KeycardPermissions.ExitGates |
        KeycardPermissions.Intercom;

    protected override byte   Rank         => 1;
    protected override string SerialNumber => "555555555555";

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => Color.magenta;
}

using Exiled.API.Enums;
using Slafight_Plugin_EXILED.API.Features;
using UnityEngine;

namespace Slafight_Plugin_EXILED.CustomItems.SlafightApiItems;

public class PlayingCard : CItemKeycard
{
    public override string DisplayName => "Playing Card";
    public override string Description => "ただのトランプ。何にも使えない";

    protected override string UniqueKey => "PlayingCard";

    protected override string KeycardLabel => "Playing Card";
    protected override Color32? KeycardLabelColor => new Color32(255, 255, 255, 255);

    protected override string KeycardName => "Role. Joker";
    protected override Color32? TintColor => new Color32(0, 0, 0, 255);
    protected override Color32? KeycardPermissionsColor => new Color32(0, 0, 0, 255);

    protected override KeycardPermissions Permissions => KeycardPermissions.None;

    protected override bool  PickupLightEnabled => true;
    protected override Color PickupLightColor   => Color.white;
}

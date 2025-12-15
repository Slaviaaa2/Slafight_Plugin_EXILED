using Exiled.CustomRoles.API.Features;

namespace Slafight_Plugin_EXILED.Abilities;

public class MagicMissileAbility : CustomAbility
{
    public override string Name { get; set; } = "MagicMissile";
    public override string Description { get; set; } = "Magic Missile";

    protected override void SubscribeEvents()
    {
        base.SubscribeEvents();
    }

    protected override void UnsubscribeEvents()
    {
        base.UnsubscribeEvents();
    }

    private void AbilityTrigger()
    {
        
    }
}
using Exiled.API.Features;
using LightContainmentZoneDecontamination;
using Slafight_Plugin_EXILED.API.Core.Features;

namespace Slafight_Plugin_EXILED.Handlers.Changes;

public class DecontaminationChanges : EventHandlerBase
{
    public override void OnServerRoundStarted()
    {
        if (DecontaminationController.Singleton.DecontaminationPhases.TryGet(0, out var phase))
        {
            Log.Debug($"Changing Decontamination Time...\nNow: {phase.TimeTrigger}");
            DecontaminationController.Singleton.DecontaminationPhases[0].TimeTrigger = 15f * 60f;
            Log.Debug($"Changed Decontamination Time!\nOld: {phase.TimeTrigger}\nNow: {DecontaminationController.Singleton.DecontaminationPhases[0].TimeTrigger}");
        }
        base.OnServerRoundStarted();
    }
}
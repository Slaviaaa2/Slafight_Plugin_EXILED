using ASS.Features.Collections;
using Exiled.Events.EventArgs.Player;
using UserSettings.ServerSpecific;
using ASS.Features.Settings;
using LabApi.Features.Wrappers;

namespace Slafight_Plugin_EXILED;

public class SS_Menu : AbstractMenu
{
    public static SS_Menu Instance { get; } = new SS_Menu();
    protected override ASSGroup Generate(Player owner)
    {
        return new ASSGroup([new ASSHeader(-14, "This came from an AbstractMenu")], viewers: p => p == owner);
    }
}
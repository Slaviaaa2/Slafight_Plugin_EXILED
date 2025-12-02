using ASS.Events.EventArgs;
using ASS.Events.Handlers;
using LabApi.Events.CustomHandlers;
using LabApi.Events.Handlers;
using PlayerRoles;
using Slafight_Plugin_EXILED;
using UnityEngine;

namespace ASS.Example.PlayerMenuExamples
{
    using System.Collections.Generic;
    using ASS.Features.Collections;
    using ASS.Features.Settings;
    using LabApi.Events.Arguments.PlayerEvents;
    using LabApi.Features.Wrappers;

    public class SS_Handler : CustomEventsHandler
    {
        public SS_Handler()
        {
            //PlayerEvents.Joined += OnJoined;
            //PlayerEvents.Left += OnLeft;
        }

        ~SS_Handler()
        {
            //PlayerEvents.Joined -= OnJoined;
            //PlayerEvents.Left -= OnLeft;
        }
        private static readonly Dictionary<Player, PlayerMenu> Menus = new();

        public static void OnJoined(PlayerJoinedEventArgs ev)
        {
            SS_Menu.Instance.Add(ev.Player);
            Menus[ev.Player] = new PlayerMenu(Generator, ev.Player);
        }

        public static void OnLeft(PlayerLeftEventArgs ev)
        {
            SS_Menu.Instance.Remove(ev.Player);
            if (Menus.TryGetValue(ev.Player, out PlayerMenu menu))
                menu.Destroy();
        }

        private static ASSGroup Generator(Player owner)
        {
            return new ASSGroup(
                [
                    new ASSHeader(-12, $"Welcome {owner.DisplayName}!"),
                    new ASSKeybind(-11,"人間との交流",KeyCode.LeftAlt)
                ],
                5, 
                p => p == owner);
        }
    }
}
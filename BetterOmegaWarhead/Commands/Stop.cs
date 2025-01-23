﻿namespace BetterOmegaWarhead.Commands
{
    using System;
    using CommandSystem;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Stop : ICommand
    {
        public string Command { get; } = "stopomegawarhead";

        public string[] Aliases { get; } = { "stopomega", "stopow", "sow" };

        public string Description { get; } = "Stops the Omega Warhead.";
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender.CheckPermission("omegawarhead"))
            {
                if (Plugin.Singleton.EventMethods.isOmegaActive())
                {
                    Plugin.Singleton.EventMethods.StopOmega();
                    Warhead.IsLocked = false;
                    Warhead.Status = WarheadStatus.NotArmed;

                    response = "Omega Warhead stopped.";
                    return false;
                }
                else
                {
                    response = "Omega Warhead is already stopped.";
                    return false;
                }
            }
            else
            {
                response = $"You need {Plugin.Singleton.Config.Permissions} permissions to use this command!";
                return true;
            }
        }
    }
}
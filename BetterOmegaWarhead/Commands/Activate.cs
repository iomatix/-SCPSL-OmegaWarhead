using CommandSystem;
using Exiled.Permissions.Extensions;
using System;

namespace BetterOmegaWarhead.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Activate : ICommand
    {
        public string Command { get; } = "activateomegawarhead";

        public string[] Aliases { get; } = { "activateomega", "activateow", "aow" };

        public string Description { get; } = "Activates the Omega Warhead.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender.CheckPermission("omegawarhead"))
            {
                if (!Plugin.Singleton.Methods.isOmegaActivated())
                {
                    Plugin.Singleton.Methods.ActivateOmegaWarhead();
                    response = "Omega Warhead activated.";
                    return false;
                }
                else
                {
                    response = "Omega Warhead is already activated.";
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
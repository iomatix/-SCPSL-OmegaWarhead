namespace BetterOmegaWarhead.Commands
{
    using System;
    using CommandSystem;
    using Exiled.Permissions.Extensions;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Activate : ICommand
    {
        public string Command { get; } = "activateomegawarhead";

        public string[] Aliases { get; } = { "activateomega", "activateow", "aow" };

        public string Description { get; } = "Activates the Omega Warhead.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            float timeToDetonation = Plugin.Singleton.Config.TimeToDetonation;
            if (!arguments.IsEmpty())
            {
                string arg = arguments.At(0).ToString();
                if (float.TryParse(arg, out float floatResult))
                {
                    timeToDetonation = floatResult;
                }
                else if (double.TryParse(arg, out double doubleResult))
                {
                    timeToDetonation = (float)doubleResult;
                }
                else if (int.TryParse(arg, out int intResult))
                {
                    timeToDetonation = (float)intResult;
                }
            }

            if (sender.CheckPermission("omegawarhead"))
            {
                if (!Plugin.Singleton.Methods.isOmegaActivated())
                {
                    Plugin.Singleton.Methods.ActivateOmegaWarhead(timeToDetonation);
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
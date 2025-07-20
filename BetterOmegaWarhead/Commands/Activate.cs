namespace BetterOmegaWarhead.Commands
{
    using BetterOmegaWarhead.Core.LoggingUtils;
    using CommandSystem;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using MEC;
    using System;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Activate : BaseCommand
    {
        public override string Command => "activateomegawarhead";
        public override string[] Aliases => new[] { "activateomega", "activateow", "aow" };
        public override string Description => "Activates the Omega Warhead.";
        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!HasPermission(sender, out response))
                return true;

            if (Plugin.Singleton.OmegaManager.IsOmegaActive)
            {
                response = "Omega Warhead is already active.";
                return true;
            }

            float detonationTime = Plugin.Singleton.Config.TimeToDetonation;
            try
            {
                if (arguments.Count >= 1)
                {
                    if (!float.TryParse(arguments.At(0), out float parsed))
                    {
                        response = $"Unable to parse detonation time '{arguments.At(0)}'. Please provide a valid number.";
                        return true;
                    }

                    if (parsed <= 0)
                    {
                        response = "Detonation time must be greater than 0 seconds.";
                        return true;
                    }

                    detonationTime = parsed;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warning($"Execute command arguments can not be parsed this time. Stack: {ex}");
            }

            Plugin.Singleton.WarheadMethods.StartSequence(detonationTime);

            response = $"Omega Warhead activated with detonation in {detonationTime}s.";
            return false;
        }
    };
}

namespace OmegaWarhead.Commands
{
    using System;
    using System.Collections.Generic;
    using CommandSystem;
    using OmegaWarhead.Core.LoggingUtils;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Activate : BaseCommand
    {
        public override string Command => "activateomegawarhead";
        public override string[] Aliases => new[] { "activateomega", "activateow", "aow" };
        public override string Description => "Activates the alternative Omega Warhead facility detonation protocol.";

        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!HasPermission(sender, out response))
            {
                return true;
            }

            if (Plugin.Singleton.OmegaManager.IsOmegaActive)
            {
                response = "Omega Warhead detonation sequence is already active.";
                return true;
            }

            float detonationTime = Plugin.Singleton.Config.TimeToDetonation;

            try
            {
                // Cast to IList<string> to enable native indexing support under .NET Framework 4.8
                IList<string> args = arguments;

                if (args.Count >= 1)
                {
                    if (!float.TryParse(args[0], out float parsed))
                    {
                        response = $"Unable to parse custom detonation time '{args[0]}'. Please provide a valid numerical value.";
                        return true;
                    }

                    if (parsed <= 0f)
                    {
                        response = "Detonation timeline constraints must be strictly greater than 0 seconds.";
                        return true;
                    }

                    detonationTime = parsed;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Warning(nameof(Activate), $"Failed to evaluate custom override arguments. Defaulting to baseline. Stack: {ex.Message}");
            }

            response = $"Omega Warhead authorization accepted. Initializing alternative nuclear payload activation sequence: {detonationTime}s.";
            Plugin.Singleton.WarheadMethods.StartSequence(detonationTime);
            return false;
        }
    }
}
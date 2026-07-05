using CommandSystem;
using System;

namespace OmegaWarhead.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Stop : BaseCommand
    {
        public override string Command => "stopomegawarhead";
        public override string[] Aliases => new[] { "stopomega", "stopow", "sow" };
        public override string Description => "Aborts the active Omega Warhead sequence and releases magnetic lockdown structures.";

        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!HasPermission(sender, out response))
                return false;

            if (!Plugin.Singleton.OmegaManager.IsOmegaActive)
            {
                response = "Execution Aborted: The Omega Warhead subsystem is currently resting in a passive state. Sequence cancellation rejected.";
                return false;
            }

            Plugin.Singleton.WarheadMethods?.StopSequence();

            response = "SUCCESS: Omega Warhead sequence terminated cleanly. Core parameters stabilized and returned to nominal status.";
            return true;
        }
    }
}
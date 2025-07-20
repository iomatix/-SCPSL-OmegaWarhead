namespace BetterOmegaWarhead.Commands
{
    using System;
    using CommandSystem;
    using Exiled.API.Enums;
    using Exiled.API.Features;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class Stop : BaseCommand
    {
        public override string Command => "stopomegawarhead";
        public override string[] Aliases => new[] { "stopomega", "stopow", "sow" };
        public override string Description => "Stops the Omega Warhead.";

        public override bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!HasPermission(sender, out response))
                return true;

            if (!Plugin.Singleton.OmegaManager.IsOmegaActive)
            {
                response = "Omega Warhead is already stopped.";
                return false;
            }

            Plugin.Singleton.WarheadMethods.StopSequence();
            Warhead.Status = WarheadStatus.NotArmed;
            Warhead.IsLocked = false;

            response = "Omega Warhead stopped.";
            return false;
        }
    }
}

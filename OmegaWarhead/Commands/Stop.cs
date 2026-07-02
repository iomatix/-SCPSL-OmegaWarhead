namespace OmegaWarhead.Commands
{
    using System;
    using CommandSystem;

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
            {
                return true;
            }

            if (!Plugin.Singleton.OmegaManager.IsOmegaActive)
            {
                response = "Omega Warhead sequence is currently passive. Abort operation rejected.";
                return true;
            }

            Plugin.Singleton.WarheadMethods.StopSequence();

            response = "Omega Warhead countdown successfully terminated. Subsystem status reverted to nominal.";
            return false;
        }
    }
}
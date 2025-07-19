namespace BetterOmegaWarhead.Commands
{
    using System;
    using CommandSystem;
    using Exiled.API.Enums;
    using Exiled.API.Features;

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

            if (Plugin.Singleton.EventMethods.isOmegaActive)
            {
                response = "Omega Warhead is already active.";
                return false;
            }

            float detonationTime = Plugin.Singleton.Config.TimeToDetonation;

            if (arguments.Count >= 1 && float.TryParse(arguments.At(0), out float parsed))
                detonationTime = parsed;

            Plugin.Singleton.EventMethods.ActivateOmegaWarhead(detonationTime);

            Warhead.Status = WarheadStatus.Armed;
            Warhead.DetonationTimer = detonationTime;
            Warhead.Controller.IsLocked = !Plugin.Singleton.Config.IsStopAllowed;

            response = $"Omega Warhead activated with detonation in {detonationTime}s.";
            return true;
        }
    };
}

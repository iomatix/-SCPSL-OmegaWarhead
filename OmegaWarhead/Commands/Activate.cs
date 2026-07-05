using CommandSystem;
using LabApi.Extensions;
using System;

namespace OmegaWarhead.Commands
{
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
                return false;

            if (Plugin.Singleton.OmegaManager.IsOmegaActive)
            {
                response = "Execution Aborted: The Omega Warhead detonation sequence is already active inside this round state.";
                return false;
            }

            float detonationTime = Plugin.Singleton.Config.TimeToDetonation;

            // Complete eradication of raw procedural parsing arrays, IList casting, and try-catch blocks.
            // Piping the incoming token array directly through our central fluent CommandExtensions layer.
            if (arguments.Count >= 1)
            {
                if (!arguments.TryGetFloat(0, out float parsed, minValue: 0.01f))
                {
                    response = $"Syntax Validation Failure: Unable to parse custom detonation time modifier '{arguments.At(0)}'. Expected non-negative float primitive.";
                    return false;
                }

                detonationTime = parsed;
            }

            response = $"SUCCESS: Omega Warhead authorization cleared. Initializing nuclear payload activation sequence: {detonationTime} seconds until site erase.";
            Plugin.Singleton.WarheadMethods?.StartSequence(detonationTime);
            return true;
        }
    }
}
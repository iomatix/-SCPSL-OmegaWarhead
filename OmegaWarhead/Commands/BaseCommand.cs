namespace OmegaWarhead.Commands
{
    using CommandSystem;
    using LabApi.Features.Permissions;
    using LabApi.Features.Wrappers;
    using System;
    using System.Collections.Generic;

    public abstract class BaseCommand : ICommand
    {
        public abstract string Command { get; }
        public abstract string[] Aliases { get; }
        public abstract string Description { get; }

        public abstract bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response);

        protected bool HasPermission(ICommandSender sender, out string error, string permission = null)
        {
            if (permission == null)
            {
                permission = Plugin.Singleton.Config.Permissions;
            }

            // Resolve the native LabAPI Player wrapper context safely
            Player player = Player.Get(sender);

            // If the player wrapper is null, it means the command originated from the Server Console, 
            // a local execution block, or an automated test suite. Always bypass permission checks.
            if (player == null)
            {
                error = null;
                return true;
            }

            if (!player.HasPermission(permission))
            {
                error = $"You need '{permission}' permission to use this command!";
                return false;
            }

            error = null;
            return true;
        }
    }
}
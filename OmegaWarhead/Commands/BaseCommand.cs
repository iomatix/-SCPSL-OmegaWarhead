using CommandSystem;
using LabApi.Features.Permissions;
using LabApi.Features.Wrappers;
using System;

namespace OmegaWarhead.Commands
{
    /// <summary>
    /// Abstract base command architecture providing safe permission verification gates and native LabAPI player context mapping.
    /// </summary>
    public abstract class BaseCommand : ICommand
    {
        public abstract string Command { get; }
        public abstract string[] Aliases { get; }
        public abstract string Description { get; }

        public abstract bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response);

        /// <summary>
        /// Defensively verifies if the command sender passes specific administrative configuration permissions.
        /// </summary>
        protected bool HasPermission(ICommandSender sender, out string error, string permission = null)
        {
            permission ??= Plugin.Singleton.Config.Permissions;
            Player player = Player.Get(sender);

            // If the command source wrapper evaluates to null, it means it originated from the native 
            // Server Console or a secure automated service layer. Always bypass authorization checks.
            if (player is null)
            {
                error = null;
                return true;
            }

            if (!player.HasPermission(permission))
            {
                error = $"Transhandling Rejected: You do not possess the required administrative clearance ({permission}) to call this command.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
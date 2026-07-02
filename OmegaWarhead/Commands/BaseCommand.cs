namespace OmegaWarhead.Commands
{
    using CommandSystem;
    using LabApi.Features.Permissions;
    using LabApi.Features.Wrappers;
    using System;

    /// <summary>
    /// Production-grade abstract base command mapping deterministic authorization and lifecycle context definitions.
    /// </summary>
    public abstract class BaseCommand : ICommand
    {
        public abstract string Command { get; }
        public abstract string[] Aliases { get; }
        public abstract string Description { get; }

        public abstract bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response);

        /// <summary>
        /// Validates executing sender identities against the string-based permission infrastructure.
        /// </summary>
        protected bool HasPermission(ICommandSender sender, out string error, string permission = null)
        {
            if (permission == null)
            {
                permission = Plugin.Singleton.Config.Permissions;
            }

            // Server console and automated local execution contexts always bypass authorization constraints
            if (sender is ServerConsoleSender || sender is LocalCallCommandSender)
            {
                error = null;
                return true;
            }

            // Resolve the native LabAPI Player wrapper context safely
            Player player = Player.Get(sender);
            if (player != null && !player.HasPermission(permission))
            {
                error = $"You need '{permission}' permission to use this command!";
                return false;
            }

            error = null;
            return true;
        }
    }
}
namespace OmegaWarhead.Commands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;

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

            if (!sender.CheckPermission(permission))
            {
                error = $"You need '{permission}' permission to use this command!";
                return false;
            }

            error = null;
            return true;
        }
    }
}

namespace Subclass.Commands
{
    using System;
    using CommandSystem;
    using Exiled.Permissions.Extensions;
    using RemoteAdmin;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Reload : ICommand
    {
        public string Command { get; } = "reloadsubclasses";

        public string[] Aliases { get; } = { "rsc" };

        public string Description { get; } = "Reloads all subclasses, takes effect the next round.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                if (!player.CheckPermission("sc.reload"))
                {
                    response = "You do not have the necessary permissions to run this command. Requires: sc.reload";
                    return false;
                }

                response = "Reloaded";

                Plugin.Instance.Classes = Plugin.Instance.GetClasses();
                TrackingAndMethods.RolesForClass.Clear();

                return true;
            }

            response = "Reloaded";
            Plugin.Instance.Classes = Plugin.Instance.GetClasses();
            TrackingAndMethods.RolesForClass.Clear();
            return true;
        }
    }
}
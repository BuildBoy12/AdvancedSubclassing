namespace Subclass.Commands
{
    using System;
    using CommandSystem;
    using Exiled.Permissions.Extensions;
    using RemoteAdmin;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Disable : ICommand
    {
        public string Command { get; } = "disablesubclass";

        public string[] Aliases { get; } = { "dsc" };

        public string Description { get; } = "Disables subclasses.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                if (!player.CheckPermission("sc.disable"))
                {
                    response = "You do not have the necessary permissions to run this command. Requires: sc.disable";
                    return false;
                }

                if (arguments.Count == 0)
                {
                    API.DisableAllClasses();
                }
                else
                {
                    if (!API.DisableClass(string.Join(" ", arguments)))
                    {
                        response = "Subclass not found";
                        return false;
                    }
                }

                response = "Disabled";
                return true;
            }

            if (arguments.Count == 0)
            {
                API.DisableAllClasses();
            }
            else
            {
                if (!API.DisableClass(string.Join(" ", arguments)))
                {
                    response = "Subclass not found";
                    return false;
                }
            }

            response = "Disabled";
            return true;
        }
    }
}
namespace Subclass.Commands
{
    using System;
    using CommandSystem;
    using Exiled.Permissions.Extensions;
    using RemoteAdmin;

    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Enable : ICommand
    {
        public string Command { get; } = "enablesubclass";

        public string[] Aliases { get; } = { "esc" };

        public string Description { get; } = "Enables subclasses.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                if (!player.CheckPermission("sc.enable"))
                {
                    response = "You do not have the necessary permissions to run this command. Requires: sc.enable";
                    return false;
                }

                if (arguments.Count == 0)
                {
                    API.EnableAllClasses();
                }
                else
                {
                    if (!API.EnableClass(string.Join(" ", arguments)))
                    {
                        response = "Subclass not found";
                        return false;
                    }
                }

                response = "Enabled";
                return true;
            }

            if (arguments.Count == 0)
            {
                API.EnableAllClasses();
            }
            else
            {
                if (!API.EnableClass(string.Join(" ", arguments)))
                {
                    response = "Subclass not found";
                    return false;
                }
            }

            response = "Enabled";
            return true;
        }
    }
}
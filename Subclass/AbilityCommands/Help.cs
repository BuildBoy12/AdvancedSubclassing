namespace Subclass.AbilityCommands
{
    using System;
    using System.Linq;
    using CommandSystem;
    using Exiled.API.Features;
    using RemoteAdmin;

    [CommandHandler(typeof(ClientCommandHandler))]
    public class Help : ICommand
    {
        public string Command { get; } = "schelp";

        public string[] Aliases { get; } = Array.Empty<string>();

        public string Description { get; } = "Get help on your current subclass, or any subclass";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(((PlayerCommandSender)sender).SenderId);
            if (arguments.Count == 0)
            {
                if (!TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player))
                {
                    response = Plugin.Instance.Config.HelpNoArgumentsProvided;
                    return true;
                }

                if (!TrackingAndMethods.PlayersWithSubclasses[player].StringOptions.ContainsKey("HelpMessage"))
                {
                    response = Plugin.Instance.Config.HelpNoHelpFound;
                    return true;
                }

                response = TrackingAndMethods.PlayersWithSubclasses[player].StringOptions["HelpMessage"];
                return true;
            }

            string sc = string.Join(" ", arguments).ToLower();
            SubClass c = Plugin.Instance.Classes.FirstOrDefault(s => s.Key.ToLower() == sc).Value;

            if (c == null)
            {
                response = Plugin.Instance.Config.HelpNoClassFound;
                return true;
            }

            if (!c.StringOptions.ContainsKey("HelpMessage"))
            {
                response = Plugin.Instance.Config.HelpNoHelpFound;
                return true;
            }

            response = c.StringOptions["HelpMessage"];
            return true;
        }
    }
}
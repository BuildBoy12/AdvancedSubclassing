namespace Subclass.AbilityCommands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using RemoteAdmin;

    [CommandHandler(typeof(ClientCommandHandler))]
    public class Revive : ICommand
    {
        public string Command { get; } = "revive";

        public string[] Aliases { get; } = Array.Empty<string>();

        public string Description { get; } = "Revive a player, if you have the revive ability.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(((PlayerCommandSender)sender).SenderId);
            Log.Debug($"Player {player.Nickname} is attempting to revive", Plugin.Instance.Config.Debug);
            if (TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) && TrackingAndMethods
                .PlayersWithSubclasses[player].Abilities.Contains(AbilityType.Revive))
            {
                SubClass subClass = TrackingAndMethods.PlayersWithSubclasses[player];
                if (!TrackingAndMethods.CanUseAbility(player, AbilityType.Revive, subClass))
                {
                    TrackingAndMethods.DisplayCantUseAbility(player, AbilityType.Revive, subClass, "revive");
                    response = string.Empty;
                    return false;
                }

                Utils.AttemptRevive(player, subClass);
            }

            response = string.Empty;
            return true;
        }
    }
}
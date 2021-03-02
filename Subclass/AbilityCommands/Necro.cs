namespace Subclass.AbilityCommands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using RemoteAdmin;

    [CommandHandler(typeof(ClientCommandHandler))]
    public class Necro : ICommand
    {
        public string Command { get; } = "necro";

        public string[] Aliases { get; } = Array.Empty<string>();

        public string Description { get; } = "Revive a player as a zombie, if you have the necromancy ability.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(((PlayerCommandSender)sender).SenderId);
            Log.Debug($"Player {player.Nickname} is attempting to necro", Plugin.Instance.Config.Debug);
            if (TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) && TrackingAndMethods
                .PlayersWithSubclasses[player].Abilities.Contains(AbilityType.Necromancy))
            {
                SubClass subClass = TrackingAndMethods.PlayersWithSubclasses[player];
                if (!TrackingAndMethods.CanUseAbility(player, AbilityType.Necromancy, subClass))
                {
                    TrackingAndMethods.DisplayCantUseAbility(player, AbilityType.Necromancy, subClass, "necro");
                    response = string.Empty;
                    return false;
                }

                Utils.AttemptRevive(player, subClass, true);
            }

            response = string.Empty;
            return true;
        }
    }
}
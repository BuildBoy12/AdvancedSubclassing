namespace Subclass.AbilityCommands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using RemoteAdmin;
    using UnityEngine;

    [CommandHandler(typeof(ClientCommandHandler))]
    public class Surge : ICommand
    {
        public string Command { get; } = "surge";

        public string[] Aliases { get; } = Array.Empty<string>();

        public string Description { get; } = "Start a local power surge, if you have the power surge ability.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(((PlayerCommandSender)sender).SenderId);
            if (!TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) || !TrackingAndMethods
                .PlayersWithSubclasses[player].Abilities.Contains(AbilityType.PowerSurge))
            {
                Log.Debug($"Player {player.Nickname} could not use power surge", Plugin.Instance.Config.Debug);
                response = string.Empty;
                return true;
            }

            SubClass subClass = TrackingAndMethods.PlayersWithSubclasses[player];
            if (TrackingAndMethods.OnCooldown(player, AbilityType.PowerSurge, subClass))
            {
                Log.Debug($"Player {player.Nickname} failed to power surge", Plugin.Instance.Config.Debug);
                TrackingAndMethods.DisplayCooldown(player, AbilityType.PowerSurge, subClass, "power surge", Time.time);
                response = string.Empty;
                return true;
            }

            if (!TrackingAndMethods.CanUseAbility(player, AbilityType.PowerSurge, subClass))
            {
                TrackingAndMethods.DisplayCantUseAbility(player, AbilityType.PowerSurge, subClass, "power surge");
                response = string.Empty;
                return true;
            }

            float radius = subClass.FloatOptions.ContainsKey("PowerSurgeRadius") ? subClass.FloatOptions["PowerSurgeRadius"] : 30f;
            foreach (Room room in Map.Rooms)
            {
                if (Vector3.Distance(room.Position, player.Position) <= radius)
                {
                    room.TurnOffLights(subClass.FloatOptions.ContainsKey("PowerSurgeDuration") ? subClass.FloatOptions["PowerSurgeDuration"] : 15f);
                }
            }

            TrackingAndMethods.AddCooldown(player, AbilityType.PowerSurge);
            TrackingAndMethods.UseAbility(player, AbilityType.PowerSurge, subClass);
            response = string.Empty;
            return true;
        }
    }
}
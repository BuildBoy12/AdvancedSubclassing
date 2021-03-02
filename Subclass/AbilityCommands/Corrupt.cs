namespace Subclass.AbilityCommands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommandSystem;
    using CustomPlayerEffects;
    using Exiled.API.Features;
    using RemoteAdmin;
    using UnityEngine;

    [CommandHandler(typeof(ClientCommandHandler))]
    public class Corrupt : ICommand
    {
        public string Command { get; } = "corrupt";

        public string[] Aliases { get; } = Array.Empty<string>();

        public string Description { get; } = "Slows players around you.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = string.Empty;
            Player player = Player.Get(((PlayerCommandSender)sender).SenderId);
            if (!TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) ||
                !TrackingAndMethods.PlayersWithSubclasses[player].Abilities.Contains(AbilityType.Corrupt))
            {
                Log.Debug($"Player {player.Nickname} could not use the corrupt command", Plugin.Instance.Config.Debug);
                response = string.Empty;
                return true;
            }

            SubClass subClass = TrackingAndMethods.PlayersWithSubclasses[player];
            if (TrackingAndMethods.OnCooldown(player, AbilityType.Corrupt, subClass))
            {
                Log.Debug($"Player {player.Nickname} failed to use the corrupt command", Plugin.Instance.Config.Debug);
                TrackingAndMethods.DisplayCooldown(player, AbilityType.Corrupt, subClass, "corrupt", Time.time);
                response = string.Empty;
                return true;
            }

            if (!TrackingAndMethods.CanUseAbility(player, AbilityType.Corrupt, subClass))
            {
                TrackingAndMethods.DisplayCantUseAbility(player, AbilityType.Corrupt, subClass, "corrupt");
                response = string.Empty;
                return true;
            }

            Collider[] colliders = Physics.OverlapSphere(player.Position, subClass.FloatOptions.ContainsKey("CorruptRange") ? subClass.FloatOptions["CorruptRange"] : 10f);
            List<Player> players = colliders.Select(c => Player.Get(c.gameObject)).Distinct().ToList();
            if (players.Any())
            {
                TrackingAndMethods.UseAbility(player, AbilityType.Corrupt, subClass);
                TrackingAndMethods.AddCooldown(player, AbilityType.Corrupt);
                foreach (Player target in players)
                {
                    if (target == null || target.Id == player.Id || player.Side == target.Side ||
                        (player.Team == Team.SCP && target.Team == Team.TUT))
                    {
                        continue;
                    }

                    target.EnableEffect<SinkHole>(subClass.FloatOptions.ContainsKey("CorruptDuration") ? subClass.FloatOptions["CorruptDuration"] : 3);
                }
            }

            return true;
        }
    }
}
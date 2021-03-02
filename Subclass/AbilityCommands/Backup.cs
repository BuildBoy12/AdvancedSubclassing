namespace Subclass.AbilityCommands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommandSystem;
    using Exiled.API.Features;
    using RemoteAdmin;
    using UnityEngine;

    [CommandHandler(typeof(ClientCommandHandler))]
    public class Backup : ICommand
    {
        private static readonly System.Random Random = new System.Random();

        public string Command { get; } = "backup";

        public string[] Aliases { get; } = Array.Empty<string>();

        public string Description { get; } = "Call for backup, if you have the backup ability.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(((PlayerCommandSender)sender).SenderId);
            if ((player.Team != Team.MTF && player.Team != Team.CHI) ||
                !TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) ||
                !TrackingAndMethods.PlayersWithSubclasses[player].Abilities.Contains(AbilityType.BackupCommand))
            {
                Log.Debug($"Player {player.Nickname} could not use the backup command", Plugin.Instance.Config.Debug);
                response = string.Empty;
                return true;
            }

            SubClass subClass = TrackingAndMethods.PlayersWithSubclasses[player];
            if (TrackingAndMethods.OnCooldown(player, AbilityType.BackupCommand, subClass))
            {
                Log.Debug($"Player {player.Nickname} failed to use the backup command", Plugin.Instance.Config.Debug);
                TrackingAndMethods.DisplayCooldown(player, AbilityType.BackupCommand, subClass, "backup", Time.time);
                response = string.Empty;
                return true;
            }

            if (!TrackingAndMethods.CanUseAbility(player, AbilityType.BackupCommand, subClass))
            {
                TrackingAndMethods.DisplayCantUseAbility(player, AbilityType.BackupCommand, subClass, "backup");
                response = string.Empty;
                return true;
            }

            int min = subClass.IntOptions.ContainsKey("BackupMinSpawn") ? subClass.IntOptions["BackupMinSpawn"] : 3;
            int max = subClass.IntOptions.ContainsKey("BackupMaxSpawn") ? subClass.IntOptions["BackupMaxSpawn"] : 7;

            List<Player> spectators = Player.List.Where(p => p.Role == RoleType.Spectator).ToList();

            if (spectators.Count == 0)
            {
                player.Broadcast(2, Plugin.Instance.Config.NoAvailableSpectators);
                response = string.Empty;
                return true;
            }

            TrackingAndMethods.UseAbility(player, AbilityType.BackupCommand, subClass);
            TrackingAndMethods.AddCooldown(player, AbilityType.BackupCommand);

            int spawns = Mathf.Clamp((int)(Random.NextDouble() * ((max - min) + 1)) + min, 0, spectators.Count);

            bool isMtf = player.Team == Team.MTF;

            int commanders = 1;
            int lieutenants = 0;

            if (isMtf)
            {
                lieutenants = Mathf.Clamp(spawns - commanders, 0, 3);
            }

            for (int i = 0; i < spawns; i++)
            {
                int index = Random.Next(spectators.Count);
                Player p = spectators[index];
                spectators.RemoveAt(index);
                if (!isMtf)
                {
                    p.SetRole(RoleType.ChaosInsurgency);
                }
                else
                {
                    if (commanders > 0)
                    {
                        p.SetRole(RoleType.NtfCommander);
                        commanders--;
                    }
                    else if (lieutenants > 0)
                    {
                        p.SetRole(RoleType.NtfLieutenant);
                        lieutenants--;
                    }
                    else
                    {
                        p.SetRole(RoleType.NtfCadet);
                    }
                }
            }

            response = string.Empty;
            return true;
        }
    }
}
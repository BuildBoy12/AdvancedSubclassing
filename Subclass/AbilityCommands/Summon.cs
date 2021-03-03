// <copyright file="Summon.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.AbilityCommands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CommandSystem;
    using Exiled.API.Features;
    using RemoteAdmin;
    using UnityEngine;

    /// <summary>
    /// The summon ability command.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Summon : ICommand
    {
        private static readonly System.Random Random = new System.Random();

        /// <inheritdoc/>
        public string Command { get; } = "summon";

        /// <inheritdoc/>
        public string[] Aliases { get; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string Description { get; } = "Summon zombies from the spectators, if you have the summon ability.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(((PlayerCommandSender)sender).SenderId);
            if (!TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) || !TrackingAndMethods
                .PlayersWithSubclasses[player].Abilities.Contains(AbilityType.Summon))
            {
                Log.Debug($"Player {player.Nickname} could not use summon", Plugin.Instance.Config.Debug);
                response = string.Empty;
                return true;
            }

            SubClass subClass = TrackingAndMethods.PlayersWithSubclasses[player];
            if (TrackingAndMethods.OnCooldown(player, AbilityType.Summon, subClass))
            {
                Log.Debug($"Player {player.Nickname} failed to summon", Plugin.Instance.Config.Debug);
                TrackingAndMethods.DisplayCooldown(player, AbilityType.Summon, subClass, "summon", Time.time);
                response = string.Empty;
                return true;
            }

            if (!TrackingAndMethods.CanUseAbility(player, AbilityType.Summon, subClass))
            {
                TrackingAndMethods.DisplayCantUseAbility(player, AbilityType.Summon, subClass, "summon");
                response = string.Empty;
                return true;
            }

            int min = subClass.IntOptions.ContainsKey("SummonMinSpawn") ? subClass.IntOptions["SummonMinSpawn"] : 1;
            int max = subClass.IntOptions.ContainsKey("SummonMaxSpawn") ? subClass.IntOptions["SummonMaxSpawn"] : 5;

            List<Player> spectators = Player.List.Where(p => p.Role == RoleType.Spectator).ToList();

            if (spectators.Count == 0)
            {
                player.Broadcast(2, Plugin.Instance.Config.NoAvailableSpectators);
                response = string.Empty;
                return true;
            }

            TrackingAndMethods.UseAbility(player, AbilityType.Summon, subClass);
            TrackingAndMethods.AddCooldown(player, AbilityType.Summon);

            int spawns = Mathf.Clamp((int)(Random.NextDouble() * ((max - min) + 1)) + min, 0, spectators.Count);

            for (int i = 0; i < spawns; i++)
            {
                int index = Random.Next(spectators.Count);
                Player p = spectators[index];
                spectators.RemoveAt(index);
                p.Role = RoleType.Scp0492;
                p.IsFriendlyFireEnabled = true;
                p.Position = player.Position + new Vector3(Random.Next(-2, 2), 1, Random.Next(-2, 2));
                TrackingAndMethods.AddZombie(player, p);
            }

            response = string.Empty;
            return true;
        }
    }
}
// <copyright file="Grenade.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.AbilityCommands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using RemoteAdmin;
    using UnityEngine;

    /// <summary>
    /// The grenade ability command.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Grenade : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "grenade";

        /// <inheritdoc/>
        public string[] Aliases { get; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string Description { get; } = "Spawn a frag grenade, if you have the grenade ability.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(((PlayerCommandSender)sender).SenderId);
            if (TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) && TrackingAndMethods
                .PlayersWithSubclasses[player].Abilities.Contains(AbilityType.GrenadeOnCommand))
            {
                SubClass subClass = TrackingAndMethods.PlayersWithSubclasses[player];
                if (!TrackingAndMethods.CanUseAbility(player, AbilityType.GrenadeOnCommand, subClass))
                {
                    TrackingAndMethods.DisplayCantUseAbility(player, AbilityType.GrenadeOnCommand, subClass, "grenade on command");
                    response = string.Empty;
                    return true;
                }

                if (TrackingAndMethods.OnCooldown(player, AbilityType.GrenadeOnCommand, subClass))
                {
                    Log.Debug($"Player {player.Nickname} failed to grenade on command", Plugin.Instance.Config.Debug);
                    TrackingAndMethods.DisplayCooldown(player, AbilityType.GrenadeOnCommand, subClass, "grenade", Time.time);
                    response = string.Empty;
                    return true;
                }

                Utils.SpawnGrenade(ItemType.GrenadeFrag, player, subClass);
                TrackingAndMethods.AddCooldown(player, AbilityType.GrenadeOnCommand);
                TrackingAndMethods.UseAbility(player, AbilityType.GrenadeOnCommand, subClass);
                Log.Debug($"Player {player.Nickname} successfully used grenade on commad", Plugin.Instance.Config.Debug);
            }
            else
            {
                Log.Debug($"Player {player.Nickname} could not grenade on command", Plugin.Instance.Config.Debug);
            }

            response = string.Empty;
            return true;
        }
    }
}
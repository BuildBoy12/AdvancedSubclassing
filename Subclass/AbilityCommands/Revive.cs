﻿// <copyright file="Revive.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.AbilityCommands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using RemoteAdmin;

    /// <summary>
    /// The revive ability command.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Revive : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "revive";

        /// <inheritdoc/>
        public string[] Aliases { get; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string Description { get; } = "Revive a player, if you have the revive ability.";

        /// <inheritdoc/>
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
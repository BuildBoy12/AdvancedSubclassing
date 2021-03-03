// <copyright file="Vent.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.AbilityCommands
{
    using System;
    using CommandSystem;
    using CustomPlayerEffects;
    using Exiled.API.Features;
    using MEC;
    using RemoteAdmin;
    using UnityEngine;

    /// <summary>
    /// The vent ability command.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Vent : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "vent";

        /// <inheritdoc/>
        public string[] Aliases { get; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string Description { get; } = "Begin venting, if you have the vent ability.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(((PlayerCommandSender)sender).SenderId);
            Scp268 scp268 = player.ReferenceHub.playerEffectsController.GetEffect<Scp268>();
            if (TrackingAndMethods.PlayersVenting.Contains(player) && scp268 != null && scp268.Enabled)
            {
                TrackingAndMethods.PlayersVenting.Remove(player);
                scp268.ServerDisable();
                response = string.Empty;
                return true;
            }

            if (!TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) || !TrackingAndMethods
                .PlayersWithSubclasses[player].Abilities.Contains(AbilityType.Vent))
            {
                Log.Debug($"Player {player.Nickname} could not vent", Plugin.Instance.Config.Debug);
                response = string.Empty;
                return true;
            }

            SubClass subClass = TrackingAndMethods.PlayersWithSubclasses[player];
            if (TrackingAndMethods.OnCooldown(player, AbilityType.Vent, subClass))
            {
                Log.Debug($"Player {player.Nickname} failed to vent", Plugin.Instance.Config.Debug);
                TrackingAndMethods.DisplayCooldown(player, AbilityType.Vent, subClass, "vent", Time.time);
                response = string.Empty;
                return true;
            }

            if (!TrackingAndMethods.CanUseAbility(player, AbilityType.Vent, subClass))
            {
                TrackingAndMethods.DisplayCantUseAbility(player, AbilityType.Vent, subClass, "vent");
                response = string.Empty;
                return true;
            }

            if (scp268 != null)
            {
                if (scp268.Enabled)
                {
                    Log.Debug($"Player {player.Nickname} failed to vent", Plugin.Instance.Config.Debug);
                    player.Broadcast(3, Plugin.Instance.Config.AlreadyInvisibleMessage);
                    response = string.Empty;
                    return true;
                }

                player.ReferenceHub.playerEffectsController.EnableEffect<Scp268>();
                TrackingAndMethods.PlayersInvisibleByCommand.Add(player);
                TrackingAndMethods.PlayersVenting.Add(player);
                Timing.CallDelayed(
                    subClass.FloatOptions.ContainsKey("VentDuration") ? subClass.FloatOptions["VentDuration"] : 15f,
                    () =>
                    {
                        if (TrackingAndMethods.PlayersVenting.Contains(player))
                        {
                            TrackingAndMethods.PlayersVenting.Remove(player);
                        }

                        if (TrackingAndMethods.PlayersInvisibleByCommand.Contains(player))
                        {
                            TrackingAndMethods.PlayersInvisibleByCommand.Remove(player);
                        }

                        if (scp268.Enabled)
                        {
                            player.ReferenceHub.playerEffectsController.DisableEffect<Scp268>();
                        }
                    });

                TrackingAndMethods.AddCooldown(player, AbilityType.Vent);
                TrackingAndMethods.UseAbility(player, AbilityType.Vent, subClass);
            }

            response = string.Empty;
            return true;
        }
    }
}
// <copyright file="Invisibility.cs" company="PlaceholderCompany">
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
    /// The invisibility ability command.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Invisibility : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "invis";

        /// <inheritdoc/>
        public string[] Aliases { get; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string Description { get; } = "Go invisible, if you have the invisibility ability.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(((PlayerCommandSender)sender).SenderId);
            if (!TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) || !TrackingAndMethods.PlayersWithSubclasses[player].Abilities.Contains(AbilityType.InvisibleOnCommand))
            {
                Log.Debug($"Player {player.Nickname} could not go invisible on command", Plugin.Instance.Config.Debug);
                response = string.Empty;
                return true;
            }

            Scp268 scp268 = player.ReferenceHub.playerEffectsController.GetEffect<Scp268>();
            if (scp268 != null)
            {
                SubClass subClass = TrackingAndMethods.PlayersWithSubclasses[player];
                if (!TrackingAndMethods.CanUseAbility(player, AbilityType.InvisibleOnCommand, subClass))
                {
                    TrackingAndMethods.DisplayCantUseAbility(player, AbilityType.InvisibleOnCommand, subClass, "invisible on command");
                    response = string.Empty;
                    return true;
                }

                if (scp268.Enabled)
                {
                    Log.Debug($"Player {player.Nickname} failed to go invisible on command", Plugin.Instance.Config.Debug);
                    player.Broadcast(3, Plugin.Instance.Config.AlreadyInvisibleMessage);
                    response = string.Empty;
                    return true;
                }

                if (TrackingAndMethods.OnCooldown(player, AbilityType.InvisibleOnCommand, subClass))
                {
                    Log.Debug($"Player {player.Nickname} failed to go invisible on command", Plugin.Instance.Config.Debug);
                    TrackingAndMethods.DisplayCooldown(player, AbilityType.InvisibleOnCommand, subClass, "invisible", Time.time);
                    response = string.Empty;
                    return true;
                }

                player.ReferenceHub.playerEffectsController.EnableEffect<Scp268>();
                TrackingAndMethods.PlayersInvisibleByCommand.Add(player);
                Timing.CallDelayed(
                    subClass.FloatOptions.ContainsKey("InvisibleOnCommandDuration")
                        ? subClass.FloatOptions["InvisibleOnCommandDuration"]
                        : 30f, () =>
                    {
                        if (TrackingAndMethods.PlayersInvisibleByCommand.Contains(player))
                        {
                            TrackingAndMethods.PlayersInvisibleByCommand.Remove(player);
                        }

                        if (scp268.Enabled)
                        {
                            player.ReferenceHub.playerEffectsController.DisableEffect<Scp268>();
                        }
                    });

                TrackingAndMethods.AddCooldown(player, AbilityType.InvisibleOnCommand);
                TrackingAndMethods.UseAbility(player, AbilityType.InvisibleOnCommand, subClass);
            }

            response = string.Empty;
            return true;
        }
    }
}
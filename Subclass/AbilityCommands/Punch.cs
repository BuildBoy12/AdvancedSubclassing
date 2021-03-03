// <copyright file="Punch.cs" company="PlaceholderCompany">
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
    /// The punch ability command.
    /// </summary>
    [CommandHandler(typeof(ClientCommandHandler))]
    public class Punch : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "punch";

        /// <inheritdoc/>
        public string[] Aliases { get; } = Array.Empty<string>();

        /// <inheritdoc/>
        public string Description { get; } = "Punch the player you're looking at.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = string.Empty;
            Player player = Player.Get(((PlayerCommandSender)sender).SenderId);
            if (!TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) ||
                !TrackingAndMethods.PlayersWithSubclasses[player].Abilities.Contains(AbilityType.Punch) ||
                player.IsCuffed)
            {
                Log.Debug($"Player {player.Nickname} could not use the punch command", Plugin.Instance.Config.Debug);
                response = string.Empty;
                return true;
            }

            SubClass subClass = TrackingAndMethods.PlayersWithSubclasses[player];

            if (!subClass.Abilities.Contains(AbilityType.InfiniteSprint))
            {
                if ((player.Stamina.RemainingStamina * 100) - (subClass.FloatOptions.ContainsKey("PunchStaminaUse")
                    ? subClass.FloatOptions["PunchStaminaUse"]
                    : 10) <= 0)
                {
                    Log.Debug($"Player {player.Nickname} failed to use the punch command", Plugin.Instance.Config.Debug);
                    player.Broadcast(5, Plugin.Instance.Config.OutOfStaminaMessage);
                    return true;
                }

                player.Stamina.RemainingStamina = Mathf.Clamp(player.Stamina.RemainingStamina - (subClass.FloatOptions.ContainsKey("PunchStaminaUse") ? subClass.FloatOptions["PunchStaminaUse"] / 100 : .1f), 0, 1);
                player.Stamina._regenerationTimer = 0;
            }

            if (!TrackingAndMethods.CanUseAbility(player, AbilityType.Punch, subClass))
            {
                TrackingAndMethods.DisplayCantUseAbility(player, AbilityType.Punch, subClass, "punch");
                response = string.Empty;
                return true;
            }

            if (TrackingAndMethods.OnCooldown(player, AbilityType.Punch, subClass))
            {
                Log.Debug($"Player {player.Nickname} failed to use punch", Plugin.Instance.Config.Debug);
                TrackingAndMethods.DisplayCooldown(player, AbilityType.Punch, subClass, "punch", Time.time);
                response = string.Empty;
                return true;
            }

            if (Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, subClass.FloatOptions.ContainsKey("PunchRange") ? subClass.FloatOptions["PunchRange"] : 1.3f))
            {
                Player target = Player.Get(hit.collider.gameObject) ??
                                Player.Get(hit.collider.GetComponentInParent<ReferenceHub>());
                if (target == null || target.Id == player.Id)
                {
                    return true;
                }

                TrackingAndMethods.AddCooldown(player, AbilityType.Punch);
                TrackingAndMethods.UseAbility(player, AbilityType.Punch, subClass);
                target.Hurt(subClass.FloatOptions["PunchDamage"], null, player.Nickname, player.Id);
            }

            return true;
        }
    }
}
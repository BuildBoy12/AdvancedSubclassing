namespace Subclass.AbilityCommands
{
    using System;
    using CommandSystem;
    using CustomPlayerEffects;
    using Exiled.API.Features;
    using RemoteAdmin;
    using UnityEngine;

    [CommandHandler(typeof(ClientCommandHandler))]
    public class Stun : ICommand
    {
        public string Command { get; } = "stun";

        public string[] Aliases { get; } = Array.Empty<string>();

        public string Description { get; } = "Stun the player you're looking at.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = string.Empty;
            Player player = Player.Get(((PlayerCommandSender)sender).SenderId);
            if (!TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) ||
                !TrackingAndMethods.PlayersWithSubclasses[player].Abilities.Contains(AbilityType.Stun) ||
                player.IsCuffed)
            {
                Log.Debug($"Player {player.Nickname} could not use the punch command", Plugin.Instance.Config.Debug);
                response = string.Empty;
                return true;
            }

            SubClass subClass = TrackingAndMethods.PlayersWithSubclasses[player];
            if (TrackingAndMethods.OnCooldown(player, AbilityType.Stun, subClass))
            {
                Log.Debug($"Player {player.Nickname} failed to use the stun command", Plugin.Instance.Config.Debug);
                TrackingAndMethods.DisplayCooldown(player, AbilityType.Stun, subClass, "stun", Time.time);
                response = string.Empty;
                return true;
            }

            if (!TrackingAndMethods.CanUseAbility(player, AbilityType.Stun, subClass))
            {
                TrackingAndMethods.DisplayCantUseAbility(player, AbilityType.Stun, subClass, "stun");
                response = string.Empty;
                return true;
            }

            if (Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, subClass.FloatOptions.ContainsKey("StunRange") ? subClass.FloatOptions["StunRange"] : 1.3f))
            {
                Player target = Player.Get(hit.collider.gameObject) ??
                                Player.Get(hit.collider.GetComponentInParent<ReferenceHub>());
                if (target == null || target.Id == player.Id || player.Side == target.Side || (player.Team == Team.SCP && target.Team == Team.TUT))
                {
                    return true;
                }

                TrackingAndMethods.UseAbility(player, AbilityType.Stun, subClass);
                TrackingAndMethods.AddCooldown(player, AbilityType.Stun);
                target.Hurt(subClass.FloatOptions["StunDamage"], null, player.Nickname, player.Id);
                target.EnableEffect<Ensnared>(subClass.FloatOptions.ContainsKey("StunDuration") ? subClass.FloatOptions["StunDuration"] : 3);
                target.EnableEffect<Blinded>(subClass.FloatOptions.ContainsKey("StunDuration") ? subClass.FloatOptions["StunDuration"] : 3);
            }

            return true;
        }
    }
}
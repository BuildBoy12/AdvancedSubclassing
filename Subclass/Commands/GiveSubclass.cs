// <copyright file="GiveSubclass.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Commands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.Permissions.Extensions;
    using RemoteAdmin;

    /// <summary>
    /// Command to grant a user a subclass.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class GiveSubclass : ICommand
    {
        private static readonly Random Random = new Random();

        /// <inheritdoc/>
        public string Command { get; } = "subclass";

        /// <inheritdoc/>
        public string[] Aliases { get; } = { "gsc" };

        /// <inheritdoc/>
        public string Description { get; } = "Gives a player a subclass";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                Player p = Player.Get(player.SenderId);
                if (!p.CheckPermission("sc.giveclass"))
                {
                    response = "You do not have the necessary permissions to run this command. Requires: sc.giveclass";
                    return false;
                }

                if (arguments.Count == 0)
                {
                    response = "Command syntax should be subclass (player id/all) [class].";
                    return false;
                }

                try
                {
                    if (Player.Get(int.Parse(arguments.Array[arguments.Offset])) != null)
                    {
                        Player player1 = Player.Get(int.Parse(arguments.Array[arguments.Offset]));
                        if (!Plugin.Instance.Classes.ContainsKey(string.Join(" ", arguments.Array.Segment(arguments.Offset + 1))))
                        {
                            response = "Class not found.";
                            return false;
                        }

                        SubClass sc =
                            Plugin.Instance.Classes[
                                string.Join(" ", arguments.Array.Segment(arguments.Offset + 1))];
                        if (!sc.AffectsRoles.Contains(player1.Role))
                        {
                            player1.SetRole(sc.AffectsRoles[Random.Next(sc.AffectsRoles.Count)], true);
                        }

                        TrackingAndMethods.RemoveAndAddRoles(player1, true);
                        TrackingAndMethods.AddClass(player1, sc);
                        response = "Success.";
                        return true;
                    }

                    if (Plugin.Instance.Classes.ContainsKey(string.Join(" ", arguments.Array.Segment(arguments.Offset))))
                    {
                        SubClass sc =
                            Plugin.Instance.Classes[string.Join(" ", arguments.Array.Segment(arguments.Offset))];
                        if (!sc.AffectsRoles.Contains(p.Role))
                        {
                            p.SetRole(sc.AffectsRoles[Random.Next(sc.AffectsRoles.Count)], true);
                        }

                        TrackingAndMethods.RemoveAndAddRoles(p, true);
                        TrackingAndMethods.AddClass(p, sc);
                        response = "Success.";
                        return true;
                    }

                    response = "Player not found.";
                    return false;
                }
                catch
                {
                    if (arguments.Array[arguments.Offset].ToLower() != "all")
                    {
                        if (!Plugin.Instance.Classes.ContainsKey(string.Join(" ", arguments.Array.Segment(arguments.Offset))))
                        {
                            response = "Class not found.";
                            return false;
                        }

                        SubClass sc =
                            Plugin.Instance.Classes[string.Join(" ", arguments.Array.Segment(arguments.Offset))];
                        if (!sc.AffectsRoles.Contains(p.Role))
                        {
                            p.SetRole(sc.AffectsRoles[Random.Next(sc.AffectsRoles.Count)], true);
                        }

                        TrackingAndMethods.RemoveAndAddRoles(p, true);
                        TrackingAndMethods.AddClass(p, sc);
                        response = "Success.";
                        return true;
                    }

                    if (!Plugin.Instance.Classes.ContainsKey(string.Join(" ", arguments.Array.Segment(arguments.Offset + 1))))
                    {
                        response = "Class not found.";
                        return false;
                    }

                    {
                        SubClass sc =
                            Plugin.Instance.Classes[
                                string.Join(" ", arguments.Array.Segment(arguments.Offset + 1))];
                        foreach (Player p1 in Player.List)
                        {
                            if (p1.Role == RoleType.Spectator)
                            {
                                continue;
                            }

                            if (!sc.AffectsRoles.Contains(p1.Role))
                            {
                                p1.SetRole(sc.AffectsRoles[Random.Next(sc.AffectsRoles.Count)], true);
                            }

                            TrackingAndMethods.RemoveAndAddRoles(p1, true);
                            TrackingAndMethods.AddClass(p1, sc);
                        }

                        response = "Success.";
                        return true;
                    }
                }
            }

            response = string.Empty;
            return false;
        }
    }
}
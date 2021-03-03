// <copyright file="Reload.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Commands
{
    using System;
    using CommandSystem;
    using Exiled.Permissions.Extensions;
    using RemoteAdmin;

    /// <summary>
    /// Command to reload all subclasses.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Reload : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "reloadsubclasses";

        /// <inheritdoc/>
        public string[] Aliases { get; } = { "rsc" };

        /// <inheritdoc/>
        public string Description { get; } = "Reloads all subclasses, takes effect the next round.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                if (!player.CheckPermission("sc.reload"))
                {
                    response = "You do not have the necessary permissions to run this command. Requires: sc.reload";
                    return false;
                }

                response = "Reloaded";

                Plugin.Instance.Classes = Plugin.Instance.GetClasses();
                TrackingAndMethods.RolesForClass.Clear();

                return true;
            }

            response = "Reloaded";
            Plugin.Instance.Classes = Plugin.Instance.GetClasses();
            TrackingAndMethods.RolesForClass.Clear();
            return true;
        }
    }
}
// <copyright file="Disable.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Commands
{
    using System;
    using CommandSystem;
    using Exiled.Permissions.Extensions;
    using RemoteAdmin;

    /// <summary>
    /// Command to disable subclasses.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Disable : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "disablesubclass";

        /// <inheritdoc/>
        public string[] Aliases { get; } = { "dsc" };

        /// <inheritdoc/>
        public string Description { get; } = "Disables subclasses.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                if (!player.CheckPermission("sc.disable"))
                {
                    response = "You do not have the necessary permissions to run this command. Requires: sc.disable";
                    return false;
                }

                if (arguments.Count == 0)
                {
                    API.DisableAllClasses();
                }
                else
                {
                    if (!API.DisableClass(string.Join(" ", arguments)))
                    {
                        response = "Subclass not found";
                        return false;
                    }
                }

                response = "Disabled";
                return true;
            }

            if (arguments.Count == 0)
            {
                API.DisableAllClasses();
            }
            else
            {
                if (!API.DisableClass(string.Join(" ", arguments)))
                {
                    response = "Subclass not found";
                    return false;
                }
            }

            response = "Disabled";
            return true;
        }
    }
}
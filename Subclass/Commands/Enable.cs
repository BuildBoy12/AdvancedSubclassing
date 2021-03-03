// <copyright file="Enable.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Commands
{
    using System;
    using CommandSystem;
    using Exiled.Permissions.Extensions;
    using RemoteAdmin;

    /// <summary>
    /// Command to enable subclasses.
    /// </summary>
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class Enable : ICommand
    {
        /// <inheritdoc/>
        public string Command { get; } = "enablesubclass";

        /// <inheritdoc/>
        public string[] Aliases { get; } = { "esc" };

        /// <inheritdoc/>
        public string Description { get; } = "Enables subclasses.";

        /// <inheritdoc/>
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender player)
            {
                if (!player.CheckPermission("sc.enable"))
                {
                    response = "You do not have the necessary permissions to run this command. Requires: sc.enable";
                    return false;
                }

                if (arguments.Count == 0)
                {
                    API.EnableAllClasses();
                }
                else
                {
                    if (!API.EnableClass(string.Join(" ", arguments)))
                    {
                        response = "Subclass not found";
                        return false;
                    }
                }

                response = "Enabled";
                return true;
            }

            if (arguments.Count == 0)
            {
                API.EnableAllClasses();
            }
            else
            {
                if (!API.EnableClass(string.Join(" ", arguments)))
                {
                    response = "Subclass not found";
                    return false;
                }
            }

            response = "Enabled";
            return true;
        }
    }
}
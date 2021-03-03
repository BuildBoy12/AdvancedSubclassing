// <copyright file="ReceivingSubclassEventArgs.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Events.EventArgs
{
#pragma warning disable SA1101

    using Exiled.API.Features;

    /// <summary>
    /// Contains the information before a player receives a subclass.
    /// </summary>
    public class ReceivingSubclassEventArgs : System.EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivingSubclassEventArgs"/> class.
        /// </summary>
        /// <param name="player">The <see cref="Exiled.API.Features.Player"/> that received the subclass</param>
        /// <param name="subClass">The <see cref="SubClass"/> the player got.</param>
        /// <param name="isAllowed">Whether or not this player should receive the subclass.</param>
        public ReceivingSubclassEventArgs(Player player, SubClass subClass, bool isAllowed = true)
        {
            Player = player;
            Subclass = subClass;
            IsAllowed = isAllowed;
        }

        public Player Player { get; }

        public SubClass Subclass { get; set; }

        public bool IsAllowed { get; set; }
    }
}
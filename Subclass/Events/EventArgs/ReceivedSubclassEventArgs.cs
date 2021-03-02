﻿namespace Subclass.Events.EventArgs
{
    using Exiled.API.Features;

    /// <summary>
    /// Contains the information after a player receives a subclass.
    /// </summary>
    public class ReceivedSubclassEventArgs : System.EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReceivedSubclassEventArgs"/> class.
        /// </summary>
        /// <param name="player">The <see cref="Exiled.API.Features.Player"/> that received the subclass</param>
        /// <param name="subClass">The <see cref="SubClass"/> the player got.</param>
        public ReceivedSubclassEventArgs(Player player, SubClass subClass)
        {
            this.Player = player;
            this.Subclass = subClass;
        }

        public Player Player { get; }

        public SubClass Subclass { get; }
    }
}
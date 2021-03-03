// <copyright file="Regeneration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Effects
{
#pragma warning disable SA1101

    using CustomPlayerEffects;
    using Exiled.API.Features;
    using Mirror;
    using UnityEngine;

    /// <summary>
    /// The Regeneration effect.
    /// </summary>
    public class Regeneration : PlayerEffect
    {
        private readonly Player player;

        private readonly float healthPerTick;

        /// <summary>
        /// Initializes a new instance of the <see cref="Regeneration"/> class.
        /// </summary>
        /// <param name="hub">The <see cref="ReferenceHub"/> of the user.</param>
        /// <param name="healthPerTick">The amount of health dealt per tick.</param>
        /// <param name="tickRate">The amount of time between each tick.</param>
        public Regeneration(ReferenceHub hub, float healthPerTick = 2f, float tickRate = 5f)
        {
            player = Player.Get(hub);

            Hub = hub;
            ActiveAt = 0f;
            TimeBetweenTicks = tickRate;
            TimeLeft = tickRate;

            this.healthPerTick = healthPerTick;
        }

        /// <summary>
        /// Gets or sets the window of time where the effect will be active.
        /// </summary>
        public float ActiveAt { get; set; }

        /// <inheritdoc/>
        public override void PublicUpdate()
        {
            if (!Enabled || Time.time < ActiveAt)
            {
                TimeLeft = TimeBetweenTicks;
                return;
            }

            if (!NetworkServer.active)
            {
                return;
            }

            if (Enabled)
            {
                TimeLeft -= Time.deltaTime;
                if (TimeLeft <= 0f)
                {
                    TimeLeft += TimeBetweenTicks;
                    if (player.Health + healthPerTick < player.MaxHealth)
                    {
                        player.Health += healthPerTick;
                    }
                    else
                    {
                        player.Health = player.MaxHealth;
                    }
                }
            }
            else
            {
                TimeLeft = TimeBetweenTicks;
            }
        }
    }
}
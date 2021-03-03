// <copyright file="DamageAura.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Effects
{
#pragma warning disable SA1101

    using System.Collections.Generic;
    using System.Linq;
    using CustomPlayerEffects;
    using Exiled.API.Features;
    using Mirror;
    using UnityEngine;

    /// <summary>
    /// The DamageAura effect.
    /// </summary>
    public class DamageAura : PlayerEffect
    {
        private readonly Player player;

        private readonly float healthPerTick;

        private readonly bool affectSelf;
        private readonly bool affectAllies;
        private readonly bool affectEnemies;

        /// <summary>
        /// Initializes a new instance of the <see cref="DamageAura"/> class.
        /// </summary>
        /// <param name="hub">The <see cref="ReferenceHub"/> of the user.</param>
        /// <param name="healthPerTick">The amount of health dealt per tick.</param>
        /// <param name="radius">The range of the effect.</param>
        /// <param name="affectSelf">If the effect should affect the user.</param>
        /// <param name="affectAllies">If the effect should affect the user's allies.</param>
        /// <param name="affectEnemies">If the effect should affect the user's enemies.</param>
        /// <param name="tickRate">The amount of time between each tick.</param>
        public DamageAura(ReferenceHub hub, float healthPerTick = 5f, float radius = 4f, bool affectSelf = false, bool affectAllies = false, bool affectEnemies = true, float tickRate = 5f)
        {
            player = Player.Get(hub);

            Hub = hub;
            TimeBetweenTicks = tickRate;
            TimeLeft = tickRate;

            this.healthPerTick = healthPerTick;
            this.affectSelf = affectSelf;
            this.affectAllies = affectAllies;
            this.affectEnemies = affectEnemies;
        }

        /// <inheritdoc/>
        public override void PublicUpdate()
        {
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
                    IEnumerable<Player> players = Physics.OverlapSphere(Hub.transform.position, 1 << 2).Where(t => Player.Get(t.gameObject) != null).Select(t => Player.Get(t.gameObject)).Distinct();
                    foreach (Player p in players)
                    {
                        if ((!affectEnemies && p.Team != player.Team) ||
                            (p.Id != player.Id && !affectAllies && p.Team == player.Team))
                        {
                            continue;
                        }

                        if (p.Id == player.Id && !affectSelf)
                        {
                            continue;
                        }

                        p.ReferenceHub.playerStats.HurtPlayer(new PlayerStats.HitInfo(healthPerTick, player.Nickname, DamageTypes.Poison, player.Id), p.GameObject);
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
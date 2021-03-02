namespace Subclass.Effects
{
    using System.Collections.Generic;
    using System.Linq;
    using CustomPlayerEffects;
    using Exiled.API.Features;
    using Mirror;
    using UnityEngine;

    public class HealAura : PlayerEffect
    {
        private readonly Player player;

        private readonly float healthPerTick;
        private readonly float radius;

        private readonly bool affectSelf;
        private readonly bool affectAllies;
        private readonly bool affectEnemies;

        public HealAura(ReferenceHub hub, float healthPerTick = 5f, float radius = 4f, bool affectSelf = true, bool affectAllies = true, bool affectEnemies = false, float tickRate = 5f)
        {
            this.player = Player.Get(hub);

            this.Hub = hub;
            this.TimeBetweenTicks = tickRate;
            this.TimeLeft = tickRate;

            this.healthPerTick = healthPerTick;
            this.radius = radius;
            this.affectSelf = affectSelf;
            this.affectAllies = affectAllies;
            this.affectEnemies = affectEnemies;
        }

        public override void PublicUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (this.Enabled)
            {
                this.TimeLeft -= Time.deltaTime;
                if (this.TimeLeft <= 0f)
                {
                    this.TimeLeft += this.TimeBetweenTicks;
                    IEnumerable<Player> players = Physics.OverlapSphere(this.Hub.transform.position, this.radius).Where(t => Player.Get(t.gameObject) != null).Select(t => Player.Get(t.gameObject)).Distinct();
                    foreach (Player p in players)
                    {
                        if ((!this.affectEnemies && p.Team != this.player.Team) || (p.Id != this.player.Id && !this.affectAllies && p.Team == this.player.Team))
                        {
                            continue;
                        }

                        if (p.Id == this.player.Id && !this.affectSelf)
                        {
                            continue;
                        }

                        if (p.Health == p.MaxHealth)
                        {
                            continue;
                        }

                        if (TrackingAndMethods.PlayersWithSubclasses.ContainsKey(p) && TrackingAndMethods.PlayersWithSubclasses[p].Abilities.Contains(AbilityType.CantHeal))
                        {
                            continue;
                        }

                        if (p.Health + this.healthPerTick < p.MaxHealth)
                        {
                            p.Health += this.healthPerTick;
                        }
                        else
                        {
                            p.Health = p.MaxHealth;
                        }
                    }
                }
            }
            else
            {
                this.TimeLeft = this.TimeBetweenTicks;
            }
        }
    }
}
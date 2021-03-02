namespace Subclass.Effects
{
    using CustomPlayerEffects;
    using Exiled.API.Features;
    using Mirror;
    using UnityEngine;

    public class Regeneration : PlayerEffect
    {
        private readonly Player player;

        private readonly float healthPerTick;

        public Regeneration(ReferenceHub hub, float healthPerTick = 2f, float tickRate = 5f)
        {
            this.player = Player.Get(hub);

            this.Hub = hub;
            this.ActiveAt = 0f;
            this.TimeBetweenTicks = tickRate;
            this.TimeLeft = tickRate;

            this.healthPerTick = healthPerTick;
        }

        public float ActiveAt { get; set; }

        public override void PublicUpdate()
        {
            if (!this.Enabled || Time.time < this.ActiveAt)
            {
                this.TimeLeft = this.TimeBetweenTicks;
                return;
            }

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
                    if (this.player.Health + this.healthPerTick < this.player.MaxHealth)
                    {
                        this.player.Health += this.healthPerTick;
                    }
                    else
                    {
                        this.player.Health = this.player.MaxHealth;
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
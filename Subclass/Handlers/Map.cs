namespace Subclass.Handlers
{
    using System.Linq;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using UnityEngine;
    using EPlayer = Exiled.API.Features.Player;

    public class Map
    {
        public static void OnExplodingGrenade(ExplodingGrenadeEventArgs ev)
        {
            if (!TrackingAndMethods.PlayersWithSubclasses.ContainsKey(ev.Thrower) ||
                (!TrackingAndMethods.PlayersWithSubclasses[ev.Thrower].Abilities
                     .Contains(AbilityType.HealGrenadeFlash) &&
                 !TrackingAndMethods.PlayersWithSubclasses[ev.Thrower].Abilities.Contains(AbilityType.HealGrenadeFrag)))
            {
                Log.Debug($"Player with name {ev.Thrower.Nickname} has no subclass", Plugin.Instance.Config.Debug);
                return;
            }

            if (TrackingAndMethods.PlayersWithSubclasses[ev.Thrower].Abilities.Contains(AbilityType.HealGrenadeFlash) &&
                !ev.IsFrag)
            {
                if (!TrackingAndMethods.CanUseAbility(ev.Thrower, AbilityType.HealGrenadeFlash, TrackingAndMethods.PlayersWithSubclasses[ev.Thrower]))
                {
                    TrackingAndMethods.DisplayCantUseAbility(ev.Thrower, AbilityType.HealGrenadeFlash, TrackingAndMethods.PlayersWithSubclasses[ev.Thrower], "heal flash");
                    return;
                }

                TrackingAndMethods.UseAbility(ev.Thrower, AbilityType.HealGrenadeFlash, TrackingAndMethods.PlayersWithSubclasses[ev.Thrower]);
                ev.IsAllowed = false;
                UpdateHealths(ev, "HealGrenadeFlashHealAmount");
            }
            else if (TrackingAndMethods.PlayersWithSubclasses[ev.Thrower].Abilities
                .Contains(AbilityType.HealGrenadeFrag) && ev.IsFrag)
            {
                if (!TrackingAndMethods.CanUseAbility(ev.Thrower, AbilityType.HealGrenadeFrag, TrackingAndMethods.PlayersWithSubclasses[ev.Thrower]))
                {
                    TrackingAndMethods.DisplayCantUseAbility(ev.Thrower, AbilityType.HealGrenadeFrag, TrackingAndMethods.PlayersWithSubclasses[ev.Thrower], "heal frag");
                    return;
                }

                TrackingAndMethods.UseAbility(ev.Thrower, AbilityType.HealGrenadeFrag, TrackingAndMethods.PlayersWithSubclasses[ev.Thrower]);
                ev.IsAllowed = false;
                UpdateHealths(ev, "HealGrenadeFragHealAmount");
            }
        }

        private static void UpdateHealths(ExplodingGrenadeEventArgs ev, string type)
        {
            Collider[] colliders = Physics.OverlapSphere(ev.Grenade.transform.position, 4);
            foreach (var collider in colliders.Where(c => c.name == "Player"))
            {
                EPlayer player = EPlayer.Get(collider.gameObject);
                if (player != null && player.Team == ev.Thrower.Team)
                {
                    if (TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) && TrackingAndMethods
                        .PlayersWithSubclasses[player].Abilities.Contains(AbilityType.CantHeal))
                    {
                        return;
                    }

                    if (TrackingAndMethods.PlayersWithSubclasses[ev.Thrower].FloatOptions.ContainsKey(type))
                    {
                        if (TrackingAndMethods.PlayersWithSubclasses[ev.Thrower].FloatOptions[type] + player.Health >
                            player.MaxHealth)
                        {
                            player.Health = player.MaxHealth;
                        }
                        else
                        {
                            player.Health += TrackingAndMethods.PlayersWithSubclasses[ev.Thrower].FloatOptions[type];
                        }
                    }
                    else
                    {
                        player.Health = player.MaxHealth;
                    }
                }
            }
        }
    }
}
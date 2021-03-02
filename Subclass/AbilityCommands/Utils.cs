﻿namespace Subclass.AbilityCommands
{
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features;
    using Grenades;
    using MEC;
    using Mirror;
    using UnityEngine;

    public static class Utils
    {
        public static void AttemptRevive(Player player, SubClass subClass, bool necro = false)
        {
            Log.Debug($"Player {player.Nickname} {(necro ? "necromancy" : "revive")} attempt", Plugin.Instance.Config.Debug);
            AbilityType ability = necro ? AbilityType.Necromancy : AbilityType.Revive;
            if (TrackingAndMethods.OnCooldown(player, ability, subClass))
            {
                Log.Debug($"Player {player.Nickname} {(necro ? "necromancy" : "revive")} on cooldown", Plugin.Instance.Config.Debug);
                TrackingAndMethods.DisplayCooldown(player, necro ? AbilityType.Necromancy : AbilityType.Revive, subClass, necro ? "necromancy" : "revive", Time.time);
                return;
            }

            List<Collider> colliders = Physics.OverlapSphere(player.Position, 3f).Where(e => e.gameObject.GetComponentInParent<Ragdoll>() != null).ToList();

            colliders.Sort((x, y) => Vector3.Distance(x.gameObject.transform.position, player.Position).CompareTo(Vector3.Distance(y.gameObject.transform.position, player.Position)));

            if (colliders.Count == 0)
            {
                player.Broadcast(2, Plugin.Instance.Config.ReviveFailedNoBodyMessage);
                Log.Debug($"Player {player.Nickname} {(necro ? "necromancy" : "revive")} overlap did not hit a ragdoll", Plugin.Instance.Config.Debug);
                return;
            }

            Ragdoll doll = colliders[0].gameObject.GetComponentInParent<Ragdoll>();

            if (doll.owner.DeathCause.GetDamageType() == DamageTypes.Lure)
            {
                Log.Debug($"Player {player.Nickname} {(necro ? "necromancy" : "revive")} failed", Plugin.Instance.Config.Debug);
                player.Broadcast(2, Plugin.Instance.Config.CantReviveMessage);
                return;
            }

            Player owner = Player.Get(doll.owner.PlayerId);
            if (owner != null && !owner.IsAlive)
            {
                bool revived = false;
                if (!necro && TrackingAndMethods.GetPreviousTeam(owner) != null &&
                    TrackingAndMethods.GetPreviousTeam(owner) == player.Team &&
                    TrackingAndMethods.RagdollRole(doll) != null && TrackingAndMethods.RagdollRole(doll) ==
                    TrackingAndMethods.GetPreviousRole(owner))
                {
                    if (TrackingAndMethods.PlayersThatJustGotAClass.ContainsKey(owner))
                    {
                        TrackingAndMethods.PlayersThatJustGotAClass[owner] = Time.time + 3f;
                    }
                    else
                    {
                        TrackingAndMethods.PlayersThatJustGotAClass.Add(owner, Time.time + 3f);
                    }

                    owner.SetRole((RoleType)TrackingAndMethods.GetPreviousRole(owner), true);

                    if (TrackingAndMethods.PreviousSubclasses.ContainsKey(owner) && TrackingAndMethods.PreviousSubclasses[owner].AffectsRoles.Contains((RoleType)TrackingAndMethods.GetPreviousRole(owner)))
                    {
                        TrackingAndMethods.AddClass(owner, TrackingAndMethods.PreviousSubclasses[owner], false, true);
                    }

                    owner.Inventory.Clear();
                    revived = true;
                }
                else if (necro)
                {
                    owner.Role = RoleType.Scp0492;
                    TrackingAndMethods.AddZombie(player, owner);
                    owner.IsFriendlyFireEnabled = true;
                    revived = true;
                }

                if (revived)
                {
                    Timing.CallDelayed(0.2f, () =>
                    {
                        owner.ReferenceHub.playerMovementSync.OverridePosition(
                            player.Position + new Vector3(0.3f, 1f, 0), 0, true);
                        if (subClass.FloatOptions.ContainsKey("PercentHealthOnRevive") && !necro)
                        {
                            owner.Health *= subClass.FloatOptions["PercentHealthOnRevive"] / 100f;
                        }
                        else if (subClass.FloatOptions.ContainsKey("PercentHealthOnNecro") && necro)
                        {
                            owner.Health *= subClass.FloatOptions["PercentHealthOnNecro"] / 100f;
                        }
                    });

                    NetworkServer.Destroy(doll.gameObject);
                    TrackingAndMethods.AddCooldown(player, ability);
                    TrackingAndMethods.UseAbility(player, ability, subClass);
                    Log.Debug($"Player {player.Nickname} {(necro ? "necromancy" : "revive")} succeeded", Plugin.Instance.Config.Debug);
                }
                else
                {
                    Log.Debug($"Player {player.Nickname} {(necro ? "necromancy" : "revive")} failed", Plugin.Instance.Config.Debug);
                    player.Broadcast(2, Plugin.Instance.Config.CantReviveMessage);
                }
            }
            else
            {
                Log.Debug($"Player {player.Nickname} {(necro ? "necromancy" : "revive")} failed", Plugin.Instance.Config.Debug);
                player.Broadcast(2, Plugin.Instance.Config.CantReviveMessage);
            }
        }

        public static void SpawnGrenade(ItemType type, Player player, SubClass subClass)
        {
            // Credit to KoukoCocoa's AdminTools for the grenade spawn script below, I was lost. https://github.com/KoukoCocoa/AdminTools/
            GrenadeManager grenadeManager = player.GrenadeManager;
            GrenadeSettings settings = grenadeManager.availableGrenades.FirstOrDefault(g => g.inventoryID == type);
            Grenades.Grenade grenade = Object.Instantiate(settings.grenadeInstance).GetComponent<Grenades.Grenade>();
            if (type == ItemType.GrenadeFlash)
            {
                grenade.fuseDuration = subClass.FloatOptions.ContainsKey("FlashOnCommandFuseTimer") ? subClass.FloatOptions["FlashOnCommandFuseTimer"] : 0.3f;
            }
            else
            {
                grenade.fuseDuration = subClass.FloatOptions.ContainsKey("GrenadeOnCommandFuseTimer") ? subClass.FloatOptions["GrenadeOnCommandFuseTimer"] : 0.3f;
            }

            grenade.FullInitData(grenadeManager, player.Position, Quaternion.Euler(grenade.throwStartAngle), grenade.throwLinearVelocityOffset, grenade.throwAngularVelocity, player.Team);
            NetworkServer.Spawn(grenade.gameObject);
        }
    }
}
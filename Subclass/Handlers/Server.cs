// <copyright file="Server.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using MEC;
    using Respawning;
    using UnityEngine;
    using EPlayer = Exiled.API.Features.Player;

    /// <summary>
    /// Event handlers for <see cref="Exiled.Events.Handlers.Server"/>.
    /// </summary>
    public class Server
    {
        private static readonly System.Random Random = new System.Random();

        public void OnRoundStarted()
        {
            TrackingAndMethods.RoundStartedAt = Time.time;
            Timing.CallDelayed(Plugin.Instance.CommonUtilsEnabled ? 2f : 0.1f, () =>
            {
                Log.Debug("Round started!", Plugin.Instance.Config.Debug);
                foreach (EPlayer player in EPlayer.List)
                {
                    TrackingAndMethods.MaybeAddRoles(player);
                }

                foreach (string message in TrackingAndMethods.QueuedCassieMessages)
                {
                    Cassie.Message(message, true, false);
                    Log.Debug($"Sending message via cassie: {message}", Plugin.Instance.Config.Debug);
                }

                TrackingAndMethods.QueuedCassieMessages.Clear();
            });
        }

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            TrackingAndMethods.KillAllCoroutines();
            TrackingAndMethods.Coroutines.Clear();
            TrackingAndMethods.PlayersWithSubclasses.Clear();
            TrackingAndMethods.Cooldowns.Clear();
            TrackingAndMethods.FriendlyFired.Clear();
            TrackingAndMethods.PlayersThatBypassedTeslaGates.Clear();
            TrackingAndMethods.PreviousRoles.Clear();
            TrackingAndMethods.PlayersWithZombies.Clear();
            TrackingAndMethods.QueuedCassieMessages.Clear();
            TrackingAndMethods.NextSpawnWave.Clear();
            TrackingAndMethods.NextSpawnWaveGetsRole.Clear();
            TrackingAndMethods.PlayersThatJustGotAClass.Clear();
            TrackingAndMethods.SubClassesSpawned.Clear();
            TrackingAndMethods.PreviousSubclasses.Clear();
            TrackingAndMethods.PreviousBadges.Clear();
            TrackingAndMethods.RagdollRoles.Clear();
            TrackingAndMethods.AbilityUses.Clear();
            TrackingAndMethods.PlayersInvisibleByCommand.Clear();
            TrackingAndMethods.PlayersVenting.Clear();
            TrackingAndMethods.NumSpawnWaves.Clear();
            TrackingAndMethods.SpawnWaveSpawns.Clear();
            TrackingAndMethods.ClassesGiven.Clear();
            TrackingAndMethods.DontGiveClasses.Clear();
            TrackingAndMethods.PlayersBloodLusting.Clear();
            TrackingAndMethods.Zombie106Kills.Clear();
            API.EnableAllClasses();
        }

        public void OnRespawningTeam(RespawningTeamEventArgs ev)
        {
            if (ev.Players.Count == 0 || !ev.IsAllowed)
            {
                return;
            }

            Team spawnedTeam = ev.NextKnownTeam == SpawnableTeamType.NineTailedFox ? Team.MTF : Team.CHI;
            if (!TrackingAndMethods.NumSpawnWaves.ContainsKey(spawnedTeam))
            {
                TrackingAndMethods.NumSpawnWaves.Add(spawnedTeam, 0);
            }

            TrackingAndMethods.NumSpawnWaves[spawnedTeam]++;
            Timing.CallDelayed(5f, () => // Clear them after the wave spawns instead.
            {
                TrackingAndMethods.NextSpawnWave.Clear();
                TrackingAndMethods.NextSpawnWaveGetsRole.Clear();
                TrackingAndMethods.SpawnWaveSpawns.Clear();
            });
            bool ntfSpawning = ev.NextKnownTeam == SpawnableTeamType.NineTailedFox;
            if (!Plugin.Instance.Config.AdditiveChance)
            {
                List<RoleType> hasRole = new List<RoleType>();
                foreach (SubClass subClass in Plugin.Instance.Classes.Values.Where(e => e.BoolOptions["Enabled"] &&
                    (!e.IntOptions.ContainsKey("MaxSpawnPerRound") ||
                     TrackingAndMethods.ClassesSpawned(e) < e.IntOptions["MaxSpawnPerRound"]) &&
                    (ntfSpawning
                        ? (e.AffectsRoles.Contains(RoleType.NtfCadet) ||
                           e.AffectsRoles.Contains(RoleType.NtfCommander) ||
                           e.AffectsRoles.Contains(RoleType.NtfLieutenant))
                        : e.AffectsRoles.Contains(RoleType.ChaosInsurgency)) &&
                    ((e.BoolOptions.ContainsKey("OnlyAffectsSpawnWave") && e.BoolOptions["OnlyAffectsSpawnWave"]) ||
                     (e.BoolOptions.ContainsKey("AffectsSpawnWave") && e.BoolOptions["AffectsSpawnWave"])) &&
                    (!e.BoolOptions.ContainsKey("WaitForSpawnWaves") || (e.BoolOptions["WaitForSpawnWaves"] &&
                                                                         TrackingAndMethods.GetNumWavesSpawned(
                                                                             e.StringOptions.ContainsKey(
                                                                                 "WaitSpawnWaveTeam")
                                                                                 ? (Team)Enum.Parse(typeof(Team), e.StringOptions["WaitSpawnWaveTeam"])
                                                                                 : Team.RIP) <
                                                                         e.IntOptions["NumSpawnWavesToWait"])) &&
                    TrackingAndMethods.EvaluateSpawnParameters(e)))
                {
                    if ((ntfSpawning
                            ? (subClass.AffectsRoles.Contains(RoleType.NtfCadet) ||
                               subClass.AffectsRoles.Contains(RoleType.NtfCommander) ||
                               subClass.AffectsRoles.Contains(RoleType.NtfLieutenant))
                            : subClass.AffectsRoles.Contains(RoleType.ChaosInsurgency)) &&
                        (Random.NextDouble() * 100) < subClass.FloatOptions["ChanceToGet"])
                    {
                        if (ntfSpawning)
                        {
                            if (!hasRole.Contains(RoleType.NtfCadet) &&
                                subClass.AffectsRoles.Contains(RoleType.NtfCadet))
                            {
                                TrackingAndMethods.NextSpawnWaveGetsRole.Add(RoleType.NtfCadet, subClass);
                                hasRole.Add(RoleType.NtfCadet);
                            }

                            if (!hasRole.Contains(RoleType.NtfLieutenant) &&
                                subClass.AffectsRoles.Contains(RoleType.NtfLieutenant))
                            {
                                TrackingAndMethods.NextSpawnWaveGetsRole.Add(RoleType.NtfLieutenant, subClass);
                                hasRole.Add(RoleType.NtfLieutenant);
                            }

                            if (!hasRole.Contains(RoleType.NtfCommander) &&
                                subClass.AffectsRoles.Contains(RoleType.NtfCommander))
                            {
                                TrackingAndMethods.NextSpawnWaveGetsRole.Add(RoleType.NtfCommander, subClass);
                                hasRole.Add(RoleType.NtfCommander);
                            }

                            if (hasRole.Count == 3)
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (subClass.AffectsRoles.Contains(RoleType.ChaosInsurgency))
                            {
                                TrackingAndMethods.NextSpawnWaveGetsRole.Add(RoleType.ChaosInsurgency, subClass);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                double num = Random.NextDouble() * 100;
                if (!ntfSpawning && !Plugin.Instance.ClassesAdditive.ContainsKey(RoleType.ChaosInsurgency))
                {
                    return;
                }

                if (ntfSpawning && !Plugin.Instance.ClassesAdditive.ContainsKey(RoleType.NtfCadet) &&
                    !Plugin.Instance.ClassesAdditive.ContainsKey(RoleType.NtfCommander) &&
                    !Plugin.Instance.ClassesAdditive.ContainsKey(RoleType.NtfLieutenant))
                {
                    return;
                }

                if (!ntfSpawning)
                {
                    foreach (var possibility in Plugin.Instance.ClassesAdditive[RoleType.ChaosInsurgency].Where(e =>
                        e.Key.BoolOptions["Enabled"] &&
                        (!e.Key.IntOptions.ContainsKey("MaxSpawnPerRound") || TrackingAndMethods.ClassesSpawned(e.Key) <
                            e.Key.IntOptions["MaxSpawnPerRound"]) &&
                        ((e.Key.BoolOptions.ContainsKey("OnlyAffectsSpawnWave") &&
                          e.Key.BoolOptions["OnlyAffectsSpawnWave"]) ||
                         (e.Key.BoolOptions.ContainsKey("AffectsSpawnWave") &&
                          e.Key.BoolOptions["AffectsSpawnWave"])) &&
                        (!e.Key.BoolOptions.ContainsKey("WaitForSpawnWaves") ||
                         (e.Key.BoolOptions["WaitForSpawnWaves"] &&
                          TrackingAndMethods.GetNumWavesSpawned(e.Key.StringOptions.ContainsKey("WaitSpawnWaveTeam")
                              ? (Team)Enum.Parse(typeof(Team), e.Key.StringOptions["WaitSpawnWaveTeam"])
                              : Team.RIP) < e.Key.IntOptions["NumSpawnWavesToWait"])) &&
                        TrackingAndMethods.EvaluateSpawnParameters(e.Key)))
                    {
                        Log.Debug($"Evaluating possible subclass {possibility.Key.Name} for next spawn wave", Plugin.Instance.Config.Debug);
                        if (num < possibility.Value)
                        {
                            TrackingAndMethods.NextSpawnWaveGetsRole.Add(RoleType.ChaosInsurgency, possibility.Key);
                            break;
                        }

                        Log.Debug($"Next spawn wave did not get subclass {possibility.Key.Name}", Plugin.Instance.Config.Debug);
                    }
                }
                else
                {
                    RoleType[] roles = { RoleType.NtfCommander, RoleType.NtfLieutenant, RoleType.NtfCadet };
                    foreach (RoleType role in roles)
                    {
                        foreach (var possibility in Plugin.Instance.ClassesAdditive[role].Where(e =>
                            e.Key.BoolOptions["Enabled"] &&
                            (!e.Key.IntOptions.ContainsKey("MaxSpawnPerRound") ||
                             TrackingAndMethods.ClassesSpawned(e.Key) < e.Key.IntOptions["MaxSpawnPerRound"]) &&
                            ((e.Key.BoolOptions.ContainsKey("OnlyAffectsSpawnWave") &&
                              e.Key.BoolOptions["OnlyAffectsSpawnWave"]) ||
                             (e.Key.BoolOptions.ContainsKey("AffectsSpawnWave") &&
                              e.Key.BoolOptions["AffectsSpawnWave"])) &&
                            (!e.Key.BoolOptions.ContainsKey("WaitForSpawnWaves") ||
                             (e.Key.BoolOptions["WaitForSpawnWaves"] &&
                              TrackingAndMethods.GetNumWavesSpawned(e.Key.StringOptions.ContainsKey("WaitSpawnWaveTeam")
                                  ? (Team)Enum.Parse(typeof(Team), e.Key.StringOptions["WaitSpawnWaveTeam"])
                                  : Team.RIP) < e.Key.IntOptions["NumSpawnWavesToWait"]))
                            && TrackingAndMethods.EvaluateSpawnParameters(e.Key)))
                        {
                            Log.Debug(
                                $"Evaluating possible subclass {possibility.Key.Name} for next spawn wave",
                                Plugin.Instance.Config.Debug);
                            if (num < possibility.Value)
                            {
                                TrackingAndMethods.NextSpawnWaveGetsRole.Add(role, possibility.Key);
                                break;
                            }

                            Log.Debug($"Next spawn wave did not get subclass {possibility.Key.Name}", Plugin.Instance.Config.Debug);
                        }
                    }
                }
            }

            TrackingAndMethods.NextSpawnWave = ev.Players;
        }

        public void AttemptRevive(SendingConsoleCommandEventArgs ev, SubClass subClass, bool necro = false)
        {
            Log.Debug($"Player {ev.Player.Nickname} {(necro ? "necromancy" : "revive")} attempt", Plugin.Instance.Config.Debug);
            AbilityType ability = necro ? AbilityType.Necromancy : AbilityType.Revive;
            if (TrackingAndMethods.OnCooldown(ev.Player, ability, subClass))
            {
                Log.Debug($"Player {ev.Player.Nickname} {(necro ? "necromancy" : "revive")} on cooldown", Plugin.Instance.Config.Debug);
                TrackingAndMethods.DisplayCooldown(ev.Player, necro ? AbilityType.Necromancy : AbilityType.Revive, subClass, necro ? "necromancy" : "revive", Time.time);
                return;
            }

            List<Collider> colliders = Physics.OverlapSphere(ev.Player.Position, 3f)
                .Where(e => e.gameObject.GetComponentInParent<Ragdoll>() != null).ToList();

            colliders.Sort((x, y) => Vector3.Distance(x.gameObject.transform.position, ev.Player.Position).CompareTo(Vector3.Distance(y.gameObject.transform.position, ev.Player.Position)));

            if (colliders.Count == 0)
            {
                ev.Player.Broadcast(2, Plugin.Instance.Config.ReviveFailedNoBodyMessage);
                Log.Debug(
                    $"Player {ev.Player.Nickname} {(necro ? "necromancy" : "revive")} overlap did not hit a ragdoll",
                    Plugin.Instance.Config.Debug);
                return;
            }

            Ragdoll doll = colliders[0].gameObject.GetComponentInParent<Ragdoll>();

            if (doll.owner.DeathCause.GetDamageType() == DamageTypes.Lure)
            {
                Log.Debug($"Player {ev.Player.Nickname} {(necro ? "necromancy" : "revive")} failed", Plugin.Instance.Config.Debug);
                ev.Player.Broadcast(2, Plugin.Instance.Config.CantReviveMessage);
                return;
            }

            EPlayer owner = EPlayer.Get(colliders[0].gameObject.GetComponentInParent<Ragdoll>().owner.PlayerId);
            if (owner != null && !owner.IsAlive)
            {
                bool revived = false;
                if (!necro && TrackingAndMethods.GetPreviousTeam(owner) != null &&
                    TrackingAndMethods.GetPreviousTeam(owner) == ev.Player.Team)
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

                    if (TrackingAndMethods.PreviousSubclasses.ContainsKey(owner) && TrackingAndMethods
                        .PreviousSubclasses[owner].AffectsRoles
                        .Contains((RoleType)TrackingAndMethods.GetPreviousRole(owner)))
                    {
                        TrackingAndMethods.AddClass(owner, TrackingAndMethods.PreviousSubclasses[owner], false, true);
                    }

                    owner.Inventory.Clear();
                    revived = true;
                }
                else if (necro)
                {
                    owner.Role = RoleType.Scp0492;
                    TrackingAndMethods.AddZombie(ev.Player, owner);
                    owner.IsFriendlyFireEnabled = true;
                    revived = true;
                }

                if (revived)
                {
                    Timing.CallDelayed(0.2f, () =>
                        {
                            owner.ReferenceHub.playerMovementSync.OverridePosition(
                                ev.Player.Position + new Vector3(0.3f, 1f, 0), 0, true);
                        });

                    UnityEngine.Object.DestroyImmediate(doll.gameObject, true);
                    TrackingAndMethods.AddCooldown(ev.Player, ability);
                    TrackingAndMethods.UseAbility(ev.Player, ability, subClass);
                    Log.Debug($"Player {ev.Player.Nickname} {(necro ? "necromancy" : "revive")} succeeded", Plugin.Instance.Config.Debug);
                }
                else
                {
                    Log.Debug($"Player {ev.Player.Nickname} revive failed", Plugin.Instance.Config.Debug);
                    ev.Player.Broadcast(2, Plugin.Instance.Config.CantReviveMessage);
                }
            }
            else
            {
                Log.Debug($"Player {ev.Player.Nickname} {(necro ? "necromancy" : "revive")} failed", Plugin.Instance.Config.Debug);
                ev.Player.Broadcast(2, Plugin.Instance.Config.CantReviveMessage);
            }
        }
    }
}
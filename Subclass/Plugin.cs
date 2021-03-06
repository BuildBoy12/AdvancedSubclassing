// <copyright file="Plugin.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass
{
#pragma warning disable SA1101

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Exiled.Loader;
    using HarmonyLib;
    using Subclass.Managers;
    using Map = Exiled.Events.Handlers.Map;
    using Player = Exiled.Events.Handlers.Player;
    using Server = Exiled.Events.Handlers.Server;

    /// <inheritdoc/>
    public class Plugin : Plugin<Config>
    {
        private int harmonyPatches;

        /// <summary>
        /// Gets the instance of the <see cref="Plugin"/> class.
        /// </summary>
        public static Plugin Instance { get; private set; }

        /// <inheritdoc/>
        public override PluginPriority Priority { get; } = PluginPriority.Last;

        /// <inheritdoc/>
        public override string Name { get; } = "Subclass";

        /// <inheritdoc/>
        public override string Author { get; } = "Steven4547466";

        /// <inheritdoc/>
        public override Version Version { get; } = new Version(1, 4, 0);

        /// <inheritdoc/>
        public override Version RequiredExiledVersion { get; } = new Version(2, 1, 28);

        /// <inheritdoc/>
        public override string Prefix { get; } = "Subclass";

        /// <summary>
        /// Gets or sets all class combinations.
        /// </summary>
        public Dictionary<string, SubClass> Classes { get; set; }

        /// <summary>
        /// Gets the additive class combinations.
        /// </summary>
        public Dictionary<RoleType, Dictionary<SubClass, float>> ClassesAdditive { get; private set; }

        /// <summary>
        /// Gets the weighted class combinations.
        /// </summary>
        public Dictionary<RoleType, Dictionary<SubClass, float>> ClassesWeighted { get; private set; }

        /// <summary>
        /// Gets a value indicating whether Scp035 is enabled.
        /// </summary>
        public bool Scp035Enabled { get; } = Loader.Plugins.Any(p => p.Name == "scp035" && p.Config.IsEnabled);

        /// <summary>
        /// Gets a value indicating whether CommonUtils is enabled.
        /// </summary>
        public bool CommonUtilsEnabled { get; } = Loader.Plugins.Any(p => p.Name == "Common Utilities" && p.Config.IsEnabled);

        /// <summary>
        /// Gets a value indicating whether RealisticSizes is enabled.
        /// </summary>
        public bool RealisticSizesEnabled { get; } = Loader.Plugins.Any(p => p.Name == "RealisticSizes" && p.Config.IsEnabled);

        /// <summary>
        /// Gets a value indicating whether Scp008X is enabled.
        /// </summary>
        public bool Scp008XEnabled { get; } = Loader.Plugins.Any(p => p.Name == "Scp008X" && p.Config.IsEnabled);

        private Handlers.Player PlayerHandler { get; set; }

        private Handlers.Server ServerHandler { get; set; }

        private Harmony HarmonyInstance { get; set; }

        /// <inheritdoc/>
        public override void OnEnabled()
        {
            Instance = this;
            RegisterEvents();
            Classes = GetClasses();

            HarmonyInstance = new Harmony($"steven4547466.subclass-{++harmonyPatches}");
            HarmonyInstance.PatchAll();
            base.OnEnabled();
        }

        /// <inheritdoc/>
        public override void OnDisabled()
        {
            UnregisterEvents();
            HarmonyInstance.UnpatchAll();
            foreach (Exiled.API.Features.Player player in Exiled.API.Features.Player.List)
            {
                TrackingAndMethods.RemoveAndAddRoles(player, true);
            }

            Instance = null;
            base.OnDisabled();
        }

        /// <summary>
        /// Gets all available classes.
        /// </summary>
        /// <returns>Returns a <see cref="Dictionary{TKey,TValue}"/> with classes based on their respective weights.</returns>
        public Dictionary<string, SubClass> GetClasses()
        {
            Dictionary<string, SubClass> classes = SubclassManager.LoadClasses();
            Config config = Instance.Config;
            if (config.AdditiveChance)
            {
                ClassesAdditive = new Dictionary<RoleType, Dictionary<SubClass, float>>();
                foreach (var item in classes)
                {
                    foreach (RoleType role in item.Value.AffectsRoles)
                    {
                        if (!ClassesAdditive.ContainsKey(role))
                        {
                            ClassesAdditive.Add(role, new Dictionary<SubClass, float> { { item.Value, item.Value.FloatOptions["ChanceToGet"] } });
                        }
                        else
                        {
                            ClassesAdditive[role].Add(item.Value, item.Value.FloatOptions["ChanceToGet"]);
                        }
                    }
                }

                Dictionary<RoleType, Dictionary<SubClass, float>> classesAdditiveCopy = new Dictionary<RoleType, Dictionary<SubClass, float>>();
                foreach (RoleType role in ClassesAdditive.Keys)
                {
                    var additiveClasses = ClassesAdditive[role].ToList();
                    additiveClasses.Sort((x, y) => y.Value.CompareTo(x.Value));
                    if (!classesAdditiveCopy.ContainsKey(role))
                    {
                        classesAdditiveCopy.Add(role, new Dictionary<SubClass, float>());
                    }

                    classesAdditiveCopy[role] = additiveClasses.ToDictionary(x => x.Key, x => x.Value);
                }

                ClassesAdditive.Clear();
                Dictionary<RoleType, float> sums = new Dictionary<RoleType, float>();
                foreach (var roles in classesAdditiveCopy)
                {
                    foreach (SubClass subClass in classesAdditiveCopy[roles.Key].Keys)
                    {
                        if (!sums.ContainsKey(roles.Key))
                        {
                            sums.Add(roles.Key, 0);
                        }

                        sums[roles.Key] += subClass.FloatOptions["ChanceToGet"];
                        if (!ClassesAdditive.ContainsKey(roles.Key))
                        {
                            ClassesAdditive.Add(roles.Key, new Dictionary<SubClass, float> { { subClass, subClass.FloatOptions["ChanceToGet"] } });
                        }
                        else
                        {
                            ClassesAdditive[roles.Key].Add(subClass, sums[roles.Key]);
                        }
                    }
                }
            }
            else if (config.WeightedChance)
            {
                ClassesWeighted = new Dictionary<RoleType, Dictionary<SubClass, float>>();
                foreach (var item in classes)
                {
                    foreach (RoleType role in item.Value.AffectsRoles)
                    {
                        if (!ClassesWeighted.ContainsKey(role))
                        {
                            ClassesWeighted.Add(role, new Dictionary<SubClass, float> { { item.Value, item.Value.FloatOptions["ChanceToGet"] } });
                        }
                        else
                        {
                            ClassesWeighted[role].Add(item.Value, item.Value.FloatOptions["ChanceToGet"]);
                        }
                    }
                }

                Dictionary<RoleType, Dictionary<SubClass, float>> classesWeightedCopy = new Dictionary<RoleType, Dictionary<SubClass, float>>();
                foreach (RoleType role in ClassesWeighted.Keys)
                {
                    var weightedClasses = ClassesWeighted[role].ToList();
                    weightedClasses.Sort((x, y) => y.Value.CompareTo(x.Value));
                    if (!classesWeightedCopy.ContainsKey(role))
                    {
                        classesWeightedCopy.Add(role, new Dictionary<SubClass, float>());
                    }

                    classesWeightedCopy[role] = weightedClasses.ToDictionary(x => x.Key, x => x.Value);
                }

                ClassesWeighted.Clear();
                Dictionary<RoleType, float> totals = new Dictionary<RoleType, float>();
                foreach (var weight in config.BaseWeights)
                {
                    if (!totals.ContainsKey(weight.Key))
                    {
                        totals.Add(weight.Key, 0f);
                    }

                    totals[weight.Key] += weight.Value;
                }

                foreach (var item in classesWeightedCopy)
                {
                    foreach (SubClass subClass in classesWeightedCopy[item.Key].Keys)
                    {
                        if (!totals.ContainsKey(item.Key))
                        {
                            totals.Add(item.Key, 0f);
                        }

                        totals[item.Key] += subClass.FloatOptions["ChanceToGet"];
                    }
                }

                Dictionary<RoleType, float> sums = new Dictionary<RoleType, float>();
                foreach (var item in classesWeightedCopy)
                {
                    foreach (SubClass subClass in classesWeightedCopy[item.Key].Keys)
                    {
                        if (!sums.ContainsKey(item.Key))
                        {
                            sums.Add(item.Key, 0f);
                        }

                        sums[item.Key] += subClass.FloatOptions["ChanceToGet"];
                        if (!ClassesWeighted.ContainsKey(item.Key))
                        {
                            ClassesWeighted.Add(item.Key, new Dictionary<SubClass, float> { { subClass, 100 * (subClass.FloatOptions["ChanceToGet"] / totals[item.Key]) } });
                        }
                        else
                        {
                            ClassesWeighted[item.Key].Add(subClass, 100 * (sums[item.Key] / totals[item.Key]));
                        }
                    }
                }
            }
            else
            {
                ClassesAdditive = null;
                ClassesWeighted = null;
            }

            return classes;
        }

        private void RegisterEvents()
        {
            PlayerHandler = new Handlers.Player();
            Player.InteractingDoor += PlayerHandler.OnInteractingDoor;
            Player.Died += PlayerHandler.OnDied;
            Player.Dying += PlayerHandler.OnDying;
            Player.Shooting += PlayerHandler.OnShooting;
            Player.InteractingLocker += PlayerHandler.OnInteractingLocker;
            Player.UnlockingGenerator += PlayerHandler.OnUnlockingGenerator;
            Player.TriggeringTesla += PlayerHandler.OnTriggeringTesla;
            Player.ChangingRole += PlayerHandler.OnChangingRole;
            Player.Spawning += PlayerHandler.OnSpawning;
            Player.Hurting += PlayerHandler.OnHurting;
            Player.EnteringFemurBreaker += PlayerHandler.OnEnteringFemurBreaker;
            Player.Escaping += PlayerHandler.OnEscaping;
            Player.FailingEscapePocketDimension += PlayerHandler.OnFailingEscapePocketDimension;
            Player.Interacted += PlayerHandler.OnInteracted;
            Player.UsingMedicalItem += PlayerHandler.OnUsingMedicalItem;
            Player.EnteringPocketDimension += PlayerHandler.OnEnteringPocketDimension;
            Player.SpawningRagdoll += PlayerHandler.OnSpawningRagdoll;

            ServerHandler = new Handlers.Server();
            Server.RoundStarted += ServerHandler.OnRoundStarted;
            Server.RoundEnded += ServerHandler.OnRoundEnded;
            Server.RespawningTeam += ServerHandler.OnRespawningTeam;

            Map.ExplodingGrenade += Handlers.Map.OnExplodingGrenade;
            Log.Info("Events registered");
        }

        private void UnregisterEvents()
        {
            Player.InteractingDoor -= PlayerHandler.OnInteractingDoor;
            Player.Died -= PlayerHandler.OnDied;
            Player.Dying -= PlayerHandler.OnDying;
            Player.Shooting -= PlayerHandler.OnShooting;
            Player.InteractingLocker -= PlayerHandler.OnInteractingLocker;
            Player.UnlockingGenerator -= PlayerHandler.OnUnlockingGenerator;
            Player.TriggeringTesla -= PlayerHandler.OnTriggeringTesla;
            Player.ChangingRole -= PlayerHandler.OnChangingRole;
            Player.Spawning -= PlayerHandler.OnSpawning;
            Player.Hurting -= PlayerHandler.OnHurting;
            Player.EnteringFemurBreaker -= PlayerHandler.OnEnteringFemurBreaker;
            Player.Escaping -= PlayerHandler.OnEscaping;
            Player.FailingEscapePocketDimension -= PlayerHandler.OnFailingEscapePocketDimension;
            Player.Interacted -= PlayerHandler.OnInteracted;
            Player.UsingMedicalItem -= PlayerHandler.OnUsingMedicalItem;
            Player.EnteringPocketDimension -= PlayerHandler.OnEnteringPocketDimension;
            Player.SpawningRagdoll -= PlayerHandler.OnSpawningRagdoll;
            PlayerHandler = null;

            Server.RoundStarted -= ServerHandler.OnRoundStarted;
            Server.RoundEnded -= ServerHandler.OnRoundEnded;
            Server.RespawningTeam -= ServerHandler.OnRespawningTeam;

            ServerHandler = null;

            Map.ExplodingGrenade -= Handlers.Map.OnExplodingGrenade;
            Log.Info("Events unregistered");
        }
    }
}
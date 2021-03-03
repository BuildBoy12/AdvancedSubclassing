// <copyright file="SubClass.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass
{
#pragma warning disable SA1101
#pragma warning disable SA1117

    using System.Collections.Generic;
    using Exiled.API.Enums;

    /// <summary>
    /// The main SubClass class which interactions are defined from.
    /// </summary>
    public class SubClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubClass"/> class.
        /// </summary>
        /// <param name="name">The name of the <see cref="SubClass"/>.</param>
        /// <param name="roles">The roles that the <see cref="SubClass"/> can be.</param>
        /// <param name="strings">The string options of the <see cref="SubClass"/>.</param>
        /// <param name="bools">The bool options of the <see cref="SubClass"/>.</param>
        /// <param name="ints">The int options of the <see cref="SubClass"/>.</param>
        /// <param name="floats">The float options of the <see cref="SubClass"/>.</param>
        /// <param name="spawns"></param>
        /// <param name="items"></param>
        /// <param name="ammo"></param>
        /// <param name="abilities"></param>
        /// <param name="cooldowns"></param>
        /// <param name="ffRules"></param>
        /// <param name="onHitEffects"></param>
        /// <param name="spawnEffects"></param>
        /// <param name="cantDamage"></param>
        /// <param name="endsRoundWith"></param>
        /// <param name="spawnsAs"></param>
        /// <param name="escapesAs"></param>
        /// <param name="onTakeDamage"></param>
        /// <param name="cantDamageRoles"></param>
        /// <param name="cantDamageTeams"></param>
        /// <param name="teamsThatCantDamage"></param>
        /// <param name="cantDamageSubclasses"></param>
        /// <param name="subclassesThatCantDamage"></param>
        /// <param name="affectsUsers"></param>
        /// <param name="permissions"></param>
        /// <param name="initialAbilityCooldowns"></param>
        /// <param name="spawnParameters"></param>
        public SubClass(string name, List<RoleType> roles, Dictionary<string, string> strings,
            Dictionary<string, bool> bools, Dictionary<string, int> ints, Dictionary<string, float> floats,
            List<string> spawns, Dictionary<int, Dictionary<string, float>> items, Dictionary<AmmoType, int> ammo,
            List<AbilityType> abilities, Dictionary<AbilityType, float> cooldowns, List<string> ffRules = null,
            List<string> onHitEffects = null, List<string> spawnEffects = null, List<RoleType> cantDamage = null,
            string endsRoundWith = "RIP", RoleType spawnsAs = RoleType.None, RoleType[] escapesAs = null,
            Dictionary<string, List<string>> onTakeDamage = null, List<RoleType> cantDamageRoles = null,
            List<Team> cantDamageTeams = null, List<Team> teamsThatCantDamage = null,
            List<string> cantDamageSubclasses = null, List<string> subclassesThatCantDamage = null,
            Dictionary<string, float> affectsUsers = null, Dictionary<string, float> permissions = null,
            Dictionary<AbilityType, float> initialAbilityCooldowns = null,
            Dictionary<string, int> spawnParameters = null)
        {
            Name = name;
            AffectsRoles = roles;
            StringOptions = strings;
            BoolOptions = bools;
            IntOptions = ints;
            FloatOptions = floats;
            SpawnLocations = spawns;
            SpawnItems = items;
            SpawnAmmo = ammo;
            Abilities = abilities;
            AbilityCooldowns = cooldowns;
            if (ffRules != null)
            {
                AdvancedFriendlyFireRules = ffRules;
            }

            if (onHitEffects != null)
            {
                OnHitEffects = onHitEffects;
            }

            if (spawnEffects != null)
            {
                OnSpawnEffects = spawnEffects;
            }

            if (cantDamage != null)
            {
                RolesThatCantDamage = cantDamage;
            }

            if (cantDamageTeams != null)
            {
                CantDamageTeams = cantDamageTeams;
            }

            if (teamsThatCantDamage != null)
            {
                TeamsThatCantDamage = teamsThatCantDamage;
            }

            if (cantDamageSubclasses != null)
            {
                CantDamageSubclasses = cantDamageSubclasses;
            }

            if (subclassesThatCantDamage != null)
            {
                SubclassesThatCantDamage = subclassesThatCantDamage;
            }

            if (endsRoundWith != "RIP")
            {
                EndsRoundWith = endsRoundWith;
            }

            if (spawnsAs != RoleType.None)
            {
                SpawnsAs = spawnsAs;
            }

            if (escapesAs != null)
            {
                EscapesAs = escapesAs;
            }

            if (onTakeDamage != null)
            {
                OnDamagedEffects = onTakeDamage;
            }

            if (cantDamageRoles != null)
            {
                CantDamageRoles = cantDamageRoles;
            }

            if (affectsUsers != null)
            {
                AffectsUsers = affectsUsers;
            }

            if (permissions != null)
            {
                Permissions = permissions;
            }

            if (initialAbilityCooldowns != null)
            {
                InitialAbilityCooldowns = initialAbilityCooldowns;
            }

            if (spawnParameters != null)
            {
                SpawnParameters = spawnParameters;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubClass"/> class.
        /// </summary>
        /// <param name="subClass">The <see cref="SubClass"/> that the properties will be used from.</param>
        public SubClass(SubClass subClass)
        {
            Name = subClass.Name;
            AffectsRoles = new List<RoleType>(subClass.AffectsRoles);
            StringOptions = new Dictionary<string, string>(subClass.StringOptions);
            BoolOptions = new Dictionary<string, bool>(subClass.BoolOptions);
            IntOptions = new Dictionary<string, int>(subClass.IntOptions);
            FloatOptions = new Dictionary<string, float>(subClass.FloatOptions);
            SpawnLocations = new List<string>(subClass.SpawnLocations);
            SpawnItems = new Dictionary<int, Dictionary<string, float>>(subClass.SpawnItems);
            SpawnAmmo = new Dictionary<AmmoType, int>(subClass.SpawnAmmo);
            Abilities = new List<AbilityType>(subClass.Abilities);
            AbilityCooldowns = new Dictionary<AbilityType, float>(subClass.AbilityCooldowns);
            AdvancedFriendlyFireRules = new List<string>(subClass.AdvancedFriendlyFireRules);
            OnHitEffects = new List<string>(subClass.OnHitEffects);
            OnSpawnEffects = new List<string>(subClass.OnSpawnEffects);
            RolesThatCantDamage = new List<RoleType>(subClass.RolesThatCantDamage);
            EndsRoundWith = subClass.EndsRoundWith;
            SpawnsAs = subClass.SpawnsAs;
            EscapesAs = subClass.EscapesAs;
            OnDamagedEffects = subClass.OnDamagedEffects;
            CantDamageRoles = subClass.CantDamageRoles;
            AffectsUsers = subClass.AffectsUsers;
            InitialAbilityCooldowns = subClass.InitialAbilityCooldowns;
            Permissions = subClass.Permissions;
            SpawnParameters = subClass.SpawnParameters;
        }

        /// <summary>
        /// Gets the name of the <see cref="SubClass"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the roles that the <see cref="SubClass"/> affects.
        /// </summary>
        public List<RoleType> AffectsRoles { get; }

        /// <summary>
        /// Gets the users that the <see cref="SubClass"/> affects.
        /// </summary>
        public Dictionary<string, float> AffectsUsers { get; }

        /// <summary>
        /// Gets the permissions that the <see cref="SubClass"/> has.
        /// </summary>
        public Dictionary<string, float> Permissions { get; }

        /// <summary>
        /// Gets the spawn parameters of the <see cref="SubClass"/>.
        /// </summary>
        public Dictionary<string, int> SpawnParameters { get; }

        /// <summary>
        /// Gets the string options of the <see cref="SubClass"/>.
        /// </summary>
        public Dictionary<string, string> StringOptions { get; }

        /// <summary>
        /// Gets the bool options of the <see cref="SubClass"/>.
        /// </summary>
        public Dictionary<string, bool> BoolOptions { get; }

        /// <summary>
        /// Gets the int options of the <see cref="SubClass"/>.
        /// </summary>
        public Dictionary<string, int> IntOptions { get; }

        /// <summary>
        /// Gets the float options of the <see cref="SubClass"/>.
        /// </summary>
        public Dictionary<string, float> FloatOptions { get; }

        /// <summary>
        /// Gets the spawn locations of the <see cref="SubClass"/>.
        /// </summary>
        public List<string> SpawnLocations { get; }

        /// <summary>
        /// Gets the items that the <see cref="SubClass"/> spawns with.
        /// </summary>
        public Dictionary<int, Dictionary<string, float>> SpawnItems { get; }

        /// <summary>
        /// Gets the ammo that the <see cref="SubClass"/> spawns with.
        /// </summary>
        public Dictionary<AmmoType, int> SpawnAmmo { get; }

        /// <summary>
        /// Gets the abilities that the <see cref="SubClass"/> has.
        /// </summary>
        public List<AbilityType> Abilities { get; }

        /// <summary>
        /// Gets the ability cooldowns of the <see cref="SubClass"/>.
        /// </summary>
        public Dictionary<AbilityType, float> AbilityCooldowns { get; }

        /// <summary>
        /// Gets the initial ability cooldowns of the <see cref="SubClass"/>.
        /// </summary>
        public Dictionary<AbilityType, float> InitialAbilityCooldowns { get; }

        /// <summary>
        /// Gets the friendly fire rules of the <see cref="SubClass"/>.
        /// </summary>
        public List<string> AdvancedFriendlyFireRules { get; }

        /// <summary>
        /// Gets the effects of the <see cref="SubClass"/> which apply on hit.
        /// </summary>
        public List<string> OnHitEffects { get; }

        /// <summary>
        /// Gets the effects of the <see cref="SubClass"/> which apply on spawning.
        /// </summary>
        public List<string> OnSpawnEffects { get; }

        /// <summary>
        /// Gets the effects of the <see cref="SubClass"/> which apply when damaged.
        /// </summary>
        public Dictionary<string, List<string>> OnDamagedEffects { get; }

        /// <summary>
        /// Gets the roles that can't damage the <see cref="SubClass"/>.
        /// </summary>
        public List<RoleType> RolesThatCantDamage { get; }

        /// <summary>
        /// Gets the roles that the <see cref="SubClass"/> can't damage.
        /// </summary>
        public List<RoleType> CantDamageRoles { get; }

        /// <summary>
        /// Gets the teams that can't damage the <see cref="SubClass"/>.
        /// </summary>
        public List<Team> TeamsThatCantDamage { get; }

        /// <summary>
        /// Gets the teams that the <see cref="SubClass"/> can't damage.
        /// </summary>
        public List<Team> CantDamageTeams { get; }

        /// <summary>
        /// Gets the subclasses that can't damage the <see cref="SubClass"/>.
        /// </summary>
        public List<string> SubclassesThatCantDamage { get; }

        /// <summary>
        /// Gets the subclasses that the <see cref="SubClass"/> can't damage.
        /// </summary>
        public List<string> CantDamageSubclasses { get; }

        /// <summary>
        /// Gets the team that the <see cref="SubClass"/> wins with.
        /// </summary>
        public string EndsRoundWith { get; }

        /// <summary>
        /// Gets the role that the <see cref="SubClass"/> spawns as.
        /// </summary>
        public RoleType SpawnsAs { get; internal set; }

        /// <summary>
        /// Gets the role that the <see cref="SubClass"/> becomes on escape.
        /// </summary>
        public RoleType[] EscapesAs { get; }
    }
}
namespace Subclass
{
    using System.Collections.Generic;
    using Exiled.API.Enums;

    public class SubClass
    {
        public string Name;

        public List<RoleType> AffectsRoles;
        public Dictionary<string, float> AffectsUsers = new Dictionary<string, float>();
        public Dictionary<string, float> Permissions = new Dictionary<string, float>();
        public Dictionary<string, int> SpawnParameters = new Dictionary<string, int>();

        public Dictionary<string, string> StringOptions;

        public Dictionary<string, bool> BoolOptions;

        public Dictionary<string, int> IntOptions;

        public Dictionary<string, float> FloatOptions;

        public List<string> SpawnLocations;

        public Dictionary<int, Dictionary<string, float>> SpawnItems;

        public Dictionary<AmmoType, int> SpawnAmmo;

        public List<AbilityType> Abilities;

        public Dictionary<AbilityType, float> AbilityCooldowns;
        public Dictionary<AbilityType, float> InitialAbilityCooldowns = new Dictionary<AbilityType, float>();

        public List<string> AdvancedFFRules = new List<string>();

        public List<string> OnHitEffects = new List<string>();

        public List<string> OnSpawnEffects = new List<string>();

        public Dictionary<string, List<string>> OnDamagedEffects = new Dictionary<string, List<string>>();

        public List<RoleType> RolesThatCantDamage = new List<RoleType>();
        public List<RoleType> CantDamageRoles = new List<RoleType>();

        public List<Team> CantDamageTeams = new List<Team>();
        public List<Team> TeamsThatCantDamage = new List<Team>();

        public List<string> CantDamageSubclasses = new List<string>();
        public List<string> SubclassesThatCantDamage = new List<string>();

        public string EndsRoundWith = "RIP";

        public RoleType SpawnsAs = RoleType.None;

        public RoleType[] EscapesAs = { RoleType.None, RoleType.None };

        public SubClass(string name, List<RoleType> roles, Dictionary<string, string> strings,
            Dictionary<string, bool> bools,
            Dictionary<string, int> ints, Dictionary<string, float> floats, List<string> spawns,
            Dictionary<int, Dictionary<string, float>> items,
            Dictionary<AmmoType, int> ammo, List<AbilityType> abilities, Dictionary<AbilityType, float> cooldowns,
            List<string> ffRules = null, List<string> onHitEffects = null, List<string> spawnEffects = null,
            List<RoleType> cantDamage = null,
            string endsRoundWith = "RIP", RoleType spawnsAs = RoleType.None, RoleType[] escapesAs = null,
            Dictionary<string, List<string>> onTakeDamage = null,
            List<RoleType> cantDamageRoles = null, List<Team> cantDamageTeams = null,
            List<Team> teamsThatCantDamage = null, List<string> cantDamageSubclasses = null,
            List<string> subclassesThatCantDamage = null,
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
            if (ffRules != null) AdvancedFFRules = ffRules;
            if (onHitEffects != null) OnHitEffects = onHitEffects;
            if (spawnEffects != null) OnSpawnEffects = spawnEffects;
            if (cantDamage != null) RolesThatCantDamage = cantDamage;
            if (cantDamageTeams != null) CantDamageTeams = cantDamageTeams;
            if (teamsThatCantDamage != null) TeamsThatCantDamage = teamsThatCantDamage;
            if (cantDamageSubclasses != null) CantDamageSubclasses = cantDamageSubclasses;
            if (subclassesThatCantDamage != null) SubclassesThatCantDamage = subclassesThatCantDamage;
            if (endsRoundWith != "RIP") EndsRoundWith = endsRoundWith;
            if (spawnsAs != RoleType.None) SpawnsAs = spawnsAs;
            if (escapesAs != null) EscapesAs = escapesAs;
            if (onTakeDamage != null) OnDamagedEffects = onTakeDamage;
            if (cantDamageRoles != null) CantDamageRoles = cantDamageRoles;
            if (affectsUsers != null) AffectsUsers = affectsUsers;
            if (permissions != null) Permissions = permissions;
            if (initialAbilityCooldowns != null) InitialAbilityCooldowns = initialAbilityCooldowns;
            if (spawnParameters != null) SpawnParameters = spawnParameters;
        }

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
            AdvancedFFRules = new List<string>(subClass.AdvancedFFRules);
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
    }
}
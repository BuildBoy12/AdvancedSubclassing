namespace Subclass.Managers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using YamlDotNet.Core;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization.NodeDeserializers;

    public static class SubclassManager
    {
        private static ISerializer Serializer { get; } = new SerializerBuilder()
            .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
            .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreFields()
            .Build();

        private static IDeserializer Deserializer { get; } = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithNodeDeserializer(inner => new ValidatingNodeDeserializer(inner), deserializer => deserializer.InsteadOf<ObjectNodeDeserializer>())
            .IgnoreFields()
            .IgnoreUnmatchedProperties()
            .Build();

        public static Dictionary<string, SubClass> LoadClasses()
        {
            try
            {
                Log.Info("Loading classes...");

                if (!Directory.Exists(Path.Combine(Paths.Configs, "Subclasses")))
                {
                    Log.Info("Subclasses directory not found, creating.");
                    Directory.CreateDirectory(Path.Combine(Paths.Configs, "Subclasses"));
                    Directory.CreateDirectory(Path.Combine(Paths.Configs, "Subclasses", "global"));
                    Directory.CreateDirectory(Path.Combine(Paths.Configs, "Subclasses", Server.Port.ToString()));
                    return new Dictionary<string, SubClass>();
                }

                if (!Directory.Exists(Path.Combine(Paths.Configs, "Subclasses", "global")))
                {
                    Log.Info("Subclasses global directory not found, creating.");
                    Directory.CreateDirectory(Path.Combine(Paths.Configs, "Subclasses", "global"));
                    Directory.CreateDirectory(Path.Combine(Paths.Configs, "Subclasses", Server.Port.ToString()));
                    return new Dictionary<string, SubClass>();
                }

                if (!Directory.Exists(Path.Combine(Paths.Configs, "Subclasses", Server.Port.ToString())))
                {
                    Log.Info("Subclasses directory for this port not found, creating.");
                    Directory.CreateDirectory(Path.Combine(Paths.Configs, "Subclasses", Server.Port.ToString()));
                    return new Dictionary<string, SubClass>();
                }

                List<string> classes = new List<string>();
                classes.AddRange(Directory.GetFiles(Path.Combine(Paths.Configs, "Subclasses", "global")));
                foreach (string directory in Directory.GetDirectories(Path.Combine(Paths.Configs, "Subclasses", "global")))
                {
                    classes.AddRange(
                        Directory.GetFiles(Path.Combine(Paths.Configs, "Subclasses", "global", directory)));
                }

                classes.AddRange(Directory.GetFiles(Path.Combine(Paths.Configs, "Subclasses", Server.Port.ToString())));
                foreach (string directory in Directory.GetDirectories(Path.Combine(Paths.Configs, "Subclasses", Server.Port.ToString())))
                {
                    classes.AddRange(Directory.GetFiles(Path.Combine(Paths.Configs, "Subclasses", Server.Port.ToString(), directory)));
                }

                Dictionary<string, SubClass> subClasses = new Dictionary<string, SubClass>();

                foreach (string path in classes.Where(f => f.EndsWith("yml")))
                {
                    string file = Read(path);
                    Dictionary<string, object> rawClass = Deserializer.Deserialize<Dictionary<string, object>>(file) ?? new Dictionary<string, object>();
                    try
                    {
                        Dictionary<string, object> obj = (Dictionary<string, object>)Deserializer.Deserialize(Serializer.Serialize(rawClass), typeof(Dictionary<string, object>));
                        Log.Debug($"Attempting to load class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);

                        Log.Debug($"Attempting to load bool options for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        Dictionary<object, object> boolOptionsTemp =
                            (Dictionary<object, object>)obj["boolean_options"];
                        Dictionary<string, bool> boolOptions = new Dictionary<string, bool>();
                        foreach (var item in boolOptionsTemp)
                        {
                            boolOptions.Add((string)item.Key, bool.Parse((string)item.Value));
                        }

                        if (!boolOptions["Enabled"])
                        {
                            Log.Debug($"Class named {(string)obj["name"]} not loaded. Enabled is set to false", Plugin.Instance.Config.Debug);
                            continue;
                        }

                        Log.Debug($"Attempting to load ff rules for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        List<string> ffRules = obj.ContainsKey("advanced_ff_rules") ? ((IEnumerable<object>)obj["advanced_ff_rules"]).Cast<string>().ToList() : null;

                        Log.Debug($"Attempting to load on hit effects for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        List<string> onHitEffects = obj.ContainsKey("on_hit_effects") ? ((IEnumerable<object>)obj["on_hit_effects"]).Cast<string>().ToList() : null;

                        Log.Debug($"Attempting to load on spawn effects for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        List<string> onSpawnEffects = obj.ContainsKey("on_spawn_effects") ? ((IEnumerable<object>)obj["on_spawn_effects"]).Cast<string>().ToList() : null;

                        Log.Debug($"Attempting to load on damaged effects for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        Dictionary<string, List<string>> onTakeDamage = new Dictionary<string, List<string>>();
                        if (obj.ContainsKey("on_damaged_effects"))
                        {
                            Dictionary<object, object> onTakeDamageTemp =
                                (Dictionary<object, object>)obj["on_damaged_effects"];
                            foreach (var item in onTakeDamageTemp)
                            {
                                onTakeDamage.Add(((string)item.Key).ToUpper(), ((IEnumerable<object>)item.Value).Cast<string>().ToList());
                            }
                        }

                        Log.Debug($"Attempting to load ends round with for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        string endsRoundWith = obj.ContainsKey("ends_round_with") ? (string)obj["ends_round_with"] : "RIP";

                        Log.Debug($"Attempting to load roles that cant damage for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        List<string> cantDamageTemp = obj.ContainsKey("roles_that_cant_damage") ? ((IEnumerable<object>)obj["roles_that_cant_damage"]).Cast<string>().ToList() : null;
                        List<RoleType> cantDamage = new List<RoleType>();
                        if (cantDamageTemp != null)
                        {
                            foreach (string role in cantDamageTemp)
                            {
                                cantDamage.Add((RoleType)Enum.Parse(typeof(RoleType), role));
                            }
                        }

                        List<RoleType> cantDamageRoles = new List<RoleType>();

                        if (obj.ContainsKey("cant_damage"))
                        {
                            Log.Debug($"Attempting to load cant damage roles for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                            List<string> cantDamageRolesTemp = ((IEnumerable<object>)obj["cant_damage"]).Cast<string>().ToList();
                            foreach (string role in cantDamageRolesTemp)
                            {
                                cantDamageRoles.Add((RoleType)Enum.Parse(typeof(RoleType), role));
                            }
                        }

                        List<Team> cantDamageTeams = new List<Team>();

                        if (obj.ContainsKey("cant_damage_teams"))
                        {
                            Log.Debug($"Attempting to load cant damage teams for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                            List<string> cantDamageTeamsTemp = ((IEnumerable<object>)obj["cant_damage_teams"]).Cast<string>().ToList();
                            foreach (string team in cantDamageTeamsTemp)
                            {
                                cantDamageTeams.Add((Team)Enum.Parse(typeof(Team), team));
                            }
                        }

                        List<Team> teamsThatCantDamage = new List<Team>();
                        if (obj.ContainsKey("teams_that_cant_damage"))
                        {
                            Log.Debug($"Attempting to load teams that cant damage for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                            List<string> teamsThatCantDamageTemp = ((IEnumerable<object>)obj["teams_that_cant_damage"]).Cast<string>().ToList();
                            foreach (string team in teamsThatCantDamageTemp)
                            {
                                teamsThatCantDamage.Add((Team)Enum.Parse(typeof(Team), team));
                            }
                        }

                        List<string> cantDamageSubclasses = new List<string>();

                        if (obj.ContainsKey("cant_damage_subclasses"))
                        {
                            Log.Debug($"Attempting to load cant damage subclasses for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                            cantDamageSubclasses = ((IEnumerable<object>)obj["cant_damage_subclasses"]).Cast<string>().ToList();
                        }

                        List<string> subclassesThatCantDamage = new List<string>();

                        if (obj.ContainsKey("subclasses_that_cant_damage"))
                        {
                            Log.Debug($"Attempting to load subclasses that cant damage for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                            subclassesThatCantDamage = ((IEnumerable<object>)obj["subclasses_that_cant_damage"]).Cast<string>().ToList();
                        }

                        Log.Debug($"Attempting to load affects roles for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        List<string> affectsRolesTemp = ((IEnumerable<object>)obj["affects_roles"]).Cast<string>().ToList();
                        List<RoleType> affectsRoles = new List<RoleType>();
                        foreach (string role in affectsRolesTemp)
                        {
                            affectsRoles.Add((RoleType)Enum.Parse(typeof(RoleType), role));
                        }

                        Dictionary<string, float> affectsUsers = new Dictionary<string, float>();
                        if (obj.ContainsKey("affects_users"))
                        {
                            Log.Debug($"Attempting to load affects users for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                            Dictionary<object, object> affectsUsersTemp = (Dictionary<object, object>)obj["affects_users"];
                            foreach (var item in affectsUsersTemp)
                            {
                                affectsUsers.Add((string)item.Key, float.Parse((string)item.Value));
                            }
                        }

                        Dictionary<string, float> permissions = new Dictionary<string, float>();
                        if (obj.ContainsKey("any_permissions"))
                        {
                            Log.Debug($"Attempting to load any permissions for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                            Dictionary<object, object> permissionsTemp = (Dictionary<object, object>)obj["any_permissions"];
                            foreach (var item in permissionsTemp)
                            {
                                permissions.Add((string)item.Key, float.Parse((string)item.Value));
                            }
                        }

                        Log.Debug($"Attempting to load string options for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        Dictionary<object, object> stringOptionsTemp = (Dictionary<object, object>)obj["string_options"];
                        Dictionary<string, string> stringOptions = new Dictionary<string, string>();
                        foreach (var item in stringOptionsTemp)
                        {
                            stringOptions.Add((string)item.Key, (string)item.Value);
                        }

                        Log.Debug($"Attempting to load int options for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        Dictionary<object, object> intOptionsTemp = (Dictionary<object, object>)obj["integer_options"];
                        Dictionary<string, int> intOptions = new Dictionary<string, int>();
                        foreach (var item in intOptionsTemp)
                        {
                            intOptions.Add((string)item.Key, int.Parse((string)item.Value));
                        }

                        Log.Debug($"Attempting to load float options for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        Dictionary<object, object> floatOptionsTemp = (Dictionary<object, object>)obj["float_options"];
                        Dictionary<string, float> floatOptions = new Dictionary<string, float>();
                        foreach (var item in floatOptionsTemp)
                        {
                            floatOptions.Add((string)item.Key, float.Parse((string)item.Value));
                        }

                        Log.Debug($"Attempting to load spawns for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        List<string> spawns = ((IEnumerable<object>)obj["spawn_locations"]).Cast<string>().ToList();

                        Log.Debug($"Attempting to load spawn items for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        Dictionary<object, object> spawnItemsTemp = (Dictionary<object, object>)obj["spawn_items"];
                        Dictionary<int, Dictionary<string, float>> spawnItems = new Dictionary<int, Dictionary<string, float>>();
                        foreach (var item in spawnItemsTemp)
                        {
                            spawnItems.Add(int.Parse((string)item.Key), new Dictionary<string, float>());
                            foreach (var item2 in (Dictionary<object, object>)spawnItemsTemp[item.Key])
                            {
                                spawnItems[int.Parse((string)item.Key)].Add((string)item2.Key, float.Parse((string)item2.Value));
                            }
                        }

                        Log.Debug($"Attempting to load spawn ammo for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        Dictionary<object, object> ammoTemp = (Dictionary<object, object>)obj["spawn_ammo"];
                        Dictionary<AmmoType, int> ammo = new Dictionary<AmmoType, int>();
                        foreach (var item in ammoTemp)
                        {
                            ammo.Add((AmmoType)Enum.Parse(typeof(AmmoType), (string)item.Key), int.Parse((string)item.Value));
                        }

                        Log.Debug($"Attempting to load abilities for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        List<string> abilitiesTemp = ((IEnumerable<object>)obj["abilities"]).Cast<string>().ToList();
                        List<AbilityType> abilities = new List<AbilityType>();
                        foreach (string ability in abilitiesTemp)
                        {
                            abilities.Add((AbilityType)Enum.Parse(typeof(AbilityType), ability));
                        }

                        Log.Debug($"Attempting to load ability cooldowns for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        Dictionary<object, object> abilityCooldownsTemp = (Dictionary<object, object>)obj["ability_cooldowns"];
                        Dictionary<AbilityType, float> abilityCooldowns = new Dictionary<AbilityType, float>();
                        foreach (var item in abilityCooldownsTemp)
                        {
                            abilityCooldowns.Add((AbilityType)Enum.Parse(typeof(AbilityType), (string)item.Key), float.Parse((string)item.Value));
                        }

                        Log.Debug($"Attempting to load ability cooldowns for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        Dictionary<AbilityType, float> initialAbilityCooldowns = new Dictionary<AbilityType, float>();
                        if (obj.ContainsKey("initial_ability_cooldowns"))
                        {
                            Dictionary<object, object> initialAbilityCooldownsTemp = (Dictionary<object, object>)obj["ability_cooldowns"];
                            foreach (var item in initialAbilityCooldownsTemp)
                            {
                                initialAbilityCooldowns.Add((AbilityType)Enum.Parse(typeof(AbilityType), (string)item.Key), float.Parse((string)item.Value));
                            }
                        }

                        Log.Debug($"Attempting to load spawns as for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        RoleType spawnsAs = obj.ContainsKey("spawns_as") ? (RoleType)Enum.Parse(typeof(RoleType), (string)obj["spawns_as"]) : RoleType.None;

                        Log.Debug($"Attempting to load escapes as for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                        RoleType[] escapesAs = { RoleType.None, RoleType.None };
                        if (obj.ContainsKey("escapes_as"))
                        {
                            Dictionary<object, object> escapesAsTemp = (Dictionary<object, object>)obj["escapes_as"];
                            if (escapesAsTemp.ContainsKey("not_cuffed"))
                            {
                                escapesAs[0] = (RoleType)Enum.Parse(typeof(RoleType), (string)escapesAsTemp["not_cuffed"]);
                            }
                            else
                            {
                                escapesAs[0] = RoleType.None;
                            }

                            if (escapesAsTemp.ContainsKey("cuffed"))
                            {
                                escapesAs[1] = (RoleType)Enum.Parse(typeof(RoleType), (string)escapesAsTemp["cuffed"]);
                            }
                            else
                            {
                                escapesAs[1] = RoleType.None;
                            }
                        }

                        Dictionary<string, int> spawnParameters = new Dictionary<string, int>();
                        if (obj.ContainsKey("spawn_parameters"))
                        {
                            Log.Debug($"Attempting to load spawn parameters for class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                            Dictionary<object, object> spawnParametersTemp = (Dictionary<object, object>)obj["spawn_parameters"];
                            foreach (var item in spawnParametersTemp)
                            {
                                spawnParameters.Add((string)item.Key, int.Parse((string)item.Value));
                            }
                        }

                        subClasses.Add((string)obj["name"], new SubClass((string)obj["name"], affectsRoles, stringOptions, boolOptions, intOptions, floatOptions, spawns, spawnItems, ammo, abilities, abilityCooldowns, ffRules, onHitEffects, onSpawnEffects, cantDamage, endsRoundWith, spawnsAs, escapesAs, onTakeDamage, cantDamageRoles, cantDamageTeams, teamsThatCantDamage, cantDamageSubclasses, subclassesThatCantDamage, affectsUsers, permissions, initialAbilityCooldowns, spawnParameters));
                        Log.Debug($"Successfully loaded class: {(string)obj["name"]}", Plugin.Instance.Config.Debug);
                    }
                    catch (YamlException yamlException)
                    {
                        Log.Error($"Class with path: {path} could not be loaded Skipping. {yamlException}");
                    }
                    catch (FormatException e)
                    {
                        Log.Error($"Class with path: {path} could not be loaded due to a format exception. {e}\nBegin stack trace:\n{e.StackTrace}");
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Class with path: {path} could not be loaded. {e}\nBegin stack trace:\n{e.StackTrace}");
                    }
                }

                Log.Info("Classes loaded successfully!");

                return subClasses;
            }
            catch (Exception exception)
            {
                Log.Error($"An error has occurred while loading subclasses! {exception}\nBegin stack trace:\n{exception.StackTrace}");
                return null;
            }
        }

        private static string Read(string path)
        {
            try
            {
                if (File.Exists(Path.Combine(Path.Combine(Paths.Configs, Path.Combine("Subclasses", path)))))
                {
                    return File.ReadAllText(Path.Combine(Paths.Configs, Path.Combine("Subclasses", path)));
                }
            }
            catch (Exception exception)
            {
                Log.Error($"An error has occurred while reading class from {Paths.Configs} path: {exception}\nBegin stack trace:\n{exception.StackTrace}");
            }

            return string.Empty;
        }
    }
}
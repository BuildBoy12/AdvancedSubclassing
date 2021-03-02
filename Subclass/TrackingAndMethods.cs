namespace Subclass
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CustomPlayerEffects;
    using Effects;
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.Loader;
    using Exiled.Permissions.Extensions;
    using Interactables.Interobjects.DoorUtils;
    using MEC;
    using MonoBehaviours;
    using UnityEngine;

    public static class TrackingAndMethods
    {
        public static readonly List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

        public static readonly Dictionary<RoleType, int> RolesForClass = new Dictionary<RoleType, int>();

        public static readonly Dictionary<SubClass, int> SubClassesSpawned = new Dictionary<SubClass, int>();

        public static Dictionary<Player, SubClass> PlayersWithSubclasses = new Dictionary<Player, SubClass>();

        public static readonly Dictionary<Player, Dictionary<AbilityType, float>> Cooldowns = new Dictionary<Player, Dictionary<AbilityType, float>>();

        public static readonly Dictionary<Player, Dictionary<AbilityType, int>> AbilityUses = new Dictionary<Player, Dictionary<AbilityType, int>>();

        public static readonly Dictionary<Player, float> PlayersThatBypassedTeslaGates = new Dictionary<Player, float>();

        public static Dictionary<Player, float> PlayersThatJustGotAClass = new Dictionary<Player, float>();

        public static readonly Dictionary<Player, int> Zombie106Kills = new Dictionary<Player, int>();

        public static readonly Dictionary<Player, List<Player>> PlayersWithZombies = new Dictionary<Player, List<Player>>();

        public static readonly Dictionary<Player, RoleType> PreviousRoles = new Dictionary<Player, RoleType>();
        public static readonly Dictionary<Player, SubClass> PreviousSubclasses = new Dictionary<Player, SubClass>();

        public static readonly Dictionary<uint, RoleType> RagdollRoles = new Dictionary<uint, RoleType>();

        public static readonly List<Player> FriendlyFired = new List<Player>();

        public static readonly List<Player> PlayersInvisibleByCommand = new List<Player>();
        public static readonly List<Player> PlayersVenting = new List<Player>();

        public static readonly List<Player> PlayersBloodLusting = new List<Player>();

        public static readonly List<string> QueuedCassieMessages = new List<string>();

        public static float RoundStartedAt = 0f;

        public static List<Player> NextSpawnWave = new List<Player>();

        public static readonly Dictionary<RoleType, SubClass> NextSpawnWaveGetsRole = new Dictionary<RoleType, SubClass>();

        public static readonly Dictionary<Team, int> NumSpawnWaves = new Dictionary<Team, int>();
        public static readonly List<SubClass> SpawnWaveSpawns = new List<SubClass>();
        public static readonly Dictionary<SubClass, int> ClassesGiven = new Dictionary<SubClass, int>();
        public static readonly List<SubClass> DontGiveClasses = new List<SubClass>();

        public static readonly Dictionary<Player, string> PreviousBadges = new Dictionary<Player, string>();

        public static readonly List<Tuple<MethodInfo, MethodInfo>> CustomWeaponGetters = new List<Tuple<MethodInfo, MethodInfo>>();

        private static readonly System.Random Random = new System.Random();


        public static void MaybeAddRoles(Player player, bool is035 = false, bool escaped = false)
        {
            if (!Round.IsStarted || IsGhost(player))
            {
                return;
            }

            if (!RolesForClass.ContainsKey(player.Role))
            {
                RolesForClass.Add(player.Role, Plugin.Instance.Classes.Values.Count(e => e.BoolOptions["Enabled"] && e.AffectsRoles.Contains(player.Role)));
            }

            if (RolesForClass[player.Role] > 0)
            {
                List<string> teamsAlive = GetTeamsAlive();

                bool gotUniqueClass = CheckUserClass(player, is035, escaped, teamsAlive) ||
                                      CheckPermissionClass(player, is035, escaped, teamsAlive);

                if (gotUniqueClass)
                {
                    return;
                }

                switch (Plugin.Instance.Config.AdditiveChance)
                {
                    case false when !Plugin.Instance.Config.WeightedChance:
                        CheckNormalSubclass(player, escaped, is035, teamsAlive);
                        break;
                    case true:
                        CheckOtherClass(player, escaped, is035, teamsAlive, true);
                        break;
                    default:
                        CheckOtherClass(player, escaped, is035, teamsAlive, false);
                        break;
                }
            }
            else
            {
                Log.Debug($"No subclasses for {player.Role}", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
            }
        }

        public static void AddClass(Player player, SubClass subClass, bool is035 = false, bool lite = false, bool escaped = false, bool disguised = false)
        {
            if (player == null)
            {
                return;
            }

            var ev = new Events.EventArgs.ReceivingSubclassEventArgs(player, subClass);
            Events.Handlers.Player.OnReceivingSubclass(ev);
            if (!ev.IsAllowed)
            {
                Log.Debug(
                    $"Player with name {player.Nickname} unable to get {subClass.Name}. 3rd party plugin cancelled event.",
                    Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                return;
            }

            subClass = ev.Subclass;
            if (is035)
            {
                SubClass copy = new SubClass(subClass);
                if (!copy.Abilities.Contains(AbilityType.Disable096Trigger))
                {
                    copy.Abilities.Add(AbilityType.Disable096Trigger);
                }

                if (!copy.Abilities.Contains(AbilityType.Disable173Stop))
                {
                    copy.Abilities.Add(AbilityType.Disable173Stop);
                }

                if (!copy.Abilities.Contains(AbilityType.NoSCPDamage))
                {
                    copy.Abilities.Add(AbilityType.NoSCPDamage);
                }

                copy.BoolOptions["HasFriendlyFire"] = true;
                copy.BoolOptions["TakesFriendlyFire"] = true;
                copy.SpawnsAs = RoleType.None;
                copy.SpawnLocations.Clear();
                copy.SpawnLocations.Add("Unknown");
                copy.IntOptions["MaxHealth"] = -1;
                copy.IntOptions["HealthOnSpawn"] = -1;
                copy.IntOptions["MaxArmor"] = -1;
                copy.IntOptions["ArmorOnSpawn"] = -1;
                copy.SpawnItems.Clear();
                copy.RolesThatCantDamage.Clear();
                copy.StringOptions["GotClassMessage"] = subClass.StringOptions["GotClassMessage"] + " You are SCP-035.";
                copy.CantDamageRoles.Clear();

                subClass = new SubClass(copy.Name + "-SCP-035 (p)", copy.AffectsRoles, copy.StringOptions, copy.BoolOptions, copy.IntOptions, copy.FloatOptions, copy.SpawnLocations, copy.SpawnItems, new Dictionary<AmmoType, int> { { AmmoType.Nato556, -1 }, { AmmoType.Nato762, -1 }, { AmmoType.Nato9, -1 } }, copy.Abilities, copy.AbilityCooldowns, copy.AdvancedFFRules, copy.OnHitEffects, copy.OnSpawnEffects, copy.RolesThatCantDamage, "SCP", RoleType.None, null, subClass.OnDamagedEffects);
            }

            if (NextSpawnWave.Contains(player) && NextSpawnWaveGetsRole.ContainsKey(player.Role) &&
                !SpawnWaveSpawns.Contains(subClass))
            {
                if (SubClassesSpawned.ContainsKey(subClass))
                {
                    SubClassesSpawned[subClass]++;
                }
                else
                {
                    SubClassesSpawned.Add(subClass, 1);
                }

                SpawnWaveSpawns.Add(subClass);
            }
            else if (!SpawnWaveSpawns.Contains(subClass))
            {
                if (SubClassesSpawned.ContainsKey(subClass))
                {
                    SubClassesSpawned[subClass]++;
                }
                else
                {
                    SubClassesSpawned.Add(subClass, 1);
                }
            }

            if (!disguised)
            {
                PlayersWithSubclasses.Add(player, subClass);
            }

            int spawnIndex = Random.Next(subClass.SpawnLocations.Count);
            List<Vector3> spawnLocations = new List<Vector3>();
            if (subClass.SpawnLocations.Contains("Lcz173Armory"))
            {
                DoorVariant door = DoorNametagExtension.NamedDoors["173_ARMORY"].TargetDoor;
                spawnLocations.Add(door.transform.position + new Vector3(1f, 0, 1f));
            }

            if (subClass.SpawnLocations.Contains("Lcz173Connector"))
            {
                DoorVariant door = DoorNametagExtension.NamedDoors["173_CONNECTOR"].TargetDoor;
                spawnLocations.Add(door.transform.position + new Vector3(1f, 0, 1f));
            }

            if (subClass.SpawnLocations.Contains("Lcz173"))
            {
                DoorVariant door = DoorNametagExtension.NamedDoors["173_GATE"].TargetDoor;
                spawnLocations.Add(door.transform.position + new Vector3(1f, 0, 1f));
            }

            if (subClass.SpawnLocations.Contains("Lcz173Bottom"))
            {
                DoorVariant door = DoorNametagExtension.NamedDoors["173_BOTTOM"].TargetDoor;
                spawnLocations.Add(door.transform.position + new Vector3(1f, 0, 1f));
            }

            spawnLocations.AddRange(Map.Rooms.Where(r => subClass.SpawnLocations.Contains(r.Type.ToString()))
                .Select(r => r.Transform.position));

            int tries = 0;
            while (!(subClass.SpawnLocations[spawnIndex] == "Unknown" ||
                     subClass.SpawnLocations[spawnIndex] == "Lcz173Armory" ||
                     subClass.SpawnLocations[spawnIndex] == "Lcz173"
                     || subClass.SpawnLocations[spawnIndex] == "Lcz173Connector" ||
                     subClass.SpawnLocations[spawnIndex] == "Lcz173Bottom")
                   && Map.Rooms.All(r => r.Type.ToString() != subClass.SpawnLocations[spawnIndex]))
            {
                spawnIndex = Random.Next(subClass.SpawnLocations.Count);
                tries++;
                if (tries > subClass.SpawnLocations.Count)
                {
                    spawnIndex = -1;
                    break;
                }
            }

            try
            {
                player.Broadcast(
                    subClass.FloatOptions.ContainsKey("BroadcastTimer")
                        ? (ushort)subClass.FloatOptions["BroadcastTimer"]
                        : (ushort)Plugin.Instance.Config.GlobalBroadcastTime,
                    subClass.StringOptions["GotClassMessage"]);

                if (subClass.StringOptions.ContainsKey("CassieAnnouncement") &&
                    !QueuedCassieMessages.Contains(subClass.StringOptions["CassieAnnouncement"]))
                {
                    QueuedCassieMessages.Add(subClass.StringOptions["CassieAnnouncement"]);
                }

                if ((!lite || escaped) && subClass.SpawnsAs != RoleType.None)
                {
                    player.SetRole(subClass.SpawnsAs, true);
                }

                if ((!lite || escaped) && subClass.SpawnItems.Count != 0)
                {
                    player.Inventory.items.Clear();
                    foreach (var item in subClass.SpawnItems)
                    {
                        foreach (var item2 in item.Value)
                        {
                            if (Random.NextDouble() * 100 < subClass.SpawnItems[item.Key][item2.Key])
                            {
                                if (item2.Key == "None")
                                {
                                    break;
                                }

                                if (Enum.TryParse(item2.Key, out ItemType theItem))
                                {
                                    player.AddItem(theItem);
                                }
                                else
                                {
                                    Inventory.SyncItemInfo syncItem = new Inventory.SyncItemInfo { id = ItemType.None };
                                    int counter = 0;
                                    foreach (var methods in CustomWeaponGetters)
                                    {
                                        try
                                        {
                                            Inventory.SyncItemInfo gotItem = (Inventory.SyncItemInfo)methods.Item1.Invoke(null, new object[] { item2.Key });
                                            if (gotItem != null && gotItem.id != ItemType.None)
                                            {
                                                syncItem = gotItem;
                                                break;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Error(
                                                $"Error getting custom weapon: Begin stack trace:\n{e.StackTrace}");
                                        }

                                        counter++;
                                    }

                                    if (syncItem.id == ItemType.None)
                                    {
                                        Log.Error($"Subclass with name: {subClass.Name} has an improper spawn item value: {item2.Key}");
                                    }
                                    else
                                    {
                                        player.AddItem(syncItem);
                                        CustomWeaponGetters[counter].Item2.Invoke(null, new object[] { player, item2.Key, player.Inventory.items.Last() });
                                    }
                                }

                                break;
                            }
                        }
                    }
                }

                if (subClass.IntOptions["MaxHealth"] != -1)
                {
                    player.MaxHealth = subClass.IntOptions["MaxHealth"];
                }

                if ((!lite || escaped) && subClass.IntOptions["HealthOnSpawn"] != -1)
                {
                    player.Health = subClass.IntOptions["HealthOnSpawn"];
                }

                if (subClass.IntOptions["MaxArmor"] != -1)
                {
                    player.MaxAdrenalineHealth = subClass.IntOptions["MaxArmor"];
                }

                if ((!lite || escaped) && subClass.IntOptions["ArmorOnSpawn"] != -1)
                {
                    player.AdrenalineHealth = subClass.IntOptions["ArmorOnSpawn"];
                }

                Timing.CallDelayed(0.3f, () =>
                {
                    Vector3 scale = new Vector3(player.Scale.x, player.Scale.y, player.Scale.z);

                    if (subClass.FloatOptions.ContainsKey("ScaleX"))
                    {
                        scale.x = subClass.FloatOptions["ScaleX"];
                    }

                    if (subClass.FloatOptions.ContainsKey("ScaleY"))
                    {
                        scale.y = subClass.FloatOptions["ScaleY"];
                    }

                    if (subClass.FloatOptions.ContainsKey("ScaleZ"))
                    {
                        scale.z = subClass.FloatOptions["ScaleZ"];
                    }

                    player.Scale = scale;
                });

                if (!subClass.BoolOptions["DisregardHasFF"])
                {
                    player.IsFriendlyFireEnabled = subClass.BoolOptions["HasFriendlyFire"];
                }
            }
            catch (KeyNotFoundException)
            {
                Log.Error($"A required option was not provided. Class: {subClass.Name}");
            }

            if (subClass.StringOptions.ContainsKey("Nickname"))
            {
                player.DisplayNickname = subClass.StringOptions["Nickname"].Replace("{name}", player.Nickname);
            }

            if (subClass.Abilities.Contains(AbilityType.GodMode))
            {
                player.IsGodModeEnabled = true;
            }

            if (subClass.Abilities.Contains(AbilityType.InvisibleUntilInteract))
            {
                player.ReferenceHub.playerEffectsController.EnableEffect<Scp268>();
            }

            if (subClass.Abilities.Contains(AbilityType.InfiniteSprint))
            {
                player.GameObject.AddComponent<InfiniteSprint>();
            }

            if (subClass.Abilities.Contains(AbilityType.Disable173Stop))
            {
                Scp173.TurnedPlayers.Add(player);
            }

            if (subClass.Abilities.Contains(AbilityType.Scp939Vision))
            {
                Timing.CallDelayed(0.3f, () =>
                {
                    Visuals939 visuals = player.ReferenceHub.playerEffectsController.GetEffect<Visuals939>();
                    visuals.Intensity = 3;
                    player.ReferenceHub.playerEffectsController.EnableEffect(visuals);
                });
            }

            if (subClass.Abilities.Contains(AbilityType.NoArmorDecay))
            {
                player.ReferenceHub.playerStats.artificialHpDecay = 0f;
            }

            if ((!lite || escaped) && subClass.SpawnAmmo[AmmoType.Nato556] != -1)
            {
                player.Ammo[(int)AmmoType.Nato556] = (uint)subClass.SpawnAmmo[AmmoType.Nato556];
            }

            if ((!lite || escaped) && subClass.SpawnAmmo[AmmoType.Nato762] != -1)
            {
                player.Ammo[(int)AmmoType.Nato762] = (uint)subClass.SpawnAmmo[AmmoType.Nato762];
            }

            if ((!lite || escaped) && subClass.SpawnAmmo[AmmoType.Nato9] != -1)
            {
                player.Ammo[(int)AmmoType.Nato9] = (uint)subClass.SpawnAmmo[AmmoType.Nato9];
            }

            if (subClass.Abilities.Contains(AbilityType.InfiniteAmmo))
            {
                player.Ammo[0] = uint.MaxValue;
                player.Ammo[1] = uint.MaxValue;
                player.Ammo[2] = uint.MaxValue;
            }

            if (subClass.Abilities.Contains(AbilityType.HealAura))
            {
                bool affectSelf = !subClass.BoolOptions.ContainsKey("HealAuraAffectsSelf") || subClass.BoolOptions["HealAuraAffectsSelf"];
                bool affectAllies = !subClass.BoolOptions.ContainsKey("HealAuraAffectsAllies") || subClass.BoolOptions["HealAuraAffectsAllies"];
                bool affectEnemies = subClass.BoolOptions.ContainsKey("HealAuraAffectsEnemies") && subClass.BoolOptions["HealAuraAffectsEnemies"];

                float healthPerTick = subClass.FloatOptions.ContainsKey("HealAuraHealthPerTick")
                    ? subClass.FloatOptions["HealAuraHealthPerTick"]
                    : 5f;
                float radius = subClass.FloatOptions.ContainsKey("HealAuraRadius")
                    ? subClass.FloatOptions["HealAuraRadius"]
                    : 4f;
                float tickRate = subClass.FloatOptions.ContainsKey("HealAuraTickRate")
                    ? subClass.FloatOptions["HealAuraTickRate"]
                    : 5f;

                player.ReferenceHub.playerEffectsController.AllEffects.Add(typeof(HealAura), new HealAura(player.ReferenceHub, healthPerTick, radius, affectSelf, affectAllies, affectEnemies, tickRate));
                Timing.CallDelayed(0.5f, () => { player.ReferenceHub.playerEffectsController.EnableEffect<HealAura>(float.MaxValue); });
            }

            if (subClass.Abilities.Contains(AbilityType.DamageAura))
            {
                bool affectSelf = subClass.BoolOptions.ContainsKey("DamageAuraAffectsSelf") && subClass.BoolOptions["DamageAuraAffectsSelf"];
                bool affectAllies = subClass.BoolOptions.ContainsKey("DamageAuraAffectsAllies") && subClass.BoolOptions["DamageAuraAffectsAllies"];
                bool affectEnemies = !subClass.BoolOptions.ContainsKey("DamageAuraAffectsEnemies") || subClass.BoolOptions["DamageAuraAffectsEnemies"];

                float healthPerTick = subClass.FloatOptions.ContainsKey("DamageAuraDamagePerTick")
                    ? subClass.FloatOptions["DamageAuraDamagePerTick"]
                    : 5f;
                float radius = subClass.FloatOptions.ContainsKey("DamageAuraRadius")
                    ? subClass.FloatOptions["DamageAuraRadius"]
                    : 4f;
                float tickRate = subClass.FloatOptions.ContainsKey("DamageAuraTickRate")
                    ? subClass.FloatOptions["DamageAuraTickRate"]
                    : 5f;

                player.ReferenceHub.playerEffectsController.AllEffects.Add(typeof(DamageAura), new DamageAura(player.ReferenceHub, healthPerTick, radius, affectSelf, affectAllies, affectEnemies, tickRate));
                Timing.CallDelayed(0.5f, () => { player.ReferenceHub.playerEffectsController.EnableEffect<DamageAura>(float.MaxValue); });
            }

            if (subClass.Abilities.Contains(AbilityType.Regeneration))
            {
                float healthPerTick = subClass.FloatOptions.ContainsKey("RegenerationHealthPerTick")
                    ? subClass.FloatOptions["RegenerationHealthPerTick"]
                    : 2f;
                float tickRate = subClass.FloatOptions.ContainsKey("RegenerationTickRate")
                    ? subClass.FloatOptions["RegenerationTickRate"]
                    : 5f;

                player.ReferenceHub.playerEffectsController.AllEffects.Add(typeof(Regeneration), new Regeneration(player.ReferenceHub, healthPerTick, tickRate));
                Timing.CallDelayed(0.5f, () => { player.ReferenceHub.playerEffectsController.EnableEffect<Regeneration>(float.MaxValue); });
            }

            if (subClass.Abilities.Contains(AbilityType.Multiply))
            {
                TheyMultiply(player, subClass);
            }

            if (!lite || escaped)
            {
                foreach (var cooldown in subClass.InitialAbilityCooldowns)
                {
                    AddCooldown(player, cooldown.Key, true);
                }
            }

            if (!is035)
            {
                if (player.GlobalBadge == null || player.GlobalBadge.Value.Type != 0)
                {
                    AddPreviousBadge(player);
                    if (subClass.StringOptions.ContainsKey("Badge"))
                    {
                        player.RankName = subClass.StringOptions["Badge"];
                    }

                    if (subClass.StringOptions.ContainsKey("BadgeColor"))
                    {
                        player.RankColor = subClass.StringOptions["BadgeColor"];
                    }
                }
            }

            if ((!lite || escaped) && subClass.OnSpawnEffects.Count != 0)
            {
                Timing.CallDelayed(0.1f, () =>
                {
                    Log.Debug($"Subclass {subClass.Name} has on spawn effects", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                    foreach (string effect in subClass.OnSpawnEffects)
                    {
                        Log.Debug($"Evaluating chance for on spawn {effect} for player {player.Nickname}", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                        if (!subClass.FloatOptions.ContainsKey("OnSpawn" + effect + "Chance"))
                        {
                            Log.Error($"ERROR! Spawn effect {effect} chance not found! Please make sure to add this to your float options");
                            continue;
                        }

                        if ((Random.NextDouble() * 100) < subClass.FloatOptions["OnSpawn" + effect + "Chance"])
                        {
                            player.ReferenceHub.playerEffectsController.EnableByString(effect, subClass.FloatOptions.ContainsKey("OnSpawn" + effect + "Duration") ? subClass.FloatOptions["OnSpawn" + effect + "Duration"] : -1, true);
                            player.ReferenceHub.playerEffectsController.ChangeByString(effect, subClass.IntOptions.ContainsKey("OnSpawn" + effect + "Intensity") ? (byte)subClass.IntOptions["OnSpawn" + effect + "Intensity"] : (byte)1);
                            Log.Debug($"Player {player.Nickname} has been given effect {effect} on spawn", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                        }
                        else
                        {
                            Log.Debug($"Player {player.Nickname} has been not given effect {effect} on spawn", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                        }
                    }
                });
            }
            else
            {
                Log.Debug($"Subclass {subClass.Name} has no on spawn effects", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
            }

            if (spawnIndex != -1 && (!lite || escaped) && subClass.SpawnLocations[spawnIndex] != "Unknown")
            {
                if (spawnLocations.Count != 0)
                {
                    Timing.CallDelayed(0.3f, () =>
                    {
                        Vector3 offset = new Vector3(0, 1f, 0);
                        if (subClass.FloatOptions.ContainsKey("SpawnOffsetX"))
                        {
                            offset.x = subClass.FloatOptions["SpawnOffsetX"];
                        }

                        if (subClass.FloatOptions.ContainsKey("SpawnOffsetY"))
                        {
                            offset.y = subClass.FloatOptions["SpawnOffsetY"];
                        }

                        if (subClass.FloatOptions.ContainsKey("SpawnOffsetZ"))
                        {
                            offset.z = subClass.FloatOptions["SpawnOffsetZ"];
                        }

                        Vector3 pos = spawnLocations[Random.Next(spawnLocations.Count)] + offset;
                        player.Position = pos;
                    });
                }
            }
            else if (spawnIndex == -1)
            {
                Log.Debug($"Unable to set spawn for class {subClass.Name} for player {player.Nickname}. No rooms found on map.", Plugin.Instance.Config.Debug);
            }

            if (subClass.IntOptions.ContainsKey("MaxPerSpawnWave"))
            {
                if (!ClassesGiven.ContainsKey(subClass))
                {
                    ClassesGiven.Add(subClass, 1);
                    Timing.CallDelayed(5f, () =>
                    {
                        DontGiveClasses.Clear();
                        ClassesGiven.Clear();
                    });
                }
                else
                {
                    ClassesGiven[subClass]++;
                }

                if (ClassesGiven[subClass] >= subClass.IntOptions["MaxPerSpawnWave"])
                {
                    if (!DontGiveClasses.Contains(subClass))
                    {
                        DontGiveClasses.Add(subClass);
                    }
                }
            }

            if (!subClass.Abilities.Contains(AbilityType.CantEscape)
                && player.Role != RoleType.ClassD && player.Role != RoleType.Scientist &&
                (subClass.EscapesAs[0] != RoleType.None || subClass.EscapesAs[1] != RoleType.None))
            {
                player.GameObject.AddComponent<EscapeBehaviour>();

                EscapeBehaviour eb = player.GameObject.GetComponent<EscapeBehaviour>();
                eb.EscapesAsNotCuffed = subClass.EscapesAs[0];
                eb.EscapesAsCuffed = subClass.EscapesAs[1];
            }

            if (!PlayersThatJustGotAClass.ContainsKey(player))
            {
                PlayersThatJustGotAClass.Add(player, Time.time + 3f);
            }
            else
            {
                PlayersThatJustGotAClass[player] = Time.time + 3f;
            }

            Log.Debug($"Player with name {player.Nickname} got subclass {subClass.Name}", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
            Timing.CallDelayed(1f, () =>
                {
                    Events.Handlers.Player.OnReceivedSubclass(
                        new Events.EventArgs.ReceivedSubclassEventArgs(player, subClass));
                });
        }

        public static void RemoveAndAddRoles(Player p, bool dontAddRoles = false, bool is035 = false, bool escaped = false, bool disguised = false)
        {
            if (PlayersThatJustGotAClass.ContainsKey(p) && PlayersThatJustGotAClass[p] > Time.time)
            {
                return;
            }

            if (RoundJustStarted())
            {
                return;
            }

            if (!disguised)
            {
                {
                if (PlayersInvisibleByCommand.Contains(p))
                {
                    PlayersInvisibleByCommand.Remove(p);
                }

                if (Cooldowns.ContainsKey(p))
                {
                    Cooldowns.Remove(p);
                }

                if (FriendlyFired.Contains(p))
                {
                    FriendlyFired.RemoveAll(e => e == p);
                }

                if (PlayersWithSubclasses.ContainsKey(p) && PlayersWithSubclasses[p].Abilities.Contains(AbilityType.Disable173Stop)
                                                         && Scp173.TurnedPlayers.Contains(p))
                {
                    Scp173.TurnedPlayers.Remove(p);
                }

                if (PlayersWithSubclasses.ContainsKey(p) &&
                    PlayersWithSubclasses[p].Abilities.Contains(AbilityType.NoArmorDecay))
                {
                    p.ReferenceHub.playerStats.artificialHpDecay = 0.75f;
                }

                if (PlayersInvisibleByCommand.Contains(p))
                {
                    PlayersInvisibleByCommand.Remove(p);
                }

                if (PlayersVenting.Contains(p))
                {
                    PlayersVenting.Remove(p);
                }

                if (PlayersBloodLusting.Contains(p))
                {
                    PlayersBloodLusting.Remove(p);
                }
                }
            }

            if (!string.IsNullOrEmpty(p.ReferenceHub.serverRoles.HiddenBadge))
            {
                p.ReferenceHub.serverRoles.HiddenBadge = null;
            }

            SubClass subClass = PlayersWithSubclasses.ContainsKey(p) ? PlayersWithSubclasses[p] : null;

            if (subClass != null)
            {
                if (!PreviousSubclasses.ContainsKey(p))
                {
                    PreviousSubclasses.Add(p, subClass);
                }
                else
                {
                    PreviousSubclasses[p] = subClass;
                }

                if (PreviousBadges.ContainsKey(p))
                {
                    if (subClass.StringOptions.ContainsKey("Badge") && p.RankName == subClass.StringOptions["Badge"])
                    {
                        p.RankName = PreviousBadges.ContainsKey(p)
                            ? System.Text.RegularExpressions.Regex.Split(PreviousBadges[p], System.Text.RegularExpressions.Regex.Escape(" [-/-] "))[0]
                            : null;
                        p.RankColor = PreviousBadges.ContainsKey(p)
                            ? System.Text.RegularExpressions.Regex.Split(PreviousBadges[p], System.Text.RegularExpressions.Regex.Escape(" [-/-] "))[1]
                            : null;
                    }
                    else if (subClass.StringOptions.ContainsKey("Badge") &&
                             p.ReferenceHub.serverRoles.HiddenBadge == subClass.StringOptions["Badge"])
                    {
                        p.ReferenceHub.serverRoles.HiddenBadge = PreviousBadges.ContainsKey(p)
                            ? System.Text.RegularExpressions.Regex.Split(PreviousBadges[p], System.Text.RegularExpressions.Regex.Escape(" [-/-] "))[0]
                            : null;
                    }
                }

                if (subClass.StringOptions.ContainsKey("Nickname"))
                {
                    p.DisplayNickname = null;
                }

                if (subClass.Abilities.Contains(AbilityType.HealAura))
                {
                    p.ReferenceHub.playerEffectsController.DisableEffect<HealAura>();
                    p.ReferenceHub.playerEffectsController.AllEffects.Remove(typeof(HealAura));
                }

                if (subClass.Abilities.Contains(AbilityType.DamageAura))
                {
                    p.ReferenceHub.playerEffectsController.DisableEffect<DamageAura>();
                    p.ReferenceHub.playerEffectsController.AllEffects.Remove(typeof(DamageAura));
                }

                if (subClass.Abilities.Contains(AbilityType.Regeneration))
                {
                    p.ReferenceHub.playerEffectsController.DisableEffect<Regeneration>();
                    p.ReferenceHub.playerEffectsController.AllEffects.Remove(typeof(Regeneration));
                }
            }

            if (p.GameObject != null && p.GameObject.GetComponent<InfiniteSprint>() != null)
            {
                Log.Debug($"Player {p.Nickname} has infinite stamina, destroying", Plugin.Instance.Config.Debug);
                p.GameObject.GetComponent<InfiniteSprint>()?.Destroy();
                p.IsUsingStamina = true; // Have to set it to true for it to remove fully... for some reason?
            }

            if (p.GameObject != null && p.GameObject.GetComponent<EscapeBehaviour>() != null)
            {
                Log.Debug($"Player {p.Nickname} has escapebehaviour, destroying", Plugin.Instance.Config.Debug);
                p.GameObject.GetComponent<EscapeBehaviour>()?.Destroy();
            }

            if (PlayersWithSubclasses.ContainsKey(p) && !disguised)
            {
                PlayersWithSubclasses.Remove(p);
            }

            if (escaped)
            {
                if (!PlayersThatJustGotAClass.ContainsKey(p))
                {
                    PlayersThatJustGotAClass.Add(p, Time.time + 3f);
                }
                else
                {
                    PlayersThatJustGotAClass[p] = Time.time + 3f;
                }
            }

            if (!dontAddRoles)
            {
                MaybeAddRoles(p, is035, escaped);
            }
        }

        public static void AddCooldown(Player p, AbilityType ability, bool initial = false)
        {
            try
            {
                SubClass subClass = PlayersWithSubclasses[p];
                if (!Cooldowns.ContainsKey(p))
                {
                    Cooldowns.Add(p, new Dictionary<AbilityType, float>());
                }

                Cooldowns[p][ability] = Time.time + (!initial ? subClass.AbilityCooldowns[ability] : subClass.InitialAbilityCooldowns[ability]);
            }
            catch (KeyNotFoundException e)
            {
                Log.Error($"You are missing an ability cooldown that MUST have a cooldown. Make sure to add {ability} to your ability cooldowns. Begin stack trace:\n{e.StackTrace}");
            }
        }

        public static bool OnCooldown(Player p, AbilityType ability, SubClass subClass)
        {
            return Cooldowns.ContainsKey(p) && Cooldowns[p].ContainsKey(ability) && Time.time <= Cooldowns[p][ability];
        }

        public static float TimeLeftOnCooldown(Player p, AbilityType ability, SubClass subClass, float time)
        {
            if (Cooldowns.ContainsKey(p) && Cooldowns[p].ContainsKey(ability))
            {
                return subClass.AbilityCooldowns[ability] - (time - Cooldowns[p][ability]);
            }

            return 0;
        }

        public static void UseAbility(Player p, AbilityType ability, SubClass subClass)
        {
            if (!subClass.IntOptions.ContainsKey(ability.ToString() + "MaxUses"))
            {
                return;
            }

            if (!AbilityUses.ContainsKey(p))
            {
                AbilityUses.Add(p, new Dictionary<AbilityType, int>());
            }

            if (!AbilityUses[p].ContainsKey(ability))
            {
                AbilityUses[p].Add(ability, 0);
            }

            AbilityUses[p][ability]++;
        }

        public static bool CanUseAbility(Player p, AbilityType ability, SubClass subClass)
        {
            return !AbilityUses.ContainsKey(p) || !AbilityUses[p].ContainsKey(ability) ||
                   !subClass.IntOptions.ContainsKey(ability + "MaxUses") ||
                   AbilityUses[p][ability] < subClass.IntOptions[ability + "MaxUses"];
        }

        public static void DisplayCantUseAbility(Player p, AbilityType ability, SubClass subClass, string abilityName)
        {
            p.ClearBroadcasts();
            p.Broadcast(4, subClass.StringOptions["OutOfAbilityUses"].Replace("{ability}", abilityName));
        }

        public static void DisplayCooldown(Player p, AbilityType ability, SubClass subClass, string abilityName, float time)
        {
            float timeLeft = TimeLeftOnCooldown(p, ability, subClass, time);
            p.ClearBroadcasts();
            p.Broadcast((ushort)Mathf.Clamp(timeLeft - (timeLeft / 4), 0.5f, 3), subClass.StringOptions["AbilityCooldownMessage"].Replace("{ability}", abilityName).Replace("{seconds}", timeLeft.ToString()));
        }

        public static bool PlayerJustBypassedTeslaGate(Player p)
        {
            return PlayersThatBypassedTeslaGates.ContainsKey(p) && Time.time - PlayersThatBypassedTeslaGates[p] < 3f;
        }

        public static bool RoundJustStarted()
        {
            return Time.time - RoundStartedAt < 10f;
        }

        public static void AddPreviousTeam(Player p)
        {
            if (PreviousRoles.ContainsKey(p))
            {
                PreviousRoles[p] = p.Role;
            }
            else
            {
                PreviousRoles.Add(p, p.Role);
            }
        }

        public static RoleType? GetPreviousRole(Player p)
        {
            if (PreviousRoles.TryGetValue(p, out RoleType roleType))
            {
                return roleType;
            }

            return null;
        }

        public static Team? GetPreviousTeam(Player p)
        {
            if (PreviousRoles.TryGetValue(p, out RoleType roleType))
            {
                return roleType.GetTeam();
            }

            return null;
        }

        public static void AddZombie(Player p, Player z)
        {
            if (PlayersWithZombies.TryGetValue(p, out List<Player> players))
            {
                players.Add(z);
                return;
            }

            PlayersWithZombies.Add(p, new List<Player>());
        }

        public static void RemoveZombie(Player p)
        {
            List<Player> toRemoveWith = new List<Player>();
            foreach (var item in PlayersWithZombies)
            {
                item.Value.Remove(p);
                if (item.Value.Count == 0)
                {
                    toRemoveWith.Add(item.Key);
                }
            }

            foreach (Player p1 in toRemoveWith)
            {
                PlayersWithZombies.Remove(p1);
            }
        }

        public static bool PlayerHasFfToPlayer(Player attacker, Player target)
        {
            Log.Debug($"Checking FF rules for Attacker: {attacker.Nickname} Target: {target?.Nickname}", Plugin.Instance.Config.Debug);
            if (target != null)
            {
                Log.Debug($"Checking zombies", Plugin.Instance.Config.Debug);
                if (PlayersWithZombies.Count(p => p.Value.Contains(target)) > 0)
                {
                    return true;
                }

                SubClass attackerClass = PlayersWithSubclasses.ContainsKey(attacker) ? PlayersWithSubclasses[attacker] : null;
                SubClass targetClass = PlayersWithSubclasses.ContainsKey(target) ? PlayersWithSubclasses[target] : null;

                Log.Debug($"Checking classes", Plugin.Instance.Config.Debug);
                if (attackerClass != null && targetClass != null && attackerClass.AdvancedFFRules.Contains(targetClass.Name))
                {
                    return true;
                }

                Log.Debug($"Checking FF rules in classes", Plugin.Instance.Config.Debug);
                if (FriendlyFired.Contains(target) ||
                    (attackerClass != null &&
                     !attackerClass.BoolOptions["DisregardHasFF"] && attackerClass.BoolOptions["HasFriendlyFire"]) ||
                    (targetClass != null && !targetClass.BoolOptions["DisregardTakesFF"] &&
                     targetClass.BoolOptions["TakesFriendlyFire"]))
                {
                    if (!FriendlyFired.Contains(target) && !(targetClass != null && targetClass.BoolOptions["TakesFriendlyFire"]))
                    {
                        AddToFf(attacker);
                    }

                    return true;
                }

                Log.Debug($"Checking takes friendly fire", Plugin.Instance.Config.Debug);
                if (targetClass != null && !targetClass.BoolOptions["DisregardTakesFF"] &&
                    !targetClass.BoolOptions["TakesFriendlyFire"])
                {
                    return false;
                }
            }

            return false;
        }

        public static bool AllowedToDamage(Player t, Player a)
        {
            Log.Debug($"Checking allowed damage rules for Attacker: {a.Nickname} to target role: {t.Role}", Plugin.Instance.Config.Debug);
            if (a.Id == t.Id)
            {
                return true;
            }

            SubClass attackerClass = PlayersWithSubclasses.ContainsKey(a) ? PlayersWithSubclasses[a] : null;
            SubClass targetClass = PlayersWithSubclasses.ContainsKey(t) ? PlayersWithSubclasses[t] : null;
            if (attackerClass != null && (attackerClass.CantDamageRoles.Contains(t.Role) ||
                                          attackerClass.CantDamageTeams.Contains(t.Team == Team.TUT ? Team.SCP : t.Team)))
            {
                return false;
            }

            if (targetClass != null && (targetClass.RolesThatCantDamage.Contains(a.Role) ||
                                        targetClass.TeamsThatCantDamage.Contains(a.Team == Team.TUT ? Team.SCP : a.Team)))
            {
                return false;
            }

            return attackerClass == null || targetClass == null || (!attackerClass.CantDamageSubclasses.Contains(targetClass.Name) && !targetClass.SubclassesThatCantDamage.Contains(attackerClass.Name));
        }

        public static IEnumerator<float> CheckRoundEnd()
        {
            if (!Round.IsStarted || RoundJustStarted() || (Player.List.Count() == 1 &&
                                                           !GameCore.ConfigFile.ServerConfig.GetBool(
                                                               "end_round_on_one_player")))
            {
                yield break;
            }

            Log.Debug("Checking round end", Plugin.Instance.Config.Debug);
            List<string> teamsAlive = GetTeamsAlive();

            List<string> uniqueTeamsAlive = new List<string>();

            foreach (string t in teamsAlive)
            {
                if (!uniqueTeamsAlive.Contains(t))
                {
                    uniqueTeamsAlive.Add(t);
                }
            }

            Log.Debug($"Number of unique teams alive: {uniqueTeamsAlive.Count}. Contains ALL? {uniqueTeamsAlive.Contains("ALL")}", Plugin.Instance.Config.Debug);
            if (Round.IsLocked)
            {
                goto swap_classes;
            }

            RoundSummary.SumInfo_ClassList classList = default;
            foreach (GameObject player in PlayerManager.players)
            {
                if (player != null)
                {
                    CharacterClassManager component = player.GetComponent<CharacterClassManager>();
                    if (component != null && component.Classes.CheckBounds(component.CurClass))
                    {
                        switch (component.Classes.SafeGet(component.CurClass).team)
                        {
                            case Team.SCP:
                                if (component.CurClass == RoleType.Scp0492)
                                {
                                    classList.zombies++;
                                }
                                else
                                {
                                    classList.scps_except_zombies++;
                                }

                                continue;
                            case Team.MTF:
                                classList.mtf_and_guards++;
                                continue;
                            case Team.CHI:
                                classList.chaos_insurgents++;
                                continue;
                            case Team.RSC:
                                classList.scientists++;
                                continue;
                            case Team.CDP:
                                classList.class_ds++;
                                continue;
                            default:
                                continue;
                        }
                    }
                }
            }

            classList.warhead_kills = AlphaWarheadController.Host.detonated ? AlphaWarheadController.Host.warheadKills : -1;
            classList.time = (int)Time.realtimeSinceStartup;

            RoundSummary.LeadingTeam leadingTeam = RoundSummary.LeadingTeam.Draw;

            RoundSummary.roundTime = classList.time - RoundSummary.singleton.classlistStart.time;

            if (uniqueTeamsAlive.Count == 2 && uniqueTeamsAlive.Contains("ALL"))
            {
                string team = uniqueTeamsAlive.Find(t => t != "ALL");
                switch (team)
                {
                    case "MTF":
                        leadingTeam = RoundSummary.LeadingTeam.FacilityForces;
                        break;
                    case "CHI":
                        leadingTeam = RoundSummary.LeadingTeam.ChaosInsurgency;
                        break;
                    case "SCP":
                        leadingTeam = RoundSummary.LeadingTeam.Anomalies;
                        break;
                }

                RoundSummary.singleton._roundEnded = true;
                RoundSummary.singleton.RpcShowRoundSummary(RoundSummary.singleton.classlistStart, classList, leadingTeam, RoundSummary.escaped_ds, RoundSummary.escaped_scientists, RoundSummary.kills_by_scp, Mathf.Clamp(GameCore.ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000));
                for (int i = 0; i < 50 * (Mathf.Clamp(GameCore.ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000) - 1); i++)
                {
                    yield return 0.0f;
                }

                RoundSummary.singleton.RpcDimScreen();
                for (int i = 0; i < 50; i++)
                {
                    yield return 0.0f;
                }

                PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
                yield break;
            }

            if (uniqueTeamsAlive.Count == 1)
            {
                if (PlayersWithSubclasses.Count > 0)
                {
                    switch (PlayersWithSubclasses.First().Value.EndsRoundWith)
                    {
                        case "MTF":
                            leadingTeam = RoundSummary.LeadingTeam.FacilityForces;
                            break;
                        case "CHI":
                            leadingTeam = RoundSummary.LeadingTeam.ChaosInsurgency;
                            break;
                        case "SCP":
                            leadingTeam = RoundSummary.LeadingTeam.Anomalies;
                            break;
                    }
                }

                RoundSummary.singleton._roundEnded = true;
                RoundSummary.singleton.RpcShowRoundSummary(RoundSummary.singleton.classlistStart, classList, leadingTeam, RoundSummary.escaped_ds, RoundSummary.escaped_scientists, RoundSummary.kills_by_scp, Mathf.Clamp(GameCore.ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000));
                for (int i = 0; i < 50 * (Mathf.Clamp(GameCore.ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000) - 1); i++)
                {
                    yield return 0.0f;
                }

                RoundSummary.singleton.RpcDimScreen();
                for (int i = 0; i < 50; i++)
                {
                    yield return 0.0f;
                }

                PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
                yield break;
            }

            swap_classes:
            if (PlayersWithSubclasses != null && PlayersWithSubclasses.Count(s => s.Value.EndsRoundWith != "RIP") > 0)
            {
                foreach (Player player in PlayersWithSubclasses.Keys)
                {
                    if ((PlayersWithSubclasses[player].BoolOptions.ContainsKey("ActAsSpy") &&
                         PlayersWithSubclasses[player].BoolOptions["ActAsSpy"]) &&
                        PlayersWithSubclasses[player].EndsRoundWith != "RIP" &&
                        PlayersWithSubclasses[player].EndsRoundWith != "ALL" &&
                        PlayersWithSubclasses[player].EndsRoundWith != player.Team.ToString() &&
                        teamsAlive.Count(e => e == PlayersWithSubclasses[player].EndsRoundWith) == 1)
                    {
                        PlayersThatJustGotAClass[player] = Time.time + 3f;
                        switch (PlayersWithSubclasses[player].EndsRoundWith)
                        {
                            case "MTF":
                                player.SetRole(RoleType.NtfScientist, true);
                                break;
                            case "CHI":
                                player.SetRole(RoleType.ChaosInsurgency, true);
                                break;
                            default:
                                player.SetRole(RoleType.Scp0492, true);
                                break;
                        }
                    }
                }
            }
        }

        public static int ClassesSpawned(SubClass subClass)
        {
            return !SubClassesSpawned.ContainsKey(subClass) ? 0 : SubClassesSpawned[subClass];
        }

        public static int GetNumWavesSpawned(Team t)
        {
            if (t != Team.RIP)
            {
                return NumSpawnWaves.ContainsKey(t) ? NumSpawnWaves[t] : 0;
            }

            int count = 0;
            foreach (var spawns in NumSpawnWaves)
            {
                count += spawns.Value;
            }

            return count;
        }

        public static bool EvaluateSpawnParameters(SubClass subClass)
        {
            List<string> evaluated = new List<string>();
            string separator = Plugin.Instance.Config.SpawnParameterSeparator;
            foreach (var param in subClass.SpawnParameters)
            {
                if (evaluated.Contains(param.Key))
                {
                    continue;
                }

                evaluated.Add(param.Key);
                string[] args = param.Key.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                if (args[0] == "sc")
                {
                    if (args[2] == "RangeMax" || args[2] == "RangeMin")
                    {
                        if (!IsInRange(evaluated, args, subClass, separator))
                        {
                            Log.Debug($"Did not pass spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                            return false;
                        }

                        Log.Debug($"Passed spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                    }
                    else if (args[2] == "Alive")
                    {
                        if (PlayersWithSubclasses.Count(t => t.Value.Name == args[1]) != param.Value)
                        {
                            Log.Debug($"Did not pass spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                            return false;
                        }

                        Log.Debug($"Passed spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                    }
                }
                else if (args[0] == "team")
                {
                    try
                    {
                        Team team = (Team)Enum.Parse(typeof(Team), args[1]);
                        if (args[2] == "RangeMax" || args[2] == "RangeMin")
                        {
                            if (!IsInRange(evaluated, args, subClass, separator, team))
                            {
                                Log.Debug($"Did not pass spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                                return false;
                            }

                            Log.Debug($"Passed spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                        }
                        else if (args[2] == "Alive")
                        {
                            if (GetTeamsAlive().Count(t => t == team.ToString()) != param.Value)
                            {
                                Log.Debug($"Did not pass spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                                return false;
                            }

                            Log.Debug($"Passed spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                        }
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error(
                            $"Spawn parameters for class {subClass.Name} has an incorrect team name. Key: {param.Key}. Begin stack trace:\n{e.StackTrace}");
                        return false;
                    }
                }
                else if (args[0] == "players")
                {
                    if (args[1] == "Alive")
                    {
                        if (args.Length == 3)
                        {
                            if (!IsInRange(evaluated, args, subClass, separator))
                            {
                                Log.Debug($"Did not pass spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                                return false;
                            }

                            Log.Debug($"Passed spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                        }
                        else
                        {
                            if (Player.List.Count(p => p.IsAlive) != param.Value)
                            {
                                Log.Debug($"Did not pass spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                                return false;
                            }

                            Log.Debug($"Passed spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                        }
                    }
                    else if (args[1] == "Dead")
                    {
                        if (args.Length == 3)
                        {
                            if (!IsInRange(evaluated, args, subClass, separator))
                            {
                                Log.Debug($"Did not pass spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                                return false;
                            }

                            Log.Debug($"Passed spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                        }
                        else
                        {
                            if (Player.List.Count(p => !p.IsAlive) != param.Value)
                            {
                                Log.Debug($"Did not pass spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                                return false;
                            }

                            Log.Debug($"Passed spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                        }
                    }
                }
                else if (args[0] == "role")
                {
                    try
                    {
                        RoleType role = (RoleType)Enum.Parse(typeof(RoleType), args[1]);
                        if (args[2] == "RangeMax" || args[2] == "RangeMin")
                        {
                            if (!IsInRange(evaluated, args, subClass, separator, Team.RIP, role))
                            {
                                Log.Debug($"Did not pass spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                                return false;
                            }

                            Log.Debug($"Passed spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                        }
                        else if (args[2] == "Alive")
                        {
                            if (Player.List.Count(p => p.Role == role) != param.Value)
                            {
                                Log.Debug($"Did not pass spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                                return false;
                            }

                            Log.Debug($"Passed spawn parameter: {param.Key}", Plugin.Instance.Config.Debug);
                        }
                    }
                    catch (ArgumentException e)
                    {
                        Log.Error($"Spawn parameters for class {subClass.Name} has an incorrect role name. Key: {param.Key}. Begin stack trace:\n{e.StackTrace}");
                        return false;
                    }
                }
            }

            return true;
        }

        public static RoleType? RagdollRole(Ragdoll doll)
        {
            if (!RagdollRoles.ContainsKey(doll.netId))
            {
                return null;
            }

            return RagdollRoles[doll.netId];
        }

        public static void KillAllCoroutines()
        {
            foreach (CoroutineHandle coroutine in Coroutines)
            {
                Timing.KillCoroutines(coroutine);
            }

            Coroutines.Clear();
        }

        private static void AddToFf(Player p)
        {
            if (!FriendlyFired.Contains(p))
            {
                FriendlyFired.Add(p);
            }
        }

        private static void AddPreviousBadge(Player p, bool hidden = false)
        {
            if (hidden)
            {
                if (PreviousBadges.ContainsKey(p))
                {
                    PreviousBadges[p] = p.ReferenceHub.serverRoles.HiddenBadge + " [-/-] ";
                }
                else
                {
                    PreviousBadges.Add(p, p.ReferenceHub.serverRoles.HiddenBadge + " [-/-] ");
                }
            }
            else
            {
                if (PreviousBadges.ContainsKey(p))
                {
                    PreviousBadges[p] = p.RankName + " [-/-] " + p.RankColor;
                }
                else
                {
                    PreviousBadges.Add(p, p.RankName + " [-/-] " + p.RankColor);
                }
            }
        }

        private static List<string> GetTeamsAlive()
        {
            List<string> teamsAlive = Player.List.Select(p1 => p1.Team.ToString()).ToList();
            teamsAlive.RemoveAll(t => t == "RIP");
            foreach (var item in PlayersWithSubclasses.Where(s => s.Value.EndsRoundWith != "RIP"))
            {
                teamsAlive.Remove(item.Key.Team.ToString());
                teamsAlive.Add(item.Value.EndsRoundWith);
            }

            for (int i = 0; i < teamsAlive.Count; i++)
            {
                string t = teamsAlive[i];
                if (t == "CDP")
                {
                    teamsAlive.RemoveAt(i);
                    teamsAlive.Insert(i, "CHI");
                }
                else if (t == "RSC")
                {
                    teamsAlive.RemoveAt(i);
                    teamsAlive.Insert(i, "MTF");
                }
                else if (t == "TUT")
                {
                    teamsAlive.RemoveAt(i);
                    teamsAlive.Insert(i, "SCP");
                }
            }

            return teamsAlive;
        }

        private static bool IsInRange(List<string> evaluated, string[] args, SubClass subClass, string separator, Team team = Team.RIP, RoleType role = RoleType.None)
        {
            int count = 0;
            switch (args[0])
            {
                case "sc":
                    count = PlayersWithSubclasses.Count(e => e.Value.Name == args[1]);
                    break;
                case "team":
                    count = GetTeamsAlive().Count(t => t == team.ToString());
                    break;
                case "players" when args[1] == "Alive":
                    count = Player.List.Count(p => p.IsAlive);
                    break;
                case "players":
                    if (args[1] == "Dead")
                    {
                        count = Player.List.Count(p => !p.IsAlive);
                    }

                    break;
                case "role":
                    count = Player.List.Count(p => p.Role == role);
                    break;
            }

            string maxKey = $"{args[0]}{separator}{args[1]}{separator}RangeMax";
            string minKey = $"{args[0]}{separator}{args[1]}{separator}RangeMin";
            if (!subClass.SpawnParameters.ContainsKey(maxKey) || !subClass.SpawnParameters.ContainsKey(minKey))
            {
                Log.Error($"Subclass spawn parameters missing range key. Contains max key ({maxKey}): {subClass.SpawnParameters.ContainsKey(maxKey)}. Contains min key ({minKey}): {subClass.SpawnParameters.ContainsKey(minKey)}");
                return false;
            }

            evaluated.Add(args[2] == "RangeMax" ? minKey : maxKey);
            int max = subClass.SpawnParameters[maxKey];
            int min = subClass.SpawnParameters[minKey];
            return count >= min && count <= max;
        }

        private static bool IsGhost(Player player)
        {
            Assembly assembly = Loader.Plugins.FirstOrDefault(pl => pl.Name == "GhostSpectator")?.Assembly;
            if (assembly == null)
            {
                return false;
            }

            return (bool)assembly.GetType("GhostSpectator.API")?.GetMethod("IsGhost")?.Invoke(null, new object[]{ player });
        }

        private static void TheyMultiply(Player player, SubClass subClass)
        {
            int coroutineIndex = Coroutines.Count;
            RoleType savedRole = player.Role;
            if (savedRole == RoleType.Scp0492 || savedRole == RoleType.Spectator)
            {
                return;
            }

            Coroutines.Add(Timing.CallDelayed(subClass.AbilityCooldowns[AbilityType.Multiply], () =>
            {
                if (player.Role == RoleType.Spectator || !PlayersWithSubclasses.ContainsKey(player) ||
                    PlayersWithSubclasses[player].Name != subClass.Name
                    || savedRole != player.Role ||
                    (Map.IsLCZDecontaminated && savedRole.GetSpawnZone() == ZoneType.LightContainment)
                    || (Warhead.IsDetonated && savedRole.GetSpawnZone() != ZoneType.Surface) ||
                    savedRole.GetSpawnZone() == ZoneType.Unspecified)
                {
                    Timing.KillCoroutines(Coroutines[coroutineIndex]);
                    Coroutines.RemoveAt(coroutineIndex);
                    return;
                }

                if (!CanUseAbility(player, AbilityType.Multiply, subClass))
                {
                    DisplayCantUseAbility(player, AbilityType.Multiply, subClass, "multiply");
                    Timing.KillCoroutines(Coroutines[coroutineIndex]);
                    Coroutines.RemoveAt(coroutineIndex);
                    return;
                }

                List<Player> spectators = Player.Get(RoleType.Spectator).ToList();
                if (spectators.Count <= 0)
                {
                    Timing.KillCoroutines(Coroutines[coroutineIndex]);
                    Coroutines.RemoveAt(coroutineIndex);
                    TheyMultiply(player, subClass);
                    return;
                }

                UseAbility(player, AbilityType.Multiply, subClass);
                Player p = spectators.ElementAt(Random.Next(spectators.Count));
                p.SetRole(savedRole);

                Timing.CallDelayed(Plugin.Instance.CommonUtilsEnabled ? 2.5f : 0.5f, () =>
                {
                    p.Health *= subClass.FloatOptions.ContainsKey("MultiplyHealthPercent") ? subClass.FloatOptions["MultiplyHealthPercent"] / 100 : .25f;
                    Timing.KillCoroutines(Coroutines[coroutineIndex]);
                    Coroutines.RemoveAt(coroutineIndex);
                    TheyMultiply(player, subClass);
                });
            }));
        }

        private static void CheckNormalSubclass(Player player, bool escaped, bool is035, List<string> teamsAlive)
        {
            Log.Debug($"Evaluating possible subclasses for player with name {player.Nickname}", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
            foreach (SubClass subClass in Plugin.Instance.Classes.Values.Where(e => e.BoolOptions["Enabled"] &&
                e.AffectsRoles.Contains(player.Role) &&
                (!e.IntOptions.ContainsKey("MaxSpawnPerRound") ||
                 ClassesSpawned(e) < e.IntOptions["MaxSpawnPerRound"]) &&
                (!e.BoolOptions.ContainsKey("OnlyAffectsSpawnWave") || !e.BoolOptions["OnlyAffectsSpawnWave"]) &&
                (!e.BoolOptions.ContainsKey("OnlyGivenOnEscape") || !e.BoolOptions["OnlyGivenOnEscape"] || (e.BoolOptions["OnlyGivenOnEscape"] && escaped)) &&
                (!e.BoolOptions.ContainsKey("GivenOnEscape") ||
                 ((!e.BoolOptions["GivenOnEscape"] && !escaped) || e.BoolOptions["GivenOnEscape"])) &&
                (!e.BoolOptions.ContainsKey("WaitForSpawnWaves") || (e.BoolOptions["WaitForSpawnWaves"] &&
                                                                     GetNumWavesSpawned(
                                                                         e.StringOptions.ContainsKey(
                                                                             "WaitSpawnWaveTeam")
                                                                             ? (Team)Enum.Parse(
                                                                                 typeof(Team),
                                                                                 e.StringOptions["WaitSpawnWaveTeam"])
                                                                             : Team.RIP) <
                                                                     e.IntOptions["NumSpawnWavesToWait"])) &&
                (!e.BoolOptions.ContainsKey("OnlyGivenOnRoundStart") ||
                 ((e.BoolOptions["OnlyGivenOnRoundStart"] && RoundJustStarted()) ||
                  !e.BoolOptions["OnlyGivenOnRoundStart"])) &&
                EvaluateSpawnParameters(e)))
            {
                double rng = Random.NextDouble() * 100;
                Log.Debug($"Evaluating possible subclass {subClass.Name} for player with name {player.Nickname}. Number generated: {rng}, must be less than {subClass.FloatOptions["ChanceToGet"]} to get class", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);

                if (DontGiveClasses.Contains(subClass))
                {
                    Log.Debug("Not giving subclass, MaxPerSpawnWave exceeded.", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                    continue;
                }

                if (rng < subClass.FloatOptions["ChanceToGet"] &&
                    (!subClass.IntOptions.ContainsKey("MaxAlive") ||
                     PlayersWithSubclasses.Count(e => e.Value.Name == subClass.Name) <
                     subClass.IntOptions["MaxAlive"]) &&
                    (subClass.EndsRoundWith == "RIP" || subClass.EndsRoundWith == "ALL" ||
                     teamsAlive.Contains(subClass.EndsRoundWith)))
                {
                    Log.Debug($"{player.Nickname} attempting to be given subclass {subClass.Name}", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                    AddClass(player, subClass, is035, is035 || escaped, escaped);
                    break;
                }

                Log.Debug($"Player with name {player.Nickname} did not get subclass {subClass.Name}", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
            }
        }

        private static void CheckOtherClass(Player player, bool escaped, bool is035, List<string> teamsAlive, bool additive)
        {
            double num = Random.NextDouble() * 100;
            Log.Debug($"Evaluating possible subclasses for player with name {player.Nickname}. Additive/Weighted chance. Number generated: {num}", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);

            var source = additive ? Plugin.Instance.ClassesAdditive : Plugin.Instance.ClassesWeighted;
            if (!source.ContainsKey(player.Role))
            {
                return;
            }

            foreach (var possibility in source[player.Role].Where(e => e.Key.BoolOptions["Enabled"] &&
                                                                     e.Key.AffectsRoles.Contains(player.Role) &&
                                                                     (!e.Key.BoolOptions.ContainsKey(
                                                                          "OnlyAffectsSpawnWave") ||
                                                                      !e.Key.BoolOptions["OnlyAffectsSpawnWave"]) &&
                                                                     (!e.Key.IntOptions
                                                                          .ContainsKey("MaxSpawnPerRound") ||
                                                                      ClassesSpawned(e.Key) <
                                                                      e.Key.IntOptions["MaxSpawnPerRound"]) &&
                                                                     (!e.Key.BoolOptions.ContainsKey(
                                                                          "OnlyGivenOnEscape") ||
                                                                      (!e.Key.BoolOptions["OnlyGivenOnEscape"] ||
                                                                       (e.Key.BoolOptions["OnlyGivenOnEscape"] &&
                                                                        escaped))) &&
                                                                     (!e.Key.BoolOptions.ContainsKey("GivenOnEscape") ||
                                                                      ((!e.Key.BoolOptions["GivenOnEscape"] &&
                                                                        !escaped) ||
                                                                       e.Key.BoolOptions["GivenOnEscape"])) &&
                                                                     (!e.Key.BoolOptions.ContainsKey(
                                                                          "WaitForSpawnWaves") ||
                                                                      (e.Key.BoolOptions["WaitForSpawnWaves"] &&
                                                                       GetNumWavesSpawned(
                                                                           e.Key.StringOptions.ContainsKey(
                                                                               "WaitSpawnWaveTeam")
                                                                               ? (Team)Enum.Parse(
                                                                                   typeof(Team),
                                                                                   e.Key.StringOptions["WaitSpawnWaveTeam"])
                                                                               : Team.RIP) <
                                                                       e.Key.IntOptions["NumSpawnWavesToWait"])) &&
                                                                     (!e.Key.BoolOptions.ContainsKey(
                                                                          "OnlyGivenOnRoundStart") ||
                                                                      ((e.Key.BoolOptions["OnlyGivenOnRoundStart"] &&
                                                                        RoundJustStarted()) ||
                                                                       !e.Key.BoolOptions["OnlyGivenOnRoundStart"])) &&
                                                                     EvaluateSpawnParameters(e.Key)))
            {
                Log.Debug($"Evaluating possible subclass {possibility.Key.Name} for player with name {player.Nickname}. Num ({num}) must be less than {possibility.Value} to obtain.", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                if (num < possibility.Value && (!possibility.Key.IntOptions.ContainsKey("MaxAlive") ||
                                              PlayersWithSubclasses
                                                  .Count(e => e.Value.Name == possibility.Key.Name) < possibility.Key.IntOptions["MaxAlive"]) &&
                    (possibility.Key.EndsRoundWith == "RIP" || possibility.Key.EndsRoundWith == "ALL" ||
                     teamsAlive.Contains(possibility.Key.EndsRoundWith)))
                {
                    Log.Debug($"{player.Nickname} attempting to be given subclass {possibility.Key.Name}", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                    AddClass(player, possibility.Key, is035, is035 || escaped, escaped);
                    break;
                }

                Log.Debug($"Player with name {player.Nickname} did not get subclass {possibility.Key.Name}", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
            }
        }

        private static bool CheckUserClass(Player player, bool is035, bool escaped, List<string> teamsAlive)
        {
            foreach (SubClass subClass in Plugin.Instance.Classes.Values.Where(e => e.BoolOptions["Enabled"] &&
                e.AffectsRoles.Contains(player.Role) &&
                e.AffectsUsers.ContainsKey(player.UserId) &&
                (!e.IntOptions.ContainsKey("MaxSpawnPerRound") ||
                 ClassesSpawned(e) < e.IntOptions["MaxSpawnPerRound"]) &&
                (!e.BoolOptions.ContainsKey("OnlyAffectsSpawnWave") || !e.BoolOptions["OnlyAffectsSpawnWave"]) &&
                (!e.BoolOptions.ContainsKey("OnlyGivenOnEscape") || !e.BoolOptions["OnlyGivenOnEscape"] || (e.BoolOptions["OnlyGivenOnEscape"] && escaped)) &&
                (!e.BoolOptions.ContainsKey("GivenOnEscape") ||
                 ((!e.BoolOptions["GivenOnEscape"] && !escaped) || e.BoolOptions["GivenOnEscape"])) &&
                (!e.BoolOptions.ContainsKey("WaitForSpawnWaves") || (e.BoolOptions["WaitForSpawnWaves"] &&
                                                                     GetNumWavesSpawned(
                                                                         e.StringOptions.ContainsKey(
                                                                             "WaitSpawnWaveTeam")
                                                                             ? (Team)Enum.Parse(
                                                                                 typeof(Team),
                                                                                 e.StringOptions["WaitSpawnWaveTeam"])
                                                                             : Team.RIP) <
                                                                     e.IntOptions["NumSpawnWavesToWait"])) &&
                (!e.BoolOptions.ContainsKey("OnlyGivenOnRoundStart") ||
                 ((e.BoolOptions["OnlyGivenOnRoundStart"] && RoundJustStarted()) ||
                  !e.BoolOptions["OnlyGivenOnRoundStart"])) &&
                EvaluateSpawnParameters(e)))
            {
                double rng = Random.NextDouble() * 100;
                Log.Debug($"Evaluating possible unique subclass {subClass.Name} for player with name {player.Nickname}. Number generated: {rng}, must be less than {subClass.AffectsUsers[player.UserId]} to get class", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                if (DontGiveClasses.Contains(subClass))
                {
                    Log.Debug("Not giving subclass, MaxPerSpawnWave exceeded.", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                    continue;
                }

                if (rng < subClass.AffectsUsers[player.UserId] && (!subClass.IntOptions.ContainsKey("MaxAlive") ||
                                                                   PlayersWithSubclasses.Count(e => e.Value.Name == subClass.Name) <
                                                                   subClass.IntOptions["MaxAlive"]) &&
                    (subClass.EndsRoundWith == "RIP" || subClass.EndsRoundWith == "ALL" ||
                     teamsAlive.Contains(subClass.EndsRoundWith)))
                {
                    Log.Debug($"{player.Nickname} attempting to be given subclass {subClass.Name}", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                    AddClass(player, subClass, is035, is035 || escaped, escaped);
                    return true;
                }
            }

            return false;
        }

        private static bool CheckPermissionClass(Player player, bool is035, bool escaped, List<string> teamsAlive)
        {
            foreach (SubClass subClass in Plugin.Instance.Classes.Values.Where(e => e.BoolOptions["Enabled"] &&
                e.AffectsRoles.Contains(player.Role) &&
                e.Permissions.Count > 0 && e.Permissions.Keys.Any(p => player.CheckPermission("sc." + p)) &&
                (!e.IntOptions.ContainsKey("MaxSpawnPerRound") ||
                 ClassesSpawned(e) < e.IntOptions["MaxSpawnPerRound"]) &&
                (!e.BoolOptions.ContainsKey("OnlyAffectsSpawnWave") || !e.BoolOptions["OnlyAffectsSpawnWave"]) &&
                (!e.BoolOptions.ContainsKey("OnlyGivenOnEscape") || (!e.BoolOptions["OnlyGivenOnEscape"] ||
                                                                     (e.BoolOptions["OnlyGivenOnEscape"] && escaped))) &&
                (!e.BoolOptions.ContainsKey("GivenOnEscape") ||
                 ((!e.BoolOptions["GivenOnEscape"] && !escaped) || e.BoolOptions["GivenOnEscape"])) &&
                (!e.BoolOptions.ContainsKey("WaitForSpawnWaves") || (e.BoolOptions["WaitForSpawnWaves"] &&
                                                                     GetNumWavesSpawned(
                                                                         e.StringOptions.ContainsKey(
                                                                             "WaitSpawnWaveTeam")
                                                                             ? (Team)Enum.Parse(
                                                                                 typeof(Team),
                                                                                 e.StringOptions["WaitSpawnWaveTeam"])
                                                                             : Team.RIP) <
                                                                     e.IntOptions["NumSpawnWavesToWait"])) &&
                (!e.BoolOptions.ContainsKey("OnlyGivenOnRoundStart") ||
                 ((e.BoolOptions["OnlyGivenOnRoundStart"] && RoundJustStarted()) ||
                  !e.BoolOptions["OnlyGivenOnRoundStart"])) &&
                EvaluateSpawnParameters(e)))
            {
                double rng = Random.NextDouble() * 100;
                float needed = subClass.Permissions.First(p => player.CheckPermission("sc." + p.Key)).Value;
                Log.Debug($"Evaluating possible permission subclass {subClass.Name} for player with name {player.Nickname}. Number generated: {rng}, must be less than {needed} to get class", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                if (DontGiveClasses.Contains(subClass))
                {
                    Log.Debug("Not giving subclass, MaxPerSpawnWave exceeded.", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                    continue;
                }

                if (rng < needed && (!subClass.IntOptions.ContainsKey("MaxAlive") || PlayersWithSubclasses.Count(e => e.Value.Name == subClass.Name) < subClass.IntOptions["MaxAlive"]) &&
                    (subClass.EndsRoundWith == "RIP" || subClass.EndsRoundWith == "ALL" ||
                     teamsAlive.Contains(subClass.EndsRoundWith)))
                {
                    Log.Debug($"{player.Nickname} attempting to be given subclass {subClass.Name}", Plugin.Instance.Config.Debug || Plugin.Instance.Config.ClassDebug);
                    AddClass(player, subClass, is035, is035 || escaped, escaped);
                    return true;
                }
            }

            return false;
        }

        // I know I can do this just from vector 3's, but meh
        private static ZoneType GetSpawnZone(this RoleType role)
        {
            switch (role)
            {
                case RoleType.ClassD:
                case RoleType.Scp173:
                case RoleType.Scientist:
                    return ZoneType.LightContainment;

                case RoleType.Scp049:
                case RoleType.Scp079: // Count it as HCZ
                case RoleType.Scp096:
                case RoleType.Scp106:
                case RoleType.Scp93953:
                case RoleType.Scp93989:
                    return ZoneType.HeavyContainment;

                case RoleType.ChaosInsurgency:
                case RoleType.NtfCadet:
                case RoleType.NtfCommander:
                case RoleType.NtfLieutenant:
                case RoleType.NtfScientist:
                case RoleType.Tutorial:
                    return ZoneType.Surface;

                case RoleType.FacilityGuard:
                    return ZoneType.Entrance;

                default:
                    return ZoneType.Unspecified;
            }
        }
    }
}
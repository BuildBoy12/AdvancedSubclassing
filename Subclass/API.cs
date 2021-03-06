﻿// <copyright file="API.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Exiled.API.Features;
    using Exiled.Loader;
    using UnityEngine;

    public static class API
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="subClass"></param>
        /// <param name="lite"></param>
        /// <returns></returns>
        public static bool GiveClass(Player p, SubClass subClass, bool lite = false)
        {
            if (PlayerHasSubclass(p) || !subClass.AffectsRoles.Contains(p.Role))
            {
                return false;
            }

            if (Plugin.Instance.Scp035Enabled)
            {
                Player scp035 = (Player)Loader.Plugins.First(pl => pl.Name == "scp035").Assembly.GetType("scp035.API.Scp035Data").GetMethod("GetScp035", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
                TrackingAndMethods.AddClass(p, subClass, Plugin.Instance.Scp035Enabled && scp035?.Id == p.Id, lite);
                return true;
            }

            TrackingAndMethods.AddClass(p, subClass, false, lite);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool RemoveClass(Player p)
        {
            if (!PlayerHasSubclass(p))
            {
                return false;
            }

            TrackingAndMethods.RemoveAndAddRoles(p, true);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, SubClass> GetClasses()
        {
            return Plugin.Instance.Classes.ToDictionary(x => x.Key, x => new SubClass(x.Value));
        }

        /// <summary>
        /// 
        /// </summary>
        public static void EnableAllClasses()
        {
            foreach (var subClass in Plugin.Instance.Classes)
            {
                subClass.Value.BoolOptions["Enabled"] = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void DisableAllClasses()
        {
            foreach (var subClass in Plugin.Instance.Classes)
            {
                subClass.Value.BoolOptions["Enabled"] = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        public static bool EnableClass(SubClass sc)
        {
            try
            {
                Plugin.Instance.Classes[sc.Name].BoolOptions["Enabled"] = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        public static bool EnableClass(string sc)
        {
            try
            {
                Plugin.Instance.Classes[sc].BoolOptions["Enabled"] = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        public static bool DisableClass(SubClass sc)
        {
            try
            {
                Plugin.Instance.Classes[sc.Name].BoolOptions["Enabled"] = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sc"></param>
        /// <returns></returns>
        public static bool DisableClass(string sc)
        {
            try
            {
                Plugin.Instance.Classes[sc].BoolOptions["Enabled"] = false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Player, SubClass> GetPlayersWithSubclasses()
        {
            return TrackingAndMethods.PlayersWithSubclasses.ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static SubClass GetPlayersSubclass(Player p)
        {
            return !PlayerHasSubclass(p) ? null : new SubClass(TrackingAndMethods.PlayersWithSubclasses[p]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool PlayerHasSubclass(Player p)
        {
            return TrackingAndMethods.PlayersWithSubclasses.ContainsKey(p);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="duration"></param>
        public static void PreventPlayerFromGettingClass(Player p, float duration)
        {
            if (!TrackingAndMethods.PlayersThatJustGotAClass.ContainsKey(p))
            {
                TrackingAndMethods.PlayersThatJustGotAClass.Add(p, Time.time + duration);
            }
            else
            {
                TrackingAndMethods.PlayersThatJustGotAClass[p] = Time.time + duration;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool PlayerHasZombies(Player p)
        {
            return TrackingAndMethods.PlayersWithZombies.ContainsKey(p);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static List<Player> PlayersZombies(Player p)
        {
            return PlayerHasZombies(p) ? new List<Player>(TrackingAndMethods.PlayersWithZombies[p]) : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="ability"></param>
        /// <returns></returns>
        public static bool AbilityOnCooldown(Player p, AbilityType ability)
        {
            return PlayerHasSubclass(p) && TrackingAndMethods.OnCooldown(p, ability, TrackingAndMethods.PlayersWithSubclasses[p]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="ability"></param>
        /// <returns></returns>
        public static float TimeLeftOnCooldown(Player p, AbilityType ability)
        {
            if (!PlayerHasSubclass(p))
            {
                return 0;
            }

            return TrackingAndMethods.TimeLeftOnCooldown(p, ability, TrackingAndMethods.PlayersWithSubclasses[p], Time.time);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="findWeapon"></param>
        /// <param name="weaponObtained"></param>
        public static void RegisterCustomWeaponGetter(MethodInfo findWeapon, MethodInfo weaponObtained)
        {
            TrackingAndMethods.CustomWeaponGetters.Add(new Tuple<MethodInfo, MethodInfo>(findWeapon, weaponObtained));
        }
    }
}
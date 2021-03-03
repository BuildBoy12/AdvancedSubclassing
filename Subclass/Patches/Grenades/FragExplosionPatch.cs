// <copyright file="FragExplosionPatch.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Patches.Grenades
{
    using System.Collections.Generic;
    using CustomPlayerEffects;
    using Exiled.API.Features;
    using Exiled.Events.EventArgs;
    using GameCore;
    using HarmonyLib;
    using Interactables.Interobjects.DoorUtils;
    using UnityEngine;
    using FragGrenade = global::Grenades.FragGrenade;

    /// <summary>
    /// 
    /// </summary>
    [HarmonyPatch(typeof(FragGrenade), nameof(FragGrenade.ServersideExplosion))]
    internal static class FragExplosionPatch
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool Prefix(FragGrenade __instance, ref bool __result)
        {
            Player thrower = Player.Get(__instance.thrower.gameObject);

            Dictionary<Player, float> damages = new Dictionary<Player, float>();

            Vector3 position = __instance.transform.position;
            int num = 0;
            Collider[] colliderArray = Physics.OverlapSphere(position, __instance.chainTriggerRadius, __instance.damageLayerMask);
            int index = 0;

            foreach (GameObject obj2 in PlayerManager.players)
            {
                if (!ServerConsole.FriendlyFire && obj2 != __instance.thrower.gameObject && !obj2.GetComponent<WeaponManager>()
                    .GetShootPermission(__instance.throwerTeam))
                {
                    continue;
                }

                PlayerStats component = obj2.GetComponent<PlayerStats>();
                if ((component != null) && component.ccm.InWorld)
                {
                    float amount = __instance.damageOverDistance.Evaluate(Vector3.Distance(position, component.transform.position)) * (component.ccm.IsHuman()
                            ? ConfigFile.ServerConfig.GetFloat("human_grenade_multiplier", 0.7f)
                            : ConfigFile.ServerConfig.GetFloat("scp_grenade_multiplier", 1f));

                    damages.Add(Player.Get(obj2), amount);
                }
            }

            var ev = new ExplodingGrenadeEventArgs(thrower, damages, true, __instance.gameObject);

            Exiled.Events.Handlers.Map.OnExplodingGrenade(ev);

            if (!ev.IsAllowed)
            {
                return false;
            }

            while (index < colliderArray.Length)
            {
                Collider collider = colliderArray[index];
                BreakableWindow component = collider.GetComponent<BreakableWindow>();
                if (component != null)
                {
                    if ((component.transform.position - position).sqrMagnitude <= __instance.sqrChainTriggerRadius)
                    {
                        component.ServerDamageWindow(500f);
                    }
                }
                else
                {
                    DoorVariant componentInParent = collider.GetComponentInParent<DoorVariant>();
                    if (componentInParent != null && componentInParent is IDamageableDoor damageableDoor)
                    {
                        damageableDoor.ServerDamage(__instance.damageOverDistance.Evaluate(Vector3.Distance(position, componentInParent.transform.position)), DoorDamageType.Grenade);
                    }
                    else if ((__instance.chainLengthLimit == -1 ||
                              __instance.chainLengthLimit > __instance.currentChainLength) &&
                             (__instance.chainConcurrencyLimit == -1 || __instance.chainConcurrencyLimit > num))
                    {
                        Pickup componentInChildren = collider.GetComponentInChildren<Pickup>();
                        if ((componentInChildren != null) && __instance.ChangeIntoGrenade(componentInChildren))
                        {
                            num++;
                        }
                    }
                }

                index++;
            }

            foreach (var item in damages)
            {
                if (item.Value > __instance.absoluteDamageFalloff)
                {
                    PlayerStats component = item.Key.GameObject.GetComponent<PlayerStats>();
                    Transform[] grenadePoints = component.grenadePoints;
                    index = 0;
                    while (true)
                    {
                        if (index < grenadePoints.Length)
                        {
                            Transform transform = grenadePoints[index];
                            if (Physics.Linecast(position, transform.position, __instance.hurtLayerMask))
                            {
                                index++;
                                continue;
                            }

                            component.HurtPlayer(new PlayerStats.HitInfo(item.Value, __instance.thrower != null ? __instance.thrower.hub.LoggedNameFromRefHub() : "(UNKNOWN)", DamageTypes.Grenade, __instance.thrower.hub.queryProcessor.PlayerId), item.Key.GameObject);
                        }

                        if (!component.ccm.IsAnyScp())
                        {
                            ReferenceHub hub = item.Key.ReferenceHub;
                            float duration = __instance.statusDurationOverDistance.Evaluate(Vector3.Distance(position, component.transform.position));
                            hub.playerEffectsController.EnableEffect(hub.playerEffectsController.GetEffect<Burned>(), duration);
                            hub.playerEffectsController.EnableEffect(hub.playerEffectsController.GetEffect<Concussed>(), duration);
                        }

                        break;
                    }
                }
            }

            string str = (__instance.thrower != null) ? __instance.thrower.hub.LoggedNameFromRefHub() : "(UNKNOWN)";

            ServerLogs.AddLog(ServerLogs.Modules.Logger, $"Player {str}'s {__instance.logName} grenade exploded.", ServerLogs.ServerLogType.GameEvent);

            if (__instance.serverGrenadeEffect != null)
            {
                Transform transform = __instance.transform;
                Object.Instantiate(__instance.serverGrenadeEffect, transform.position, transform.rotation);
            }

            __result = true;
            return false;
        }
    }
}
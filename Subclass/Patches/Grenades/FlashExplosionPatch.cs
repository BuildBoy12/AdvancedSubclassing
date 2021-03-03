// <copyright file="FlashExplosionPatch.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Patches.Grenades
{
    using CustomPlayerEffects;
    using Exiled.API.Features;
    using global::Grenades;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// 
    /// </summary>
    [HarmonyPatch(typeof(FlashGrenade), nameof(FlashGrenade.ServersideExplosion))]
    internal static class FlashExplosionPatch
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        /// <returns></returns>
        public static bool Prefix(FlashGrenade __instance, ref bool __result)
        {
            Log.Debug($"Flash grenade explosion", Plugin.Instance.Config.Debug);
            foreach (GameObject obj2 in PlayerManager.players)
            {
                Player target = Player.Get(obj2);
                Vector3 position = __instance.transform.position;
                ReferenceHub hub = ReferenceHub.GetHub(obj2);
                Flashed effect = hub.playerEffectsController.GetEffect<Flashed>();
                Deafened deafened = hub.playerEffectsController.GetEffect<Deafened>();
                Log.Debug($"Flash target is: {target?.Nickname}", Plugin.Instance.Config.Debug);
                if ((effect != null) &&
                    ((__instance.thrower != null)
                     && (__instance._friendlyFlash || effect.Flashable(ReferenceHub.GetHub(__instance.thrower.gameObject), position, __instance._ignoredLayers))))
                {
                    if (!TrackingAndMethods.PlayersWithSubclasses.ContainsKey(target) ||
                        !TrackingAndMethods.PlayersWithSubclasses[target].Abilities.Contains(AbilityType.FlashImmune))
                    {
                        float num = __instance.powerOverDistance.Evaluate(
                                        Vector3.Distance(obj2.transform.position, position) /
                                        (position.y > 900f
                                            ? __instance.distanceMultiplierSurface
                                            : __instance.distanceMultiplierFacility)) *
                                    __instance.powerOverDot.Evaluate(Vector3.Dot(hub.PlayerCameraReference.forward, (hub.PlayerCameraReference.position - position).normalized));
                        byte intensity = (byte)Mathf.Clamp(Mathf.RoundToInt(num * 10f * __instance.maximumDuration), 1, 0xff);
                        if (intensity >= effect.Intensity && num > 0f)
                        {
                            hub.playerEffectsController.ChangeEffectIntensity<Flashed>(intensity);
                            if (deafened != null)
                            {
                                hub.playerEffectsController.EnableEffect(deafened, num * __instance.maximumDuration, true);
                            }
                        }
                    }
                    else
                    {
                        Concussed concussedEffect = hub.playerEffectsController.GetEffect<Concussed>();
                        concussedEffect.Intensity = 3;
                        hub.playerEffectsController.EnableEffect(concussedEffect, 5);
                        Disabled disabledEffect = hub.playerEffectsController.GetEffect<Disabled>();
                        disabledEffect.Intensity = 2;
                        hub.playerEffectsController.EnableEffect(disabledEffect, 5);
                    }
                }
            }

            if (__instance.serverGrenadeEffect != null)
            {
                Transform transform = __instance.transform;
                Object.Instantiate(__instance.serverGrenadeEffect, transform.position, transform.rotation);
            }

            string str = __instance.thrower != null ? __instance.thrower.hub.LoggedNameFromRefHub() : "(UNKNOWN)";
            ServerLogs.AddLog(ServerLogs.Modules.Logger, $"Player {str}'s {__instance.logName} grenade exploded.", ServerLogs.ServerLogType.GameEvent);
            __result = true;
            return false;
        }
    }
}
// <copyright file="PlayerEffectPatches.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Patches
{
    using CustomPlayerEffects;
    using Exiled.API.Features;
    using HarmonyLib;
    using MEC;
    using Mirror;
    using UnityEngine;

    [HarmonyPatch(typeof(PlayerEffect), nameof(Scp268.ServerDisable))]
    internal static class PlayerEffectServerDisablePatch
    {
        public static bool Prefix(Scp268 __instance)
        {
            Player player = Player.Get(__instance.Hub);
            if (player == null)
            {
                return true;
            }

            if (TrackingAndMethods.PlayersVenting.Contains(player))
            {
                return false;
            }

            if (TrackingAndMethods.PlayersInvisibleByCommand.Contains(player))
            {
                return false;
            }

            if (!TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) || !TrackingAndMethods
                .PlayersWithSubclasses[player].Abilities.Contains(AbilityType.InvisibleUntilInteract))
            {
                // Log.Debug($"Player {player.Nickname} does not have subclass or invisibility", Subclass.Instance.Config.Debug);
                return true;
            }

            float cooldown = TrackingAndMethods.PlayersWithSubclasses[player].AbilityCooldowns[AbilityType.InvisibleUntilInteract];
            player.Broadcast((ushort)Mathf.Clamp(cooldown / 2, 0.5f, 3), TrackingAndMethods.PlayersWithSubclasses[player].StringOptions["AbilityCooldownMessage"].Replace("{ability}", "invisibility").Replace("{seconds}", cooldown.ToString()));
            Timing.CallDelayed(cooldown, () =>
            {
                if (TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) &&
                    TrackingAndMethods.PlayersWithSubclasses[player].Abilities
                        .Contains(AbilityType.InvisibleUntilInteract))
                {
                    player.ReferenceHub.playerEffectsController.EnableEffect<Scp268>();
                }
            });

            return true;
        }
    }

    [HarmonyPatch(typeof(Scp268), nameof(Scp268.PublicUpdate))]
    internal static class Scp268PublicUpdatePatch
    {
        public static bool Prefix(Scp268 __instance)
        {
            if (NetworkServer.active && __instance.Enabled)
            {
                __instance.curTime += Time.deltaTime;
                Player player = Player.Get(__instance.Hub);
                if (!(TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) && TrackingAndMethods
                        .PlayersWithSubclasses[player].Abilities.Contains(AbilityType.InvisibleUntilInteract)) &&
                    __instance.curTime > 15f)
                {
                    __instance.ServerDisable();
                }

                using (SyncList<Inventory.SyncItemInfo>.SyncListEnumerator enumerator = __instance.Hub.inventory.items.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current.id != ItemType.SCP268)
                        {
                            continue;
                        }
                    }
                }
            }

            if (__instance.Hub.inventory.isLocalPlayer)
            {
                if (__instance.Enabled)
                {
                    __instance.animationTime += Time.deltaTime;
                    if (__instance.animationTime > 1f)
                    {
                        __instance.animationTime = 1f;
                    }
                }
                else
                {
                    __instance.animationTime -= Time.deltaTime * 2f;
                    if (__instance.animationTime < 0f)
                    {
                        __instance.animationTime = 0f;
                    }
                }

                if (__instance.prevAnim != __instance.animationTime)
                {
                    bool flag2;
                    __instance.prevAnim = __instance.animationTime;
                    CameraFilterPack_Colors_Adjust_ColorRGB effect =
                        __instance.Hub.gfxController.CustomCameraEffects[1].Effect as
                            CameraFilterPack_Colors_Adjust_ColorRGB;
                    effect.enabled = flag2 = __instance.animationTime > 0f;
                    CameraFilterPack_TV_Vignetting vignetting1 =
                        __instance.Hub.gfxController.CustomCameraEffects[0].Effect as CameraFilterPack_TV_Vignetting;
                    vignetting1.enabled = flag2;
                    vignetting1.Vignetting = vignetting1.VignettingFull = __instance.animationTime;
                    vignetting1.VignettingColor = new Color32(0, 1, 2, 0xff);
                    effect.Blue = __instance.animationTime * 0.98f;
                    effect.Brightness = __instance.animationTime * -0.97f;
                    effect.Red = effect.Green = __instance.animationTime * 0.97f;
                }
            }

            return false;
        }
    }
}
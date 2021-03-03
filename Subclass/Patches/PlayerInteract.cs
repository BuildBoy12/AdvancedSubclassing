// <copyright file="PlayerInteract.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Patches
{
    using Exiled.API.Features;
    using HarmonyLib;

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdContain106))]
    internal static class PlayerInteractCallCmdContain106Patch
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            Player player = Player.Get(__instance._hub);
            if (player != null)
            {
                if (TrackingAndMethods.PlayersWithSubclasses.ContainsKey(player) && TrackingAndMethods.PlayersWithSubclasses[player].Abilities.Contains(AbilityType.CantActivateFemurBreaker))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
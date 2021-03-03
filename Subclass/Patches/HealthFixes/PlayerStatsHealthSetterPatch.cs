// <copyright file="PlayerStatsHealthSetterPatch.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Patches.HealthFixes
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using HarmonyLib;

    /// <summary>
    /// Prevents health from being reset at the start of the round.
    /// </summary>
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Health), MethodType.Setter)]
    internal static class PlayerStatsHealthSetterPatch
    {
        /// <summary>
        /// Uses <see cref="AdditionalInstructions.AddInstructions"/> to prevent issues with health resetting.
        /// </summary>
        /// <param name="instructions">The instructions of the original method.</param>
        /// <param name="generator">An instance of the <see cref="ILGenerator"/> class.</param>
        /// <returns>The adjusted code from <see cref="AdditionalInstructions.AddInstructions"/>.</returns>
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return AdditionalInstructions.AddInstructions(instructions, generator);
        }
    }
}
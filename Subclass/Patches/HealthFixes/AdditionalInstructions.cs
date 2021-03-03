// <copyright file="AdditionalInstructions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Patches.HealthFixes
{
#pragma warning disable SA1118

    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using Exiled.API.Features;
    using HarmonyLib;
    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Provides <see cref="AddInstructions"/> for health based methods in <see cref="PlayerStats"/>.
    /// </summary>
    public static class AdditionalInstructions
    {
        /// <summary>
        /// Prevents the game from resetting health to avoid issues where a subclasses health isn't properly set.
        /// </summary>
        /// <param name="instructions">The instructions of the original method.</param>
        /// <param name="generator">An instance of the <see cref="ILGenerator"/> class.</param>
        /// <returns>The updated instructions.</returns>
        public static List<CodeInstruction> AddInstructions(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = instructions.ToList();
            var local = generator.DeclareLocal(typeof(Player));
            var label = generator.DefineLabel();
            newInstructions[0].labels.Add(label);
            newInstructions.InsertRange(0, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(PlayerStats), nameof(PlayerStats.gameObject))),
                new CodeInstruction(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(UnityEngine.GameObject) })),
                new CodeInstruction(OpCodes.Stloc_0, local.LocalIndex),
                new CodeInstruction(OpCodes.Ldsfld, Field(typeof(TrackingAndMethods), nameof(TrackingAndMethods.PlayersThatJustGotAClass))),
                new CodeInstruction(OpCodes.Ldloc_0, local.LocalIndex),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(Dictionary<Player, float>), nameof(Dictionary<Player, float>.ContainsKey))),
                new CodeInstruction(OpCodes.Brfalse_S, label),
                new CodeInstruction(OpCodes.Ldsfld, Field(typeof(TrackingAndMethods), nameof(TrackingAndMethods.PlayersThatJustGotAClass))),
                new CodeInstruction(OpCodes.Ldloc_0, local.LocalIndex),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Dictionary<Player, float>), "Item")),
                new CodeInstruction(OpCodes.Ldc_R4, 7f),
                new CodeInstruction(OpCodes.Add),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(UnityEngine.Time), nameof(UnityEngine.Time.time))),
                new CodeInstruction(OpCodes.Ble_Un_S, label),
                new CodeInstruction(OpCodes.Call, Method(typeof(TrackingAndMethods), nameof(TrackingAndMethods.RoundJustStarted))),
                new CodeInstruction(OpCodes.Brfalse, label),
                new CodeInstruction(OpCodes.Ret),
            });

            return newInstructions;
        }
    }
}
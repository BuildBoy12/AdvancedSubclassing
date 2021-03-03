// <copyright file="Scp096Patches.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Subclass.Patches
{
#pragma warning disable SA1118

    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using Exiled.API.Features;
    using HarmonyLib;
    using static HarmonyLib.AccessTools;

    [HarmonyPatch(typeof(PlayableScps.Scp096), nameof(PlayableScps.Scp096.UpdateVision))]
    internal static class Scp096UpdateVisionPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = instructions.ToList();
            var local = generator.DeclareLocal(typeof(Player));
            var ldloc3Label = generator.DefineLabel();
            var ldlocasLabel = generator.DefineLabel();

            var offset = newInstructions.FindIndex(c => c.opcode == OpCodes.Stloc_3) + 1;
            var ldlocasIndex = newInstructions.FindLastIndex(c => c.opcode == OpCodes.Ldnull) + 6;
            newInstructions[offset].labels.Add(ldloc3Label);
            newInstructions[ldlocasIndex].labels.Add(ldlocasLabel);

            newInstructions.InsertRange(offset, new[]
            {
                new CodeInstruction(OpCodes.Ldloc_3),
                new CodeInstruction(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })),
                new CodeInstruction(OpCodes.Stloc_S, local.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_S, local.LocalIndex),
                new CodeInstruction(OpCodes.Brfalse_S, ldloc3Label),
                new CodeInstruction(OpCodes.Ldsfld, Field(typeof(TrackingAndMethods), nameof(TrackingAndMethods.PlayersWithSubclasses))),
                new CodeInstruction(OpCodes.Ldloc_S, local.LocalIndex),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(Dictionary<Player, SubClass>), nameof(Dictionary<Player, SubClass>.ContainsKey), new[] { typeof(Player) })),
                new CodeInstruction(OpCodes.Brfalse_S, ldloc3Label),
                new CodeInstruction(OpCodes.Ldsfld, Field(typeof(TrackingAndMethods), nameof(TrackingAndMethods.PlayersWithSubclasses))),
                new CodeInstruction(OpCodes.Ldloc_S, local.LocalIndex),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Dictionary<Player, SubClass>), "Item")),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(SubClass), nameof(SubClass.Abilities))),
                new CodeInstruction(OpCodes.Ldc_I4_8),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(List<AbilityType>), nameof(List<AbilityType>.Contains), new[] { typeof(AbilityType) })),
                new CodeInstruction(OpCodes.Brtrue, ldlocasLabel),
            });

            foreach (var code in newInstructions)
            {
                yield return code;
            }
        }
    }

    [HarmonyPatch(typeof(PlayableScps.Scp096), nameof(PlayableScps.Scp096.OnDamage))]
    internal static class Scp096OnDamagePatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = instructions.ToList();

            var returnLabel = newInstructions.First(i => i.opcode == OpCodes.Brfalse_S).operand;
            var continueLabel = generator.DefineLabel();
            var firstOffset = newInstructions.FindIndex(i => i.opcode == OpCodes.Ldloc_0) + 1;

            var player = generator.DeclareLocal(typeof(Player));

            newInstructions.InsertRange(firstOffset, new[]
            {
                new CodeInstruction(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(UnityEngine.GameObject) })),
                new CodeInstruction(OpCodes.Stloc_1, player.LocalIndex),
                new CodeInstruction(OpCodes.Ldloc_0),
            });

            var secondOffset = newInstructions.FindIndex(i => i.opcode == OpCodes.Ldnull) + 3;

            var secondInstructions = new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Brfalse_S, continueLabel),
                new CodeInstruction(OpCodes.Ldsfld, Field(typeof(TrackingAndMethods), nameof(TrackingAndMethods.PlayersWithSubclasses))),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(Dictionary<Player, SubClass>), nameof(Dictionary<Player, SubClass>.ContainsKey), new[] { typeof(Player) })),
                new CodeInstruction(OpCodes.Brfalse_S, continueLabel),
                new CodeInstruction(OpCodes.Ldsfld, Field(typeof(TrackingAndMethods), nameof(TrackingAndMethods.PlayersWithSubclasses))),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Dictionary<Player, SubClass>), "Item")),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(SubClass), nameof(SubClass.Abilities))),
                new CodeInstruction(OpCodes.Ldc_I4_8),
                new CodeInstruction(OpCodes.Callvirt, Method(typeof(List<AbilityType>), nameof(List<AbilityType>.Contains), new[] { typeof(AbilityType) })),
                new CodeInstruction(OpCodes.Brtrue, returnLabel),
            };

            newInstructions.InsertRange(secondOffset, secondInstructions);

            newInstructions[secondOffset + secondInstructions.Length].labels.Add(continueLabel);

            foreach (var code in newInstructions)
            {
                yield return code;
            }
        }
    }
}
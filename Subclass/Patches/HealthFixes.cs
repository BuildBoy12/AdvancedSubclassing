namespace Subclass.Patches
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using Exiled.API.Features;
    using HarmonyLib;
    using static HarmonyLib.AccessTools;

    internal static class HealthFixes
    {
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
                new CodeInstruction(OpCodes.Call,
                    Method(typeof(Player), nameof(Player.Get), new[] {typeof(UnityEngine.GameObject)})),
                new CodeInstruction(OpCodes.Stloc_0, local.LocalIndex),
                new CodeInstruction(OpCodes.Ldsfld,
                    Field(typeof(TrackingAndMethods), nameof(TrackingAndMethods.PlayersThatJustGotAClass))),
                new CodeInstruction(OpCodes.Ldloc_0, local.LocalIndex),
                new CodeInstruction(OpCodes.Callvirt,
                    Method(typeof(Dictionary<Player, float>), nameof(Dictionary<Player, float>.ContainsKey))),
                new CodeInstruction(OpCodes.Brfalse_S, label),
                new CodeInstruction(OpCodes.Ldsfld,
                    Field(typeof(TrackingAndMethods), nameof(TrackingAndMethods.PlayersThatJustGotAClass))),
                new CodeInstruction(OpCodes.Ldloc_0, local.LocalIndex),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Dictionary<Player, float>), "Item")),
                new CodeInstruction(OpCodes.Ldc_R4, 7f),
                new CodeInstruction(OpCodes.Add),
                new CodeInstruction(OpCodes.Call,
                    PropertyGetter(typeof(UnityEngine.Time), nameof(UnityEngine.Time.time))),
                new CodeInstruction(OpCodes.Ble_Un_S, label),
                new CodeInstruction(OpCodes.Call,
                    Method(typeof(TrackingAndMethods), nameof(TrackingAndMethods.RoundJustStarted))),
                new CodeInstruction(OpCodes.Brfalse, label),
                new CodeInstruction(OpCodes.Ret)
            });

            return newInstructions;
        }
    }

    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Health), MethodType.Setter)]
    internal static class PlayerStatsHealthSetterPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = HealthFixes.AddInstructions(instructions, generator);

            foreach (var code in newInstructions)
            {
                yield return code;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.maxHP), MethodType.Setter)]
    internal static class PlayerStatsMaxHealthSetterPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = HealthFixes.AddInstructions(instructions, generator);

            foreach (var code in newInstructions)
            {
                yield return code;
            }
        }
    }
}
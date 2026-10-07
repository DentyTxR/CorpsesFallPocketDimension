using CommandSystem.Commands.RemoteAdmin.Cleanup;
using CorpsesFallPocketDimension;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using MEC;
using PlayerRoles.PlayableScps.Scp106;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace CorpsesFallPocketDimension
{
    [HarmonyPatch(typeof(Scp106PocketItemManager), nameof(Scp106PocketItemManager.Update))]
    public static class Scp106PocketItemManagerTranspiler
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchStartForward(new CodeMatch(i =>
                i.opcode == OpCodes.Callvirt &&
                i.operand is MethodInfo mi &&
                mi.Name == nameof(Component.TryGetComponent)
            ));

            if (matcher.IsInvalid)
            {
                return instructions;
            }

            matcher.SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Scp106PocketItemManagerTranspiler), nameof(CustomDrop))));

            return matcher.Instructions();
        }

        private static bool CustomDrop(ItemPickupBase key, out Rigidbody rigidbody)
        {
            rigidbody = null;

            if (UnityEngine.Random.value > Main.Singleton.Config.ChanceToDropCorpse && CorpseTracker.PlayerRagdolls.Count > 0)
            {
                if (Scp106PocketItemManager.TrackedItems.TryGetValue(key, out var pocketItem))
                {
                    Vector3 dropPos = pocketItem.DropPosition.Position;
#if EXILED
                    Exiled.API.Features.Log.Debug($"picked ragdoll drop at position {dropPos}");
#elif LABAPI
                    LabApi.Features.Console.Logger.Debug($"picked ragdoll drop at position {dropPos}");
#endif

                    SpawnPocketRagdoll(dropPos);
                }

                Timing.CallDelayed(0.1f, () =>
                {
                    if (key != null)
                    {
                        key.DestroySelf();
                    }
                });

                return false;
            }
#if EXILED
            Exiled.API.Features.Log.Debug($"picked random item drop");
#elif LABAPI
            LabApi.Features.Console.Logger.Debug($"picked random item drop");

#endif
            return key.TryGetComponent(out rigidbody);
        }

        private static void SpawnPocketRagdoll(Vector3 position)
        {
            CorpseTracker.PickRandomRagdoll(position);
        }
    }
}

[HarmonyPatch(typeof(CorpsesCommand), nameof(CorpsesCommand.Execute))]
public static class PatchCleanupCommand
{
    public static void Postfix(bool __result)
    {
        if (!__result)
            return;
#if EXILED
        Exiled.API.Features.Log.Warn("basegame corpse cleanup command was called, clearing corpsetracker list");
#elif LABAPI
        LabApi.Features.Console.Logger.Warn("basegame corpse cleanup command was called, clearing corpsetracker list");
#endif

        CorpseTracker.PlayerRagdolls.RemoveAll(CorpseTracker.IsRagdollInvalid);
    }
}
using CommandSystem.Commands.RemoteAdmin.Cleanup;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using MEC;
using PlayerRoles.PlayableScps.Scp106;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace SCP106RagdollFallExiled
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

            if (UnityEngine.Random.Range(0, 2) == 1 && CorpseTracker.PlayerRagdolls.Count > 0 && Scp106PocketItemManager.TrackedItems.TryGetValue(key, out var pocketItem))
            {
                Vector3 rawPosition = pocketItem.DropPosition.Position;
                bool isSurface = Room.Get(rawPosition)?.Zone == ZoneType.Surface;

                if (rawPosition == Vector3.zero || isSurface)
                {
                    Player fallbackTarget = Player.List.FirstOrDefault(p => p.IsAlive && !p.IsInPocketDimension && p.Zone != ZoneType.Surface);

                    if (fallbackTarget != null)
                    {
                        rawPosition = fallbackTarget.Position;
                    }
                    else
                    {
                        var validRooms = Room.List.Where(r => r.Zone != ZoneType.Surface).ToList();

                        if (validRooms.Count > 0)
                        {
                            rawPosition = validRooms[UnityEngine.Random.Range(0, validRooms.Count)].Position;
                        }
                    }
                }

                if (rawPosition != Vector3.zero)
                {
                    Vector3 dropPos = rawPosition + new Vector3(0, 2.8f, 0);

                    Log.Debug($"picked ragdoll drop at position {dropPos}");
                    CorpseTracker.PickRandomRagdoll(dropPos);

                    Timing.CallDelayed(0.1f, () =>
                    {
                        if (key != null)
                        {
                            key.DestroySelf();
                        }
                    });

                    return false;
                }
            }

            Log.Debug($"picked random itemdrop");
            return key.TryGetComponent(out rigidbody);
        }
    }

    [HarmonyPatch(typeof(CorpsesCommand), nameof(CorpsesCommand.Execute))]
    public static class PatchCleanupCommand
    {
        public static void Postfix(bool __result)
        {
            if (!__result)
                return;

            Log.Warn("basegame corpse cleanup command was called, clearing corpsetracker list");
            CorpseTracker.PlayerRagdolls.RemoveAll(r => r == null || r.GameObject == null);
        }
    }
}
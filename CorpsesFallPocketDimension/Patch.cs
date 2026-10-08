using CommandSystem.Commands.RemoteAdmin.Cleanup;
using CorpsesFallPocketDimension.Components;
using CorpsesFallPocketDimension.Features;
using HarmonyLib;
using Hazards;
using InventorySystem.Items.Pickups;
using MEC;
using Mirror;
using PlayerRoles.PlayableScps.Scp106;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace CorpsesFallPocketDimension
{
    [HarmonyPatch(typeof(Scp106PocketItemManager), nameof(Scp106PocketItemManager.Update))]
    public static class Scp106PocketItemManagerTranspiler
    {
        private static SinkholeEnvironmentalHazard _cachedSinkholePrefab;
        private static bool _chanceTriggered = false;

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher.MatchStartForward(new CodeMatch(i =>
                i.opcode == OpCodes.Call &&
                i.operand is MethodInfo mi &&
                mi.Name == nameof(NetworkServer.SendToAll)
            ));

            if (matcher.IsValid)
            {
                matcher.SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Scp106PocketItemManagerTranspiler), nameof(CustomWarning))));
            }

            matcher.MatchStartForward(new CodeMatch(i =>
                i.opcode == OpCodes.Callvirt &&
                i.operand is MethodInfo mi &&
                mi.Name == nameof(Component.TryGetComponent)
            ));

            if (matcher.IsValid)
            {
                matcher.SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Scp106PocketItemManagerTranspiler), nameof(CustomDrop))));
            }

            return matcher.Instructions();
        }

        private static void CustomWarning(Scp106PocketItemManager.WarningMessage msg, int channelId, bool record)
        {
            bool rollPassed = UnityEngine.Random.value < Main.Singleton.Config.ChanceToDropCorpse;

            _chanceTriggered = rollPassed && CorpseTracker.PlayerRagdolls.Count > 0;
#if EXILED
            if (_chanceTriggered && Main.Singleton.Config.ApplyCustomSinkholeToCorpses)
#elif LABAPI
            if (_chanceTriggered && Main.Singleton.Config.ApplyCustomSinkholeToCorpses)
#endif
            {
                NetworkServer.SendToAll(msg, channelId, record);
#if EXILED
                Timing.CallDelayed(Main.Singleton.Config.SinkholeDelay, () =>
#elif LABAPI
                Timing.CallDelayed(Main.Singleton.Config.SinkholeDelay, () =>
#endif
                {
                    SpawnSinkhole(msg.Position.Position);
                });
            }
#if EXILED
            else if (Main.Singleton.Config.ApplyCustomSinkholeToItems)
#elif LABAPI
            else if (Main.Singleton.Config.ApplyCustomSinkholeToItems)
#endif
            {
                NetworkServer.SendToAll(msg, channelId, record);
#if EXILED
                Timing.CallDelayed(Main.Singleton.Config.SinkholeDelay, () =>
#elif LABAPI
                Timing.CallDelayed(Main.Singleton.Config.SinkholeDelay, () =>
#endif
                {
                    SpawnSinkhole(msg.Position.Position);
                });
            }
            else
            {
                NetworkServer.SendToAll(msg, channelId, record);
            }
        }

        private static bool CustomDrop(ItemPickupBase pickup, out Rigidbody rigidbody)
        {
            rigidbody = null;

            if (_chanceTriggered)
            {
                if (Scp106PocketItemManager.TrackedItems.TryGetValue(pickup, out var pocketItem))
                {
                    Vector3 dropPos = pocketItem.DropPosition.Position;
#if EXILED
                    Exiled.API.Features.Log.Debug($"[PocketDrop] Spawning ragdoll at {dropPos}");
#elif LABAPI
                    LabApi.Features.Console.Logger.Debug($"[PocketDrop] Spawning ragdoll at {dropPos}", Main.Singleton.Config.Debug);
#endif

                    SpawnPocketRagdoll(dropPos);
                }

                Timing.CallDelayed(0.1f, () =>
                {
                    if (pickup != null)
                    {
                        pickup.DestroySelf();
                    }
                });

                return false;
            }

#if EXILED
            Exiled.API.Features.Log.Debug($"[PocketDrop] Dropping regular item");
#elif LABAPI
            LabApi.Features.Console.Logger.Debug($"[PocketDrop] Dropping regular item", Main.Singleton.Config.Debug);
#endif

            return pickup.TryGetComponent(out rigidbody);
        }

        private static void SpawnPocketRagdoll(Vector3 position)
        {
            CorpseTracker.PickRandomRagdoll(position);

            DecalRpcCache.PlaceBlood(position, Vector3.down);
            DecalRpcCache.PlaceBlood(position, Vector3.down);
            DecalRpcCache.PlaceBlood(position, Vector3.down);
        }

        private static void SpawnSinkhole(Vector3 position)
        {
            if (_cachedSinkholePrefab == null)
            {
                foreach (GameObject prefab in NetworkClient.prefabs.Values)
                {
                    if (prefab.TryGetComponent(out SinkholeEnvironmentalHazard foundHazard))
                    {
                        _cachedSinkholePrefab = foundHazard;
                        break;
                    }
                }
            }

            if (_cachedSinkholePrefab == null) return;

            var fixedPosition = new Vector3(position.x, position.y - 0.1f, position.z);

            SinkholeEnvironmentalHazard hazardInstance = UnityEngine.Object.Instantiate(_cachedSinkholePrefab, fixedPosition, Quaternion.identity);
            hazardInstance.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            hazardInstance.transform.rotation = Quaternion.Euler(180f, 0f, 0f);

            hazardInstance.gameObject.AddComponent<CustomHazardComponent>();

            hazardInstance.IsActive = true;

            NetworkServer.Spawn(hazardInstance.gameObject);

            Timing.CallDelayed(4f, () =>
            {
                if (hazardInstance != null && hazardInstance.gameObject)
                {
                    NetworkServer.Destroy(hazardInstance.gameObject);
                }
            });
        }
    }
}

[HarmonyPatch(typeof(SinkholeEnvironmentalHazard), nameof(SinkholeEnvironmentalHazard.OnEnter))]
public static class PatchSinkholeOnEnter
{
    public static bool Prefix(SinkholeEnvironmentalHazard __instance, ReferenceHub player, ref bool __result)
    {
        if (__instance.TryGetComponent<CustomHazardComponent>(out _))
        {
            __result = false;
            return false;
        }
        return true;
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
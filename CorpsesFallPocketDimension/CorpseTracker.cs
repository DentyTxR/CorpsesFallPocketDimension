#if EXILED

using Exiled.API.Features;

#elif LABAPI

using LabApi.Features.Wrappers;

#endif

using Mirror;
using PlayerRoles.Ragdolls;
using RelativePositioning;
using UnityEngine;

namespace CorpsesFallPocketDimension
{
    public class CorpseTracker
    {
        public static List<Ragdoll> PlayerRagdolls = new();

        public static bool IsRagdollInvalid(Ragdoll r)
        {
            if (r == null) return true;
            try
            {
#if EXILED
                return r.GameObject == null;
#elif LABAPI

                return r.Base.gameObject == null;
#endif
            }
            catch
            {
                return true;
            }
        }

        public static void Register(Ragdoll ragdoll)
        {
            if (ragdoll != null)
            {
#if EXILED
                Log.Debug($"registering ragdoll: {ragdoll.GameObject.name}");

#elif LABAPI
                LabApi.Features.Console.Logger.Debug($"registering ragdoll: {ragdoll.Base.gameObject.name}");

#endif
                PlayerRagdolls.Add(ragdoll);
            }
        }

        public static void PickRandomRagdoll(Vector3 position)
        {
            PlayerRagdolls.RemoveAll(IsRagdollInvalid);

            if (PlayerRagdolls.Count == 0)
            {
#if EXILED
                Log.Warn("no ragdolls found");
#elif LABAPI
                LabApi.Features.Console.Logger.Warn("no ragdolls found");

#endif
                return;
            }
            int randomIndex = UnityEngine.Random.Range(0, PlayerRagdolls.Count);
            Ragdoll pickedRagdoll = PlayerRagdolls[randomIndex];
#if EXILED
            GameObject ragdollObj = pickedRagdoll.GameObject;

#elif LABAPI
            GameObject ragdollObj = pickedRagdoll.Base.gameObject;

#endif

#if EXILED
            Log.Debug($"teleporting actual ragdoll: {ragdollObj.name}");
#elif LABAPI
            LabApi.Features.Console.Logger.Debug($"teleporting actual ragdoll: {ragdollObj.name}");
#endif

            if (ragdollObj.TryGetComponent<BasicRagdoll>(out var basicRagdoll))
            {
                RagdollData originalData = basicRagdoll.Info;

                NetworkServer.Destroy(ragdollObj);

#if EXILED
                RagdollData updatedData = new RagdollData(
                    originalData.OwnerHub,
                    originalData.Handler,
                    originalData.RoleType,
                    new RelativePosition(position),
                    originalData.StartRelativeRotation,
                    originalData.Scale,
                    originalData.Nickname,
                    NetworkTime.time
                );

                Ragdoll newRagdoll = Ragdoll.CreateAndSpawn(updatedData);

#elif LABAPI
                Quaternion rotation = originalData.StartRelativeRotation;

                Ragdoll newRagdoll = Ragdoll.SpawnRagdoll(
                    originalData.RoleType,
                    position,
                    rotation,
                    originalData.Handler,
                    originalData.Nickname,
                    null,
                    null,
                    originalData.OwnerHub
                );
#endif

                PlayerRagdolls.RemoveAt(randomIndex);
            }
        }
    }
}
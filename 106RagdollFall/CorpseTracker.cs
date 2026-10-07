using Exiled.API.Features;
using Mirror;
using PlayerRoles.Ragdolls;
using RelativePositioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCP106RagdollFall
{
    public class CorpseTracker
    {
        public static List<Ragdoll> PlayerRagdolls = new();

        public static void Register(Ragdoll ragdoll)
        {
            if (ragdoll != null)
            {
                Log.Debug($"registering ragdoll: {ragdoll.GameObject.name}");
                PlayerRagdolls.Add(ragdoll);
            }
        }

        public static void PickRandomRagdoll(Vector3 position)
        {
            PlayerRagdolls.RemoveAll(r => r == null || r.GameObject == null);

            if (PlayerRagdolls.Count == 0)
            {
                Log.Warn("no ragdolls found");
                return;
            }

            int randomIndex = UnityEngine.Random.Range(0, PlayerRagdolls.Count);
            Ragdoll pickedRagdoll = PlayerRagdolls[randomIndex];
            GameObject ragdollObj = pickedRagdoll.GameObject;

            Log.Debug($"teleporting actual ragdoll: {ragdollObj.name}");

            if (ragdollObj.TryGetComponent<BasicRagdoll>(out var basicRagdoll))
            {
                RagdollData originalData = basicRagdoll.Info;

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

                NetworkServer.Destroy(ragdollObj);

                Ragdoll newRagdoll = Ragdoll.CreateAndSpawn(updatedData);
                PlayerRagdolls.RemoveAt(randomIndex);
            }
        }
    }
}
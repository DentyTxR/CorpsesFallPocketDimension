#if EXILED

using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;

#elif LABAPI

#endif

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Console;

namespace CorpsesFallPocketDimension
{
    public class EventHandler
    {
        public void OnWaitingForPlayers()
        {
            CorpseTracker.PlayerRagdolls.Clear();
        }

#if EXILED
        public void SpawnedRagdoll(SpawnedRagdollEventArgs ev)
        {
            if (ev.Ragdoll.Room.Type == RoomType.Pocket)
            {
                CorpseTracker.Register(ev.Ragdoll);
                Log.Debug($"ragdoll spawned for {ev.Player.UserId} while in pocket dimension, total tracked corpses: {CorpseTracker.PlayerRagdolls.Count}");
            }
        }
#elif LABAPI

        public void SpawnedRagdoll(PlayerSpawnedRagdollEventArgs ev)
        {
            if (ev.Player.Room.Name == MapGeneration.RoomName.Pocket)
            {
                CorpseTracker.Register(ev.Ragdoll);
                Logger.Debug($"ragdoll spawned for {ev.Player.UserId} while in pocket dimension, total tracked corpses: {CorpseTracker.PlayerRagdolls.Count}");
            }
        }

#endif
    }
}
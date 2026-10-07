using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events;
using Exiled.Events.EventArgs.Player;
using GameCore;
using MEC;

namespace SCP106RagdollFall
{
    public class EventHandler
    {
        public void Spawning(SpawningEventArgs ev)
        {
        }

        public void SpawnedRagdoll(SpawnedRagdollEventArgs ev)
        {
            Log.Info(ev.Ragdoll.Room.Type.ToString());
            Log.Info(ev.Player.CurrentRoom.ToString());
            if (ev.Ragdoll.Room.Type == RoomType.Pocket)
            {
                CorpseTracker.Register(ev.Ragdoll);
                Log.Info($"ragdoll spawned for {ev.Player.UserId} while in pocket dimension, total tracked corpses: {CorpseTracker.PlayerRagdolls.Count}");
            }
        }
    }
}
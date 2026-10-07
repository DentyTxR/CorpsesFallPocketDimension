using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;

namespace CorpsesFallPocketDimension
{
    public class EventHandler
    {
        public void OnWaitingForPlayers()
        {
            CorpseTracker.PlayerRagdolls.Clear();
        }

        public void SpawnedRagdoll(SpawnedRagdollEventArgs ev)
        {
            if (ev.Ragdoll.Room.Type == RoomType.Pocket)
            {
                CorpseTracker.Register(ev.Ragdoll);
                Log.Debug($"ragdoll spawned for {ev.Player.UserId} while in pocket dimension, total tracked corpses: {CorpseTracker.PlayerRagdolls.Count}");
            }
        }
    }
}
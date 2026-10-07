using Exiled.API.Features;
using HarmonyLib;
using System.Reflection;

namespace SCP106RagdollFallExiled
{
    public class Main : Plugin<Config>
    {
        public override string Name => "scp106RagdollFall";
        public override string Author => "Denty";
        public override Version Version { get; } = new(1, 0, 0);

        public static Main Singleton;
        private EventHandler EventHandler;
        public static Harmony Harmony;

        public override void OnEnabled()
        {
            Singleton = this;
            EventHandler = new EventHandler();

            Harmony = new Harmony($"{Author}.{Name}");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            Exiled.Events.Handlers.Player.Spawning += EventHandler.Spawning;
            Exiled.Events.Handlers.Player.SpawnedRagdoll += EventHandler.SpawnedRagdoll;
        }

        public override void OnDisabled()
        {
            Singleton = null;

            Harmony.UnpatchAll(Harmony.Id);

            Exiled.Events.Handlers.Player.Spawning -= EventHandler.Spawning;
            Exiled.Events.Handlers.Player.SpawnedRagdoll -= EventHandler.SpawnedRagdoll;

            EventHandler = null;
        }
    }
}
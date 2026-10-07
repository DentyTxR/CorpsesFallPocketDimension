using Exiled.API.Features;
using HarmonyLib;
using System.Reflection;

namespace CorpsesFallPocketDimension
{
    public class Main : Plugin<Config>
    {
        public override string Name => "CorpsesFallPocketDimension";
        public override string Author => "Denty";
        public override Version Version { get; } = new(1, 0, 1);

        public static Main Singleton;
        private EventHandler EventHandler;
        public static Harmony Harmony;

        public override void OnEnabled()
        {
            Singleton = this;
            EventHandler = new EventHandler();

            Harmony = new Harmony($"{Author}.{Name}");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            Exiled.Events.Handlers.Server.WaitingForPlayers += EventHandler.OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.SpawnedRagdoll += EventHandler.SpawnedRagdoll;
        }

        public override void OnDisabled()
        {
            Singleton = null;

            Harmony.UnpatchAll(Harmony.Id);

            Exiled.Events.Handlers.Server.WaitingForPlayers -= EventHandler.OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.SpawnedRagdoll -= EventHandler.SpawnedRagdoll;

            EventHandler = null;
        }
    }
}
using HarmonyLib;
using System.Reflection;

#if EXILED

using Exiled.API.Features;

#elif LABAPI

using LabApi.Loader.Features.Plugins;

#endif

namespace CorpsesFallPocketDimension
{
#if EXILED

    public class Main : Plugin<Config>
    {
        public override string Name => "CorpsesFallPocketDimensionExiled";
        public override string Author => "Denty";
        public override Version Version { get; } = new(1, 1, 0);

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

#elif LABAPI

    public class Main : Plugin<Config>
    {
        public override string Name => "CorpsesFallPocketDimensionLabApi";
        public override string Description => "Simple plugin that makes corpses from pocket dimension fall just like items";
        public override string Author => "Denty";
        public override Version RequiredApiVersion => new Version("1.1.0");

        public static Main Singleton;
        private EventHandler EventHandler;
        public static Harmony Harmony;

        public override void Enable()
        {
            Singleton = this;
            EventHandler = new EventHandler();

            Harmony = new Harmony($"{Author}.{Name}");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            LabApi.Events.Handlers.ServerEvents.WaitingForPlayers += EventHandler.OnWaitingForPlayers;
            LabApi.Events.Handlers.PlayerEvents.SpawnedRagdoll += EventHandler.SpawnedRagdoll;
        }

        public override void Disable()
        {
            Singleton = null;

            Harmony.UnpatchAll(Harmony.Id);

            LabApi.Events.Handlers.ServerEvents.WaitingForPlayers -= EventHandler.OnWaitingForPlayers;
            LabApi.Events.Handlers.PlayerEvents.SpawnedRagdoll -= EventHandler.SpawnedRagdoll;

            EventHandler = null;
        }
    }

#endif
}
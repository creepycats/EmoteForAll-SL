using Exiled.API.Features;
using HarmonyLib;
using System;
using System.Runtime.InteropServices;

namespace EmoteForAll
{
    public class EmoteForAll : Plugin<Config.Config>
    {
        public override string Name => "EmoteForAll";
        public override string Author => "creepycats";
        public override Version Version => new Version(1, 1, 2);

        public static EmoteForAll Instance { get; set; }
        private Harmony _harmony;

        private handlers.playerHandler PlayerHandler;

        public override void OnEnabled()
        {
            Instance = this;
            Log.Info($"{Name} v{Version} - made for v13 by creepycats");

            if (_harmony is null)
            {
                _harmony = new("EmoteForAll");
                _harmony.PatchAll();
            }

            PlayerHandler = new handlers.playerHandler();

            Exiled.Events.Handlers.Player.Hurting += PlayerHandler.Hurting;

            Log.Info("Plugin Enabled!");
        }
        public override void OnDisabled()
        {
            if (_harmony is not null)
            {
                _harmony.UnpatchAll();
            }

            Exiled.Events.Handlers.Player.Hurting -= PlayerHandler.Hurting;

            Log.Info("Disabled Plugin Successfully");
        }
    }
}
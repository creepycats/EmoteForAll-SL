using Exiled.API.Features;
using HarmonyLib;
using System;

namespace EmoteForAll
{
    public class EmoteForAll : Plugin<Config.Config>
    {
        public override string Name => "EmoteForAll";
        public override string Author => "creepycats";
        public override Version Version => new Version(1, 0, 0);

        public static EmoteForAll Instance { get; set; }

        private Harmony _harmony;

        public override void OnEnabled()
        {
            Instance = this;
            Log.Info($"{Name} v{Version} - made for v13 by creepycats");

            if (_harmony is null)
            {
                _harmony = new("EmoteForAll");
                _harmony.PatchAll();
            }

            Log.Info("Plugin Enabled!");
        }
        public override void OnDisabled()
        {
            if (_harmony is not null)
            {
                _harmony.UnpatchAll();
            }

            Log.Info("Disabled Plugin Successfully");
        }
    }
}
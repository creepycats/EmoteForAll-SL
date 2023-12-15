using Exiled.API.Features;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp3114;

namespace EmoteForAll.Patches
{
    [HarmonyPatch(typeof(Scp3114VoiceLines), nameof(Scp3114VoiceLines.ServerPlayConditionally))]
    internal static class ServerPlayConditionally
    {
        [HarmonyPrefix]
        private static bool Prefix(ref Scp3114VoiceLines __instance, Scp3114VoiceLines.VoiceLinesName lineToPlay)
        {
            if (Player.Get(__instance.Owner).IsNPC)
                return false;

            return true;
        }
    }
}

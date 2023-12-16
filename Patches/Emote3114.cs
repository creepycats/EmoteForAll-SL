using Exiled.API.Features;
using HarmonyLib;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerRoles.Ragdolls;
using Respawning.NamingRules;
using Respawning;
using UnityEngine;
using System.Text;
using PlayerStatsSystem;
using EmoteForAll.Classes;
using System.Linq;

namespace EmoteForAll.Patches
{
    [HarmonyPatch(typeof(Scp3114VoiceLines), nameof(Scp3114VoiceLines.ServerPlayConditionally))]
    internal static class ServerPlayConditionally
    {
        [HarmonyPrefix]
        private static bool Prefix(ref Scp3114VoiceLines __instance, Scp3114VoiceLines.VoiceLinesName lineToPlay)
        {
            if (EmoteHandler.emoteAttachedNPC.Values.Contains(Npc.Get(__instance.Owner)))
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(HumeShieldStat), nameof(HumeShieldStat.Update))]
    internal static class Update
    {
        [HarmonyPrefix]
        private static bool Prefix(ref HumeShieldStat __instance)
        {
            if (EmoteHandler.emoteAttachedNPC.Values.Contains(Npc.Get(__instance.Hub)))
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(Scp3114Role), nameof(Scp3114Role.TryPreventHitmarker))]
    internal static class TryPreventHitmarker
    {
        [HarmonyPostfix]
        private static void Postfix(ref Scp3114Role __instance, AttackerDamageHandler adh, ref bool __result)
        {
            ReferenceHub rhub;
            __instance.TryGetOwner(out rhub);
            Npc npc = Npc.Get(rhub);
            if (npc != null && EmoteHandler.emoteAttachedNPC.Values.Contains(npc))
            {
                __result = !HitboxIdentity.CheckFriendlyFire(adh.Attacker.Role, __instance.CurIdentity.StolenRole, false);
                EmoteHandler emHand;
                if (!__result && npc.GameObject.TryGetComponent<EmoteHandler>(out emHand))
                {
                    emHand.KillEmote(plrDamage: adh.DealtHealthDamage);
                }
            }
        }
    }
}

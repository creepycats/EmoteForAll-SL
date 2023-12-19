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
using Mirror;
using System.Collections.Generic;
using NorthwoodLib.Pools;
using System.Reflection.Emit;
using EmoteForAll.Types;

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

    [HarmonyPatch(typeof(Scp3114Dance), nameof(Scp3114Dance.ServerWriteRpc))]
    internal static class ServerWriteRpc
    {
        // Transpiler Intercepts the Written Bit
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            Label skip = generator.DefineLabel();

            newInstructions.Add(new CodeInstruction(OpCodes.Ret));
            newInstructions[newInstructions.Count - 1].labels.Add(skip);

            newInstructions.InsertRange(7, new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Br_S, skip),
            });

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

        // Postfix Replaces that writing function with our own
        [HarmonyPostfix]
        private static void Postfix(ref Scp3114Dance __instance, NetworkWriter writer)
        {
            Npc npc = Npc.Get(__instance.Owner);
            if (npc != null && EmoteHandler.emoteAttachedNPC.Values.Contains(npc))
            {
                EmoteHandler handle = npc.GameObject.GetComponent<EmoteHandler>();
                Scp3114DanceType danceType = handle.danceType;
                writer.WriteByte((byte)danceType);
                return;
            }
            writer.WriteByte((byte)Random.Range(0, 255)); // Default Dance
            return;
        }
    }
}

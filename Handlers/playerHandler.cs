using EmoteForAll.Classes;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using System.Linq;
using UnityEngine;

namespace EmoteForAll.handlers
{
    public class playerHandler
    {
        public void Hurting(HurtingEventArgs args)
        {
            if (args.Player == null) return;

            if (EmoteHandler.emoteAttachedNPC.Keys.Contains(args.Player.UserId))
            {
                EmoteHandler.emoteAttachedNPC[args.Player.UserId].GameObject.GetComponent<EmoteHandler>().KillEmote(plrDamage: args.Amount);
            } else
            {
                Npc checknpc = Npc.Get(args.Player.ReferenceHub);
                if (checknpc == null) return;
                if (EmoteHandler.emoteAttachedNPC.Values.Contains(checknpc))
                {
                    if (args.Attacker != null)
                    {
                        Player owner = Player.Get(checknpc.GameObject.GetComponent<EmoteHandler>().OwnerUserId);
                        if (owner != null && owner.Role.Side != args.Attacker.Role.Side)
                        {
                            checknpc.GameObject.GetComponent<EmoteHandler>().KillEmote(plrDamage: args.Amount);
                        }
                    }
                    args.IsAllowed = false;
                }
            }
        }
    }
}

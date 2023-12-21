using EmoteForAll.Types;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Components;
using Exiled.API.Features.Roles;
using MapEditorReborn.Commands.UtilityCommands;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerStatsSystem;
using RelativePositioning;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static PlayerRoles.PlayableScps.Scp3114.Scp3114Identity;

namespace EmoteForAll.Classes
{
    public class EmoteHandler : MonoBehaviour
    {
        public static Dictionary<string, Npc> emoteAttachedNPC = new Dictionary<string, Npc>();

        public static bool MakePlayerEmote(Player plr, Scp3114DanceType danceType)
        {
            if (plr.Role.Team == Team.Dead || plr.Role.Team == Team.SCPs) return false;
            if (emoteAttachedNPC.TryGetValue(plr.UserId, out Npc ExistingNpc)) {
                EmoteHandler handler = ExistingNpc.GameObject.GetComponent<EmoteHandler>();
                if (ExistingNpc.Role is Scp3114Role scpRole)
                {
                    handler.danceType = danceType;
                    scpRole.Dance.IsDancing = true;
                    scpRole.Dance._serverStartPos = new RelativePosition(scpRole.Dance.CastRole.FpcModule.Position);
                    scpRole.Dance.ServerSendRpc(true);
                    return true;
                }
                return false;
            }

            Npc EmoteNpc = SpawnFix($"{plr.Nickname}-emote", RoleTypeId.Scp3114, position: new Vector3(-9999f, -9999f, -9999f));
            emoteAttachedNPC.Add(plr.UserId, EmoteNpc);

            Round.IgnoredPlayers.Add(EmoteNpc.ReferenceHub);

            plr.ShowHint("<size=450>\n</size><color=yellow><size=35>Initializing Emote...</color>\n</size><size=25>Please Wait 5 Seconds...</size>", 5f);

            ReferenceHub rhub = plr.ReferenceHub;

            string uid = plr.UserId;

            Timing.CallDelayed(0.5f, () =>
            {
                EmoteNpc.Health = 9999f;
                EmoteNpc.HumeShield = 0f;
                EmoteNpc.Scale = plr.Scale;
                if (EmoteNpc.Role is Scp3114Role scpRole)
                {
                    Ragdoll ragdoll = Ragdoll.CreateAndSpawn(
                        plr.Role.Type,
                        plr.DisplayNickname,
                        "Placeholder",
                        new Vector3(-9999f, -9999f, -9999f),
                        plr.Rotation);
                    scpRole.Ragdoll = ragdoll;
                    scpRole.DisguiseStatus = DisguiseStatus.Equipping;
                }
            });

            Timing.CallDelayed(5f, () =>
            {
                EmoteNpc.Position = Player.Get(uid) != null ? Player.Get(uid).Position : Vector3.zero;

                EmoteHandler handler = EmoteNpc.GameObject.AddComponent<EmoteHandler>();
                handler.PlayerOwner = rhub;
                handler.NpcOwner = EmoteNpc;
                handler.OwnerUserId = uid;

                if (EmoteNpc.Role is Scp3114Role scpRole)
                {
                    handler.danceType = danceType;
                    scpRole.Dance.IsDancing = true;
                    scpRole.Dance._serverStartPos = new RelativePosition(scpRole.Dance.CastRole.FpcModule.Position);
                    scpRole.Dance.ServerSendRpc(true);
                }

                Vector3 realScale = plr.ReferenceHub.transform.localScale;
                plr.ReferenceHub.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                foreach (Player item in Player.List)
                {
                    if (item.UserId != plr.UserId)
                        Server.SendSpawnMessage?.Invoke(null, new object[2] { plr.NetworkIdentity, item.Connection });
                }
                plr.ReferenceHub.transform.localScale = realScale;
            });

            return true;
        }

        public static Npc SpawnFix(string name, RoleTypeId role, int id = 0, Vector3? position = null)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            Npc npc = new(gameObject)
            {
                IsNPC = true
            };
            try
            {
                npc.ReferenceHub.roleManager.InitializeNewRole(RoleTypeId.None, RoleChangeReason.None);
            }
            catch (Exception arg)
            {
                Log.Debug($"Ignore: {arg}");
            }

            if (RecyclablePlayerId.FreeIds.Contains(id))
            {
                RecyclablePlayerId.FreeIds.RemoveFromQueue(id);
            }
            else if (RecyclablePlayerId._autoIncrement >= id)
            {
                id = ++RecyclablePlayerId._autoIncrement;
            }

            NetworkServer.AddPlayerForConnection(new FakeConnection(id), gameObject);
            try
            {
                npc.ReferenceHub.authManager.SyncedUserId = null;
            }
            catch (Exception e)
            {
                Log.Debug($"Ignore: {e}");
            }

            npc.ReferenceHub.nicknameSync.Network_myNickSync = name;
            Player.Dictionary.Add(gameObject, npc);
            Timing.CallDelayed(0.3f, delegate
            {
                npc.Role.Set(role, SpawnReason.ForceClass, position.HasValue ? RoleSpawnFlags.AssignInventory : RoleSpawnFlags.All);
                npc.ClearInventory();
            });
            if (position.HasValue)
            {
                Timing.CallDelayed(0.5f, delegate
                {
                    npc.Position = position.Value;
                });
            }

            return npc;
        }

        // source for this method: o5zereth on discord
        public static (ushort horizontal, ushort vertical) ToClientUShorts(Quaternion rotation)
        {
            if (rotation.eulerAngles.z != 0f)
            {
                rotation = Quaternion.LookRotation(rotation * Vector3.forward, Vector3.up);
            }

            float outfHorizontal = rotation.eulerAngles.y;
            float outfVertical = -rotation.eulerAngles.x;

            if (outfVertical < -90f)
            {
                outfVertical += 360f;
            }
            else if (outfVertical > 270f)
            {
                outfVertical -= 360f;
            }

            return (ToHorizontal(outfHorizontal), ToVertical(outfVertical));

            static ushort ToHorizontal(float horizontal)
            {
                const float ToHorizontal = 65535f / 360f;

                horizontal = Mathf.Clamp(horizontal, 0f, 360f);

                return (ushort)Mathf.RoundToInt(horizontal * ToHorizontal);
            }

            static ushort ToVertical(float vertical)
            {
                const float ToVertical = 65535f / 176f;

                vertical = Mathf.Clamp(vertical, -88f, 88f) + 88f;

                return (ushort)Mathf.RoundToInt(vertical * ToVertical);
            }
        }

        public static void LookAt(Npc npc, Vector3 position)
        {
            Vector3 direction = position - npc.Position;
            Quaternion quat = Quaternion.LookRotation(direction, Vector3.up);
            var mouseLook = ((IFpcRole)npc.ReferenceHub.roleManager.CurrentRole).FpcModule.MouseLook;
            (ushort horizontal, ushort vertical) = ToClientUShorts(quat);
            mouseLook.ApplySyncValues(horizontal, vertical);
        }

        // ====================================================================================================
        // COMPONENT STUFF HERE
        // ====================================================================================================

        /// <summary>
        /// The ReferenceHub of the Player Owner
        /// </summary>
        public ReferenceHub PlayerOwner { get; set; }

        public string OwnerUserId { get; set; }

        public Npc NpcOwner { get; set; }

        public Scp3114DanceType danceType { get; set; }

        private bool KeepRunning { get; set; } = true;

        private long LastHintTime { get; set; } = 0;

        private void Start()
        {
            Timing.RunCoroutine(CheckIfKill().CancelWith(this).CancelWith(gameObject));
        }

        private IEnumerator<float> CheckIfKill()
        {
            while (KeepRunning)
            {
                yield return Timing.WaitForSeconds(0.1f);

                try
                {
                    long CurrentTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

                    Player plr = Player.Get(OwnerUserId);
                    if (plr == null)
                    {
                        KillEmote(true);
                        continue;
                    }
                    if (plr.Role.IsDead)
                    {
                        KillEmote();
                        continue;
                    }
                    if (plr.Scale != NpcOwner.Scale)
                    {
                        Vector3 realScale = plr.ReferenceHub.transform.localScale;
                        plr.ReferenceHub.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                        foreach (Player item in Player.List)
                        {
                            if (item.UserId != plr.UserId)
                                Server.SendSpawnMessage?.Invoke(null, new object[2] { plr.NetworkIdentity, item.Connection });
                        }
                        plr.ReferenceHub.transform.localScale = realScale;

                        NpcOwner.Scale = plr.Scale;
                    }
                    if (Vector3.Distance(NpcOwner.Position, PlayerOwner.transform.position) > 0.05)
                    {
                        KillEmote();
                        continue;
                    }
                    LookAt(NpcOwner, Player.Get(OwnerUserId).CameraTransform.forward + Player.Get(OwnerUserId).CameraTransform.position);

                    NpcOwner.Health = 9999f;
                    NpcOwner.HumeShield = 0f;

                    if (CurrentTime - LastHintTime > 500)
                    {
                        LastHintTime = CurrentTime;
                        plr.ShowHint($"<size=450>\n</size><size=35>Current Emote: <color=yellow>{Enum.GetName(typeof(Scp3114DanceType), danceType)}</color></size>\n<size=25><color=red>[Cancel]</color> by Moving.\nUse '.emote list' to see Available Emotes.</size>", 0.75f);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        public void KillEmote(bool skipOwner = false, float plrDamage = 0)
        {
            if (KeepRunning == false) return;

            KeepRunning = false;
            NpcOwner.Position = new Vector3(-9999f, -9999f, -9999f);
            if (!skipOwner)
            {
                Player ownerplr = Player.Get(PlayerOwner);
                ownerplr.ShowHint($"<size=450>\n</size><color=red><size=35>Emote Cancelled.</size></color>\n<size=25>" + (plrDamage != 0 ? "(You took damage from Someone)" : "(You moved)") + "</size>", 3f);

                foreach (Player item in Player.List)
                {
                    if (item.UserId != OwnerUserId)
                        Server.SendSpawnMessage?.Invoke(null, new object[2] { ownerplr.NetworkIdentity, item.Connection });
                }

                if (plrDamage != 0)
                {
                    ownerplr.Health -= plrDamage;
                    if (ownerplr.Health <= 0) ownerplr.Health = 1;
                }
            }
            Timing.CallDelayed(0.5f, () =>
            {
                emoteAttachedNPC.Remove(OwnerUserId);
                NetworkServer.Destroy(NpcOwner.GameObject);
            });
        }
    }
}

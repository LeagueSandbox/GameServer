﻿using ENet;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.NetInfo;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using PacketDefinitions420.Enums;
using PacketDefinitions420.PacketDefinitions;
using PacketDefinitions420.PacketDefinitions.C2S;
using PacketDefinitions420.PacketDefinitions.S2C;
using LeaguePackets.Game;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;
using PingLoadInfoRequest = GameServerCore.Packets.PacketDefinitions.Requests.PingLoadInfoRequest;
using ViewRequest = GameServerCore.Packets.PacketDefinitions.Requests.ViewRequest;
using LeaguePackets.Game.Common;
using LeaguePackets.Common;
using System.Linq;
using static GameServerCore.Content.HashFunctions;
using System.Text;
using Force.Crc32;

namespace PacketDefinitions420
{
    /// <summary>
    /// Class containing all function related packets (except handshake) which are sent by the server to game clients.
    /// </summary>
    public class PacketNotifier : IPacketNotifier
    {
        private readonly IPacketHandlerManager _packetHandlerManager;
        private readonly INavigationGrid _navGrid;

        /// <summary>
        /// Instantiation which preps PacketNotifier for packet sending.
        /// </summary>
        /// <param name="packetHandlerManager"></param>
        /// <param name="navGrid"></param>
        public PacketNotifier(IPacketHandlerManager packetHandlerManager, INavigationGrid navGrid)
        {
            _packetHandlerManager = packetHandlerManager;
            _navGrid = navGrid;
        }

        // TODO: Maybe clean up all the function parameters somehow?

        /// <summary>
        /// Sends a packet to the specified user which is intended to creates a client-side debug object. *NOTE*: Has not been tested, function implementation may be incorrect.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="unit">Unit that </param>
        /// <param name="objNetId">NetID to assign to the debug object.</param>
        /// <param name="lifetime">How long the debug object should exist (in seconds).</param>
        /// <param name="radius">Distance from the center of the debug object to its edge.</param>
        /// <param name="pos1">Position of the first point. Untested.</param>
        /// <param name="pos2">Position of the second point. Untested.</param>
        /// <param name="objID">Index of the debug object. Untested, function unknown.</param>
        /// <param name="type">Type of debug object. Untested, possible types unknown.</param>
        /// <param name="name">Name of the debug object. Untested. Might be displayed as floating text?</param>
        /// <param name="r">Red hex color value.</param>
        /// <param name="g">Green hex color value.</param>
        /// <param name="b">Blue hex color value.</param>
        public void NotifyAddDebugObject(int userId, IAttackableUnit unit, uint objNetId, float lifetime, float radius, Vector3 pos1, Vector3 pos2, int objID = 0, byte type = 0x0, string name = "debugobj", byte r = 0xFF, byte g = 0x46, byte b = 0x0)
        {
            //TODO: Implement a DebugObject class so this is cleaner
            var color = new LeaguePackets.Game.Common.Color
            {
                Red = r,
                Green = g,
                Blue = b
            };
            var debugObjPacket = new S2C_AddDebugObject
            {
                SenderNetID = unit.NetId,
                DebugID = objID,
                Lifetime = lifetime,
                Type = type,
                NetID1 = unit.NetId,
                NetID2 = objNetId,
                Radius = radius,
                Point1 = pos1,
                Point2 = pos2,
                Color = color,
                MaxSize = 0, // TODO: Verify what this does
                Bitfield = 0x0, // TODO: Verify what this does
                StringBuffer = name
            };
            _packetHandlerManager.SendPacket(userId, debugObjPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that the specified Champion has killed a specified player and received a specified amount of gold.
        /// </summary>
        /// <param name="c">Champion that killed a unit.</param>
        /// <param name="died">AttackableUnit that died to the Champion.</param>
        /// <param name="gold">Amount of gold the Champion gained for the kill.</param>
        /// TODO: Only use BroadcastPacket when the unit that died is a Champion.
        public void NotifyAddGold(IChampion c, IAttackableUnit died, float gold)
        {
            var ag = new AddGold(c, died, gold);
            _packetHandlerManager.BroadcastPacket(ag, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that the specified Champion has gained the specified amount of experience.
        /// </summary>
        /// <param name="champion">Champion that gained the experience.</param>
        /// <param name="experience">Amount of experience gained.</param>
        /// TODO: Verify if sending to all players is correct.
        public void NotifyAddXp(IChampion champion, float experience)
        {
            var xp = new AddXp(champion, experience);
            _packetHandlerManager.BroadcastPacket(xp, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that announces a specified message (ex: "Minions have spawned.")
        /// </summary>
        /// <param name="mapId">Current map ID.</param>
        /// <param name="messageId">Message ID to announce.</param>
        /// <param name="isMapSpecific">Whether the announce is specific to the map ID.</param>
        public void NotifyAnnounceEvent(int mapId, Announces messageId, bool isMapSpecific)
        {
            var announce = new Announce(messageId, isMapSpecific ? mapId : 0);
            _packetHandlerManager.BroadcastPacket(announce, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user that informs them of their summoner data such as runes, summoner spells, masteries (or talents as named internally), etc.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="client">Info about the player's summoner data.</param>
        public void NotifyAvatarInfo(int userId, ClientInfo client)
        {
            var avatar = new AvatarInfo_Server();
            avatar.SenderNetID = client.Champion.NetId;
            var skills = new uint[] { HashFunctions.HashString(client.SummonerSkills[0]), HashFunctions.HashString(client.SummonerSkills[1]) };

            avatar.SummonerIDs[0] = skills[0];
            avatar.SummonerIDs[1] = skills[1];
            for (int i = 0; i < client.Champion.RuneList.Runes.Count; ++i)
            {
                int runeValue = 0;
                client.Champion.RuneList.Runes.TryGetValue(i, out runeValue);
                avatar.ItemIDs[i] = (uint)runeValue;
            }
            // TODO: add talents
            _packetHandlerManager.SendPacket(userId, avatar.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that have vision of the specified Azir turret that it has spawned.
        /// </summary>
        /// <param name="azirTurret">AzirTurret instance.</param>
        public void NotifyAzirTurretSpawned(IAzirTurret azirTurret)
        {
            var spawnPacket = new SpawnAzirTurret(azirTurret);
            _packetHandlerManager.BroadcastPacketVision(azirTurret, spawnPacket, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that the specified attacker is starting their first auto attack.
        /// </summary>
        /// <param name="attacker">AttackableUnit that is starting an auto attack.</param>
        /// <param name="victim">AttackableUnit that is the target of the auto attack.</param>
        /// <param name="futureProjNetId">NetID of the projectile that will be created for the auto attack.</param>
        /// <param name="isCritical">Whether or not the auto attack is a critical.</param>
        public void NotifyBeginAutoAttack(IAttackableUnit attacker, IAttackableUnit victim, uint futureProjNetId, bool isCritical)
        {
            var aa = new BeginAutoAttack(_navGrid, attacker, victim, futureProjNetId, isCritical);
            _packetHandlerManager.BroadcastPacket(aa, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a side bar tip to the specified player (ex: quest tips).
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="title">Title of the tip.</param>
        /// <param name="text">Description text of the tip.</param>
        /// <param name="imagePath">Path to an image that will be embedded in the tip.</param>
        /// <param name="tipCommand">Action suggestion(? unconfirmed).</param>
        /// <param name="playerNetId">NetID to send the packet to.</param>
        /// <param name="targetNetId">NetID of the target referenced by the tip.</param>
        /// TODO: tipCommand should be a lib/core enum that gets translated into a league version specific packet enum as it may change over time.
        public void NotifyBlueTip(int userId, string title, string text, string imagePath, byte tipCommand, uint playerNetId,
            uint targetNetId)
        {
            var packet = new BlueTip(title, text, imagePath, tipCommand, playerNetId, targetNetId);
            _packetHandlerManager.SendPacket(userId, packet, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the player attempting to buy an item that their purchase was successful.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="gameObject">GameObject of type ObjAiBase that can buy items.</param>
        /// <param name="itemInstance">Item instance housing all information about the item that has been bought.</param>
        public void NotifyBuyItem(int userId, IObjAiBase gameObject, IItem itemInstance)
        {
            ItemData itemData = new ItemData
            {
                ItemID = (uint)itemInstance.ItemData.ItemId,
                Slot = gameObject.Inventory.GetItemSlot(itemInstance),
                ItemsInSlot = (byte)itemInstance.StackCount,
                SpellCharges = 0 // TODO: Unhardcode
            };

            //TODO find out what bitfield does, currently unknown
            var buyItemPacket = new BuyItemAns
            {
                SenderNetID = gameObject.NetId,
                Item = itemData,
                Bitfield = 0 //TODO: find out what this does, currently unknown
            };

            _packetHandlerManager.SendPacket(userId, buyItemPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players updating a champion's death timer.
        /// </summary>
        /// <param name="champion">Champion that died.</param>
        public void NotifyChampionDeathTimer(IChampion champion)
        {
            var cdt = new ChampionDeathTimer(champion);
            _packetHandlerManager.BroadcastPacket(cdt, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that a champion has died and calls a death timer update packet.
        /// </summary>
        /// <param name="champion">Champion that died.</param>
        /// <param name="killer">Unit that killed the Champion.</param>
        /// <param name="goldFromKill">Amount of gold the killer received.</param>
        public void NotifyChampionDie(IChampion champion, IAttackableUnit killer, int goldFromKill)
        {
            var cd = new ChampionDie(champion, killer, goldFromKill);
            _packetHandlerManager.BroadcastPacket(cd, Channel.CHL_S2C);

            NotifyChampionDeathTimer(champion);
        }

        /// <summary>
        /// Sends a packet to all players that a champion has respawned.
        /// </summary>
        /// <param name="c">Champion that respawned.</param>
        public void NotifyChampionRespawn(IChampion c)
        {
            var cr = new ChampionRespawn(c);
            _packetHandlerManager.BroadcastPacket(cr, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players of the specified team that a champion has spawned (for the first time).
        /// </summary>
        /// <param name="c">Champion that has spawned.</param>
        /// <param name="team">Team to send the packet to.</param>
        public void NotifyChampionSpawned(IChampion c, TeamId team)
        {
            var hs = new HeroSpawn2(c);
            _packetHandlerManager.BroadcastPacketTeam(team, hs, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of a specified ObjAiBase explaining that their specified spell's cooldown has been set.
        /// </summary>
        /// <param name="u">ObjAiBase who owns the spell going on cooldown.</param>
        /// <param name="slotId">Slot of the spell.</param>
        /// <param name="currentCd">Amount of time the spell has already been on cooldown (if applicable).</param>
        /// <param name="totalCd">Amount of time that the spell should have be in cooldown before going off cooldown.</param>
        public void NotifyCHAR_SetCooldown(IObjAiBase u, byte slotId, float currentCd, float totalCd)
        {
            var cdPacket = new CHAR_SetCooldown
            {
                SenderNetID = u.NetId,
                Slot = slotId,
                PlayVOWhenCooldownReady = true, // TODO: Unhardcode
                IsSummonerSpell = false, // TODO: Unhardcode
                Cooldown = currentCd,
                MaxCooldownForDisplay = totalCd
            };
            if (u is IChampion && (slotId == 0 || slotId == 1))
            {
                cdPacket.IsSummonerSpell = true; // TODO: Verify functionality
            }
            _packetHandlerManager.BroadcastPacketVision(u, cdPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user that highlights the specified GameObject.
        /// </summary>
        /// <param name="userId">ID of the user to send the packet to.</param>
        /// <param name="unit">GameObject to highlght.</param>
        public void NotifyCreateUnitHighlight(int userId, IGameObject unit)
        {
            var highlightPacket = new S2C_CreateUnitHighlight
            {
                SenderNetID = unit.NetId,
                TargetNetID = unit.NetId
            };

            _packetHandlerManager.SendPacket(userId, highlightPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to optionally all players (given isGlobal), a specified user that is the source of damage, or a specified user that is receiving the damage. The packet details an instance of damage being applied to a unit by another unit.
        /// </summary>
        /// <param name="source">Unit which caused the damage.</param>
        /// <param name="target">Unit which is taking the damage.</param>
        /// <param name="amount">Amount of damage dealt to the target (usually the end result of all damage calculations).</param>
        /// <param name="type">Type of damage being dealt; PHYSICAL/MAGICAL/TRUE</param>
        /// <param name="damagetext">Type of text to show above the target; INVULNERABLE/DODGE/CRIT/NORMAL/MISS</param>
        /// <param name="isGlobal">Whether or not the packet should be sent to all players.</param>
        /// <param name="sourceId">ID of the user who dealt the damage that should receive the packet.</param>
        /// <param name="targetId">ID of the user who is taking the damage that should receive the packet.</param>
        public void NotifyDamageDone(IAttackableUnit source, IAttackableUnit target, float amount, DamageType type, DamageText damagetext, bool isGlobal = true, int sourceId = 0, int targetId = 0)
        {
            var dd = new DamageDone(source, target, amount, type, damagetext);
            if (isGlobal)
            {
                _packetHandlerManager.BroadcastPacket(dd, Channel.CHL_S2C);
            }
            else
            {
                if (sourceId != 0)
                {
                    _packetHandlerManager.SendPacket(sourceId, dd, Channel.CHL_S2C);
                }

                if (targetId != 0)
                {
                    _packetHandlerManager.SendPacket(targetId, dd, Channel.CHL_S2C);
                }
            }
        }

        /// <summary>
        /// Sends a packet to all players that have vision of the specified unit. The packet details a group of waypoints which the unit will move to given the specified parameters.
        /// </summary>
        /// <param name="u">Unit that is dashing.</param>
        /// <param name="t">Target the unit is dashing to (single-point or unit).</param>
        /// <param name="dashSpeed">Constant speed that the unit will have during the dash.</param>
        /// <param name="keepFacingLastDirection">Whether or not the unit should maintain the direction they were facing before dashing.</param>
        /// <param name="leapHeight">How high the unit will reach at the peak of the dash.</param>
        /// <param name="followTargetMaxDistance">Maximum distance the unit will follow the Target before stopping the dash or reaching to the Target.</param>
        /// <param name="backDistance">Unknown parameter.</param>
        /// <param name="travelTime">Total time the dash will attempt to travel to the Target before stopping or reaching the Target.</param>
        /// TODO: Replace current implementation with LeaguePackets as this is incomplete.
        public void NotifyDash(IAttackableUnit u,
                               ITarget t,
                               float dashSpeed,
                               bool keepFacingLastDirection,
                               float leapHeight,
                               float followTargetMaxDistance,
                               float backDistance,
                               float travelTime)
        {
            var dash = new Dash(_navGrid,
                                u,
                                t,
                                dashSpeed,
                                keepFacingLastDirection,
                                leapHeight,
                                followTargetMaxDistance,
                                backDistance,
                                travelTime);
            _packetHandlerManager.BroadcastPacketVision(u, dash, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing a debug message.
        /// </summary>
        /// <param name="htmlDebugMessage">Debug message to send.</param>
        public void NotifyDebugMessage(string htmlDebugMessage)
        {
            var dm = new DebugMessage(htmlDebugMessage);
            _packetHandlerManager.BroadcastPacket(dm, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user detailing a debug message.
        /// </summary>
        /// <param name="userId">ID of the user to send the packet to.</param>
        /// <param name="message">Debug message to send.</param>
        public void NotifyDebugMessage(int userId, string message)
        {
            var dm = new DebugMessage(message);
            _packetHandlerManager.SendPacket(userId, dm, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified team detailing a debug message.
        /// </summary>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="message">Debug message to send.</param>
        public void NotifyDebugMessage(TeamId team, string message)
        {
            var dm = new DebugMessage(message);
            _packetHandlerManager.BroadcastPacketTeam(team, dm, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user which is intended for debugging.
        /// </summary>
        /// <param name="userId">ID of the user to send the packet to.</param>
        /// <param name="data">Array of bytes representing the packet's data.</param>
        public void NotifyDebugPacket(int userId, byte[] data)
        {
            _packetHandlerManager.SendPacket(userId, data, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing the destruction of (usually) an auto attack missile.
        /// </summary>
        /// <param name="p">Projectile that is being destroyed.</param>
        public void NotifyDestroyClientMissile(IProjectile p)
        {
            var misPacket = new S2C_DestroyClientMissile
            {
                SenderNetID = p.NetId
            };
            _packetHandlerManager.BroadcastPacket(misPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified team detailing the destruction of (usually) an auto attack missile.
        /// </summary>
        /// <param name="p">Projectile that is being destroyed.</param>
        /// <param name="team">TeamId to send the packet to.</param>
        public void NotifyDestroyClientMissile(IProjectile p, TeamId team)
        {
            var misPacket = new S2C_DestroyClientMissile
            {
                SenderNetID = p.NetId
            };
            _packetHandlerManager.BroadcastPacketTeam(team, misPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing an emotion that is being performed by the unit that owns the specified netId.
        /// </summary>
        /// <param name="type">Type of emotion being performed; DANCE/TAUNT/LAUGH/JOKE/UNK.</param>
        /// <param name="netId">NetID of the unit performing the emotion.</param>
        public void NotifyEmotions(Emotions type, uint netId)
        {
            // convert type
            EmotionType targetType;
            switch (type)
            {
                case Emotions.DANCE:
                    targetType = EmotionType.DANCE;
                    break;
                case Emotions.TAUNT:
                    targetType = EmotionType.TAUNT;
                    break;
                case Emotions.LAUGH:
                    targetType = EmotionType.LAUGH;
                    break;
                case Emotions.JOKE:
                    targetType = EmotionType.JOKE;
                    break;
                case Emotions.UNK:
                    targetType = (EmotionType)type;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            var packet = new EmotionPacketResponse((byte)targetType, netId);
            _packetHandlerManager.BroadcastPacket(packet, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user detailing that the GameObject that owns the specified netId has finished being initialized into vision.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetID of the GameObject coming into vision.</param>
        public void NotifyEnterLocalVisibilityClient(int userId, uint netId)
        {
            var enterLocalVis = new OnEnterLocalVisiblityClient
            {
                SenderNetID = netId
            };

            _packetHandlerManager.SendPacket(userId, enterLocalVis.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to either all players with vision of the specified GameObject or a specified user. The packet contains details of the GameObject's health (given it is of the type AttackableUnit) and is meant for after the GameObject is first initialized into vision.
        /// </summary>
        /// <param name="o">GameObject coming into vision.</param>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifyEnterLocalVisibilityClient(IGameObject o, int userId = 0)
        {
            var enterLocalVis = new OnEnterLocalVisiblityClient
            {
                SenderNetID = o.NetId,
            };

            if (o is IAttackableUnit u)
            {
                enterLocalVis.MaxHealth = u.Stats.HealthPoints.Total;
                enterLocalVis.Health = u.Stats.CurrentHealth;
            }

            if (userId == 0)
            {
                _packetHandlerManager.BroadcastPacketVision(o, enterLocalVis.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, enterLocalVis.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to either all players of the specified team or the specified user. The packet details the data surrounding the specified GameObject that is required by players when a GameObject enters vision such as items, shields, skin, and movements.
        /// </summary>
        /// <param name="o">GameObject entering vision.</param>
        /// <param name="team">TeamId to send the packet to.</param>
        /// <param name="userId">User to send the packet to.</param>
        /// TODO: Incomplete implementation.
        public void NotifyEnterVisibilityClient(IGameObject o, TeamId team, int userId = 0)
        {
            var enterVis = new OnEnterVisiblityClient(); // TYPO >:(
            enterVis.SenderNetID = o.NetId;
            var itemData = new List<ItemData>(); //TODO: Fix item system so this can be finished
            enterVis.Items = itemData;
            var shields = new ShieldValues(); //TODO: Implement shields so this can be finished
            enterVis.ShieldValues = shields;
            var charStackDataList = new List<CharacterStackData>();
            var charStackData = new CharacterStackData
            {
                SkinID = 0,
                OverrideSpells = false,
                ModelOnly = false,
                ReplaceCharacterPackage = false,
                ID = 0
            };
            if (o is IAttackableUnit a)
            {
                charStackData.SkinName = a.Model;

                if (a is IChampion c)
                {
                    charStackData.SkinID = (uint)c.Skin;
                }
            }
            charStackDataList.Add(charStackData);
            enterVis.CharacterDataStack = charStackDataList;

            enterVis.LookAtPosition = new Vector3(1, 0, 0);
            if (o is IObjAiBase)
            {
                //TODO: Use a non-empty buff list here
                var emptyBuffCountList = new List<KeyValuePair<byte, int>>();
                enterVis.BuffCount = emptyBuffCountList;
            }
            enterVis.UnknownIsHero = false;
            //TODO: Use MovementDataNormal instead, because currently we desync if the unit is moving
            // TODO: Save unit waypoints in unit class so they can be used here for MovementDataNormal
            var md = new MovementDataStop
            {
                Position = o.GetPosition(),
                Forward = new Vector2(0, 1),
                SyncID = (int)o.SyncId
            };
            enterVis.MovementData = md;

            if (userId != 0)
            {
                _packetHandlerManager.SendPacket(userId, enterVis.GetBytes(), Channel.CHL_S2C);
                NotifyEnterLocalVisibilityClient(o, userId);
            }
            else
            {
                _packetHandlerManager.BroadcastPacketTeam(team, enterVis.GetBytes(), Channel.CHL_S2C);
                NotifyEnterLocalVisibilityClient(o);
            }
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the unit is facing the specified direction.
        /// </summary>
        /// <param name="u">Unit that is changing their facing direction.</param>
        /// <param name="direction">2D, top-down direction the unit will face.</param>
        /// <param name="isInstant">Whether or not the unit should instantly turn to the direction.</param>
        /// <param name="turnTime">The amount of time (seconds) the turn should take.</param>
        public void NotifyFaceDirection(IAttackableUnit u, Vector2 direction, bool isInstant = true, float turnTime = 0.0833f)
        {
            var height = _navGrid.GetHeightAtLocation(direction);
            var fd = new FaceDirection(u, direction.X, direction.Y, height, isInstant, turnTime);
            _packetHandlerManager.BroadcastPacketVision(u, fd, Channel.CHL_S2C);
        }

        public void NotifyFogUpdate2(IAttackableUnit u, uint newFogId)
        {
            var fog = new FogUpdate2(u, newFogId);
            _packetHandlerManager.BroadcastPacketTeam(u.Team, fog, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that (usually) an auto attack missile has been created.
        /// </summary>
        /// <param name="p">Projectile that was created.</param>
        public void NotifyForceCreateMissile(IProjectile p)
        {
            var misPacket = new S2C_ForceCreateMissile();
            misPacket.SenderNetID = p.Owner.NetId;
            misPacket.MissileNetID = p.NetId;
            _packetHandlerManager.BroadcastPacket(misPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to, optionally, a specified player, all players with vision of the particle, or all players (given the particle is set as globally visible).
        /// </summary>
        /// <param name="particle">Particle to network.</param>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifyFXCreateGroup(IParticle particle, int playerId = 0)
        {
            var fxPacket = new FX_Create_Group();
            fxPacket.SenderNetID = particle.Owner.NetId;

            var fxDataList = new List<FXCreateData>();

            var ownerHeight = _navGrid.GetHeightAtLocation(particle.Owner.X, particle.Owner.Y);
            var targetHeight = _navGrid.GetHeightAtLocation(particle.Target.X, particle.Target.Y);
            var particleHeight = _navGrid.GetHeightAtLocation(particle.X, particle.Y);
            var higherValue = Math.Max(targetHeight, particleHeight);

            // TODO: implement option for multiple particles instead of hardcoding one
            var fxData1 = new FXCreateData
            {
                NetAssignedNetID = particle.NetId,
                CasterNetID = particle.Owner.NetId,
                KeywordNetID = 0, // Not sure what this is

                PositionX = (short)((particle.X - _navGrid.MapWidth / 2) / 2),
                PositionY = higherValue,
                PositionZ = (short)((particle.Y - _navGrid.MapHeight / 2) / 2),

                TargetPositionY = targetHeight,

                OwnerPositionX = (short)((particle.Owner.X - _navGrid.MapWidth / 2) / 2),
                OwnerPositionY = ownerHeight,
                OwnerPositionZ = (short)((particle.Owner.Y - _navGrid.MapHeight / 2) / 2),

                // NOTE: particles may have a set lifetime, which ignores this
                TimeSpent = particle.Lifetime,
                ScriptScale = particle.Scale
            };

            if (particle.Target.IsSimpleTarget) // Non-object target (usually a position)
            {
                fxData1.TargetNetID = particle.Owner.NetId; // Probably not correct, but it works
                fxData1.BindNetID = 0; // Not sure what this is

                fxData1.TargetPositionX = (short)((particle.Target.X - _navGrid.MapWidth / 2) / 2);
                fxData1.TargetPositionZ = (short)((particle.Target.Y - _navGrid.MapHeight / 2) / 2);
            }
            else
            {
                fxData1.TargetNetID = (particle.Target as IGameObject).NetId;
                fxData1.BindNetID = (particle.Target as IGameObject).NetId; // Not sure what this is

                fxData1.TargetPositionX = (short)(particle.Target as IGameObject).X;
                fxData1.TargetPositionZ = (short)(particle.Target as IGameObject).Y;
            }

            if (particle.Direction.Length() <= 0)
            {
                fxData1.OrientationVector = Vector3.Zero;
            }
            else
            {
                fxData1.OrientationVector = particle.Direction;
            }

            fxDataList.Add(fxData1);

            // TODO: implement option for multiple groups of particles instead of hardcoding one
            var fxGroups = new List<FXCreateGroupData>();

            var fxGroupData1 = new FXCreateGroupData
            {
                EffectNameHash = HashString(particle.Name),
                //TODO: un-hardcode flags
                Flags = 0x20, // Taken from SpawnParticle packet
                TargetBoneNameHash = 0,
                // TODO: Verify if the above is the same as below (most likely relate to bone of origin and bone of end point)
                BoneNameHash = HashString(particle.BoneName),

                FXCreateData = fxDataList
            };

            if (particle.Owner is IObjAiBase o)
            {
                fxGroupData1.PackageHash = o.GetObjHash();
            }
            else
            {
                fxGroupData1.PackageHash = 0; // TODO: Verify
            }

            fxGroups.Add(fxGroupData1);

            fxPacket.FXCreateGroup = fxGroups;

            if (playerId == 0)
            {
                if (particle.VisionAffected)
                {
                    _packetHandlerManager.BroadcastPacketVision(particle, fxPacket.GetBytes(), Channel.CHL_S2C);
                }
                else
                {
                    _packetHandlerManager.BroadcastPacket(fxPacket.GetBytes(), Channel.CHL_S2C);
                }
            }
            else
            {
                _packetHandlerManager.SendPacket(playerId, fxPacket.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to the specified team detailing that the specified Particle has become visible.
        /// </summary>
        /// <param name="particle">Particle that came into vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        public void NotifyFXEnterTeamVisibility(IParticle particle, TeamId team)
        {
            var fxVisPacket = new S2C_FX_OnEnterTeamVisiblity
            {
                SenderNetID = particle.NetId,
                NetID = particle.NetId,
                VisibilityTeam = (byte)team
            };
            _packetHandlerManager.BroadcastPacketTeam(team, fxVisPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified particle has been destroyed.
        /// </summary>
        /// <param name="particle">Particle that is being destroyed.</param>
        /// TODO: Change to only broadcast to players who have vision of the particle (maybe?).
        public void NotifyFXKill(IParticle particle)
        {
            var fxKill = new FX_Kill
            {
                SenderNetID = particle.NetId,
                NetID = particle.NetId
            };
            _packetHandlerManager.BroadcastPacket(fxKill.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified team detailing that the specified Particle has left vision.
        /// </summary>
        /// <param name="particle">Particle that left vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        public void NotifyFXLeaveTeamVisibility(IParticle particle, TeamId team)
        {
            var fxVisPacket = new S2C_FX_OnLeaveTeamVisiblity
            {
                SenderNetID = particle.NetId,
                NetID = particle.NetId,
                VisibilityTeam = (byte)team
            };
            _packetHandlerManager.BroadcastPacketTeam(team, fxVisPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends packets to all players which force the players' cameras to the nexus being destroyed, hides their UI, and ends the game.
        /// </summary>
        /// <param name="cameraPosition">Position of the nexus being destroyed.</param>
        /// <param name="nexus">Nexus being destroyed.</param>
        /// <param name="players">All players that can receive packets.</param>
        public void NotifyGameEnd(Vector3 cameraPosition, INexus nexus, List<Tuple<uint, ClientInfo>> players)
        {
            var losingTeam = nexus.Team;

            foreach (var p in players)
            {
                var cam = new MoveCamera(p.Item2.Champion, cameraPosition.X, cameraPosition.Y, cameraPosition.Z, 2);
                _packetHandlerManager.SendPacket((int)p.Item2.PlayerId, cam, Channel.CHL_S2C);
                _packetHandlerManager.SendPacket((int)p.Item2.PlayerId, new HideUi(), Channel.CHL_S2C);
            }

            _packetHandlerManager.BroadcastPacket(new ExplodeNexus(nexus), Channel.CHL_S2C);

            var timer = new Timer(5000) { AutoReset = false };
            timer.Elapsed += (a, b) =>
            {
                var gameEndPacket = new GameEnd(losingTeam != TeamId.TEAM_BLUE);
                _packetHandlerManager.BroadcastPacket(gameEndPacket, Channel.CHL_S2C);
            };
            timer.Start();
        }

        /// <summary>
        /// Sends a packet to all players detailing that the game has started. Sent when all players have finished loading.
        /// </summary>
        public void NotifyGameStart()
        {
            var start = new StatePacket(PacketCmd.PKT_S2C_START_GAME);
            _packetHandlerManager.BroadcastPacket(start, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing the amount of time since the game started (in seconds).
        /// </summary>
        /// <param name="gameTime">Time since the game started (in milliseconds).</param>
        public void NotifyGameTimer(float gameTime)
        {
            var gameTimer = new GameTimer(gameTime / 1000.0f);
            _packetHandlerManager.BroadcastPacket(gameTimer, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing the amount of time since the game started (in seconds).
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="time">Time since the game started (in milliseconds).</param>
        public void NotifyGameTimer(int userId, float time)
        {
            var timer = new GameTimer(time / 1000.0f);
            _packetHandlerManager.SendPacket(userId, timer, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing the amount of time since the game started (in seconds). Used to initialize the user's in-game timer.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="time">Time since the game started (in milliseconds).</param>
        public void NotifyGameTimerUpdate(int userId, float time)
        {
            var timer = new GameTimerUpdate(time / 1000.0f);
            _packetHandlerManager.SendPacket(userId, timer, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that (usually) their hero has spawned.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="client">Info of the client which the user owns.</param>
        public void NotifyHeroSpawn(int userId, ClientInfo client)
        {
            var spawn = new HeroSpawn(client);
            _packetHandlerManager.SendPacket(userId, spawn, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player which spawns (usually) their Champion.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="champion">Champion which the user owns.</param>
        public void NotifyHeroSpawn2(int userId, IChampion champion)
        {
            var heroSpawnPacket = new HeroSpawn2(champion);
            _packetHandlerManager.SendPacket(userId, heroSpawnPacket, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players which announces that the team which owns the specified inhibitor has an inhibitor which is respawning soon.
        /// </summary>
        /// <param name="inhibitor">Inhibitor that is respawning soon.</param>
        public void NotifyInhibitorSpawningSoon(IInhibitor inhibitor)
        {
            var packet = new UnitAnnounce(UnitAnnounces.INHIBITOR_ABOUT_TO_SPAWN, inhibitor);
            _packetHandlerManager.BroadcastPacket(packet, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing the state (DEAD/ALIVE) of the specified inhibitor.
        /// </summary>
        /// <param name="inhibitor">Inhibitor to check.</param>
        /// <param name="killer">Killer of the inhibitor (if applicable).</param>
        /// <param name="assists">Assists of the killer (if applicable).</param>
        public void NotifyInhibitorState(IInhibitor inhibitor, IGameObject killer = null, List<IChampion> assists = null)
        {
            UnitAnnounce announce;
            switch (inhibitor.InhibitorState)
            {
                case InhibitorState.DEAD:
                    announce = new UnitAnnounce(UnitAnnounces.INHIBITOR_DESTROYED, inhibitor, killer, assists);
                    _packetHandlerManager.BroadcastPacket(announce, Channel.CHL_S2C);

                    var anim = new InhibitorDeathAnimation(inhibitor, killer);
                    _packetHandlerManager.BroadcastPacket(anim, Channel.CHL_S2C);
                    break;
                case InhibitorState.ALIVE:
                    announce = new UnitAnnounce(UnitAnnounces.INHIBITOR_SPAWNED, inhibitor, killer, assists);
                    _packetHandlerManager.BroadcastPacket(announce, Channel.CHL_S2C);
                    break;
            }
            var packet = new InhibitorStateUpdate(inhibitor);
            _packetHandlerManager.BroadcastPacket(packet, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified Champion detailing that the Champion's items have been swapped.
        /// </summary>
        /// <param name="c">Champion who swapped their items.</param>
        /// <param name="fromSlot">Slot the item was previously in.</param>
        /// <param name="toSlot">Slot the item was swapped to.</param>
        public void NotifyItemsSwapped(IChampion c, byte fromSlot, byte toSlot)
        {
            var sia = new SwapItemsResponse(c, fromSlot, toSlot);
            _packetHandlerManager.BroadcastPacketVision(c, sia, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified team detailing that the specified LaneMinion has spawned.
        /// </summary>
        /// <param name="m">LaneMinion that spawned.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        public void NotifyLaneMinionSpawned(ILaneMinion m, TeamId team)
        {
            var p = new Barrack_SpawnUnit
            {
                SenderNetID = m.NetId,
                ObjectID = m.NetId,
                ObjectNodeID = 0x40, // TODO: check this
                BarracksNetID = 0xFF000000 | Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(m.BarracksName)), // TODO: Verify
                WaveCount = 1, // TODO: Unhardcode
                MinionType = (byte)m.MinionSpawnType,
                DamageBonus = 10, // TODO: Unhardcode
                HealthBonus = 7, // TODO: Unhardcode
                MinionLevel = 1 // TODO: Unhardcode
            };

            var md = new MovementDataNormal
            {
                Waypoints = Convertors.Vector2ToWaypoint(m.Waypoints, _navGrid),
                TeleportNetID = m.NetId,
                HasTeleportID = false, // TODO: Unhardcode
                SyncID = (int)m.SyncId
            };

            var visionPacket = new OnEnterVisiblityClient
            {
                MovementData = md,
                LookAtPosition = new Vector3(1, 0, 0) // TODO: Unhardcode
            };
            visionPacket.Packets.Add(p);
            visionPacket.SenderNetID = m.NetId;

            _packetHandlerManager.BroadcastPacketTeam(team, visionPacket.GetBytes(), Channel.CHL_S2C);
            NotifyEnterLocalVisibilityClient(m);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the GameObject which has the specified netId has left vision.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the GameObject that left vision.</param>
        public void NotifyLeaveLocalVisibilityClient(int userId, uint netId)
        {
            var leaveLocalVis = new OnLeaveLocalVisiblityClient
            {
                SenderNetID = netId
            };

            _packetHandlerManager.SendPacket(userId, leaveLocalVis.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to either the specified player or team detailing that the specified GameObject has left vision.
        /// </summary>
        /// <param name="o">GameObject that left vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifyLeaveLocalVisibilityClient(IGameObject o, TeamId team, int userId = 0)
        {
            var leaveLocalVis = new OnLeaveLocalVisiblityClient
            {
                SenderNetID = o.NetId
            };

            if (userId == 0)
            {
                _packetHandlerManager.BroadcastPacketTeam(team, leaveLocalVis.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, leaveLocalVis.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to either the specified user or team detailing that the specified GameObject has left vision.
        /// </summary>
        /// <param name="o">GameObject that left vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="userId">User to send the packet to (if applicable).</param>
        public void NotifyLeaveVisibilityClient(IGameObject o, TeamId team, int userId = 0)
        {
            var leaveVis = new OnLeaveVisiblityClient
            {
                SenderNetID = o.NetId
            };

            if (userId != 0)
            {
                _packetHandlerManager.SendPacket(userId, leaveVis.GetBytes(), Channel.CHL_S2C);
                NotifyLeaveLocalVisibilityClient(userId, o.NetId);
            }
            else
            {
                _packetHandlerManager.BroadcastPacketTeam(team, leaveVis.GetBytes(), Channel.CHL_S2C);
                NotifyLeaveLocalVisibilityClient(o, team);
            }
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the specified GameObject of type LevelProp has spawned.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="levelProp">LevelProp that has spawned.</param>
        public void NotifyLevelPropSpawn(int userId, ILevelProp levelProp)
        {
            var levelPropSpawnPacket = new LevelPropSpawn(levelProp);
            _packetHandlerManager.SendPacket(userId, levelPropSpawnPacket, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified Champion has leveled up.
        /// </summary>
        /// <param name="c">Champion which leveled up.</param>
        public void NotifyLevelUp(IChampion c)
        {
            var lu = new LevelUp(c);
            _packetHandlerManager.BroadcastPacket(lu, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing the order and size of both teams on the loading screen.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="players">Client info of all players in the loading screen.</param>
        public void NotifyLoadScreenInfo(int userId, List<Tuple<uint, ClientInfo>> players)
        {
            var screenInfo = new LoadScreenInfo(players);
            _packetHandlerManager.SendPacket(userId, screenInfo, Channel.CHL_LOADING_SCREEN);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing skin information of all specified players on the loading screen.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="player">Player information to send.</param>
        public void NotifyLoadScreenPlayerChampion(int userId, Tuple<uint, ClientInfo> player)
        {
            var loadChampion = new LoadScreenPlayerChampion(player);
            _packetHandlerManager.SendPacket(userId, loadChampion, Channel.CHL_LOADING_SCREEN);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing skin and player name information of all soecified players on the loading screen.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="player">Player information to send.</param>
        public void NotifyLoadScreenPlayerName(int userId, Tuple<uint, ClientInfo> player)
        {
            var loadName = new LoadScreenPlayerName(player);
            _packetHandlerManager.SendPacket(userId, loadName, Channel.CHL_LOADING_SCREEN);
        }

        /// <summary>
        /// Sends a packet to all players who have vision of the specified Minion detailing that it has spawned.
        /// </summary>
        /// <param name="minion">Minion that is spawning.</param>
        /// <param name="team">Unused, to be removed.</param>
        public void NotifyMinionSpawned(IMinion minion, TeamId team)
        {
            var spawnPacket = new SpawnMinionS2C();
            spawnPacket.SkinName = minion.Model;
            spawnPacket.Name = minion.Name;
            spawnPacket.VisibilitySize = minion.VisionRadius; // Might be incorrect
            spawnPacket.IsTargetableToTeamSpellFlags = (uint)SpellFlags.TargetableToAll;
            spawnPacket.IsTargetable = true;
            spawnPacket.IsBot = minion.IsBot;
            spawnPacket.IsLaneMinion = minion.IsLaneMinion;
            spawnPacket.IsWard = minion.IsWard;
            spawnPacket.IgnoreCollision = false;
            spawnPacket.TeamID = (ushort)minion.Team;
            // CloneNetID, clones not yet implemented
            spawnPacket.SkinID = 0;
            spawnPacket.Position = new Vector3(minion.GetPosition().X, minion.GetHeight(), minion.GetPosition().Y); // check if work, probably not
            spawnPacket.SenderNetID = minion.NetId;
            spawnPacket.NetNodeID = (byte)NetNodeID.Spawned;
            if (minion.IsLaneMinion) // Should probably change/optimize at some point
            {
                spawnPacket.OwnerNetID = minion.Owner.NetId;
            }
            else
            {
                spawnPacket.OwnerNetID = minion.NetId;
            }
            spawnPacket.NetID = minion.NetId;
            // ID, not sure if it should be here
            spawnPacket.InitialLevel = 1;
            var visionPacket = new OnEnterVisiblityClient();
            visionPacket.LookAtPosition = new Vector3(1, 0, 0);
            var md = new MovementDataStop();
            md.Position = minion.GetPosition();
            md.Forward = new Vector2(0, 1);
            md.SyncID = (int)minion.SyncId;
            visionPacket.MovementData = md;
            visionPacket.Packets.Add(spawnPacket);
            visionPacket.SenderNetID = minion.NetId;
            _packetHandlerManager.BroadcastPacketVision(minion, visionPacket.GetBytes(), Channel.CHL_S2C);
            NotifyEnterLocalVisibilityClient(minion);
            //var spawnPacket = new SpawnMinion(minion);
            //_packetHandlerManager.BroadcastPacketVision(minion, spawnPacket, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to either all players with vision (given the projectile is networked to the client) of the projectile, or all players. The packet contains all details regarding the specified projectile's creation.
        /// </summary>
        /// <param name="p">Projectile that was created.</param>
        public void NotifyMissileReplication(IProjectile p)
        {
            var misPacket = new MissileReplication();
            misPacket.SenderNetID = p.Owner.NetId;
            misPacket.Position = new Vector3(p.X, p.GetHeight(), p.Y);
            misPacket.CasterPosition = new Vector3(p.Owner.X, p.Owner.GetHeight(), p.Owner.Y);
            var current = new Vector3(p.X, _navGrid.GetHeightAtLocation(p.X, p.Y), p.Y);
            var to = Vector3.Normalize(new Vector3(p.Target.X, _navGrid.GetHeightAtLocation(p.Target.X, p.Target.Y), p.Target.Y) - current);
            // Not sure if we want to add height for these, but i did it anyway
            misPacket.Direction = new Vector3(to.X, 0, to.Y);
            misPacket.Velocity = new Vector3(to.X * p.GetMoveSpeed(), 0, to.Y * p.GetMoveSpeed());
            misPacket.StartPoint = new Vector3(p.X, p.GetHeight(), p.Y);
            misPacket.EndPoint = new Vector3(p.Target.X, _navGrid.GetHeightAtLocation(p.Target.X, p.Target.Y), p.Target.Y);
            misPacket.UnitPosition = new Vector3(p.Owner.X, p.Owner.GetHeight(), p.Owner.Y);
            misPacket.TimeFromCreation = 0f; // TODO: Unhardcode
            misPacket.Speed = p.GetMoveSpeed();
            misPacket.LifePercentage = 0f; // TODO: Unhardcode
            //TODO: Implement time limited projectiles
            misPacket.TimedSpeedDelta = 0f; // TODO: Implement time limited projectiles for this
            misPacket.TimedSpeedDeltaTime = 0x7F7FFFFF; // Same as above (this value is from the SpawnProjectile packet, it is a placeholder)
            misPacket.Bounced = false; //TODO: Implement bouncing projectiles
            var cast = new CastInfo();
            cast.SpellHash = (uint)p.ProjectileId;
            cast.SpellNetID = p.OriginSpell != null ? p.OriginSpell.SpellNetId : 0;
            cast.SpellLevel = p.OriginSpell != null ? p.OriginSpell.Level : (byte)0;
            cast.AttackSpeedModifier = 1.0f; // Unsure of a use for this
            cast.CasterNetID = p.OriginSpell != null ? p.OriginSpell.Owner.NetId : p.Owner.NetId;
            //TODO: Implement spell chains
            cast.SpellChainOwnerNetID = p.OriginSpell != null ? p.OriginSpell.Owner.NetId : p.Owner.NetId; // TODO: Implement spell chains
            cast.PackageHash = p.OriginSpell != null ? (p.Owner as IObjAiBase).GetObjHash() : 0;
            cast.MissileNetID = p.NetId;
            // Not sure if we want to add height for these, but i did it anyway
            cast.TargetPosition = new Vector3(p.Target.X, _navGrid.GetHeightAtLocation(p.Target.X, p.Target.Y), p.Target.Y);
            cast.TargetPositionEnd = new Vector3(p.Target.X, _navGrid.GetHeightAtLocation(p.Target.X, p.Target.Y), p.Target.Y);

            if (!p.Target.IsSimpleTarget)
            {
                var targets = new List<CastInfo.Target>();
                var tar = new CastInfo.Target();
                tar.UnitNetID = (p.Target as IAttackableUnit).NetId;
                tar.HitResult = 0; // TODO: Unhardcode
                targets.Add(tar);
                cast.Targets = targets;
            }

            cast.DesignerCastTime = p.OriginSpell != null ? p.OriginSpell.CastTime : 1.0f; // TODO: Verify
            cast.ExtraCastTime = 0f; // TODO: Unhardcode
            cast.DesignerTotalTime = p.OriginSpell != null ? p.OriginSpell.CastTime : 1.0f; // TODO: Verify
            cast.Cooldown = p.OriginSpell != null ? p.OriginSpell.GetCooldown() : 0f;
            cast.StartCastTime = p.OriginSpell != null ? p.OriginSpell.CastTime : 0f; // TODO: Verify

            //TODO: Implement spell flags so these aren't set manually
            cast.IsAutoAttack = false;
            cast.IsSecondAutoAttack = false;
            cast.IsForceCastingOrChannel = false;
            cast.IsOverrideCastPosition = false;
            cast.IsClickCasted = false;

            cast.SpellSlot = p.OriginSpell != null ? p.OriginSpell.Slot : (byte)0x30;
            cast.ManaCost = p.OriginSpell != null ? p.OriginSpell.SpellData.ManaCost[p.OriginSpell.Level] : 0f;
            cast.SpellCastLaunchPosition = new Vector3(p.X, p.GetHeight(), p.Y);
            cast.AmmoUsed = p.OriginSpell != null ? p.OriginSpell.SpellData.AmmoUsed[p.OriginSpell.Level] : 0;
            cast.AmmoRechargeTime = p.OriginSpell != null ? p.OriginSpell.SpellData.AmmoRechargeTime[p.OriginSpell.Level] : 0f;

            misPacket.CastInfo = cast;
            if (!p.IsServerOnly)
            {
                _packetHandlerManager.BroadcastPacketVision(p, misPacket.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.BroadcastPacket(misPacket.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to all players that updates the specified unit's model.
        /// </summary>
        /// <param name="obj">AttackableUnit to update.</param>
        public void NotifyModelUpdate(IAttackableUnit obj)
        {
            var mp = new UpdateModel(obj.NetId, obj.Model);
            _packetHandlerManager.BroadcastPacket(mp, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the specified debug object's radius has changed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="sender">NetId of the GameObject that is responsible for this packet being sent.</param>
        /// <param name="objID">ID of the Debug Object.</param>
        /// <param name="newRadius">New radius of the Debug Object.</param>
        public void NotifyModifyDebugCircleRadius(int userId, uint sender, int objID, float newRadius)
        {
            var debugPacket = new S2C_ModifyDebugCircleRadius
            {
                SenderNetID = sender,
                ObjectID = objID,
                Radius = newRadius
            };
            _packetHandlerManager.SendPacket(userId, debugPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the specified Debug Object's color has changed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="sender">NetId of the GameObject responsible for this packet being sent.</param>
        /// <param name="objID">ID of the Debug Object.</param>
        /// <param name="r">Red hex value of the Debug Object.</param>
        /// <param name="g">Green hex value of the Debug Object.</param>
        /// <param name="b">Blue hex value of the Debug Object.</param>
        public void NotifyModifyDebugObjectColor(int userId, uint sender, int objID, byte r, byte g, byte b)
        {
            var debugObjPacket = new S2C_ModifyDebugObjectColor
            {
                SenderNetID = sender,
                ObjectID = objID
            };
            var color = new LeaguePackets.Game.Common.Color();
            color.Red = r;
            color.Green = g;
            color.Blue = b;
            debugObjPacket.Color = color;
            _packetHandlerManager.SendPacket(userId, debugObjPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified unit has had their shield values modified.
        /// </summary>
        /// <param name="unit">Unit who's shield is being modified.</param>
        /// <param name="amount">Shield amount.</param>
        /// <param name="IsPhysical">Whether or not the shield being modified is of the Physical type.</param>
        /// <param name="IsMagical">Whether or not the shield being modified is of the Magical type.</param>
        /// <param name="StopShieldFade">Whether the shield should stay static or fade.</param>
        public void NotifyModifyShield(IAttackableUnit unit, float amount, bool IsPhysical, bool IsMagical, bool StopShieldFade)
        {
            var mods = new ModifyShield
            {
                SenderNetID = unit.NetId,
                Physical = IsPhysical,
                Magical = IsMagical,
                Amount = amount
            };
            _packetHandlerManager.BroadcastPacket(mods.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified Monster that it has spawned.
        /// </summary>
        /// <param name="m">GameObject of type Monster that spawned.</param>
        public void NotifyMonsterSpawned(IMonster m)
        {
            var sp = new SpawnMonster(_navGrid, m);
            _packetHandlerManager.BroadcastPacketVision(m, sp, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that have vision of the specified GameObject that it has made a movement.
        /// </summary>
        /// <param name="o">GameObject moving.</param>
        /// TODO: Make moving only applicable to ObjAiBase.
        public void NotifyMovement(IGameObject o)
        {
            var answer = new MovementResponse(_navGrid, o);
            _packetHandlerManager.BroadcastPacketVision(o, answer, Channel.CHL_LOW_PRIORITY);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified attacker unit is starting their next auto attack.
        /// </summary>
        /// <param name="attacker">AttackableUnit auto attacking.</param>
        /// <param name="target">AttackableUnit that is the target of the auto attack.</param>
        /// <param name="futureProjNetId">NetId of the auto attack projectile.</param>
        /// <param name="isCritical">Whether or not the auto attack will crit.</param>
        /// <param name="nextAttackFlag">Whether or not to increase the time to auto attack due to being the next auto attack.</param>
        public void NotifyNextAutoAttack(IAttackableUnit attacker, IAttackableUnit target, uint futureProjNetId, bool isCritical,
            bool nextAttackFlag)
        {
            var aa = new NextAutoAttack(attacker, target, futureProjNetId, isCritical, nextAttackFlag);
            _packetHandlerManager.BroadcastPacket(aa, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players who have vision of the specified buff's target detailing that the buff has been added to the target.
        /// </summary>
        /// <param name="b">Buff being added.</param>
        public void NotifyNPC_BuffAdd2(IBuff b)
        {
            var addPacket = new NPC_BuffAdd2
            {
                SenderNetID = b.SourceUnit.NetId,
                BuffSlot = b.Slot,
                BuffType = (byte)b.BuffType,
                Count = (byte)b.StackCount,
                IsHidden = b.IsHidden,
                BuffNameHash = HashFunctions.HashString(b.Name),
                PackageHash = b.OriginSpell.Owner.GetObjHash(), // TODO: Verify
                RunningTime = b.TimeElapsed,
                Duration = b.Duration,
                CasterNetID = b.OriginSpell.Owner.NetId
            };
            _packetHandlerManager.BroadcastPacketVision(b.TargetUnit, addPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified ObjAiBase detailing that the specified group of buffs has been added to the ObjAiBase.
        /// </summary>
        /// <param name="target">ObjAiBase who is receiving the group of buffs.</param>
        /// <param name="buffs">Group of buffs being added to the target.</param>
        /// <param name="buffType">Type of buff that applies to the entire group of buffs.</param>
        /// <param name="buffName">Internal name of the buff that applies to the group of buffs.</param>
        /// <param name="runningTime">Time that has passed since the group of buffs was created.</param>
        /// <param name="duration">Total amount of time the group of buffs should be active.</param>
        public void NotifyNPC_BuffAddGroup(IObjAiBase target, List<IBuff> buffs, BuffType buffType, string buffName, float runningTime, float duration)
        {
            var addGroupPacket = new NPC_BuffAddGroup
            {
                SenderNetID = 0,
                BuffType = (byte)buffType,
                BuffNameHash = HashFunctions.HashString(buffName),
                PackageHash = target.GetObjHash(), // TODO: Verify
                RunningTime = runningTime,
                Duration = duration
            };
            var entries = new List<BuffAddGroupEntry>();
            for (int i = 0; i < buffs.Count; i++)
            {
                var entry = new BuffAddGroupEntry
                {
                    OwnerNetID = buffs[i].SourceUnit.NetId,
                    CasterNetID = buffs[i].OriginSpell.Owner.NetId,
                    Slot = buffs[i].Slot,
                    Count = (byte)buffs[i].StackCount,
                    IsHidden = buffs[i].IsHidden
                };
                entries.Add(entry);
            }
            addGroupPacket.Entries = entries;

            _packetHandlerManager.BroadcastPacketVision(target, addGroupPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players who have vision of the target of the specified buff detailing that the buff was removed from its target.
        /// </summary>
        /// <param name="b">Buff that was removed.</param>
        public void NotifyNPC_BuffRemove2(IBuff b)
        {
            var removePacket = new NPC_BuffRemove2
            {
                SenderNetID = b.SourceUnit.NetId, //TODO: Verify if this should change depending on the removal source
                BuffSlot = b.Slot,
                BuffNameHash = HashFunctions.HashString(b.Name),
                RunTimeRemove = b.Duration - b.TimeElapsed
            };
            _packetHandlerManager.BroadcastPacketVision(b.TargetUnit, removePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified ObjAiBase detailing that the specified group of buffs is being removed from the ObjAiBase.
        /// </summary>
        /// <param name="target">ObjAiBase getting their group of buffs removed.</param>
        /// <param name="buffs">Group of buffs getting removed.</param>
        /// <param name="buffName">Internal name of the buff that is applicable to the entire group of buffs.</param>
        public void NotifyNPC_BuffRemoveGroup(IObjAiBase target, List<IBuff> buffs, string buffName)
        {
            var removeGroupPacket = new NPC_BuffRemoveGroup
            {
                SenderNetID = 0,
                BuffNameHash = HashFunctions.HashString(buffName),
            };
            var entries = new List<BuffRemoveGroupEntry>();
            for (int i = 0; i < buffs.Count; i++)
            {
                var entry = new BuffRemoveGroupEntry
                {
                    OwnerNetID = buffs[i].SourceUnit.NetId,
                    Slot = buffs[i].Slot,
                    RunTimeRemove = buffs[i].Duration - buffs[i].TimeElapsed
                };
                entries.Add(entry);
            }
            removeGroupPacket.Entries = entries;

            _packetHandlerManager.BroadcastPacketVision(target, removeGroupPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the target of the specified buff detailing that the buff previously in the same slot was replaced by the newly specified buff.
        /// </summary>
        /// <param name="b">Buff that will replace the old buff in the same slot.</param>
        public void NotifyNPC_BuffReplace(IBuff b)
        {
            var replacePacket = new NPC_BuffReplace
            {
                SenderNetID = b.SourceUnit.NetId,
                BuffSlot = b.Slot,
                RunningTime = b.TimeElapsed,
                Duration = b.Duration,
                CasterNetID = b.OriginSpell.Owner.NetId
            };
            _packetHandlerManager.BroadcastPacketVision(b.TargetUnit, replacePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified ObjAiBase detailing that the buffs already occupying the slots of the group of buffs were replaced by the newly specified group of buffs.
        /// </summary>
        /// <param name="target">ObjAiBase getting their group of buffs replaced.</param>
        /// <param name="buffs">Group of buffs replacing buffs in the same slots.</param>
        /// <param name="runningtime">Time since the group of buffs was created.</param>
        /// <param name="duration">Total time the group of buffs should be active.</param>
        public void NotifyNPC_BuffReplaceGroup(IObjAiBase target, List<IBuff> buffs, float runningtime, float duration)
        {
            var replaceGroupPacket = new NPC_BuffReplaceGroup
            {
                SenderNetID = 0,
                RunningTime = runningtime,
                Duration = duration
            };
            var entries = new List<BuffReplaceGroupEntry>();
            for (int i = 0; i < buffs.Count; i++)
            {
                var entry = new BuffReplaceGroupEntry
                {
                    OwnerNetID = buffs[i].SourceUnit.NetId,
                    CasterNetID = buffs[i].OriginSpell.Owner.NetId,
                    Slot = buffs[i].Slot
                };
                entries.Add(entry);
            }
            replaceGroupPacket.Entries = entries;

            _packetHandlerManager.BroadcastPacketVision(target, replaceGroupPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the target of the specified buff detailing an update to the number of buffs in the specified buff's slot
        /// </summary>
        /// <param name="b">Buff who's count is being updated.</param>
        /// <param name="duration">Total time the buff should last.</param>
        /// <param name="runningTime">Time since the buff's creation.</param>
        public void NotifyNPC_BuffUpdateCount(IBuff b, float duration, float runningTime)
        {
            var updatePacket = new NPC_BuffUpdateCount
            {
                SenderNetID = b.SourceUnit.NetId,
                BuffSlot = b.Slot,
                Count = (byte)b.StackCount,
                Duration = duration,
                RunningTime = runningTime,
                CasterNetID = b.SourceUnit.NetId
            };
            _packetHandlerManager.BroadcastPacketVision(b.TargetUnit, updatePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified target detailing an update to the number of buffs in each of the buff slots occupied by the specified group of buffs.
        /// </summary>
        /// <param name="target">ObjAiBase who's buffs will be updated.</param>
        /// <param name="buffs">Group of buffs to update.</param>
        /// <param name="duration">Total time the buff should last.</param>
        /// <param name="runningTime">Time since the buff's creation.</param>
        public void NotifyNPC_BuffUpdateCountGroup(IObjAiBase target, List<IBuff> buffs, float duration, float runningTime)
        {
            var updateGroupPacket = new NPC_BuffUpdateCountGroup
            {
                SenderNetID = 0,
                Duration = duration,
                RunningTime = runningTime
            };
            var entries = new List<BuffUpdateCountGroupEntry>();
            for (int i = 0; i < buffs.Count; i++)
            {
                var entry = new BuffUpdateCountGroupEntry
                {
                    OwnerNetID = buffs[i].SourceUnit.NetId,
                    CasterNetID = buffs[i].OriginSpell.Owner.NetId,
                    BuffSlot = buffs[i].Slot,
                    Count = (byte)buffs[i].StackCount
                };
                entries.Add(entry);
            }
            updateGroupPacket.Entries = entries;

            _packetHandlerManager.BroadcastPacketVision(target, updateGroupPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the target of the specified buff detailing an update to the stack counter of the specified buff.
        /// </summary>
        /// <param name="b">Buff who's stacks will be updated.</param>
        public void NotifyNPC_BuffUpdateNumCounter(IBuff b)
        {
            var updateNumPacket = new NPC_BuffUpdateNumCounter
            {
                SenderNetID = b.SourceUnit.NetId,
                BuffSlot = b.Slot,
                Counter = b.StackCount // TODO: Verify if it allows stacks to go above 255 on the buff bar
            };
            _packetHandlerManager.BroadcastPacketVision(b.TargetUnit, updateNumPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the owner of the specified spell detailing that a spell has been cast.
        /// </summary>
        /// <param name="navGrid">NavigationGrid instance used for networking positional data.</param>
        /// <param name="s">Spell being cast.</param>
        /// <param name="start">Starting position of the spell.</param>
        /// <param name="end">End position of the spell.</param>
        /// <param name="futureProjNetId">NetId of the projectile that may be spawned by the spell.</param>
        public void NotifyNPC_CastSpellAns(INavigationGrid navGrid, ISpell s, Vector2 start, Vector2 end, uint futureProjNetId)
        {
            var castInfo = new CastInfo
            {
                SpellHash = (uint)s.GetId(),
                SpellNetID = s.SpellNetId,
                SpellLevel = s.Level,
                AttackSpeedModifier = 1.0f, // TOOD: Unhardcode
                CasterNetID = s.Owner.NetId,
                SpellChainOwnerNetID = s.Owner.NetId,
                PackageHash = s.Owner.GetObjHash(),
                MissileNetID = futureProjNetId,
                TargetPosition = new Vector3(start.X, navGrid.GetHeightAtLocation(start.X, start.Y), start.Y),
                TargetPositionEnd = new Vector3(end.X, navGrid.GetHeightAtLocation(end.X, end.Y), end.Y),

                // TODO: Implement castInfo.Targets

                DesignerCastTime = s.SpellData.GetCastTime(), // TODO: Verify
                ExtraCastTime = 0.0f, // TODO: Unhardcode
                DesignerTotalTime = s.SpellData.GetCastTimeTotal(), // TODO: Verify
                Cooldown = s.GetCooldown(),
                StartCastTime = 0.0f, // TODO: Unhardcode

                //TODO: Implement castInfo.IsAutoAttack/IsSecondAutoAttack/IsForceCastingOrChannel/IsOverrideCastPosition/IsClickCasted (you may have checks for all of these, but only one of these can be present in the packet when sent)

                SpellSlot = s.Slot,
                SpellCastLaunchPosition = new Vector3(s.Owner.X, navGrid.GetHeightAtLocation(s.Owner.GetPosition()), s.Owner.Y),
                AmmoUsed = 0, // TODO: Unhardcode (requires implementing Ammo)
                AmmoRechargeTime = s.GetCooldown() // TODO: Implement correctly (requires implementing Ammo)
            };
            if (s.Level > 0)
            {
                castInfo.ManaCost = s.SpellData.ManaCost[s.Level - 1];
            }
            else
            {
                castInfo.ManaCost = s.SpellData.ManaCost[s.Level];
            }

            var castAnsPacket = new NPC_CastSpellAns
            {
                SenderNetID = s.Owner.NetId,
                CasterPositionSyncID = (int)s.Owner.SyncId,
                Unknown1 = false, // TODO: Find what this is (if false, CasterPositionSyncID is used)
                CastInfo = castInfo
            };
            _packetHandlerManager.BroadcastPacketVision(s.Owner, castAnsPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified AttackableUnit detailing that the attacker has abrubtly stopped their attack (can be a spell or auto attack, although internally AAs are also spells).
        /// </summary>
        /// <param name="attacker">AttackableUnit that stopped their auto attack.</param>
        /// <param name="isSummonerSpell">Whether or not the spell is a summoner spell.</param>
        /// <param name="missileNetID">NetId of the missile that may have been spawned by the spell.</param>
        public void NotifyNPC_InstantStopAttack(IAttackableUnit attacker, bool isSummonerSpell, uint missileNetID = 0)
        {
            var stopAttack = new NPC_InstantStop_Attack
            {
                SenderNetID = attacker.NetId,
                MissileNetID = missileNetID, //TODO: Fix MissileNetID, currently it only works when it is 0
                KeepAnimating = false,
                DestroyMissile = true,
                OverrideVisibility = true,
                IsSummonerSpell = isSummonerSpell,
                ForceDoClient = false
            };
            _packetHandlerManager.BroadcastPacketVision(attacker, stopAttack.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified AttackableUnit die has died to the specified AttackableUnit killer.
        /// </summary>
        /// <param name="unit">AttackableUnit that was killed.</param>
        /// <param name="killer">AttackableUnit that killed the unit.</param>
        public void NotifyNpcDie(IAttackableUnit die, IAttackableUnit killer)
        {
            var nd = new NpcDie(die, killer);
            _packetHandlerManager.BroadcastPacket(nd, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified attacker has begun trying to attack (has targeted) the specified target. Functionally makes the attacker face the target.
        /// </summary>
        /// <param name="attacker">AttackableUnit that is targeting another unit.</param>
        /// <param name="target">AttackableUnit being targetted by the attacker.</param>
        /// <param name="attackType">Type of attack; RADIAL/MELEE/TARGETED</param>
        public void NotifyOnAttack(IAttackableUnit attacker, IAttackableUnit attacked, AttackType attackType)
        {
            var oa = new OnAttack(attacker, attacked, attackType);
            _packetHandlerManager.BroadcastPacket(oa, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the game has paused.
        /// </summary>
        /// <param name="seconds">Amount of time till the pause ends.</param>
        /// <param name="showWindow">Whether or not to show a pause window.</param>
        public void NotifyPauseGame(int seconds, bool showWindow)
        {
            var pg = new PauseGame(seconds, showWindow);
            _packetHandlerManager.BroadcastPacket(pg, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified client's team detailing a map ping.
        /// </summary>
        /// <param name="client">Info of the client that initiated the ping.</param>
        /// <param name="pos">2D top-down position of the ping.</param>
        /// <param name="targetNetId">Target of the ping (if applicable).</param>
        /// <param name="type">Type of ping; COMMAND/ATTACK/DANGER/MISSING/ONMYWAY/FALLBACK/REQUESTHELP. *NOTE*: Not all ping types are supported yet.</param>
        public void NotifyPing(ClientInfo client, Vector2 pos, int targetNetId, Pings type)
        {
            var ping = new AttentionPingRequest(pos.X, pos.Y, targetNetId, type);
            var response = new AttentionPingResponse(client, ping);
            _packetHandlerManager.BroadcastPacketTeam(client.Team, response, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing the specified client's loading screen progress.
        /// </summary>
        /// <param name="request">Info of the target client given via the client who requested loading screen progress.</param>
        /// <param name="clientInfo">Client info of the client who's progress is being requested.</param>
        public void NotifyPingLoadInfo(PingLoadInfoRequest request, ClientInfo clientInfo)
        {
            var response = new PingLoadInfoResponse(request.NetId, clientInfo.ClientId, request.Loaded, request.Unk2,
                request.Ping, request.Unk3, request.Unk4, clientInfo.PlayerId);

            //Logging->writeLine("loaded: %f, ping: %f, %f", loadInfo->loaded, loadInfo->ping, loadInfo->f3);
            _packetHandlerManager.BroadcastPacket(response, Channel.CHL_LOW_PRIORITY, PacketFlags.None);
        }

        /// <summary>
        /// Sends a packet to all players detailing the stats (CS, kills, deaths, etc) of the player who owns the specified Champion.
        /// </summary>
        /// <param name="champion">Champion owned by the player.</param>
        public void NotifyPlayerStats(IChampion champion)
        {
            var response = new PlayerStats(champion);
            // TODO: research how to send the packet
            _packetHandlerManager.BroadcastPacket(response, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player which is meant as a response to the players query about the status of the game.
        /// </summary>
        /// <param name="userId">User to send the packet to; player that sent the query.</param>
        public void NotifyQueryStatus(int userId)
        {
            var response = new QueryStatus();
            _packetHandlerManager.SendPacket(userId, response, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the specified Debug Object has been removed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="sender">NetId of the GameObject that is responsible for this packet being sent.</param>
        /// <param name="objID">Debug Object being removed.</param>
        public void NotifyRemoveDebugObject(int userId, uint sender, int objID)
        {
            var debugObjPacket = new S2C_RemoveDebugObject
            {
                SenderNetID = sender,
                ObjectID = objID
            };
            _packetHandlerManager.SendPacket(userId, debugObjPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified Champion detailing that item in the specified slot was removed (or the number of stacks of the item in that slot changed).
        /// </summary>
        /// <param name="c">Champion with the items.</param>
        /// <param name="slot">Slot of the item that was removed.</param>
        /// <param name="remaining">Number of stacks of the item left (0 if not applicable).</param>
        public void NotifyRemoveItem(IChampion c, byte slot, byte remaining)
        {
            var ria = new RemoveItemAns()
            {
                SenderNetID = c.NetId,
                Slot = slot,
                ItemsInSlot = remaining,
                NotifyInventoryChange = true
            };
            _packetHandlerManager.BroadcastPacketVision(c, ria.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the highlight of the specified GameObject was removed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="unit">GameObject that had the highlight.</param>
        public void NotifyRemoveUnitHighlight(int userId, IGameObject unit)
        {
            var highlightPacket = new S2C_RemoveUnitHighlight
            {
                SenderNetID = unit.NetId,
                NetID = unit.NetId
            };
            _packetHandlerManager.SendPacket(userId, highlightPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the game has been unpaused.
        /// </summary>
        /// <param name="unpauser">Unit that unpaused the game.</param>
        /// <param name="showWindow">Whether or not to show a window before unpausing (delay).</param>
        public void NotifyResumeGame(IAttackableUnit unpauser, bool showWindow)
        {
            UnpauseGame upg;
            if (unpauser == null)
            {
                upg = new UnpauseGame(0, showWindow);
            }
            else
            {
                upg = new UnpauseGame(unpauser.NetId, showWindow);
            }

            _packetHandlerManager.BroadcastPacket(upg, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the server has ticked within the specified time delta.
        /// Unused.
        /// </summary>
        /// <param name="delta">Time it took to tick.</param>
        public void NotifyServerTick(float delta)
        {
            var tickPacket = new ServerTick
            {
                Delta = delta
            };

            _packetHandlerManager.BroadcastPacket(tickPacket.GetBytes(), Channel.CHL_GAMEPLAY);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the unit's animations have been set.
        /// </summary>
        /// <param name="u">AttackableUnit to set the animations of.</param>
        /// <param name="animationPairs">Animations to apply.</param>
        public void NotifySetAnimation(IAttackableUnit u, List<string> animationPairs)
        {
            var setAnimation = new SetAnimation(u, animationPairs);
            _packetHandlerManager.BroadcastPacketVision(u, setAnimation, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players on the specified team detailing whether the team has become able to surrender.
        /// </summary>
        /// <param name="can">Whether or not the team should be able to surrender.</param>
        /// <param name="team">Team to send the packet to.</param>
        public void NotifySetCanSurrender(bool can, TeamId team)
        {
            var canSurrender = new S2C_SetCanSurrender()
            {
                CanSurrender = can
            };
            _packetHandlerManager.BroadcastPacketTeam(team, canSurrender.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that their range of movement has changed. This packet forces the game client to only send movement request packets when the distance from the specified center is less than the specified radius.
        /// </summary>
        /// <param name="ai">ObjAiBase that the restriction is being applied to.</param>
        /// <param name="center">Center of the restriction circle.</param>
        /// <param name="radius">Radius of the restriction circle; minimum distance from center required to move.</param>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="restrictCam">Whether or not the player's camera is also restricted to the same area.</param>
        public void NotifySetCircularMovementRestriction(IObjAiBase ai, Vector2 center, float radius, int userId, bool restrictCam = false)
        {
            var restrictPacket = new S2C_SetCircularMovementRestriction()
            {
                SenderNetID = ai.NetId,
                Center = new Vector3(center.X, ai.GetHeight(), center.Y),
                Radius = radius,
                RestrictCamera = restrictCam
            };
            _packetHandlerManager.SendPacket(userId, restrictPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the specified Debug Object has become hidden.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="sender">NetId that is sending this packet.</param>
        /// <param name="objID">ID of the Debug Object that will be hidden.</param>
        /// <param name="bitfield">Unknown variable.</param>
        public void NotifySetDebugHidden(int userId, uint sender, int objID, byte bitfield = 0x0)
        {
            var debugObjPacket = new S2C_SetDebugHidden
            {
                SenderNetID = sender,
                ObjectID = objID,
                Bitfield = bitfield // Not sure what this does
            };
            _packetHandlerManager.SendPacket(userId, debugObjPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified attacker detailing that they have targeted the specified target.
        /// </summary>
        /// <param name="attacker">AttackableUnit that is targeting an AttackableUnit.</param>
        /// <param name="target">AttackableUnit that is being targeted by the attacker.</param>
        public void NotifySetTarget(IAttackableUnit attacker, IAttackableUnit target)
        {
            var st = new AI_TargetS2C
            {
                SenderNetID = attacker.NetId,
                TargetNetID = target.NetId
            };
            _packetHandlerManager.BroadcastPacketVision(attacker, st.GetBytes(), Channel.CHL_S2C);

            // TODO: Verify if this is the correct usage of the AI_TargetHeroS2C packet.
            if (target is IChampion)
            {
                var st2 = new AI_TargetHeroS2C
                {
                    SenderNetID = attacker.NetId,
                    TargetNetID = target.NetId
                };
                _packetHandlerManager.BroadcastPacketVision(attacker, st2.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified unit's team has been set.
        /// </summary>
        /// <param name="unit">AttackableUnit who's team has been set.</param>
        public void NotifySetTeam(IAttackableUnit unit)
        {
            var p = new S2C_UnitChangeTeam
            {
                SenderNetID = unit.NetId,
                UnitNetID = unit.NetId,
                TeamID = (uint)unit.Team // TODO: Verify if TeamID is actually supposed to be a uint
            };
            _packetHandlerManager.BroadcastPacket(p.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that they have leveled up the specified skill.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the GameObject whos skill is being leveled up.</param>
        /// <param name="skill">Slot of the skill being leveled up.</param>
        /// <param name="level">Current level of the skill.</param>
        /// <param name="pointsLeft">Number of skill points available after the skill has been leveled up.</param>
        public void NotifySkillUp(int userId, uint netId, byte skill, byte level, byte pointsLeft)
        {
            var skillUpResponse = new NPC_UpgradeSpellAns
            {
                SenderNetID = netId,
                Slot = skill,
                SpellLevel = level,
                SkillPoints = pointsLeft
            };
            _packetHandlerManager.SendPacket(userId, skillUpResponse.GetBytes(), Channel.CHL_GAMEPLAY);
        }

        /// <summary>
        /// Calls for the appropriate spawn packet to be sent given the specified GameObject's type and calls for a vision packet to be sent for the specified GameObject.
        /// </summary>
        /// <param name="o">GameObject that has spawned.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        public void NotifySpawn(IGameObject o, TeamId team)
        {
            switch (o)
            {
                case ILaneMinion m:
                    NotifyLaneMinionSpawned(m, team);
                    break;
                case IChampion c:
                    NotifyChampionSpawned(c, c.Team.GetEnemyTeam());
                    break;
                case IMonster monster:
                    NotifyMonsterSpawned(monster);
                    break;
                case IMinion minion:
                    NotifyMinionSpawned(minion, minion.Team.GetEnemyTeam());
                    break;
                case IAzirTurret azirTurret:
                    NotifyAzirTurretSpawned(azirTurret);
                    break;
            }

            NotifyEnterVisibilityClient(o, o.Team);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the spawning (of champions & buildings) that occurs at the start of the game has ended.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifySpawnEnd(int userId)
        {
            var endSpawnPacket = new StatePacket(PacketCmd.PKT_S2C_END_SPAWN);
            _packetHandlerManager.SendPacket(userId, endSpawnPacket, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the game has started the spawning GameObjects that occurs at the start of the game.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifySpawnStart(int userId)
        {
            var start = new S2C_StartSpawn
            {
                // TODO: Set these values when bots are implemented.
                BotCountOrder = 0,
                BotCountChaos = 0
            };
            _packetHandlerManager.SendPacket(userId, start.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the unit is playing an animation.
        /// </summary>
        /// <param name="u">Unit that will do the animation.</param>
        /// <param name="animation">Animation that the unit will do.</param>
        public void NotifySpellAnimation(IAttackableUnit u, string animation)
        {
            var sa = new SpellAnimation(u, animation);
            _packetHandlerManager.BroadcastPacketVision(u, sa, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the GameObject associated with the specified NetID has spawned.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetID of the GameObject that has spawned.</param>
        /// TODO: Remove this and replace all usages with NotifyEnterVisibilityClient, refer to the MinionSpawn2 packet as it uses the same packet command.
        public void NotifyStaticObjectSpawn(int userId, uint netId)
        {
            var minionSpawnPacket = new MinionSpawn2(netId);
            _packetHandlerManager.SendPacket(userId, minionSpawnPacket, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing the results of server's the version and game info check for the specified player.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="players">List of ClientInfo of all players set to connect to the game.</param>
        /// <param name="version">Version of the player being checked.</param>
        /// <param name="gameMode">String of the internal name of the gamemode being played.</param>
        /// <param name="mapId">ID of the map being played.</param>
        public void NotifySynchVersion(int userId, List<Tuple<uint, ClientInfo>> players, string version, string gameMode, int mapId)
        {
            var response = new SynchVersionResponse(players, version, "CLASSIC", mapId);
            _packetHandlerManager.SendPacket(userId, response, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing the status (results) of a surrender vote that was called for and ended.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="team">TeamId that called for the surrender vote; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="reason">SurrenderReason of why the vote ended.</param>
        /// <param name="yesVotes">Number of votes for the surrender.</param>
        /// <param name="noVotes">Number of votes against the surrender.</param>
        public void NotifyTeamSurrenderStatus(int userId, TeamId team, SurrenderReason reason, byte yesVotes, byte noVotes)
        {
            var surrenderStatus = new S2C_TeamSurrenderStatus()
            {
                SurrenderReason = (uint)reason,
                ForVote = yesVotes,
                AgainstVote = noVotes,
                TeamID = (uint)team,
            };
            _packetHandlerManager.SendPacket(userId, surrenderStatus.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players on the same team as the Champion that made the surrender vote detailing what vote was made.
        /// </summary>
        /// <param name="starter">Champion that made the surrender vote.</param>
        /// <param name="open">Whether or not to automatically open the surrender voting menu.</param>
        /// <param name="votedYes">Whether or not voting for the surrender is still available.</param>
        /// <param name="yesVotes">Number of players currently for the surrender.</param>
        /// <param name="noVotes">Number of players currently against the surrender.</param>
        /// <param name="maxVotes">Maximum number of votes possible.</param>
        /// <param name="timeOut">Time until voting becomes unavailable.</param>
        public void NotifyTeamSurrenderVote(IChampion starter, bool open, bool votedYes, byte yesVotes, byte noVotes, byte maxVotes, float timeOut)
        {
            var surrender = new S2C_TeamSurrenderVote()
            {
                PlayerNetID = starter.NetId,
                OpenVoteMenu = open,
                VoteYes = votedYes,
                ForVote = yesVotes,
                AgainstVote = noVotes,
                NumPlayers = maxVotes,
                TeamID = (uint)starter.Team,
                TimeOut = timeOut,
            };
            _packetHandlerManager.BroadcastPacketTeam(starter.Team, surrender.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the unit has teleported to the specified position.
        /// </summary>
        /// <param name="u">AttackableUnit that teleported.</param>
        /// <param name="pos">2D top-down position that the unit teleported to.</param>
        /// TODO: Take into account any movements (waypoints) that should carry over after the teleport.
        public void NotifyTeleport(IAttackableUnit u, Vector2 pos)
        {
            // position is already in centered format
            var packet = new WaypointGroup
            {
                SenderNetID = u.NetId,
                SyncID = (int)u.SyncId,
                Movements = new List<MovementDataNormal>()
            };

            var tp = new MovementDataNormal
            {
                SyncID = (int)u.SyncId,
                HasTeleportID = true,
                TeleportID = 1,
                TeleportNetID = u.NetId,
                Waypoints = new List<CompressedWaypoint>()
            };
            // Not needed
            //var npos = MovementVector.ToCenteredScaledCoordinates(pos, _navGrid);
            var way = new CompressedWaypoint((short)pos.X, (short)pos.Y);
            tp.Waypoints.Add(way);

            packet.Movements.Add(tp);

            _packetHandlerManager.BroadcastPacketVision(u, packet.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that their screen's tint is shifting to the specified color.
        /// </summary>
        /// <param name="team">TeamID to apply the tint to.</param>
        /// <param name="enable">Whether or not to fade in the tint.</param>
        /// <param name="speed">Amount of time that should pass before tint is fully applied.</param>
        /// <param name="color">Color of the tint.</param>
        public void NotifyTint(TeamId team, bool enable, float speed, GameServerCore.Content.Color color)
        {
            var c = new LeaguePackets.Game.Common.Color
            {
                Blue = color.B,
                Green = color.G,
                Red = color.R,
                Alpha = color.A
            };
            var tint = new S2C_ColorRemapFX
            {
                IsFadingIn = enable,
                FadeTime = speed,
                TeamID = (uint)team,
                Color = c,
                MaxWeight = (c.Alpha / 255.0f) // TODO: Implement this correctly, current implementation taken from old LS packet
            };
            _packetHandlerManager.BroadcastPacket(tint.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the specified LaneTurret has spawned.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="turret">LaneTurret that spawned.</param>
        public void NotifyTurretSpawn(int userId, ILaneTurret turret)
        {
            var turretSpawn = new TurretSpawn(turret);
            _packetHandlerManager.SendPacket(userId, turretSpawn, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified event has occurred.
        /// </summary>
        /// <param name="messageId">ID of the event that has occurred. *NOTE*: This enum is incomplete and will be renamed to EventID</param>
        /// <param name="target">Unit that caused the event to occur.</param>
        /// <param name="killer">Optional killer of the unit that caused the event to occur.</param>
        /// <param name="assists">Optional number of assists of the killer.</param>
        /// TODO: Replace this with LeaguePackets, rename UnitAnnounces to EventID, and complete its enum (refer to LeaguePackets.Game.Events.EventID).
        public void NotifyUnitAnnounceEvent(UnitAnnounces messageId, IAttackableUnit target, IGameObject killer = null,
            List<IChampion> assists = null)
        {
            var announce = new UnitAnnounce(messageId, target, killer, assists);
            _packetHandlerManager.BroadcastPacket(announce, Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the specified target GameObject's (debug?) path drawing mode has been set to the specified mode.
        /// </summary>
        /// <param name="userId">User to send the packet to(?).</param>
        /// <param name="unit">Unit that has called for the packet.</param>
        /// <param name="target">GameObject who's (debug?) draw path mode is being set.</param>
        /// <param name="mode">Draw path mode to set.</param>
        /// TODO: Verify the functionality of this packet (and its parameters) and create an enum for the mode.
        public void NotifyUnitSetDrawPathMode(int userId, IAttackableUnit unit, IGameObject target, byte mode)
        {
            var drawPacket = new S2C_UnitSetDrawPathMode
            {
                SenderNetID = unit.NetId,
                TargetNetID = target.NetId,
                DrawPathMode = 0x1,
                UpdateRate = 0.1f
            };
            _packetHandlerManager.SendPacket(userId, drawPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Unfinished(?) function which intends to resume the game automatically (without client requests). This is usually called after the pause time has ended in Game.GameLoop.
        /// </summary>
        /// TODO: Verify if this works and if not, then finish it.
        public void NotifyUnpauseGame()
        {
            // TODO: currently unpause disabled cause it shouldn't handled like this
            _packetHandlerManager.UnpauseGame();
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the specified unit's stats have been updated.
        /// </summary>
        /// <param name="u">Unit who's stats have been updated.</param>
        /// <param name="partial">Whether or not the packet should be counted as a partial update (whether the stats have actually changed or not). *NOTE*: Use case for this parameter is unknown.</param>
        /// TODO: Replace with LeaguePackets and preferably move all uses of this function to a central EventHandler class (if one is fully implemented).
        public void NotifyUpdatedStats(IAttackableUnit u, bool partial = true)
        {
            if (u.Replication != null)
            {
                var us = new UpdateStats(u.Replication, partial);
                var channel = Channel.CHL_LOW_PRIORITY;
                _packetHandlerManager.BroadcastPacketVision(u, us, channel, PacketFlags.Unsequenced);
                if (partial)
                {
                    u.Replication.MarkAsUnchanged();
                }
            }
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that their request to view something with their camera has been acknowledged.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="request">ViewRequest housing information about the camera's view.</param>
        /// TODO: Verify if this is the correct implementation.
        public void NotifyViewResponse(int userId, ViewRequest request)
        {
            var answer = new ViewResponse(request.NetId);
            if (request.RequestNo == 0xFE)
            {
                answer.SetRequestNo(0xFF);
            }
            else
            {
                answer.SetRequestNo(request.RequestNo);
            }

            _packetHandlerManager.SendPacket(userId, answer, Channel.CHL_S2C, PacketFlags.None);
        }
    }
}

using ENet;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using GameServerCore.NetInfo;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using PacketDefinitions420.Enums;
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
using static GameServerCore.Content.HashFunctions;
using System.Text;
using Force.Crc32;
using System.Linq;

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
        /// Sends a packet to the specified team that a part of the map has changed. Known to be used in League for initializing turret vision and collision.
        /// </summary>
        /// <param name="newFogId">NetID of the owner of the region.</param>
        /// <param name="team">Team to send the packet to.</param>
        /// <param name="position">2D top-down position of the region.</param>
        /// <param name="time">Amount of time the region lasts.</param>
        /// <param name="radius">Radius of the region.</param>
        /// <param name="regionType">Type of region, possible values unknown.</param>
        /// <param name="clientInfo">Info about a client that might own (or be the target of) the region.</param>
        /// <param name="obj">GameObject that might own (or be the target of) the region.</param>
        /// <param name="collisionRadius">Collision radius for the region (only if it should have collision).</param>
        /// <param name="grassRadius">Radius of the region's grass.</param>
        /// <param name="sizemult">Multiplier that is applied to the radius of the region.</param>
        /// <param name="addsize">Number of units to add to the region's radius.</param>
        /// <param name="grantVis">Whether or not the region should give the region's team vision of enemy units.</param>
        /// <param name="stealthVis">Whether or not invisible units should be visible in the region.</param>
        /// TODO: Implement a Region class so we can easily grab these parameters instead of listing them all in the function.
        public void NotifyAddRegion(uint newFogId, TeamId team, Vector2 position, float time, float radius = 0, int regionType = 0, ClientInfo clientInfo = null, IGameObject obj = null, float collisionRadius = 0, float grassRadius = 0, float sizemult = 1.0f, float addsize = 0, bool grantVis = true, bool stealthVis = false)
        {
            var regionPacket = new AddRegion
            {
                TeamID = (uint)team,
                RegionType = regionType, // TODO: Find out what values this can be and make an enum for it (so far: -2 for turrets)
                UnitNetID = newFogId, // TODO: Verify (usually 0 for vision only?)
                BubbleNetID = newFogId, // TODO: Verify (is usually different from UnitNetID in packets, may also be a remnant or for internal use)
                VisionTargetNetID = 0,
                Position = position,
                TimeToLive = time, // For turrets, usually 25000.0 is used
                ColisionRadius = collisionRadius, // 88.4 for turrets
                GrassRadius = grassRadius, // 130.0 for turrets
                SizeMultiplier = sizemult,
                SizeAdditive = addsize,

                HasCollision = false,
                GrantVision = grantVis,
                RevealStealth = stealthVis,

                BaseRadius = radius // 800.0 for turrets
            };

            if (clientInfo != null)
            {
                if (clientInfo.Champion != null)
                {
                    regionPacket.VisionTargetNetID = clientInfo.Champion.NetId;
                }
                regionPacket.ClientID = (int)clientInfo.ClientId;
            }

            if (obj != null)
            {
                regionPacket.VisionTargetNetID = obj.NetId;
            }

            // TODO: Verify
            if (collisionRadius > 0.0f)
            {
                regionPacket.HasCollision = true;
            }


            _packetHandlerManager.BroadcastPacketTeam(team, regionPacket.GetBytes(), Channel.CHL_S2C);
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
        /// Sends a packet to all players with vision of the specified attacker detailing that they have targeted the specified target.
        /// </summary>
        /// <param name="attacker">AI that is targeting an AttackableUnit.</param>
        /// <param name="target">AttackableUnit that is being targeted by the attacker.</param>
        public void NotifyAI_TargetS2C(IObjAiBase attacker, IAttackableUnit target)
        {
            var targetPacket = new AI_TargetS2C
            {
                SenderNetID = attacker.NetId,
                TargetNetID = 0
            };

            if (target != null)
            {
                targetPacket.TargetNetID = target.NetId;
            }

            // TODO: Verify if we need to account for other cases.
            if (attacker is IBaseTurret)
            {
                _packetHandlerManager.BroadcastPacket(targetPacket.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.BroadcastPacketVision(attacker, targetPacket.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified attacker detailing that they have targeted the specified champion.
        /// </summary>
        /// <param name="attacker">AI that is targeting a champion.</param>
        /// <param name="target">Champion that is being targeted by the attacker.</param>
        public void NotifyAI_TargetHeroS2C(IObjAiBase attacker, IChampion target)
        {
            var targetPacket = new AI_TargetHeroS2C
            {
                SenderNetID = attacker.NetId,
                TargetNetID = 0
            };

            if (target != null)
            {
                targetPacket.TargetNetID = target.NetId;
            }

            _packetHandlerManager.BroadcastPacketVision(attacker, targetPacket.GetBytes(), Channel.CHL_S2C);
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
        /// Sends a packet to all players detailing that the specified  unit is starting their next auto attack.
        /// </summary>
        /// <param name="attacker">Unit that is attacking.</param>
        /// <param name="target">AttackableUnit being attacked.</param>
        /// <param name="futureProjNetId">NetId of the auto attack projectile.</param>
        /// <param name="isCrit">Whether or not the auto attack will crit.</param>
        /// <param name="nextAttackFlag">Whether or this basic attack is not the first time this basic attack has been performed on the given target.</param>
        /// TODO: Verify the differences between normal Basic_Attack and Basic_Attack_Pos.
        public void NotifyBasic_Attack(IObjAiBase attacker, IAttackableUnit target, uint futureProjNetId, bool isCrit, bool nextAttackFlag)
        {
            var targetPos = MovementVector.ToCenteredScaledCoordinates(target.Position, _navGrid);

            // TODO: Remove attacker, target, and futureProjNetId parameters and replace with CastInfo
            var basicAttackData = new BasicAttackData
            {
                TargetNetID = target.NetId,
                ExtraTime = attacker.AutoAttackSpell.CastInfo.ExtraCastTime, // TODO: Verify, maybe related to CastInfo.ExtraCastTime?
                MissileNextID = futureProjNetId,
                AttackSlot = attacker.AutoAttackSpell.CastInfo.SpellSlot,
                // TODO: Verify TargetPosition, taken from LS packet
                TargetPosition = new Vector3(targetPos.X, _navGrid.GetHeightAtLocation(targetPos.X, targetPos.Y), targetPos.Y)
            };

            if (!attacker.HasMadeInitialAttack)
            {
                basicAttackData.ExtraTime = -0.01f; // TODO: Verify, maybe related to CastInfo.ExtraCastTime?
            }

            var basicAttackPacket = new Basic_Attack
            {
                SenderNetID = attacker.NetId,
                Attack = basicAttackData
            };
            _packetHandlerManager.BroadcastPacketVision(attacker, basicAttackPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that the specified attacker is starting their first auto attack.
        /// </summary>
        /// <param name="attacker">AI that is starting an auto attack.</param>
        /// <param name="target">AttackableUnit being attacked.</param>
        /// <param name="futureProjNetId">NetID of the projectile that will be created for the auto attack.</param>
        /// <param name="isCrit">Whether or not the auto attack is a critical.</param>
        /// TODO: Verify the differences between BasicAttackPos and normal BasicAttack.
        public void NotifyBasic_Attack_Pos(IObjAiBase attacker, IAttackableUnit target, uint futureProjNetId, bool isCrit)
        {
            var targetPos = MovementVector.ToCenteredScaledCoordinates(target.Position, _navGrid);

            // TODO: Remove attacker, target, and futureProjNetId parameters and replace with CastInfo
            var basicAttackData = new BasicAttackData
            {
                TargetNetID = target.NetId,
                ExtraTime = attacker.AutoAttackSpell.CastInfo.ExtraCastTime, // TODO: Verify, maybe related to CastInfo.ExtraCastTime?
                MissileNextID = futureProjNetId,
                AttackSlot = attacker.AutoAttackSpell.CastInfo.SpellSlot,
                TargetPosition = new Vector3(targetPos.X, target.GetHeight(), targetPos.Y)
            };

            var basicAttackPacket = new Basic_Attack_Pos
            {
                SenderNetID = attacker.NetId,
                Attack = basicAttackData,
                Position = attacker.Position // TODO: Verify
            };
            _packetHandlerManager.BroadcastPacketVision(attacker, basicAttackPacket.GetBytes(), Channel.CHL_S2C);
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
        /// Sends a packet to the specified user detailing that the specified owner unit's spell in the specified slot has been changed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="owner">Unit that owns the spell being changed.</param>
        /// <param name="slot">Slot of the spell being changed.</param>
        /// <param name="changeType">Type of change being made.</param>
        /// <param name="isSummonerSpell">Whether or not the spell being changed is a summoner spell.</param>
        /// <param name="targetingType">New targeting type to set.</param>
        /// <param name="newName">New internal name of a spell to set.</param>
        /// <param name="newRange">New cast range for the spell to set.</param>
        /// <param name="newMaxCastRange">New max cast range for the spell to set.</param>
        /// <param name="newDisplayRange">New max display range for the spell to set.</param>
        /// <param name="newIconIndex">New index of an icon for the spell to set.</param>
        /// <param name="offsetTargets">New target netids for the spell to set.</param>
        public void NotifyChangeSlotSpellData(int userId, IObjAiBase owner, byte slot, GameServerCore.Enums.ChangeSlotSpellDataType changeType, bool isSummonerSpell = false, TargetingType targetingType = TargetingType.Invalid, string newName = "", float newRange = 0, float newMaxCastRange = 0, float newDisplayRange = 0, byte newIconIndex = 0x0, List<uint> offsetTargets = null)
        {
            ChangeSpellData spellData = new ChangeSpellDataUnknown()
            {
                SpellSlot = slot,
                IsSummonerSpell = isSummonerSpell
            };

            switch(changeType)
            {
                case GameServerCore.Enums.ChangeSlotSpellDataType.TargetingType:
                {
                    if (targetingType != TargetingType.Invalid)
                    {
                        spellData = new ChangeSpellDataTargetingType()
                        {
                            SpellSlot = slot,
                            IsSummonerSpell = isSummonerSpell,
                            TargetingType = (byte)targetingType
                        };
                    }
                    break;
                }
                case GameServerCore.Enums.ChangeSlotSpellDataType.SpellName:
                {
                    spellData = new ChangeSpellDataSpellName()
                    {
                        SpellSlot = slot,
                        IsSummonerSpell = isSummonerSpell,
                        SpellName = newName
                    };
                    break;
                }
                case GameServerCore.Enums.ChangeSlotSpellDataType.Range:
                {
                    spellData = new ChangeSpellDataRange()
                    {
                        SpellSlot = slot,
                        IsSummonerSpell = isSummonerSpell,
                        CastRange = newRange
                    };
                    break;
                }
                case GameServerCore.Enums.ChangeSlotSpellDataType.MaxGrowthRange:
                {
                    spellData = new ChangeSpellDataMaxGrowthRange()
                    {
                        SpellSlot = slot,
                        IsSummonerSpell = isSummonerSpell,
                        OverrideMaxCastRange = newMaxCastRange
                    };
                    break;
                }
                case GameServerCore.Enums.ChangeSlotSpellDataType.RangeDisplay:
                {
                    spellData = new ChangeSpellDataRangeDisplay()
                    {
                        SpellSlot = slot,
                        IsSummonerSpell = isSummonerSpell,
                        OverrideCastRangeDisplay = newDisplayRange
                    };
                    break;
                }
                case GameServerCore.Enums.ChangeSlotSpellDataType.IconIndex:
                {
                    spellData = new ChangeSpellDataIconIndex()
                    {
                        SpellSlot = slot,
                        IsSummonerSpell = isSummonerSpell,
                        IconIndex = newIconIndex
                    };
                    break;
                }
                case GameServerCore.Enums.ChangeSlotSpellDataType.OffsetTarget:
                {
                    if (offsetTargets != null)
                    {
                        spellData = new ChangeSpellDataOffsetTarget()
                        {
                            SpellSlot = slot,
                            IsSummonerSpell = isSummonerSpell,
                            Targets = offsetTargets
                        };
                    }
                    break;
                }
            }

            var changePacket = new ChangeSlotSpellData()
            {
                SenderNetID = owner.NetId,
                ChangeSpellData = spellData
            };

            _packetHandlerManager.SendPacket(userId, changePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of a specified ObjAiBase explaining that their specified spell's cooldown has been set.
        /// </summary>
        /// <param name="u">ObjAiBase who owns the spell going on cooldown.</param>
        /// <param name="slotId">Slot of the spell.</param>
        /// <param name="currentCd">Amount of time the spell has already been on cooldown (if applicable).</param>
        /// <param name="totalCd">Maximum amount of time the spell's cooldown can be.</param>
        public void NotifyCHAR_SetCooldown(IObjAiBase u, byte slotId, float currentCd, float totalCd)
        {
            var cdPacket = new CHAR_SetCooldown
            {
                SenderNetID = u.NetId,
                Slot = slotId,
                PlayVOWhenCooldownReady = false, // TODO: Unhardcode
                IsSummonerSpell = false, // TODO: Unhardcode
                Cooldown = currentCd,
                MaxCooldownForDisplay = 0 // TODO: Verify (packet loses functionality otherwise)
            };
            if (u is IChampion && (slotId == 4 || slotId == 5))
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
        public void NotifyDestroyClientMissile(ISpellMissile p)
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
        public void NotifyDestroyClientMissile(ISpellMissile p, TeamId team)
        {
            var misPacket = new S2C_DestroyClientMissile
            {
                SenderNetID = p.NetId
            };
            _packetHandlerManager.BroadcastPacketTeam(team, misPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to either all players with vision of a target, or the specified player.
        /// The packet displays the specified message of the specified type as floating text over a target.
        /// </summary>
        /// <param name="target">Target to display on.</param>
        /// <param name="message">Message to display.</param>
        /// <param name="textType">Type of text to display. Refer to FloatTextType</param>
        /// <param name="userId">User to send to. 0 = sends to all in vision.</param>
        /// <param name="param">Optional parameters for the text. Untested, function unknown.</param>
        public void NotifyDisplayFloatingText(IGameObject target, string message, FloatTextType textType = FloatTextType.Debug, int userId = 0, int param = 0)
        {
            var textPacket = new DisplayFloatingText
            {
                TargetNetID = target.NetId,
                FloatTextType = (uint)textType,
                Param = param,
                Message = message
            };

            if (userId == 0)
            {
                _packetHandlerManager.BroadcastPacketVision(target, textPacket.GetBytes(), Channel.CHL_S2C);
            }

            _packetHandlerManager.SendPacket(userId, textPacket.GetBytes(), Channel.CHL_S2C);
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
            var enterLocalVis = new OnEnterLocalVisibilityClient
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
        /// <param name="ignoreVision">Optionally ignore vision checks when sending this packet.</param>
        public void NotifyEnterLocalVisibilityClient(IGameObject o, int userId = 0, bool ignoreVision = false)
        {
            var enterLocalVis = new OnEnterLocalVisibilityClient
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
                if (ignoreVision)
                {
                    _packetHandlerManager.BroadcastPacket(enterLocalVis.GetBytes(), Channel.CHL_S2C);
                    return;
                }

                _packetHandlerManager.BroadcastPacketVision(o, enterLocalVis.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.SendPacket(userId, enterLocalVis.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to either all players with vision of the specified object or the specified user. The packet details the data surrounding the specified GameObject that is required by players when a GameObject enters vision such as items, shields, skin, and movements.
        /// </summary>
        /// <param name="o">GameObject entering vision.</param>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="isChampion">Whether or not the GameObject entering vision is a Champion.</param>
        /// <param name="useTeleportID">Whether or not to teleport the object to its current position.</param>
        /// <param name="ignoreVision">Optionally ignore vision checks when sending this packet.</param>
        /// TODO: Incomplete implementation.
        public void NotifyEnterVisibilityClient(IGameObject o, int userId = 0, bool isChampion = false, bool useTeleportID = false, bool ignoreVision = false)
        {
            var itemData = new List<ItemData>(); //TODO: Fix item system so this can be finished
            var shields = new ShieldValues(); //TODO: Implement shields so this can be finished

            var charStackDataList = new List<CharacterStackData>();
            var charStackData = new CharacterStackData
            {
                SkinID = 0,
                OverrideSpells = false,
                ModelOnly = false,
                ReplaceCharacterPackage = false,
                ID = 0
            };

            var buffCountList = new List<KeyValuePair<byte, int>>();

            if (o is IAttackableUnit a)
            {
                charStackData.SkinName = a.Model;

                if (a is IChampion c)
                {
                    charStackData.SkinID = (uint)c.Skin;
                }

                buffCountList = new List<KeyValuePair<byte, int>>();
                var tempBuffs = a.GetParentBuffs();

                for (var i = 0; i < tempBuffs.Count; i++)
                {
                    var buff = tempBuffs.ElementAt(i).Value;
                    buffCountList.Add(new KeyValuePair<byte, int>(buff.Slot, buff.StackCount));
                }

                // TODO: if (a.IsDashing), requires SpeedParams, add it to AttackableUnit so it can be accessed outside of initialization
            }

            charStackDataList.Add(charStackData);

            var type = MovementDataType.Normal;

            if (o is IAttackableUnit unit && unit.Waypoints.Count <= 1)
            {
                type = MovementDataType.Stop;
            }

            var md = PacketExtensions.CreateMovementData(o, _navGrid, type, useTeleportID: useTeleportID);

            var enterVis = new OnEnterVisibilityClient // TYPO >:(
            {
                SenderNetID = o.NetId,
                Items = itemData,
                ShieldValues = shields,
                CharacterDataStack = charStackDataList,
                BuffCount = buffCountList,
                LookAtPosition = new Vector3(1, 0, 0),
                // TODO: Verify
                UnknownIsHero = isChampion,
                MovementData = md
            };

            if (userId != 0)
            {
                _packetHandlerManager.SendPacket(userId, enterVis.GetBytes(), Channel.CHL_S2C);
                NotifyEnterLocalVisibilityClient(o, userId, ignoreVision);
            }
            else
            {
                if (ignoreVision)
                {
                    _packetHandlerManager.BroadcastPacket(enterVis.GetBytes(), Channel.CHL_S2C);
                    NotifyEnterLocalVisibilityClient(o, ignoreVision: true);
                    return;
                }

                _packetHandlerManager.BroadcastPacketVision(o, enterVis.GetBytes(), Channel.CHL_S2C);
                NotifyEnterLocalVisibilityClient(o);
            }
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the unit has begun facing the specified direction.
        /// </summary>
        /// <param name="obj">GameObject that is changing their orientation.</param>
        /// <param name="direction">3D direction the unit will face.</param>
        /// <param name="isInstant">Whether or not the unit should instantly turn to the direction.</param>
        /// <param name="turnTime">The amount of time (seconds) the turn should take.</param>
        public void NotifyFaceDirection(IGameObject obj, Vector3 direction, bool isInstant = true, float turnTime = 0.0833f)
        {
            var facePacket = new S2C_FaceDirection()
            {
                SenderNetID = obj.NetId,
                Direction = direction,
                DoLerpTime = !isInstant,
                LerpTime = turnTime
            };

            _packetHandlerManager.BroadcastPacketVision(obj, facePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that (usually) an auto attack missile has been created.
        /// </summary>
        /// <param name="p">Projectile that was created.</param>
        public void NotifyForceCreateMissile(ISpellMissile p)
        {
            var misPacket = new S2C_ForceCreateMissile
            {
                SenderNetID = p.CastInfo.Owner.NetId,
                MissileNetID = p.NetId
            };

            _packetHandlerManager.BroadcastPacket(misPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to, optionally, a specified player, all players with vision of the particle, or all players (given the particle is set as globally visible).
        /// </summary>
        /// <param name="particle">Particle to network.</param>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifyFXCreateGroup(IParticle particle, int userId = 0)
        {
            uint bindNetID = 0;
            uint targetNetID = 0;

            if (particle.BindObject != null)
            {
                bindNetID = particle.BindObject.NetId;
            }
            if (particle.TargetObject != null)
            {
                targetNetID = particle.TargetObject.NetId;
            }

            var position = particle.GetPosition3D();

            var ownerPos = position;
            if (particle.Caster != null)
            {
                ownerPos = particle.Caster.GetPosition3D();
            }

            var fxPacket = new FX_Create_Group();
            var fxDataList = new List<FXCreateData>();

            var targetHeight = _navGrid.GetHeightAtLocation(particle.TargetPosition.X, particle.TargetPosition.Y);
            var higherValue = Math.Max(targetHeight, particle.GetHeight());

            // TODO: implement option for multiple particles instead of hardcoding one
            var fxData1 = new FXCreateData
            {
                NetAssignedNetID = particle.NetId,
                CasterNetID = 0,
                KeywordNetID = 0, // Not sure what this is

                PositionX = (short)((position.X - _navGrid.MapWidth / 2) / 2),
                PositionY = higherValue,
                PositionZ = (short)((position.Z - _navGrid.MapHeight / 2) / 2),

                TargetPositionX = (short)((particle.TargetPosition.X - _navGrid.MapWidth / 2) / 2),
                TargetPositionY = higherValue,
                TargetPositionZ = (short)((particle.TargetPosition.Y - _navGrid.MapHeight / 2) / 2),

                OwnerPositionX = (short)((ownerPos.X - _navGrid.MapWidth / 2) / 2),
                OwnerPositionY = ownerPos.Y,
                OwnerPositionZ = (short)((ownerPos.Z - _navGrid.MapHeight / 2) / 2),

                TimeSpent = particle.GetTimeAlive(),
                ScriptScale = particle.Scale,
                TargetNetID = targetNetID,
                BindNetID = bindNetID
            };

            // TODO: Verify if there is more to this.
            if (particle.FollowsGroundTilt)
            {
                fxData1.TargetPositionY = targetHeight;
            }

            if (particle.Caster != null)
            {
                fxData1.CasterNetID = particle.Caster.NetId;
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
                TargetBoneNameHash = HashString(particle.TargetBoneName),
                BoneNameHash = HashString(particle.BoneName),

                FXCreateData = fxDataList
            };

            if (particle.Caster != null && particle.Caster is IObjAiBase o)
            {
                fxGroupData1.PackageHash = o.GetObjHash();
            }
            else
            {
                fxGroupData1.PackageHash = 0; // TODO: Verify
            }

            fxGroups.Add(fxGroupData1);

            fxPacket.FXCreateGroup = fxGroups;

            if (userId == 0)
            {
                if (particle.SpecificTeam != TeamId.TEAM_NEUTRAL)
                {
                    _packetHandlerManager.BroadcastPacketTeam(particle.SpecificTeam, fxPacket.GetBytes(), Channel.CHL_S2C);
                    return;
                }

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
                _packetHandlerManager.SendPacket(userId, fxPacket.GetBytes(), Channel.CHL_S2C);
            }
        }

        /// <summary>
        /// Sends a packet to the specified team detailing that the specified Particle has become visible.
        /// </summary>
        /// <param name="particle">Particle that came into vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        public void NotifyFXEnterTeamVisibility(IParticle particle, TeamId team)
        {
            var fxVisPacket = new S2C_FX_OnEnterTeamVisibility
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
            var fxVisPacket = new S2C_FX_OnLeaveTeamVisibility
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
            var start = new S2C_StartGame
            {
                EnablePause = true
            };
            _packetHandlerManager.BroadcastPacket(start.GetBytes(), Channel.CHL_S2C);
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
        /// Sends a packet to all players detailing that the specified LaneMinion has spawned.
        /// </summary>
        /// <param name="m">LaneMinion that spawned.</param>
        /// TODO: Implement wave counter.
        public void NotifyLaneMinionSpawned(ILaneMinion m)
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

            _packetHandlerManager.BroadcastPacket(p.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the GameObject which has the specified netId has left vision.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the GameObject that left vision.</param>
        /// TODO: Verify where this should be used.
        public void NotifyLeaveLocalVisibilityClient(int userId, uint netId)
        {
            var leaveLocalVis = new OnLeaveLocalVisibilityClient
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
        /// TODO: Verify where this should be used.
        public void NotifyLeaveLocalVisibilityClient(IGameObject o, TeamId team, int userId = 0)
        {
            var leaveLocalVis = new OnLeaveLocalVisibilityClient
            {
                SenderNetID = o.NetId
            };

            if (userId != 0)
            {
                _packetHandlerManager.SendPacket(userId, leaveLocalVis.GetBytes(), Channel.CHL_S2C);
                return;
            }

            _packetHandlerManager.BroadcastPacketTeam(team, leaveLocalVis.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to either the specified user or team detailing that the specified GameObject has left vision.
        /// </summary>
        /// <param name="o">GameObject that left vision.</param>
        /// <param name="team">TeamId to send the packet to; BLUE/PURPLE/NEUTRAL.</param>
        /// <param name="userId">User to send the packet to (if applicable).</param>
        /// TODO: Verify where this should be used.
        public void NotifyLeaveVisibilityClient(IGameObject o, TeamId team, int userId = 0)
        {
            var leaveVis = new OnLeaveVisibilityClient
            {
                SenderNetID = o.NetId
            };

            if (userId != 0)
            {
                _packetHandlerManager.SendPacket(userId, leaveVis.GetBytes(), Channel.CHL_S2C);
                NotifyLeaveLocalVisibilityClient(userId, o.NetId);
                return;
            }

            _packetHandlerManager.BroadcastPacketTeam(team, leaveVis.GetBytes(), Channel.CHL_S2C);
            NotifyLeaveLocalVisibilityClient(o, team);
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
        public void NotifyMinionSpawned(IMinion minion)
        {
            var spawnPacket = new SpawnMinionS2C
            {
                SenderNetID = minion.NetId,
                NetID = minion.NetId,
                OwnerNetID = minion.NetId,
                NetNodeID = (byte)NetNodeID.Spawned, // TODO: Verify
                Position = minion.GetPosition3D(), // TODO: Verify
                SkinID = 0, // TODO: Unhardcode
                // CloneNetID, clones not yet implemented
                TeamID = (ushort)minion.Team,
                IgnoreCollision = false, // TODO: Unhardcode
                IsWard = minion.IsWard,
                IsLaneMinion = minion.IsLaneMinion,
                IsBot = minion.IsBot,
                IsTargetable = true, // TODO: Unhardcode

                IsTargetableToTeamSpellFlags = (uint)SpellDataFlags.TargetableToAll, // TODO: Unhardcode
                VisibilitySize = minion.VisionRadius, // TODO: Verify
                Name = minion.Name,
                SkinName = minion.Model,
                InitialLevel = 1, // TODO: Unhardcode
                OnlyVisibleToNetID = 0 // TODO: Unhardcode
            };

            if (minion.Owner != null) // Should probably change/optimize at some point
            {
                spawnPacket.OwnerNetID = minion.Owner.NetId;
            }

            _packetHandlerManager.BroadcastPacketVision(minion, spawnPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to either all players with vision (given the projectile is networked to the client) of the projectile, or all players. The packet contains all details regarding the specified projectile's creation.
        /// </summary>
        /// <param name="p">Projectile that was created.</param>
        public void NotifyMissileReplication(ISpellMissile m)
        {
            var castInfo = new CastInfo
            {
                SpellHash = m.CastInfo.SpellHash,
                SpellNetID = m.CastInfo.SpellNetID,

                SpellLevel = m.CastInfo.SpellLevel,
                AttackSpeedModifier = m.CastInfo.AttackSpeedModifier,
                CasterNetID = m.CastInfo.Owner.NetId,
                // TODO: Implement spell chains?
                SpellChainOwnerNetID = m.CastInfo.Owner.NetId,
                PackageHash = m.CastInfo.PackageHash,
                MissileNetID = m.CastInfo.MissileNetID,
                // Not sure if we want to add height for these, but i did it anyway
                TargetPosition = m.CastInfo.TargetPosition,
                TargetPositionEnd = m.CastInfo.TargetPositionEnd,
                DesignerCastTime = m.CastInfo.DesignerCastTime,
                ExtraCastTime = m.CastInfo.ExtraCastTime,
                DesignerTotalTime = m.CastInfo.DesignerTotalTime,

                Cooldown = m.CastInfo.Cooldown,
                StartCastTime = m.CastInfo.StartCastTime,

                IsAutoAttack = m.CastInfo.IsAutoAttack,
                IsSecondAutoAttack = m.CastInfo.IsSecondAutoAttack,
                IsForceCastingOrChannel = m.CastInfo.IsForceCastingOrChannel,
                IsOverrideCastPosition = m.CastInfo.IsOverrideCastPosition,
                IsClickCasted = m.CastInfo.IsClickCasted,

                SpellSlot = m.CastInfo.SpellSlot,
                ManaCost = m.CastInfo.ManaCost,
                SpellCastLaunchPosition = m.CastInfo.SpellCastLaunchPosition,
                AmmoUsed = m.CastInfo.AmmoUsed,
                AmmoRechargeTime = m.CastInfo.AmmoRechargeTime
            };

            if (m.CastInfo.Targets.Count > 0)
            {
                m.CastInfo.Targets.ForEach(t =>
                {
                    if (t.Unit != null)
                    {
                        castInfo.Targets.Add(new CastInfo.Target() { UnitNetID = t.Unit.NetId, HitResult = (byte)t.HitResult });
                    }
                    else
                    {
                        castInfo.Targets.Add(new CastInfo.Target() { UnitNetID = 0, HitResult = (byte)t.HitResult });
                    }
                });
            }

            var misPacket = new MissileReplication
            {
                SenderNetID = m.CastInfo.Owner.NetId,
                Position = m.GetPosition3D(),
                CasterPosition = m.CastInfo.Owner.GetPosition3D(),
                // Not sure if we want to add height for these, but i did it anyway
                Direction = m.Direction,
                Velocity = m.Direction * m.GetSpeed(),
                StartPoint = m.CastInfo.SpellCastLaunchPosition,
                EndPoint = m.CastInfo.TargetPositionEnd,
                // TODO: Verify
                UnitPosition = m.CastInfo.Owner.GetPosition3D(),
                TimeFromCreation = m.GetTimeSinceCreation(), // TODO: Unhardcode
                Speed = m.GetSpeed(),
                LifePercentage = 0f, // TODO: Unhardcode
                //TODO: Implement time limited projectiles
                TimedSpeedDelta = 0f, // TODO: Implement time limited projectiles for this
                TimedSpeedDeltaTime = 0x7F7FFFFF, // Same as above (this value is from the SpawnProjectile packet, it is a placeholder)

                Bounced = false, //TODO: Implement bouncing projectiles

                CastInfo = castInfo
            };

            if (m is ISpellChainMissile chainMissile && chainMissile.ObjectsHit.Count > 0)
            {
                misPacket.Bounced = true;
            }

            if (!m.IsServerOnly)
            {
                _packetHandlerManager.BroadcastPacketVision(m, misPacket.GetBytes(), Channel.CHL_S2C);
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
        /// Sends a packet to all players detailing the movement driver homing data for the given unit.
        /// Used to sync homing (target-based) dashes between client and server.
        /// </summary>
        /// <param name="unit">Unit to sync.</param>
        public void NotifyMovementDriverReplication(IObjAiBase unit)
        {
            var targetPos = unit.TargetUnit.Position;
            if (targetPos == new Vector2(float.NaN, float.NaN))
            {
                return;
            }

            var current = unit.GetPosition3D();
            var to = Vector3.Normalize(new Vector3(targetPos.X, _navGrid.GetHeightAtLocation(targetPos.X, targetPos.Y), targetPos.Y) - current);

            var hd = new MovementDriverHomingData
            {
                TargetNetID = unit.TargetUnit.NetId,
                TargetHeightModifier = 0, // TODO: Verify
                TargetPosition = unit.TargetUnit.GetPosition3D(),
                Speed = unit.MovementParameters.PathSpeedOverride,
                Gravity = 0, // TODO: Implement gravity for AttackableUnits.
                RateOfTurn = 1.0f, // TODO: Implement TurnRate.
                Duration = unit.MovementParameters.FollowTravelTime,
                MovementPropertyFlags = 0 // TODO: Implement MovementPropertyFlags.
            };

            var replication = new MovementDriverReplication
            {
                SenderNetID = unit.NetId,
                MovementTypeID = 0, // TODO: Find out these values and place in GameServerCore.Enums.MovementTypeID
                Position = unit.GetPosition3D(),
                Velocity = new Vector3(to.X * unit.MovementParameters.PathSpeedOverride, 0, to.Y * unit.MovementParameters.PathSpeedOverride),
                MovementDriverHomingData = hd
            };

            // Homing projectiles are visible regardless of vision.
            _packetHandlerManager.BroadcastPacket(replication.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players who have vision of the specified buff's target detailing that the buff has been added to the target.
        /// </summary>
        /// <param name="b">Buff being added.</param>
        public void NotifyNPC_BuffAdd2(IBuff b, float duration, float runningTime)
        {
            var addPacket = new NPC_BuffAdd2
            {
                SenderNetID = b.TargetUnit.NetId,
                BuffSlot = b.Slot,
                BuffType = (byte)b.BuffType,
                Count = (byte)b.StackCount,
                IsHidden = b.IsHidden,
                BuffNameHash = HashFunctions.HashString(b.Name),
                PackageHash = b.SourceUnit.GetObjHash(), // TODO: Verify
                RunningTime = runningTime,
                Duration = duration,
                CasterNetID = b.SourceUnit.NetId
            };
            _packetHandlerManager.BroadcastPacketVision(b.TargetUnit, addPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified ObjAiBase detailing that the specified group of buffs has been added to the ObjAiBase.
        /// </summary>
        /// <param name="target">Unit that is receiving the group of buffs.</param>
        /// <param name="buffs">Group of buffs being added to the target.</param>
        /// <param name="buffType">Type of buff that applies to the entire group of buffs.</param>
        /// <param name="buffName">Internal name of the buff that applies to the group of buffs.</param>
        /// <param name="runningTime">Time that has passed since the group of buffs was created.</param>
        /// <param name="duration">Total amount of time the group of buffs should be active.</param>
        public void NotifyNPC_BuffAddGroup(IAttackableUnit target, List<IBuff> buffs, BuffType buffType, string buffName, float runningTime, float duration)
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
                    OwnerNetID = buffs[i].TargetUnit.NetId,
                    CasterNetID = buffs[i].OriginSpell.CastInfo.Owner.NetId,
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
                SenderNetID = b.TargetUnit.NetId, //TODO: Verify if this should change depending on the removal source
                BuffSlot = b.Slot,
                BuffNameHash = HashFunctions.HashString(b.Name),
                RunTimeRemove = b.Duration - b.TimeElapsed
            };
            _packetHandlerManager.BroadcastPacket(removePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified ObjAiBase detailing that the specified group of buffs is being removed from the ObjAiBase.
        /// </summary>
        /// <param name="target">Unit getting their group of buffs removed.</param>
        /// <param name="buffs">Group of buffs getting removed.</param>
        /// <param name="buffName">Internal name of the buff that is applicable to the entire group of buffs.</param>
        public void NotifyNPC_BuffRemoveGroup(IAttackableUnit target, List<IBuff> buffs, string buffName)
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
                    OwnerNetID = buffs[i].TargetUnit.NetId,
                    Slot = buffs[i].Slot,
                    RunTimeRemove = buffs[i].Duration - buffs[i].TimeElapsed
                };
                entries.Add(entry);
            }
            removeGroupPacket.Entries = entries;

            _packetHandlerManager.BroadcastPacket(removeGroupPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the target of the specified buff detailing that the buff previously in the same slot was replaced by the newly specified buff.
        /// </summary>
        /// <param name="b">Buff that will replace the old buff in the same slot.</param>
        public void NotifyNPC_BuffReplace(IBuff b)
        {
            var replacePacket = new NPC_BuffReplace
            {
                SenderNetID = b.TargetUnit.NetId,
                BuffSlot = b.Slot,
                RunningTime = b.TimeElapsed,
                Duration = b.Duration,
                CasterNetID = b.OriginSpell.CastInfo.Owner.NetId
            };
            _packetHandlerManager.BroadcastPacketVision(b.TargetUnit, replacePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that the buffs already occupying the slots of the group of buffs were replaced by the newly specified group of buffs.
        /// </summary>
        /// <param name="target">Unit getting their group of buffs replaced.</param>
        /// <param name="buffs">Group of buffs replacing buffs in the same slots.</param>
        /// <param name="runningtime">Time since the group of buffs was created.</param>
        /// <param name="duration">Total time the group of buffs should be active.</param>
        public void NotifyNPC_BuffReplaceGroup(IAttackableUnit target, List<IBuff> buffs, float runningtime, float duration)
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
                    OwnerNetID = buffs[i].TargetUnit.NetId,
                    CasterNetID = buffs[i].OriginSpell.CastInfo.Owner.NetId,
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
                SenderNetID = b.TargetUnit.NetId,
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
        /// <param name="target">Unit who's buffs will be updated.</param>
        /// <param name="buffs">Group of buffs to update.</param>
        /// <param name="duration">Total time the buff should last.</param>
        /// <param name="runningTime">Time since the buff's creation.</param>
        public void NotifyNPC_BuffUpdateCountGroup(IAttackableUnit target, List<IBuff> buffs, float duration, float runningTime)
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
                    OwnerNetID = buffs[i].TargetUnit.NetId,
                    CasterNetID = buffs[i].OriginSpell.CastInfo.Owner.NetId,
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
                SenderNetID = b.TargetUnit.NetId,
                BuffSlot = b.Slot,
                Counter = b.StackCount // TODO: Verify if it allows stacks to go above 255 on the buff bar
            };
            _packetHandlerManager.BroadcastPacketVision(b.TargetUnit, updateNumPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the owner of the specified spell detailing that a spell has been cast.
        /// </summary>
        /// <param name="s">Spell being cast.</param>
        public void NotifyNPC_CastSpellAns(ISpell s)
        {
            var castInfo = new CastInfo
            {
                SpellHash = (uint)s.GetId(),
                SpellNetID = s.CastInfo.SpellNetID,
                SpellLevel = s.CastInfo.SpellLevel,
                AttackSpeedModifier = s.CastInfo.AttackSpeedModifier,
                CasterNetID = s.CastInfo.Owner.NetId,
                SpellChainOwnerNetID = s.CastInfo.Owner.NetId,
                PackageHash = s.CastInfo.Owner.GetObjHash(),
                MissileNetID = s.CastInfo.MissileNetID,
                TargetPosition = s.CastInfo.TargetPosition,
                TargetPositionEnd = s.CastInfo.TargetPositionEnd,
                DesignerCastTime = s.CastInfo.DesignerCastTime,
                ExtraCastTime = s.CastInfo.ExtraCastTime,
                DesignerTotalTime = s.CastInfo.DesignerTotalTime,
                Cooldown = s.CastInfo.Cooldown,
                StartCastTime = s.CastInfo.StartCastTime,
                IsAutoAttack = s.CastInfo.IsAutoAttack,
                IsSecondAutoAttack = s.CastInfo.IsSecondAutoAttack,
                IsForceCastingOrChannel = s.CastInfo.IsForceCastingOrChannel,
                IsOverrideCastPosition = s.CastInfo.IsOverrideCastPosition,
                IsClickCasted = s.CastInfo.IsClickCasted,
                SpellSlot = s.CastInfo.SpellSlot,
                SpellCastLaunchPosition = s.CastInfo.SpellCastLaunchPosition,
                AmmoUsed = s.CastInfo.AmmoUsed,
                AmmoRechargeTime = s.CastInfo.AmmoRechargeTime
            };

            if (s.CastInfo.Targets.Count > 0)
            {
                s.CastInfo.Targets.ForEach(t =>
                {
                    if (t.Unit != null)
                    {
                        castInfo.Targets.Add(new CastInfo.Target() { UnitNetID = t.Unit.NetId, HitResult = (byte)t.HitResult });
                    }
                    else
                    {
                        castInfo.Targets.Add(new CastInfo.Target() { UnitNetID = 0, HitResult = (byte)t.HitResult });
                    }
                });
            }

            if (s.CastInfo.SpellLevel > 0)
            {
                castInfo.ManaCost = s.SpellData.ManaCost[s.CastInfo.SpellLevel];
            }
            else
            {
                castInfo.ManaCost = s.SpellData.ManaCost[0];
            }

            var castAnsPacket = new NPC_CastSpellAns
            {
                SenderNetID = s.CastInfo.Owner.NetId,
                CasterPositionSyncID = s.CastInfo.Owner.SyncId,
                // TODO: Find what this is (if false, causes CasterPositionSyncID to be used)
                Unknown1 = false,
                CastInfo = castInfo
            };
            _packetHandlerManager.BroadcastPacketVision(s.CastInfo.Owner, castAnsPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified Champion has leveled up.
        /// </summary>
        /// <param name="c">Champion which leveled up.</param>
        public void NotifyNPC_LevelUp(IChampion c)
        {
            var levelUp = new NPC_LevelUp()
            {
                SenderNetID = c.NetId,
                Level = c.Stats.Level,
                // TODO: Typo :(
                AveliablePoints = c.SkillPoints
            };

            _packetHandlerManager.BroadcastPacketVision(c, levelUp.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that they have leveled up the specified skill.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the GameObject whos skill is being leveled up.</param>
        /// <param name="slot">Slot of the skill being leveled up.</param>
        /// <param name="level">Current level of the skill.</param>
        /// <param name="points">Number of skill points available after the skill has been leveled up.</param>
        public void NotifyNPC_UpgradeSpellAns(int userId, uint netId, byte slot, byte level, byte points)
        {
            var upgradeSpellPacket = new NPC_UpgradeSpellAns
            {
                SenderNetID = netId,
                Slot = slot,
                SpellLevel = level,
                SkillPoints = points
            };

            _packetHandlerManager.SendPacket(userId, upgradeSpellPacket.GetBytes(), Channel.CHL_GAMEPLAY);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified AttackableUnit detailing that the attacker has abrubtly stopped their attack (can be a spell or auto attack, although internally AAs are also spells).
        /// </summary>
        /// <param name="attacker">AttackableUnit that stopped their auto attack.</param>
        /// <param name="isSummonerSpell">Whether or not the spell is a summoner spell.</param>
        /// <param name="keepAnimating">Whether or not to continue the auto attack animation after the abrupt stop.</param>
        /// <param name="destroyMissile">Whether or not to destroy the missile which may have been created before stopping (client-side removal).</param>
        /// <param name="overrideVisibility">Whether or not stopping this auto attack overrides visibility checks.</param>
        /// <param name="forceClient">Whether or not this packet should be forcibly applied, regardless of if an auto attack is being performed client-side.</param>
        /// <param name="missileNetID">NetId of the missile that may have been spawned by the spell.</param>
        /// TODO: Find a better way to implement these parameters
        public void NotifyNPC_InstantStop_Attack(IAttackableUnit attacker, bool isSummonerSpell,
            bool keepAnimating = false,
            bool destroyMissile = true,
            bool overrideVisibility = true,
            bool forceClient = false,
            uint missileNetID = 0)
        {
            var stopAttack = new NPC_InstantStop_Attack
            {
                SenderNetID = attacker.NetId,
                MissileNetID = missileNetID, //TODO: Fix MissileNetID, currently it only works when it is 0
                KeepAnimating = keepAnimating,
                DestroyMissile = destroyMissile,
                OverrideVisibility = overrideVisibility,
                IsSummonerSpell = isSummonerSpell,
                ForceDoClient = forceClient
            };
            _packetHandlerManager.BroadcastPacketVision(attacker, stopAttack.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified AttackableUnit die has died to the specified AttackableUnit killer.
        /// </summary>
        /// <param name="unit">AttackableUnit that was killed.</param>
        /// <param name="killer">AttackableUnit that killed the unit.</param>
        public void NotifyNpcDie(IAttackableUnit unit, IAttackableUnit killer)
        {
            var nd = new NpcDie(unit, killer);
            _packetHandlerManager.BroadcastPacket(nd, Channel.CHL_S2C);
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
        /// Sends a packet to all players with vision of the specified AI detailing that item in the specified slot was removed (or the number of stacks of the item in that slot changed).
        /// </summary>
        /// <param name="ai">AI with the items.</param>
        /// <param name="slot">Slot of the item that was removed.</param>
        /// <param name="remaining">Number of stacks of the item left (0 if not applicable).</param>
        public void NotifyRemoveItem(IObjAiBase ai, byte slot, byte remaining)
        {
            var ria = new RemoveItemAns()
            {
                SenderNetID = ai.NetId,
                Slot = slot,
                ItemsInSlot = remaining,
                NotifyInventoryChange = true
            };
            _packetHandlerManager.BroadcastPacketVision(ai, ria.GetBytes(), Channel.CHL_S2C);
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
        /// Sends a packet to all players with vision of the given chain missile that it has updated (unit/position).
        /// </summary>
        /// <param name="p">Missile that should be synced.</param>
        public void NotifyS2C_ChainMissileSync(ISpellMissile m)
        {
            if (!m.HasTarget())
            {
                return;
            }

            var syncPacket = new S2C_ChainMissileSync()
            {
                SenderNetID = m.NetId,
                TargetCount = m.CastInfo.Targets.Count,
                // TODO: Verify
                OwnerNetworkID = m.CastInfo.Owner.NetId
            };

            for (int i = 0; i < syncPacket.TargetNetIDs.Length; i++)
            {
                if (m.CastInfo.Targets.Count == i)
                {
                    break;
                }

                syncPacket.TargetNetIDs[i] = m.CastInfo.Targets[i].Unit.NetId;
            }

            _packetHandlerManager.BroadcastPacketVision(m, syncPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the given projectile that it has changed targets (unit/position).
        /// </summary>
        /// <param name="p">Projectile that has changed target.</param>
        public void NotifyS2C_ChangeMissileTarget(ISpellMissile p)
        {
            if (!p.HasTarget())
            {
                return;
            }

            var changePacket = new S2C_ChangeMissileTarget()
            {
                SenderNetID = p.NetId,
                TargetNetID = 0,
                TargetPosition = new Vector3(p.GetTargetPosition().X, _navGrid.GetHeightAtLocation(p.GetTargetPosition()), p.GetTargetPosition().Y)
            };

            if (p.CastInfo.Targets.Count > 0)
            {
                if (p.CastInfo.Targets[0].Unit != null)
                {
                    changePacket.TargetNetID = p.CastInfo.Targets[0].Unit.NetId;
                }
            }

            _packetHandlerManager.BroadcastPacketVision(p, changePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user detailing that the hero designated to the given clientInfo has been created.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="clientInfo">Information about the client which had their hero created.</param>
        public void NotifyS2C_CreateHero(int userId, ClientInfo clientInfo)
        {
            var champion = clientInfo.Champion;
            var heroPacket = new S2C_CreateHero()
            {
                NetID = champion.NetId,
                ClientID = (int)clientInfo.ClientId,
                // NetNodeID,
                // For bots (0 = Beginner, 1 = Intermediate)
                SkillLevel = 0,
                // TODO: Implement bots and unhardcode this.
                IsBot = false,
                // BotRank, deprecated as of v4.18
                // TODO: Unhardcode
                SpawnPositionIndex = 0,
                SkinID = champion.Skin,
                Name = clientInfo.Name,
                Skin = champion.Model,
                DeathDurationRemaining = champion.RespawnTimer,
                // TimeSinceDeath
            };

            if (champion.Team == TeamId.TEAM_BLUE)
            {
                heroPacket.TeamIsOrder = true;
            }
            else
            {
                heroPacket.TeamIsOrder = false;
            }

            _packetHandlerManager.SendPacket(userId, heroPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified object detailing that it is playing the specified animation.
        /// </summary>
        /// <param name="obj">GameObject that is playing the animation.</param>
        /// <param name="animation">Internal name of the animation to play.</param>
        /// TODO: Implement AnimationFlags enum for this and fill it in.
        /// <param name="flags">Animation flags. Refer to AnimationFlags enum.</param>
        /// <param name="timeScale">How fast the animation should play. Default 1x speed.</param>
        /// <param name="startTime">Time in the animation to start at.</param>
        /// TODO: Verify if this description is correct, if not, correct it.
        /// <param name="speedScale">How much the speed of the GameObject should affect the animation.</param>
        public void NotifyS2C_PlayAnimation(IGameObject obj, string animation, AnimationFlags flags = 0, float timeScale = 1.0f, float startTime = 0.0f, float speedScale = 1.0f)
        {
            var animPacket = new S2C_PlayAnimation
            {
                SenderNetID = obj.NetId,
                AnimationFlags = (byte)flags,
                ScaleTime = timeScale,
                StartProgress = startTime,
                SpeedRatio = speedScale,
                AnimationName = animation
            };

            _packetHandlerManager.BroadcastPacketVision(obj, animPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing that the specified object's current animations have been paused/unpaused.
        /// </summary>
        /// <param name="obj">GameObject that is playing the animation.</param>
        /// <param name="pause">Whether or not to pause/unpause animations.</param>
        public void NotifyS2C_PauseAnimation(IGameObject obj, bool pause)
        {
            var animPacket = new S2C_PauseAnimation
            {
                SenderNetID = obj.NetId,
                Pause = pause
            };

            _packetHandlerManager.BroadcastPacket(animPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified unit detailing that its animation states have changed to the specified animation pairs.
        /// Replaces the unit's normal animation behaviors with the given animation pairs. Structure of the animationPairs is expected to follow the same structure from before the replacement.
        /// </summary>
        /// <param name="u">AttackableUnit to change.</param>
        /// <param name="animationPairs">Dictionary of animations to set.</param>
        public void NotifyS2C_SetAnimStates(IAttackableUnit u, Dictionary<string, string> animationPairs)
        {
            var setAnimPacket = new S2C_SetAnimStates
            {
                SenderNetID = u.NetId,
                AnimationOverrides = animationPairs
            };

            _packetHandlerManager.BroadcastPacket(setAnimPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyS2C_SetInputLockFlag(int userId, InputLockFlags flags, bool enabled)
        {
            var inputLockPacket = new S2C_SetInputLockFlag
            {
                InputLockFlags = (uint)flags,
                Value = enabled
            };

            _packetHandlerManager.SendPacket(userId, inputLockPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user detailing that the spell in the given slot has had its spelldata changed to the spelldata of the given spell name.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the unit that owns the spell being changed.</param>
        /// <param name="spellName">Internal name of the spell to grab spell data from (to set).</param>
        /// <param name="slot">Slot of the spell being changed.</param>
        public void NotifyS2C_SetSpellData(int userId, uint netId, string spellName, byte slot)
        {
            var spellDataPacket = new S2C_SetSpellData
            {
                SenderNetID = netId,
                ObjectNetID = netId,
                HashedSpellName = HashString(spellName),
                SpellSlot = slot
            };

            _packetHandlerManager.SendPacket(userId, spellDataPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the level of the spell in the given slot has changed.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="netId">NetId of the unit that owns the spell being changed.</param>
        /// <param name="slot">Slot of the spell being changed.</param>
        /// <param name="level">New level of the spell to set.</param>
        public void NotifyS2C_SetSpellLevel(int userId, uint netId, int slot, int level)
        {
            var spellLevelPacket = new S2C_SetSpellLevel
            {
                SenderNetID = netId,
                SpellSlot = slot,
                SpellLevel = level
            };

            _packetHandlerManager.SendPacket(userId, spellLevelPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the game has started the spawning GameObjects that occurs at the start of the game.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifyS2C_StartSpawn(int userId)
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
        /// Sends a packet to all players detailing that the specified object has stopped playing an animation.
        /// </summary>
        /// <param name="obj">GameObject that is playing the animation.</param>
        /// <param name="animation">Internal name of the animation to stop.</param>
        /// <param name="stopAll">Whether or not to stop all animations. Only works if animation is empty/null.</param>
        /// <param name="fade">Whether or not the animation should fade before stopping.</param>
        /// <param name="ignoreLock">Whether or not locked animations should still be stopped.</param>
        public void NotifyS2C_StopAnimation(IGameObject obj, string animation, bool stopAll = false, bool fade = false, bool ignoreLock = true)
        {
            var animPacket = new S2C_StopAnimation
            {
                SenderNetID = obj.NetId,
                Fade = fade,
                IgnoreLock = ignoreLock,
                StopAll = stopAll,
                AnimationName = animation
            };

            _packetHandlerManager.BroadcastPacket(animPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the given user detailing that the specified input locking flags have been toggled.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="flags">InputLockFlags to toggle.</param>
        public void NotifyS2C_ToggleInputLockFlag(int userId, InputLockFlags flags)
        {
            var inputLockPacket = new S2C_ToggleInputLockFlag
            {
                InputLockFlags = (uint)flags
            };

            _packetHandlerManager.SendPacket(userId, inputLockPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the specified attacker that it is looking at (targeting) the specified attacked unit with the given AttackType.
        /// </summary>
        /// <param name="attacker">Unit that is attacking.</param>
        /// <param name="attacked">Unit that is being attacked.</param>
        /// <param name="attackType">AttackType that the attacker is using to attack.</param>
        public void NotifyS2C_UnitSetLookAt(IAttackableUnit attacker, IAttackableUnit attacked, AttackType attackType)
        {
            var lookAtPacket = new S2C_UnitSetLookAt
            {
                SenderNetID = attacker.NetId,
                LookAtType = (byte)attackType,
                TargetPosition = attacked.GetPosition3D(),
                TargetNetID = attacked.NetId
            };

            _packetHandlerManager.BroadcastPacketVision(attacker, lookAtPacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing the attack speed cap overrides for this game.
        /// </summary>
        /// <param name="overrideMax">Whether or not to override the maximum attack speed cap.</param>
        /// <param name="maxAttackSpeedOverride">Value to override the maximum attack speed cap.</param>
        /// <param name="overrideMin">Whether or not to override the minimum attack speed cap.</param>
        /// <param name="minAttackSpeedOverride">Value to override the minimum attack speed cap.</param>
        public void NotifyS2C_UpdateAttackSpeedCapOverrides(bool overrideMax, float maxAttackSpeedOverride, bool overrideMin, float minAttackSpeedOverride)
        {
            var overridePacket = new S2C_UpdateAttackSpeedCapOverrides
            {
                DoOverrideMax = overrideMax,
                DoOverrideMin = overrideMin,
                MaxAttackSpeedOverride = maxAttackSpeedOverride,
                MinAttackSpeedOverride = minAttackSpeedOverride
            };

            _packetHandlerManager.BroadcastPacket(overridePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the given bounce missile that it has updated (unit/position).
        /// </summary>
        /// <param name="p">Missile that has been updated.</param>
        public void NotifyS2C_UpdateBounceMissile(ISpellMissile p)
        {
            if (!p.HasTarget())
            {
                return;
            }

            var changePacket = new S2C_UpdateBounceMissile()
            {
                SenderNetID = p.NetId,
                TargetNetID = 0,
                CasterPosition = p.CastInfo.SpellCastLaunchPosition
            };

            if (p.CastInfo.Targets.Count > 0)
            {
                if (p.CastInfo.Targets[0].Unit != null)
                {
                    changePacket.TargetNetID = p.CastInfo.Targets[0].Unit.NetId;
                }
            }

            _packetHandlerManager.BroadcastPacketVision(p, changePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified user detailing that the specified spell's toggle state has been updated.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="s">Spell being updated.</param>
        public void NotifyS2C_UpdateSpellToggle(int userId, ISpell s)
        {
            var spellTogglePacket = new S2C_UpdateSpellToggle
            {
                SenderNetID = s.CastInfo.Owner.NetId,
                SpellSlot = s.CastInfo.SpellSlot,
                ToggleValue = s.Toggle
            };

            _packetHandlerManager.SendPacket(userId, spellTogglePacket.GetBytes(), Channel.CHL_S2C);
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
        /// Calls for the appropriate spawn packet to be sent given the specified GameObject's type and calls for a vision packet to be sent for the specified GameObject.
        /// </summary>
        /// <param name="o">GameObject that has spawned.</param>
        public void NotifySpawn(IGameObject o)
        {
            switch (o)
            {
                case ILaneMinion m:
                    NotifyLaneMinionSpawned(m);
                    break;
                case IChampion c:
                    NotifyEnterVisibilityClient(c, isChampion: true);
                    return;
                case IMonster monster:
                    NotifyMonsterSpawned(monster);
                    break;
                case IMinion minion:
                    NotifyMinionSpawned(minion);
                    break;
                case IAzirTurret azirTurret:
                    NotifyAzirTurretSpawned(azirTurret);
                    break;
            }

            NotifyEnterVisibilityClient(o);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the spawning (of champions & buildings) that occurs at the start of the game has ended.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        public void NotifySpawnEnd(int userId)
        {
            var endSpawnPacket = new S2C_EndSpawn();
            _packetHandlerManager.SendPacket(userId, endSpawnPacket.GetBytes(), Channel.CHL_S2C);
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
        /// Sends a packet to the specified player detailing the amount of time since the game started (in seconds). Used to initialize the user's in-game timer.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="time">Time since the game started (in milliseconds).</param>
        public void NotifySyncMissionStartTimeS2C(int userId, float time)
        {
            var sync = new SyncMissionStartTimeS2C()
            {
                StartTime = time / 1000.0f
            };

            _packetHandlerManager.SendPacket(userId, sync.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players detailing the amount of time since the game started (in seconds).
        /// </summary>
        /// <param name="gameTime">Time since the game started (in milliseconds).</param>
        public void NotifySynchSimTimeS2C(float gameTime)
        {
            var sync = new SynchSimTimeS2C()
            {
                SynchTime = gameTime / 1000.0f
            };

            _packetHandlerManager.BroadcastPacket(sync.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing the amount of time since the game started (in seconds).
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="time">Time since the game started (in milliseconds).</param>
        public void NotifySynchSimTimeS2C(int userId, float time)
        {
            var sync = new SynchSimTimeS2C()
            {
                SynchTime = time / 1000.0f
            };

            _packetHandlerManager.SendPacket(userId, sync.GetBytes(), Channel.CHL_S2C);
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
        /// Sends a packet to all players with vision of the given unit detailing that it has teleported to the given position.
        /// </summary>
        /// <param name="unit">Unit that has teleported.</param>
        /// <param name="position">Position the unit teleported to.</param>
        public void NotifyTeleport(IAttackableUnit unit, Vector2 position)
        {
            var md = new MovementDataNormal()
            {
                SyncID = unit.SyncId,
                TeleportNetID = unit.NetId,
                HasTeleportID = true,
                TeleportID = unit.TeleportID,
                Waypoints = new List<CompressedWaypoint> { PacketExtensions.Vector2ToWaypoint(PacketExtensions.TranslateToCenteredCoordinates(position, _navGrid)) },
            };

            var wpGroup = new WaypointGroup()
            {
                SyncID = unit.SyncId,
                Movements = new List<MovementDataNormal> { md }
            };

            _packetHandlerManager.BroadcastPacketVision(unit, wpGroup.GetBytes(), Channel.CHL_LOW_PRIORITY);
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
        public void NotifyUnitApplyDamage(IAttackableUnit source, IAttackableUnit target, float amount, DamageType type, DamageResultType damagetext, bool isGlobal = true, int sourceId = 0, int targetId = 0)
        {
            var damagePacket = new UnitApplyDamage
            {
                SenderNetID = source.NetId,
                DamageResultType = (byte)damagetext,
                DamageType = (byte)type,
                TargetNetID = target.NetId,
                SourceNetID = source.NetId,
                Damage = amount
            };

            _packetHandlerManager.BroadcastPacket(damagePacket.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to the specified player detailing that the specified target GameObject's (debug?) path drawing mode has been set to the specified mode.
        /// </summary>
        /// <param name="userId">User to send the packet to(?).</param>
        /// <param name="unit">Unit that has called for the packet.</param>
        /// <param name="target">GameObject who's (debug?) draw path mode is being set.</param>
        /// <param name="mode">Draw path mode to set. Refer to DrawPathMode enum.</param>
        /// TODO: Verify the functionality of this packet (and its parameters) and create an enum for the mode.
        public void NotifyUnitSetDrawPathMode(int userId, IAttackableUnit unit, IGameObject target, DrawPathMode mode)
        {
            var drawPacket = new S2C_UnitSetDrawPathMode
            {
                SenderNetID = unit.NetId,
                TargetNetID = target.NetId,
                DrawPathMode = (byte)mode,
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
        /// Sends a packet to the player attempting to use an item that the item was used successfully.
        /// </summary>
        /// <param name="userId">User to send the packet to.</param>
        /// <param name="ai">GameObject of type ObjAiBase that can buy items.</param>
        /// <param name="itemInstance">Item instance housing all information about the item that has been used.</param>
        public void NotifyUseItemAns(int userId, IObjAiBase ai, IItem itemInstance)
        {
            var useItemPacket = new UseItemAns
            {
                SenderNetID = ai.NetId,
                Slot = ai.Inventory.GetItemSlot(itemInstance),
                SpellCharges = (byte)itemInstance.StackCount // TODO: Unhardcode
            };

            _packetHandlerManager.SendPacket(userId, useItemPacket.GetBytes(), Channel.CHL_S2C);
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

        /// <summary>
        /// Sends a packet to all players that have vision of the specified unit that it has made a movement.
        /// </summary>
        /// <param name="u">AttackableUnit that is moving.</param>
        /// <param name="useTeleportID">Whether or not to teleport the unit to its current position in its path.</param>
        public void NotifyWaypointGroup(IAttackableUnit u, bool useTeleportID = true)
        {
            // TODO: Verify if casts correctly
            var move = (MovementDataNormal)PacketExtensions.CreateMovementData(u, _navGrid, MovementDataType.Normal, useTeleportID: useTeleportID);

            // TODO: Implement support for multiple movements.
            var packet = new WaypointGroup
            {
                SenderNetID = u.NetId,
                SyncID = u.SyncId,
                Movements = new List<MovementDataNormal>() { move }
            };

            _packetHandlerManager.BroadcastPacketVision(u, packet.GetBytes(), Channel.CHL_LOW_PRIORITY);
        }

        /// <summary>
        /// Sends a packet to all players that have vision of the specified unit.
        /// The packet details a group of waypoints with speed parameters which determine what kind of movement will be done to reach the waypoints, or optionally a GameObject.
        /// Functionally referred to as a dash in-game.
        /// </summary>
        /// <param name="u">Unit that is dashing.</param>
        /// TODO: Implement ForceMovement class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        public void NotifyWaypointGroupWithSpeed
        (
            IAttackableUnit u
        )
        {
            // TODO: Implement Dash class and house a List of these with waypoints.
            var speeds = new SpeedParams
            {
                PathSpeedOverride = u.MovementParameters.PathSpeedOverride,
                ParabolicGravity = u.MovementParameters.ParabolicGravity,
                // TODO: Implement as parameter (ex: Aatrox Q).
                ParabolicStartPoint = u.MovementParameters.ParabolicStartPoint,
                Facing = u.MovementParameters.KeepFacingDirection,
                FollowNetID = u.MovementParameters.FollowNetID,
                FollowDistance = u.MovementParameters.FollowDistance,
                FollowBackDistance = u.MovementParameters.FollowBackDistance,
                FollowTravelTime = u.MovementParameters.FollowTravelTime
            };

            // TODO: Verify if cast works.
            var md = (MovementDataWithSpeed)PacketExtensions.CreateMovementData(u, _navGrid, MovementDataType.WithSpeed, speeds);

            var speedWpGroup = new WaypointGroupWithSpeed
            {
                SenderNetID = 0,
                SyncID = u.SyncId,
                // TOOD: Implement support for multiple speed-based movements (functionally known as dashes).
                Movements = new List<MovementDataWithSpeed> { md }
            };

            _packetHandlerManager.BroadcastPacketVision(u, speedWpGroup.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the given unit detailing its waypoints.
        /// </summary>
        /// <param name="unit">Unit to send.</param>
        public void NotifyWaypointList(IAttackableUnit unit)
        {
            var wpList = new WaypointList
            {
                SenderNetID = unit.NetId,
                SyncID = unit.SyncId,
                Waypoints = unit.Waypoints
            };

            _packetHandlerManager.BroadcastPacketVision(unit, wpList.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players with vision of the given GameObject detailing its waypoints.
        /// </summary>
        /// <param name="obj">GameObject to send.</param>
        public void NotifyWaypointList(IGameObject obj, List<Vector2> waypoints)
        {
            var wpList = new WaypointList
            {
                SenderNetID = obj.NetId,
                SyncID = obj.SyncId,
                Waypoints = waypoints
            };

            _packetHandlerManager.BroadcastPacketVision(obj, wpList.GetBytes(), Channel.CHL_S2C);
        }

        /// <summary>
        /// Sends a packet to all players that have vision of the specified unit.
        /// The packet details a list of waypoints with speed parameters which determine what kind of movement will be done to reach the waypoints, or optionally a GameObject.
        /// Functionally referred to as a dash in-game.
        /// </summary>
        /// <param name="u">Unit that is dashing.</param>
        /// <param name="dashSpeed">Constant speed that the unit will have during the dash.</param>
        /// <param name="leapGravity">Optionally how much gravity the unit will experience when above the ground while dashing.</param>
        /// <param name="keepFacingLastDirection">Optionally whether or not the unit should maintain the direction they were facing before dashing.</param>
        /// <param name="target">Optional GameObject to follow.</param>
        /// <param name="followTargetMaxDistance">Optional maximum distance the unit will follow the Target before stopping the dash or reaching to the Target.</param>
        /// <param name="backDistance">Optional unknown parameter.</param>
        /// <param name="travelTime">Optional total time the dash will follow the GameObject before stopping or reaching the Target.</param>
        /// TODO: Implement ForceMovement class which houses these parameters, then have that as the only parameter to this function (and other Dash-based functions).
        public void NotifyWaypointListWithSpeed
        (
            IAttackableUnit u,
            float dashSpeed,
            float leapGravity = 0,
            bool keepFacingLastDirection = false,
            IGameObject target = null,
            float followTargetMaxDistance = 0,
            float backDistance = 0,
            float travelTime = 0
        )
        {
            // TODO: Implement ForceMovement class/interface and house a List of these with waypoints.
            var speeds = new SpeedParams
            {
                PathSpeedOverride = dashSpeed,
                ParabolicGravity = leapGravity,
                // TODO: Implement as parameter (ex: Aatrox Q).
                ParabolicStartPoint = u.Position,
                Facing = keepFacingLastDirection,
                FollowNetID = 0,
                FollowDistance = followTargetMaxDistance,
                FollowBackDistance = backDistance,
                FollowTravelTime = travelTime
            };

            if (target != null)
            {
                speeds.FollowNetID = target.NetId;
            }

            var speedWpGroup = new WaypointListHeroWithSpeed
            {
                SenderNetID = u.NetId,
                SyncID = u.SyncId,
                // TOOD: Implement support for multiple speed-based movements (functionally known as dashes).
                WaypointSpeedParams = speeds,
                Waypoints = u.Waypoints
            };

            _packetHandlerManager.BroadcastPacketVision(u, speedWpGroup.GetBytes(), Channel.CHL_S2C);
        }
    }
}

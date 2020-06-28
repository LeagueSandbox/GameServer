using ENet;
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
    public class PacketNotifier : IPacketNotifier
    {
        private readonly IPacketHandlerManager _packetHandlerManager;
        private readonly INavigationGrid _navGrid;

        public PacketNotifier(IPacketHandlerManager packetHandlerManager, INavigationGrid navGrid)
        {
            _packetHandlerManager = packetHandlerManager;
            _navGrid = navGrid;
        }

        public void NotifyAddDebugObject(int userId, IAttackableUnit unit, uint objNetId, float lifetime, float radius, Vector3 pos1, Vector3 pos2, int objID = 0, byte type = 0x0, string name = "debugobj", byte r = 0xFF, byte g = 0x46, byte b = 0x0)
        {
            //TODO: Implement a DebugObject class so this is cleaner
            var debugObjPacket = new S2C_AddDebugObject();
            debugObjPacket.SenderNetID = unit.NetId;
            debugObjPacket.DebugID = objID;
            debugObjPacket.Lifetime = lifetime;
            debugObjPacket.Type = type;
            debugObjPacket.NetID1 = unit.NetId;
            debugObjPacket.NetID2 = objNetId;
            debugObjPacket.Radius = radius;
            debugObjPacket.Point1 = pos1;
            debugObjPacket.Point2 = pos2;
            var color = new LeaguePackets.Game.Common.Color();
            color.Red = r;
            color.Green = g;
            color.Blue = b;
            debugObjPacket.Color = color;
            debugObjPacket.MaxSize = 0; // Not sure what this does
            //debugObjPacket.Bitfield = 0x0; // Not sure what this does
            debugObjPacket.StringBuffer = name;
            _packetHandlerManager.SendPacket(userId, debugObjPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyAddGold(IChampion c, IAttackableUnit died, float gold)
        {
            var ag = new AddGold(c, died, gold);
            _packetHandlerManager.BroadcastPacket(ag, Channel.CHL_S2C);
        }

        public void NotifyAddXp(IChampion champion, float experience)
        {
            var xp = new AddXp(champion, experience);
            _packetHandlerManager.BroadcastPacket(xp, Channel.CHL_S2C);
        }

        public void NotifyAnnounceEvent(int mapId, Announces messageId, bool isMapSpecific)
        {
            var announce = new Announce(messageId, isMapSpecific ? mapId : 0);
            _packetHandlerManager.BroadcastPacket(announce, Channel.CHL_S2C);
        }

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

        private void NotifyAzirTurretSpawned(IAzirTurret azirTurret)
        {
            var spawnPacket = new SpawnAzirTurret(azirTurret);
            _packetHandlerManager.BroadcastPacketVision(azirTurret, spawnPacket, Channel.CHL_S2C);
        }

        public void NotifyBeginAutoAttack(IAttackableUnit attacker, IAttackableUnit victim, uint futureProjNetId, bool isCritical)
        {
            var aa = new BeginAutoAttack(_navGrid, attacker, victim, futureProjNetId, isCritical);
            _packetHandlerManager.BroadcastPacket(aa, Channel.CHL_S2C);
        }

        public void NotifyBlueTip(int userId, string title, string text, string imagePath, byte tipCommand, uint playerNetId,
            uint targetNetId)
        {
            var packet = new BlueTip(title, text, imagePath, tipCommand, playerNetId, targetNetId);
            _packetHandlerManager.SendPacket(userId, packet, Channel.CHL_S2C);
        }

        public void NotifyBuyItem(int userId, IObjAiBase gameObject, IItem itemInstance)
        {
            ItemData itemData = new ItemData
            {
                ItemID = (uint)itemInstance.ItemData.ItemId,
                Slot = gameObject.Inventory.GetItemSlot(itemInstance),
                ItemsInSlot = (byte)itemInstance.StackCount,
                SpellCharges = 0
            };

            //TODO find out what bitfield does, currently unknown
            var buyItemPacket = new BuyItemAns
            {
                Item = itemData,
                SenderNetID = gameObject.NetId
            };

            _packetHandlerManager.BroadcastPacket(buyItemPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyChampionDeathTimer(IChampion die)
        {
            var cdt = new ChampionDeathTimer(die);
            _packetHandlerManager.BroadcastPacket(cdt, Channel.CHL_S2C);
        }

        public void NotifyChampionDie(IChampion die, IAttackableUnit killer, int goldFromKill)
        {
            var cd = new ChampionDie(die, killer, goldFromKill);
            _packetHandlerManager.BroadcastPacket(cd, Channel.CHL_S2C);

            NotifyChampionDeathTimer(die);
        }

        public void NotifyChampionRespawn(IChampion c)
        {
            var cr = new ChampionRespawn(c);
            _packetHandlerManager.BroadcastPacket(cr, Channel.CHL_S2C);
        }

        public void NotifyChampionSpawned(IChampion c, TeamId team)
        {
            var hs = new HeroSpawn2(c);
            _packetHandlerManager.BroadcastPacketTeam(team, hs, Channel.CHL_S2C);
        }

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
            _packetHandlerManager.BroadcastPacket(cdPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyCreateUnitHighlight(int userId, IGameObject unit)
        {
            var highlightPacket = new S2C_CreateUnitHighlight();
            highlightPacket.SenderNetID = unit.NetId;
            highlightPacket.TargetNetID = unit.NetId;

            _packetHandlerManager.SendPacket(userId, highlightPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyDamageDone(IAttackableUnit source, IAttackableUnit target, float amount, GameServerCore.Enums.DamageType type, DamageText damagetext, bool isGlobal = true, int sourceId = 0, int targetId = 0)
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

        public void NotifyDebugMessage(string htmlDebugMessage)
        {
            var dm = new DebugMessage(htmlDebugMessage);
            _packetHandlerManager.BroadcastPacket(dm, Channel.CHL_S2C);
        }

        public void NotifyDebugMessage(int userId, string message)
        {
            var dm = new DebugMessage(message);
            _packetHandlerManager.SendPacket(userId, dm, Channel.CHL_S2C);
        }

        public void NotifyDebugMessage(TeamId team, string message)
        {
            var dm = new DebugMessage(message);
            _packetHandlerManager.BroadcastPacketTeam(team, dm, Channel.CHL_S2C);
        }

        public void NotifyDebugPacket(int userId, byte[] data)
        {
            _packetHandlerManager.SendPacket(userId, data, Channel.CHL_S2C);
        }

        public void NotifyDestroyClientMissile(IProjectile p)
        {
            var misPacket = new S2C_DestroyClientMissile();
            misPacket.SenderNetID = p.NetId;
            _packetHandlerManager.BroadcastPacket(misPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyDestroyClientMissile(IProjectile p, TeamId team)
        {
            var misPacket = new S2C_DestroyClientMissile();
            misPacket.SenderNetID = p.NetId;
            _packetHandlerManager.BroadcastPacketTeam(team, misPacket.GetBytes(), Channel.CHL_S2C);
        }

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

        public void NotifyEnterLocalVisibilityClient(int userId, uint netId)
        {
            var enterLocalVis = new OnEnterLocalVisiblityClient
            {
                SenderNetID = netId
            };

            _packetHandlerManager.SendPacket(userId, enterLocalVis.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyEnterLocalVisibilityClient(IAttackableUnit unit, int userId = 0)
        {
            var enterLocalVis = new OnEnterLocalVisiblityClient
            {
                SenderNetID = unit.NetId,
                MaxHealth = unit.Stats.HealthPoints.Total,
                Health = unit.Stats.CurrentHealth
            };
            if (userId == 0)
                _packetHandlerManager.BroadcastPacketVision(unit, enterLocalVis.GetBytes(), Channel.CHL_S2C);
            else _packetHandlerManager.SendPacket(userId, enterLocalVis.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyEnterVisibilityClient(IAttackableUnit u, TeamId team, int userId = 0)
        {
            var enterVis = new OnEnterVisiblityClient(); // TYPO >:(
            var itemData = new List<ItemData>(); //TODO: Fix item system so this can be finished
            enterVis.Items = itemData;
            var shields = new ShieldValues(); //TODO: Implement shields so this can be finished
            enterVis.ShieldValues = shields;
            var charStackDataList = new List<CharacterStackData>();
            var charStackData = new CharacterStackData
            {
                SkinName = u.Model,
                OverrideSpells = false,
                ModelOnly = false,
                ReplaceCharacterPackage = false,
                ID = 0
            };
            enterVis.LookAtPosition = new Vector3(1, 0, 0);
            if (u is IObjAiBase)
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
                Position = u.GetPosition(),
                Forward = new Vector2(0, 1),
                SyncID = (int)u.SyncId
            };
            enterVis.MovementData = md;
            enterVis.SenderNetID = u.NetId;
            switch (u)
            {
                case IMinion m:
                    {
                        // TODO: This implementation will probably need a refactor later
                        charStackData.SkinID = 0;
                        charStackDataList.Add(charStackData);
                        enterVis.CharacterDataStack = charStackDataList;
                        if (userId != 0)
                        {
                            _packetHandlerManager.SendPacket(userId, enterVis.GetBytes(), Channel.CHL_S2C);
                        }
                        else
                        {
                            _packetHandlerManager.BroadcastPacketTeam(team, enterVis.GetBytes(), Channel.CHL_S2C);
                            NotifyEnterLocalVisibilityClient(m);
                        }
                        return;
                    }
                case IChampion c:
                    {
                        charStackData.SkinID = (uint)c.Skin;
                        charStackDataList.Add(charStackData);
                        enterVis.CharacterDataStack = charStackDataList;
                        if (userId != 0)
                        {
                            _packetHandlerManager.SendPacket(userId, enterVis.GetBytes(), Channel.CHL_S2C);
                        }
                        else
                        {
                            _packetHandlerManager.BroadcastPacketTeam(team, enterVis.GetBytes(), Channel.CHL_S2C);
                            NotifyEnterLocalVisibilityClient(c);
                        }
                        break;
                    }
            }
        }

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

        public void NotifyForceCreateMissile(IProjectile p)
        {
            var misPacket = new S2C_ForceCreateMissile();
            misPacket.SenderNetID = p.Owner.NetId;
            misPacket.MissileNetID = p.NetId;
            _packetHandlerManager.BroadcastPacket(misPacket.GetBytes(), Channel.CHL_S2C);
        }

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

        public void NotifyFXKill(IParticle particle)
        {
            var fxKill = new FX_Kill
            {
                SenderNetID = particle.NetId,
                NetID = particle.NetId
            };
            _packetHandlerManager.BroadcastPacket(fxKill.GetBytes(), Channel.CHL_S2C);
        }

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

        public void NotifyGameStart()
        {
            var start = new StatePacket(PacketCmd.PKT_S2C_START_GAME);
            _packetHandlerManager.BroadcastPacket(start, Channel.CHL_S2C);
        }

        public void NotifyGameTimer(float gameTime)
        {
            var gameTimer = new GameTimer(gameTime / 1000.0f);
            _packetHandlerManager.BroadcastPacket(gameTimer, Channel.CHL_S2C);
        }

        public void NotifyGameTimer(int userId, float time)
        {
            var timer = new GameTimer(time / 1000.0f);
            _packetHandlerManager.SendPacket(userId, timer, Channel.CHL_S2C);
        }

        public void NotifyGameTimerUpdate(int userId, float time)
        {
            var timer = new GameTimerUpdate(time / 1000.0f);
            _packetHandlerManager.SendPacket(userId, timer, Channel.CHL_S2C);
        }

        public void NotifyHeroSpawn(int userId, ClientInfo client)
        {
            var spawn = new HeroSpawn(client);
            _packetHandlerManager.SendPacket(userId, spawn, Channel.CHL_S2C);
        }

        public void NotifyHeroSpawn2(int userId, IChampion champion)
        {
            var heroSpawnPacket = new HeroSpawn2(champion);
            _packetHandlerManager.SendPacket(userId, heroSpawnPacket, Channel.CHL_S2C);
        }

        public void NotifyInhibitorSpawningSoon(IInhibitor inhibitor)
        {
            var packet = new UnitAnnounce(UnitAnnounces.INHIBITOR_ABOUT_TO_SPAWN, inhibitor);
            _packetHandlerManager.BroadcastPacket(packet, Channel.CHL_S2C);
        }

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

        public void NotifyItemsSwapped(IChampion c, byte fromSlot, byte toSlot)
        {
            var sia = new SwapItemsResponse(c, fromSlot, toSlot);
            _packetHandlerManager.BroadcastPacketVision(c, sia, Channel.CHL_S2C);
        }

        public void NotifyKeyCheck(ulong playerId, uint clientId)
        {
            var response = new KeyCheckResponse(playerId, clientId);
            _packetHandlerManager.SendPacket((int)playerId, response, Channel.CHL_HANDSHAKE);
        }

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

        public void NotifyLeaveVision(IGameObject o, TeamId team)
        {
            var lv = new LeaveVision(o);
            _packetHandlerManager.BroadcastPacketTeam(team, lv, Channel.CHL_S2C);

            // Not exactly sure what this is yet
            var c = o as IChampion;
            if (o == null)
            {
                var deleteObj = new DeleteObjectFromVision(o);
                _packetHandlerManager.BroadcastPacketTeam(team, deleteObj, Channel.CHL_S2C);
            }
        }

        public void NotifyLevelPropSpawn(int userId, ILevelProp levelProp)
        {
            var levelPropSpawnPacket = new LevelPropSpawn(levelProp);
            _packetHandlerManager.SendPacket(userId, levelPropSpawnPacket, Channel.CHL_S2C);
        }

        public void NotifyLevelUp(IChampion c)
        {
            var lu = new LevelUp(c);
            _packetHandlerManager.BroadcastPacket(lu, Channel.CHL_S2C);
        }

        public void NotifyLoadScreenInfo(int userId, List<Tuple<uint, ClientInfo>> players)
        {
            var screenInfo = new LoadScreenInfo(players);
            _packetHandlerManager.SendPacket(userId, screenInfo, Channel.CHL_LOADING_SCREEN);
        }

        public void NotifyLoadScreenPlayerChampion(int userId, Tuple<uint, ClientInfo> player)
        {
            var loadChampion = new LoadScreenPlayerChampion(player);
            _packetHandlerManager.SendPacket(userId, loadChampion, Channel.CHL_LOADING_SCREEN);
        }

        public void NotifyLoadScreenPlayerName(int userId, Tuple<uint, ClientInfo> player)
        {
            var loadName = new LoadScreenPlayerName(player);
            _packetHandlerManager.SendPacket(userId, loadName, Channel.CHL_LOADING_SCREEN);
        }

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
            spawnPacket.Position = new Vector3(minion.GetPosition().X, minion.GetZ(), minion.GetPosition().Y); // check if work, probably not
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

        public void NotifyMissileReplication(IProjectile p)
        {
            var misPacket = new MissileReplication();
            misPacket.SenderNetID = p.Owner.NetId;
            misPacket.Position = new Vector3(p.X, p.GetZ(), p.Y);
            misPacket.CasterPosition = new Vector3(p.Owner.X, p.Owner.GetZ(), p.Owner.Y);
            var current = new Vector3(p.X, _navGrid.GetHeightAtLocation(p.X, p.Y), p.Y);
            var to = Vector3.Normalize(new Vector3(p.Target.X, _navGrid.GetHeightAtLocation(p.Target.X, p.Target.Y), p.Target.Y) - current);
            // Not sure if we want to add height for these, but i did it anyway
            misPacket.Direction = new Vector3(to.X, 0, to.Y);
            misPacket.Velocity = new Vector3(to.X * p.GetMoveSpeed(), 0, to.Y * p.GetMoveSpeed());
            misPacket.StartPoint = new Vector3(p.X, p.GetZ(), p.Y);
            misPacket.EndPoint = new Vector3(p.Target.X, _navGrid.GetHeightAtLocation(p.Target.X, p.Target.Y), p.Target.Y);
            misPacket.UnitPosition = new Vector3(p.Owner.X, p.Owner.GetZ(), p.Owner.Y);
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
            cast.SpellCastLaunchPosition = new Vector3(p.X, p.GetZ(), p.Y);
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

        public void NotifyModelUpdate(IAttackableUnit obj)
        {
            var mp = new UpdateModel(obj.NetId, obj.Model);
            _packetHandlerManager.BroadcastPacket(mp, Channel.CHL_S2C);
        }

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

        private void NotifyMonsterSpawned(IMonster m)
        {
            var sp = new SpawnMonster(_navGrid, m);
            _packetHandlerManager.BroadcastPacketVision(m, sp, Channel.CHL_S2C);
        }

        public void NotifyMovement(IGameObject o)
        {
            var answer = new MovementResponse(_navGrid, o);
            _packetHandlerManager.BroadcastPacketVision(o, answer, Channel.CHL_LOW_PRIORITY);
        }

        public void NotifyNextAutoAttack(IAttackableUnit attacker, IAttackableUnit target, uint futureProjNetId, bool isCritical,
            bool nextAttackFlag)
        {
            var aa = new NextAutoAttack(attacker, target, futureProjNetId, isCritical, nextAttackFlag);
            _packetHandlerManager.BroadcastPacket(aa, Channel.CHL_S2C);
        }

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

        public void NotifyNPC_CastSpellAns(INavigationGrid navGrid, ISpell s, Vector2 start, Vector2 end, uint futureProjNetId)
        {
            var castAnsPacket = new NPC_CastSpellAns
            {
                SenderNetID = s.Owner.NetId,
                CasterPositionSyncID = (int)s.Owner.SyncId,
                Unknown1 = false, // TODO: Find what this is (if false, CasterPositionSyncID is used)
            };
            var castInfo = new CastInfo();
            castInfo.SpellHash = (uint)s.GetId();
            castInfo.SpellNetID = s.SpellNetId;
            castInfo.SpellLevel = s.Level;
            castInfo.AttackSpeedModifier = 1.0f; // TOOD: Unhardcode
            castInfo.CasterNetID = s.Owner.NetId;
            castInfo.SpellChainOwnerNetID = s.Owner.NetId;
            castInfo.PackageHash = s.Owner.GetObjHash();
            castInfo.MissileNetID = futureProjNetId;
            castInfo.TargetPosition = new Vector3(start.X, navGrid.GetHeightAtLocation(start.X, start.Y), start.Y);
            castInfo.TargetPositionEnd = new Vector3(end.X, navGrid.GetHeightAtLocation(end.X, end.Y), end.Y);
            // TODO: Implement castInfo.Targets
            castInfo.DesignerCastTime = s.SpellData.GetCastTime(); // TODO: Verify
            castInfo.ExtraCastTime = 0.0f; // TODO: Unhardcode
            castInfo.DesignerTotalTime = s.SpellData.GetCastTimeTotal(); // TODO: Verify
            castInfo.Cooldown = s.GetCooldown();
            castInfo.StartCastTime = 0.0f; // TODO: Unhardcode
            //TODO: Implement castInfo.IsAutoAttack/IsSecondAutoAttack/IsForceCastingOrChannel/IsOverrideCastPosition/IsClickCasted (you may have checks for all of these, but only one of these can be present in the packet when sent)
            castInfo.SpellSlot = s.Slot;
            if (s.Level > 0)
            {
                castInfo.ManaCost = s.SpellData.ManaCost[s.Level - 1];
            }
            else
            {
                castInfo.ManaCost = s.SpellData.ManaCost[s.Level];
            }
            castInfo.SpellCastLaunchPosition = new Vector3(s.Owner.X, s.Owner.GetZ(), s.Owner.Y);
            castInfo.AmmoUsed = 1; // TODO: Unhardcode (requires implementing Ammo)
            castInfo.AmmoRechargeTime = s.GetCooldown(); // TODO: Implement correctly (requires implementing Ammo)
            castAnsPacket.CastInfo = castInfo;
            _packetHandlerManager.BroadcastPacketVision(s.Owner, castAnsPacket.GetBytes(), Channel.CHL_S2C);
        }

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

        public void NotifyNpcDie(IAttackableUnit die, IAttackableUnit killer)
        {
            var nd = new NpcDie(die, killer);
            _packetHandlerManager.BroadcastPacket(nd, Channel.CHL_S2C);
        }

        public void NotifyOnAttack(IAttackableUnit attacker, IAttackableUnit attacked, AttackType attackType)
        {
            var oa = new OnAttack(attacker, attacked, attackType);
            _packetHandlerManager.BroadcastPacket(oa, Channel.CHL_S2C);
        }

        public void NotifyPauseGame(int seconds, bool showWindow)
        {
            var pg = new PauseGame(seconds, showWindow);
            _packetHandlerManager.BroadcastPacket(pg, Channel.CHL_S2C);
        }

        public void NotifyPing(ClientInfo client, Vector2 pos, int targetNetId, Pings type)
        {
            var ping = new AttentionPingRequest(pos.X, pos.Y, targetNetId, type);
            var response = new AttentionPingResponse(client, ping);
            _packetHandlerManager.BroadcastPacketTeam(client.Team, response, Channel.CHL_S2C);
        }

        public void NotifyPingLoadInfo(PingLoadInfoRequest request, ClientInfo clientInfo)
        {
            var response = new PingLoadInfoResponse(request.NetId, clientInfo.ClientId, request.Loaded, request.Unk2,
                request.Ping, request.Unk3, request.Unk4, clientInfo.PlayerId);

            //Logging->writeLine("loaded: %f, ping: %f, %f", loadInfo->loaded, loadInfo->ping, loadInfo->f3);
            _packetHandlerManager.BroadcastPacket(response, Channel.CHL_LOW_PRIORITY, PacketFlags.None);
        }

        public void NotifyPlayerStats(IChampion champion)
        {
            var response = new PlayerStats(champion);
            // TODO: research how to send the packet
            _packetHandlerManager.BroadcastPacket(response, Channel.CHL_S2C);
        }

        public void NotifyQueryStatus(int userId)
        {
            var response = new QueryStatus();
            _packetHandlerManager.SendPacket(userId, response, Channel.CHL_S2C);
        }

        public void NotifyRemoveDebugObject(int userId, uint sender, int objID)
        {
            var debugObjPacket = new S2C_RemoveDebugObject
            {
                SenderNetID = sender,
                ObjectID = objID
            };
            _packetHandlerManager.SendPacket(userId, debugObjPacket.GetBytes(), Channel.CHL_S2C);
        }

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

        public void NotifyRemoveUnitHighlight(int userId, IGameObject unit)
        {
            var highlightPacket = new S2C_RemoveUnitHighlight
            {
                SenderNetID = unit.NetId,
                NetID = unit.NetId
            };
            _packetHandlerManager.SendPacket(userId, highlightPacket.GetBytes(), Channel.CHL_S2C);
        }

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

        public void NotifySetAnimation(IAttackableUnit u, List<string> animationPairs)
        {
            var setAnimation = new SetAnimation(u, animationPairs);
            _packetHandlerManager.BroadcastPacketVision(u, setAnimation, Channel.CHL_S2C);
        }

        public void NotifySetCanSurrender(bool can, TeamId team)
        {
            var canSurrender = new S2C_SetCanSurrender()
            {
                CanSurrender = can
            };
            _packetHandlerManager.BroadcastPacketTeam(team, canSurrender.GetBytes(), Channel.CHL_S2C);
        }

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

        public void NotifySetTarget(IAttackableUnit attacker, IAttackableUnit target)
        {
            var st = new SetTarget(attacker, target);
            _packetHandlerManager.BroadcastPacket(st, Channel.CHL_S2C);

            var st2 = new SetTarget2(attacker, target);
            _packetHandlerManager.BroadcastPacket(st2, Channel.CHL_S2C);
        }

        public void NotifySetTeam(IAttackableUnit unit, TeamId team)
        {
            var p = new SetTeam(unit, team);
            _packetHandlerManager.BroadcastPacket(p, Channel.CHL_S2C);
        }

        public void NotifySkillUp(int userId, uint netId, byte skill, byte level, byte pointsLeft)
        {
            var skillUpResponse = new SkillUpResponse(netId, skill, level, pointsLeft);
            _packetHandlerManager.SendPacket(userId, skillUpResponse, Channel.CHL_GAMEPLAY);
        }

        public void NotifySpawn(IAttackableUnit u)
        {
            switch (u)
            {
                case ILaneMinion m:
                    NotifyLaneMinionSpawned(m, m.Team.GetEnemyTeam());
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

            NotifyEnterLocalVisibilityClient(u);
        }

        public void NotifySpawnEnd(int userId)
        {
            var endSpawnPacket = new StatePacket(PacketCmd.PKT_S2C_END_SPAWN);
            _packetHandlerManager.SendPacket(userId, endSpawnPacket, Channel.CHL_S2C);
        }

        public void NotifySpawnStart(int userId)
        {
            var start = new StatePacket2(PacketCmd.PKT_S2C_START_SPAWN);
            _packetHandlerManager.SendPacket(userId, start, Channel.CHL_S2C);
        }

        public void NotifySpellAnimation(IAttackableUnit u, string animation)
        {
            var sa = new SpellAnimation(u, animation);
            _packetHandlerManager.BroadcastPacketVision(u, sa, Channel.CHL_S2C);
        }

        public void NotifyStaticObjectSpawn(int userId, uint netId)
        {
            var minionSpawnPacket = new MinionSpawn2(netId);
            _packetHandlerManager.SendPacket(userId, minionSpawnPacket, Channel.CHL_S2C);
        }

        public void NotifySynchVersion(int userId, List<Tuple<uint, ClientInfo>> players, string version, string gameMode, int mapId)
        {
            var response = new SynchVersionResponse(players, version, "CLASSIC", mapId);
            _packetHandlerManager.SendPacket(userId, response, Channel.CHL_S2C);
        }

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

        public void NotifyTeleport(IAttackableUnit u, Vector2 pos)
        {
            var packet = new TeleportRequest(u.NetId, pos.X, pos.Y, false);
            _packetHandlerManager.BroadcastPacketVision(u, packet, Channel.CHL_S2C);
        }

        public void NotifyTint(TeamId team, bool enable, float speed, GameServerCore.Content.Color color)
        {
            var tint = new SetScreenTint(team, enable, speed, color.R, color.G, color.B, color.A);
            _packetHandlerManager.BroadcastPacket(tint, Channel.CHL_S2C);
        }

        public void NotifyTurretSpawn(int userId, ILaneTurret turret)
        {
            var turretSpawn = new TurretSpawn(turret);
            _packetHandlerManager.SendPacket(userId, turretSpawn, Channel.CHL_S2C);
        }

        public void NotifyUnitAnnounceEvent(UnitAnnounces messageId, IAttackableUnit target, IGameObject killer = null,
            List<IChampion> assists = null)
        {
            var announce = new UnitAnnounce(messageId, target, killer, assists);
            _packetHandlerManager.BroadcastPacket(announce, Channel.CHL_S2C);
        }

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

        public void NotifyUnpauseGame()
        {
            // TODO: currently unpause disabled cause it shouldn't handled like this
            _packetHandlerManager.UnpauseGame();
        }

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

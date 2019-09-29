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

namespace PacketDefinitions420
{
    public class PacketNotifier : IPacketNotifier
    {
        private readonly IPacketHandlerManager _packetHandlerManager;
        private readonly INavGrid _navGrid;

        public PacketNotifier(IPacketHandlerManager packetHandlerManager, INavGrid navGrid)
        {
            _packetHandlerManager = packetHandlerManager;
            _navGrid = navGrid;
        }

        public void NotifyLaneMinionSpawned(ILaneMinion m, TeamId team)
        {
            var ms = new LaneMinionSpawn(_navGrid, m);
            _packetHandlerManager.BroadcastPacketTeam(team, ms, Channel.CHL_S2C);
            NotifyEnterLocalVisibilityClient(m);
        }

        public void NotifyGameEnd(Vector3 cameraPosition, INexus nexus, List<Pair<uint, ClientInfo>> players)
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

        public void NotifyPing(ClientInfo client, Vector2 pos, int targetNetId, Pings type)
        {
            var ping = new AttentionPingRequest(pos.X, pos.Y, targetNetId, type);
            var response = new AttentionPingResponse(client, ping);
            _packetHandlerManager.BroadcastPacketTeam(client.Team, response, Channel.CHL_S2C);
        }

        public void NotifyTint(TeamId team, bool enable, float speed, GameServerCore.Content.Color color)
        {
            var tint = new SetScreenTint(team, enable, speed, color.R, color.G, color.B, color.A);
            _packetHandlerManager.BroadcastPacket(tint, Channel.CHL_S2C);
        }

        public void NotifySkillUp(int userId, uint netId, byte skill, byte level, byte pointsLeft)
        {
            var skillUpResponse = new SkillUpResponse(netId, skill, level, pointsLeft);
            _packetHandlerManager.SendPacket(userId, skillUpResponse, Channel.CHL_GAMEPLAY);
        }

        public void NotifySetTeam(IAttackableUnit unit, TeamId team)
        {
            var p = new SetTeam(unit, team);
            _packetHandlerManager.BroadcastPacket(p, Channel.CHL_S2C);
        }

        public void NotifyCastSpell(INavGrid navGrid, ISpell s, Vector2 start, Vector2 end, uint futureProjNetId,
            uint spellNetId)
        {
            var response = new CastSpellResponse(navGrid, s, start.X, start.Y, end.X, end.Y, futureProjNetId, spellNetId);
            _packetHandlerManager.BroadcastPacket(response, Channel.CHL_S2C);
        }

        public void NotifyBlueTip(int userId, string title, string text, string imagePath, byte tipCommand, uint playerNetId,
            uint targetNetId)
        {
            var packet = new BlueTip(title, text, imagePath, tipCommand, playerNetId, targetNetId);
            _packetHandlerManager.SendPacket(userId, packet, Channel.CHL_S2C);
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

        // TODO: check if this is broadcast or not
        public void NotifyKeyCheck(ulong playerId, uint clientId)
        {
            var response = new KeyCheckResponse(playerId, clientId);
            _packetHandlerManager.SendPacket((int)playerId, response, Channel.CHL_HANDSHAKE);
        }

        public void NotifyPingLoadInfo(PingLoadInfoRequest request, ClientInfo clientInfo)
        {
            var response = new PingLoadInfoResponse(request.NetId, clientInfo.ClientId, request.Loaded, request.Unk2,
                request.Ping, request.Unk3, request.Unk4, clientInfo.PlayerId);

            //Logging->writeLine("loaded: %f, ping: %f, %f", loadInfo->loaded, loadInfo->ping, loadInfo->f3);
            _packetHandlerManager.BroadcastPacket(response, Channel.CHL_LOW_PRIORITY, PacketFlags.None);
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

        public void NotifySynchVersion(int userId, List<Pair<uint, ClientInfo>> players, string version, string gameMode, int mapId)
        {
            var response = new SynchVersionResponse(players, version, "CLASSIC", mapId);
            _packetHandlerManager.SendPacket(userId, response, Channel.CHL_S2C);
        }

        public void NotifyLoadScreenInfo(int userId, List<Pair<uint,ClientInfo>> players)
        {
            var screenInfo = new LoadScreenInfo(players);
            _packetHandlerManager.SendPacket(userId, screenInfo, Channel.CHL_LOADING_SCREEN);
        }

        public void NotifyLoadScreenPlayerName(int userId, Pair<uint,ClientInfo> player)
        {
            var loadName = new LoadScreenPlayerName(player);
            _packetHandlerManager.SendPacket(userId, loadName, Channel.CHL_LOADING_SCREEN);
        }

        public void NotifyLoadScreenPlayerChampion(int userId, Pair<uint,ClientInfo> player)
        {
            var loadChampion = new LoadScreenPlayerChampion(player);
            _packetHandlerManager.SendPacket(userId, loadChampion, Channel.CHL_LOADING_SCREEN);
        }

        public void NotifyQueryStatus(int userId)
        {
            var response = new QueryStatus();
            _packetHandlerManager.SendPacket(userId, response, Channel.CHL_S2C);
        }

        public void NotifyPlayerStats(IChampion champion)
        {
            var response = new PlayerStats(champion);
            // TODO: research how to send the packet
            _packetHandlerManager.BroadcastPacket(response, Channel.CHL_S2C);
        }

        public void NotifyUnpauseGame()
        {
            // TODO: currently unpause disabled cause it shouldn't handled like this
            _packetHandlerManager.UnpauseGame();
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

        public void NotifySetCanSurrender(bool can, TeamId team)
        {
            var canSurrender = new S2C_SetCanSurrender()
            {
                CanSurrender = can
            };
            _packetHandlerManager.BroadcastPacketTeam(team, canSurrender.GetBytes(), Channel.CHL_S2C );
        }

        public void NotifyGameStart()
        {
            var start = new StatePacket(PacketCmd.PKT_S2C_START_GAME);
            _packetHandlerManager.BroadcastPacket(start, Channel.CHL_S2C);
        }

        public void NotifyHeroSpawn2(int userId, IChampion champion)
        {
            var heroSpawnPacket = new HeroSpawn2(champion);
            _packetHandlerManager.SendPacket(userId, heroSpawnPacket, Channel.CHL_S2C);
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

        public void NotifySpawnStart(int userId)
        {
            var start = new StatePacket2(PacketCmd.PKT_S2C_START_SPAWN);
            _packetHandlerManager.SendPacket(userId, start, Channel.CHL_S2C);
        }

        public void NotifySpawnEnd(int userId)
        {
            var endSpawnPacket = new StatePacket(PacketCmd.PKT_S2C_END_SPAWN);
            _packetHandlerManager.SendPacket(userId, endSpawnPacket, Channel.CHL_S2C);
        }

        public void NotifyHeroSpawn(int userId, ClientInfo client)
        {
            var spawn = new HeroSpawn(client);
            _packetHandlerManager.SendPacket(userId, spawn, Channel.CHL_S2C);
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
                avatar.ItemIDs[i] =(uint) runeValue;
            }
            // TODO: add talents
            _packetHandlerManager.SendPacket(userId, avatar.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyBuyItem(int userId, IObjAiBase gameObject, IItem itemInstance)
        {
            ItemData itemData = new ItemData
            {
                ItemID = (uint)itemInstance.ItemData.ItemId,
                Slot = gameObject.Inventory.GetItemSlot(itemInstance),
                ItemsInSlot = itemInstance.StackCount,
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

        public void NotifyTurretSpawn(int userId, ILaneTurret turret)
        {
            var turretSpawn = new TurretSpawn(turret);
            _packetHandlerManager.SendPacket(userId, turretSpawn, Channel.CHL_S2C);
        }

        public void NotifyLevelPropSpawn(int userId, ILevelProp levelProp)
        {
            var levelPropSpawnPacket = new LevelPropSpawn(levelProp);
            _packetHandlerManager.SendPacket(userId, levelPropSpawnPacket, Channel.CHL_S2C);
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

        public void NotifyStaticObjectSpawn(int userId, uint netId)
        {
            var minionSpawnPacket = new MinionSpawn2(netId);
            _packetHandlerManager.SendPacket(userId, minionSpawnPacket, Channel.CHL_S2C);
        }

        public void NotifyEnterLocalVisibilityClient(int userId, uint netId)
        {
            var enterLocalVis = new OnEnterLocalVisiblityClient
            {
                SenderNetID = netId
            };

            _packetHandlerManager.SendPacket(userId, enterLocalVis.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyFaceDirection(IAttackableUnit u, Vector2 direction, bool isInstant = true, float turnTime = 0.0833f)
        {
            var height = _navGrid.GetHeightAtLocation(direction);
            var fd = new FaceDirection(u, direction.X, direction.Y, height, isInstant, turnTime);
            _packetHandlerManager.BroadcastPacketVision(u, fd, Channel.CHL_S2C);
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

        public void NotifyInhibitorSpawningSoon(IInhibitor inhibitor)
        {
            var packet = new UnitAnnounce(UnitAnnounces.INHIBITOR_ABOUT_TO_SPAWN, inhibitor);
            _packetHandlerManager.BroadcastPacket(packet, Channel.CHL_S2C);
        }

        public void NotifyAddBuff(IBuff b)
        {
            var add = new AddBuff(b.TargetUnit, b.SourceUnit, b.StackCount, b.Duration, b.BuffType, b.Name, b.Slot);
            _packetHandlerManager.BroadcastPacket(add, Channel.CHL_S2C);
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

        public void NotifyEditBuff(IBuff b, int stacks)
        {
            var edit = new EditBuff(b.TargetUnit, b.Slot, b.StackCount);
            _packetHandlerManager.BroadcastPacket(edit, Channel.CHL_S2C);
        }

        public void NotifyRemoveBuff(IAttackableUnit u, string buffName, byte slot = 0x01)
        {
            var remove = new RemoveBuff(u, buffName, slot);
            _packetHandlerManager.BroadcastPacket(remove, Channel.CHL_S2C);
        }

        public void NotifyTeleport(IAttackableUnit u, Vector2 pos)
        {
            var packet = new TeleportRequest(u.NetId, pos.X, pos.Y, false);
            _packetHandlerManager.BroadcastPacketVision(u, packet, Channel.CHL_S2C);
        }

        public void NotifyMovement(IGameObject o)
        {
            var answer = new MovementResponse(_navGrid, o);
            _packetHandlerManager.BroadcastPacketVision(o, answer, Channel.CHL_LOW_PRIORITY);
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

        public void NotifyModifyShield(IAttackableUnit unit, float amount, bool IsPhysical, bool IsMagical, bool StopShieldFade)
        {
            var mods = new ModifyShield();
            mods.SenderNetID = unit.NetId;
            mods.Physical = IsPhysical;
            mods.Magical = IsMagical;
            mods.Ammount = amount;
            _packetHandlerManager.BroadcastPacket(mods.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyBeginAutoAttack(IAttackableUnit attacker, IAttackableUnit victim, uint futureProjNetId, bool isCritical)
        {
            var aa = new BeginAutoAttack(_navGrid, attacker, victim, futureProjNetId, isCritical);
            _packetHandlerManager.BroadcastPacket(aa, Channel.CHL_S2C);
        }

        public void NotifyNextAutoAttack(IAttackableUnit attacker, IAttackableUnit target, uint futureProjNetId, bool isCritical,
            bool nextAttackFlag)
        {
            var aa = new NextAutoAttack(attacker, target, futureProjNetId, isCritical, nextAttackFlag);
            _packetHandlerManager.BroadcastPacket(aa, Channel.CHL_S2C);
        }

        public void NotifyOnAttack(IAttackableUnit attacker, IAttackableUnit attacked, AttackType attackType)
        {
            var oa = new OnAttack(attacker, attacked, attackType);
            _packetHandlerManager.BroadcastPacket(oa, Channel.CHL_S2C);
        }

        public void NotifyMissileReplication(IProjectile p)
        {
            //TODO: Add OwnerSpell var to Projectile/IProjectile class to make things easier
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
            misPacket.TimeFromCreation = 0f; // Unsure of a use for this
            misPacket.Speed = p.GetMoveSpeed();
            misPacket.LifePercentage = 0f; // Unsure of a use for this
            //TODO: Implement time limited projectiles
            misPacket.TimedSpeedDelta = 0f; // Likely for time limited projectiles, implement this
            misPacket.TimedSpeedDeltaTime = 0x7F7FFFFF; // Same as above (this value is from the SpawnProjectile packet, it is a placeholder)
            misPacket.Bounced = false; //TODO: Implement bouncing projectiles
            var cast = new CastInfo();
            cast.SpellHash = (uint)p.ProjectileId;
            cast.SpellNetID = p.OriginSpell != null ? p.OriginSpell.SpellNetId : 0;
            cast.SpellLevel = p.OriginSpell != null ? p.OriginSpell.Level : (byte)0;
            cast.AttackSpeedModifier = 1.0f; // Unsure of a use for this
            cast.CasterNetID = p.OriginSpell != null ? p.OriginSpell.Owner.NetId : p.Owner.NetId;
            //TODO: Implement spell chains
            cast.SpellChainOwnerNetID = p.OriginSpell != null ? p.OriginSpell.Owner.NetId : p.Owner.NetId; // Might change in the future, spell chains not implemented
            cast.PackageHash = p.OriginSpell != null ? (uint)(p.Owner as IChampion).GetChampionHash() : 0; // Probably incorrect, taken from SpawnProjectile packet
            cast.MissileNetID = p.NetId;
            // Not sure if we want to add height for these, but i did it anyway
            cast.TargetPosition = new Vector3(p.Target.X, _navGrid.GetHeightAtLocation(p.Target.X, p.Target.Y), p.Target.Y);
            cast.TargetPositionEnd = new Vector3(p.Target.X, _navGrid.GetHeightAtLocation(p.Target.X, p.Target.Y), p.Target.Y);

            if (!p.Target.IsSimpleTarget)
            {
                var targets = new List<CastInfo.Target>();
                var tar = new CastInfo.Target();
                tar.UnitNetID = (p.Target as IAttackableUnit).NetId;
                tar.HitResult = 0; // Not sure what to put here
                targets.Add(tar);
                cast.Targets = targets;
            }

            cast.DesignerCastTime = p.OriginSpell != null ? p.OriginSpell.CastTime : 1.0f; // Probably incorrect
            cast.ExtraCastTime = 0f; // Unsure of a use for this
            cast.DesignerTotalTime = p.OriginSpell != null ? p.OriginSpell.CastTime : 1.0f; // Probably incorrect
            cast.Cooldown = p.OriginSpell != null ? p.OriginSpell.GetCooldown() : 0f;
            cast.StartCastTime = p.OriginSpell != null ? p.OriginSpell.CastTime : 0f; // Probably incorrect, maybe channel time?

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

        public void NotifyForceCreateMissile(IProjectile p)
        {
            var misPacket = new S2C_ForceCreateMissile();
            misPacket.SenderNetID = p.Owner.NetId;
            misPacket.MissileNetID = p.NetId;
            _packetHandlerManager.BroadcastPacket(misPacket.GetBytes(), Channel.CHL_S2C);
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

        public void NotifyFXCreateGroup(IParticle particle, Vector3 direction = new Vector3(), float timespent = 0, bool reqvision = true)
        {
            var fxPacket = new FX_Create_Group();
            fxPacket.SenderNetID = particle.Owner.NetId;
            var fxData1 = new FXCreateData();
            if (particle.Target.IsSimpleTarget) // Non-object target (usually a position)
            {
                fxData1.TargetNetID = particle.Owner.NetId; // Probably not correct, but it works
            }
            else
            {
                fxData1.TargetNetID = (particle.Target as IGameObject).NetId;
            }
            fxData1.NetAssignedNetID = particle.NetId;
            fxData1.CasterNetID = particle.Owner.NetId;
            if (particle.Target.IsSimpleTarget)
            {
                fxData1.BindNetID = 0; // Not sure what this is
            }
            else
            {
                fxData1.BindNetID = (particle.Target as IGameObject).NetId; // Not sure what this is
            }
            fxData1.KeywordNetID = 0; // Not sure what this is
            var ownerHeight = _navGrid.GetHeightAtLocation(particle.Owner.X, particle.Owner.Y);
            var targetHeight = _navGrid.GetHeightAtLocation(particle.Target.X, particle.Target.Y);
            var particleHeight = _navGrid.GetHeightAtLocation(particle.X, particle.Y);
            var higherValue = Math.Max(targetHeight, particleHeight);
            fxData1.PositionX = (short)((particle.X - _navGrid.MapWidth / 2) / 2);
            fxData1.PositionY = higherValue;
            fxData1.PositionZ = (short)((particle.Y - _navGrid.MapHeight / 2) / 2);
            if (!particle.Target.IsSimpleTarget)
            {
                fxData1.TargetPositionX = (short)(particle.Target as IGameObject).X;
                fxData1.TargetPositionZ = (short)(particle.Target as IGameObject).Y;
            }
            else
            {
                fxData1.TargetPositionX = (short)((particle.Target.X - _navGrid.MapWidth / 2) / 2);
                fxData1.TargetPositionZ = (short)((particle.Target.Y - _navGrid.MapHeight / 2) / 2);
            }
            fxData1.TargetPositionY = targetHeight;
            fxData1.OwnerPositionX = (short)((particle.Owner.X - _navGrid.MapWidth / 2) / 2);
            fxData1.OwnerPositionY = ownerHeight;
            fxData1.OwnerPositionZ = (short)((particle.Owner.Y - _navGrid.MapHeight / 2) / 2);
            if (direction.Length() <= 0)
            {
                fxData1.OrientationVector = Vector3.Zero;
            }
            else
            {
                fxData1.OrientationVector = direction;
            }
            fxData1.TimeSpent = timespent; // TODO: would be nice to have some option for time before removal (if possible) (not likely, as some particles have a set lifetime)
            fxData1.ScriptScale = particle.Size;
            var fxDataList = new List<FXCreateData>();
            // TODO: implement option for multiple particles instead of hardcoding one
            fxDataList.Add(fxData1);
            var fxGroupData1 = new FXCreateGroupData();
            fxGroupData1.PackageHash = (uint)particle.Owner.GetChampionHash();
            fxGroupData1.EffectNameHash = HashFunctions.HashString(particle.Name);
            //TODO: un-hardcode flags
            fxGroupData1.Flags = 0x20; // Taken from SpawnParticle packet
            fxGroupData1.TargetBoneNameHash = 0; // Are these the same?
            fxGroupData1.BoneNameHash = HashFunctions.HashString(particle.BoneName); // Are these the same?
            fxGroupData1.FXCreateData = fxDataList;
            var fxGroup = new List<FXCreateGroupData>();
            fxGroup.Add(fxGroupData1);
            fxPacket.FXCreateGroup = fxGroup;
            if (reqvision)
            {
                _packetHandlerManager.BroadcastPacketVision(particle, fxPacket.GetBytes(), Channel.CHL_S2C);
            }
            else
            {
                _packetHandlerManager.BroadcastPacket(fxPacket.GetBytes(), Channel.CHL_S2C);
            }
        }

        public void NotifyFXKill(IParticle particle)
        {
            var fxKill = new FX_Kill();
            fxKill.SenderNetID = particle.NetId;
            fxKill.NetID = particle.NetId;
            _packetHandlerManager.BroadcastPacket(fxKill.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyFXEnterTeamVisibility(IParticle particle, TeamId team)
        {
            var fxVisPacket = new S2C_FX_OnEnterTeamVisiblity();
            fxVisPacket.SenderNetID = particle.NetId;
            fxVisPacket.NetID = particle.NetId;
            fxVisPacket.VisibilityTeam = (byte)team;
            _packetHandlerManager.BroadcastPacketTeam(team, fxVisPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyFXLeaveTeamVisibility(IParticle particle, TeamId team)
        {
            var fxVisPacket = new S2C_FX_OnLeaveTeamVisiblity();
            fxVisPacket.SenderNetID = particle.NetId;
            fxVisPacket.NetID = particle.NetId;
            fxVisPacket.VisibilityTeam = (byte)team;
            _packetHandlerManager.BroadcastPacketTeam(team, fxVisPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyModelUpdate(IAttackableUnit obj)
        {
            var mp = new UpdateModel(obj.NetId, obj.Model);
            _packetHandlerManager.BroadcastPacket(mp, Channel.CHL_S2C);
        }

        public void NotifyInstantStopAttack(IAttackableUnit attacker, bool isSummonerSpell, uint missileNetID = 0)
        {
            var stopAttack = new NPC_InstantStop_Attack();
            stopAttack.SenderNetID = attacker.NetId;
            stopAttack.MissileNetID = missileNetID; //TODO: Fix MissileNetID, currently it only works when it is 0
            stopAttack.KeepAnimating = false;
            stopAttack.DestroyMissile = true;
            stopAttack.OverrideVisibility = true;
            stopAttack.IsSummonerSpell = isSummonerSpell;
            stopAttack.ForceDoClient = false;
            _packetHandlerManager.BroadcastPacket(stopAttack.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyFogUpdate2(IAttackableUnit u, uint newFogId)
        {
            var fog = new FogUpdate2(u, newFogId);
            _packetHandlerManager.BroadcastPacketTeam(u.Team, fog, Channel.CHL_S2C);
        }

        public void NotifyItemsSwapped(IChampion c, byte fromSlot, byte toSlot)
        {
            var sia = new SwapItemsResponse(c, fromSlot, toSlot);
            _packetHandlerManager.BroadcastPacketVision(c, sia, Channel.CHL_S2C);
        }

        public void NotifyLevelUp(IChampion c)
        {
            var lu = new LevelUp(c);
            _packetHandlerManager.BroadcastPacket(lu, Channel.CHL_S2C);
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

        public void NotifySetTarget(IAttackableUnit attacker, IAttackableUnit target)
        {
            var st = new SetTarget(attacker, target);
            _packetHandlerManager.BroadcastPacket(st, Channel.CHL_S2C);

            var st2 = new SetTarget2(attacker, target);
            _packetHandlerManager.BroadcastPacket(st2, Channel.CHL_S2C);
        }

        public void NotifyChampionDie(IChampion die, IAttackableUnit killer, int goldFromKill)
        {
            var cd = new ChampionDie(die, killer, goldFromKill);
            _packetHandlerManager.BroadcastPacket(cd, Channel.CHL_S2C);

            NotifyChampionDeathTimer(die);
        }

        public void NotifyChampionDeathTimer(IChampion die)
        {
            var cdt = new ChampionDeathTimer(die);
            _packetHandlerManager.BroadcastPacket(cdt, Channel.CHL_S2C);
        }

        public void NotifyChampionRespawn(IChampion c)
        {
            var cr = new ChampionRespawn(c);
            _packetHandlerManager.BroadcastPacket(cr, Channel.CHL_S2C);
        }

        public void NotifyNpcDie(IAttackableUnit die, IAttackableUnit killer)
        {
            var nd = new NpcDie(die, killer);
            _packetHandlerManager.BroadcastPacket(nd, Channel.CHL_S2C);
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

        public void NotifyDebugMessage(string htmlDebugMessage)
        {
            var dm = new DebugMessage(htmlDebugMessage);
            _packetHandlerManager.BroadcastPacket(dm, Channel.CHL_S2C);
        }

        public void NotifyPauseGame(int seconds, bool showWindow)
        {
            var pg = new PauseGame(seconds, showWindow);
            _packetHandlerManager.BroadcastPacket(pg, Channel.CHL_S2C);
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

        private void NotifyAzirTurretSpawned(IAzirTurret azirTurret)
        {
            var spawnPacket = new SpawnAzirTurret(azirTurret);
            _packetHandlerManager.BroadcastPacketVision(azirTurret, spawnPacket, Channel.CHL_S2C);
        }

        public void NotifyMinionSpawned(IMinion minion, TeamId team)
        {
            var spawnPacket = new SpawnMinionS2C();
            spawnPacket.SkinName = minion.Model;
            spawnPacket.Name = minion.Name;
            spawnPacket.VisibilitySize = minion.VisionRadius; // Might be incorrect
            spawnPacket.IsTargetableToTeamSpellFlags = (uint) SpellFlags.TargetableToAll;
            spawnPacket.IsTargetable = true;
            spawnPacket.IsBot = minion.IsBot;
            spawnPacket.IsLaneMinion = minion.IsLaneMinion;
            spawnPacket.IsWard = minion.IsWard;
            spawnPacket.IgnoreCollision = false;
            spawnPacket.TeamID =(ushort) minion.Team;
            // CloneNetID, clones not yet implemented
            spawnPacket.SkinID = 0;
            spawnPacket.Position = new Vector3(minion.GetPosition().X, minion.GetZ(), minion.GetPosition().Y); // check if work, probably not
            spawnPacket.SenderNetID = minion.NetId;
            spawnPacket.NetNodeID = (byte) NetNodeID.Spawned;
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
            md.SyncID = 0x0006E4CF; //TODO: generate real movement SyncId
            visionPacket.MovementData = md;
            visionPacket.Packets.Add(spawnPacket);
            visionPacket.SenderNetID = minion.NetId;
            _packetHandlerManager.BroadcastPacketVision(minion, visionPacket.GetBytes(), Channel.CHL_S2C);
            NotifyEnterLocalVisibilityClient(minion);
            //var spawnPacket = new SpawnMinion(minion);
            //_packetHandlerManager.BroadcastPacketVision(minion, spawnPacket, Channel.CHL_S2C);
        }

        private void NotifyMonsterSpawned(IMonster m)
        {
            var sp = new SpawnMonster(_navGrid, m);
            _packetHandlerManager.BroadcastPacketVision(m, sp, Channel.CHL_S2C);
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
                SyncID = 0x0006E4CF //TODO: generate real movement SyncId
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

        public void NotifyChampionSpawned(IChampion c, TeamId team)
        {
            var hs = new HeroSpawn2(c);
            _packetHandlerManager.BroadcastPacketTeam(team, hs, Channel.CHL_S2C);
        }

        public void NotifySetCooldown(IChampion c, byte slotId, float currentCd, float totalCd)
        {
            var cd = new SetCooldown(c.NetId, slotId, currentCd, totalCd);
            _packetHandlerManager.BroadcastPacket(cd, Channel.CHL_S2C);
        }

        public void NotifyGameTimer(float gameTime)
        {
            var gameTimer = new GameTimer(gameTime / 1000.0f);
            _packetHandlerManager.BroadcastPacket(gameTimer, Channel.CHL_S2C);
        }

        public void NotifyUnitAnnounceEvent(UnitAnnounces messageId, IAttackableUnit target, IGameObject killer = null,
            List<IChampion> assists = null)
        {
            var announce = new UnitAnnounce(messageId, target, killer, assists);
            _packetHandlerManager.BroadcastPacket(announce, Channel.CHL_S2C);
        }

        public void NotifyAnnounceEvent(int mapId, Announces messageId, bool isMapSpecific)
        {
            var announce = new Announce(messageId, isMapSpecific ? mapId : 0);
            _packetHandlerManager.BroadcastPacket(announce, Channel.CHL_S2C);
        }

        public void NotifySpellAnimation(IAttackableUnit u, string animation)
        {
            var sa = new SpellAnimation(u, animation);
            _packetHandlerManager.BroadcastPacketVision(u, sa, Channel.CHL_S2C);
        }

        public void NotifySetAnimation(IAttackableUnit u, List<string> animationPairs)
        {
            var setAnimation = new SetAnimation(u, animationPairs);
            _packetHandlerManager.BroadcastPacketVision(u, setAnimation, Channel.CHL_S2C);
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

        public void NotifyModifyDebugCircleRadius(int userId, uint sender, int objID, float newRadius)
        {
            var debugPacket = new S2C_ModifyDebugCircleRadius();
            debugPacket.SenderNetID = sender;
            debugPacket.ObjectID = objID;
            debugPacket.Radius = newRadius;
            _packetHandlerManager.SendPacket(userId, debugPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyModifyDebugObjectColor(int userId, uint sender, int objID, byte r, byte g, byte b)
        {
            var debugObjPacket = new S2C_ModifyDebugObjectColor();
            debugObjPacket.SenderNetID = sender;
            debugObjPacket.ObjectID = objID;
            var color = new LeaguePackets.Game.Common.Color();
            color.Red = r;
            color.Green = g;
            color.Blue = b;
            debugObjPacket.Color = color;
            _packetHandlerManager.SendPacket(userId, debugObjPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyRemoveDebugObject(int userId, uint sender, int objID)
        {
            var debugObjPacket = new S2C_RemoveDebugObject();
            debugObjPacket.SenderNetID = sender;
            debugObjPacket.ObjectID = objID;
            _packetHandlerManager.SendPacket(userId, debugObjPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifySetDebugHidden(int userId, uint sender, int objID, byte bitfield = 0x0)
        {
            var debugObjPacket = new S2C_SetDebugHidden();
            debugObjPacket.SenderNetID = sender;
            debugObjPacket.ObjectID = objID;
            debugObjPacket.Bitfield = bitfield; // Not sure what this does
            _packetHandlerManager.SendPacket(userId, debugObjPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyCreateUnitHighlight(int userId, IGameObject unit)
        {
            var highlightPacket = new S2C_CreateUnitHighlight();
            highlightPacket.SenderNetID = unit.NetId;
            highlightPacket.TargetNetID = unit.NetId;
            _packetHandlerManager.SendPacket(userId, highlightPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyRemoveUnitHighlight(int userId, IGameObject unit)
        {
            var highlightPacket = new S2C_RemoveUnitHighlight();
            highlightPacket.SenderNetID = unit.NetId;
            highlightPacket.NetID = unit.NetId;
            _packetHandlerManager.SendPacket(userId, highlightPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyUnitSetDrawPathMode(int userId, IAttackableUnit unit, IGameObject target, byte mode)
        {
            var drawPacket = new S2C_UnitSetDrawPathMode();
            drawPacket.SenderNetID = unit.NetId;
            drawPacket.TargetNetID = target.NetId;
            drawPacket.DrawPathMode = 0x1;
            drawPacket.UpdateRate = 0.1f;
            _packetHandlerManager.SendPacket(userId, drawPacket.GetBytes(), Channel.CHL_S2C);
        }

        public void NotifyDebugPacket(int userId, byte[] data)
        {
            _packetHandlerManager.SendPacket(userId, data, Channel.CHL_S2C);
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
    }
}

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
using LeaguePackets.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;
using PingLoadInfoRequest = GameServerCore.Packets.PacketDefinitions.Requests.PingLoadInfoRequest;
using ViewRequest = GameServerCore.Packets.PacketDefinitions.Requests.ViewRequest;

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
            NotifySetHealth(m);
        }

        public void NotifySetHealth(IAttackableUnit u)
        {
            var sh = new SetHealth(u);
            _packetHandlerManager.BroadcastPacketVision(u, sh, Channel.CHL_S2C);
        }

        public void NotifyGameEnd(Vector3 cameraPosition, INexus nexus, List<Pair<uint, ClientInfo>> players)
        {
            var losingTeam = nexus.Team;

            foreach (var p in players)
            {
                var cam = new MoveCamera(p.Item2.Champion, cameraPosition.X, cameraPosition.Y, cameraPosition.Z, 2);
                _packetHandlerManager.SendPacket((int)p.Item2.UserId, cam, Channel.CHL_S2C);
                _packetHandlerManager.SendPacket((int)p.Item2.UserId, new HideUi(), Channel.CHL_S2C);
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
        public void NotifyKeyCheck(long userId, int playerNo)
        {
            var response = new KeyCheckResponse(userId, playerNo);
            _packetHandlerManager.SendPacket((int)userId, response, Channel.CHL_HANDSHAKE);
        }

        public void NotifyPingLoadInfo(PingLoadInfoRequest request, long userId)
        {
            var response = new PingLoadInfoResponse(request.NetId, (uint)request.Position, request.Loaded, request.Unk2,
                request.Ping, request.Unk3, request.Unk4, userId);

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

        public void NotifySurrender(IChampion starter, byte flag, byte yesVotes, byte noVotes, byte maxVotes, TeamId team,
            float timeOut)
        {
            var surrender = new Surrender(starter, flag, yesVotes, noVotes, maxVotes, team, timeOut);
            _packetHandlerManager.BroadcastPacketTeam(team, surrender, Channel.CHL_S2C);
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

        public void NotifyHeroSpawn(int userId, ClientInfo client, int playerId)
        {
            var spawn = new HeroSpawn(client, playerId);
            _packetHandlerManager.SendPacket(userId, spawn, Channel.CHL_S2C);
        }

        public void NotifyAvatarInfo(int userId, ClientInfo client)
        {
            var info = new PacketDefinitions.S2C.AvatarInfo(client);
            _packetHandlerManager.SendPacket(userId, info, Channel.CHL_S2C);
        }

        public void NotifyBuyItem(int userId, IChampion champion, IItem itemInstance)
        {
            var buyItem = new BuyItemResponse(champion, itemInstance);
            _packetHandlerManager.SendPacket(userId, buyItem, Channel.CHL_S2C);
        }

        public void NotifyTurretSpawn(int userId, ILaneTurret turret)
        {
            var turretSpawn = new TurretSpawn(turret);
            _packetHandlerManager.SendPacket(userId, turretSpawn, Channel.CHL_S2C);
        }

        public void NotifySetHealth(int userId, IAttackableUnit unit)
        {
            var setHealthPacket = new SetHealth(unit);
            _packetHandlerManager.SendPacket(userId, setHealthPacket, Channel.CHL_S2C);
        }

        public void NotifyLevelPropSpawn(int userId, ILevelProp levelProp)
        {
            var levelPropSpawnPacket = new LevelPropSpawn(levelProp);
            _packetHandlerManager.SendPacket(userId, levelPropSpawnPacket, Channel.CHL_S2C);
        }

        public void NotifyEnterVision(int userId, IChampion champion)
        {
            var enterVisionPacket = new EnterVisionAgain(_navGrid, champion);
            _packetHandlerManager.SendPacket(userId, enterVisionPacket, Channel.CHL_S2C);
        }

        public void NotifyStaticObjectSpawn(int userId, uint netId)
        {
            var minionSpawnPacket = new MinionSpawn2(netId);
            _packetHandlerManager.SendPacket(userId, minionSpawnPacket, Channel.CHL_S2C);
        }

        public void NotifySetHealth(int userId, uint netId)
        {
            var setHealthPacket = new SetHealth(netId);
            _packetHandlerManager.SendPacket(userId, setHealthPacket, Channel.CHL_S2C);
        }

        public void NotifyProjectileSpawn(int userId, IProjectile projectile)
        {
            var spawnProjectilePacket = new SpawnProjectile(_navGrid, projectile);
            _packetHandlerManager.SendPacket(userId, spawnProjectilePacket, Channel.CHL_S2C);
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

        public void NotifyModifyShield(IAttackableUnit unit, float amount, ShieldType type)
        {
            var ms = new ModifyShield(unit, amount, type);
            _packetHandlerManager.BroadcastPacket(ms, Channel.CHL_S2C);
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

        public void NotifyProjectileSpawn(IProjectile p)
        {
            var sp = new SpawnProjectile(_navGrid, p);
            _packetHandlerManager.BroadcastPacket(sp, Channel.CHL_S2C);
        }

        public void NotifyProjectileDestroy(IProjectile p)
        {
            var dp = new DestroyProjectile(p);
            _packetHandlerManager.BroadcastPacket(dp, Channel.CHL_S2C);
        }

        public void NotifyParticleSpawn(IParticle particle)
        {
            var sp = new SpawnParticle(_navGrid, particle);
            _packetHandlerManager.BroadcastPacket(sp, Channel.CHL_S2C);
        }

        public void NotifyParticleDestroy(IParticle particle)
        {
            var dp = new DestroyParticle(particle);
            _packetHandlerManager.BroadcastPacket(dp, Channel.CHL_S2C);
        }

        public void NotifyModelUpdate(IAttackableUnit obj)
        {
            var mp = new UpdateModel(obj.NetId, obj.Model);
            _packetHandlerManager.BroadcastPacket(mp, Channel.CHL_S2C);
        }

        public void NotifyItemBought(IAttackableUnit u, IItem i)
        {
            var response = new BuyItemResponse(u, i);
            _packetHandlerManager.BroadcastPacketVision(u, response, Channel.CHL_S2C);
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
            var ri = new RemoveItem(c, slot, remaining);
            _packetHandlerManager.BroadcastPacketVision(c, ri, Channel.CHL_S2C);
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

        public void NotifyShowProjectile(IProjectile p)
        {
            var sp = new ShowProjectile(p);
            _packetHandlerManager.BroadcastPacket(sp, Channel.CHL_S2C);
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

        public void NotifyStopAutoAttack(IAttackableUnit attacker)
        {
            var saa = new StopAutoAttack(attacker);
            _packetHandlerManager.BroadcastPacket(saa, Channel.CHL_S2C);
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

            NotifySetHealth(u);
        }

        private void NotifyAzirTurretSpawned(IAzirTurret azirTurret)
        {
            var spawnPacket = new SpawnAzirTurret(azirTurret);
            _packetHandlerManager.BroadcastPacketVision(azirTurret, spawnPacket, Channel.CHL_S2C);
        }

        public void NotifyMinionSpawned(IMinion minion, TeamId team)
        {
            var spawnPacket = new LeaguePackets.GamePackets.SpawnMinionS2C();
            spawnPacket.NetID = (NetID)minion.NetId;
            spawnPacket.SenderNetID = (NetID)minion.NetId;
            if (minion.Owner == null) {
                spawnPacket.OwnerNetID = (NetID)minion.NetId;
            }
            else {
                spawnPacket.OwnerNetID = (NetID)minion.Owner.NetId;
            }
            spawnPacket.NetNodeID = NetNodeID.Spawned;
            spawnPacket.Position = new Vector3(minion.GetPosition().X, minion.GetZ(), minion.GetPosition().Y); // check if work, probably not
            spawnPacket.TeamID = (TeamID)minion.Team;
            spawnPacket.Name = minion.Name;
            spawnPacket.SkinName = minion.Model;
            spawnPacket.InitialLevel = 1;
            spawnPacket.VisibilitySize = 0;
            spawnPacket.SkinID = 0;
            spawnPacket.IsTargetable = true;
            spawnPacket.IsWard = true;
            spawnPacket.IsTargetableToTeam = SpellFlags.TargetableToAll;
            var visionPacket = new LeaguePackets.GamePackets.OnEnterVisiblityClient();
            var vd = new LeaguePackets.CommonData.VisibilityDataAIMinion();
            vd.LookAtPosition = new Vector3(1, 0, 0);
            var md = new LeaguePackets.CommonData.MovementDataStop();
            md.Position = minion.GetPosition();
            md.Forward = new Vector2(0, 1);
            vd.MovementSyncID = 0x0006E4CF;
            vd.MovementData = md;
            visionPacket.VisibilityData = vd;
            visionPacket.Packets.Add(spawnPacket);
            visionPacket.SenderNetID = (NetID)minion.NetId;
            _packetHandlerManager.BroadcastPacketVision(minion, visionPacket.GetBytes(), Channel.CHL_S2C);
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

        public void NotifyEnterVision(IGameObject o, TeamId team)
        {
            switch (o)
            {
                case IMinion m:
                    {
                        var eva = new EnterVisionAgain(_navGrid, m);
                        _packetHandlerManager.BroadcastPacketTeam(team, eva, Channel.CHL_S2C);
                        NotifySetHealth(m);
                        return;
                    }
                // TODO: Fix bug where enemy champion is not visible to user when vision is acquired until the enemy champion moves
                case IChampion c:
                    {
                        var eva = new EnterVisionAgain(_navGrid, c);
                        _packetHandlerManager.BroadcastPacketTeam(team, eva, Channel.CHL_S2C);
                        NotifySetHealth(c);
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

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Timers;
using ENet;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Logic;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Logic.GameObjects.Missiles;
using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.GameObjects.Spells;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Packets.Enums;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;
using PacketDefinitions420.Enums;
using PingLoadInfoRequest = LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests.PingLoadInfoRequest;
using ViewRequest = LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests.ViewRequest;

namespace PacketDefinitions420
{
    public class PacketNotifier : IPacketNotifier
    {
        private readonly IPacketHandlerManager _packetHandlerManager;
        private readonly NavGrid _navGrid;
        private readonly PlayerManager _playerManager;
        private readonly NetworkIdManager _networkIdManager;

        public PacketNotifier(IPacketHandlerManager packetHandlerManager, NavGrid navGrid, PlayerManager playerManager, NetworkIdManager networkIdManager)
        {
            _packetHandlerManager = packetHandlerManager;
            _navGrid = navGrid;
            _playerManager = playerManager;
            _networkIdManager = networkIdManager;
        }

        public void NotifyMinionSpawned(Minion m, TeamId team)
        {
            var ms = new MinionSpawn(_navGrid, m);
            _packetHandlerManager.BroadcastPacketTeam(team, ms, Channel.CHL_S2C);
            NotifySetHealth(m);
        }

        public void NotifySetHealth(AttackableUnit u)
        {
            var sh = new SetHealth(u);
            _packetHandlerManager.BroadcastPacketVision(u, sh, Channel.CHL_S2C);
        }

        public void NotifyGameEnd(Vector3 cameraPosition, Nexus nexus)
        {
            var losingTeam = nexus.Team;

            foreach (var p in _playerManager.GetPlayers())
            {
                var cam = new MoveCamera(p.Item2.Champion, cameraPosition.X, cameraPosition.Y, cameraPosition.Z, 2);
                _packetHandlerManager.SendPacket(p.Item2.Peer, cam, Channel.CHL_S2C);
                _packetHandlerManager.SendPacket(p.Item2.Peer, new HideUi(), Channel.CHL_S2C);
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

        public void NotifyUpdatedStats(AttackableUnit u, bool partial = true)
        {
            if (u.Replication != null)
            {
                var us = new UpdateStats(u.Replication, partial);
                var channel = Channel.CHL_LOW_PRIORITY;
                _packetHandlerManager.BroadcastPacketVision(u, us, channel, PacketFlags.Unsequenced);
                if (partial)
                {
                    foreach (var x in u.Replication.Values)
                    {
                        if (x != null)
                        {
                            x.Changed = false;
                        }
                    }
                    u.Replication.Changed = false;
                }
            }
        }

        public void NotifyPing(ClientInfo client, float x, float y, int targetNetId, Pings type)
        {
            var ping = new AttentionPingRequest(x, y, targetNetId, type);
            var response = new AttentionPingResponse(client, ping);
            _packetHandlerManager.BroadcastPacketTeam(client.Team, response, Channel.CHL_S2C);
        }

        public void NotifyTint(TeamId team, bool enable, float speed, byte r, byte g, byte b, float a)
        {
            var tint = new SetScreenTint(team, enable, speed, r, g, b, a);
            _packetHandlerManager.BroadcastPacket(tint, Channel.CHL_S2C);
        }

        public void NotifySkillUp(Peer peer, uint netId, byte skill, byte level, byte pointsLeft)
        {
            var skillUpResponse = new SkillUpResponse(netId, skill, level, pointsLeft);
            _packetHandlerManager.SendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
        }

        public void NotifySetTeam(AttackableUnit unit, TeamId team)
        {
            var p = new SetTeam(unit, team);
            _packetHandlerManager.BroadcastPacket(p, Channel.CHL_S2C);
        }

        public void NotifyCastSpell(NavGrid navGrid, Spell s, float x, float y, float xDragEnd, float yDragEnd, uint futureProjNetId,
            uint spellNetId)
        {

            var response = new CastSpellResponse(navGrid, s, x, y, xDragEnd, yDragEnd, futureProjNetId, spellNetId);
            _packetHandlerManager.BroadcastPacket(response, Channel.CHL_S2C);
        }

        public void NotifyBlueTip(Peer peer, string title, string text, string imagePath, byte tipCommand, uint playerNetId,
            uint targetNetId)
        {
            var packet = new BlueTip(title, text, imagePath, tipCommand, playerNetId, targetNetId);
            _packetHandlerManager.SendPacket(peer, packet, Channel.CHL_S2C);
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

        public void NotifyKeyCheck(long userId, int playerNo)
        {
            var response = new KeyCheckResponse(userId, playerNo);
            _packetHandlerManager.BroadcastPacket(response, Channel.CHL_HANDSHAKE);
        }

        public void NotifyKeyCheck(Peer peer, long userId, int playerNo)
        {
            var response = new KeyCheckResponse(userId, playerNo);
            _packetHandlerManager.SendPacket(peer, response, Channel.CHL_HANDSHAKE);
        }

        public void NotifyPingLoadInfo(PingLoadInfoRequest request, long userId)
        {
            var response = new PingLoadInfoResponse(request.NetId, (uint)request.Position, request.Loaded, request.Unk2,
                request.Ping, request.Unk3, request.Unk4, userId);

            //Logging->writeLine("loaded: %f, ping: %f, %f", loadInfo->loaded, loadInfo->ping, loadInfo->f3);
            _packetHandlerManager.BroadcastPacket(response, Channel.CHL_LOW_PRIORITY, PacketFlags.None);
        }

        public void NotifyViewResponse(Peer peer, ViewRequest request)
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

            _packetHandlerManager.SendPacket(peer, answer, Channel.CHL_S2C, PacketFlags.None);
        }

        public void NotifySynchVersion(Peer peer, List<Pair<uint, ClientInfo>> players, string version, string gameMode, int mapId)
        {
            var response = new SynchVersionResponse(_playerManager.GetPlayers(), Config.VERSION_STRING, "CLASSIC", mapId);
            _packetHandlerManager.SendPacket(peer, response, Channel.CHL_S2C);
        }

        public void NotifyLoadScreenInfo(Peer peer, List<Pair<uint, ClientInfo>> getPlayers)
        {
            var screenInfo = new LoadScreenInfo(_playerManager.GetPlayers());
            _packetHandlerManager.SendPacket(peer, screenInfo, Channel.CHL_LOADING_SCREEN);
        }

        public void NotifyLoadScreenPlayerName(Peer peer, Pair<uint, ClientInfo> player)
        {
            var loadName = new LoadScreenPlayerName(player);
            _packetHandlerManager.SendPacket(peer, loadName, Channel.CHL_LOADING_SCREEN);
        }

        public void NotifyLoadScreenPlayerChampion(Peer peer, Pair<uint, ClientInfo> player)
        {
            var loadChampion = new LoadScreenPlayerChampion(player);
            _packetHandlerManager.SendPacket(peer, loadChampion, Channel.CHL_LOADING_SCREEN);
        }

        public void NotifyQueryStatus(Peer peer)
        {
            var response = new QueryStatus();
            _packetHandlerManager.SendPacket(peer, response, Channel.CHL_S2C);
        }

        public void NotifyPlayerStats(Champion champion)
        {
            var response = new PlayerStats(champion);
            // TODO: research how to send the packet
            _packetHandlerManager.BroadcastPacket(response, Channel.CHL_S2C);
        }

        public void NotifySurrender(Champion starter, byte flag, byte yesVotes, byte noVotes, byte maxVotes, TeamId team,
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

        public void NotifyHeroSpawn2(Peer peer, Champion champion)
        {
            var heroSpawnPacket = new HeroSpawn2(champion);
            _packetHandlerManager.SendPacket(peer, heroSpawnPacket, Channel.CHL_S2C);
        }

        public void NotifyGameTimer(Peer peer, float time)
        {
            var timer = new GameTimer(time / 1000.0f);
            _packetHandlerManager.SendPacket(peer, timer, Channel.CHL_S2C);
        }

        public void NotifyGameTimerUpdate(Peer peer, float time)
        {
            var timer = new GameTimerUpdate(time / 1000.0f);
            _packetHandlerManager.SendPacket(peer, timer, Channel.CHL_S2C);
        }

        public void NotifySpawnStart(Peer peer)
        {
            var start = new StatePacket2(PacketCmd.PKT_S2C_START_SPAWN);
            _packetHandlerManager.SendPacket(peer, start, Channel.CHL_S2C);
        }

        public void NotifySpawnEnd(Peer peer)
        {
            var endSpawnPacket = new StatePacket(PacketCmd.PKT_S2C_END_SPAWN);
            _packetHandlerManager.SendPacket(peer, endSpawnPacket, Channel.CHL_S2C);
        }

        public void NotifyHeroSpawn(Peer peer, ClientInfo client, int playerId)
        {
            var spawn = new HeroSpawn(client, playerId);
            _packetHandlerManager.SendPacket(peer, spawn, Channel.CHL_S2C);
        }

        public void NotifyAvatarInfo(Peer peer, ClientInfo client)
        {
            var info = new AvatarInfo(client);
            _packetHandlerManager.SendPacket(peer, info, Channel.CHL_S2C);
        }

        public void NotifyBuyItem(Peer peer, Champion champion, Item itemInstance)
        {
            var buyItem = new BuyItemResponse(champion, itemInstance);
            _packetHandlerManager.SendPacket(peer, buyItem, Channel.CHL_S2C);
        }

        public void NotifyTurretSpawn(Peer peer, LaneTurret turret)
        {
            var turretSpawn = new TurretSpawn(turret);
            _packetHandlerManager.SendPacket(peer, turretSpawn, Channel.CHL_S2C);
        }

        public void NotifySetHealth(Peer peer, AttackableUnit unit)
        {
            var setHealthPacket = new SetHealth(unit);
            _packetHandlerManager.SendPacket(peer, setHealthPacket, Channel.CHL_S2C);
        }

        public void NotifyLevelPropSpawn(Peer peer, LevelProp levelProp)
        {
            var levelPropSpawnPacket = new LevelPropSpawn(levelProp);
            _packetHandlerManager.SendPacket(peer, levelPropSpawnPacket, Channel.CHL_S2C);
        }

        public void NotifyEnterVision(Peer peer, Champion champion)
        {
            var enterVisionPacket = new EnterVisionAgain(_navGrid, champion);
            _packetHandlerManager.SendPacket(peer, enterVisionPacket, Channel.CHL_S2C);
        }

        public void NotifyStaticObjectSpawn(Peer peer, uint netId)
        {
            var minionSpawnPacket = new MinionSpawn2(netId);
            _packetHandlerManager.SendPacket(peer, minionSpawnPacket, Channel.CHL_S2C);
        }

        public void NotifySetHealth(Peer peer, uint netId)
        {
            var setHealthPacket = new SetHealth(netId);
            _packetHandlerManager.SendPacket(peer, setHealthPacket, Channel.CHL_S2C);
        }

        public void NotifyProjectileSpawn(Peer peer, Projectile projectile)
        {
            var spawnProjectilePacket = new SpawnProjectile(_navGrid, projectile);
            _packetHandlerManager.SendPacket(peer, spawnProjectilePacket, Channel.CHL_S2C);
        }

        public void NotifyFaceDirection(AttackableUnit u, Vector2 direction, bool isInstant = true, float turnTime = 0.0833f)
        {
            var height = _navGrid.GetHeightAtLocation(direction);
            var fd = new FaceDirection(u, direction.X, direction.Y, height, isInstant, turnTime);
            _packetHandlerManager.BroadcastPacketVision(u, fd, Channel.CHL_S2C);
        }

        public void NotifyInhibitorState(Inhibitor inhibitor, GameObject killer = null, List<Champion> assists = null)
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

        public void NotifyInhibitorSpawningSoon(Inhibitor inhibitor)
        {
            var packet = new UnitAnnounce(UnitAnnounces.INHIBITOR_ABOUT_TO_SPAWN, inhibitor);
            _packetHandlerManager.BroadcastPacket(packet, Channel.CHL_S2C);
        }

        public void NotifyAddBuff(Buff b)
        {
            var add = new AddBuff(b.TargetUnit, b.SourceUnit, b.Stacks, b.Duration, b.BuffType, b.Name, b.Slot);
            _packetHandlerManager.BroadcastPacket(add, Channel.CHL_S2C);
        }

        public void NotifyDebugMessage(Peer peer, string message)
        {
            var dm = new DebugMessage(message);
            _packetHandlerManager.SendPacket(peer, dm, Channel.CHL_S2C);
        }

        public void NotifyDebugMessage(TeamId team, string message)
        {
            var dm = new DebugMessage(message);
            _packetHandlerManager.BroadcastPacketTeam(team, dm, Channel.CHL_S2C);
        }

        public void NotifyEditBuff(Buff b, int stacks)
        {
            var edit = new EditBuff(b.TargetUnit, b.Slot, (byte)b.Stacks);
            _packetHandlerManager.BroadcastPacket(edit, Channel.CHL_S2C);
        }

        public void NotifyRemoveBuff(AttackableUnit u, string buffName, byte slot = 0x01)
        {
            var remove = new RemoveBuff(u, buffName, slot);
            _packetHandlerManager.BroadcastPacket(remove, Channel.CHL_S2C);
        }

        public void NotifyTeleport(AttackableUnit u, float x, float y)
        {
            // Can't teleport to this point of the map
            if (!_navGrid.IsWalkable(x, y))
            {
                x = MovementVector.TargetXToNormalFormat(_navGrid, u.X);
                y = MovementVector.TargetYToNormalFormat(_navGrid, u.Y);
            }
            else
            {
                u.SetPosition(x, y);

                //TeleportRequest first(u.NetId, u.teleportToX, u.teleportToY, true);
                //sendPacket(currentPeer, first, Channel.CHL_S2C);

                x = MovementVector.TargetXToNormalFormat(_navGrid, x);
                y = MovementVector.TargetYToNormalFormat(_navGrid, y);
            }

            var second = new TeleportRequest(u.NetId, x, y, false);
            _packetHandlerManager.BroadcastPacketVision(u, second, Channel.CHL_S2C);
        }

        public void NotifyMovement(GameObject o)
        {
            var answer = new MovementResponse(_navGrid, o);
            _packetHandlerManager.BroadcastPacketVision(o, answer, Channel.CHL_LOW_PRIORITY);
        }

        public void NotifyDamageDone(AttackableUnit source, AttackableUnit target, float amount, DamageType type, DamageText damagetext)
        {
            var dd = new DamageDone(source, target, amount, type, damagetext);
            _packetHandlerManager.BroadcastPacket(dd, Channel.CHL_S2C);
        }

        public void NotifyModifyShield(AttackableUnit unit, float amount, ShieldType type)
        {
            var ms = new ModifyShield(unit, amount, type);
            _packetHandlerManager.BroadcastPacket(ms, Channel.CHL_S2C);
        }

        public void NotifyBeginAutoAttack(AttackableUnit attacker, AttackableUnit victim, uint futureProjNetId, bool isCritical)
        {
            var aa = new BeginAutoAttack(_navGrid, attacker, victim, futureProjNetId, isCritical);
            _packetHandlerManager.BroadcastPacket(aa, Channel.CHL_S2C);
        }

        public void NotifyNextAutoAttack(AttackableUnit attacker, AttackableUnit target, uint futureProjNetId, bool isCritical,
            bool nextAttackFlag)
        {
            var aa = new NextAutoAttack(attacker, target, futureProjNetId, isCritical, nextAttackFlag);
            _packetHandlerManager.BroadcastPacket(aa, Channel.CHL_S2C);
        }

        public void NotifyOnAttack(AttackableUnit attacker, AttackableUnit attacked, AttackType attackType)
        {
            var oa = new OnAttack(attacker, attacked, attackType);
            _packetHandlerManager.BroadcastPacket(oa, Channel.CHL_S2C);
        }

        public void NotifyProjectileSpawn(Projectile p)
        {
            var sp = new SpawnProjectile(_navGrid, p);
            _packetHandlerManager.BroadcastPacket(sp, Channel.CHL_S2C);
        }

        public void NotifyProjectileDestroy(Projectile p)
        {
            var dp = new DestroyProjectile(p);
            _packetHandlerManager.BroadcastPacket(dp, Channel.CHL_S2C);
        }

        public void NotifyParticleSpawn(Particle particle)
        {
            var sp = new SpawnParticle(_navGrid, particle);
            _packetHandlerManager.BroadcastPacket(sp, Channel.CHL_S2C);
        }

        public void NotifyParticleDestroy(Particle particle)
        {
            var dp = new DestroyParticle(particle);
            _packetHandlerManager.BroadcastPacket(dp, Channel.CHL_S2C);
        }

        public void NotifyModelUpdate(AttackableUnit obj)
        {
            var mp = new UpdateModel(obj.NetId, obj.Model);
            _packetHandlerManager.BroadcastPacket(mp, Channel.CHL_S2C);
        }

        public void NotifyItemBought(AttackableUnit u, Item i)
        {
            var response = new BuyItemResponse(u, i);
            _packetHandlerManager.BroadcastPacketVision(u, response, Channel.CHL_S2C);
        }

        public void NotifyFogUpdate2(AttackableUnit u)
        {
            var fog = new FogUpdate2(u, _networkIdManager);
            _packetHandlerManager.BroadcastPacketTeam(u.Team, fog, Channel.CHL_S2C);
        }

        public void NotifyItemsSwapped(Champion c, byte fromSlot, byte toSlot)
        {
            var sia = new SwapItemsResponse(c, fromSlot, toSlot);
            _packetHandlerManager.BroadcastPacketVision(c, sia, Channel.CHL_S2C);
        }

        public void NotifyLevelUp(Champion c)
        {
            var lu = new LevelUp(c);
            _packetHandlerManager.BroadcastPacket(lu, Channel.CHL_S2C);
        }

        public void NotifyRemoveItem(Champion c, byte slot, byte remaining)
        {
            var ri = new RemoveItem(c, slot, remaining);
            _packetHandlerManager.BroadcastPacketVision(c, ri, Channel.CHL_S2C);
        }

        public void NotifySetTarget(AttackableUnit attacker, AttackableUnit target)
        {
            var st = new SetTarget(attacker, target);
            _packetHandlerManager.BroadcastPacket(st, Channel.CHL_S2C);

            var st2 = new SetTarget2(attacker, target);
            _packetHandlerManager.BroadcastPacket(st2, Channel.CHL_S2C);
        }

        public void NotifyChampionDie(Champion die, AttackableUnit killer, int goldFromKill)
        {
            var cd = new ChampionDie(die, killer, goldFromKill);
            _packetHandlerManager.BroadcastPacket(cd, Channel.CHL_S2C);

            NotifyChampionDeathTimer(die);
        }

        public void NotifyChampionDeathTimer(Champion die)
        {
            var cdt = new ChampionDeathTimer(die);
            _packetHandlerManager.BroadcastPacket(cdt, Channel.CHL_S2C);
        }

        public void NotifyChampionRespawn(Champion c)
        {
            var cr = new ChampionRespawn(c);
            _packetHandlerManager.BroadcastPacket(cr, Channel.CHL_S2C);
        }

        public void NotifyShowProjectile(Projectile p)
        {
            var sp = new ShowProjectile(p);
            _packetHandlerManager.BroadcastPacket(sp, Channel.CHL_S2C);
        }

        public void NotifyNpcDie(AttackableUnit die, AttackableUnit killer)
        {
            var nd = new NpcDie(die, killer);
            _packetHandlerManager.BroadcastPacket(nd, Channel.CHL_S2C);
        }

        public void NotifyAddGold(Champion c, AttackableUnit died, float gold)
        {
            var ag = new AddGold(c, died, gold);
            _packetHandlerManager.BroadcastPacket(ag, Channel.CHL_S2C);
        }

        public void NotifyAddXp(Champion champion, float experience)
        {
            var xp = new AddXp(champion, experience);
            _packetHandlerManager.BroadcastPacket(xp, Channel.CHL_S2C);
        }

        public void NotifyStopAutoAttack(AttackableUnit attacker)
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

        public void NotifyResumeGame(AttackableUnit unpauser, bool showWindow)
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

        public void NotifySpawn(AttackableUnit u)
        {
            switch (u)
            {
                case Minion m:
                    NotifyMinionSpawned(m, CustomConvert.GetEnemyTeam(m.Team));
                    break;
                case Champion c:
                    NotifyChampionSpawned(c, CustomConvert.GetEnemyTeam(c.Team));
                    break;
                case Monster monster:
                    NotifyMonsterSpawned(monster);
                    break;
                case Placeable placeable:
                    NotifyPlaceableSpawned(placeable);
                    break;
                case AzirTurret azirTurret:
                    NotifyAzirTurretSpawned(azirTurret);
                    break;
            }

            NotifySetHealth(u);
        }

        private void NotifyAzirTurretSpawned(AzirTurret azirTurret)
        {
            var spawnPacket = new SpawnAzirTurret(azirTurret);
            _packetHandlerManager.BroadcastPacketVision(azirTurret, spawnPacket, Channel.CHL_S2C);
        }

        private void NotifyPlaceableSpawned(Placeable placeable)
        {
            var spawnPacket = new SpawnPlaceable(placeable);
            _packetHandlerManager.BroadcastPacketVision(placeable, spawnPacket, Channel.CHL_S2C);
        }

        private void NotifyMonsterSpawned(Monster m)
        {
            var sp = new SpawnMonster(_navGrid, m);
            _packetHandlerManager.BroadcastPacketVision(m, sp, Channel.CHL_S2C);
        }

        public void NotifyLeaveVision(GameObject o, TeamId team)
        {
            var lv = new LeaveVision(o);
            _packetHandlerManager.BroadcastPacketTeam(team, lv, Channel.CHL_S2C);

            // Not exactly sure what this is yet
            var c = o as Champion;
            if (o == null)
            {
                var deleteObj = new DeleteObjectFromVision(o);
                _packetHandlerManager.BroadcastPacketTeam(team, deleteObj, Channel.CHL_S2C);
            }
        }

        public void NotifyEnterVision(GameObject o, TeamId team)
        {
            switch (o)
            {
                case Minion m:
                    {
                        var eva = new EnterVisionAgain(_navGrid, m);
                        _packetHandlerManager.BroadcastPacketTeam(team, eva, Channel.CHL_S2C);
                        NotifySetHealth(m);
                        return;
                    }
                // TODO: Fix bug where enemy champion is not visible to user when vision is acquired until the enemy champion moves
                case Champion c:
                    {
                        var eva = new EnterVisionAgain(_navGrid, c);
                        _packetHandlerManager.BroadcastPacketTeam(team, eva, Channel.CHL_S2C);
                        NotifySetHealth(c);
                        break;
                    }
            }
        }

        public void NotifyChampionSpawned(Champion c, TeamId team)
        {
            var hs = new HeroSpawn2(c);
            _packetHandlerManager.BroadcastPacketTeam(team, hs, Channel.CHL_S2C);
        }

        public void NotifySetCooldown(Champion c, byte slotId, float currentCd, float totalCd)
        {
            var cd = new SetCooldown(c.NetId, slotId, currentCd, totalCd);
            _packetHandlerManager.BroadcastPacket(cd, Channel.CHL_S2C);
        }

        public void NotifyGameTimer(float gameTime)
        {
            var gameTimer = new GameTimer(gameTime / 1000.0f);
            _packetHandlerManager.BroadcastPacket(gameTimer, Channel.CHL_S2C);
        }

        public void NotifyUnitAnnounceEvent(UnitAnnounces messageId, AttackableUnit target, GameObject killer = null,
            List<Champion> assists = null)
        {
            var announce = new UnitAnnounce(messageId, target, killer, assists);
            _packetHandlerManager.BroadcastPacket(announce, Channel.CHL_S2C);
        }

        public void NotifyAnnounceEvent(int mapId, Announces messageId, bool isMapSpecific)
        {
            var announce = new LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C.Announce(messageId, isMapSpecific ? mapId : 0);
            _packetHandlerManager.BroadcastPacket(announce, Channel.CHL_S2C);
        }

        public void NotifySpellAnimation(AttackableUnit u, string animation)
        {
            var sa = new SpellAnimation(u, animation);
            _packetHandlerManager.BroadcastPacketVision(u, sa, Channel.CHL_S2C);
        }

        public void NotifySetAnimation(AttackableUnit u, List<string> animationPairs)
        {
            var setAnimation = new SetAnimation(u, animationPairs);
            _packetHandlerManager.BroadcastPacketVision(u, setAnimation, Channel.CHL_S2C);
        }

        public void NotifyDash(AttackableUnit u,
                               Target t,
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

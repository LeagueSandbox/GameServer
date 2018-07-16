using System.Collections.Generic;
using System.Numerics;
using System.Timers;
using ENet;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Logic.GameObjects.Missiles;
using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.GameObjects.Spells;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;
using Announce = LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C.Announce;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class PacketNotifier
    {
        private Game _game;
        private PlayerManager _playerManager;
        private readonly NetworkIdManager _networkIdManager;

        public PacketNotifier(Game game)
        {
            _game = game;
            _playerManager = game.GetPlayerManager();
            _networkIdManager = game.GetNetworkManager();
        }

        public void NotifyMinionSpawned(Minion m, TeamId team)
        {
            var ms = new MinionSpawn(_game, m);
            _game.PacketHandlerManager.BroadcastPacketTeam(team, ms, Channel.CHL_S2_C);
            NotifySetHealth(m);
        }

        public void NotifySetHealth(AttackableUnit u)
        {
            var sh = new SetHealth(_game, u);
            _game.PacketHandlerManager.BroadcastPacketVision(u, sh, Channel.CHL_S2_C);
        }

        public void NotifyGameEnd(Nexus nexus)
        {
            var losingTeam = nexus.Team;

            foreach (var p in _playerManager.GetPlayers())
            {
                var coords = _game.Map.MapGameScript.GetEndGameCameraPosition(losingTeam);
                var cam = new MoveCamera(_game, p.Item2.Champion, coords.X, coords.Y, coords.Z, 2);
                _game.PacketHandlerManager.SendPacket(p.Item2.Peer, cam, Channel.CHL_S2_C);
                _game.PacketHandlerManager.SendPacket(p.Item2.Peer, new HideUi(_game), Channel.CHL_S2_C);
            }

            _game.PacketHandlerManager.BroadcastPacket(new ExplodeNexus(_game, nexus), Channel.CHL_S2_C);

            var timer = new Timer(5000) { AutoReset = false };
            timer.Elapsed += (a, b) =>
            {
                var gameEndPacket = new GameEnd(_game, losingTeam != TeamId.TEAM_BLUE);
                _game.PacketHandlerManager.BroadcastPacket(gameEndPacket, Channel.CHL_S2_C);
            };
            timer.Start();
            _game.SetGameToExit();
        }

        public void NotifyUpdatedStats(AttackableUnit u, bool partial = true)
        {
            if (u.Replication != null)
            {
                var us = new UpdateStats(_game, u.Replication, partial);
                var channel = Channel.CHL_LOW_PRIORITY;
                _game.PacketHandlerManager.BroadcastPacketVision(u, us, channel, PacketFlags.Unsequenced);
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

        public void NotifyFaceDirection(AttackableUnit u, Vector2 direction, bool isInstant = true, float turnTime = 0.0833f)
        {
            var height = _game.Map.NavGrid.GetHeightAtLocation(direction);
            var fd = new FaceDirection(_game, u, direction.X, direction.Y, height, isInstant, turnTime);
            _game.PacketHandlerManager.BroadcastPacketVision(u, fd, Channel.CHL_S2_C);
        }

        public void NotifyInhibitorState(Inhibitor inhibitor, GameObject killer = null, List<Champion> assists = null)
        {
            UnitAnnounce announce;
            switch (inhibitor.InhibitorState)
            {
                case InhibitorState.DEAD:
                    announce = new UnitAnnounce(_game, UnitAnnounces.INHIBITOR_DESTROYED, inhibitor, killer, assists);
                    _game.PacketHandlerManager.BroadcastPacket(announce, Channel.CHL_S2_C);

                    var anim = new InhibitorDeathAnimation(_game, inhibitor, killer);
                    _game.PacketHandlerManager.BroadcastPacket(anim, Channel.CHL_S2_C);
                    break;
                case InhibitorState.ALIVE:
                    announce = new UnitAnnounce(_game, UnitAnnounces.INHIBITOR_SPAWNED, inhibitor, killer, assists);
                    _game.PacketHandlerManager.BroadcastPacket(announce, Channel.CHL_S2_C);
                    break;
            }
            var packet = new InhibitorStateUpdate(_game, inhibitor);
            _game.PacketHandlerManager.BroadcastPacket(packet, Channel.CHL_S2_C);
        }

        public void NotifyInhibitorSpawningSoon(Inhibitor inhibitor)
        {
            var packet = new UnitAnnounce(_game, UnitAnnounces.INHIBITOR_ABOUT_TO_SPAWN, inhibitor);
            _game.PacketHandlerManager.BroadcastPacket(packet, Channel.CHL_S2_C);
        }

        public void NotifyAddBuff(Buff b)
        {
            var add = new AddBuff(_game, b.TargetUnit, b.SourceUnit, b.Stacks, b.Duration, b.BuffType, b.Name, b.Slot);
            _game.PacketHandlerManager.BroadcastPacket(add, Channel.CHL_S2_C);
        }

        public void NotifyEditBuff(Buff b, int stacks)
        {
            var edit = new EditBuff(_game, b.TargetUnit, b.Slot, (byte)b.Stacks);
            _game.PacketHandlerManager.BroadcastPacket(edit, Channel.CHL_S2_C);
        }

        public void NotifyRemoveBuff(AttackableUnit u, string buffName, byte slot = 0x01)
        {
            var remove = new RemoveBuff(_game, u, buffName, slot);
            _game.PacketHandlerManager.BroadcastPacket(remove, Channel.CHL_S2_C);
        }

        public void NotifyTeleport(AttackableUnit u, float x, float y)
        {
            // Can't teleport to this point of the map
            if (!_game.Map.NavGrid.IsWalkable(x, y))
            {
                x = MovementVector.TargetXToNormalFormat(_game, u.X);
                y = MovementVector.TargetYToNormalFormat(_game, u.Y);
            }
            else
            {
                u.SetPosition(x, y);

                //TeleportRequest first(u.NetId, u.teleportToX, u.teleportToY, true);
                //sendPacket(currentPeer, first, Channel.CHL_S2C);

                x = MovementVector.TargetXToNormalFormat(_game, x);
                y = MovementVector.TargetYToNormalFormat(_game, y);
            }

            var second = new TeleportRequest(_game, u.NetId, x, y, false);
            _game.PacketHandlerManager.BroadcastPacketVision(u, second, Channel.CHL_S2_C);
        }

        public void NotifyMovement(GameObject o)
        {
            var answer = new MovementResponse(_game, o);
            _game.PacketHandlerManager.BroadcastPacketVision(o, answer, Channel.CHL_LOW_PRIORITY);
        }

        public void NotifyDamageDone(AttackableUnit source, AttackableUnit target, float amount, DamageType type, DamageText damagetext)
        {
            var dd = new DamageDone(_game, source, target, amount, type, damagetext);
            _game.PacketHandlerManager.BroadcastPacket(dd, Channel.CHL_S2_C);
        }

        public void NotifyModifyShield(AttackableUnit unit, float amount, ShieldType type)
        {
            var ms = new ModifyShield(_game, unit, amount, type);
            _game.PacketHandlerManager.BroadcastPacket(ms, Channel.CHL_S2_C);
        }

        public void NotifyBeginAutoAttack(AttackableUnit attacker, AttackableUnit victim, uint futureProjNetId, bool isCritical)
        {
            var aa = new BeginAutoAttack(_game, attacker, victim, futureProjNetId, isCritical);
            _game.PacketHandlerManager.BroadcastPacket(aa, Channel.CHL_S2_C);
        }

        public void NotifyNextAutoAttack(AttackableUnit attacker, AttackableUnit target, uint futureProjNetId, bool isCritical,
            bool nextAttackFlag)
        {
            var aa = new NextAutoAttack(_game, attacker, target, futureProjNetId, isCritical, nextAttackFlag);
            _game.PacketHandlerManager.BroadcastPacket(aa, Channel.CHL_S2_C);
        }

        public void NotifyOnAttack(AttackableUnit attacker, AttackableUnit attacked, AttackType attackType)
        {
            var oa = new OnAttack(_game, attacker, attacked, attackType);
            _game.PacketHandlerManager.BroadcastPacket(oa, Channel.CHL_S2_C);
        }

        public void NotifyProjectileSpawn(Projectile p)
        {
            var sp = new SpawnProjectile(_game, p);
            _game.PacketHandlerManager.BroadcastPacket(sp, Channel.CHL_S2_C);
        }

        public void NotifyProjectileDestroy(Projectile p)
        {
            var dp = new DestroyProjectile(_game, p);
            _game.PacketHandlerManager.BroadcastPacket(dp, Channel.CHL_S2_C);
        }

        public void NotifyParticleSpawn(Particle particle)
        {
            var sp = new SpawnParticle(_game, particle);
            _game.PacketHandlerManager.BroadcastPacket(sp, Channel.CHL_S2_C);
        }

        public void NotifyParticleDestroy(Particle particle)
        {
            var dp = new DestroyParticle(_game, particle);
            _game.PacketHandlerManager.BroadcastPacket(dp, Channel.CHL_S2_C);
        }

        public void NotifyModelUpdate(AttackableUnit obj)
        {
            var mp = new UpdateModel(_game, obj.NetId, obj.Model);
            _game.PacketHandlerManager.BroadcastPacket(mp, Channel.CHL_S2_C);
        }

        public void NotifyItemBought(AttackableUnit u, Item i)
        {
            var response = new BuyItemResponse(_game, u, i);
            _game.PacketHandlerManager.BroadcastPacketVision(u, response, Channel.CHL_S2_C);
        }

        public void NotifyFogUpdate2(AttackableUnit u)
        {
            var fog = new FogUpdate2(_game, u, _networkIdManager);
            _game.PacketHandlerManager.BroadcastPacketTeam(u.Team, fog, Channel.CHL_S2_C);
        }

        public void NotifyItemsSwapped(Champion c, byte fromSlot, byte toSlot)
        {
            var sia = new SwapItemsResponse(_game, c, fromSlot, toSlot);
            _game.PacketHandlerManager.BroadcastPacketVision(c, sia, Channel.CHL_S2_C);
        }

        public void NotifyLevelUp(Champion c)
        {
            var lu = new LevelUp(_game, c);
            _game.PacketHandlerManager.BroadcastPacket(lu, Channel.CHL_S2_C);
        }

        public void NotifyRemoveItem(Champion c, byte slot, byte remaining)
        {
            var ri = new RemoveItem(_game, c, slot, remaining);
            _game.PacketHandlerManager.BroadcastPacketVision(c, ri, Channel.CHL_S2_C);
        }

        public void NotifySetTarget(AttackableUnit attacker, AttackableUnit target)
        {
            var st = new SetTarget(_game, attacker, target);
            _game.PacketHandlerManager.BroadcastPacket(st, Channel.CHL_S2_C);

            var st2 = new SetTarget2(_game, attacker, target);
            _game.PacketHandlerManager.BroadcastPacket(st2, Channel.CHL_S2_C);
        }

        public void NotifyChampionDie(Champion die, AttackableUnit killer, int goldFromKill)
        {
            var cd = new ChampionDie(_game, die, killer, goldFromKill);
            _game.PacketHandlerManager.BroadcastPacket(cd, Channel.CHL_S2_C);

            NotifyChampionDeathTimer(die);
        }

        public void NotifyChampionDeathTimer(Champion die)
        {
            var cdt = new ChampionDeathTimer(_game, die);
            _game.PacketHandlerManager.BroadcastPacket(cdt, Channel.CHL_S2_C);
        }

        public void NotifyChampionRespawn(Champion c)
        {
            var cr = new ChampionRespawn(_game, c);
            _game.PacketHandlerManager.BroadcastPacket(cr, Channel.CHL_S2_C);
        }

        public void NotifyShowProjectile(Projectile p)
        {
            var sp = new ShowProjectile(_game, p);
            _game.PacketHandlerManager.BroadcastPacket(sp, Channel.CHL_S2_C);
        }

        public void NotifyNpcDie(AttackableUnit die, AttackableUnit killer)
        {
            var nd = new NpcDie(_game, die, killer);
            _game.PacketHandlerManager.BroadcastPacket(nd, Channel.CHL_S2_C);
        }

        public void NotifyAddGold(Champion c, AttackableUnit died, float gold)
        {
            var ag = new AddGold(_game, c, died, gold);
            _game.PacketHandlerManager.BroadcastPacket(ag, Channel.CHL_S2_C);
        }

        public void NotifyAddXp(Champion champion, float experience)
        {
            var xp = new AddXp(_game, champion, experience);
            _game.PacketHandlerManager.BroadcastPacket(xp, Channel.CHL_S2_C);
        }

        public void NotifyStopAutoAttack(AttackableUnit attacker)
        {
            var saa = new StopAutoAttack(_game, attacker);
            _game.PacketHandlerManager.BroadcastPacket(saa, Channel.CHL_S2_C);
        }

        public void NotifyDebugMessage(string htmlDebugMessage)
        {
            var dm = new DebugMessage(_game, htmlDebugMessage);
            _game.PacketHandlerManager.BroadcastPacket(dm, Channel.CHL_S2_C);
        }

        public void NotifyPauseGame(int seconds, bool showWindow)
        {
            var pg = new PauseGame(_game, seconds, showWindow);
            _game.PacketHandlerManager.BroadcastPacket(pg, Channel.CHL_S2_C);
        }

        public void NotifyResumeGame(AttackableUnit unpauser, bool showWindow)
        {
            UnpauseGame upg;
            if (unpauser == null)
            {
                upg = new UnpauseGame(_game, 0, showWindow);
            }
            else
            {
                upg = new UnpauseGame(_game, unpauser.NetId, showWindow);
            }

            _game.PacketHandlerManager.BroadcastPacket(upg, Channel.CHL_S2_C);
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
            var spawnPacket = new SpawnAzirTurret(_game, azirTurret);
            _game.PacketHandlerManager.BroadcastPacketVision(azirTurret, spawnPacket, Channel.CHL_S2_C);
        }

        private void NotifyPlaceableSpawned(Placeable placeable)
        {
            var spawnPacket = new SpawnPlaceable(_game, placeable);
            _game.PacketHandlerManager.BroadcastPacketVision(placeable, spawnPacket, Channel.CHL_S2_C);
        }

        private void NotifyMonsterSpawned(Monster m)
        {
            var sp = new SpawnMonster(_game, m);
            _game.PacketHandlerManager.BroadcastPacketVision(m, sp, Channel.CHL_S2_C);
        }

        public void NotifyLeaveVision(GameObject o, TeamId team)
        {
            var lv = new LeaveVision(_game, o);
            _game.PacketHandlerManager.BroadcastPacketTeam(team, lv, Channel.CHL_S2_C);

            // Not exactly sure what this is yet
            var c = o as Champion;
            if (o == null)
            {
                var deleteObj = new DeleteObjectFromVision(_game, o);
                _game.PacketHandlerManager.BroadcastPacketTeam(team, deleteObj, Channel.CHL_S2_C);
            }
        }

        public void NotifyEnterVision(GameObject o, TeamId team)
        {
            switch (o)
            {
                case Minion m:
                {
                    var eva = new EnterVisionAgain(_game, m);
                    _game.PacketHandlerManager.BroadcastPacketTeam(team, eva, Channel.CHL_S2_C);
                    NotifySetHealth(m);
                    return;
                }
                // TODO: Fix bug where enemy champion is not visible to user when vision is acquired until the enemy champion moves
                case Champion c:
                {
                    var eva = new EnterVisionAgain(_game, c);
                    _game.PacketHandlerManager.BroadcastPacketTeam(team, eva, Channel.CHL_S2_C);
                    NotifySetHealth(c);
                    break;
                }
            }
        }

        public void NotifyChampionSpawned(Champion c, TeamId team)
        {
            var hs = new HeroSpawn2(_game, c);
            _game.PacketHandlerManager.BroadcastPacketTeam(team, hs, Channel.CHL_S2_C);
        }

        public void NotifySetCooldown(Champion c, byte slotId, float currentCd, float totalCd)
        {
            var cd = new SetCooldown(_game, c.NetId, slotId, currentCd, totalCd);
            _game.PacketHandlerManager.BroadcastPacket(cd, Channel.CHL_S2_C);
        }

        public void NotifyGameTimer()
        {
            var gameTimer = new GameTimer(_game, _game.GameTime / 1000.0f);
            _game.PacketHandlerManager.BroadcastPacket(gameTimer, Channel.CHL_S2_C);
        }

        public void NotifyUnitAnnounceEvent(UnitAnnounces messageId, AttackableUnit target, GameObject killer = null,
            List<Champion> assists = null)
        {
            var announce = new UnitAnnounce(_game, messageId, target, killer, assists);
            _game.PacketHandlerManager.BroadcastPacket(announce, Channel.CHL_S2_C);
        }

        public void NotifyAnnounceEvent(Announces messageId, bool isMapSpecific)
        {
            var announce = new Announce(_game, messageId, isMapSpecific ? _game.Map.Id : 0);
            _game.PacketHandlerManager.BroadcastPacket(announce, Channel.CHL_S2_C);
        }

        public void NotifySpellAnimation(AttackableUnit u, string animation)
        {
            var sa = new SpellAnimation(_game, u, animation);
            _game.PacketHandlerManager.BroadcastPacketVision(u, sa, Channel.CHL_S2_C);
        }

        public void NotifySetAnimation(AttackableUnit u, List<string> animationPairs)
        {
            var setAnimation = new SetAnimation(_game, u, animationPairs);
            _game.PacketHandlerManager.BroadcastPacketVision(u, setAnimation, Channel.CHL_S2_C);
        }

        public void NotifyDash(Game game,
                               AttackableUnit u,
                               Target t,
                               float dashSpeed,
                               bool keepFacingLastDirection,
                               float leapHeight,
                               float followTargetMaxDistance,
                               float backDistance,
                               float travelTime)
        {
            var dash = new Dash(game,
                                u,
                                t,
                                dashSpeed,
                                keepFacingLastDirection,
                                leapHeight,
                                followTargetMaxDistance,
                                backDistance,
                                travelTime);
            _game.PacketHandlerManager.BroadcastPacketVision(u, dash, Channel.CHL_S2_C);
        }
    }
}

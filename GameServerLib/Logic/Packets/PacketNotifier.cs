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
        private Game Game;

        public PacketNotifier(Game game)
        {
            Game = game;
        }

        public void NotifyMinionSpawned(Minion m, TeamId team)
        {
            var ms = new MinionSpawn(m);
            Game.PacketHandlerManager.BroadcastPacketTeam(team, ms, Channel.CHL_S2_C);
            NotifySetHealth(m);
        }

        public void NotifySetHealth(AttackableUnit u)
        {
            var sh = new SetHealth(u);
            Game.PacketHandlerManager.BroadcastPacketVision(u, sh, Channel.CHL_S2_C);
        }

        public void NotifyGameEnd(Nexus nexus)
        {
            var losingTeam = nexus.Team;

            foreach (var p in PlayerManager.GetPlayers())
            {
                var coords = Game.Map.MapGameScript.GetEndGameCameraPosition(losingTeam);
                var cam = new MoveCamera(p.Item2.Champion, coords.X, coords.Y, coords.Z, 2);
                Game.PacketHandlerManager.SendPacket(p.Item2.Peer, cam, Channel.CHL_S2_C);
                Game.PacketHandlerManager.SendPacket(p.Item2.Peer, new HideUi(), Channel.CHL_S2_C);
            }

            Game.PacketHandlerManager.BroadcastPacket(new ExplodeNexus(nexus), Channel.CHL_S2_C);

            var timer = new Timer(5000) { AutoReset = false };
            timer.Elapsed += (a, b) =>
            {
                var gameEndPacket = new GameEnd(losingTeam != TeamId.TEAM_BLUE);
                Game.PacketHandlerManager.BroadcastPacket(gameEndPacket, Channel.CHL_S2_C);
            };
            timer.Start();
            Program.SetToExit();
        }

        public void NotifyUpdatedStats(AttackableUnit u, bool partial = true)
        {
            if (u.Replication != null)
            {
                var us = new UpdateStats(u.Replication, partial);
                var channel = Channel.CHL_LOW_PRIORITY;
                Game.PacketHandlerManager.BroadcastPacketVision(u, us, channel, PacketFlags.Unsequenced);
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
            var height = Game.Map.NavGrid.GetHeightAtLocation(direction);
            var fd = new FaceDirection(u, direction.X, direction.Y, height, isInstant, turnTime);
            Game.PacketHandlerManager.BroadcastPacketVision(u, fd, Channel.CHL_S2_C);
        }

        public void NotifyInhibitorState(Inhibitor inhibitor, GameObject killer = null, List<Champion> assists = null)
        {
            UnitAnnounce announce;
            switch (inhibitor.InhibitorState)
            {
                case InhibitorState.DEAD:
                    announce = new UnitAnnounce(UnitAnnounces.INHIBITOR_DESTROYED, inhibitor, killer, assists);
                    Game.PacketHandlerManager.BroadcastPacket(announce, Channel.CHL_S2_C);

                    var anim = new InhibitorDeathAnimation(inhibitor, killer);
                    Game.PacketHandlerManager.BroadcastPacket(anim, Channel.CHL_S2_C);
                    break;
                case InhibitorState.ALIVE:
                    announce = new UnitAnnounce(UnitAnnounces.INHIBITOR_SPAWNED, inhibitor, killer, assists);
                    Game.PacketHandlerManager.BroadcastPacket(announce, Channel.CHL_S2_C);
                    break;
            }
            var packet = new InhibitorStateUpdate(inhibitor);
            Game.PacketHandlerManager.BroadcastPacket(packet, Channel.CHL_S2_C);
        }

        public void NotifyInhibitorSpawningSoon(Inhibitor inhibitor)
        {
            var packet = new UnitAnnounce(UnitAnnounces.INHIBITOR_ABOUT_TO_SPAWN, inhibitor);
            Game.PacketHandlerManager.BroadcastPacket(packet, Channel.CHL_S2_C);
        }

        public void NotifyAddBuff(Buff b)
        {
            var add = new AddBuff(b.TargetUnit, b.SourceUnit, b.Stacks, b.Duration, b.BuffType, b.Name, b.Slot);
            Game.PacketHandlerManager.BroadcastPacket(add, Channel.CHL_S2_C);
        }

        public void NotifyEditBuff(Buff b, int stacks)
        {
            var edit = new EditBuff(b.TargetUnit, b.Slot, (byte)b.Stacks);
            Game.PacketHandlerManager.BroadcastPacket(edit, Channel.CHL_S2_C);
        }

        public void NotifyRemoveBuff(AttackableUnit u, string buffName, byte slot = 0x01)
        {
            var remove = new RemoveBuff(u, buffName, slot);
            Game.PacketHandlerManager.BroadcastPacket(remove, Channel.CHL_S2_C);
        }

        public void NotifyTeleport(AttackableUnit u, float x, float y)
        {
            // Can't teleport to this point of the map
            if (!Game.Map.NavGrid.IsWalkable(x, y))
            {
                x = MovementVector.TargetXToNormalFormat(u.X);
                y = MovementVector.TargetYToNormalFormat(u.Y);
            }
            else
            {
                u.SetPosition(x, y);

                //TeleportRequest first(u.NetId, u.teleportToX, u.teleportToY, true);
                //sendPacket(currentPeer, first, Channel.CHL_S2C);

                x = MovementVector.TargetXToNormalFormat(x);
                y = MovementVector.TargetYToNormalFormat(y);
            }

            var second = new TeleportRequest(u.NetId, x, y, false);
            Game.PacketHandlerManager.BroadcastPacketVision(u, second, Channel.CHL_S2_C);
        }

        public void NotifyMovement(GameObject o)
        {
            var answer = new MovementResponse(o);
            Game.PacketHandlerManager.BroadcastPacketVision(o, answer, Channel.CHL_LOW_PRIORITY);
        }

        public void NotifyDamageDone(AttackableUnit source, AttackableUnit target, float amount, DamageType type, DamageText damagetext)
        {
            var dd = new DamageDone(source, target, amount, type, damagetext);
            Game.PacketHandlerManager.BroadcastPacket(dd, Channel.CHL_S2_C);
        }

        public void NotifyModifyShield(AttackableUnit unit, float amount, ShieldType type)
        {
            var ms = new ModifyShield(unit, amount, type);
            Game.PacketHandlerManager.BroadcastPacket(ms, Channel.CHL_S2_C);
        }

        public void NotifyBeginAutoAttack(AttackableUnit attacker, AttackableUnit victim, uint futureProjNetId, bool isCritical)
        {
            var aa = new BeginAutoAttack(attacker, victim, futureProjNetId, isCritical);
            Game.PacketHandlerManager.BroadcastPacket(aa, Channel.CHL_S2_C);
        }

        public void NotifyNextAutoAttack(AttackableUnit attacker, AttackableUnit target, uint futureProjNetId, bool isCritical,
            bool nextAttackFlag)
        {
            var aa = new NextAutoAttack(attacker, target, futureProjNetId, isCritical, nextAttackFlag);
            Game.PacketHandlerManager.BroadcastPacket(aa, Channel.CHL_S2_C);
        }

        public void NotifyOnAttack(AttackableUnit attacker, AttackableUnit attacked, AttackType attackType)
        {
            var oa = new OnAttack(attacker, attacked, attackType);
            Game.PacketHandlerManager.BroadcastPacket(oa, Channel.CHL_S2_C);
        }

        public void NotifyProjectileSpawn(Projectile p)
        {
            var sp = new SpawnProjectile(p);
            Game.PacketHandlerManager.BroadcastPacket(sp, Channel.CHL_S2_C);
        }

        public void NotifyProjectileDestroy(Projectile p)
        {
            var dp = new DestroyProjectile(p);
            Game.PacketHandlerManager.BroadcastPacket(dp, Channel.CHL_S2_C);
        }

        public void NotifyParticleSpawn(Particle particle)
        {
            var sp = new SpawnParticle(particle);
            Game.PacketHandlerManager.BroadcastPacket(sp, Channel.CHL_S2_C);
        }

        public void NotifyParticleDestroy(Particle particle)
        {
            var dp = new DestroyParticle(particle);
            Game.PacketHandlerManager.BroadcastPacket(dp, Channel.CHL_S2_C);
        }

        public void NotifyModelUpdate(AttackableUnit obj)
        {
            var mp = new UpdateModel(obj.NetId, obj.Model);
            Game.PacketHandlerManager.BroadcastPacket(mp, Channel.CHL_S2_C);
        }

        public void NotifyItemBought(AttackableUnit u, Item i)
        {
            var response = new BuyItemResponse(u, i);
            Game.PacketHandlerManager.BroadcastPacketVision(u, response, Channel.CHL_S2_C);
        }

        public void NotifyFogUpdate2(AttackableUnit u)
        {
            var fog = new FogUpdate2(u);
            Game.PacketHandlerManager.BroadcastPacketTeam(u.Team, fog, Channel.CHL_S2_C);
        }

        public void NotifyItemsSwapped(Champion c, byte fromSlot, byte toSlot)
        {
            var sia = new SwapItemsResponse(c, fromSlot, toSlot);
            Game.PacketHandlerManager.BroadcastPacketVision(c, sia, Channel.CHL_S2_C);
        }

        public void NotifyLevelUp(Champion c)
        {
            var lu = new LevelUp(c);
            Game.PacketHandlerManager.BroadcastPacket(lu, Channel.CHL_S2_C);
        }

        public void NotifyRemoveItem(Champion c, byte slot, byte remaining)
        {
            var ri = new RemoveItem(c, slot, remaining);
            Game.PacketHandlerManager.BroadcastPacketVision(c, ri, Channel.CHL_S2_C);
        }

        public void NotifySetTarget(AttackableUnit attacker, AttackableUnit target)
        {
            var st = new SetTarget(attacker, target);
            Game.PacketHandlerManager.BroadcastPacket(st, Channel.CHL_S2_C);

            var st2 = new SetTarget2(attacker, target);
            Game.PacketHandlerManager.BroadcastPacket(st2, Channel.CHL_S2_C);
        }

        public void NotifyChampionDie(Champion die, AttackableUnit killer, int goldFromKill)
        {
            var cd = new ChampionDie(die, killer, goldFromKill);
            Game.PacketHandlerManager.BroadcastPacket(cd, Channel.CHL_S2_C);

            NotifyChampionDeathTimer(die);
        }

        public void NotifyChampionDeathTimer(Champion die)
        {
            var cdt = new ChampionDeathTimer(die);
            Game.PacketHandlerManager.BroadcastPacket(cdt, Channel.CHL_S2_C);
        }

        public void NotifyChampionRespawn(Champion c)
        {
            var cr = new ChampionRespawn(c);
            Game.PacketHandlerManager.BroadcastPacket(cr, Channel.CHL_S2_C);
        }

        public void NotifyShowProjectile(Projectile p)
        {
            var sp = new ShowProjectile(p);
            Game.PacketHandlerManager.BroadcastPacket(sp, Channel.CHL_S2_C);
        }

        public void NotifyNpcDie(AttackableUnit die, AttackableUnit killer)
        {
            var nd = new NpcDie(die, killer);
            Game.PacketHandlerManager.BroadcastPacket(nd, Channel.CHL_S2_C);
        }

        public void NotifyAddGold(Champion c, AttackableUnit died, float gold)
        {
            var ag = new AddGold(c, died, gold);
            Game.PacketHandlerManager.BroadcastPacket(ag, Channel.CHL_S2_C);
        }

        public void NotifyAddXp(Champion champion, float experience)
        {
            var xp = new AddXp(champion, experience);
            Game.PacketHandlerManager.BroadcastPacket(xp, Channel.CHL_S2_C);
        }

        public void NotifyStopAutoAttack(AttackableUnit attacker)
        {
            var saa = new StopAutoAttack(attacker);
            Game.PacketHandlerManager.BroadcastPacket(saa, Channel.CHL_S2_C);
        }

        public void NotifyDebugMessage(string htmlDebugMessage)
        {
            var dm = new DebugMessage(htmlDebugMessage);
            Game.PacketHandlerManager.BroadcastPacket(dm, Channel.CHL_S2_C);
        }

        public void NotifyPauseGame(int seconds, bool showWindow)
        {
            var pg = new PauseGame(seconds, showWindow);
            Game.PacketHandlerManager.BroadcastPacket(pg, Channel.CHL_S2_C);
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

            Game.PacketHandlerManager.BroadcastPacket(upg, Channel.CHL_S2_C);
        }

        public void NotifySpawn(AttackableUnit u)
        {
            var m = u as Minion;
            if (m != null)
            {
                NotifyMinionSpawned(m, CustomConvert.GetEnemyTeam(m.Team));
            }

            var c = u as Champion;
            if (c != null)
            {
                NotifyChampionSpawned(c, CustomConvert.GetEnemyTeam(c.Team));
            }

            var monster = u as Monster;
            if (monster != null)
            {
                NotifyMonsterSpawned(monster);
            }

            var placeable = u as Placeable;
            if (placeable != null)
            {
                NotifyPlaceableSpawned(placeable);
            }

            var azirTurret = u as AzirTurret;
            if (azirTurret != null)
            {
                NotifyAzirTurretSpawned(azirTurret);
            }

            NotifySetHealth(u);
        }

        private void NotifyAzirTurretSpawned(AzirTurret azirTurret)
        {
            var spawnPacket = new SpawnAzirTurret(azirTurret);
            Game.PacketHandlerManager.BroadcastPacketVision(azirTurret, spawnPacket, Channel.CHL_S2_C);
        }

        private void NotifyPlaceableSpawned(Placeable placeable)
        {
            var spawnPacket = new SpawnPlaceable(placeable);
            Game.PacketHandlerManager.BroadcastPacketVision(placeable, spawnPacket, Channel.CHL_S2_C);
        }

        private void NotifyMonsterSpawned(Monster m)
        {
            var sp = new SpawnMonster(m);
            Game.PacketHandlerManager.BroadcastPacketVision(m, sp, Channel.CHL_S2_C);
        }

        public void NotifyLeaveVision(GameObject o, TeamId team)
        {
            var lv = new LeaveVision(o);
            Game.PacketHandlerManager.BroadcastPacketTeam(team, lv, Channel.CHL_S2_C);

            // Not exactly sure what this is yet
            var c = o as Champion;
            if (o == null)
            {
                var deleteObj = new DeleteObjectFromVision(o);
                Game.PacketHandlerManager.BroadcastPacketTeam(team, deleteObj, Channel.CHL_S2_C);
            }
        }

        public void NotifyEnterVision(GameObject o, TeamId team)
        {
            var m = o as Minion;
            if (m != null)
            {
                var eva = new EnterVisionAgain(m);
                Game.PacketHandlerManager.BroadcastPacketTeam(team, eva, Channel.CHL_S2_C);
                NotifySetHealth(m);
                return;
            }

            var c = o as Champion;
            // TODO: Fix bug where enemy champion is not visible to user when vision is acquired until the enemy champion moves
            if (c != null)
            {
                var eva = new EnterVisionAgain(c);
                Game.PacketHandlerManager.BroadcastPacketTeam(team, eva, Channel.CHL_S2_C);
                NotifySetHealth(c);
            }
        }

        public void NotifyChampionSpawned(Champion c, TeamId team)
        {
            var hs = new HeroSpawn2(c);
            Game.PacketHandlerManager.BroadcastPacketTeam(team, hs, Channel.CHL_S2_C);
        }

        public void NotifySetCooldown(Champion c, byte slotId, float currentCd, float totalCd)
        {
            var cd = new SetCooldown(c.NetId, slotId, currentCd, totalCd);
            Game.PacketHandlerManager.BroadcastPacket(cd, Channel.CHL_S2_C);
        }

        public void NotifyGameTimer()
        {
            var gameTimer = new GameTimer(Game.GameTime / 1000.0f);
            Game.PacketHandlerManager.BroadcastPacket(gameTimer, Channel.CHL_S2_C);
        }

        public void NotifyUnitAnnounceEvent(UnitAnnounces messageId, AttackableUnit target, GameObject killer = null,
            List<Champion> assists = null)
        {
            var announce = new UnitAnnounce(messageId, target, killer, assists);
            Game.PacketHandlerManager.BroadcastPacket(announce, Channel.CHL_S2_C);
        }

        public void NotifyAnnounceEvent(Announces messageId, bool isMapSpecific)
        {
            var announce = new Announce(messageId, isMapSpecific ? Game.Map.Id : 0);
            Game.PacketHandlerManager.BroadcastPacket(announce, Channel.CHL_S2_C);
        }

        public void NotifySpellAnimation(AttackableUnit u, string animation)
        {
            var sa = new SpellAnimation(u, animation);
            Game.PacketHandlerManager.BroadcastPacketVision(u, sa, Channel.CHL_S2_C);
        }

        public void NotifySetAnimation(AttackableUnit u, List<string> animationPairs)
        {
            var setAnimation = new SetAnimation(u, animationPairs);
            Game.PacketHandlerManager.BroadcastPacketVision(u, setAnimation, Channel.CHL_S2_C);
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
            var dash = new Dash(u,
                                t,
                                dashSpeed,
                                keepFacingLastDirection,
                                leapHeight,
                                followTargetMaxDistance,
                                backDistance,
                                travelTime);
            Game.PacketHandlerManager.BroadcastPacketVision(u, dash, Channel.CHL_S2_C);
        }
    }
}

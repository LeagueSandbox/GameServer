﻿using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class PacketNotifier
    {
        private Game _game;

        public PacketNotifier(Game game)
        {
            _game = game;
        }

        public void notifyMinionSpawned(Minion m, TeamId team)
        {
            var ms = new MinionSpawn(m);
            _game.PacketHandlerManager.broadcastPacketTeam(team, ms, Channel.CHL_S2C);
            notifySetHealth(m);
        }

        public void notifySetHealth(Unit u)
        {
            var sh = new SetHealth(u);
            _game.PacketHandlerManager.broadcastPacketVision(u, sh, Channel.CHL_S2C);
        }

        public void NotifyGameEnd(Nexus nexus)
        {
            var losingTeam = nexus.getTeam();

            foreach (var p in _game.GetPlayers())
            {
                var coords = _game.GetMap().GetEndGameCameraPosition(losingTeam);
                var cam = new MoveCamera(p.Item2.GetChampion(), coords[0], coords[1], coords[2], 2);
                _game.PacketHandlerManager.sendPacket(p.Item2.GetPeer(), cam, Channel.CHL_S2C);
                _game.PacketHandlerManager.sendPacket(p.Item2.GetPeer(), new HideUi(), Channel.CHL_S2C);
            }
            _game.PacketHandlerManager.broadcastPacket(new ExplodeNexus(nexus), Channel.CHL_S2C);

            var timer = new System.Timers.Timer(5000);
            timer.AutoReset = false;
            timer.Elapsed += (a, b) =>
            {
                var win = new GameEnd(true);
                _game.PacketHandlerManager.broadcastPacketTeam(CustomConvert.getEnemyTeam(losingTeam), win, Channel.CHL_S2C);
                var lose = new GameEnd(false);
                _game.PacketHandlerManager.broadcastPacketTeam(losingTeam, lose, Channel.CHL_S2C);
            };
            timer.Start();
        }

        public void notifyUpdatedStats(Unit u, bool partial = true)
        {
            //if (u is Monster)
            //    return;
            var us = new UpdateStats(u, partial);
            var t = u as Turret;

            if (t != null)
            {
                _game.PacketHandlerManager.broadcastPacket(us, Channel.CHL_LOW_PRIORITY, ENet.PacketFlags.Unsequenced);
                return;
            }

            if (!partial)
            {
                _game.PacketHandlerManager.broadcastPacketTeam(u.getTeam(), us, Channel.CHL_LOW_PRIORITY, ENet.PacketFlags.Unsequenced);
            }
            else
            {
                _game.PacketHandlerManager.broadcastPacketVision(u, us, Channel.CHL_LOW_PRIORITY, ENet.PacketFlags.Unsequenced);
            }
        }

        public void NotifyInhibitorState(Inhibitor inhibitor, GameObject killer = null, List<Champion> assists = null)
        {
            UnitAnnounce announce;
            switch (inhibitor.getState())
            {
                case InhibitorState.Dead:
                    announce = new UnitAnnounce(UnitAnnounces.InhibitorDestroyed, inhibitor, killer, assists);
                    _game.PacketHandlerManager.broadcastPacket(announce, Channel.CHL_S2C);

                    var anim = new InhibitorDeathAnimation(inhibitor, killer);
                    _game.PacketHandlerManager.broadcastPacket(anim, Channel.CHL_S2C);
                    break;
                case InhibitorState.Alive:
                    announce = new UnitAnnounce(UnitAnnounces.InhibitorSpawned, inhibitor, killer, assists);
                    _game.PacketHandlerManager.broadcastPacket(announce, Channel.CHL_S2C);
                    break;
            }
            var packet = new InhibitorStateUpdate(inhibitor);
            _game.PacketHandlerManager.broadcastPacket(packet, Channel.CHL_S2C);
        }

        public void NotifyInhibitorSpawningSoon(Inhibitor inhibitor)
        {
            var packet = new UnitAnnounce(UnitAnnounces.InhibitorAboutToSpawn, inhibitor);
            _game.PacketHandlerManager.broadcastPacket(packet, Channel.CHL_S2C);
        }

        public void notifyAddBuff(Buff b)
        {
            var add = new AddBuff(b.GetUnit(), b.GetSourceUnit(), b.GetStacks(), b.GetDuration(), BuffType.Aura, b.GetName(), 1);
            _game.PacketHandlerManager.broadcastPacket(add, Channel.CHL_S2C);
        }

        public void notifyRemoveBuff(Unit u, string buffName)
        {
            var remove = new RemoveBuff(u, buffName);
            _game.PacketHandlerManager.broadcastPacket(remove, Channel.CHL_S2C);
        }

        public void notifyTeleport(Unit u, float _x, float _y)
        {
            // Can't teleport to this point of the map
            if (!_game.GetMap().IsWalkable(_x, _y))
            {
                _x = MovementVector.targetXToNormalFormat(u.getPosition().X);
                _y = MovementVector.targetYToNormalFormat(u.getPosition().Y);
            }
            else
            {
                u.setPosition(_x, _y);

                //TeleportRequest first(u.getNetId(), u.teleportToX, u.teleportToY, true);
                //sendPacket(currentPeer, first, Channel.CHL_S2C);

                _x = MovementVector.targetXToNormalFormat(_x);
                _y = MovementVector.targetYToNormalFormat(_y);
            }

            var second = new TeleportRequest(u.getNetId(), _x, _y, false);
            _game.PacketHandlerManager.broadcastPacketVision(u, second, Channel.CHL_S2C);
        }

        public void notifyMovement(GameObject o)
        {
            var answer = new MovementAns(o, _game.GetMap());
            _game.PacketHandlerManager.broadcastPacketVision(o, answer, Channel.CHL_LOW_PRIORITY);
        }

        public void notifyDamageDone(Unit source, Unit target, float amount, DamageType type)
        {
            var dd = new DamageDone(source, target, amount, type);
            _game.PacketHandlerManager.broadcastPacket(dd, Channel.CHL_S2C);
        }

        public void notifyBeginAutoAttack(Unit attacker, Unit victim, uint futureProjNetId, bool isCritical)
        {
            var aa = new BeginAutoAttack(attacker, victim, futureProjNetId, isCritical);
            _game.PacketHandlerManager.broadcastPacket(aa, Channel.CHL_S2C);
        }

        public void notifyNextAutoAttack(Unit attacker, Unit target, uint futureProjNetId, bool isCritical, bool nextAttackFlag)
        {
            var aa = new NextAutoAttack(attacker, target, futureProjNetId, isCritical, nextAttackFlag);
            _game.PacketHandlerManager.broadcastPacket(aa, Channel.CHL_S2C);
        }

        public void notifyOnAttack(Unit attacker, Unit attacked, AttackType attackType)
        {
            var oa = new OnAttack(attacker, attacked, attackType);
            _game.PacketHandlerManager.broadcastPacket(oa, Channel.CHL_S2C);
        }

        public void notifyProjectileSpawn(Projectile p)
        {
            var sp = new SpawnProjectile(p);
            _game.PacketHandlerManager.broadcastPacket(sp, Channel.CHL_S2C);
        }

        public void notifyProjectileDestroy(Projectile p)
        {
            var dp = new DestroyProjectile(p);
            _game.PacketHandlerManager.broadcastPacket(dp, Channel.CHL_S2C);
        }

        public void notifyParticleSpawn(Champion source, GameObjects.Target target, string particleName)
        {
            var sp = new SpawnParticle(source, target, particleName, _game.GetNewNetID());
            _game.PacketHandlerManager.broadcastPacket(sp, Channel.CHL_S2C);
        }

        public void notifyModelUpdate(Unit obj)
        {
            var mp = new UpdateModel(obj.getNetId(), obj.getModel());
            _game.PacketHandlerManager.broadcastPacket(mp, Channel.CHL_S2C);
        }

        public void notifyItemBought(Champion c, Item i)
        {
            var response = new BuyItemAns(c, i);
            _game.PacketHandlerManager.broadcastPacketVision(c, response, Channel.CHL_S2C);
        }

        public void notifyItemsSwapped(Champion c, byte fromSlot, byte toSlot)
        {
            var sia = new SwapItems(c, fromSlot, toSlot);
            _game.PacketHandlerManager.broadcastPacketVision(c, sia, Channel.CHL_S2C);
        }

        public void notifyLevelUp(Champion c)
        {
            var lu = new LevelUp(c);
            _game.PacketHandlerManager.broadcastPacket(lu, Channel.CHL_S2C);
        }

        public void notifyRemoveItem(Champion c, byte slot, byte remaining)
        {
            var ri = new RemoveItem(c, slot, remaining);
            _game.PacketHandlerManager.broadcastPacketVision(c, ri, Channel.CHL_S2C);
        }

        public void notifySetTarget(Unit attacker, Unit target)
        {
            var st = new SetTarget(attacker, target);
            _game.PacketHandlerManager.broadcastPacket(st, Channel.CHL_S2C);

            var st2 = new SetTarget2(attacker, target);
            _game.PacketHandlerManager.broadcastPacket(st2, Channel.CHL_S2C);
        }

        public void notifyChampionDie(Champion die, Unit killer, int goldFromKill)
        {
            var cd = new ChampionDie(die, killer, goldFromKill);
            _game.PacketHandlerManager.broadcastPacket(cd, Channel.CHL_S2C);

            notifyChampionDeathTimer(die);
        }

        public void notifyChampionDeathTimer(Champion die)
        {
            var cdt = new ChampionDeathTimer(die);
            _game.PacketHandlerManager.broadcastPacket(cdt, Channel.CHL_S2C);
        }

        public void notifyChampionRespawn(Champion c)
        {
            var cr = new ChampionRespawn(c);
            _game.PacketHandlerManager.broadcastPacket(cr, Channel.CHL_S2C);
        }

        public void notifyShowProjectile(Projectile p)
        {
            var sp = new ShowProjectile(p);
            _game.PacketHandlerManager.broadcastPacket(sp, Channel.CHL_S2C);
        }

        public void notifyNpcDie(Unit die, Unit killer)
        {
            var nd = new NpcDie(die, killer);
            _game.PacketHandlerManager.broadcastPacket(nd, Channel.CHL_S2C);
        }

        public void notifyAddGold(Champion c, Unit died, float gold)
        {
            var ag = new AddGold(c, died, gold);
            _game.PacketHandlerManager.broadcastPacket(ag, Channel.CHL_S2C);
        }

        public void notifyStopAutoAttack(Unit attacker)
        {
            var saa = new StopAutoAttack(attacker);
            _game.PacketHandlerManager.broadcastPacket(saa, Channel.CHL_S2C);
        }

        public void notifyDebugMessage(string htmlDebugMessage)
        {
            var dm = new DebugMessage(htmlDebugMessage);
            _game.PacketHandlerManager.broadcastPacket(dm, Channel.CHL_S2C);
        }

        public void notifySpawn(Unit u)
        {
            var m = u as Minion;
            if (m != null)
                notifyMinionSpawned(m, CustomConvert.getEnemyTeam(m.getTeam()));

            var c = u as Champion;
            if (c != null)
                notifyChampionSpawned(c, CustomConvert.getEnemyTeam(c.getTeam()));

            var monster = u as Monster;
            if (monster != null)
                notifyMonsterSpawned(monster);

            notifySetHealth(u);
        }

        private void notifyMonsterSpawned(Monster m)
        {
            var sp = new SpawnMonster(m);
            _game.PacketHandlerManager.broadcastPacketVision(m, sp, Channel.CHL_S2C);
        }

        public void notifyLeaveVision(GameObject o, TeamId team)
        {
            var lv = new LeaveVision(o);
            _game.PacketHandlerManager.broadcastPacketTeam(team, lv, Channel.CHL_S2C);

            // Not exactly sure what this is yet
            var c = o as Champion;
            if (o == null)
            {
                var deleteObj = new DeleteObjectFromVision(o);
                _game.PacketHandlerManager.broadcastPacketTeam(team, deleteObj, Channel.CHL_S2C);
            }
        }

        public void notifyEnterVision(GameObject o, TeamId team)
        {
            var m = o as Minion;
            if (m != null)
            {
                var eva = new EnterVisionAgain(m);
                _game.PacketHandlerManager.broadcastPacketTeam(team, eva, Channel.CHL_S2C);
                notifySetHealth(m);
                return;
            }

            var c = o as Champion;
            // TODO: Fix bug where enemy champion is not visible to user when vision is acquired until the enemy champion moves
            if (c != null)
            {
                var eva = new EnterVisionAgain(c);
                _game.PacketHandlerManager.broadcastPacketTeam(team, eva, Channel.CHL_S2C);
                notifySetHealth(c);
                return;
            }
        }

        public void notifyChampionSpawned(Champion c, TeamId team)
        {
            var hs = new HeroSpawn2(c);
            _game.PacketHandlerManager.broadcastPacketTeam(team, hs, Channel.CHL_S2C);
        }

        public void notifySetCooldown(Champion c, byte slotId, float currentCd, float totalCd)
        {
            var cd = new SetCooldown(c.getNetId(), slotId, currentCd, totalCd);
            _game.PacketHandlerManager.broadcastPacket(cd, Channel.CHL_S2C);
        }

        public void notifyGameTimer()
        {
            var gameTimer = new GameTimer(_game.GetMap().GetGameTime() / 1000.0f);
            _game.PacketHandlerManager.broadcastPacket(gameTimer, Channel.CHL_S2C);
        }
        public void notifyUnitAnnounceEvent(UnitAnnounces messageId, Unit target, GameObject killer = null, List<Champion> assists = null)
        {
            UnitAnnounce announce;
            announce = new UnitAnnounce(messageId, target, killer, assists);
            _game.PacketHandlerManager.broadcastPacket(announce, Channel.CHL_S2C);
        }
        public void notifyAnnounceEvent(Announces messageId, bool isMapSpecific)
        {
            var announce = new Announce(messageId, isMapSpecific ? _game.GetMap().GetMapId() : 0);
            _game.PacketHandlerManager.broadcastPacket(announce, Channel.CHL_S2C);
        }

        public void notifySpellAnimation(Unit u, string animation)
        {
            var sa = new SpellAnimation(u, animation);
            _game.PacketHandlerManager.broadcastPacketVision(u, sa, Channel.CHL_S2C);
        }

        public void notifySetAnimation(Unit u, List<string> animationPairs)
        {
            var setAnimation = new SetAnimation(u, animationPairs);
            _game.PacketHandlerManager.broadcastPacketVision(u, setAnimation, Channel.CHL_S2C);
        }

        public void notifyDash(Unit u, float _x, float _y, float dashSpeed)
        {
            // TODO: Fix dash: it stays in the current location and doesn't hit a wall if the target location can't be reached
            float _z = u.GetZ();

            /*if (!map.isWalkable(_x, _y)) {
               _x = u.getPosition().X;
               _y = u.getPosition().Y;
            }
            else {
               // Relative coordinates to dash towards
               float newX = _x;
               float newY = _y;
               _z -= map.getHeightAtLocation(_x, _y);
               _x = u.getPosition().X - _x;
               _y = u.getPosition().Y - _y;

               u.setPosition(newX, newY);
            }*/

            var dash = new Dash(u, _x, _y, dashSpeed);
            _game.PacketHandlerManager.broadcastPacketVision(u, dash, Channel.CHL_S2C);
        }
    }
}

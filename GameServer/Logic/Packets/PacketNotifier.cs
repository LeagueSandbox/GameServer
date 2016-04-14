using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
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
    public static class PacketNotifier
    {
        private static Map map;

        public static void notifyMinionSpawned(Minion m, TeamId team)
        {
            var ms = new MinionSpawn(m);
            PacketHandlerManager.getInstace().broadcastPacketTeam(team, ms, Channel.CHL_S2C);
            notifySetHealth(m);
        }

        public static void notifySetHealth(Unit u)
        {
            var sh = new SetHealth(u);
            PacketHandlerManager.getInstace().broadcastPacketVision(u, sh, Channel.CHL_S2C);
        }

        public static void notifyUpdatedStats(Unit u, bool partial = true)
        {
            //if (u is Monster)
            //    return;
            var us = new UpdateStats(u, partial);
            var t = u as Turret;

            if (t != null)
            {
                PacketHandlerManager.getInstace().broadcastPacket(us, Channel.CHL_LOW_PRIORITY, ENet.PacketFlags.Unsequenced);
                return;
            }

            if (!partial)
            {
                PacketHandlerManager.getInstace().broadcastPacketTeam(u.getTeam(), us, Channel.CHL_LOW_PRIORITY, ENet.PacketFlags.Unsequenced);
            }
            else
            {
                PacketHandlerManager.getInstace().broadcastPacketVision(u, us, Channel.CHL_LOW_PRIORITY, ENet.PacketFlags.Unsequenced);
            }
        }

        public static void notifyAddBuff(Buff b)
        {
            var add = new AddBuff(b.getUnit(), b.getSourceUnit(), b.getStacks(), b.getName());
            PacketHandlerManager.getInstace().broadcastPacket(add, Channel.CHL_S2C);
        }

        internal static void setMap(Map m)
        {
            map = m;
        }

        public static void notifyRemoveBuff(Unit u, string buffName)
        {
            var remove = new RemoveBuff(u, buffName);
            PacketHandlerManager.getInstace().broadcastPacket(remove, Channel.CHL_S2C);
        }

        public static void notifyTeleport(Unit u, float _x, float _y)
        {
            // Can't teleport to this point of the map
            if (!map.isWalkable(_x, _y))
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
            PacketHandlerManager.getInstace().broadcastPacketVision(u, second, Channel.CHL_S2C);
        }

        public static void notifyMovement(GameObject o)
        {
            var answer = new MovementAns(o, map);
            PacketHandlerManager.getInstace().broadcastPacketVision(o, answer, Channel.CHL_LOW_PRIORITY);
        }

        public static void notifyDamageDone(Unit source, Unit target, float amount, DamageType type)
        {
            var dd = new DamageDone(source, target, amount, type);
            PacketHandlerManager.getInstace().broadcastPacket(dd, Channel.CHL_S2C);
        }

        public static void notifyBeginAutoAttack(Unit attacker, Unit victim, uint futureProjNetId, bool isCritical)
        {
            var aa = new BeginAutoAttack(attacker, victim, futureProjNetId, isCritical);
            PacketHandlerManager.getInstace().broadcastPacket(aa, Channel.CHL_S2C);
        }

        public static void notifyNextAutoAttack(Unit attacker, Unit target, uint futureProjNetId, bool isCritical, bool nextAttackFlag)
        {
            var aa = new NextAutoAttack(attacker, target, futureProjNetId, isCritical, nextAttackFlag);
            PacketHandlerManager.getInstace().broadcastPacket(aa, Channel.CHL_S2C);
        }

        public static void notifyOnAttack(Unit attacker, Unit attacked, AttackType attackType)
        {
            var oa = new OnAttack(attacker, attacked, attackType);
            PacketHandlerManager.getInstace().broadcastPacket(oa, Channel.CHL_S2C);
        }

        public static void notifyProjectileSpawn(Projectile p)
        {
            var sp = new SpawnProjectile(p);
            PacketHandlerManager.getInstace().broadcastPacket(sp, Channel.CHL_S2C);
        }

        public static void notifyProjectileDestroy(Projectile p)
        {
            var dp = new DestroyProjectile(p);
            PacketHandlerManager.getInstace().broadcastPacket(dp, Channel.CHL_S2C);
        }

        public static void notifyParticleSpawn(Champion source, GameObjects.Target target, string particleName)
        {
            var sp = new SpawnParticle(source, target, particleName, Game.GetNewNetID());
            PacketHandlerManager.getInstace().broadcastPacket(sp, Channel.CHL_S2C);
        }

        public static void notifyModelUpdate(Unit obj)
        {
            var mp = new UpdateModel(obj.getNetId(), obj.getModel());
            PacketHandlerManager.getInstace().broadcastPacket(mp, Channel.CHL_S2C);
        }

        public static void notifyItemBought(Champion c, ItemInstance i)
        {
            var response = new BuyItemAns(c, i);
            PacketHandlerManager.getInstace().broadcastPacketVision(c, response, Channel.CHL_S2C);
        }

        public static void notifyItemsSwapped(Champion c, byte fromSlot, byte toSlot)
        {
            var sia = new SwapItems(c, fromSlot, toSlot);
            PacketHandlerManager.getInstace().broadcastPacketVision(c, sia, Channel.CHL_S2C);
        }

        public static void notifyLevelUp(Champion c)
        {
            var lu = new LevelUp(c);
            PacketHandlerManager.getInstace().broadcastPacket(lu, Channel.CHL_S2C);
        }

        public static void notifyRemoveItem(Champion c, byte slot, byte remaining)
        {
            var ri = new RemoveItem(c, slot, remaining);
            PacketHandlerManager.getInstace().broadcastPacketVision(c, ri, Channel.CHL_S2C);
        }

        public static void notifySetTarget(Unit attacker, Unit target)
        {
            var st = new SetTarget(attacker, target);
            //PacketHandlerManager.getInstace().broadcastPacket(st, Channel.CHL_S2C);

            var st2 = new SetTarget2(attacker, target);
            //PacketHandlerManager.getInstace().broadcastPacket(st2, Channel.CHL_S2C);
        }

        public static void notifyChampionDie(Champion die, Unit killer, int goldFromKill)
        {
            var cd = new ChampionDie(die, killer, goldFromKill);
            PacketHandlerManager.getInstace().broadcastPacket(cd, Channel.CHL_S2C);

            notifyChampionDeathTimer(die);
        }

        public static void notifyChampionDeathTimer(Champion die)
        {
            var cdt = new ChampionDeathTimer(die);
            PacketHandlerManager.getInstace().broadcastPacket(cdt, Channel.CHL_S2C);
        }

        public static void notifyChampionRespawn(Champion c)
        {
            var cr = new ChampionRespawn(c);
            PacketHandlerManager.getInstace().broadcastPacket(cr, Channel.CHL_S2C);
        }

        public static void notifyShowProjectile(Projectile p)
        {
            var sp = new ShowProjectile(p);
            PacketHandlerManager.getInstace().broadcastPacket(sp, Channel.CHL_S2C);
        }

        public static void notifyNpcDie(Unit die, Unit killer)
        {
            var nd = new NpcDie(die, killer);
            PacketHandlerManager.getInstace().broadcastPacket(nd, Channel.CHL_S2C);
        }

        public static void notifyAddGold(Champion c, Unit died, float gold)
        {
            var ag = new AddGold(c, died, gold);
            PacketHandlerManager.getInstace().broadcastPacket(ag, Channel.CHL_S2C);
        }

        public static void notifyStopAutoAttack(Unit attacker)
        {
            var saa = new StopAutoAttack(attacker);
            PacketHandlerManager.getInstace().broadcastPacket(saa, Channel.CHL_S2C);
        }

        public static void notifyDebugMessage(string htmlDebugMessage)
        {
            var dm = new DebugMessage(htmlDebugMessage);
            PacketHandlerManager.getInstace().broadcastPacket(dm, Channel.CHL_S2C);
        }

        public static void notifySpawn(Unit u)
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

        private static void notifyMonsterSpawned(Monster m)
        {
            var sp = new SpawnMonster(m);
            PacketHandlerManager.getInstace().broadcastPacketVision(m, sp, Channel.CHL_S2C);
        }

        public static void notifyLeaveVision(GameObject o, TeamId team)
        {
            var lv = new LeaveVision(o);
            PacketHandlerManager.getInstace().broadcastPacketTeam(team, lv, Channel.CHL_S2C);

            // Not exactly sure what this is yet
            var c = o as Champion;
            if (o == null)
            {
                var deleteObj = new DeleteObjectFromVision(o);
                PacketHandlerManager.getInstace().broadcastPacketTeam(team, deleteObj, Channel.CHL_S2C);
            }
        }

        public static void notifyEnterVision(GameObject o, TeamId team)
        {
            var m = o as Minion;
            if (m != null)
            {
                var eva = new EnterVisionAgain(m);
                PacketHandlerManager.getInstace().broadcastPacketTeam(team, eva, Channel.CHL_S2C);
                notifySetHealth(m);
                return;
            }

            var c = o as Champion;
            // TODO: Fix bug where enemy champion is not visible to user when vision is acquired until the enemy champion moves
            if (c != null)
            {
                var eva = new EnterVisionAgain(c);
                PacketHandlerManager.getInstace().broadcastPacketTeam(team, eva, Channel.CHL_S2C);
                notifySetHealth(c);
                return;
            }
        }

        public static void notifyChampionSpawned(Champion c, TeamId team)
        {
            var hs = new HeroSpawn2(c);
            PacketHandlerManager.getInstace().broadcastPacketTeam(team, hs, Channel.CHL_S2C);
        }

        public static void notifySetCooldown(Champion c, byte slotId, float currentCd, float totalCd)
        {
            var cd = new SetCooldown(c.getNetId(), slotId, currentCd, totalCd);
            PacketHandlerManager.getInstace().broadcastPacket(cd, Channel.CHL_S2C);
        }

        public static void notifyGameTimer()
        {
            var gameTimer = new GameTimer(map.getGameTime() / 1000.0f);
            PacketHandlerManager.getInstace().broadcastPacket(gameTimer, Channel.CHL_S2C);
        }

        public static void notifyAnnounceEvent(byte messageId, bool isMapSpecific)
        {
            var announce = new Announce(messageId, isMapSpecific ? map.getMapId() : 0);
            PacketHandlerManager.getInstace().broadcastPacket(announce, Channel.CHL_S2C);
        }

        public static void notifySpellAnimation(Unit u, string animation)
        {
            var sa = new SpellAnimation(u, animation);
            PacketHandlerManager.getInstace().broadcastPacketVision(u, sa, Channel.CHL_S2C);
        }

        public static void notifySetAnimation(Unit u, List<Tuple<string, string>> animationPairs)
        {
            var setAnimation = new SetAnimation(u, animationPairs);
            PacketHandlerManager.getInstace().broadcastPacketVision(u, setAnimation, Channel.CHL_S2C);
        }

        public static void notifyDash(Unit u, float _x, float _y, float dashSpeed)
        {
            // TODO: Fix dash: it stays in the current location and doesn't hit a wall if the target location can't be reached
            float _z = u.getZ();

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
            PacketHandlerManager.getInstace().broadcastPacketVision(u, dash, Channel.CHL_S2C);
        }
    }
}

using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum DamageType : byte
    {
        DAMAGE_TYPE_PHYSICAL = 0,
        DAMAGE_TYPE_MAGICAL = 1,
        DAMAGE_TYPE_TRUE = 2
    }

    public enum DamageSource
    {
        DAMAGE_SOURCE_ATTACK,
        DAMAGE_SOURCE_SPELL,
        DAMAGE_SOURCE_SUMMONER_SPELL, //Ignite shouldn't destroy Banshee's
        DAMAGE_SOURCE_PASSIVE //Red/Thornmail shouldn't as well
    }

    public enum AttackType : byte
    {
        ATTACK_TYPE_RADIAL,
        ATTACK_TYPE_MELEE,
        ATTACK_TYPE_TARGETED
    }

    public enum MoveOrder
    {
        MOVE_ORDER_MOVE,
        MOVE_ORDER_ATTACKMOVE
    }

    public class Unit : GameObject
    {
        internal const float DETECT_RANGE = 475.0f;
        internal const int EXP_RANGE = 1400;

        protected Stats stats;

        protected float autoAttackDelay = 0;
        protected float autoAttackProjectileSpeed = 0;
        protected float autoAttackCurrentCooldown = 0;
        protected float autoAttackCurrentDelay = 0;
        protected bool isAttacking = false;
        protected bool modelUpdated = false;
        protected bool melee = false;
        protected bool initialAttackDone = false;
        protected bool nextAttackFlag = false;
        protected Unit distressCause;
        protected long statUpdateTimer = 0;
        protected uint autoAttackProjId;
        protected MoveOrder moveOrder = MoveOrder.MOVE_ORDER_MOVE;

        /**
         * Unit we want to attack as soon as in range
         */
        protected Unit targetUnit;
        protected Unit autoAttackTarget;

        protected bool deathFlag = false;

        protected string model;

        protected bool targetable;
        protected bool nextAutoIsCrit = false;
        protected LuaScript unitScript = new LuaScript();

        protected int killDeathCounter = 0;
        private object _buffsLock = new object();
        private Dictionary<string, Buff> _buffs = new Dictionary<string, Buff>();

        public Unit(Game game, uint id, string model, Stats stats, int collisionRadius = 40, float x = 0, float y = 0, int visionRadius = 0) : base(game, id, x, y, collisionRadius, visionRadius)
        {
            this.stats = stats;
            this.model = model;
        }

        public Stats getStats()
        {
            return stats;
        }

        public override void update(long diff)
        {
            //fuck LUA
            /* if (unitScript.isLoaded())
             {
                 try
                 {
                     unitScript.lua.get<sol::function>("onUpdate").call<void>(diff);
                 }
                 catch (sol::error e)
                 {
                     CORE_ERROR("%s", e.what());
                 }
             }*/

            if (isDead())
            {
                if (targetUnit != null)
                {
                    setTargetUnit(null);
                    autoAttackTarget = null;
                    isAttacking = false;
                    _game.GetPacketNotifier().notifySetTarget(this, null);
                    initialAttackDone = false;
                }
                return;
            }

            if (targetUnit != null)
            {
                if (targetUnit.isDead() || !_game.GetMap().teamHasVisionOn(getTeam(), targetUnit))
                {
                    setTargetUnit(null);
                    isAttacking = false;
                    _game.GetPacketNotifier().notifySetTarget(this, null);
                    initialAttackDone = false;

                }
                else if (isAttacking && autoAttackTarget != null)
                {
                    autoAttackCurrentDelay += diff / 1000.0f;
                    if (autoAttackCurrentDelay >= autoAttackDelay / stats.getAttackSpeedMultiplier())
                    {
                        if (!isMelee())
                        {
                            Projectile p = new Projectile(_game, autoAttackProjId, x, y, 5, this, autoAttackTarget, null, autoAttackProjectileSpeed, 0);
                            _game.GetMap().AddObject(p);
                            _game.GetPacketNotifier().notifyShowProjectile(p);
                        }
                        else
                        {
                            autoAttackHit(autoAttackTarget);
                        }
                        autoAttackCurrentCooldown = 1.0f / (stats.getTotalAttackSpeed());
                        isAttacking = false;
                    }

                }
                else if (distanceWith(targetUnit) <= stats.getRange())
                {
                    refreshWaypoints();
                    nextAutoIsCrit = new Random().Next(0, 100) <= stats.getCritChance() * 100;
                    if (autoAttackCurrentCooldown <= 0)
                    {
                        isAttacking = true;
                        autoAttackCurrentDelay = 0;
                        autoAttackProjId = _game.GetNewNetID();
                        autoAttackTarget = targetUnit;

                        if (!initialAttackDone)
                        {
                            initialAttackDone = true;
                            _game.GetPacketNotifier().notifyBeginAutoAttack(this, targetUnit, autoAttackProjId, nextAutoIsCrit);
                        }
                        else
                        {
                            nextAttackFlag = !nextAttackFlag; // The first auto attack frame has occurred
                            _game.GetPacketNotifier().notifyNextAutoAttack(this, targetUnit, autoAttackProjId, nextAutoIsCrit, nextAttackFlag);
                        }

                        var attackType = isMelee() ? AttackType.ATTACK_TYPE_MELEE : AttackType.ATTACK_TYPE_TARGETED;
                        _game.GetPacketNotifier().notifyOnAttack(this, targetUnit, attackType);
                    }

                }
                else
                {
                    refreshWaypoints();
                }

            }
            else if (isAttacking)
            {
                if (autoAttackTarget == null || autoAttackTarget.isDead() || !_game.GetMap().teamHasVisionOn(getTeam(), autoAttackTarget))
                {
                    isAttacking = false;
                    initialAttackDone = false;
                    autoAttackTarget = null;
                }
            }

            base.update(diff);

            if (autoAttackCurrentCooldown > 0)
            {
                autoAttackCurrentCooldown -= diff / 1000.0f;
            }

            statUpdateTimer += diff;
            if (statUpdateTimer >= 500)
            { // update stats (hpregen, manaregen) every 0.5 seconds
                stats.update(statUpdateTimer);
                statUpdateTimer = 0;
            }
        }

        public override float getMoveSpeed()
        {
            return stats.getMovementSpeed();
        }

        public int getKillDeathCounter()
        {
            return killDeathCounter;
        }


        public Dictionary<string, Buff> GetBuffs()
        {
            var toReturn = new Dictionary<string, Buff>();
            lock (_buffsLock)
            {
                foreach (var buff in _buffs)
                    toReturn.Add(buff.Key, buff.Value);

                return toReturn;
            }
        }

        public int GetBuffsCount()
        {
            return _buffs.Count;
        }

        /**
        * This is called by the AA projectile when it hits its target
        */
        public virtual void autoAttackHit(Unit target)
        {
            float damage = (nextAutoIsCrit) ? stats.getCritDamagePct() * stats.getTotalAd() : stats.getTotalAd();
            dealDamageTo(target, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK);

            //fuck LUA
            /*if (unitScript.isLoaded())
            {
                try
                {
                    unitScript.lua.get<sol::function>("onAutoAttack").call<void>(target);
                }
                catch (sol::error e)
                {
                    CORE_ERROR("Error callback ondealdamage: %s", e.what());
                }
            }*/
        }

        public virtual void dealDamageTo(Unit target, float damage, DamageType type, DamageSource source)
        {

            /* //Fuck LUA
            if (unitScript.isLoaded())
            {
                try
                {
                    //damage = 
                    unitScript.lua.get<sol::function>("onDealDamage").call<void>(target, damage, type, source);
                }
                catch (sol::error e)
                {
                    CORE_ERROR("Error callback ondealdamage: %s", e.what());
                }
            }*/


            float defense = 0;
            float regain = 0;
            switch (type)
            {
                case DamageType.DAMAGE_TYPE_PHYSICAL:
                    defense = target.getStats().getArmor();
                    defense = ((1 - stats.getArmorPenPct()) * defense) - stats.getArmorPenFlat();

                    break;
                case DamageType.DAMAGE_TYPE_MAGICAL:
                    defense = target.getStats().getMagicArmor();
                    defense = ((1 - stats.getMagicPenPct()) * defense) - stats.getMagicPenFlat();
                    break;
            }

            switch (source)
            {
                case DamageSource.DAMAGE_SOURCE_SPELL:
                    regain = stats.getSpellVamp();
                    break;
                case DamageSource.DAMAGE_SOURCE_ATTACK:
                    regain = stats.getLifeSteal();
                    break;
            }

            //Damage dealing. (based on leagueoflegends' wikia)
            damage = defense >= 0 ? (100 / (100 + defense)) * damage : (2 - (100 / (100 - defense))) * damage;

            target.getStats().setCurrentHealth(Math.Max(0.0f, target.getStats().getCurrentHealth() - damage));
            if (!target.deathFlag && target.getStats().getCurrentHealth() <= 0)
            {
                target.deathFlag = true;
                target.die(this);
            }
            _game.GetPacketNotifier().notifyDamageDone(this, target, damage, type);

            //Get health from lifesteal/spellvamp
            if (regain != 0)
            {
                stats.setCurrentHealth(Math.Min(stats.getMaxHealth(), stats.getCurrentHealth() + (regain * damage)));
                _game.GetPacketNotifier().notifyUpdatedStats(this);
            }
        }

        public bool isDead()
        {
            return deathFlag;
        }

        public virtual void die(Unit killer)
        {
            setToRemove();
            _game.GetMap().stopTargeting(this);

            _game.GetPacketNotifier().notifyNpcDie(this, killer);

            float exp = _game.GetMap().getExperienceFor(this);
            var champs = _game.GetMap().getChampionsInRange(this, EXP_RANGE, true);
            //Cull allied champions
            champs.RemoveAll(l => l.getTeam() == getTeam());

            if (champs.Count > 0)
            {
                float expPerChamp = exp / champs.Count;
                foreach (var c in champs)
                    c.getStats().setExp(c.getStats().getExperience() + expPerChamp);
            }

            if (killer != null)
            {
                var cKiller = killer as Champion;

                if (cKiller == null)
                    return;

                float gold = _game.GetMap().getGoldFor(this);
                if (gold <= 0)
                    return;

                cKiller.getStats().setGold(cKiller.getStats().getGold() + gold);
                _game.GetPacketNotifier().notifyAddGold(cKiller, this, gold);

                if (cKiller.killDeathCounter < 0)
                {
                    cKiller.setChampionGoldFromMinions(cKiller.getChampionGoldFromMinions() + gold);
                    Logger.LogCoreInfo("Adding gold form minions to reduce death spree: " + cKiller.getChampionGoldFromMinions());
                }

                if (cKiller.getChampionGoldFromMinions() >= 50 && cKiller.killDeathCounter < 0)
                {
                    cKiller.setChampionGoldFromMinions(0);
                    cKiller.killDeathCounter += 1;
                }
            }
        }

        public void setAutoAttackDelay(float newDelay)
        {
            autoAttackDelay = newDelay;
        }

        public void setAutoAttackProjectileSpeed(float newSpeed)
        {
            autoAttackProjectileSpeed = newSpeed;
        }

        public void setModel(string newModel)
        {
            model = newModel;
            modelUpdated = true;
        }

        public string getModel()
        {
            return model;
        }

        public bool isModelUpdated()
        {
            return modelUpdated;
        }

        public void clearModelUpdated()
        {
            modelUpdated = false;
        }

        public void AddBuff(Buff b)
        {
            lock (_buffsLock)
            {
                if (!_buffs.ContainsKey(b.getName()))
                {
                    _buffs.Add(b.getName(), b);
                    getStats().addMovementSpeedPercentageModifier(b.getMovementSpeedPercentModifier());
                }
                else
                {
                    _buffs[b.getName()].setTimeElapsed(0); // if buff already exists, just restart its timer
                }
            }
        }

        public void RemoveBuff(Buff b)
        {
            //TODO add every stat
            getStats().addMovementSpeedPercentageModifier(b.getMovementSpeedPercentModifier());
            RemoveBuff(b.getName());
        }

        public void RemoveBuff(string b)
        {
            lock (_buffsLock)
                _buffs.Remove(b);
        }

        public void setDistressCall(Unit distress)
        {
            distressCause = distress;
        }

        public Unit getDistressCall()
        {
            return distressCause;
        }

        public virtual bool isInDistress()
        {
            return false; /*return distressCause;*/
        }

        //todo: use statmods
        public Buff GetBuff(string name)
        {
            lock (_buffsLock)
            {
                if (_buffs.ContainsKey(name))
                    return _buffs[name];

                return null;
            }
        }

        public void setMoveOrder(MoveOrder moveOrder)
        {
            this.moveOrder = moveOrder;
        }

        public void setTargetUnit(Unit target)
        {
            if (target == null) // If we are unsetting the target (moving around)
            {
                if (targetUnit != null) // and we had a target
                    targetUnit.setDistressCall(null); // Unset the distress call	
                                                      // TODO: Replace this with a delay?
            }
            else
            {
                target.setDistressCall(this); // Otherwise set the distress call
            }

            targetUnit = target;
            refreshWaypoints();
        }

        public void setAutoAttackTarget(Unit target)
        {
            autoAttackTarget = target;
        }

        public Unit getTargetUnit()
        {
            return targetUnit;
        }

        public virtual void refreshWaypoints()
        {
            if (targetUnit == null || (distanceWith(targetUnit) <= stats.getRange() && waypoints.Count == 1))
                return;

            if (distanceWith(targetUnit) <= stats.getRange() - 2.0f)
            {
                setWaypoints(new List<Vector2> { new Vector2(x, y) });
            }
            else
            {
                Target t = new Target(waypoints[waypoints.Count - 1]);
                if (t.distanceWith(targetUnit) >= 25.0f)
                {
                    setWaypoints(new List<Vector2> { new Vector2(x, y), new Vector2(targetUnit.getX(), targetUnit.getY()) });
                }
            }
        }
        public bool isMelee()
        {
            return melee;
        }
        public void setMelee(bool melee)
        {
            this.melee = melee;
        }
        public int classifyTarget(Unit target)
        {
            /*
Under normal circumstances, a minion痴 behavior is simple. Minions follow their attack route until they reach an enemy to engage. 
Every few seconds, they will scan the area around them for the highest priority target. When a minion receives a call for help 
from an ally, it will evaluate its current target in relation to the target designated by the call. It will switch its attack 
to the new target if and only if the new target is of a higher priority than their current target. Minions prioritize targets 
in the following order:

    1. An enemy champion designated by a call for help from an allied champion. (Enemy champion attacking an Allied champion)
    2. An enemy minion designated by a call for help from an allied champion. (Enemy minion attacking an Allied champion)
    3. An enemy minion designated by a call for help from an allied minion. (Enemy minion attacking an Allied minion)
    4. An enemy turret designated by a call for help from an allied minion. (Enemy turret attacking an Allied minion)
    5. An enemy champion designated by a call for help from an allied minion. (Enemy champion attacking an Allied minion)
    6. The closest enemy minion.
    7. The closest enemy champion.
*/

            if (target.targetUnit != null && target.targetUnit.isInDistress()) // If an ally is in distress, target this unit. (Priority 1~5)
            {
                if (target is Champion && target.targetUnit is Champion) // If it's a champion attacking a friendly champion
                    return 1;
                else if (target is Minion && target.targetUnit is Champion) // If it's a minion attacking a friendly champion.
                    return 2;
                else if (target is Minion && target.targetUnit is Minion) // Minion attacking minion
                    return 3;
                else if (target is Turret && target.targetUnit is Minion) // Turret attacking minion
                    return 4;
                else if (target is Champion && target.targetUnit is Minion) // Champion attacking minion
                    return 5;
            }

            var m = target as Minion;
            if (m != null)
            {
                switch (m.getType())
                {
                    case MinionSpawnType.MINION_TYPE_MELEE:
                        return 6;
                    case MinionSpawnType.MINION_TYPE_CASTER:
                        return 7;
                    case MinionSpawnType.MINION_TYPE_CANNON:
                    case MinionSpawnType.MINION_TYPE_SUPER:
                        return 8;
                }
            }

            if (target is Champion)
                return 9;

            return 10;

            /*Turret* t = dynamic_cast<Turret*>(target);

            // Turrets before champions
            if (t) {
               return 6;
            }

            Minion* m = dynamic_cast<Minion*>(target);

            if (m) {
               switch (m.getType()) {
                  case MINION_TYPE_MELEE:
                     return 4;
                  case MINION_TYPE_CASTER:
                     return 5;
                  case MINION_TYPE_CANNON:
                  case MINION_TYPE_SUPER:
                     return 3;
               }
            }

            Champion* c = dynamic_cast<Champion*>(target);
            if (c) {
               return 7;
            }

            //Trap (Shaco box) return 1
            //Pet (Tibbers) return 2

            return 10;*/
        }
    }
}

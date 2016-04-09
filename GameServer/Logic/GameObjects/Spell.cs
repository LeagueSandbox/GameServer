using InibinSharp;
using InibinSharp.RAF;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using IntWarsSharp.Core.Logic.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum SpellFlag : uint
    {
        SPELL_FLAG_AutoCast = 0x00000002,
        SPELL_FLAG_InstantCast = 0x00000004,
        SPELL_FLAG_PersistThroughDeath = 0x00000008,
        SPELL_FLAG_NonDispellable = 0x00000010,
        SPELL_FLAG_NoClick = 0x00000020,
        SPELL_FLAG_AffectImportantBotTargets = 0x00000040,
        SPELL_FLAG_AllowWhileTaunted = 0x00000080,
        SPELL_FLAG_NotAffectZombie = 0x00000100,
        SPELL_FLAG_AffectUntargetable = 0x00000200,
        SPELL_FLAG_AffectEnemies = 0x00000400,
        SPELL_FLAG_AffectFriends = 0x00000800,
        SPELL_FLAG_AffectBuildings = 0x00001000,
        SPELL_FLAG_NotAffectSelf = 0x00002000,
        SPELL_FLAG_AffectNeutral = 0x00004000,
        SPELL_FLAG_AffectAllSides = 0x00004C00,
        SPELL_FLAG_AffectMinions = 0x00008000,
        SPELL_FLAG_AffectHeroes = 0x00010000,
        SPELL_FLAG_AffectTurrets = 0x00020000,
        SPELL_FLAG_AffectAllUnitTypes = 0x00038000,
        SPELL_FLAG_AlwaysSelf = 0x00040000,
        SPELL_FLAG_AffectDead = 0x00080000,
        SPELL_FLAG_AffectNotPet = 0x00100000,
        SPELL_FLAG_AffectBarracksOnly = 0x00200000,
        SPELL_FLAG_IgnoreVisibilityCheck = 0x00400000,
        SPELL_FLAG_NonTargetableAlly = 0x00800000,
        SPELL_FLAG_NonTargetableEnemy = 0x01000000,
        SPELL_FLAG_NonTargetableAll = 0x01800000,
        SPELL_FLAG_TargetableToAll = 0x02000000,
        SPELL_FLAG_AffectWards = 0x04000000,
        SPELL_FLAG_AffectUseable = 0x08000000,
        SPELL_FLAG_IgnoreAllyMinion = 0x10000000,
        SPELL_FLAG_IgnoreEnemyMinion = 0x20000000,
        SPELL_FLAG_IgnoreLaneMinion = 0x40000000,
        SPELL_FLAG_IgnoreClones = 0x80000000,
    };

    public enum SpellState
    {
        STATE_READY,
        STATE_CASTING,
        STATE_COOLDOWN
    };

    public enum SpellTargetType : int
    {
        TARGET_SELF = 0, // teemo W ; xin Q
        TARGET_UNIT = 1, // Taric E ; Annie Q ; teemo Q ; xin E
        TARGET_LOC_AOE = 2, // Lux E, Ziggs R
        TARGET_CONE = 3, // Annie W, Kass E
        TARGET_SELF_AOE = 4, // sivir R, Gangplanck E
        TARGET_LOC = 6, // Ez Q, W, E, R ; Mundo Q
        TARGET_LOC2 = 7  // Morg Q, Cait's Q -- These don't seem to have Missile inibins, and SpawnProjectile doesn't seem necessary to show the projectiles
    };
    public class Spell
    {
        protected Champion owner;
        protected short level = 0;
        protected byte slot;
        protected string spellName;
        protected float targetType;
        protected int flags = 0;
        protected int projectileFlags = 0;

        protected float castTime = 0.0f;
        protected float castRange = 1000.0f;
        protected float projectileSpeed = 2000.0f;
        protected float lineWidth;
        protected float[] cooldown = new float[5] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
        protected float[] cost = new float[5] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };

        // Warning : this value usually contains one of the "ad/ap" bonus coefficient, as seen in "deals 50 (+{coefficient}%) damage"
        // However, it may not be accurate and there's no way to tell whether it's the ad or ap bonus for hybrid spells
        // Sometimes, it is also stored as an effect value instead of the coefficient
        protected float coefficient;
        protected List<List<float>> effects = new List<List<float>>();

        protected float range = 0;

        protected SpellState state = SpellState.STATE_READY;
        protected float currentCooldown = 0;
        protected float currentCastTime = 0;
        protected int futureProjNetId;
        protected int spellNetId;

        protected Unit target;
        protected float x, y;

        public Spell(Champion owner, string spellName, byte slot)
        {
            this.owner = owner;
            this.spellName = spellName;
            this.slot = slot;

            Inibin inibin;
            if (!RAFManager.getInstance().readInibin("DATA/Spells/" + spellName + ".inibin", out inibin))
            {
                if (!RAFManager.getInstance().readInibin("DATA/Characters/" + owner.getType() + "/Spells/" + spellName + ".inibin", out inibin))
                {
                    if (!RAFManager.getInstance().readInibin("DATA/Characters/" + owner.getType() + "/" + spellName + ".inibin", out inibin))
                    {
                        Logger.LogCoreError("Couldn't find spell stats for " + spellName);
                        return;
                    }
                }
            }

            var i = 0;
            // Generate cooldown values for each level of the spell
            for (i = 0; i < cooldown.Length; ++i)
            {
                cooldown[i] = inibin.GetValue<float>("SpellData", "Cooldown" + (i + 1));
            }

            castTime = ((1.0f + inibin.GetValue<float>("SpellData", "DelayCastOffsetPercent"))) / 2.0f;

            flags = inibin.GetValue<int>("SpellData", "Flags");
            castRange = inibin.GetValue<float>("SpellData", "CastRange");
            projectileSpeed = inibin.GetValue<float>("SpellData", "MissileSpeed");
            coefficient = inibin.GetValue<float>("SpellData", "Coefficient");
            lineWidth = inibin.GetValue<float>("SpellData", "LineWidth");

            i = 1;
            while (true)
            {
                string key = "Effect" + (0 + i) + "Level0Amount";
                if (inibin.GetValue<object>("SpellData", key) == null)
                    break;


                List<float> effectValues = new List<float>();
                for (var j = 0; j < 6; ++j)
                {
                    key = "Effect" + (0 + i) + "Level" + (0 + j) + "Amount";
                    effectValues.Add(inibin.GetValue<float>("SpellData", key));
                }

                effects.Add(effectValues);
                ++i;
            }

            targetType = (float)Math.Floor(inibin.GetValue<float>("SpellData", "TargettingType") + 0.5f);


            // This is starting to get ugly. How many more names / paths to go ?
            if (!RAFManager.getInstance().readInibin("DATA/Spells/" + spellName + "Missile.inibin", out inibin))
            {
                if (!RAFManager.getInstance().readInibin("DATA/Spells/" + spellName + "Mis.inibin", out inibin))
                {
                    if (!RAFManager.getInstance().readInibin("DATA/Characters/" + owner.getType() + "/Spells/" + spellName + "Missile.inibin", out inibin))
                    {
                        if (!RAFManager.getInstance().readInibin("DATA/Characters/" + owner.getType() + "/" + spellName + "Missile.inibin", out inibin))
                        {
                            if (!RAFManager.getInstance().readInibin("DATA/Characters/" + owner.getType() + "/Spells/" + spellName + "Mis.inibin", out inibin))
                            {
                                if (!RAFManager.getInstance().readInibin("DATA/Characters/" + owner.getType() + "/" + spellName + "Mis.inibin", out inibin))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            castRange = inibin.GetValue<float>("SpellData", "CastRange");
            projectileSpeed = inibin.GetValue<float>("SpellData", "MissileSpeed");
            projectileFlags = inibin.GetValue<int>("SpellData", "Flags");
        }

        /**
         * Called when the character casts the spell
         */
        public virtual bool cast(float x, float y, Unit u = null, int futureProjNetId = 0, int spellNetId = 0)
        {
            this.x = x;
            this.y = y;
            this.target = u;
            this.futureProjNetId = futureProjNetId;
            this.spellNetId = spellNetId;

            if (castTime > 0 && flags != (int)SpellFlag.SPELL_FLAG_InstantCast)
            {
                owner.setPosition(owner.getX(), owner.getY());//stop moving serverside too. TODO: check for each spell if they stop movement or not
                state = SpellState.STATE_CASTING;
                currentCastTime = castTime;
            }
            else
            {
                finishCasting();
            }
            return true;
        }

        /**
         * Called when the spell is finished casting and we're supposed to do things
         * such as projectile spawning, etc.
         */
        public virtual void finishCasting()
        {
            doLua();

            state = SpellState.STATE_COOLDOWN;
            currentCooldown = getCooldown();
        }

        /**
         * Called every diff milliseconds to update the spell
         */
        public virtual void update(long diff)
        {
            switch (state)
            {
                case SpellState.STATE_READY:
                    return;
                case SpellState.STATE_CASTING:

                    currentCastTime -= diff / 1000.0f;
                    if (currentCastTime <= 0)
                    {
                        finishCasting();
                    }
                    break;
                case SpellState.STATE_COOLDOWN:
                    currentCooldown -= diff / 1000.0f;
                    if (currentCooldown < 0)
                    {
                        state = SpellState.STATE_READY;
                    }
                    break;
            }
        }

        /**
         * Called by projectiles when they land / hit
         * In here we apply the effects : damage, buffs, debuffs...
         */

        public void applyEffects(Unit u, Projectile p = null)
        {
            LuaScript script = new LuaScript();

            script.lua["u"] = u;
            script.lua.DoString(@"
                function getTarget()
                    return u
                end");

            script.lua.DoString("getTarget()");

            script.lua["p"] = p;
            script.lua.DoString(@"
                function destroyProjectile()
                    p:setToRemove()
                end");

            script.lua["DAMAGE_TYPE_PHYSICAL"] = DamageType.DAMAGE_TYPE_MAGICAL;
            script.lua["DAMAGE_TYPE_MAGICAL"] = DamageType.DAMAGE_TYPE_MAGICAL;
            script.lua["DAMAGE_SOURCE_SPELL"] = DamageSource.DAMAGE_SOURCE_SPELL;


            script.lua.DoString(@"
                function dealPhysicalDamage(amount)
                    getOwner():dealDamageTo(u, amount, DAMAGE_TYPE_PHYSICAL, DAMAGE_SOURCE_SPELL)
                end");

            script.lua.DoString(@"
                function dealMagicalDamage(amount)
                    getOwner():dealDamageTo(u, amount, DAMAGE_TYPE_MAGICAL, DAMAGE_SOURCE_SPELL)
                end");

            script.lua.DoString(@"
                function getNumberObjectsHit(amount)
                    return p.getObjectsHit().Count
                end");


            script.lua["BUFFTYPE_TEMPORARY"] = BuffType.BUFFTYPE_TEMPORARY;
            script.lua["BUFFTYPE_ETERNAL"] = BuffType.BUFFTYPE_ETERNAL;

            script.lua.RegisterFunction("addBuff", this, typeof(Spell).GetMethod("addBuff", new Type[] { typeof(string), typeof(float), typeof(BuffType), typeof(Unit)}));

            script.lua.RegisterFunction("printChat", this, typeof(Spell).GetMethod("printChat", new Type[] { typeof(string) }));

            loadLua(script); //comment this line for no reload on the fly, better performance

            try
            {
                script.lua.DoString("applyEffects()");
            }
            catch (NLua.Exceptions.LuaScriptException ex)
            {
                Logger.LogCoreError("Lua exception " + ex.Message);
            }
        }

        public Champion getOwner()
        {
            return owner;
        }

        public Unit getTarget()
        {
            return target;
        }

        public float getX()
        {
            return x;
        }

        public float getY()
        {
            return y;
        }

        public float getRange()
        {
            return range;
        }

        public void teleportTo(float x, float y)
        {
            PacketNotifier.notifyTeleport(owner, x, y);
        }

        public bool isWalkable(float x, float y)
        {
            return owner.getMap().isWalkable(x, y);
        }

        public float getProjectileSpeed()
        {
            return projectileSpeed;
        }

        public float getCoefficient()
        {
            return coefficient;
        }

        public float getEffectValue(int effectNo)
        {
            if (effectNo >= effects.Count || level >= effects[effectNo].Count)
            {
                return 0.0f;
            }
            return effects[effectNo][level];
        }

        public void addProjectile(float toX, float toY)
        {
            Projectile p = new Projectile(owner.getMap(), Game.GetNewNetID(), owner.getX(), owner.getY(), (int)lineWidth, owner, new Target(toX, toY), this, projectileSpeed, (int)RAFManager.getInstance().getHash(spellName + "Missile"), projectileFlags != 0 ? projectileFlags : flags);
            owner.getMap().addObject(p);
            PacketNotifier.notifyProjectileSpawn(p);
        }

        public void addProjectileTarget(Target target)
        {
            Projectile p = new Projectile(owner.getMap(), Game.GetNewNetID(), owner.getX(), owner.getY(), (int)lineWidth, owner, target, this, projectileSpeed, (int)RAFManager.getInstance().getHash(spellName + "Missile"), projectileFlags != 0 ? projectileFlags : flags);
            owner.getMap().addObject(p);
            PacketNotifier.notifyProjectileSpawn(p);
        }

        public void addBuff(string buffName, float dur, BuffType type, Unit u)
        {
            u.addBuff(new Buff(buffName, dur, type, u));
        }

        public void addParticle(string particle, float toX, float toY)
        {
            Target t = new Target(toX, toY);
            PacketNotifier.notifyParticleSpawn(owner, t, particle);
        }

        public void addParticleTarget(string particle, Target t)
        {
            PacketNotifier.notifyParticleSpawn(owner, t, particle);
        }

        public void printChat(string msg)
        {
            var dm = new SpawnParticle.DebugMessage(msg);
            PacketHandlerManager.getInstace().broadcastPacket(dm, Channel.CHL_S2C);
        }

        /**
         * @return Spell's unique ID
         */
        public int getId()
        {
            return (int)RAFManager.getInstance().getHash(spellName);
        }

        public float getCastTime()
        {
            return castTime;
        }

        public string getStringForSlot()
        {
            switch (getSlot())
            {
                case 0:
                    return "Q";
                case 1:
                    return "W";
                case 2:
                    return "E";
                case 3:
                    return "R";
                case 4:
                    return "D";
                case 5:
                    return "F";
            }

            return "undefined";
        }

        /*
         * does spell effects in lua if defined.
         */
        public void doLua()
        {
            //Fuck LUA
            LuaScript script = new LuaScript();

            loadLua(script); //comment this line for no reload on the fly, better performance

            Logger.LogCoreInfo("Spell from slot " + getSlot());
            try
            {
                script.lua.DoString("finishCasting()");
            }
            catch (NLua.Exceptions.LuaScriptException ex)
            {
                Logger.LogCoreError("Lua exception " + ex.Message);
            }
            /*try
            {
                script.lua.script("finishCasting()");
            }
            catch (sol::error e)
            {//lua error? don't crash the whole server
                CORE_ERROR("%s", e.what());
            }*/
        }
        public void loadLua(LuaScript script)
        {
            string scriptloc = Config.contentManager.GetSpellScriptPath(owner.getType(), getStringForSlot());
            script.lua.DoString("package.path = 'LuaLib/?.lua;' .. package.path");
            script.lua.RegisterFunction("getOwner", this, typeof(Spell).GetMethod("getOwner"));
            script.lua.RegisterFunction("getOwnerX", owner, typeof(Champion).GetMethod("getX"));
            script.lua.RegisterFunction("getOwnerY", owner, typeof(Champion).GetMethod("getY"));
            script.lua.RegisterFunction("getSpellLevel", this, typeof(Spell).GetMethod("getLevel"));
            script.lua.RegisterFunction("getOwnerLevel", owner.getStats(), typeof(Stats).GetMethod("getLevel"));
            script.lua.RegisterFunction("getChampionModel", owner,typeof(Champion).GetMethod("getModel"));
            script.lua.RegisterFunction("getCastTarget", this, typeof(Spell).GetMethod("getTarget"));

            script.lua.RegisterFunction("getSpellToX", this, typeof(Spell).GetMethod("getX"));
            script.lua.RegisterFunction("getSpellToY", this, typeof(Spell).GetMethod("getY"));
            script.lua.RegisterFunction("getRange", this, typeof(Spell).GetMethod("getRange"));

            script.lua.RegisterFunction("setChampionModel", owner, typeof(Champion).GetMethod("setModel", new Type[] { typeof(string) }));

            script.lua.RegisterFunction("teleportTo", this, typeof(Spell).GetMethod("teleportTo", new Type[] { typeof(float), typeof(float) }));
            script.lua.RegisterFunction("isWalkable", this, typeof(Spell).GetMethod("isWalkable", new Type[] { typeof(float), typeof(float) }));

            script.lua.RegisterFunction("getProjectileSpeed", this, typeof(Spell).GetMethod("isWalkable", new Type[] { typeof(float), typeof(float) }));
            script.lua.RegisterFunction("getCoefficient", this, typeof(Spell).GetMethod("isWalkable", new Type[] { typeof(float), typeof(float) }));


            script.lua.RegisterFunction("getProjectileSpeed", this, typeof(Spell).GetMethod("getProjectileSpeed"));
            script.lua.RegisterFunction("getCoefficient", this, typeof(Spell).GetMethod("getCoefficient"));

            script.lua.RegisterFunction("addProjectile", this, typeof(Spell).GetMethod("addProjectile", new Type[] { typeof(float), typeof(float) }));
            script.lua.RegisterFunction("addProjectileTarget", this, typeof(Spell).GetMethod("addProjectileTarget", new Type[] { typeof(Target) }));

            script.lua.RegisterFunction("getEffectValue", this, typeof(Spell).GetMethod("getEffectValue", new Type[] { typeof(int) }));

            script.lua.RegisterFunction("addParticle", this, typeof(Spell).GetMethod("addParticle", new Type[] { typeof(string), typeof(float), typeof(float) }));
            script.lua.RegisterFunction("addParticleTarget", this, typeof(Spell).GetMethod("addParticleTarget", new Type[] { typeof(string), typeof(Target) }));

            script.lua["BUFFTYPE_TEMPORARY"] = BuffType.BUFFTYPE_TEMPORARY;
            script.lua["BUFFTYPE_ETERNAL"] = BuffType.BUFFTYPE_ETERNAL;

            script.lua.RegisterFunction("addBuff", this, typeof(Spell).GetMethod("addBuff", new Type[] { typeof(string), typeof(float), typeof(BuffType), typeof(Unit) }));

            script.lua.RegisterFunction("printChat", this, typeof(Spell).GetMethod("printChat", new Type[] { typeof(string) }));

            /*
            * This have to be in general function, not in spell
            script.lua.set_function("getTeam", [this](Object * o) { return o->getTeam(); });
            script.lua.set_function("isDead", [this](Unit * u) { return u->isDead(); });
            */


            //TODO : Add buffs

            /*script.lua.set_function("addMovementSpeedBuff", [this](Unit* u, float amount, float duration) { // expose teleport to lua
                Buff* b = new Buff(duration);
                b->setMovementSpeedPercentModifier(amount);
                u->addBuff(b);
                u->getStats().addMovementSpeedPercentageModifier(b->getMovementSpeedPercentModifier());
               return;
            });*/

            /*
            script.lua.set_function("addProjectileCustom", [this](const std::string&name, float projSpeed, float toX, float toY) {
                Projectile* p = new Projectile(owner->getMap(), GetNewNetID(), owner->getX(), owner->getY(), lineWidth, owner, new Target(toX, toY), this, projectileSpeed, RAFFile::getHash(name), projectileFlags ? projectileFlags : flags);
                owner->getMap()->addObject(p);
                owner->getMap()->getGame()->notifyProjectileSpawn(p);

                return;
            });

            script.lua.set_function("addProjectileTargetCustom", [this](const std::string&name, float projSpeed, Target *t) {
                Projectile* p = new Projectile(owner->getMap(), GetNewNetID(), owner->getX(), owner->getY(), lineWidth, owner, t, this, projectileSpeed, RAFFile::getHash(name), projectileFlags ? projectileFlags : flags);
                owner->getMap()->addObject(p);
                owner->getMap()->getGame()->notifyProjectileSpawn(p);

                return;
            });

            
             //For spells that don't require SpawnProjectile, but for which we still need to track the projectile server-side
             
            script.lua.set_function("addServerProjectile", [this](float toX, float toY) {
                Projectile* p = new Projectile(owner->getMap(), futureProjNetId, owner->getX(), owner->getY(), lineWidth, owner, new Target(toX, toY), this, projectileSpeed, 0, projectileFlags ? projectileFlags : flags);
                owner->getMap()->addObject(p);

                return;
            });

            script.lua.set_function("spellAnimation", [this](const std::string&animation, Unit* u) {
                owner->getMap()->getGame()->notifySpellAnimation(u, animation);
                return;
            });

            // TODO: Set multiple animations
            script.lua.set_function("setAnimation", [this](const std::string&animation1, const std::string&animation2, Unit* u) {
                std::vector < std::pair < std::string, std::string>> animationPairs;
                animationPairs.push_back(std::make_pair(animation1, animation2));

                owner->getMap()->getGame()->notifySetAnimation(u, animationPairs);
                return;
            });

            script.lua.set_function("resetAnimations", [this](Unit * u) {
                std::vector < std::pair < std::string, std::string>> animationPairs;
                owner->getMap()->getGame()->notifySetAnimation(u, animationPairs);
                return;
            });

            script.lua.set_function("dashTo", [this](Unit * u, float x, float y, float dashSpeed) {
                u->dashTo(x, y, dashSpeed);
                u->setTargetUnit(0);
                owner->getMap()->getGame()->notifyDash(u, x, y, dashSpeed);
                return;
            });

            script.lua.set_function("getUnitsInRange", [this](Target * t, float range, bool isAlive) {
                return owner->getMap()->getUnitsInRange(t, range, isAlive);
            });

            script.lua.set_function("getChampionsInRange", [this](Target * t, float range, bool isAlive) {
                return owner->getMap()->getChampionsInRange(t, range, isAlive);
            });
            */
            script.loadScript(scriptloc); //todo: abstract class that loads a lua file for any lua
        }

        //public void reloadLua();

        public void setSlot(byte _slot)
        {
            slot = _slot;
        }

        /**
         * TODO : Add in CDR % from champion's stat
         */
        public float getCooldown()
        {
            return 0; // TODO: Remove this
            if (level <= 0)
                return 0;

            return cooldown[level - 1];
        }

        /**
         * @return the mana/energy/health cost
         */
        public float getCost()
        {
            return 0; // TODO: Remove this
            if (level <= 0)
                return 0;

            return cost[level - 1];
        }

        public int getFlags()
        {
            return flags;
        }

        public int getLevel()
        {
            return level;
        }

        public virtual void levelUp()
        {
            ++level;
        }

        public SpellState getState()
        {
            return state;
        }

        public float getLineWidth()
        {
            return lineWidth;
        }

        public float getProjectileFlags()
        {
            return projectileFlags;
        }

        public byte getSlot()
        {
            return slot;
        }
    }
}

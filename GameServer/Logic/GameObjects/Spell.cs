using InibinSharp;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using NLua.Exceptions;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Scripting;
using LeagueSandbox.GameServer.Logic.Scripting.Lua;

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
        public Champion Owner { get; private set; }
        public short Level { get; private set; }
        public byte Slot { get; set; }
        public int Flags { get; private set; }
        public int ProjectileFlags { get; private set; }
        public float CastTime { get; private set; }
        public float ProjectileSpeed { get; private set; }
        public float LineWidth { get; private set; }


        private string _spellName;
        private float _targetType;
        private float _castRange;

        protected float[] cooldown = new float[5] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
        protected float[] cost = new float[5] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };

        // Warning : this value usually contains one of the "ad/ap" bonus coefficient, as seen in "deals 50 (+{coefficient}%) damage"
        // However, it may not be accurate and there's no way to tell whether it's the ad or ap bonus for hybrid spells
        // Sometimes, it is also stored as an effect value instead of the coefficient
        public float Coefficient { get; private set; }
        protected List<List<float>> effects = new List<List<float>>();

        protected SpellState state = SpellState.STATE_READY;
        private float _currentCooldown;
        private float _currentCastTime;
        protected uint futureProjNetId;
        protected uint spellNetId;

        public Unit Target { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }

        public static bool NO_COOLDOWN = true;
        public static bool NO_MANACOST = true;

        private IScriptEngine _scriptEngine;
        private RAFManager _rafManager = Program.ResolveDependency<RAFManager>();
        private Logger _logger = Program.ResolveDependency<Logger>();
        private Game _game = Program.ResolveDependency<Game>();
        private NetworkIdManager _networkIdManager = Program.ResolveDependency<NetworkIdManager>();

        public Spell(Champion owner, string spellName, byte slot)
        {
            this.Owner = owner;
            this._spellName = spellName;
            this.Slot = slot;
            this.Level = 0;
            this.Flags = 0;
            this.ProjectileFlags = 0;
            this.CastTime = 0.0f;
            this._castRange = 1000.0f;
            this.ProjectileSpeed = 2000.0f;
            this._currentCastTime = 0.0f;

            Inibin inibin;

            _scriptEngine = new LuaScriptEngine();

            LoadLua(_scriptEngine);

            if (slot > 3)
            {
                if (!_rafManager.readInibin("DATA/Spells/" + spellName + ".inibin", out inibin))
                {
                    return;
                }

                // Generate cooldown values for each level of the spell
                for (var i = 0; i < cooldown.Length; ++i)
                {
                    cooldown[i] = inibin.GetValue<float>("SpellData", "Cooldown");
                }

                return;
            }

            if (!_rafManager.readInibin("DATA/Spells/" + spellName + ".inibin", out inibin))
            {
                if (!_rafManager.readInibin("DATA/Characters/" + owner.Model + "/Spells/" + spellName + ".inibin", out inibin))
                {
                    if (!_rafManager.readInibin("DATA/Characters/" + owner.Model + "/" + spellName + ".inibin", out inibin))
                    {
                        _logger.LogCoreError("Couldn't find spell stats for " + spellName);
                        return;
                    }
                }
            }

            // Generate cooldown values for each level of the spell
            for (var i = 0; i < cooldown.Length; ++i)
            {
                cooldown[i] = inibin.GetValue<float>("SpellData", "Cooldown" + (i + 1));
            }

            for (var i = 0; i < cost.Length; ++i)
            {
                cost[i] = inibin.GetValue<float>("SpellData", "ManaCost" + (i + 1));
            }

            CastTime = ((1.0f + inibin.GetValue<float>("SpellData", "DelayCastOffsetPercent"))) / 2.0f;

            Flags = inibin.GetValue<int>("SpellData", "Flags");
            _castRange = inibin.GetValue<float>("SpellData", "CastRange");
            ProjectileSpeed = inibin.GetValue<float>("SpellData", "MissileSpeed");
            Coefficient = inibin.GetValue<float>("SpellData", "Coefficient");
            LineWidth = inibin.GetValue<float>("SpellData", "LineWidth");

            for (var i = 0; true; i++)
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

            _targetType = (float)Math.Floor(inibin.GetValue<float>("SpellData", "TargettingType") + 0.5f);


            // This is starting to get ugly. How many more names / paths to go ?
            if (!_rafManager.readInibin("DATA/Spells/" + spellName + "Missile.inibin", out inibin))
            {
                if (!_rafManager.readInibin("DATA/Spells/" + spellName + "Mis.inibin", out inibin))
                {
                    if (!_rafManager.readInibin("DATA/Characters/" + owner.Model + "/Spells/" + spellName + "Missile.inibin", out inibin))
                    {
                        if (!_rafManager.readInibin("DATA/Characters/" + owner.Model + "/" + spellName + "Missile.inibin", out inibin))
                        {
                            if (!_rafManager.readInibin("DATA/Characters/" + owner.Model + "/Spells/" + spellName + "Mis.inibin", out inibin))
                            {
                                if (!_rafManager.readInibin("DATA/Characters/" + owner.Model + "/" + spellName + "Mis.inibin", out inibin))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            _castRange = inibin.GetValue<float>("SpellData", "CastRange");
            ProjectileSpeed = inibin.GetValue<float>("SpellData", "MissileSpeed");
            ProjectileFlags = inibin.GetValue<int>("SpellData", "Flags");
        }

        /**
         * Called when the character casts the spell
         */
        public virtual bool cast(float x, float y, Unit u = null, uint futureProjNetId = 0, uint spellNetId = 0)
        {
            this.X = x;
            this.Y = y;
            this.Target = u;
            this.futureProjNetId = futureProjNetId;
            this.spellNetId = spellNetId;
            _scriptEngine.SetGlobalVariable("castTarget", Target);

            if (CastTime > 0 && Flags != (int)SpellFlag.SPELL_FLAG_InstantCast)
            {
                Owner.setPosition(Owner.X, Owner.Y);//stop moving serverside too. TODO: check for each spell if they stop movement or not
                state = SpellState.STATE_CASTING;
                _currentCastTime = CastTime;
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

            _logger.LogCoreInfo("Spell from slot " + Slot);
            try
            {
                _scriptEngine.Execute("onFinishCasting()");
            }
            catch (LuaException e)
            {
                _logger.LogCoreError("LUA ERROR : " + e.Message);
            }

            state = SpellState.STATE_COOLDOWN;

            _currentCooldown = getCooldown();

            if (Slot < 4)
            {
                _game.PacketNotifier.notifySetCooldown(Owner, Slot, _currentCooldown, getCooldown());
            }
            else if (Slot == 4) //Done this because summ-spells are hard-coded
            {                        //Fix these when they are not
                _game.PacketNotifier.notifySetCooldown(Owner, Slot, 240, 240);
            }
            else if (Slot == 5)
            {
                _game.PacketNotifier.notifySetCooldown(Owner, Slot, 300, 300);
            }

            Owner.IsCastingSpell = false;
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
                    Owner.IsCastingSpell = true;
                    _currentCastTime -= diff / 1000.0f;
                    if (_currentCastTime <= 0)
                    {
                        finishCasting();
                    }
                    break;
                case SpellState.STATE_COOLDOWN:
                    _currentCooldown -= diff / 1000.0f;
                    if (_currentCooldown < 0)
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
            _scriptEngine.SetGlobalVariable("u", u);
            _scriptEngine.Execute(@"
                function getTarget()
                    return u
                end");

            _scriptEngine.SetGlobalVariable("p", p);
            _scriptEngine.Execute(@"
                function destroyProjectile()
                    p:setToRemove()
                end");

            _scriptEngine.SetGlobalVariable("TYPE_PHYSICAL", DamageType.DAMAGE_TYPE_PHYSICAL);
            _scriptEngine.SetGlobalVariable("TYPE_MAGICAL", DamageType.DAMAGE_TYPE_MAGICAL);
            _scriptEngine.SetGlobalVariable("TYPE_TRUE", DamageType.DAMAGE_TYPE_TRUE);
            _scriptEngine.SetGlobalVariable("SOURCE_SPELL", DamageSource.DAMAGE_SOURCE_SPELL);
            _scriptEngine.SetGlobalVariable("SOURCE_SUMMONER_SPELL", DamageSource.DAMAGE_SOURCE_SUMMONER_SPELL);
            _scriptEngine.SetGlobalVariable("SOURCE_ATTACK", DamageSource.DAMAGE_SOURCE_ATTACK);
            _scriptEngine.SetGlobalVariable("SOURCE_PASSIVE", DamageSource.DAMAGE_SOURCE_PASSIVE);
            _scriptEngine.SetGlobalVariable("countObjectsHit", p.ObjectsHit.Count);


            _scriptEngine.Execute(@"
                function dealPhysicalDamage(amount)
                    owner:dealDamageTo(u, amount, TYPE_PHYSICAL, SOURCE_SPELL, false)
                end");

            _scriptEngine.Execute(@"
                function dealMagicalDamage(amount)
                    owner:dealDamageTo(u, amount, TYPE_MAGICAL, SOURCE_SPELL, false)
                end");

            _scriptEngine.Execute(@"
                function dealTrueDamage(amount)
                    owner:dealDamageTo(u, amount, TYPE_TRUE, SOURCE_SPELL, false)
                end");

            _scriptEngine.Execute(@"
                function getNumberObjectsHit()
                    return countObjectsHit
                end");

            try
            {
                _scriptEngine.Execute("applyEffects()");
            }
            catch (LuaException e)
            {
                _logger.LogCoreError("LUA ERROR : " + e.Message);
            }
        }

        public float getEffectValue(int effectNo)
        {
            if (effectNo >= effects.Count || Level >= effects[effectNo].Count)
            {
                return 0.0f;
            }
            return effects[effectNo][Level];
        }

        public void addProjectile(string nameMissile, float toX, float toY)
        {
            Projectile p = new Projectile(
                Owner.X,
                Owner.Y,
                (int)LineWidth,
                Owner,
                new Target(toX, toY),
                this,
                ProjectileSpeed,
                (int)_rafManager.getHash(nameMissile),
                ProjectileFlags != 0 ? ProjectileFlags : Flags
            );
            _game.Map.AddObject(p);
            _game.PacketNotifier.notifyProjectileSpawn(p);
        }

        public void addProjectileTarget(string nameMissile, Target target)
        {
            Projectile p = new Projectile(
                Owner.X,
                Owner.Y,
                (int)LineWidth,
                Owner,
                target,
                this,
                ProjectileSpeed,
                (int)_rafManager.getHash(nameMissile),
                ProjectileFlags != 0 ? ProjectileFlags : Flags
            );
            _game.Map.AddObject(p);
            _game.PacketNotifier.notifyProjectileSpawn(p);
        }

        public void spellAnimation(string animName, Unit target)
        {
            _game.PacketNotifier.notifySpellAnimation(target, animName);
        }

        public void setAnimation(string animation, string animation2, Unit target)
        {
            List<string> animList = new List<string>();
            animList.Add(animation);
            animList.Add(animation2);
            _game.PacketNotifier.notifySetAnimation(target, animList);
        }

        public void resetAnimations(Unit target)
        {
            List<string> animList = new List<string>();
            _game.PacketNotifier.notifySetAnimation(target, animList);
        }

        public int getOtherSpellLevel(int slotId)
        {
            return Owner.Spells[slotId].Level;
        }

        public string GetChampionModel()
        {
            return Owner.Model;
        }

        /**
         * @return Spell's unique ID
         */
        public int getId()
        {
            return (int)_rafManager.getHash(_spellName);
        }

        public string getStringForSlot()
        {
            switch (Slot)
            {
                case 0:
                    return "Q";
                case 1:
                    return "W";
                case 2:
                    return "E";
                case 3:
                    return "R";
            }

            return "undefined";
        }

        public void AddPlaceable(float toX, float toY, string model, string name)
        {
            var p = new Placeable(this.Owner, toX, toY, model, name);
            p.SetTeam(Owner.Team);

            p.SetVisibleByTeam(Enet.TeamId.TEAM_BLUE, true);
            p.SetVisibleByTeam(Enet.TeamId.TEAM_PURPLE, true);

            _game.Map.AddObject(p);
        }

        public void LoadLua(IScriptEngine scriptEngine)
        {
            var config = _game.Config;
            string scriptloc;

            if (Slot > 3)
            {
                scriptloc = config.ContentManager.GetSpellScriptPath("Global", _spellName);
            }
            else
            {
                scriptloc = config.ContentManager.GetSpellScriptPath(Owner.Model, getStringForSlot());
            }
            scriptEngine.Execute("package.path = 'LuaLib/?.lua;' .. package.path");
            scriptEngine.Execute(@"
                function onFinishCasting()
                end");
            scriptEngine.Execute(@"
                function applyEffects()
                end");
            ApiFunctionManager.AddBaseFunctionToLuaScript(scriptEngine);
            scriptEngine.SetGlobalVariable("owner", Owner);
            //scriptEngine.RegisterFunction("getOwnerX", Owner, typeof(Champion).GetMethod("getX"));
            //scriptEngine.RegisterFunction("getOwnerY", Owner, typeof(Champion).GetMethod("getY"));
            scriptEngine.SetGlobalVariable("spellLevel", Level);
            scriptEngine.RegisterFunction("getOwnerLevel", Owner.GetStats(), typeof(Stats).GetMethod("GetLevel"));
            scriptEngine.RegisterFunction("getChampionModel", Owner, typeof(Spell).GetMethod("GetChampionModel"));
            //scriptEngine.SetGlobalVariable("castTarget", Target);
            //scriptEngine.SetGlobalVariable("spellToX", X);
            //scriptEngine.SetGlobalVariable("spellToY", Y);
            scriptEngine.SetGlobalVariable("spell", this);
            scriptEngine.SetGlobalVariable("projectileSpeed", ProjectileSpeed);
            scriptEngine.SetGlobalVariable("coefficient", Coefficient);
            scriptEngine.RegisterFunction("addProjectile", this, typeof(Spell).GetMethod("addProjectile", new Type[] { typeof(string), typeof(float), typeof(float) }));
            scriptEngine.RegisterFunction("addProjectileTarget", this, typeof(Spell).GetMethod("addProjectileTarget", new Type[] { typeof(string), typeof(Target) }));
            scriptEngine.RegisterFunction("getEffectValue", this, typeof(Spell).GetMethod("getEffectValue", new Type[] { typeof(int) }));
            scriptEngine.RegisterFunction("spellAnimation", this, typeof(Spell).GetMethod("spellAnimation", new Type[] { typeof(string), typeof(Unit) }));
            scriptEngine.RegisterFunction("setAnimation", this, typeof(Spell).GetMethod("setAnimation", new Type[] { typeof(string), typeof(string), typeof(Unit) }));
            scriptEngine.RegisterFunction("resetAnimations", this, typeof(Spell).GetMethod("resetAnimations", new Type[] { typeof(Unit) }));
            scriptEngine.RegisterFunction("getOtherSpellLevel", this, typeof(Spell).GetMethod("getOtherSpellLevel", new Type[] { typeof(int) } ));
            scriptEngine.RegisterFunction("addPlaceable", this, typeof(Spell).GetMethod("AddPlaceable", new Type[] { typeof(float), typeof(float), typeof(string), typeof(string) }));

            /*script.lua.set_function("addMovementSpeedBuff", [this](Unit* u, float amount, float duration) { // expose teleport to lua
                Buff* b = new Buff(duration);
                b->setMovementSpeedPercentModifier(amount);
                u->addBuff(b);
                u->GetStats().addMovementSpeedPercentageModifier(b->getMovementSpeedPercentModifier());
               return;
            });*/

            /*
            script.lua.set_function("addProjectileCustom", [this](const std::string&name, float projSpeed, float toX, float toY) {
                Projectile* p = new Projectile(owner->getMap(), GetNewNetID(), owner->X, owner->Y, lineWidth, owner, new Target(toX, toY), this, projectileSpeed, RAFFile::getHash(name), projectileFlags ? projectileFlags : flags);
                owner->getMap()->addObject(p);
                owner->getMap()->Game->notifyProjectileSpawn(p);

                return;
            });

            script.lua.set_function("addProjectileTargetCustom", [this](const std::string&name, float projSpeed, Target *t) {
                Projectile* p = new Projectile(owner->getMap(), GetNewNetID(), owner->X, owner->Y, lineWidth, owner, t, this, projectileSpeed, RAFFile::getHash(name), projectileFlags ? projectileFlags : flags);
                owner->getMap()->addObject(p);
                owner->getMap()->Game->notifyProjectileSpawn(p);

                return;
            });


             //For spells that don't require SpawnProjectile, but for which we still need to track the projectile server-side

            script.lua.set_function("addServerProjectile", [this](float toX, float toY) {
                Projectile* p = new Projectile(owner->getMap(), futureProjNetId, owner->X, owner->Y, lineWidth, owner, new Target(toX, toY), this, projectileSpeed, 0, projectileFlags ? projectileFlags : flags);
                owner->getMap()->addObject(p);

                return;
            });

            script.lua.set_function("spellAnimation", [this](const std::string&animation, Unit* u) {
                owner->getMap()->Game->notifySpellAnimation(u, animation);
                return;
            });

            // TODO: Set multiple animations
            script.lua.set_function("setAnimation", [this](const std::string&animation1, const std::string&animation2, Unit* u) {
                std::vector < std::pair < std::string, std::string>> animationPairs;
                animationPairs.push_back(std::make_pair(animation1, animation2));

                owner->getMap()->Game->notifySetAnimation(u, animationPairs);
                return;
            });

            script.lua.set_function("resetAnimations", [this](Unit * u) {
                std::vector < std::pair < std::string, std::string>> animationPairs;
                owner->getMap()->Game->notifySetAnimation(u, animationPairs);
                return;
            });*/

            scriptEngine.Load(scriptloc); //todo: abstract class that loads a lua file for any lua
        }

        //public void reloadLua();

        /**
         * TODO : Add in CDR % from champion's stat
         */
        public float getCooldown()
        {
            if (Level <= 0 || NO_COOLDOWN)
                return 0;

            return cooldown[Level - 1];
        }

        /**
         * @return the mana/energy/health cost
         */
        public float getCost()
        {
            if (Level <= 0 || NO_MANACOST)
                return 0;

            return cost[Level - 1];
        }

        public virtual void levelUp()
        {
            ++Level;
        }

        public SpellState getState()
        {
            return state;
        }
    }
}

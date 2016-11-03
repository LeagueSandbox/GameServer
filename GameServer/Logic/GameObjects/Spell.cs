using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using NLua.Exceptions;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Scripting;
using LeagueSandbox.GameServer.Logic.Scripting.Lua;
using Newtonsoft.Json.Linq;

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

        protected float[] _castRange = { 1000.0f, 1000.0f, 1000.0f, 1000.0f, 1000.0f };
        protected float[] cooldown = { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
        protected float[] cost = { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
        protected string hitEffectName;

        public float[] Coefficient { get; private set; }
        protected List<List<float>> effects = new List<List<float>>();

        public SpellState state { get; protected set; } = SpellState.STATE_READY;
        private float _currentCooldown;
        private float _currentCastTime;
        protected uint futureProjNetId;
        protected uint spellNetId;

        public Unit Target { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }

        public bool NoCooldown { get; }
        public bool NoManacost { get; }

        private IScriptEngine _scriptEngine;
        private Logger _logger = Program.ResolveDependency<Logger>();
        private Game _game = Program.ResolveDependency<Game>();
        private RAFManager _rafManager = Program.ResolveDependency<RAFManager>();

        public Spell(Champion owner, string spellName, byte slot)
        {
            Owner = owner;
            _spellName = spellName;
            Slot = slot;
            Level = 0;
            Flags = 0;
            ProjectileFlags = 0;
            CastTime = 0.0f;
            ProjectileSpeed = 2000.0f;
            _currentCastTime = 0.0f;
            NoCooldown = _game.Config.CooldownsDisabled;
            NoManacost = _game.Config.ManaCostsDisabled;

            _scriptEngine = new LuaScriptEngine();
            LoadLua(_scriptEngine);

            JObject data;

            if (slot > 3)
            {
                if (!_rafManager.ReadSpellData(spellName, out data))
                {
                    return;
                }

                // Generate cooldown values for each level of the spell
                for (var i = 0; i < cooldown.Length; ++i)
                {
                    cooldown[i] = _rafManager.GetFloatValue(data, "SpellData", "Cooldown");
                }

                return;
            }

            if (!_rafManager.ReadSpellData(spellName, out data))
            {
                _logger.LogCoreError("Couldn't find spell stats for " + spellName);
                return;
            }

            // Generate cooldown values for each level of the spell
            for (var i = 0; i < cooldown.Length; ++i)
            {
                cooldown[i] = _rafManager.GetFloatValue(data, "SpellData", "Cooldown" + (i + 1));
            }

            for (var i = 0; i < cost.Length; ++i)
            {
                cost[i] = _rafManager.GetFloatValue(data, "SpellData", "ManaCost" + (i + 1));
            }

            for (var i = 0; i < _castRange.Length; ++i)
            {
                if (_rafManager.GetFloatValue(data, "SpellData", "CastRange" + (i + 1)) == 0)
                {
                    _castRange[i] = _rafManager.GetFloatValue(data, "SpellData", "CastRange");
                }
                else
                {
                    _castRange[i] = _rafManager.GetFloatValue(data, "SpellData", "CastRange" + (i + 1));
                }
            }

            CastTime = (1.0f + _rafManager.GetFloatValue(data, "SpellData", "DelayCastOffsetPercent")) / 2.0f;

            Flags = _rafManager.GetIntValue(data, "SpellData", "Flags");
            ProjectileSpeed = _rafManager.GetFloatValue(data, "SpellData", "MissileSpeed");
            for (var i = 0; true; i++)
            {
                if (_rafManager.GetValue(data, "SpellData", "Coefficient" + i) == null)
                {
                    break;
                }

                var coeffValue = _rafManager.GetFloatValue(data, "SpellData", "Coefficient" + i);
                Coefficient[i] = coeffValue;
                i++;
            }
            LineWidth = _rafManager.GetFloatValue(data, "SpellData", "LineWidth");
            hitEffectName = _rafManager.GetStringValue(data, "SpellData", "HitEffectName");

            for (var i = 0; true; i++)
            {
                string key = "Effect" + (0 + i) + "Level0Amount";
                if (_rafManager.GetValue(data, "SpellData", key) == null)
                {
                    break;
                }

                var effectValues = new List<float>();
                for (var j = 0; j < 6; ++j)
                {
                    key = "Effect" + (0 + i) + "Level" + (0 + j) + "Amount";
                    effectValues.Add(_rafManager.GetFloatValue(data, "SpellData", key));
                }

                effects.Add(effectValues);
                ++i;
            }

            _targetType = (float) Math.Floor(
                _rafManager.GetFloatValue(data, "SpellData", "TargettingType") +
                0.5f
            );

            ReloadLua();
        }

        public void LoadExtraSpells(Champion champ)
        {
            JObject data;
            var possibilities = new List<string>
            {
                _spellName + "Missile",
                _spellName + "Mis"
            };

            foreach (var spell in champ.ExtraSpells)
            {
                if (!possibilities.Contains(spell))
                {
                    continue;
                }

                if (_rafManager.ReadSpellData(spell, out data))
                {
                    hitEffectName = _rafManager.GetStringValue(data, "SpellData", "HitEffectName");
                    ProjectileSpeed = _rafManager.GetFloatValue(data, "SpellData", "MissileSpeed");
                    ProjectileFlags = _rafManager.GetIntValue(data, "SpellData", "Flags");
                }
            }
        }

        /**
         * Called when the character casts the spell
         */
        public virtual bool cast(float x, float y, Unit u = null, uint futureProjNetId = 0, uint spellNetId = 0)
        {
            X = x;
            Y = y;
            Target = u;
            this.futureProjNetId = futureProjNetId;
            this.spellNetId = spellNetId;
            _scriptEngine.SetGlobalVariable("castTarget", Target);

            if (_targetType == 1 && Target != null && Target.GetDistanceTo(Owner) > _castRange[Level - 1])
            {
                return false;
            }

            RunCastLua();

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

        private void RunCastLua()
        {
            if (!_scriptEngine.IsLoaded())
                return;

            try
            {
                _scriptEngine.SetGlobalVariable("castTarget", Target);
                _scriptEngine.SetGlobalVariable("spell", this);
                _scriptEngine.Execute("onStartCasting()");
            }
            catch (LuaException e)
            {
                _logger.LogCoreError("LUA ERROR : " + e);
            }
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

            if (_scriptEngine.IsLoaded())
            {
                try
                {
                    _scriptEngine.SetGlobalVariable("diff", diff);
                    _scriptEngine.Execute("onUpdate(diff)");
                }
                catch (LuaException e)
                {
                    _logger.LogCoreError("LUA ERROR : " + e);
                }
            }
        }

        /**
         * Called by projectiles when they land / hit
         * In here we apply the effects : damage, buffs, debuffs...
         */

        public void applyEffects(Unit u, Projectile p = null)
        {
            if (!string.IsNullOrEmpty(hitEffectName))
            {
                ApiFunctionManager.AddParticleTarget(Owner, hitEffectName, u);
            }

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

        public void AddProjectile(string nameMissile, float toX, float toY, bool isServerOnly = false)
        {
            Projectile p = new Projectile(
                Owner.X,
                Owner.Y,
                (int)LineWidth,
                Owner,
                new Target(toX, toY),
                this,
                ProjectileSpeed,
                (int)_rafManager.GetHash(nameMissile),
                ProjectileFlags != 0 ? ProjectileFlags : Flags
            );
            _game.Map.AddObject(p);
            if (!isServerOnly)
            {
                _game.PacketNotifier.notifyProjectileSpawn(p);
            }
        }

        public void AddProjectileTarget(string nameMissile, Target target, bool isServerOnly = false)
        {
            Projectile p = new Projectile(
                Owner.X,
                Owner.Y,
                (int)LineWidth,
                Owner,
                target,
                this,
                ProjectileSpeed,
                (int)_rafManager.GetHash(nameMissile),
                ProjectileFlags != 0 ? ProjectileFlags : Flags
            );
            _game.Map.AddObject(p);
            if (!isServerOnly)
            {
                _game.PacketNotifier.notifyProjectileSpawn(p);
            }
        }

        public void AddProjectileCustom(string name, float fromX, float fromY, float toX, float toY,
            bool isServerOnly)
        {
            var p = new Projectile(
                fromX,
                fromY,
                (int)LineWidth,
                Owner,
                new Target(toX, toY),
                this,
                ProjectileSpeed,
                (int)_rafManager.GetHash(name),
                ProjectileFlags != 0 ? ProjectileFlags : Flags
            );

            _game.Map.AddObject(p);
            if (!isServerOnly)
            {
                _game.PacketNotifier.notifyProjectileSpawn(p);
            }
        }

        public void AddProjectileCustomTarget(string name, float fromX, float fromY, Target target,
            bool isServerOnly)
        {
            var p = new Projectile(
                fromX,
                fromY,
                (int)LineWidth,
                Owner,
                target,
                this,
                ProjectileSpeed,
                (int)_rafManager.GetHash(name),
                ProjectileFlags != 0 ? ProjectileFlags : Flags
            );

            _game.Map.AddObject(p);
            if (!isServerOnly)
            {
                _game.PacketNotifier.notifyProjectileSpawn(p);
            }
        }

        public void AddLaser(float toX, float toY, bool affectAsCastIsOver = true)
        {
            var l = new Laser(
                Owner.X,
                Owner.Y,
                (int)LineWidth,
                Owner,
                new Target(toX, toY),
                this,
                ProjectileFlags != 0 ? ProjectileFlags : Flags,
                affectAsCastIsOver
            );
            _game.Map.AddObject(l);
        }

        public void spellAnimation(string animName, Unit target)
        {
            _game.PacketNotifier.notifySpellAnimation(target, animName);
        }

        public void setAnimation(string animation, string animation2, Unit target)
        {
            List<string> animList = new List<string> { animation, animation2 };
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
            return (int)_rafManager.GetHash(_spellName);
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
            var p = new Placeable(Owner, toX, toY, model, name);
            p.SetTeam(Owner.Team);

            p.SetVisibleByTeam(Owner.Team, true);

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
            scriptEngine.Execute(@"
                function onUpdate(diff)
                end");
            scriptEngine.Execute(@"
                function onStartCasting()
                end");
            ApiFunctionManager.AddBaseFunctionToLuaScript(scriptEngine);
            scriptEngine.SetGlobalVariable("owner", Owner);
            scriptEngine.SetGlobalVariable("spellLevel", Level);
            scriptEngine.RegisterFunction("getOwnerLevel", Owner.GetStats(), typeof(Stats).GetMethod("GetLevel"));
            scriptEngine.RegisterFunction("getChampionModel", Owner, typeof(Spell).GetMethod("GetChampionModel"));
            scriptEngine.SetGlobalVariable("spell", this);
            scriptEngine.SetGlobalVariable("projectileSpeed", ProjectileSpeed);
            scriptEngine.SetGlobalVariable("coefficient", Coefficient);
            scriptEngine.RegisterFunction("addProjectile", this, typeof(Spell).GetMethod("AddProjectile", new[] { typeof(string), typeof(float), typeof(float), typeof(bool) }));
            scriptEngine.RegisterFunction("addProjectileTarget", this, typeof(Spell).GetMethod("AddProjectileTarget", new[] { typeof(string), typeof(Target), typeof(bool) }));
            scriptEngine.RegisterFunction("getEffectValue", this, typeof(Spell).GetMethod("getEffectValue", new[] { typeof(int) }));
            scriptEngine.RegisterFunction("spellAnimation", this, typeof(Spell).GetMethod("spellAnimation", new[] { typeof(string), typeof(Unit) }));
            scriptEngine.RegisterFunction("setAnimation", this, typeof(Spell).GetMethod("setAnimation", new[] { typeof(string), typeof(string), typeof(Unit) }));
            scriptEngine.RegisterFunction("resetAnimations", this, typeof(Spell).GetMethod("resetAnimations", new[] { typeof(Unit) }));
            scriptEngine.RegisterFunction("getOtherSpellLevel", this, typeof(Spell).GetMethod("getOtherSpellLevel", new[] { typeof(int) }));
            scriptEngine.RegisterFunction("addPlaceable", this, typeof(Spell).GetMethod("AddPlaceable", new[] { typeof(float), typeof(float), typeof(string), typeof(string) }));
            scriptEngine.RegisterFunction("addProjectileCustom", this, typeof(Spell).GetMethod("AddProjectileCustom", new[] { typeof(string), typeof(float), typeof(float), typeof(float), typeof(float), typeof(bool) }));
            scriptEngine.RegisterFunction("addProjectileCustomTarget", this, typeof(Spell).GetMethod("AddProjectileCustomTarget", new[] { typeof(string), typeof(float), typeof(float), typeof(Target), typeof(bool) }));
            scriptEngine.RegisterFunction("setCooldown", this, typeof(Spell).GetMethod("SetCooldown", new[] { typeof(byte), typeof(float) }));
            scriptEngine.RegisterFunction("lowerCooldown", this, typeof(Spell).GetMethod("LowerCooldown", new[] { typeof(byte), typeof(float) }));
            scriptEngine.RegisterFunction("addLaser", this, typeof(Spell).GetMethod("AddLaser", new[] { typeof(float), typeof(float), typeof(bool) }));

            /*scriptEngine.RegisterFunction("addMovementSpeedBuff", this, typeof(Spell).GetMethod("addMovementSpeedBuff", new Type[] { typeof(Unit), typeof(float), typeof(float) }));
            
            public void addMovementSpeedBuff(Unit u, float amount, float duration)
            {
                Buff b = new Buff(duration);
                b.setMovementSpeedPercentModifier(amount);
                u.AddBuff(b);
                u.GetStats().addMovementSpeedPercentageModifier(b.getMovementSpeedPercentModifier());
            }*/

            scriptEngine.Load(scriptloc); //todo: abstract class that loads a lua file for any lua
        }

        public void ReloadLua()
        {
            LoadLua(_scriptEngine);
        }

        /**
         * TODO : Add in CDR % from champion's stat
         */
        public float getCooldown()
        {
            if (Level <= 0 || NoCooldown)
                return 0;

            return cooldown[Level - 1];
        }

        /**
         * @return the mana/energy/health cost
         */
        public float getCost()
        {
            if (Level <= 0 || NoManacost)
                return 0;

            return cost[Level - 1];
        }

        public virtual void levelUp()
        {
            ++Level;
        }

        public void SetCooldown(byte slot, float newCd)
        {
            var targetSpell = Owner.Spells[slot];

            if (newCd <= 0)
            {
                _game.PacketNotifier.notifySetCooldown(Owner, slot, 0, 0);
                targetSpell.state = SpellState.STATE_READY;
                targetSpell._currentCooldown = 0;
            }
            else
            {
                _game.PacketNotifier.notifySetCooldown(Owner, slot, newCd, targetSpell.getCooldown());
                targetSpell.state = SpellState.STATE_COOLDOWN;
                targetSpell._currentCooldown = newCd;
            }
        }

        public void LowerCooldown(byte slot, float lowerValue)
        {
            SetCooldown(slot, Owner.Spells[slot]._currentCooldown - lowerValue);
        }
    }
}

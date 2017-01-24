using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using System;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;
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
        SPELL_FLAG_IgnoreClones = 0x80000000
    }

    public enum SpellState
    {
        STATE_READY,
        STATE_CASTING,
        STATE_COOLDOWN,
        STATE_CHANNELING
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
        protected float[] channelDuration = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        protected string hitEffectName;

        public float[] Coefficient { get; private set; }
        protected List<List<float>> effects = new List<List<float>>();

        public SpellState state { get; protected set; } = SpellState.STATE_READY;
        private float _currentCooldown;
        private float _currentCastTime;
        private float _currentChannelDuration;
        protected uint futureProjNetId;
        protected uint spellNetId;

        public Unit Target { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }

        public bool NoCooldown { get; }
        public bool NoManacost { get; }

        private CSharpScriptEngine _scriptEngine = Program.ResolveDependency<CSharpScriptEngine>();
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
            _currentChannelDuration = 0.0f;
            NoCooldown = !_game.Config.CooldownsEnabled;
            NoManacost = !_game.Config.ManaCostsEnabled;

            _scriptEngine = Program.ResolveDependency<CSharpScriptEngine>();

            JObject data;

            if (!_rafManager.ReadSpellData(spellName, out data))
            {
                _logger.LogCoreError("Couldn't find spell stats for " + spellName);
                return;
            }

            // Generate cooldown values for each level of the spell
            for (var i = 0; i < cooldown.Length; ++i)
            {
                // If Cooldown<level> exists, use its value
                if (_rafManager.DoesValueExist(data, "SpellData", "Cooldown" + (i + 1)))
                {
                    cooldown[i] = _rafManager.GetFloatValue(data, "SpellData", "Cooldown" + (i + 1));
                }
                else // If not, use the value in Cooldown
                {
                    cooldown[i] = _rafManager.GetFloatValue(data, "SpellData", "Cooldown");
                }
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

            for (var i = 0; i < channelDuration.Length; ++i)
            {
                if (_rafManager.GetFloatValue(data, "SpellData", "ChannelDuration" + (i + 1)) == 0)
                {
                    channelDuration[i] = _rafManager.GetFloatValue(data, "SpellData", "ChannelDuration");
                }
                else
                {
                    channelDuration[i] = _rafManager.GetFloatValue(data, "SpellData", "ChannelDuration" + (i + 1));
                }
            }

            if (slot == 13)
            {
                channelDuration = new float[5] { 8.0f, 8.0f, 8.0f, 8.0f, 8.0f };
            }

            if (_rafManager.DoesValueExist(data, "SpellData", "OverrideCastTime"))
            {
                CastTime = _rafManager.GetFloatValue(data, "SpellData", "OverrideCastTime");;
            }
            else
            {
                CastTime = (1.0f + _rafManager.GetFloatValue(data, "SpellData", "DelayCastOffsetPercent")) / 2.0f;
            }

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
                var key = "Effect" + (0 + i) + "Level0Amount";
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

        /// <summary>
        /// Called when the character casts the spell
        /// </summary>
        public virtual bool cast(float x, float y, Unit u = null, uint futureProjNetId = 0, uint spellNetId = 0)
        {
            X = x;
            Y = y;
            Target = u;
            this.futureProjNetId = futureProjNetId;
            this.spellNetId = spellNetId;

            if (_targetType == 1 && Target != null && Target.GetDistanceTo(Owner) > _castRange[Level - 1])
            {
                return false;
            }


            RunCast();

            if (CastTime > 0 && (Flags & (int)SpellFlag.SPELL_FLAG_InstantCast) == 0)
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

        
        private void RunCast()
        {
            var onStartCasting =
                _scriptEngine.GetStaticMethod<Action<Champion, Spell,Unit>>(GetSpellScriptClass(), GetSpellScriptName(), "onStartCasting");
            onStartCasting(Owner, this, Target);
        }
        
        public string GetSpellScriptClass()
        {
            if (Slot > 3)
            {
                return "Global";
            }
            else
            {
                return Owner.Model;
            }
        }
        public string GetSpellScriptName()
        {
            if (Slot > 3)
            {
                return _spellName;
            }
            else
            {
                return getStringForSlot();
            }
        }

        /// <summary>
        /// Called when the spell is finished casting and we're supposed to do things such as projectile spawning, etc.
        /// </summary>
        public virtual void finishCasting()
        {
            //Champion owner, Spell spell, Unit target
            var onFinishCasting =
                _scriptEngine.GetStaticMethod<Action<Champion, Spell, Unit>>(GetSpellScriptClass(), GetSpellScriptName(), "onFinishCasting");
            onFinishCasting(Owner, this, Target);
            if (getChannelDuration() == 0)
            {
                state = SpellState.STATE_COOLDOWN;

                _currentCooldown = getCooldown();

                if (Slot < 4)
                {
                    _game.PacketNotifier.NotifySetCooldown(Owner, Slot, _currentCooldown, getCooldown());
                }

                Owner.IsCastingSpell = false;
            }
        }

        /// <summary>
        /// Called when the spell is started casting and we're supposed to do things such as projectile spawning, etc.
        /// </summary>
        public virtual void channel()
        {
            state = SpellState.STATE_CHANNELING;
            _currentChannelDuration = getChannelDuration();
            RunChannelLua();
        }

        private void RunChannelLua()
        {
        }

        /// <summary>
        /// Called when the character finished channeling
        /// </summary>
        public virtual void finishChanneling()
        {
            state = SpellState.STATE_COOLDOWN;

            _currentCooldown = getCooldown();

            if (Slot < 4)
            {
                _game.PacketNotifier.NotifySetCooldown(Owner, Slot, _currentCooldown, getCooldown());
            }

            Owner.IsCastingSpell = false;
        }

        /// <summary>
        /// Called every diff milliseconds to update the spell
        /// </summary>
        public virtual void update(float diff)
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
                        if(getChannelDuration() > 0)
                        {
                            channel();
                        }
                    }
                    break;
                case SpellState.STATE_COOLDOWN:
                    _currentCooldown -= diff / 1000.0f;
                    if (_currentCooldown < 0)
                    {
                        state = SpellState.STATE_READY;
                    }
                    break;
                case SpellState.STATE_CHANNELING:
                    _currentChannelDuration -= diff / 1000.0f;
                    if(_currentChannelDuration <= 0)
                    {
                        finishChanneling();
                    }
                    break;
            }

            var onUpdate =
                _scriptEngine.GetStaticMethod<Action<double>>(GetSpellScriptClass(), GetSpellScriptName(), "onUpdate");
            onUpdate(diff);
        }

        /// <summary>
        /// Called by projectiles when they land / hit, this is where we apply damage/slows etc.
        /// </summary>
        public void applyEffects(Unit u, Projectile p = null)
        {
            if (!string.IsNullOrEmpty(hitEffectName))
            {
                ApiFunctionManager.AddParticleTarget(Owner, hitEffectName, u);
            }
            
            var applyEffects =
                _scriptEngine.GetStaticMethod<Action<Champion, Unit, Spell, Projectile>>(GetSpellScriptClass(), GetSpellScriptName(), "applyEffects");
            applyEffects(Owner, u, this, p);
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
            var p = new Projectile(
                Owner.X,
                Owner.Y,
                (int)LineWidth,
                Owner,
                new Target(toX, toY),
                this,
                ProjectileSpeed,
                nameMissile,
                ProjectileFlags != 0 ? ProjectileFlags : Flags
            );
            _game.Map.AddObject(p);
            if (!isServerOnly)
            {
                _game.PacketNotifier.NotifyProjectileSpawn(p);
            }
        }

        public void AddProjectileTarget(string nameMissile, Target target, bool isServerOnly = false)
        {
            var p = new Projectile(
                Owner.X,
                Owner.Y,
                (int)LineWidth,
                Owner,
                target,
                this,
                ProjectileSpeed,
                nameMissile,
                ProjectileFlags != 0 ? ProjectileFlags : Flags
            );
            _game.Map.AddObject(p);
            if (!isServerOnly)
            {
                _game.PacketNotifier.NotifyProjectileSpawn(p);
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
                name,
                ProjectileFlags != 0 ? ProjectileFlags : Flags
            );

            _game.Map.AddObject(p);
            if (!isServerOnly)
            {
                _game.PacketNotifier.NotifyProjectileSpawn(p);
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
                name,
                ProjectileFlags != 0 ? ProjectileFlags : Flags
            );

            _game.Map.AddObject(p);
            if (!isServerOnly)
            {
                _game.PacketNotifier.NotifyProjectileSpawn(p);
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
            _game.PacketNotifier.NotifySpellAnimation(target, animName);
        }

        public void setAnimation(string animation, string animation2, Unit target)
        {
            var animList = new List<string> { animation, animation2 };
            _game.PacketNotifier.NotifySetAnimation(target, animList);
        }

        public void resetAnimations(Unit target)
        {
            var animList = new List<string>();
            _game.PacketNotifier.NotifySetAnimation(target, animList);
        }

        public int getOtherSpellLevel(short slotId)
        {
            return Owner.Spells[slotId].Level;
        }

        public string GetChampionModel()
        {
            return Owner.Model;
        }

        /// <returns>spell's unique ID</returns>
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
        /**
         * TODO : Add in CDR % from champion's stat
         */
        public float getCooldown()
        {
            if (Level <= 0 || NoCooldown)
                return 0;

            return cooldown[Level - 1];
        }

        /// <returns>the mana/energy/health cost</returns>
        public float getCost()
        {
            if (Level <= 0 || NoManacost)
                return 0;

            return cost[Level - 1];
        }

        /// <returns>channelduration</returns>
        public float getChannelDuration()
        {
            if (Level <= 0)
                return channelDuration[0];
            return channelDuration[Level - 1];
        }

        public virtual void levelUp()
        {
            if (Level <= 5)
            {
                ++Level;
            }
            if (Slot < 4)
            {
                Owner.GetStats().ManaCost[Slot] = cost[Level - 1];
            }
        }

        public void SetCooldown(byte slot, float newCd)
        {
            var targetSpell = Owner.Spells[slot];

            if (newCd <= 0)
            {
                _game.PacketNotifier.NotifySetCooldown(Owner, slot, 0, 0);
                targetSpell.state = SpellState.STATE_READY;
                targetSpell._currentCooldown = 0;
            }
            else
            {
                _game.PacketNotifier.NotifySetCooldown(Owner, slot, newCd, targetSpell.getCooldown());
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

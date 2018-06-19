using LeagueSandbox.GameServer.Core.Logic;
using System;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;
using Newtonsoft.Json.Linq;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum SpellState
    {
        STATE_READY,
        STATE_CASTING,
        STATE_COOLDOWN,
        STATE_CHANNELING
    };

    public class Spell
    {
        public static bool CooldownsEnabled;
        public static bool ManaCostsEnabled;
        public Champion Owner { get; private set; }
        public short Level { get; private set; }
        public byte Slot { get; set; }
        public float CastTime { get; private set; } = 0;

        public string SpellName { get; private set; }
        public bool HasEmptyScript { get { return spellGameScript.GetType() == typeof(GameScriptEmpty); } }

        public SpellState state { get; protected set; } = SpellState.STATE_READY;
        public float CurrentCooldown { get; protected set; }
        public float CurrentCastTime { get; protected set; }
        public float CurrentChannelDuration { get; protected set; }
        public uint FutureProjNetId { get; protected set; }
        public uint SpellNetId { get; protected set; }

        public AttackableUnit Target { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float X2 { get; private set; }
        public float Y2 { get; private set; }

        private static CSharpScriptEngine _scriptEngine = Program.ResolveDependency<CSharpScriptEngine>();
        private static Logger _logger = Program.ResolveDependency<Logger>();
        private static Game _game = Program.ResolveDependency<Game>();

        private GameScript spellGameScript;
        protected NetworkIdManager _networkIdManager = Program.ResolveDependency<NetworkIdManager>();

        public SpellData SpellData { get; private set; }

        static Spell()
        {
            CooldownsEnabled = _game.Config.CooldownsEnabled;
            ManaCostsEnabled = _game.Config.ManaCostsEnabled;
        }

        public Spell(Champion owner, string spellName, byte slot)
        {
            Owner = owner;
            SpellName = spellName;
            Slot = slot;
            SpellData = _game.Config.ContentManager.GetSpellData(spellName);
            _scriptEngine = Program.ResolveDependency<CSharpScriptEngine>();

            //Set the game script for the spell
            if (!string.IsNullOrWhiteSpace(spellName))
            {
                spellGameScript = _scriptEngine.CreateObject<GameScript>("Spells", spellName);
            }
            if (spellGameScript == null)
            {
                spellGameScript = new GameScriptEmpty();
            }
            //Activate spell - Notes: Deactivate is never called as spell removal hasn't been added
            spellGameScript.OnActivate(owner);
        }

        // TODO: Make a better way to deactivate spells; this is placeholder for activated items.
        public void DeactivateSpell()
        {
            spellGameScript.OnDeactivate(Owner);
        }

        /// <summary>
        /// Called when the character casts the spell
        /// </summary>
        public virtual bool cast(float x, float y, float x2, float y2, AttackableUnit u = null)
        {
            if (HasEmptyScript) return false;

            var stats = Owner.GetStats();
            if ((SpellData.ManaCost[Level] * (1 - stats.getSpellCostReduction())) >= stats.CurrentMana || 
                state != SpellState.STATE_READY)
                return false;

            stats.CurrentMana = stats.CurrentMana - SpellData.ManaCost[Level] * (1 - stats.getSpellCostReduction());
            X = x;
            Y = y;
            X2 = x2;
            Y2 = y2;
            Target = u;
            FutureProjNetId = _networkIdManager.GetNewNetID();
            SpellNetId = _networkIdManager.GetNewNetID();

            if (SpellData.TargettingType == 1 && Target != null && Target.GetDistanceTo(Owner) > SpellData.CastRange[Level])
            {
                return false;
            }

            spellGameScript.OnStartCasting(Owner, this, Target);

            if (SpellData.GetCastTime() > 0 && (SpellData.Flags & (int)SpellFlag.SPELL_FLAG_InstantCast) == 0)
            {
                Owner.setPosition(Owner.X, Owner.Y);//stop moving serverside too. TODO: check for each spell if they stop movement or not
                state = SpellState.STATE_CASTING;
                CurrentCastTime = SpellData.GetCastTime();
            }
            else
            {
                finishCasting();
            }
            var response = new CastSpellResponse(this, x, y, x2, y2, FutureProjNetId, SpellNetId);
            _game.PacketHandlerManager.broadcastPacket(response, Channel.CHL_S2C);
            return true;
        }
        
        /// <summary>
        /// Called when the spell is finished casting and we're supposed to do things such as projectile spawning, etc.
        /// </summary>
        public virtual void finishCasting()
        {
            spellGameScript.OnFinishCasting(Owner, this, Target);
            if (SpellData.ChannelDuration[Level] == 0)
            {
                state = SpellState.STATE_COOLDOWN;

                CurrentCooldown = getCooldown();

                if (Slot < 4)
                {
                    _game.PacketNotifier.NotifySetCooldown(Owner, Slot, CurrentCooldown, getCooldown());
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
            CurrentChannelDuration = SpellData.ChannelDuration[Level];
        }

        /// <summary>
        /// Called when the character finished channeling
        /// </summary>
        public virtual void finishChanneling()
        {
            state = SpellState.STATE_COOLDOWN;

            CurrentCooldown = getCooldown();

            if (Slot < 4)
            {
                _game.PacketNotifier.NotifySetCooldown(Owner, Slot, CurrentCooldown, getCooldown());
            }

            Owner.IsCastingSpell = false;
        }

        /// <summary>
        /// Called every diff milliseconds to update the spell
        /// </summary>
        public virtual void update(float diff)
        {
            spellGameScript.OnUpdate(diff);
            switch (state)
            {
                case SpellState.STATE_READY:
                    break;
                case SpellState.STATE_CASTING:
                    Owner.IsCastingSpell = true;
                    CurrentCastTime -= diff / 1000.0f;
                    if (CurrentCastTime <= 0)
                    {
                        finishCasting();
                        if(SpellData.ChannelDuration[Level] > 0)
                        {
                            channel();
                        }
                    }
                    break;
                case SpellState.STATE_COOLDOWN:
                    CurrentCooldown -= diff / 1000.0f;
                    if (CurrentCooldown < 0)
                    {
                        state = SpellState.STATE_READY;
                    }
                    break;
                case SpellState.STATE_CHANNELING:
                    CurrentChannelDuration -= diff / 1000.0f;
                    if(CurrentChannelDuration <= 0)
                    {
                        finishChanneling();
                    }
                    break;
            }
        }

        /// <summary>
        /// Called by projectiles when they land / hit, this is where we apply damage/slows etc.
        /// </summary>
        public void applyEffects(AttackableUnit u, Projectile p = null)
        {
            if (SpellData.HaveHitEffect && !string.IsNullOrEmpty(SpellData.HitEffectName))
            {
                ApiFunctionManager.AddParticleTarget(Owner, SpellData.HitEffectName, u);
            }

            spellGameScript.ApplyEffects(Owner, u, this, p);
        }

        public void AddProjectile(string nameMissile, float toX, float toY, bool isServerOnly = false)
        {
            var p = new Projectile(
                Owner.X,
                Owner.Y,
                (int) SpellData.LineWidth,
                Owner,
                new Target(toX, toY),
                this,
                SpellData.MissileSpeed,
                nameMissile,
                SpellData.Flags
            );
            _game.ObjectManager.AddObject(p);
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
                (int)SpellData.LineWidth,
                Owner,
                target,
                this,
                SpellData.MissileSpeed,
                nameMissile,
                SpellData.Flags
            );
            _game.ObjectManager.AddObject(p);
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
                (int)SpellData.LineWidth,
                Owner,
                new Target(toX, toY),
                this,
                SpellData.Flags,
                affectAsCastIsOver
            );
            _game.ObjectManager.AddObject(l);
        }

        public void spellAnimation(string animName, AttackableUnit target)
        {
            _game.PacketNotifier.NotifySpellAnimation(target, animName);
        }

        /// <returns>spell's unique ID</returns>
        public int getId()
        {
            return (int)HashFunctions.HashString(SpellName);
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
                case 14:
                    return "Passive";
            }

            return "undefined";
        }

        /**
         * TODO : Add in CDR % from champion's stat
         */
        public float getCooldown()
        {
            return SpellData.Cooldown[Level];
        }

        public virtual void levelUp()
        {
            if (Level <= 5)
            {
                ++Level;
            }
            if (Slot < 4)
            {
                Owner.GetStats().ManaCost[Slot] = SpellData.ManaCost[Level];
            }
            ApiEventManager.OnLevelUpSpell.Publish(this, Owner);
        }

        public void SetCooldown(byte slot, float newCd)
        {
            var targetSpell = Owner.Spells[slot];

            if (newCd <= 0)
            {
                _game.PacketNotifier.NotifySetCooldown(Owner, slot, 0, 0);
                targetSpell.state = SpellState.STATE_READY;
                targetSpell.CurrentCooldown = 0;
            }
            else
            {
                _game.PacketNotifier.NotifySetCooldown(Owner, slot, newCd, targetSpell.getCooldown());
                targetSpell.state = SpellState.STATE_COOLDOWN;
                targetSpell.CurrentCooldown = newCd;
            }
        }

        public void LowerCooldown(byte slot, float lowerValue)
        {
            SetCooldown(slot, Owner.Spells[slot].CurrentCooldown - lowerValue);
        }
    }
}

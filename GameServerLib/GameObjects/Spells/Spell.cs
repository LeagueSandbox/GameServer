using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Missiles;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.Packets;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.Spells
{
    public class Spell : ISpell
    {

        public IChampion Owner { get; private set; }
        public byte Level { get; private set; }
        public byte Slot { get; set; }
        public float CastTime { get; private set; } = 0;

        public string SpellName { get; private set; }
        public bool HasEmptyScript => _spellGameScript.GetType() == typeof(GameScriptEmpty);

        public SpellState State { get; protected set; } = SpellState.STATE_READY;
        public float CurrentCooldown { get; protected set; }
        public float CurrentCastTime { get; protected set; }
        public float CurrentChannelDuration { get; protected set; }
        public uint FutureProjNetId { get; protected set; }
        public uint SpellNetId { get; protected set; }

        public IAttackableUnit Target { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }
        public float X2 { get; private set; }
        public float Y2 { get; private set; }

        private CSharpScriptEngine _scriptEngine;
        private Game _game;
        protected INetworkIdManager _networkIdManager;

        private IGameScript _spellGameScript;

        public ISpellData SpellData { get; private set; }

        public Spell(Game game, IChampion owner, string spellName, byte slot)
        {
            Owner = owner;
            SpellName = spellName;
            Slot = slot;
            _game = game;
            SpellData = game.Config.ContentManager.GetSpellData(spellName);
            _scriptEngine = game.ScriptEngine;
            _networkIdManager = game.NetworkIdManager;

            //Set the game script for the spell
            _spellGameScript = _scriptEngine.CreateObject<IGameScript>("Spells", spellName) ?? new GameScriptEmpty();
            //Activate spell - Notes: Deactivate is never called as spell removal hasn't been added
            _spellGameScript.OnActivate(owner);
        }

        void ISpell.Deactivate()
        {
            _spellGameScript.OnDeactivate(Owner);
        }

        /// <summary>
        /// Called when the character casts the spell
        /// </summary>
        public virtual bool Cast(float x, float y, float x2, float y2, IAttackableUnit u = null)
        {
            if (HasEmptyScript)
            {
                return false;
            }

            var stats = Owner.Stats;
            if (SpellData.ManaCost[Level] * (1 - stats.SpellCostReduction) >= stats.CurrentMana ||
                State != SpellState.STATE_READY)
            {
                return false;
            }

            if (_game.Config.ManaCostsEnabled)
            {
                stats.CurrentMana = stats.CurrentMana - SpellData.ManaCost[Level] * (1 - stats.SpellCostReduction);
            }

            X = x;
            Y = y;
            X2 = x2;
            Y2 = y2;
            Target = u;
            FutureProjNetId = _networkIdManager.GetNewNetId();
            SpellNetId = _networkIdManager.GetNewNetId();

            if (SpellData.TargettingType == 1 && Target != null && Target.GetDistanceTo(Owner) > SpellData.CastRange[Level])
            {
                return false;
            }

            _spellGameScript.OnStartCasting(Owner, this, Target);

            if (SpellData.GetCastTime() > 0 && (SpellData.Flags & (int)SpellFlag.SPELL_FLAG_INSTANT_CAST) == 0)
            {
                Owner.SetPosition(Owner.X, Owner.Y); //stop moving serverside too. TODO: check for each spell if they stop movement or not
                State = SpellState.STATE_CASTING;
                CurrentCastTime = SpellData.GetCastTime();
            }
            else
            {
                FinishCasting();
            }

            _game.PacketNotifier.NotifyCastSpell(_game.Map.NavGrid, this, new Vector2(x, y) , new Vector2(x2, y2), FutureProjNetId, SpellNetId);
            return true;
        }

        /// <summary>
        /// Called when the spell is finished casting and we're supposed to do things such as projectile spawning, etc.
        /// </summary>
        public virtual void FinishCasting()
        {
            _spellGameScript.OnFinishCasting(Owner, this, Target);
            if (SpellData.ChannelDuration[Level] <= 0)
            {
                State = SpellState.STATE_COOLDOWN;

                CurrentCooldown = GetCooldown();

                if (Slot < 4)
                {
                    _game.PacketNotifier.NotifySetCooldown(Owner, Slot, CurrentCooldown, GetCooldown());
                }

                Owner.IsCastingSpell = false;
            }
        }

        /// <summary>
        /// Called when the spell is started casting and we're supposed to do things such as projectile spawning, etc.
        /// </summary>
        public virtual void Channel()
        {
            State = SpellState.STATE_CHANNELING;
            CurrentChannelDuration = SpellData.ChannelDuration[Level];
        }

        /// <summary>
        /// Called when the character finished channeling
        /// </summary>
        public virtual void FinishChanneling()
        {
            State = SpellState.STATE_COOLDOWN;

            CurrentCooldown = GetCooldown();

            if (Slot < 4)
            {
                _game.PacketNotifier.NotifySetCooldown(Owner, Slot, CurrentCooldown, GetCooldown());
            }

            Owner.IsCastingSpell = false;
        }

        /// <summary>
        /// Called every diff milliseconds to update the spell
        /// </summary>
        public virtual void Update(float diff)
        {
            switch (State)
            {
                case SpellState.STATE_READY:
                    break;
                case SpellState.STATE_CASTING:
                    Owner.IsCastingSpell = true;
                    CurrentCastTime -= diff / 1000.0f;
                    if (CurrentCastTime <= 0)
                    {
                        FinishCasting();
                        if (SpellData.ChannelDuration[Level] > 0)
                        {
                            Channel();
                        }
                    }
                    break;
                case SpellState.STATE_COOLDOWN:
                    CurrentCooldown -= diff / 1000.0f;
                    if (CurrentCooldown < 0)
                    {
                        State = SpellState.STATE_READY;
                    }
                    break;
                case SpellState.STATE_CHANNELING:
                    CurrentChannelDuration -= diff / 1000.0f;
                    if (CurrentChannelDuration <= 0)
                    {
                        FinishChanneling();
                    }
                    break;
            }
        }

        /// <summary>
        /// Called by projectiles when they land / hit, this is where we apply damage/slows etc.
        /// </summary>
        public void ApplyEffects(IAttackableUnit u, IProjectile p = null)
        {
            if (SpellData.HaveHitEffect && !string.IsNullOrEmpty(SpellData.HitEffectName))
            {
                ApiFunctionManager.AddParticleTarget(Owner, SpellData.HitEffectName, u);
            }

            _spellGameScript.ApplyEffects(Owner, u, this, p);
        }

        public void AddProjectile(string nameMissile, float toX, float toY, bool isServerOnly = false)
        {
            var p = new Projectile(
                _game,
                Owner.X,
                Owner.Y,
                (int)SpellData.LineWidth,
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

        public void AddProjectileTarget(string nameMissile, ITarget target, bool isServerOnly = false)
        {
            var p = new Projectile(
                _game,
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

        public void AddLaser(string effectName, float toX, float toY, bool affectAsCastIsOver = true)
        {
            var l = new Laser(
                _game,
                Owner.X,
                Owner.Y,
                (int)SpellData.LineWidth,
                Owner,
                new Target(toX, toY),
                this,
                effectName,
                SpellData.Flags,
                affectAsCastIsOver
            );
            _game.ObjectManager.AddObject(l);
        }

        public void AddCone(string effectName, float toX, float toY, float angleDeg, bool affectAsCastIsOver = true)
        {
            var c = new Cone(
                _game,
                Owner.X,
                Owner.Y,
                (int)SpellData.LineWidth,
                Owner,
                new Target(toX, toY),
                this,
                effectName,
                SpellData.Flags,
                affectAsCastIsOver,
                angleDeg
            );
            _game.ObjectManager.AddObject(c);
        }

        public void SpellAnimation(string animName, IAttackableUnit target)
        {
            _game.PacketNotifier.NotifySpellAnimation(target, animName);
        }

        /// <returns>spell's unique ID</returns>
        public int GetId()
        {
            return (int)HashFunctions.HashString(SpellName);
        }

        public string GetStringForSlot()
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

        public float GetCooldown()
        {
            return _game.Config.CooldownsEnabled ? SpellData.Cooldown[Level] * (1 - Owner.Stats.CooldownReduction.Total) : 0;
        }

        public void LevelUp()
        {
            if (Level <= 5)
            {
                ++Level;
            }

            if (Slot < 4)
            {
                Owner.Stats.ManaCost[Slot] = SpellData.ManaCost[Level];
            }
        }

        public void SetCooldown(float newCd)
        {
            if (newCd <= 0)
            {
                _game.PacketNotifier.NotifySetCooldown(Owner, Slot, 0, 0);
                State = SpellState.STATE_READY;
                CurrentCooldown = 0;
            }
            else
            {
                _game.PacketNotifier.NotifySetCooldown(Owner, Slot, newCd, GetCooldown());
                State = SpellState.STATE_COOLDOWN;
                CurrentCooldown = newCd;
            }
        }

        public void LowerCooldown(float lowerValue)
        {
            SetCooldown(CurrentCooldown - lowerValue);
        }
    }
}

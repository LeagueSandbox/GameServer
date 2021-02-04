using GameServerCore.Content;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Spell.Missile;
using LeagueSandbox.GameServer.GameObjects.Spell.Sector;
using LeagueSandbox.GameServer.Packets;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Collections.Generic;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.Spell
{
    public class Spell : ISpell
    {
        // Crucial Vars.
        private CSharpScriptEngine _scriptEngine;
        private Game _game;
        protected NetworkIdManager _networkIdManager;
        private uint _futureProjNetId;
        private IGameScript _spellGameScript;

        public ICastInfo CastInfo { get; private set; } = new CastInfo();

        public string SpellName { get; private set; }
        public bool HasEmptyScript => _spellGameScript.GetType() == typeof(GameScriptEmpty);

        public SpellState State { get; protected set; } = SpellState.STATE_READY;
        public float CurrentCooldown { get; protected set; }
        public float CurrentCastTime { get; protected set; }
        public float CurrentChannelDuration { get; protected set; }
        public float CurrentDelayTime { get; protected set; }
        public bool Toggle { get; protected set; }
        public Dictionary<uint, IProjectile> Projectiles { get; protected set; }

        public ISpellData SpellData { get; private set; }

        public Spell(Game game, IObjAiBase owner, string spellName, byte slot)
        {
            _game = game;
            _scriptEngine = game.ScriptEngine;
            _networkIdManager = game.NetworkIdManager;

            CastInfo.Owner = owner;
            SpellName = spellName;
            CastInfo.SpellHash = (uint)GetId();
            CastInfo.AttackSpeedModifier = owner.Stats.AttackSpeedMultiplier.Total;
            CastInfo.PackageHash = owner.GetObjHash();
            CastInfo.Targets = new List<ICastTarget>();
            CastInfo.SpellSlot = slot;

            CastInfo.IsSecondAutoAttack = false;

            if (CastInfo.SpellSlot >= 64)
            {
                CastInfo.IsAutoAttack = true;
            }

            SpellData = game.Config.ContentManager.GetSpellData(spellName);

            //Set the game script for the spell
            _spellGameScript = _scriptEngine.CreateObject<IGameScript>("Spells", spellName) ?? new GameScriptEmpty();
            //Activate spell - Notes: Deactivate is never called as spell removal hasn't been added
            _spellGameScript.OnActivate(owner, this);
        }

        public void AddCone(string effectName, Vector2 target, float angleDeg, HitResult hitResult = HitResult.HIT_Normal, bool affectAsCastIsOver = true)
        {
            ISpellData projectileSpellData = _game.Config.ContentManager.GetSpellData(effectName);

            var castInfo = new CastInfo
            {
                SpellHash = HashFunctions.HashString(effectName), // TODO: Verify
                SpellNetID = _futureProjNetId,
                SpellLevel = 1, // TODO: Verify
                AttackSpeedModifier = CastInfo.Owner.Stats.AttackSpeedMultiplier.Total,
                Owner = CastInfo.Owner,
                SpellChainOwnerNetID = CastInfo.SpellChainOwnerNetID,
                PackageHash = CastInfo.PackageHash,
                MissileNetID = _futureProjNetId,
                TargetPosition = CastInfo.Owner.GetPosition3D(), // TODO: Verify
                TargetPositionEnd = new Vector3(target.X, _game.Map.NavigationGrid.GetHeightAtLocation(target.X, target.Y), target.Y),
                DesignerCastTime = 0, // TODO: Verify
                ExtraCastTime = 0, // TODO: Verify
                DesignerTotalTime = 0, // TODO: Verify
                Cooldown = projectileSpellData.Cooldown[0], // TODO: Verify
                StartCastTime = 0, // TODO: Verify
                IsAutoAttack = false,
                IsSecondAutoAttack = false,
                IsForceCastingOrChannel = false, // TODO: Verify
                IsOverrideCastPosition = false, // TODO: Verify
                IsClickCasted = false, // TODO: Verify
                SpellSlot = 0x33, // TODO: Verify
                ManaCost = projectileSpellData.ManaCost[0], // TODO: Verify
                SpellCastLaunchPosition = CastInfo.SpellCastLaunchPosition, // TODO: Verify
                AmmoUsed = 0, // TODO: Verify
                AmmoRechargeTime = 0 // TODO: Verify
            };

            // TODO: implement check for IsForceCastingOrChannel and IsOverrideCastPosition
            if (projectileSpellData.CastType == (int)CastType.CAST_TargetMissile)
            {
                castInfo.IsClickCasted = true; // TODO: Verify
            }

            for (var i = 0; i < CastInfo.Owner.CharData.SpellNames.Length; i++)
            {
                if (CastInfo.Owner.CharData.SpellNames[i] == effectName)
                {
                    castInfo.SpellSlot = (byte)(i + 51); // TODO: Verify
                }
            }

            var castTargets = new List<ICastTarget>();
            var castTarget = new CastTarget(null, hitResult); // TODO: Verify if 0 NetId is OK
            castTargets.Add(castTarget);
            castInfo.Targets = castTargets;

            var c = new Cone(
                _game,
                // TODO: Change this to a parameter.
                (int)SpellData.LineWidth,
                this,
                castInfo,
                effectName,
                SpellData.Flags,
                affectAsCastIsOver,
                angleDeg,
                _futureProjNetId
            );
            Projectiles.Add(c.NetId, c);
            _game.ObjectManager.AddObject(c);
            _futureProjNetId = _networkIdManager.GetNewNetId();
        }

        public void AddLaser(string effectName, Vector2 target, HitResult hitResult = HitResult.HIT_Normal, bool affectAsCastIsOver = true)
        {
            ISpellData projectileSpellData = _game.Config.ContentManager.GetSpellData(effectName);

            var castInfo = new CastInfo
            {
                SpellHash = HashFunctions.HashString(effectName), // TODO: Verify

                SpellNetID = _futureProjNetId,
                SpellLevel = 1, // TODO: Verify
                AttackSpeedModifier = CastInfo.Owner.Stats.AttackSpeedMultiplier.Total,
                Owner = CastInfo.Owner,
                SpellChainOwnerNetID = CastInfo.SpellChainOwnerNetID,
                PackageHash = CastInfo.PackageHash,
                MissileNetID = _futureProjNetId,
                TargetPosition = CastInfo.Owner.GetPosition3D(), // TODO: Verify
                TargetPositionEnd = new Vector3(target.X, _game.Map.NavigationGrid.GetHeightAtLocation(target.X, target.Y), target.Y),
                DesignerCastTime = 0, // TODO: Verify
                ExtraCastTime = 0, // TODO: Verify
                DesignerTotalTime = 0, // TODO: Verify
                Cooldown = projectileSpellData.Cooldown[0], // TODO: Verify
                StartCastTime = 0, // TODO: Verify
                IsAutoAttack = false,
                IsSecondAutoAttack = false,
                IsForceCastingOrChannel = false, // TODO: Verify
                IsOverrideCastPosition = false, // TODO: Verify
                IsClickCasted = false, // TODO: Verify
                SpellSlot = 0x33, // TODO: Verify
                ManaCost = projectileSpellData.ManaCost[0], // TODO: Verify
                SpellCastLaunchPosition = CastInfo.SpellCastLaunchPosition, // TODO: Verify
                AmmoUsed = 0, // TODO: Verify
                AmmoRechargeTime = 0 // TODO: Verify
            };

            // TODO: implement check for IsForceCastingOrChannel and IsOverrideCastPosition
            if (projectileSpellData.CastType == (int)CastType.CAST_TargetMissile)
            {
                castInfo.IsClickCasted = true; // TODO: Verify
            }

            for (var i = 0; i < CastInfo.Owner.CharData.SpellNames.Length; i++)
            {
                if (CastInfo.Owner.CharData.SpellNames[i] == effectName)
                {
                    castInfo.SpellSlot = (byte)(i + 51); // TODO: Verify
                }
            }

            var castTargets = new List<ICastTarget>();

            var castTarget = new CastTarget(null, hitResult); // TODO: Verify if 0 NetId is OK
            castTargets.Add(castTarget);
            castInfo.Targets = castTargets;

            var l = new Laser(
                _game,
                (int)SpellData.LineWidth, // TODO: Change this to a parameter.
                this,
                castInfo,
                effectName,
                SpellData.Flags,
                affectAsCastIsOver, // TODO: Verify if this is needed.
                _futureProjNetId
            );
            Projectiles.Add(l.NetId, l);
            _game.ObjectManager.AddObject(l);
            _futureProjNetId = _networkIdManager.GetNewNetId();
        }

        public void AddProjectile(string nameMissile, Vector2 from, Vector2 to, HitResult hitResult = HitResult.HIT_Normal, bool isServerOnly = false)
        {
            var castTargets = new List<ICastTarget>();
            var castTarget = new CastTarget(null, hitResult); // TODO: Verify if 0 NetId is OK
            castTargets.Add(castTarget);
            ICastInfo castInfo = new CastInfo
            {
                SpellNetID = _futureProjNetId,
                SpellLevel = 1, // TODO: Verify
                AttackSpeedModifier = CastInfo.AttackSpeedModifier,
                Owner = CastInfo.Owner,
                SpellChainOwnerNetID = CastInfo.SpellChainOwnerNetID,
                PackageHash = CastInfo.PackageHash,
                MissileNetID = _futureProjNetId,
                TargetPosition = new Vector3(from.X, _game.Map.NavigationGrid.GetHeightAtLocation(from.X, from.Y), from.Y),
                TargetPositionEnd = new Vector3(to.X, _game.Map.NavigationGrid.GetHeightAtLocation(to.X, to.Y), to.Y),
                Targets = castTargets,
                DesignerCastTime = 0, // TODO: Verify
                ExtraCastTime = 0, // TODO: Verify
                DesignerTotalTime = 0, // TODO: Verify
                Cooldown = 0, // TODO: Verify
                StartCastTime = 0, // TODO: Verify
                IsForceCastingOrChannel = false, // TODO: Verify
                IsOverrideCastPosition = false, // TODO: Verify
                ManaCost = 0,
                SpellCastLaunchPosition = new Vector3(from.X, _game.Map.NavigationGrid.GetHeightAtLocation(from.X, from.Y), from.Y),
                AmmoUsed = 0, // TODO: Verify
                AmmoRechargeTime = 0 // TODO: Verify
            };

            IProjectile p;
            if (nameMissile == "")
            {
                castInfo.SpellHash = HashFunctions.HashString(""); // TODO: Verify
                castInfo.IsAutoAttack = true;
                castInfo.IsSecondAutoAttack = CastInfo.Owner.HasMadeInitialAttack;
                castInfo.IsClickCasted = false; // TODO: Verify

                p = new Projectile(
                    _game,
                    5,
                    this,
                    castInfo,
                    SpellData.MissileSpeed,
                    nameMissile,
                    0,
                    _futureProjNetId
                );
            }
            else
            {
                ISpellData projectileSpellData = _game.Config.ContentManager.GetSpellData(nameMissile);

                castInfo.SpellHash = HashFunctions.HashString(nameMissile); // TODO: Verify
                castInfo.IsAutoAttack = false;
                castInfo.IsSecondAutoAttack = false;
                castInfo.IsClickCasted = false; // TODO: Verify
                castInfo.SpellSlot = 0x33; // TODO: Verify

                // TODO: implement check for IsForceCastingOrChannel and IsOverrideCastPosition
                if (projectileSpellData.CastType == (int)CastType.CAST_TargetMissile)
                {
                    castInfo.IsClickCasted = true; // TODO: Verify
                }

                for (var i = 0; i < CastInfo.Owner.CharData.SpellNames.Length; i++)
                {
                    if (CastInfo.Owner.CharData.SpellNames[i] == nameMissile)
                    {
                        castInfo.SpellSlot = (byte)(i + 51); // TODO: Verify
                    }
                }

                p = new Projectile(
                    _game,
                    (int)projectileSpellData.LineWidth,
                    this,
                    castInfo,
                    projectileSpellData.MissileSpeed,
                    nameMissile,
                    projectileSpellData.Flags,
                    _futureProjNetId,
                    isServerOnly
                );
            }

            Projectiles.Add(p.NetId, p);
            _game.ObjectManager.AddObject(p);

            if (!isServerOnly && nameMissile != "")
            {
                _game.PacketNotifier.NotifyMissileReplication(p);
            }
            else if (nameMissile != "")
            {
                _game.PacketNotifier.NotifyForceCreateMissile(p);
            }

            _futureProjNetId = _networkIdManager.GetNewNetId();
        }

        public void AddProjectileTarget(string nameMissile, IAttackableUnit target, HitResult hitResult = HitResult.HIT_Normal, bool isServerOnly = false)
        {
            var castTargets = new List<ICastTarget>();
            var castTarget = new CastTarget(target, hitResult); // TODO: Verify if 0 NetId is OK
            castTargets.Add(castTarget);
            ICastInfo castInfo = new CastInfo
            {
                SpellNetID = _futureProjNetId,
                SpellLevel = 1, // TODO: Verify
                AttackSpeedModifier = CastInfo.AttackSpeedModifier,
                Owner = CastInfo.Owner,
                SpellChainOwnerNetID = CastInfo.SpellChainOwnerNetID,
                PackageHash = CastInfo.PackageHash,
                MissileNetID = _futureProjNetId,
                TargetPosition = target.GetPosition3D(),
                TargetPositionEnd = target.GetPosition3D(),
                Targets = castTargets,
                DesignerCastTime = 0, // TODO: Verify
                ExtraCastTime = 0, // TODO: Verify
                DesignerTotalTime = 0, // TODO: Verify
                Cooldown = 0, // TODO: Verify
                StartCastTime = 0, // TODO: Verify
                IsAutoAttack = true,
                IsSecondAutoAttack = CastInfo.Owner.HasMadeInitialAttack,
                IsForceCastingOrChannel = false, // TODO: Verify
                IsOverrideCastPosition = false, // TODO: Verify
            };

            IProjectile p;

            if (nameMissile == "")
            {
                castInfo.SpellHash = HashFunctions.HashString(""); // TODO: Verify
                castInfo.IsClickCasted = true; // TODO: Verify
                castInfo.ManaCost = 0;
                castInfo.SpellCastLaunchPosition = CastInfo.Owner.GetPosition3D();
                castInfo.AmmoUsed = 0; // TODO: Verify
                castInfo.AmmoRechargeTime = 0; // TODO: Verify

                p = new Projectile(
                    _game,
                    5,
                    this,
                    castInfo,
                    SpellData.MissileSpeed,
                    "",
                    0,
                    _futureProjNetId
                );
            }
            else
            {
                ISpellData projectileSpellData = _game.Config.ContentManager.GetSpellData(nameMissile);

                castInfo.SpellHash = HashFunctions.HashString(nameMissile); // TODO: Verify
                castInfo.IsClickCasted = false; // TODO: Verify
                castInfo.SpellSlot = 0x33; // TODO: Verify
                castInfo.ManaCost = projectileSpellData.ManaCost[0]; // TODO: Verify
                castInfo.SpellCastLaunchPosition = CastInfo.SpellCastLaunchPosition; // TODO: Verify
                castInfo.AmmoUsed = 0; // TODO: Verify
                castInfo.AmmoRechargeTime = 0; // TODO: Verify

                // TODO: implement check for IsForceCastingOrChannel and IsOverrideCastPosition
                if (projectileSpellData.CastType == (int)CastType.CAST_TargetMissile)
                {
                    castInfo.IsClickCasted = true; // TODO: Verify
                }

                for (var i = 0; i < CastInfo.Owner.CharData.SpellNames.Length; i++)
                {
                    if (CastInfo.Owner.CharData.SpellNames[i] == nameMissile)
                    {
                        castInfo.SpellSlot = (byte)(i + 51); // TODO: Verify
                    }
                }

                p = new Projectile(
                    _game,
                    (int)projectileSpellData.LineWidth,
                    this,
                    castInfo,
                    projectileSpellData.MissileSpeed,
                    nameMissile,
                    projectileSpellData.Flags,
                    _futureProjNetId,
                    isServerOnly
                );
            }

            Projectiles.Add(p.NetId, p);
            _game.ObjectManager.AddObject(p);

            if (!isServerOnly && nameMissile != "")
            {
                _game.PacketNotifier.NotifyMissileReplication(p);
            }
            else if (nameMissile != "")
            {
                _game.PacketNotifier.NotifyForceCreateMissile(p);
            }

            _futureProjNetId = _networkIdManager.GetNewNetId();
        }

        /// <summary>
        /// Called by projectiles when they land / hit, this is where we apply damage/slows etc.
        /// </summary>
        public void ApplyEffects(IAttackableUnit u, IProjectile p = null)
        {
            if (SpellData.HaveHitEffect && !string.IsNullOrEmpty(SpellData.HitEffectName))
            {
                ApiFunctionManager.AddParticleTarget(CastInfo.Owner, SpellData.HitEffectName, u);
            }

            if (!HasEmptyScript)
            {
                _spellGameScript.ApplyEffects(CastInfo.Owner, u, this, p);
            }
            if (p != null && p.IsToRemove())
            {
                Projectiles.Remove(p.NetId);
            }
        }

        /// <summary>
        /// Called when the character casts the spell
        /// </summary>
        public virtual bool Cast(Vector2 start, Vector2 end, IAttackableUnit unit = null)
        {
            var stats = CastInfo.Owner.Stats;

            if ((SpellData.ManaCost[CastInfo.SpellLevel] * (1 - stats.SpellCostReduction) >= stats.CurrentMana && !CastInfo.IsAutoAttack) || State != SpellState.STATE_READY)
            {
                return false;
            }

            CastInfo.SpellNetID = _networkIdManager.GetNewNetId();

            CastInfo.AttackSpeedModifier = stats.AttackSpeedMultiplier.Total;

            if (_game.Config.ManaCostsEnabled)
            {
                stats.CurrentMana -= SpellData.ManaCost[CastInfo.SpellLevel] * (1 - stats.SpellCostReduction);
            }

            _futureProjNetId = _networkIdManager.GetNewNetId();

            CastInfo.MissileNetID = _futureProjNetId;
            CastInfo.TargetPosition = new Vector3(start.X, _game.Map.NavigationGrid.GetHeightAtLocation(start.X, start.Y), start.Y);
            CastInfo.TargetPositionEnd = new Vector3(end.X, _game.Map.NavigationGrid.GetHeightAtLocation(end.X, end.Y), end.Y);

            CastInfo.Targets.Clear();
            // TODO: Account for multiple targets
            CastInfo.Targets.Add(new CastTarget(unit, CastTarget.GetHitResult(unit, CastInfo)));

            CastInfo.ExtraCastTime = 0.0f; // TODO: Unhardcode
            CastInfo.Cooldown = GetCooldown();
            CastInfo.StartCastTime = 0.0f; // TODO: Unhardcode

            // For things like Garen Q, Leona Q, and TF W (and probably more)
            if (SpellData.ConsideredAsAutoAttack || SpellData.UseAutoattackCastTime || CastInfo.UseAttackCastDelay) // TODO: Verify
            {
                CastInfo.IsAutoAttack = false;
                CastInfo.DesignerCastTime = SpellData.GetCharacterAttackCastDelay(CastInfo.AttackSpeedModifier, CastInfo.Owner.CharData.AttackDelayOffsetPercent[0], CastInfo.Owner.CharData.AttackDelayCastOffsetPercent[0], CastInfo.Owner.CharData.AttackDelayCastOffsetPercentAttackSpeedRatio[0]);
                CastInfo.DesignerTotalTime = SpellData.GetCharacterAttackDelay(CastInfo.AttackSpeedModifier, CastInfo.Owner.CharData.AttackDelayOffsetPercent[0]);
            }
            else
            {
                CastInfo.DesignerCastTime = SpellData.GetCastTime();
                CastInfo.DesignerTotalTime = SpellData.GetCastTime() + SpellData.ChannelDuration[CastInfo.SpellLevel];
            }

            // Otherwise, use the normal auto attack setup
            if (CastInfo.IsAutoAttack)
            {
                CastInfo.UseAttackCastTime = true;
                CastInfo.AmmoUsed = 0; // TODO: Verify
                CastInfo.AmmoRechargeTime = 0; // TODO: Verify
                CastInfo.IsSecondAutoAttack = CastInfo.Owner.HasMadeInitialAttack;
            }
            else
            {
                CastInfo.AmmoUsed = 1; // TODO: Verify
                CastInfo.AmmoRechargeTime = CastInfo.Cooldown; // TODO: Verify
            }

            // TODO: implement check for IsForceCastingOrChannel and IsOverrideCastPosition
            if (SpellData.CastType == (int)CastType.CAST_TargetMissile)
            {
                // TODO: Verify
                CastInfo.IsClickCasted = true;
            }

            // TODO: Verify
            CastInfo.SpellCastLaunchPosition = CastInfo.Owner.GetPosition3D();

            Projectiles = new Dictionary<uint, IProjectile>();

            if (!CastInfo.IsAutoAttack && SpellData.TargetingType == TargetingType.Target && CastInfo.Targets[0].Unit != null
                && Vector2.DistanceSquared(CastInfo.Targets[0].Unit.Position, CastInfo.Owner.Position) > SpellData.CastRange[CastInfo.SpellLevel] * SpellData.CastRange[CastInfo.SpellLevel])
            {
                CastInfo.Owner.SetSpellToCast(this);
                return false;
            }

            _spellGameScript.OnStartCasting(CastInfo.Owner, this, CastInfo.Targets[0].Unit);

            if (CastInfo.IsAutoAttack || CastInfo.UseAttackCastTime)
            {
                int startIndex = 64;
                if (CastInfo.SpellSlot >= 44 && CastInfo.SpellSlot < 51)
                {
                    startIndex = CastInfo.SpellSlot;
                }

                float autoAttackTotalTime = CastInfo.Owner.CharData.GlobalCharData.AttackDelay * (1.0f + CastInfo.Owner.CharData.AttackDelayOffsetPercent[0]);
                CurrentCastTime = autoAttackTotalTime * (CastInfo.Owner.CharData.GlobalCharData.AttackDelayCastPercent + CastInfo.Owner.CharData.AttackDelayCastOffsetPercent[CastInfo.SpellSlot - startIndex]);

                if (CastInfo.UseAttackCastTime)
                {
                    CastInfo.DesignerCastTime = CurrentCastTime;
                    // TODO: Verify
                    CastInfo.DesignerTotalTime = CurrentCastTime + SpellData.ChannelDuration[CastInfo.SpellLevel];
                    State = SpellState.STATE_CASTING;
                    _game.PacketNotifier.NotifyNPC_CastSpellAns(this, _futureProjNetId);
                }
                var attackType = AttackType.ATTACK_TYPE_TARGETED;
                if (CastInfo.Owner.IsMelee)
                {
                    attackType = AttackType.ATTACK_TYPE_MELEE;
                }
                // TODO: Probably want to use this for both attacks and spells
                _game.PacketNotifier.NotifyS2C_UnitSetLookAt(CastInfo.Owner, CastInfo.Targets[0].Unit, attackType);

                State = SpellState.STATE_CASTING;

                if (!CastInfo.IsSecondAutoAttack)
                {
                    _game.PacketNotifier.NotifyBasic_Attack_Pos(CastInfo.Owner, CastInfo.Targets[0].Unit, _futureProjNetId, CastInfo.Owner.IsNextAutoCrit);
                }
                else
                {
                    _game.PacketNotifier.NotifyBasic_Attack(CastInfo.Owner, CastInfo.Targets[0].Unit, _futureProjNetId, CastInfo.Owner.IsNextAutoCrit, CastInfo.Owner.HasMadeInitialAttack);
                }
            }
            else
            {
                if (CastInfo.DesignerCastTime > 0)
                {
                    if (!CastInfo.UseAttackCastDelay)
                    {
                        if ((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_INSTANT_CAST) == 0)
                        {
                            State = SpellState.STATE_CASTING;
                        }
                        else
                        {
                            FinishCasting();
                        }
                    }
                    else
                    {
                        State = SpellState.STATE_CASTING;
                    }
                    CurrentCastTime = CastInfo.DesignerCastTime;
                }
                else
                {
                    FinishCasting();
                }

                _game.PacketNotifier.NotifyNPC_CastSpellAns(this, _futureProjNetId);
            }
            return true;
        }

        /// <summary>
        /// Called when the spell is started casting and we're supposed to do things such as projectile spawning, etc.
        /// </summary>
        public virtual void Channel()
        {
            State = SpellState.STATE_CHANNELING;
            CurrentChannelDuration = SpellData.ChannelDuration[CastInfo.SpellLevel];
        }

        void ISpell.Deactivate()
        {
            CastInfo.Targets.Clear();
            _spellGameScript.OnDeactivate(CastInfo.Owner, this);
        }

        /// <summary>
        /// Called when the spell is finished casting and we're supposed to do things such as projectile spawning, etc.
        /// </summary>
        public virtual void FinishCasting()
        {
            if (CastInfo.IsAutoAttack || CastInfo.UseAttackCastTime)
            {
                CastInfo.Owner.HasAutoAttacked = true;
                if (!CastInfo.Owner.HasMadeInitialAttack)
                {
                    CastInfo.Owner.HasMadeInitialAttack = true;
                }
                if (!CastInfo.Owner.IsMelee)
                {
                    // TODO: Add checks for Dodge and Miss
                    HitResult hitResult = HitResult.HIT_Normal;
                    if (CastInfo.Owner.IsNextAutoCrit)
                    {
                        hitResult = HitResult.HIT_Critical;
                    }
                    if (HasEmptyScript)
                    {
                        AddProjectileTarget("", CastInfo.Targets[0].Unit, hitResult);
                    }
                    else
                    {
                        _spellGameScript.OnFinishCasting(CastInfo.Owner, this, CastInfo.Targets[0].Unit);
                    }
                }
                else
                {
                    if (CastInfo.Owner is IChampion c)
                    {
                        ApiEventManager.OnChampionHitUnit.Publish(c, CastInfo.Targets[0].Unit, CastInfo.Owner.IsNextAutoCrit);
                    }
                    ApiEventManager.OnHitUnit.Publish(CastInfo.Owner, CastInfo.Targets[0].Unit, CastInfo.Owner.IsNextAutoCrit);

                    _spellGameScript.OnFinishCasting(CastInfo.Owner, this, CastInfo.Targets[0].Unit);
                    CastInfo.Owner.AutoAttackHit(CastInfo.Targets[0].Unit);
                    _spellGameScript.ApplyEffects(CastInfo.Owner, CastInfo.Targets[0].Unit, this, null);
                }

                State = SpellState.STATE_READY;
            }
            else
            {
                _spellGameScript.OnFinishCasting(CastInfo.Owner, this, CastInfo.Targets[0].Unit);
                if (SpellData.ChannelDuration[CastInfo.SpellLevel] <= 0)
                {
                    State = SpellState.STATE_COOLDOWN;

                    CurrentCooldown = GetCooldown();

                    if (CastInfo.SpellSlot < 4)
                    {
                        _game.PacketNotifier.NotifyCHAR_SetCooldown(CastInfo.Owner, CastInfo.SpellSlot, CurrentCooldown, GetCooldown());
                    }

                    CastInfo.Owner.IsCastingSpell = false;
                }
            }
        }

        /// <summary>
        /// Called when the character finished channeling
        /// </summary>
        public virtual void FinishChanneling()
        {
            State = SpellState.STATE_COOLDOWN;

            CurrentCooldown = GetCooldown();

            if (CastInfo.SpellSlot < 4)
            {
                _game.PacketNotifier.NotifyCHAR_SetCooldown(CastInfo.Owner, CastInfo.SpellSlot, CurrentCooldown, GetCooldown());
            }

            CastInfo.Owner.IsCastingSpell = false;
        }

        public float GetCooldown()
        {
            return _game.Config.CooldownsEnabled ? SpellData.Cooldown[CastInfo.SpellLevel] * (1 - CastInfo.Owner.Stats.CooldownReduction.Total) : 0;
        }

        /// <returns>spell's unique ID</returns>
        public int GetId()
        {
            return (int)HashFunctions.HashString(SpellName);
        }

        public string GetStringForSlot()
        {
            switch (CastInfo.SpellSlot)
            {
                case 0:
                    return "Q";
                case 1:
                    return "W";
                case 2:
                    return "E";
                case 3:
                    return "R";
                case 62:
                    return "Passive";
                case var n when (n <= 81 && n >= 64):
                {
                    return "Attack";
                }
            }

            return "undefined";
        }

        public void LevelUp()
        {
            if (CastInfo.SpellLevel <= 5)
            {
                ++CastInfo.SpellLevel;
            }

            if (CastInfo.SpellSlot < 4)
            {
                CastInfo.Owner.Stats.ManaCost[CastInfo.SpellSlot] = SpellData.ManaCost[CastInfo.SpellLevel];
            }
        }

        public void LowerCooldown(float lowerValue)
        {
            SetCooldown(CurrentCooldown - lowerValue);
        }

        public void ResetSpellDelay()
        {
            CurrentDelayTime = 0;
        }

        public virtual void SetCooldown(float newCd)
        {
            if (newCd <= 0)
            {
                _game.PacketNotifier.NotifyCHAR_SetCooldown(CastInfo.Owner, CastInfo.SpellSlot, 0, 0);
                State = SpellState.STATE_READY;
                CurrentCooldown = 0;
            }
            else
            {
                _game.PacketNotifier.NotifyCHAR_SetCooldown(CastInfo.Owner, CastInfo.SpellSlot, newCd, GetCooldown());
                State = SpellState.STATE_COOLDOWN;
                CurrentCooldown = newCd;
            }
        }

        public void SetLevel(byte toLevel)
        {
            if (toLevel <= 5)
            {
                CastInfo.SpellLevel = toLevel;
            }

            if (CastInfo.SpellSlot < 4)
            {
                CastInfo.Owner.Stats.ManaCost[CastInfo.SpellSlot] = SpellData.ManaCost[CastInfo.SpellLevel];
            }
        }

        public void SetSpellState(SpellState state)
        {
            State = state;
        }

        public void SpellAnimation(string animName, IAttackableUnit target)
        {
            _game.PacketNotifier.NotifyS2C_PlayAnimation(target, animName);
        }

        /// <summary>
        /// Called every diff milliseconds to update the spell
        /// </summary>
        public virtual void Update(float diff)
        {
            if (!HasEmptyScript)
            {
                _spellGameScript.OnUpdate(diff);
            }

            switch (State)
            {
                case SpellState.STATE_READY:
                    break;
                case SpellState.STATE_CASTING:
                {
                    if (!CastInfo.IsAutoAttack && !CastInfo.UseAttackCastTime)
                    {
                        CastInfo.Owner.IsCastingSpell = true;
                        CurrentCastTime -= diff / 1000.0f;
                        if (CurrentCastTime <= 0)
                        {
                            FinishCasting();
                            if (SpellData.ChannelDuration[CastInfo.SpellLevel] > 0)
                            {
                                Channel();
                            }
                        }
                    }
                    else
                    {
                        CurrentDelayTime += diff / 1000.0f;
                        if (CurrentDelayTime >= CurrentCastTime / CastInfo.AttackSpeedModifier)
                        {
                            FinishCasting();
                        }
                    }
                    break;
                }
                case SpellState.STATE_COOLDOWN:
                {
                    CurrentCooldown -= diff / 1000.0f;
                    if (CurrentCooldown < 0)
                    {
                        State = SpellState.STATE_READY;
                    }
                    break;
                }
                case SpellState.STATE_CHANNELING:
                {
                    CurrentChannelDuration -= diff / 1000.0f;
                    if (CurrentChannelDuration <= 0)
                    {
                        FinishChanneling();
                    }
                    break;
                }
            }
        }
    }
}

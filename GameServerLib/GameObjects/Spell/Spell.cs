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
        private ISpellScript _spellScript;

        public ICastInfo CastInfo { get; private set; } = new CastInfo();

        public string SpellName { get; private set; }
        public bool HasEmptyScript => _spellScript.GetType() == typeof(SpellScriptEmpty);

        public SpellState State { get; protected set; } = SpellState.STATE_READY;
        public float CurrentCooldown { get; protected set; }
        public float CurrentCastTime { get; protected set; }
        public float CurrentChannelDuration { get; protected set; }
        public float CurrentDelayTime { get; protected set; }
        public bool Toggle { get; protected set; }

        public ISpellData SpellData { get; private set; }

        public Spell(Game game, IObjAiBase owner, string spellName, byte slot)
        {
            _game = game;
            _scriptEngine = game.ScriptEngine;
            _networkIdManager = game.NetworkIdManager;
            _futureProjNetId = _networkIdManager.GetNewNetId();

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
            _spellScript = _scriptEngine.CreateObject<ISpellScript>("Spells", spellName) ?? new SpellScriptEmpty();

            if (_spellScript.ScriptMetadata.TriggersSpellCasts)
            {
                ApiEventManager.OnSpellCast.AddListener(_spellScript, this, _spellScript.OnSpellCast);
                ApiEventManager.OnSpellPostCast.AddListener(_spellScript, this, _spellScript.OnSpellPostCast);
            }

            if (_spellScript.ScriptMetadata.ChannelDuration > 0)
            {
                ApiEventManager.OnSpellChannel.AddListener(_spellScript, this, _spellScript.OnSpellChannel);
                ApiEventManager.OnSpellChannelCancel.AddListener(_spellScript, this, _spellScript.OnSpellChannelCancel);
                ApiEventManager.OnSpellPostChannel.AddListener(_spellScript, this, _spellScript.OnSpellPostChannel);
            }

            //Activate spell - Notes: Deactivate is never called as spell removal hasn't been added
            _spellScript.OnActivate(owner, this);
        }

        /// <summary>
        /// Called by projectiles when they land / hit, this is where we apply damage/slows etc.
        /// </summary>
        public void ApplyEffects(IAttackableUnit u, ISpellMissile p = null)
        {
            if (SpellData.HaveHitEffect && !string.IsNullOrEmpty(SpellData.HitEffectName) && !CastInfo.IsAutoAttack && HasEmptyScript)
            {
                if (SpellData.HaveHitBone)
                {
                    ApiFunctionManager.AddParticleTarget(CastInfo.Owner, SpellData.HitEffectName, u, bone: SpellData.HitBoneName, lifetime: 1.0f);
                }
                else
                {
                    ApiFunctionManager.AddParticleTarget(CastInfo.Owner, SpellData.HitEffectName, u, lifetime: 1.0f);
                }
            }

            ApiEventManager.OnSpellHit.Publish(CastInfo.Owner, this, u, p);
        }

        /// <summary>
        /// Called when the character casts this spell. Initializes the CastInfo for this spell and begins casting.
        /// </summary>
        public bool Cast(Vector2 start, Vector2 end, IAttackableUnit unit = null)
        {
            if (unit == null && SpellData.TargetingType == TargetingType.Target)
            {
                return false;
            }

            if (unit == null
                && (SpellData.TargetingType == TargetingType.Self
                || SpellData.TargetingType == TargetingType.SelfAOE
                || SpellData.TargetingType == TargetingType.TargetOrLocation))
            {
                unit = CastInfo.Owner;
            }

            var attackType = AttackType.ATTACK_TYPE_RADIAL;
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

            // TODO: Unhardcode (wind down? If so, make it cancelable via casting a different spell and via changing move order to AttackTo or MoveTo.)
            CastInfo.ExtraCastTime = 0.0f;
            CastInfo.Cooldown = GetCooldown();
            // TODO: Unhardcode (extra windup?)
            CastInfo.StartCastTime = 0.0f;

            // For things like Garen Q, Leona Q, and TF W (and probably more)
            if (SpellData.ConsideredAsAutoAttack || SpellData.UseAutoattackCastTime || CastInfo.UseAttackCastDelay) // TODO: Verify
            {
                CastInfo.IsAutoAttack = false;
                CastInfo.DesignerCastTime = SpellData.GetCharacterAttackCastDelay(CastInfo.AttackSpeedModifier, CastInfo.Owner.CharData.AttackDelayOffsetPercent[0], CastInfo.Owner.CharData.AttackDelayCastOffsetPercent[0], CastInfo.Owner.CharData.AttackDelayCastOffsetPercentAttackSpeedRatio[0]);
                CastInfo.DesignerTotalTime = SpellData.GetCharacterAttackDelay(CastInfo.AttackSpeedModifier, CastInfo.Owner.CharData.AttackDelayOffsetPercent[0]);
                CastInfo.UseAttackCastDelay = true;
            }
            else
            {
                CastInfo.DesignerCastTime = SpellData.GetCastTime();
                CastInfo.DesignerTotalTime = SpellData.GetCastTime() + SpellData.ChannelDuration[CastInfo.SpellLevel];
            }

            if (SpellData.OverrideCastTime > 0)
            {
                CastInfo.DesignerCastTime = SpellData.OverrideCastTime;
            }

            if (_spellScript.ScriptMetadata.CastTime > 0)
            {
                CastInfo.DesignerCastTime = _spellScript.ScriptMetadata.CastTime;
            }

            // Otherwise, use the normal auto attack setup
            if (CastInfo.IsAutoAttack)
            {
                attackType = AttackType.ATTACK_TYPE_TARGETED;
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

            // TODO: Account for multiple targets
            CastInfo.Targets.Add(new CastTarget(unit, CastTarget.GetHitResult(unit, CastInfo.IsAutoAttack, CastInfo.Owner.IsNextAutoCrit)));

            // TODO: implement check for IsForceCastingOrChannel and IsOverrideCastPosition
            if (SpellData.CastType == (int)CastType.CAST_TargetMissile
             || SpellData.CastType == (int)CastType.CAST_ChainMissile)
            {
                // TODO: Verify
                CastInfo.IsClickCasted = true;
            }

            // TODO: Verify
            CastInfo.SpellCastLaunchPosition = CastInfo.Owner.GetPosition3D();

            var targetingType = SpellData.TargetingType;
            if (!CastInfo.IsAutoAttack
             && (targetingType == TargetingType.Target
             || targetingType == TargetingType.Area
             || targetingType == TargetingType.Location
             || targetingType == TargetingType.DragDirection))
            {
                var distance = Vector2.DistanceSquared(CastInfo.Owner.Position, start);
                var castRange = GetCurrentCastRange();

                if (targetingType == TargetingType.Target)
                {
                    attackType = AttackType.ATTACK_TYPE_TARGETED;
                    distance = Vector2.DistanceSquared(CastInfo.Owner.Position, unit.Position);

                    if (distance > castRange * castRange)
                    {
                        CastInfo.Owner.SetSpellToCast(this, Vector2.Zero, unit);
                        return false;
                    }
                }

                if (distance > castRange * castRange)
                {
                    CastInfo.Owner.SetSpellToCast(this, start);
                    return false;
                }
            }

            CastInfo.Owner.UpdateMoveOrder(OrderType.TempCastSpell, true);

            _spellScript.OnSpellPreCast(CastInfo.Owner, this, unit, start, end);

            if (!CastInfo.IsAutoAttack && (!SpellData.IsToggleSpell && !SpellData.CanMoveWhileChanneling
                        && SpellData.CantCancelWhileChanneling)
                        || (!SpellData.NoWinddownIfCancelled
                        && !SpellData.Flags.HasFlag(SpellDataFlags.InstantCast)
                        && SpellData.CantCancelWhileWindingUp))
            {
                if (_spellScript.ScriptMetadata.TriggersSpellCasts)
                {
                    if (!SpellData.Flags.HasFlag(SpellDataFlags.InstantCast))
                    {
                        CastInfo.Owner.StopMovement();
                    }

                    var goingTo = new Vector2(CastInfo.TargetPosition.X, CastInfo.TargetPosition.Z) - CastInfo.Owner.Position;

                    if (unit != null)
                    {
                        goingTo = unit.Position - CastInfo.Owner.Position;
                    }

                    var dirTemp = Vector2.Normalize(goingTo);
                    CastInfo.Owner.FaceDirection(new Vector3(dirTemp.X, 0, dirTemp.Y), false);
                }

                if (!SpellData.Flags.HasFlag(SpellDataFlags.InstantCast))
                {
                    CastInfo.Owner.UpdateMoveOrder(OrderType.CastSpell, true);
                }
            }

            // If we are supposed to automatically cast a skillshot for this spell, then calculate the proper end position before casting.
            if (_spellScript.ScriptMetadata.MissileParameters != null && _spellScript.ScriptMetadata.MissileParameters.Type == MissileType.Circle)
            {
                var targetPos = ApiFunctionManager.GetPointFromUnit(CastInfo.Owner, GetCurrentCastRange());
                CastInfo.TargetPosition = new Vector3(targetPos.X, _game.Map.NavigationGrid.GetHeightAtLocation(targetPos.X, targetPos.Y), targetPos.Y);
                // TODO: Verify if we should also override TargetPositionEnd (probably not due to things like Viktor E).
            }

            if (CastInfo.IsAutoAttack && CastInfo.Owner.IsMelee)
            {
                attackType = AttackType.ATTACK_TYPE_MELEE;
            }

            if (CastInfo.Targets[0].Unit != null && CastInfo.Targets[0].Unit != CastInfo.Owner)
            {
                ApiFunctionManager.FaceDirection(CastInfo.Targets[0].Unit.Position, CastInfo.Owner);
                _game.PacketNotifier.NotifyS2C_UnitSetLookAt(CastInfo.Owner, CastInfo.Targets[0].Unit, attackType);
            }

            if (CastInfo.IsAutoAttack || CastInfo.UseAttackCastTime)
            {
                // We assume it is already an attack.
                int index = CastInfo.SpellSlot - 64;
                if (CastInfo.SpellSlot >= 45 && CastInfo.SpellSlot <= 60)
                {
                    // Extra Spells which UseAttackCastTime just use the base auto attack's cast time.
                    index = 64;
                }

                float autoAttackTotalTime = CastInfo.Owner.CharData.GlobalCharData.AttackDelay * (1.0f + CastInfo.Owner.CharData.AttackDelayOffsetPercent[0]);
                CastInfo.DesignerCastTime = autoAttackTotalTime * (CastInfo.Owner.CharData.GlobalCharData.AttackDelayCastPercent + CastInfo.Owner.CharData.AttackDelayCastOffsetPercent[index]);

                if (CastInfo.IsAutoAttack)
                {
                    if (!CastInfo.IsSecondAutoAttack)
                    {
                        _game.PacketNotifier.NotifyBasic_Attack_Pos(CastInfo.Owner, CastInfo.Targets[0].Unit, _futureProjNetId, CastInfo.Owner.IsNextAutoCrit);
                    }
                    else
                    {
                        _game.PacketNotifier.NotifyBasic_Attack(CastInfo.Owner, CastInfo.Targets[0].Unit, _futureProjNetId, CastInfo.Owner.IsNextAutoCrit, CastInfo.Owner.HasMadeInitialAttack);
                    }
                }

                if (CastInfo.UseAttackCastTime)
                {
                    // TODO: Verify
                    CastInfo.DesignerTotalTime = CastInfo.DesignerCastTime + SpellData.ChannelDuration[CastInfo.SpellLevel];
                }
            }

            if (CastInfo.DesignerCastTime > 0)
            {
                if (_spellScript.ScriptMetadata.TriggersSpellCasts)
                {
                    ApiEventManager.OnSpellCast.Publish(this);
                }

                if (CastInfo.IsAutoAttack)
                {
                    ApiEventManager.OnPreAttack.Publish(CastInfo.Owner, this);
                }

                if (!CastInfo.UseAttackCastDelay)
                {
                    if (!SpellData.Flags.HasFlag(SpellDataFlags.InstantCast))
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

            if (!CastInfo.IsAutoAttack)
            {
                _game.PacketNotifier.NotifyNPC_CastSpellAns(this);
            }

            return true;
        }

        /// <summary>
        /// Called when a script manually casts this spell.
        /// </summary>
        public bool Cast(ICastInfo castInfo, bool cast)
        {
            CastInfo = castInfo;
            var start = new Vector2(CastInfo.TargetPosition.X, CastInfo.TargetPosition.Z);
            var end = new Vector2(CastInfo.TargetPositionEnd.X, CastInfo.TargetPositionEnd.Z);

            _spellScript.OnSpellPreCast(CastInfo.Owner, this, castInfo.Targets[0].Unit, start, end);

            var stats = CastInfo.Owner.Stats;

            if (cast)
            {
                if ((SpellData.ManaCost[CastInfo.SpellLevel] * (1 - stats.SpellCostReduction) >= stats.CurrentMana && !CastInfo.IsAutoAttack) || State != SpellState.STATE_READY)
                {
                    return false;
                }

                if (_game.Config.ManaCostsEnabled)
                {
                    stats.CurrentMana -= SpellData.ManaCost[CastInfo.SpellLevel] * (1 - stats.SpellCostReduction);
                }
            }

            _futureProjNetId = _networkIdManager.GetNewNetId();

            CastInfo.MissileNetID = _futureProjNetId;

            CastInfo.ExtraCastTime = 0.0f; // TODO: Unhardcode
            CastInfo.Cooldown = GetCooldown();
            CastInfo.StartCastTime = 0.0f; // TODO: Unhardcode

            // For things like Garen Q, Leona Q, and TF W (and probably more)
            if (SpellData.ConsideredAsAutoAttack || SpellData.UseAutoattackCastTime || CastInfo.UseAttackCastDelay) // TODO: Verify
            {
                CastInfo.IsAutoAttack = false;
                CastInfo.DesignerCastTime = SpellData.GetCharacterAttackCastDelay(CastInfo.AttackSpeedModifier, CastInfo.Owner.CharData.AttackDelayOffsetPercent[0], CastInfo.Owner.CharData.AttackDelayCastOffsetPercent[0], CastInfo.Owner.CharData.AttackDelayCastOffsetPercentAttackSpeedRatio[0]);
                CastInfo.DesignerTotalTime = SpellData.GetCharacterAttackDelay(CastInfo.AttackSpeedModifier, CastInfo.Owner.CharData.AttackDelayOffsetPercent[0]);
                CastInfo.UseAttackCastDelay = true;
            }
            else
            {
                CastInfo.DesignerCastTime = SpellData.GetCastTime();
                CastInfo.DesignerTotalTime = SpellData.GetCastTime() + SpellData.ChannelDuration[CastInfo.SpellLevel];
            }

            if (SpellData.OverrideCastTime > 0)
            {
                CastInfo.DesignerCastTime = SpellData.OverrideCastTime;
            }

            if (_spellScript.ScriptMetadata.CastTime > 0)
            {
                CastInfo.DesignerCastTime = _spellScript.ScriptMetadata.CastTime;
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
            if (SpellData.CastType == (int)CastType.CAST_TargetMissile
             || SpellData.CastType == (int)CastType.CAST_ChainMissile)
            {
                // TODO: Verify
                CastInfo.IsClickCasted = true;
            }

            // TODO: Verify
            var attackType = AttackType.ATTACK_TYPE_RADIAL;

            if (cast && !CastInfo.IsAutoAttack && (!SpellData.IsToggleSpell && !SpellData.CanMoveWhileChanneling
                        && SpellData.CantCancelWhileChanneling)
                        || (!SpellData.NoWinddownIfCancelled
                        && !SpellData.Flags.HasFlag(SpellDataFlags.InstantCast)
                        && SpellData.CantCancelWhileWindingUp))
            {
                if (_spellScript.ScriptMetadata.TriggersSpellCasts)
                {
                    if (!SpellData.Flags.HasFlag(SpellDataFlags.InstantCast))
                    {
                        CastInfo.Owner.StopMovement();
                    }

                    ApiFunctionManager.FaceDirection(CastInfo.Targets[0].Unit.Position, CastInfo.Owner);
                }

                if (!SpellData.Flags.HasFlag(SpellDataFlags.InstantCast))
                {
                    CastInfo.Owner.UpdateMoveOrder(OrderType.CastSpell, true);
                }
            }

            if (CastInfo.IsAutoAttack && CastInfo.Owner.IsMelee)
            {
                attackType = AttackType.ATTACK_TYPE_MELEE;
            }

            if (cast && CastInfo.Targets[0].Unit != null && CastInfo.Targets[0].Unit != CastInfo.Owner)
            {
                /*
                if (CastInfo.Targets[0].Unit != CastInfo.Owner.TargetUnit)
                {
                    CastInfo.Owner.SetTargetUnit(CastInfo.Targets[0].Unit, false);
                }*/

                ApiFunctionManager.FaceDirection(CastInfo.Targets[0].Unit.Position, CastInfo.Owner);

                _game.PacketNotifier.NotifyS2C_UnitSetLookAt(CastInfo.Owner, CastInfo.Targets[0].Unit, attackType);
            }

            if (CastInfo.IsAutoAttack || CastInfo.UseAttackCastTime)
            {
                // We assume it is already an attack.
                int index = CastInfo.SpellSlot - 64;
                if (CastInfo.SpellSlot >= 45 && CastInfo.SpellSlot <= 60)
                {
                    // Extra Spells which UseAttackCastTime just use the base auto attack's cast time.
                    index = 64;
                }

                float autoAttackTotalTime = CastInfo.Owner.CharData.GlobalCharData.AttackDelay * (1.0f + CastInfo.Owner.CharData.AttackDelayOffsetPercent[0]);
                CastInfo.DesignerCastTime = autoAttackTotalTime * (CastInfo.Owner.CharData.GlobalCharData.AttackDelayCastPercent + CastInfo.Owner.CharData.AttackDelayCastOffsetPercent[index]);

                // TODO: Verify if this should be affected by cast variable.
                if (CastInfo.IsAutoAttack)
                {
                    if (!CastInfo.IsSecondAutoAttack)
                    {
                        _game.PacketNotifier.NotifyBasic_Attack_Pos(CastInfo.Owner, CastInfo.Targets[0].Unit, _futureProjNetId, CastInfo.Owner.IsNextAutoCrit);
                    }
                    else
                    {
                        _game.PacketNotifier.NotifyBasic_Attack(CastInfo.Owner, CastInfo.Targets[0].Unit, _futureProjNetId, CastInfo.Owner.IsNextAutoCrit, CastInfo.Owner.HasMadeInitialAttack);
                    }
                }

                if (CastInfo.UseAttackCastTime)
                {
                    // TODO: Verify
                    CastInfo.DesignerTotalTime = CastInfo.DesignerCastTime + SpellData.ChannelDuration[CastInfo.SpellLevel];
                }
            }

            if (cast)
            {
                if (CastInfo.DesignerCastTime > 0)
                {
                    if (_spellScript.ScriptMetadata.TriggersSpellCasts)
                    {
                        ApiEventManager.OnSpellCast.Publish(this);
                    }

                    if (CastInfo.IsAutoAttack)
                    {
                        ApiEventManager.OnPreAttack.Publish(CastInfo.Owner, this);
                    }

                    if (!CastInfo.UseAttackCastDelay)
                    {
                        if (!SpellData.Flags.HasFlag(SpellDataFlags.InstantCast))
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

                if (!CastInfo.IsAutoAttack)
                {
                    _game.PacketNotifier.NotifyNPC_CastSpellAns(this);
                }
            }
            else
            {
                if (_spellScript.ScriptMetadata.MissileParameters != null)
                {
                    var missileType = _spellScript.ScriptMetadata.MissileParameters.Type;

                    if (missileType == MissileType.Target)
                    {
                        CreateSpellMissile();
                    }

                    if (missileType == MissileType.Chained)
                    {
                        CreateSpellChainMissile();
                    }
                    
                    if (missileType == MissileType.Circle)
                    {
                        CreateSpellCircleMissile();
                    }

                    if (missileType == MissileType.Arc)
                    {
                        CreateSpellLineMissile();
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Called after the spell has finished casting and is beginning a channel.
        /// </summary>
        public void Channel()
        {
            State = SpellState.STATE_CHANNELING;
            CurrentChannelDuration = SpellData.ChannelDuration[CastInfo.SpellLevel];
            if (_spellScript.ScriptMetadata.ChannelDuration > 0)
            {
                CurrentChannelDuration = _spellScript.ScriptMetadata.ChannelDuration;
                ApiEventManager.OnSpellChannel.Publish(this);
            }

            if (CurrentChannelDuration > 0)
            {
                CastInfo.Owner.SetChannelSpell(this);
            }
        }

        /// <summary>
        /// Forces this spell to stop channeling based on the given condition for the given reason.
        /// </summary>
        /// <param name="condition">Canceled or successful?</param>
        /// <param name="reason">How it should be treated.</param>
        public void StopChanneling(ChannelingStopCondition condition, ChannelingStopSource reason)
        {
            if (condition == ChannelingStopCondition.Cancel)
            {
                ApiEventManager.OnSpellChannelCancel.Publish(this);
            }

            State = SpellState.STATE_READY;

            if (reason == ChannelingStopSource.TimeCompleted)
            {
                FinishChanneling();
            }
        }

        void ISpell.Deactivate()
        {
            CastInfo.Targets.Clear();
            _spellScript.OnDeactivate(CastInfo.Owner, this);
        }

        /// <summary>
        /// Called when the spell is finished casting and we're supposed to do things such as projectile spawning, etc.
        /// </summary>
        public void FinishCasting()
        {
            if (_spellScript.ScriptMetadata.TriggersSpellCasts)
            {
                ApiEventManager.OnSpellPostCast.Publish(this);
            }

            if (CastInfo.IsAutoAttack)
            {
                ApiEventManager.OnLaunchAttack.Publish(CastInfo.Owner, this);
            }

            if (CastInfo.IsAutoAttack || CastInfo.UseAttackCastTime)
            {
                CastInfo.Owner.HasAutoAttacked = true;
                if (!CastInfo.Owner.HasMadeInitialAttack)
                {
                    CastInfo.Owner.HasMadeInitialAttack = true;
                }
                if (!CastInfo.Owner.IsMelee)
                {
                    if (HasEmptyScript)
                    {
                        CreateSpellMissile();
                    }
                }
                else
                {
                    if (_spellScript.ScriptMetadata.MissileParameters == null)
                    {
                        ApplyEffects(CastInfo.Targets[0].Unit, null);
                    }
                    CastInfo.Owner.AutoAttackHit(CastInfo.Targets[0].Unit);
                }

                State = SpellState.STATE_READY;
            }
            else
            {
                if (_spellScript.ScriptMetadata.MissileParameters == null)
                {
                    ApplyEffects(CastInfo.Targets[0].Unit, null);
                }

                if (SpellData.ChannelDuration[CastInfo.SpellLevel] <= 0)
                {
                    State = SpellState.STATE_COOLDOWN;

                    CurrentCooldown = GetCooldown();

                    if (CastInfo.SpellSlot < 4)
                    {
                        _game.PacketNotifier.NotifyCHAR_SetCooldown(CastInfo.Owner, CastInfo.SpellSlot, CurrentCooldown, GetCooldown());
                    }
                }
            }

            if (_spellScript.ScriptMetadata.MissileParameters != null)
            {
                var missileType = _spellScript.ScriptMetadata.MissileParameters.Type;

                if (missileType == MissileType.Target)
                {
                    CreateSpellMissile();
                }

                if (missileType == MissileType.Chained)
                {
                    CreateSpellChainMissile();
                }

                if (missileType == MissileType.Circle)
                {
                    CreateSpellCircleMissile();
                }

                if (missileType == MissileType.Arc)
                {
                    CreateSpellLineMissile();
                }
            }

            if (SpellData.Flags.HasFlag(SpellDataFlags.InstantCast))
            {
                if (!CastInfo.Owner.IsPathEnded())
                {
                    CastInfo.Owner.UpdateMoveOrder(OrderType.MoveTo, true);
                }
                if (CastInfo.Owner.TargetUnit != null)
                {
                    CastInfo.Owner.UpdateMoveOrder(OrderType.AttackTo, true);
                }
            }
            else
            {
                // TODO: Verify
                CastInfo.Owner.UpdateMoveOrder(OrderType.Hold, true);
            }

            if (CastInfo.Owner.SpellToCast != null)
            {
                CastInfo.Owner.SetSpellToCast(null, Vector2.Zero);
            }
        }

        /// <summary>
        /// Creates a single-target missile with the specified properties.
        /// </summary>
        public ISpellMissile CreateSpellMissile()
        {
            if (CastInfo.MissileNetID == 0)
            {
                //_game.PacketNotifier.NotifyDestroyClientMissile(p);
                return null;
            }

            var isServerOnly = false;

            if (SpellData.MissileEffect != "")
            {
                isServerOnly = true;
            }

            var p = new SpellMissile(
                _game,
                (int)SpellData.LineWidth,
                this,
                CastInfo,
                SpellData.MissileSpeed,
                SpellData.Flags,
                CastInfo.MissileNetID,
                isServerOnly
            );

            _game.ObjectManager.AddObject(p);

            ApiEventManager.OnLaunchMissile.Publish(new KeyValuePair<IObjAiBase, ISpell>(CastInfo.Owner, this), p);

            _game.PacketNotifier.NotifyMissileReplication(p);

            // TODO: Verify when NotifyForceCreateMissile should be used instead.

            return p;
        }

        /// <summary>
        /// Creates a single-target bouncing missile with the specified properties.
        /// </summary>
        public ISpellMissile CreateSpellChainMissile()
        {
            if (CastInfo.MissileNetID == 0)
            {
                //_game.PacketNotifier.NotifyDestroyClientMissile(p);
                return null;
            }

            var isServerOnly = false;

            if (SpellData.MissileEffect != "")
            {
                isServerOnly = true;
            }

            var p = new SpellChainMissile(
                _game,
                (int)SpellData.LineWidth,
                this,
                CastInfo,
                _spellScript.ScriptMetadata.MissileParameters,
                SpellData.MissileSpeed,
                SpellData.Flags,
                CastInfo.MissileNetID,
                isServerOnly
            );

            _game.ObjectManager.AddObject(p);

            ApiEventManager.OnLaunchMissile.Publish(new KeyValuePair<IObjAiBase, ISpell>(CastInfo.Owner, this), p);

            _game.PacketNotifier.NotifyMissileReplication(p);

            // TODO: Verify when NotifyForceCreateMissile should be used instead.

            return p;
        }

        /// <summary>
        /// Creates a line missile with the specified properties.
        /// </summary>
        public ISpellMissile CreateSpellCircleMissile()
        {
            if (CastInfo.MissileNetID == 0)
            {
                //_game.PacketNotifier.NotifyDestroyClientMissile(p);
                return null;
            }

            var isServerOnly = false;

            if (SpellData.MissileEffect != "")
            {
                isServerOnly = true;
            }

            var p = new SpellCircleMissile(
                _game,
                (int)SpellData.LineWidth,
                this,
                CastInfo,
                SpellData.MissileSpeed,
                SpellData.Flags,
                CastInfo.MissileNetID,
                isServerOnly
            );

            _game.ObjectManager.AddObject(p);

            ApiEventManager.OnLaunchMissile.Publish(new KeyValuePair<IObjAiBase, ISpell>(CastInfo.Owner, this), p);

            _game.PacketNotifier.NotifyMissileReplication(p);

            // TODO: Verify when NotifyForceCreateMissile should be used instead.

            return p;
        }

        /// <summary>
        /// Creates an arc missile with the specified properties.
        /// </summary>
        public ISpellMissile CreateSpellLineMissile()
        {
            if (CastInfo.MissileNetID == 0)
            {
                //_game.PacketNotifier.NotifyDestroyClientMissile(p);
                return null;
            }

            var isServerOnly = false;

            if (SpellData.MissileEffect != "")
            {
                isServerOnly = true;
            }

            var p = new SpellLineMissile(
                _game,
                (int)SpellData.LineWidth,
                this,
                CastInfo,
                SpellData.MissileSpeed,
                SpellData.Flags,
                CastInfo.MissileNetID,
                isServerOnly
            );

            _game.ObjectManager.AddObject(p);

            ApiEventManager.OnLaunchMissile.Publish(new KeyValuePair<IObjAiBase, ISpell>(CastInfo.Owner, this), p);

            _game.PacketNotifier.NotifyMissileReplication(p);

            // TODO: Verify when NotifyForceCreateMissile should be used instead.

            return p;
        }

        /// <summary>
        /// Called when the character finished channeling
        /// </summary>
        public void FinishChanneling()
        {
            ApiEventManager.OnSpellPostChannel.Publish(this);

            CastInfo.Owner.SetChannelSpell(null);

            CastInfo.Owner.UpdateMoveOrder(OrderType.Hold, true);

            State = SpellState.STATE_COOLDOWN;

            CurrentCooldown = GetCooldown();

            if (CastInfo.SpellSlot < 4)
            {
                _game.PacketNotifier.NotifyCHAR_SetCooldown(CastInfo.Owner, CastInfo.SpellSlot, CurrentCooldown, GetCooldown());
            }
        }

        public float GetCooldown()
        {
            return _game.Config.CooldownsEnabled ? SpellData.Cooldown[CastInfo.SpellLevel] * (1 - CastInfo.Owner.Stats.CooldownReduction.Total) : 0;
        }

        /// <summary>
        /// Gets the cast range for this spell (based on level).
        /// </summary>
        /// <returns>Cast range based on level.</returns>
        public float GetCurrentCastRange()
        {
            var castRange = SpellData.CastRange[0];

            if (CastInfo.SpellLevel == 0)
            {
                return castRange;
            }

            if (CastInfo.SpellLevel > 0)
            {
                for (int i = 1; i < SpellData.CastRange.Length - 1; i++)
                {
                    if (SpellData.CastRange[i] > castRange && CastInfo.SpellLevel == i)
                    {
                        castRange = SpellData.CastRange[i];
                    }
                }
            }

            return castRange;
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

        public void SetCooldown(float newCd)
        {
            if (newCd <= 0)
            {
                _game.PacketNotifier.NotifyCHAR_SetCooldown(CastInfo.Owner, CastInfo.SpellSlot, 0, 0);
                // Changing the state of the spell to READY during casting prevents the spell from finishing casting, thus locking the player in the move order CastSpell.
                if (State != SpellState.STATE_CASTING && State != SpellState.STATE_CHANNELING)
                {
                    State = SpellState.STATE_READY;
                    CurrentCooldown = 0;
                }
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

        /// <summary>
        /// Sets the state of the spell to the specified state. Often used when reseting time between spell casts.
        /// </summary>
        /// <param name="state"></param>
        public void SetSpellState(SpellState state)
        {
            State = state;
        }

        /// <summary>
        /// Sets the toggle state of this spell to the specified state. True = usable, false = sealed, unusable.
        /// </summary>
        /// <param name="toggle">True/False.</param>
        public void SetSpellToggle(bool toggle)
        {
            Toggle = toggle;

            if (CastInfo.Owner is IChampion ch)
            {
                var clientInfo = _game.PlayerManager.GetClientInfoByChampion(ch);
                _game.PacketNotifier.NotifyS2C_UpdateSpellToggle((int)clientInfo.PlayerId, this);
            }
        }

        /// <summary>
        /// Called every diff milliseconds to update the spell
        /// </summary>
        public void Update(float diff)
        {
            if (!HasEmptyScript)
            {
                _spellScript.OnUpdate(diff);
            }

            switch (State)
            {
                case SpellState.STATE_READY:
                    break;
                case SpellState.STATE_CASTING:
                {
                    if (!CastInfo.IsAutoAttack && !CastInfo.UseAttackCastTime)
                    {
                        CurrentCastTime -= diff / 1000.0f;
                        if (CurrentCastTime <= 0)
                        {
                            FinishCasting();
                            if (SpellData.ChannelDuration[CastInfo.SpellLevel] > 0 || _spellScript.ScriptMetadata.ChannelDuration > 0)
                            {
                                Channel();
                            }
                        }
                    }
                    else
                    {
                        if (CastInfo.Owner.TargetUnit != CastInfo.Targets[0].Unit)
                        {
                            State = SpellState.STATE_READY;
                            CurrentDelayTime = 0;
                            CastInfo.Owner.CancelAutoAttack(true);
                            break;
                        }

                        CurrentDelayTime += diff / 1000.0f;
                        if (CurrentDelayTime >= CastInfo.DesignerCastTime / CastInfo.AttackSpeedModifier)
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

﻿using GameServerCore.Content;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
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
        private readonly Game _game;
        private readonly CSharpScriptEngine _scriptEngine;
        private readonly NetworkIdManager _networkIdManager;
        private uint _futureProjNetId;
        private float _overrrideCastRange;

        /// <summary>
        /// General information about this spell when it is cast. Refer to CastInfo class.
        /// </summary>
        public ICastInfo CastInfo { get; private set; } = new CastInfo();
        /// <summary>
        /// Current cooldown of this spell.
        /// </summary>
        public float CurrentCooldown { get; protected set; }
        /// <summary>
        /// Time until casting will end for this spell.
        /// </summary>
        public float CurrentCastTime { get; protected set; }
        /// <summary>
        /// Time until channeling will finish for this spell.
        /// </summary>
        public float CurrentChannelDuration { get; protected set; }
        /// <summary>
        /// Time until the same spell can be cast again. Usually only applicable to auto attack spells.
        /// </summary>
        public float CurrentDelayTime { get; protected set; }
        /// <summary>
        /// The toggle state of this spell.
        /// </summary>
        public bool Toggle { get; protected set; }
        /// <summary>
        /// Spell data for this spell used for interactions between units, cooldown, channeling time, etc. Refer to SpellData class.
        /// </summary>
        public ISpellData SpellData { get; }
        /// <summary>
        /// Internal name of this spell.
        /// </summary>
        public string SpellName { get; }
        /// <summary>
        /// State of this spell. Refer to SpellState enum.
        /// </summary>
        public SpellState State { get; protected set; } = SpellState.STATE_READY;
        /// <summary>
        /// Script instance assigned to this spell.
        /// </summary>
        public ISpellScript Script { get; private set; }
        /// <summary>
        /// Whether or not the script for this spell is the default empty script.
        /// </summary>
        public bool HasEmptyScript { get; private set; } = true;

        public Spell(Game game, IObjAiBase owner, string spellName, byte slot)
        {
            _game = game;
            _scriptEngine = game.ScriptEngine;
            _networkIdManager = game.NetworkIdManager;
            _futureProjNetId = _networkIdManager.GetNewNetId();
            _overrrideCastRange = 0;

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

            //Checks if the spell is in the passive slot, so it doesn't try to load it twice under the "Spells" and "Passives" namespaces
            if (CastInfo.SpellSlot != (int)SpellSlotType.PassiveSpellSlot)
            {
                //Set the game script for the spell
                LoadScript();
                HasEmptyScript = Script.GetType() == typeof(SpellScriptEmpty);
            }
            else
            {
                owner.LoadCharScript(this);
            }
        }

        public void LoadScript()
        {
            ApiEventManager.RemoveAllListenersForOwner(Script);
            string nameSpace = "Spells";
            if (CastInfo.SpellSlot >= (byte)SpellSlotType.InventorySlots && CastInfo.SpellSlot < (byte)SpellSlotType.BluePillSlot)
            {
               nameSpace = "ItemSpells";
            }
            Script = _scriptEngine.CreateObject<ISpellScript>(nameSpace, SpellName) ?? new SpellScriptEmpty();

            if (Script.ScriptMetadata.TriggersSpellCasts)
            {
                ApiEventManager.OnSpellCast.AddListener(Script, this, Script.OnSpellCast);
                ApiEventManager.OnSpellPostCast.AddListener(Script, this, Script.OnSpellPostCast);
            }

            if (Script.ScriptMetadata.ChannelDuration > 0)
            {
                ApiEventManager.OnSpellChannel.AddListener(Script, this, Script.OnSpellChannel);
                ApiEventManager.OnSpellChannelCancel.AddListener(Script, this, Script.OnSpellChannelCancel);
                ApiEventManager.OnSpellPostChannel.AddListener(Script, this, Script.OnSpellPostChannel);
            }

            //Activate spell - Notes: Deactivate is never called as spell removal hasn't been added
            Script.OnActivate(CastInfo.Owner, this);
        }

        public void ApplyEffects(IAttackableUnit u, ISpellMissile p = null, ISpellSector s = null)
        {
            if (SpellData.HaveHitEffect && !string.IsNullOrEmpty(SpellData.HitEffectName) && !CastInfo.IsAutoAttack && HasEmptyScript)
            {
                if (SpellData.HaveHitBone)
                {
                    ApiFunctionManager.AddParticleTarget(CastInfo.Owner, null, SpellData.HitEffectName, u, targetBone: SpellData.HitBoneName, lifetime: 1.0f);
                }
                else
                {
                    ApiFunctionManager.AddParticleTarget(CastInfo.Owner, null, SpellData.HitEffectName, u, lifetime: 1.0f);
                }
            }

            if (CastInfo.IsAutoAttack)
            {
                ApiEventManager.OnBeingHit.Publish(u, CastInfo.Owner);
            }
            else
            {
                ApiEventManager.OnSpellHit.Publish(CastInfo.Owner, this, u, p, s);
            }
        }

        public void CastCancelCheck()
        {
            if (CastInfo.Owner.IsDead
            && !SpellData.CanOnlyCastWhileDead)
            {
                ResetSpellCast();
                return;
            }

            if (CastInfo.Targets[0].Unit != null)
            {
                var spellTarget = CastInfo.Targets[0].Unit;

                if (spellTarget != null)
                {
                    if (!spellTarget.IsVisibleByTeam(CastInfo.Owner.Team)
                    || !spellTarget.Status.HasFlag(StatusFlags.Targetable))
                    {
                        if (CastInfo.IsAutoAttack)
                        {
                            CastInfo.Owner.CancelAutoAttack(true);
                            return;
                        }

                        ResetSpellCast();
                        return;
                    }

                    // Regular auto attacks can lose their target due to untargetability and distance. The limit for range is 3x attack range.
                    if (CastInfo.IsAutoAttack
                    && (Vector2.Distance(spellTarget.Position, CastInfo.Owner.Position) > (CastInfo.Owner.Stats.Range.Total + spellTarget.CollisionRadius) * 3f
                    || CastInfo.Owner.GetCastSpell() != null
                    || CastInfo.Owner.ChannelSpell != null))
                    {
                        CastInfo.Owner.CancelAutoAttack(true);
                        return;
                    }
                }
            }

            // Uncancellable
            if (SpellData.CantCancelWhileWindingUp)
            {
                return;
            }

            var status = CastInfo.Owner.Status;

            if (status == StatusFlags.Charmed
            || status == StatusFlags.Feared
            || status == StatusFlags.Stunned
            || status == StatusFlags.Suppressed
            || status == StatusFlags.Taunted
            || (CastInfo.IsAutoAttack && (status == StatusFlags.Disarmed || !status.HasFlag(StatusFlags.CanAttack)))
            || (!CastInfo.IsAutoAttack && (status == StatusFlags.Silenced || !status.HasFlag(StatusFlags.CanCast))))
            {
                ResetSpellCast();
            }
        }

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

            if ((SpellData.ManaCost[CastInfo.SpellLevel] * (1 - stats.SpellCostReduction) > stats.CurrentMana && !CastInfo.IsAutoAttack) || State != SpellState.STATE_READY)
            {
                return false;
            }

            CastInfo.SpellNetID = _networkIdManager.GetNewNetId();

            CastInfo.AttackSpeedModifier = stats.AttackSpeedMultiplier.Total;

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

            var trueChannel = SpellData.ChannelDuration[CastInfo.SpellLevel];
            if (Script.ScriptMetadata.ChannelDuration > 0)
            {
                trueChannel = Script.ScriptMetadata.ChannelDuration;
            }

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
                CastInfo.DesignerTotalTime = CastInfo.DesignerCastTime + trueChannel;
            }

            if (SpellData.OverrideCastTime > 0)
            {
                CastInfo.DesignerCastTime = SpellData.OverrideCastTime;
                CastInfo.DesignerTotalTime = SpellData.OverrideCastTime + trueChannel;
            }

            if (Script.ScriptMetadata.CastTime > 0)
            {
                CastInfo.DesignerCastTime = Script.ScriptMetadata.CastTime;
                CastInfo.DesignerTotalTime = Script.ScriptMetadata.CastTime + trueChannel;
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

            // All spell checks and steps passed, set the casting spell on the owner.
            // TODO: Verify if we should also do this for manual SpellCasts
            if (!CastInfo.IsAutoAttack)
            {
                CastInfo.Owner.SetCastSpell(this);
                CastInfo.Owner.CancelAutoAttack(false, true);
            }

            CastInfo.Owner.SetTargetUnit(unit, true);
            CastInfo.Owner.UpdateMoveOrder(OrderType.TempCastSpell, true);

            Script.OnSpellPreCast(CastInfo.Owner, this, unit, start, end);

            if (_game.Config.GameFeatures.HasFlag(FeatureFlags.EnableManaCosts))
            {
                stats.CurrentMana -= SpellData.ManaCost[CastInfo.SpellLevel] * (1 - stats.SpellCostReduction);
            }

            if (!CastInfo.IsAutoAttack && !SpellData.IsToggleSpell
                        || (!SpellData.NoWinddownIfCancelled
                        && !SpellData.Flags.HasFlag(SpellDataFlags.InstantCast)
                        && SpellData.CantCancelWhileWindingUp))
            {
                if (Script.ScriptMetadata.TriggersSpellCasts || SpellData.ChannelDuration[CastInfo.SpellLevel] > 0 || Script.ScriptMetadata.ChannelDuration > 0)
                {
                    if (!SpellData.Flags.HasFlag(SpellDataFlags.InstantCast))
                    {
                        CastInfo.Owner.StopMovement();

                        // TODO: Verify if we should move this outside of this TriggersSpellCasts if statement.
                        CastInfo.Owner.UpdateMoveOrder(OrderType.CastSpell, true);
                    }

                    if (Script.ScriptMetadata.AutoFaceDirection)
                    {
                        var goingTo = end - CastInfo.Owner.Position;

                        if (unit != null)
                        {
                            goingTo = unit.Position - CastInfo.Owner.Position;
                        }

                        var dirTemp = Vector2.Normalize(goingTo);
                        CastInfo.Owner.FaceDirection(new Vector3(dirTemp.X, 0, dirTemp.Y), false);
                    }
                }
            }

            // If we are supposed to automatically cast a skillshot for this spell, then calculate the proper end position before casting.
            if (Script.ScriptMetadata.MissileParameters != null && Script.ScriptMetadata.MissileParameters.Type == MissileType.Circle)
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
                if (Script.ScriptMetadata.AutoFaceDirection)
                {
                    ApiFunctionManager.FaceDirection(CastInfo.Targets[0].Unit.Position, CastInfo.Owner);
                }
                _game.PacketNotifier.NotifyS2C_UnitSetLookAt(CastInfo.Owner, CastInfo.Targets[0].Unit, attackType);
            }

            if (CastInfo.IsAutoAttack || CastInfo.UseAttackCastTime)
            {
                // We assume it is already an attack.
                int index = CastInfo.SpellSlot - 64;
                if (CastInfo.SpellSlot >= 45 && CastInfo.SpellSlot <= 60)
                {
                    // Extra Spells which UseAttackCastTime just use the base auto attack's cast time.
                    index = 0;
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
                    CastInfo.DesignerTotalTime = CastInfo.DesignerCastTime + trueChannel;
                }
            }

            if (!CastInfo.IsAutoAttack)
            {
                _game.PacketNotifier.NotifyNPC_CastSpellAns(this);
            }

            if (CastInfo.DesignerCastTime > 0)
            {
                if (Script.ScriptMetadata.TriggersSpellCasts)
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

            return true;
        }

        public bool Cast(ICastInfo castInfo, bool cast)
        {
            CastInfo = castInfo;
            var start = new Vector2(CastInfo.TargetPosition.X, CastInfo.TargetPosition.Z);
            var end = new Vector2(CastInfo.TargetPositionEnd.X, CastInfo.TargetPositionEnd.Z);

            Script.OnSpellPreCast(CastInfo.Owner, this, castInfo.Targets[0].Unit, start, end);

            var stats = CastInfo.Owner.Stats;

            if (cast)
            {
                if ((SpellData.ManaCost[CastInfo.SpellLevel] * (1 - stats.SpellCostReduction) > stats.CurrentMana && !CastInfo.IsAutoAttack) || State != SpellState.STATE_READY)
                {
                    return false;
                }

                if (_game.Config.GameFeatures.HasFlag(FeatureFlags.EnableManaCosts))
                {
                    stats.CurrentMana -= SpellData.ManaCost[CastInfo.SpellLevel] * (1 - stats.SpellCostReduction);
                }
            }

            _futureProjNetId = _networkIdManager.GetNewNetId();

            CastInfo.MissileNetID = _futureProjNetId;

            CastInfo.ExtraCastTime = 0.0f; // TODO: Unhardcode
            CastInfo.Cooldown = GetCooldown();
            CastInfo.StartCastTime = 0.0f; // TODO: Unhardcode

            var trueChannel = SpellData.ChannelDuration[CastInfo.SpellLevel];
            if (Script.ScriptMetadata.ChannelDuration > 0)
            {
                trueChannel = Script.ScriptMetadata.ChannelDuration;
            }

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
                CastInfo.DesignerTotalTime = CastInfo.DesignerCastTime + trueChannel;
            }

            if (SpellData.OverrideCastTime > 0)
            {
                CastInfo.DesignerCastTime = SpellData.OverrideCastTime;
                CastInfo.DesignerTotalTime = SpellData.OverrideCastTime + trueChannel;
            }

            if (Script.ScriptMetadata.CastTime > 0)
            {
                CastInfo.DesignerCastTime = Script.ScriptMetadata.CastTime;
                CastInfo.DesignerTotalTime = Script.ScriptMetadata.CastTime + trueChannel;
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

            if (cast && (!CastInfo.IsAutoAttack && !SpellData.IsToggleSpell
                        || (!SpellData.NoWinddownIfCancelled
                        && !SpellData.Flags.HasFlag(SpellDataFlags.InstantCast)
                        && SpellData.CantCancelWhileWindingUp)))
            {
                if (Script.ScriptMetadata.TriggersSpellCasts || SpellData.ChannelDuration[CastInfo.SpellLevel] > 0 || Script.ScriptMetadata.ChannelDuration > 0)
                {
                    if (!SpellData.Flags.HasFlag(SpellDataFlags.InstantCast))
                    {
                        CastInfo.Owner.StopMovement();

                        // TODO: Verify if we should move this outside of this TriggersSpellCasts if statement.
                        CastInfo.Owner.UpdateMoveOrder(OrderType.CastSpell, true);
                    }

                    if (Script.ScriptMetadata.AutoFaceDirection)
                    {
                        var goingTo = end - CastInfo.Owner.Position;

                        if (CastInfo.Targets[0].Unit != null)
                        {
                            goingTo = CastInfo.Targets[0].Unit.Position - CastInfo.Owner.Position;
                        }

                        var dirTemp = Vector2.Normalize(goingTo);
                        CastInfo.Owner.FaceDirection(new Vector3(dirTemp.X, 0, dirTemp.Y), false);
                    }
                }
            }

            if (CastInfo.IsAutoAttack && CastInfo.Owner.IsMelee)
            {
                attackType = AttackType.ATTACK_TYPE_MELEE;
            }

            if (cast && CastInfo.Targets[0].Unit != null && CastInfo.Targets[0].Unit != CastInfo.Owner)
            {
                if (Script.ScriptMetadata.AutoFaceDirection)
                {
                    ApiFunctionManager.FaceDirection(CastInfo.Targets[0].Unit.Position, CastInfo.Owner);
                }

                _game.PacketNotifier.NotifyS2C_UnitSetLookAt(CastInfo.Owner, CastInfo.Targets[0].Unit, attackType);
            }

            if (CastInfo.IsAutoAttack || CastInfo.UseAttackCastTime)
            {
                // We assume it is already an attack.
                int index = CastInfo.SpellSlot - 64;
                if (CastInfo.SpellSlot >= 45 && CastInfo.SpellSlot <= 60)
                {
                    // Extra Spells which UseAttackCastTime just use the base auto attack's cast time.
                    index = 0;
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
                    CastInfo.DesignerTotalTime = CastInfo.DesignerCastTime + trueChannel;
                }
            }

            if (cast)
            {
                if (!CastInfo.IsAutoAttack)
                {
                    _game.PacketNotifier.NotifyNPC_CastSpellAns(this);
                }

                if (CastInfo.DesignerCastTime > 0)
                {
                    if (Script.ScriptMetadata.TriggersSpellCasts)
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
            }
            else
            {
                if (Script.ScriptMetadata.MissileParameters != null)
                {
                    CreateSpellMissile();
                }

                if (Script.ScriptMetadata.SectorParameters != null)
                {
                    CreateSpellSector();
                }
            }

            return true;
        }

        public void Channel()
        {
            State = SpellState.STATE_CHANNELING;
            CurrentChannelDuration = SpellData.ChannelDuration[CastInfo.SpellLevel];
            if (Script.ScriptMetadata.ChannelDuration > 0)
            {
                CurrentChannelDuration = Script.ScriptMetadata.ChannelDuration;
                ApiEventManager.OnSpellChannel.Publish(this);
            }

            if (CurrentChannelDuration > 0)
            {
                CastInfo.Owner.SetChannelSpell(this);
            }
        }

        public void ChannelCancelCheck()
        {
            // Uncancellable
            if (SpellData.CantCancelWhileChanneling)
            {
                return;
            }

            if (CastInfo.Targets[0].Unit != null)
            {
                var spellTarget = CastInfo.Targets[0].Unit;

                if (spellTarget != null
                && !spellTarget.IsVisibleByTeam(CastInfo.Owner.Team))
                {
                    CastInfo.Owner.StopChanneling(ChannelingStopCondition.Cancel, ChannelingStopSource.LostTarget);
                    return;
                }
            }

            var status = CastInfo.Owner.Status;

            if (status == StatusFlags.Charmed
            || status == StatusFlags.Feared
            || status == StatusFlags.Silenced
            || status == StatusFlags.Stunned
            || status == StatusFlags.Suppressed
            || status == StatusFlags.Taunted)
            {
                CastInfo.Owner.StopChanneling(ChannelingStopCondition.Cancel, ChannelingStopSource.StunnedOrSilencedOrTaunted);
            }

            if (CastInfo.Owner.IsDead)
            {
                CastInfo.Owner.StopChanneling(ChannelingStopCondition.Cancel, ChannelingStopSource.Die);
                return;
            }

            // TODO: ChannelingStopSource.HeroReincarnate

            var order = CastInfo.Owner.MoveOrder;

            if (!SpellData.CanMoveWhileChanneling
            && (order == OrderType.MoveTo
                || order == OrderType.AttackMove
                || order == OrderType.PetHardMove))
            {
                if (order == OrderType.MoveTo
                || order == OrderType.AttackMove
                || order == OrderType.PetHardMove)
                {
                    CastInfo.Owner.StopChanneling(ChannelingStopCondition.Cancel, ChannelingStopSource.Move);
                    return;
                }
            }

            if (order == OrderType.AttackTo
            || order == OrderType.AttackTerrainOnce
            || order == OrderType.AttackTerrainSustained
            || order == OrderType.PetHardAttack)
            {
                CastInfo.Owner.StopChanneling(ChannelingStopCondition.Cancel, ChannelingStopSource.Attack);
                return;
            }

            if (CastInfo.Owner.GetCastSpell() != null
            && !CastInfo.Owner.GetCastSpell().SpellData.DoesntBreakChannels
            && (order == OrderType.CastSpell
            || order == OrderType.TempCastSpell))
            {
                CastInfo.Owner.StopChanneling(ChannelingStopCondition.Cancel, ChannelingStopSource.Casting);
                return;
            }

            // TODO: ChannelingStopSource.Unknown
        }

        public void StopChanneling(ChannelingStopCondition condition, ChannelingStopSource reason)
        {
            if (condition == ChannelingStopCondition.Cancel)
            {
                ApiEventManager.OnSpellChannelCancel.Publish(this, reason);

                // For some reason this is the packet used for manually cancelling channels.
                _game.PacketNotifier.NotifyNPC_InstantStop_Attack(CastInfo.Owner, false);

                // TODO: Find out how League calculates cooldown reduction for incomplete channels (assuming it isn't done in-script).
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
            Script.OnDeactivate(CastInfo.Owner, this);
        }

        public void FinishCasting()
        {
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
                        CreateSpellMissile(new MissileParameters
                        {
                            Type = MissileType.Target
                        });
                    }
                }
                else
                {
                    ApplyEffects(CastInfo.Targets[0].Unit);
                    CastInfo.Owner.AutoAttackHit(CastInfo.Targets[0].Unit);
                }

                State = SpellState.STATE_READY;
            }
            else
            {
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

            if (Script.ScriptMetadata.TriggersSpellCasts)
            {
                ApiEventManager.OnSpellPostCast.Publish(this);
            }

            if (Script.ScriptMetadata.MissileParameters != null)
            {
                CreateSpellMissile();
            }

            if (Script.ScriptMetadata.SectorParameters != null)
            {
                CreateSpellSector();
            }

            if (CastInfo.Owner.SpellToCast != null)
            {
                CastInfo.Owner.SetSpellToCast(null, Vector2.Zero);
            }

            if (CastInfo.Owner.GetCastSpell() == this)
            {
                CastInfo.Owner.SetCastSpell(null);
            }

            // Updates move order before script PostCast so teleports are sent to clients correctly (not sent if MoveOrder == CastSpell).
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
        }

        /// <summary>
        /// Creates a spell missile with the given parameters.
        /// </summary>
        /// <param name="parameters">Parameters of the missile.</param>
        public ISpellMissile CreateSpellMissile(IMissileParameters parameters)
        {
            if (CastInfo.MissileNetID == 0)
            {
                return null;
            }

            var netId = CastInfo.MissileNetID;

            if (_game.ObjectManager.GetObjectById(netId) != null)
            {
                netId = _game.NetworkIdManager.GetNewNetId();
            }

            bool isServerOnly = SpellData.MissileEffect != "";

            ISpellMissile p = null;

            switch (parameters.Type)
            {
                case MissileType.Target:
                {
                    p = new SpellMissile(
                        _game,
                        (int)SpellData.LineWidth,
                        this,
                        CastInfo,
                        SpellData.MissileSpeed,
                        SpellData.Flags,
                        netId,
                        isServerOnly
                    );
                    break;
                }
                case MissileType.Chained:
                {
                    p = new SpellChainMissile(
                        _game,
                        (int)SpellData.LineWidth,
                        this,
                        CastInfo,
                        parameters,
                        SpellData.MissileSpeed,
                        SpellData.Flags,
                        netId,
                        isServerOnly
                    );
                    break;
                }
                case MissileType.Circle:
                {
                    p = new SpellCircleMissile(
                        _game,
                        (int)SpellData.LineWidth,
                        this,
                        CastInfo,
                        SpellData.MissileSpeed,
                        parameters.OverrideEndPosition,
                        SpellData.Flags,
                        netId,
                        isServerOnly
                    );
                    break;
                }
                case MissileType.Arc:
                {
                    p = new SpellLineMissile(
                        _game,
                        (int)SpellData.LineWidth,
                        this,
                        CastInfo,
                        SpellData.MissileSpeed,
                        parameters.OverrideEndPosition,
                        SpellData.Flags,
                        netId,
                        isServerOnly
                    );
                    break;
                }
            }

            // If the position is the same as the destination, the server will have destroyed the missile before notifying of creation, causing the client to crash.
            // TODO: Make a better check.
            if (p == null || (p is ISpellCircleMissile c && c.Position == c.Destination)
                || p.Position == p.GetTargetPosition())
            {
                return null;
            }

            _game.ObjectManager.AddObject(p);

            ApiEventManager.OnLaunchMissile.Publish(new KeyValuePair<IObjAiBase, ISpell>(CastInfo.Owner, this), p);

            _game.PacketNotifier.NotifyMissileReplication(p);

            // TODO: Verify when NotifyForceCreateMissile should be used instead.

            return p;
        }

        /// <summary>
        /// Creates a spell missile using this spell's script for the parameters.
        /// </summary>
        public ISpellMissile CreateSpellMissile()
        {
            return CreateSpellMissile(Script.ScriptMetadata.MissileParameters);
        }

        /// <summary>
        /// Creates a spell sector with the given parameters.
        /// </summary>
        /// <param name="parameters">Parameters of the sector.</param>
        public ISpellSector CreateSpellSector(ISectorParameters parameters)
        {
            if (CastInfo.MissileNetID == 0)
            {
                return null;
            }

            var netId = CastInfo.MissileNetID;

            if (_game.ObjectManager.GetObjectById(netId) != null)
            {
                netId = _game.NetworkIdManager.GetNewNetId();
            }

            ISpellSector s = null;

            switch (parameters.Type)
            {
                case SectorType.Area:
                {
                    s = new SpellSector(
                        _game,
                        parameters,
                        this,
                        CastInfo,
                        netId
                    );
                    break;
                }
                case SectorType.Cone:
                {
                    s = new SpellSectorCone(
                        _game,
                        parameters,
                        this,
                        CastInfo,
                        netId
                    );
                    break;
                }
                case SectorType.Polygon:
                {
                    s = new SpellSectorPolygon(
                        _game,
                        parameters,
                        this,
                        CastInfo,
                        netId
                    );
                    break;
                }
                case SectorType.Ring:
                {
                    // TODO
                    break;
                }
            }

            _game.ObjectManager.AddObject(s);

            ApiEventManager.OnCreateSector.Publish(new KeyValuePair<IObjAiBase, ISpell>(CastInfo.Owner, this), s);

            // TODO: Verify when NotifyForceCreateMissile should be used instead.

            return s;
        }

        /// <summary>
        /// Creates a spell sector using this spell's script for the parameters.
        /// </summary>
        public ISpellSector CreateSpellSector()
        {
            return CreateSpellSector(Script.ScriptMetadata.SectorParameters);
        }

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
            return _game.Config.GameFeatures.HasFlag(FeatureFlags.EnableCooldowns) ? SpellData.Cooldown[CastInfo.SpellLevel] * (1 - CastInfo.Owner.Stats.CooldownReduction.Total) : 0;
        }

        public float GetCurrentCastRange()
        {
            if (_overrrideCastRange > 0)
            {
                return _overrrideCastRange;
            }

            float castRange = SpellData.CastRange[0];

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

        public int GetId()
        {
            return (int)HashFunctions.HashString(SpellName);
        }

        public string GetStringForSlot()
        {
            return CastInfo.SpellSlot switch
            {
                0 => "Q",
                1 => "W",
                2 => "E",
                3 => "R",
                62 => "Passive",
                var n when (n <= 81 && n >= 64) => "Attack",
                _ => "undefined"
            };
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

        public void ResetSpellCast()
        {
            State = SpellState.STATE_READY;
            CurrentCastTime = 0;
            CurrentChannelDuration = 0;
            CurrentDelayTime = 0;
        }

        /// <summary>
        /// Toggles the auto cast state for this spell.
        /// </summary>
        public void SetAutocast()
        {
            _game.PacketNotifier.NotifyNPC_SetAutocast(CastInfo.Owner, this);
        }

        /// <summary>
        /// Overrides the normal cast range for this spell. Set to 0 to revert.
        /// </summary>
        /// <param name="newCastRange">Cast range to set.</param>
        public void SetOverrideCastRange(float newCastRange)
        {
            _overrrideCastRange = newCastRange;

            if (CastInfo.Owner is IChampion champion)
            {
                _game.PacketNotifier.NotifyChangeSlotSpellData
                (
                    (int)_game.PlayerManager.GetClientInfoByChampion(champion).PlayerId,
                    champion,
                    (byte)CastInfo.SpellSlot,
                    ChangeSlotSpellDataType.Range,
                    newRange: newCastRange
                );
            }
        }

        /// <summary>
        /// Sets the cooldown of this spell.
        /// </summary>
        /// <param name="newCd">Cooldown to set.</param>
        /// <param name="ignoreCDR">Whether or not to ignore cooldown reduction.</param>
        public void SetCooldown(float newCd, bool ignoreCDR = false)
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
                if (!ignoreCDR)
                {
                    newCd *= (1 - CastInfo.Owner.Stats.CooldownReduction.Total);
                }

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

            if (CastInfo.Owner is IChampion champion)
            {
                _game.PacketNotifier.NotifyS2C_SetSpellLevel((int)_game.PlayerManager.GetClientInfoByChampion(champion).PlayerId, champion.NetId, CastInfo.SpellSlot, toLevel);
            }
        }

        public void SetSpellState(SpellState state)
        {
            State = state;
        }


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
                Script.OnUpdate(diff);
            }

            switch (State)
            {
                case SpellState.STATE_READY:
                    break;
                case SpellState.STATE_CASTING:
                {
                    CastCancelCheck();
                    if (!CastInfo.IsAutoAttack && !CastInfo.UseAttackCastTime)
                    {
                        CurrentCastTime -= diff / 1000.0f;
                        if (CurrentCastTime <= 0)
                        {
                            FinishCasting();
                            if (SpellData.ChannelDuration[CastInfo.SpellLevel] > 0 || Script.ScriptMetadata.ChannelDuration > 0)
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
                    ChannelCancelCheck();
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

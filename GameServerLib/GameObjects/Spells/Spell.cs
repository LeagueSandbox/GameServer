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
        public Dictionary<uint, IProjectile> Projectiles { get; protected set; }

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

            Projectiles = new Dictionary<uint, IProjectile>();

            //Set the game script for the spell
            _spellScript = _scriptEngine.CreateObject<ISpellScript>("Spells", spellName) ?? new SpellScriptEmpty();

            if (_spellScript.ScriptMetadata.TriggersSpellCasts)
            {
                ApiEventManager.OnSpellCast.AddListener(_spellScript, this, _spellScript.OnSpellCast);
                ApiEventManager.OnSpellPostCast.AddListener(_spellScript, this, _spellScript.OnSpellPostCast);
            }

            if (_spellScript.ScriptMetadata.ChannelDuration > 0)
            {
                // TODO: Implement OnSpellChannel and OnSpellPostChannel in ISpellScript and AddListener on both.
            }

            //Activate spell - Notes: Deactivate is never called as spell removal hasn't been added
            _spellScript.OnActivate(owner, this);
        }

        /// <summary>
        /// Creates a area of effect cone at this spell's owner position pointing towards the given target position that will act as follows:
        /// ApplyEffect for each unit that has not been affected yet within the area.
        /// If affectAsCastIsOver = true: when the spell origin has finished casting, despawns after doing one area of effect check.
        /// If false: performs continuous area of effect checks until manually SetToRemove.
        /// </summary>
        /// <param name="effectName">Internal name of the cone to spawn. Required for cone features.</param>
        /// <param name="target">Position the area of effect will point towards (from the owner's position).</param>
        /// <param name="hitResult">How the damage applied by this area of effect should be shown to clients.</param>
        /// <param name="affectAsCastIsOver">Whether or not the area of effect will last until its origin spell is finished casting. False = lasts forever (or until something calls SetToRemove for it manually, likely via spell script).</param>
        /// <returns>Newly created area of effect cone with the given functionality.</returns>
        public IProjectile AddCone(string effectName, Vector2 target, float angleDeg, HitResult hitResult = HitResult.HIT_Normal, bool affectAsCastIsOver = true)
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

            return c;
        }

        /// <summary>
        /// Creates a area of effect rectangle at this spell's owner position pointing towards the given target position that will act as follows:
        /// ApplyEffect for each unit that has not been affected yet within the area.
        /// If affectAsCastIsOver = true: when the spell origin has finished casting, despawns after doing one area of effect check.
        /// If false: performs continuous area of effect checks until manually SetToRemove.
        /// </summary>
        /// <param name="effectName">Internal name of the laser to spawn. Required for laser features.</param>
        /// <param name="target">Position the area of effect will point towards (from the owner's position).</param>
        /// <param name="hitResult">How the damage applied by this area of effect should be shown to clients.</param>
        /// <param name="affectAsCastIsOver">Whether or not the area of effect will last until its origin spell is finished casting. False = lasts forever (or until something calls SetToRemove for it manually, likely via spell script).</param>
        /// <returns>Newly created area of effect rectangle with the given functionality.</returns>
        public IProjectile AddLaser(string effectName, Vector2 target, HitResult hitResult = HitResult.HIT_Normal, bool affectAsCastIsOver = true)
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

            return l;
        }

        /// <summary>
        /// Creates a line missile with the specified properties.
        /// </summary>
        public IProjectile AddProjectile(ICastInfo castInfo)
        {
            var isServerOnly = false;

            if (SpellData.MissileEffect != "")
            {
                isServerOnly = true;
            }

            var p = new Projectile(
                _game,
                (int)SpellData.LineWidth,
                this,
                castInfo,
                SpellData.MissileSpeed,
                SpellName,
                SpellData.Flags,
                castInfo.MissileNetID,
                isServerOnly
            );

            _game.ObjectManager.AddObject(p);

            _game.PacketNotifier.NotifyMissileReplication(p);
            
            // TODO: Verify when NotifyForceCreateMissile should be used instead.

            return p;
        }

        /// <summary>
        /// Creates a single-target missile at the specified cast position that will move as follows: Owner.Position -> target.Position. Despawns when Position = target.Position
        /// </summary>
        /// <param name="nameMissile">Internal name of the missile to spawn. Required for missile features.</param>
        /// <param name="castPos">Position the missile will spawn at.</param>
        /// <param name="target">Unit the missile will move towards. Once hit, this missile will despawn (unless it has bounces left).</param>
        /// <param name="hitResult">How the damage applied by this projectile should be shown to clients.</param>
        /// <param name="isServerOnly">Whether or not this missile will only spawn server-side.</param>
        /// <param name="overrideCastPosition">Whether or not to override default cast position behavior with the given cast position.</param>
        /// <returns>Newly created missile with the given functionality.</returns>
        public IProjectile AddProjectileTarget(string nameMissile, Vector3 castPos, IAttackableUnit target, HitResult hitResult = HitResult.HIT_Normal, bool isServerOnly = false, bool overrideCastPosition = false)
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
                IsOverrideCastPosition = overrideCastPosition, // TODO: Verify
            };

            IProjectile p;

            if (nameMissile == "")
            {
                castInfo.SpellHash = HashFunctions.HashString(""); // TODO: Verify
                castInfo.IsClickCasted = true; // TODO: Verify
                castInfo.ManaCost = 0;
                castInfo.SpellCastLaunchPosition = castInfo.Owner.GetPosition3D();
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
                if (float.IsNaN(castPos.Y) || castPos.Y == 0.0f)
                {
                    castPos.Y = _game.Map.NavigationGrid.GetHeightAtLocation(castPos.X, castPos.Z);
                }
                castInfo.SpellCastLaunchPosition = castPos;
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

            return p;
        }

        /// <summary>
        /// Called by projectiles when they land / hit, this is where we apply damage/slows etc.
        /// </summary>
        public void ApplyEffects(IAttackableUnit u, IProjectile p = null)
        {
            if (SpellData.HaveHitEffect && !string.IsNullOrEmpty(SpellData.HitEffectName))
            {
                ApiFunctionManager.AddParticleTarget(CastInfo.Owner, SpellData.HitEffectName, u, lifetime: 3.0f);
            }

            ApiEventManager.OnSpellHit.Publish(CastInfo.Owner, this, u, p);

            if (p != null && p.IsToRemove())
            {
                Projectiles.Remove(p.NetId);
            }
        }

        /// <summary>
        /// Removes the given projectile instance from this spell's dictionary of projectiles. Will automatically SetToRemove the projectile.
        /// </summary>
        /// <param name="p">Projectile to remove.</param>
        public void RemoveProjectile(IProjectile p)
        {
            if (Projectiles.ContainsKey(p.NetId))
            {
                Projectiles.Remove(p.NetId);
                if (!p.IsToRemove())
                {
                    p.SetToRemove();
                }
            }
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
                && SpellData.TargetingType == TargetingType.Self
                || SpellData.TargetingType == TargetingType.SelfAOE
                || SpellData.TargetingType == TargetingType.TargetOrLocation)
            {
                unit = CastInfo.Owner;
            }

            var distance = Vector2.DistanceSquared(start, CastInfo.Owner.Position);
            if (SpellData.TargetingType == TargetingType.Target)
            {
                distance = Vector2.DistanceSquared(start, unit.Position);
            }
            var castRange = GetCurrentCastRange();
            if (distance > castRange * castRange)
            {
                return false;
            }

            _spellScript.OnSpellPreCast(CastInfo.Owner, this, unit, start, end);

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

            if (Vector2.DistanceSquared(CastInfo.Owner.Position, start) > SpellData.CastRangeDisplayOverride * SpellData.CastRangeDisplayOverride)
            {
                start = Extensions.GetClosestCircleEdgePoint(start, CastInfo.Owner.Position, SpellData.CastRangeDisplayOverride);
            }

            CastInfo.TargetPosition = new Vector3(start.X, _game.Map.NavigationGrid.GetHeightAtLocation(start.X, start.Y), start.Y);
            CastInfo.TargetPositionEnd = new Vector3(end.X, _game.Map.NavigationGrid.GetHeightAtLocation(end.X, end.Y), end.Y);

            CastInfo.Targets.Clear();

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

            // TODO: Account for multiple targets
            CastInfo.Targets.Add(new CastTarget(unit, CastTarget.GetHitResult(unit, CastInfo.IsAutoAttack, CastInfo.Owner.IsNextAutoCrit)));

            // TODO: implement check for IsForceCastingOrChannel and IsOverrideCastPosition
            if (SpellData.CastType == (int)CastType.CAST_TargetMissile)
            {
                // TODO: Verify
                CastInfo.IsClickCasted = true;
            }

            // TODO: Verify
            CastInfo.SpellCastLaunchPosition = CastInfo.Owner.GetPosition3D();

            var attackType = AttackType.ATTACK_TYPE_RADIAL;

            if (!CastInfo.IsAutoAttack
                && (SpellData.TargetingType == TargetingType.Target && CastInfo.Targets[0].Unit != null
                && Vector2.DistanceSquared(CastInfo.Targets[0].Unit.Position, CastInfo.Owner.Position) > SpellData.CastRange[CastInfo.SpellLevel] * SpellData.CastRange[CastInfo.SpellLevel])
                || ((SpellData.TargetingType == TargetingType.Area
                    || SpellData.TargetingType == TargetingType.Cone
                    || SpellData.TargetingType == TargetingType.Direction
                    || SpellData.TargetingType == TargetingType.Location
                    || SpellData.TargetingType == TargetingType.TargetOrLocation
                    || SpellData.TargetingType == TargetingType.DragDirection)
                    && (Vector2.DistanceSquared(new Vector2(CastInfo.TargetPosition.X, CastInfo.TargetPosition.Z), CastInfo.Owner.Position) > SpellData.CastRange[CastInfo.SpellLevel] * SpellData.CastRange[CastInfo.SpellLevel]))
                    || (!CastInfo.IsAutoAttack && CastInfo.Owner.IsAttacking))
            {
                attackType = AttackType.ATTACK_TYPE_TARGETED;
                //CastInfo.Owner.SetSpellToCast(this);
                return false;
            }

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

                    var goingTo = start - CastInfo.Owner.Position;

                    if (unit != null)
                    {
                        goingTo = unit.Position - CastInfo.Owner.Position;
                    }

                    var dirTemp = Vector2.Normalize(goingTo);
                    CastInfo.Owner.FaceDirection(new Vector3(dirTemp.X, 0, dirTemp.Y), false);
                }
                CastInfo.Owner.UpdateMoveOrder(OrderType.CastSpell);
            }

            if (CastInfo.IsAutoAttack && CastInfo.Owner.IsMelee)
            {
                attackType = AttackType.ATTACK_TYPE_MELEE;
            }

            if (CastInfo.Targets[0].Unit != null && CastInfo.Targets[0].Unit != CastInfo.Owner)
            {
                if (CastInfo.Targets[0].Unit != CastInfo.Owner.TargetUnit)
                {
                    CastInfo.Owner.SetTargetUnit(CastInfo.Targets[0].Unit, false);
                }

                _game.PacketNotifier.NotifyS2C_UnitSetLookAt(CastInfo.Owner, CastInfo.Targets[0].Unit, attackType);
            }

            // TODO: Implement a default FaceDirection behavior and let scripts override it.

            if (CastInfo.IsAutoAttack || CastInfo.UseAttackCastTime)
            {
                int startIndex = 64;
                if (CastInfo.SpellSlot >= 44 && CastInfo.SpellSlot < 51)
                {
                    startIndex = CastInfo.SpellSlot;
                }

                float autoAttackTotalTime = CastInfo.Owner.CharData.GlobalCharData.AttackDelay * (1.0f + CastInfo.Owner.CharData.AttackDelayOffsetPercent[0]);
                CastInfo.DesignerCastTime = autoAttackTotalTime * (CastInfo.Owner.CharData.GlobalCharData.AttackDelayCastPercent + CastInfo.Owner.CharData.AttackDelayCastOffsetPercent[CastInfo.SpellSlot - startIndex]);

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
                    CastInfo.DesignerTotalTime = CurrentCastTime + SpellData.ChannelDuration[CastInfo.SpellLevel];
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
                    ApiEventManager.OnLaunchAttack.Publish(CastInfo.Owner, this);
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

            //if (Vector2.DistanceSquared(CastInfo.Owner.Position, start) > SpellData.CastRangeDisplayOverride * SpellData.CastRangeDisplayOverride)
            //{
            //    start = Extensions.GetClosestCircleEdgePoint(start, CastInfo.Owner.Position, SpellData.CastRangeDisplayOverride);
            //}

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
            if (SpellData.CastType == (int)CastType.CAST_TargetMissile)
            {
                // TODO: Verify
                CastInfo.IsClickCasted = true;
            }

            // TODO: Verify
            var attackType = AttackType.ATTACK_TYPE_RADIAL;

            /*
            if (!CastInfo.IsAutoAttack
                && (SpellData.TargetingType == TargetingType.Target && CastInfo.Targets[0].Unit != null
                && Vector2.DistanceSquared(CastInfo.Targets[0].Unit.Position, CastInfo.Owner.Position) > SpellData.CastRange[CastInfo.SpellLevel] * SpellData.CastRange[CastInfo.SpellLevel])
                || ((SpellData.TargetingType == TargetingType.Area
                    || SpellData.TargetingType == TargetingType.Cone
                    || SpellData.TargetingType == TargetingType.Direction
                    || SpellData.TargetingType == TargetingType.Location
                    || SpellData.TargetingType == TargetingType.TargetOrLocation
                    || SpellData.TargetingType == TargetingType.DragDirection)
                    && (Vector2.DistanceSquared(new Vector2(CastInfo.TargetPosition.X, CastInfo.TargetPosition.Z), CastInfo.Owner.Position) > SpellData.CastRange[CastInfo.SpellLevel] * SpellData.CastRange[CastInfo.SpellLevel]))
                    || (!CastInfo.IsAutoAttack && CastInfo.Owner.IsAttacking))
            {
                //attackType = AttackType.ATTACK_TYPE_TARGETED;
                CastInfo.Owner.SetSpellToCast(this);
                return false;
            }
            */

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

                    var goingTo = start - CastInfo.Owner.Position;

                    if (castInfo.Targets[0].Unit != null)
                    {
                        goingTo = castInfo.Targets[0].Unit.Position - CastInfo.Owner.Position;
                    }

                    var dirTemp = Vector2.Normalize(goingTo);
                    CastInfo.Owner.FaceDirection(new Vector3(dirTemp.X, 0, dirTemp.Y), false);
                }
                CastInfo.Owner.UpdateMoveOrder(OrderType.CastSpell);
            }

            if (CastInfo.IsAutoAttack && CastInfo.Owner.IsMelee)
            {
                attackType = AttackType.ATTACK_TYPE_MELEE;
            }

            if (cast && CastInfo.Targets[0].Unit != null && CastInfo.Targets[0].Unit != CastInfo.Owner)
            {
                _game.PacketNotifier.NotifyS2C_UnitSetLookAt(CastInfo.Owner, CastInfo.Targets[0].Unit, attackType);
            }

            // TODO: Implement a default FaceDirection behavior and let scripts override it.

            if (CastInfo.IsAutoAttack || CastInfo.UseAttackCastTime)
            {
                int startIndex = 64;
                if (CastInfo.SpellSlot >= 44 && CastInfo.SpellSlot < 51)
                {
                    startIndex = CastInfo.SpellSlot;
                }

                float autoAttackTotalTime = CastInfo.Owner.CharData.GlobalCharData.AttackDelay * (1.0f + CastInfo.Owner.CharData.AttackDelayOffsetPercent[0]);
                CastInfo.DesignerCastTime = autoAttackTotalTime * (CastInfo.Owner.CharData.GlobalCharData.AttackDelayCastPercent + CastInfo.Owner.CharData.AttackDelayCastOffsetPercent[CastInfo.SpellSlot - startIndex]);

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
                    CastInfo.DesignerTotalTime = CurrentCastTime + SpellData.ChannelDuration[CastInfo.SpellLevel];
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
                        ApiEventManager.OnLaunchAttack.Publish(CastInfo.Owner, this);
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
                    AddProjectile(CastInfo);
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
        }

        void ISpell.Deactivate()
        {
            CastInfo.Targets.Clear();
            Projectiles.Clear();
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
                        AddProjectileTarget(SpellName, CastInfo.SpellCastLaunchPosition, CastInfo.Targets[0].Unit, hitResult);
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
                AddProjectile(CastInfo);
            }

            CastInfo.Owner.UpdateMoveOrder(OrderType.Hold);

            if (SpellData.Flags.HasFlag(SpellDataFlags.InstantCast))
            {
                if (!CastInfo.Owner.IsPathEnded())
                {
                    CastInfo.Owner.UpdateMoveOrder(OrderType.MoveTo);
                }
                if (CastInfo.Owner.TargetUnit != null)
                {
                    CastInfo.Owner.UpdateMoveOrder(OrderType.AttackTo);
                }
            }

            if (CastInfo.Owner.SpellToCast != null)
            {
                //CastInfo.Owner.SetSpellToCast(null);
            }
        }

        /// <summary>
        /// Called when the character finished channeling
        /// </summary>
        public void FinishChanneling()
        {
            ApiEventManager.OnSpellPostChannel.Publish(this);

            CastInfo.Owner.UpdateMoveOrder(OrderType.Hold);

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

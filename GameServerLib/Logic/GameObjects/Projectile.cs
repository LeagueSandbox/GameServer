using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Core.Logic;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Projectile : GameObject
    {
        public List<GameObject> ObjectsHit { get; private set; }
        public Unit Owner { get; private set; }
        public int ProjectileId { get; private set; }
        protected float _moveSpeed;
        protected int _flags;
        protected Spell _originSpell;
        private RAFManager _rafManager = Program.ResolveDependency<RAFManager>();
        private Logger _logger = Program.ResolveDependency<Logger>();

        public Projectile(
            float x,
            float y,
            int collisionRadius,
            Unit owner,
            Target target,
            Spell originSpell,
            float moveSpeed,
            string projectileName,
            int flags = 0,
            uint netId = 0
        ) : base(x, y, collisionRadius, 0, netId)
        {
            _originSpell = originSpell;
            _moveSpeed = moveSpeed;
            Owner = owner;
            Team = owner.Team;
            ProjectileId = (int)_rafManager.GetHash(projectileName);
            if (!string.IsNullOrEmpty(projectileName))
            {
                JObject data;
                if (!_rafManager.ReadSpellData(projectileName, out data))
                {
                    _logger.LogCoreError("Couldn't find projectile stats for " + projectileName);
                    return;
                }
                VisionRadius = _rafManager.GetFloatValue(data, "SpellData", "MissilePerceptionBubbleRadius");
            }
            _flags = flags;
            ObjectsHit = new List<GameObject>();

            Target = target;

            if (!target.IsSimpleTarget)
            {
                ((GameObject)target).incrementAttackerCount();
            }

            owner.incrementAttackerCount();
        }

        public override void update(float diff)
        {
            if (Target == null)
            {
                setToRemove();
                return;
            }

            base.update(diff);
        }

        public override void onCollision(GameObject collider)
        {
            base.onCollision(collider);
            if (Target != null && Target.IsSimpleTarget && !isToRemove())
            {
                CheckFlagsForUnit(collider as Unit);
            }
            else
            {
                if (Target == collider)
                {
                    CheckFlagsForUnit(collider as Unit);
                }
            }
        }

        public override float getMoveSpeed()
        {
            return _moveSpeed;
        }

        protected virtual void CheckFlagsForUnit(Unit unit)
        {
            if (Target == null)
            {
                return;
            }

            if (Target.IsSimpleTarget)
            { // Skillshot
                if (unit == null || ObjectsHit.Contains(unit))
                    return;

                if (unit.Team == Owner.Team
                    && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectFriends) > 0))
                    return;

                if (unit.Team == TeamId.TEAM_NEUTRAL
                    && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectNeutral) > 0))
                    return;

                if (unit.Team != Owner.Team
                    && unit.Team != TeamId.TEAM_NEUTRAL
                    && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectEnemies) > 0))
                    return;


                if (unit.IsDead && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectDead) > 0))
                    return;

                var m = unit as Minion;
                if (m != null && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectMinions) > 0))
                    return;

                var p = unit as Placeable;
                if (p != null && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectUseable) > 0))
                    return;

                var t = unit as BaseTurret;
                if (t != null && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectTurrets) > 0))
                    return;

                var i = unit as Inhibitor;
                var n = unit as Nexus;
                if ((i != null || n != null) && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectBuildings) > 0))
                    return;

                var c = unit as Champion;
                if (c != null && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectHeroes) > 0))
                    return;

                ObjectsHit.Add(unit);
                _originSpell.applyEffects(unit, this);
            }
            else
            {
                var u = Target as Unit;
                if (u != null)
                { // Autoguided spell
                    if (_originSpell != null)
                    {
                        _originSpell.applyEffects(u, this);
                    }
                    else
                    { // auto attack
                        Owner.AutoAttackHit(u);
                        setToRemove();
                    }
                }
            }
        }

        public override void setToRemove()
        {
            if (Target != null && !Target.IsSimpleTarget)
                (Target as GameObject).decrementAttackerCount();

            Owner.decrementAttackerCount();
            base.setToRemove();
            _game.PacketNotifier.NotifyProjectileDestroy(this);
        }
    }
}

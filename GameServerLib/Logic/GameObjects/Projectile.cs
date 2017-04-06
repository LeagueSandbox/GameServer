using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Core.Logic;
using Newtonsoft.Json.Linq;
using LeagueSandbox.GameServer.Logic.Content;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Projectile : GameObject
    {
        public List<GameObject> ObjectsHit { get; private set; }
        public Unit Owner { get; private set; }
        public int ProjectileId { get; private set; }
        public SpellData SpellData { get; private set; }
        protected float _moveSpeed;
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
            SpellData = _game.Config.ContentManager.GetSpellData(projectileName);
            _originSpell = originSpell;
            _moveSpeed = moveSpeed;
            Owner = owner;
            Team = owner.Team;
            ProjectileId = (int)_rafManager.GetHash(projectileName);
            if (!string.IsNullOrEmpty(projectileName))
            {
                VisionRadius = SpellData.MissilePerceptionBubbleRadius;
            }
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
                    && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectFriends) > 0))
                    return;

                if (unit.Team == TeamId.TEAM_NEUTRAL
                    && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectNeutral) > 0))
                    return;

                if (unit.Team != Owner.Team
                    && unit.Team != TeamId.TEAM_NEUTRAL
                    && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectEnemies) > 0))
                    return;


                if (unit.IsDead && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectDead) > 0))
                    return;

                var m = unit as Minion;
                if (m != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectMinions) > 0))
                    return;

                var p = unit as Placeable;
                if (p != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectUseable) > 0))
                    return;

                var t = unit as BaseTurret;
                if (t != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectTurrets) > 0))
                    return;

                var i = unit as Inhibitor;
                var n = unit as Nexus;
                if ((i != null || n != null) && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectBuildings) > 0))
                    return;

                var c = unit as Champion;
                if (c != null && !((SpellData.Flags & (int)SpellFlag.SPELL_FLAG_AffectHeroes) > 0))
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

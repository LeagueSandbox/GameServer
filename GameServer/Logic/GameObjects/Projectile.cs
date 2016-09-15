using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.Enet;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Projectile : GameObject
    {
        public List<GameObject> ObjectsHit { get; private set; }
        public Unit Owner { get; private set; }
        public int ProjectileId { get; private set; }
        private float _moveSpeed;
        private int _flags;
        private Spell _originSpell;

        public Projectile(
            float x,
            float y,
            int collisionRadius,
            Unit owner,
            Target target,
            Spell originSpell,
            float moveSpeed,
            int projectileId,
            int flags = 0,
            uint netId = 0
        ) : base(x, y, collisionRadius, 0, netId)
        {
            this._originSpell = originSpell;
            this._moveSpeed = moveSpeed;
            this.Owner = owner;
            this.ProjectileId = projectileId;
            this._flags = flags;
            this.ObjectsHit = new List<GameObject>();

            Target = target;

            if (!target.isSimpleTarget())
                ((GameObject)target).incrementAttackerCount();

            owner.incrementAttackerCount();
        }

        public override void update(long diff)
        {
            if (Target == null)
            {
                setToRemove();
                return;
            }

            if (Target.isSimpleTarget())
            { // Skillshot
                var objects = _game.Map.GetObjects();
                foreach (var it in objects)
                {
                    if (isToRemove())
                        return;

                    if (Collide(it.Value))
                    {
                        if (ObjectsHit.Contains(it.Value))
                            continue;

                        var u = it.Value as Unit;
                        if (u == null)
                            continue;

                        if (u.Team == Owner.Team
                            && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectFriends) > 0))
                            continue;

                        if (u.Team == TeamId.TEAM_NEUTRAL
                            && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectNeutral) > 0))
                            continue;

                        if (u.Team != Owner.Team
                            && u.Team != TeamId.TEAM_NEUTRAL
                            && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectEnemies) > 0))
                            continue;


                        if (u.IsDead && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectDead) > 0))
                            continue;

                        var m = u as Minion;
                        if (m != null && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectMinions) > 0))
                            continue;

                        var p = u as Placeable;
                        if (p != null && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectUseable) > 0))
                            continue;

                        var t = u as LaneTurret;
                        if (t != null && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectTurrets) > 0))
                            continue;

                        var i = u as Inhibitor;
                        if (i != null && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectBuildings) > 0))
                            continue;

                        var n = u as Nexus;
                        if (n != null && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectBuildings) > 0))
                            continue;

                        var c = u as Champion;
                        if (c != null && !((_flags & (int)SpellFlag.SPELL_FLAG_AffectHeroes) > 0))
                            continue;

                        ObjectsHit.Add(u);
                        _originSpell.applyEffects(u, this);
                    }
                }
            }
            else
            {
                var u = Target as Unit;
                if (u != null && Collide(u))
                { // Autoguided spell
                    if (_originSpell != null)
                    {
                        _originSpell.applyEffects(u, this);
                    }
                    else
                    { // auto attack
                        Owner.autoAttackHit(u);
                        setToRemove();
                    }
                }
            }

            base.update(diff);
        }

        public override float getMoveSpeed()
        {
            return _moveSpeed;
        }

        public override void setToRemove()
        {
            if (Target != null && !Target.isSimpleTarget())
                (Target as GameObject).decrementAttackerCount();

            Owner.decrementAttackerCount();
            base.setToRemove();
            _game.PacketNotifier.notifyProjectileDestroy(this);
        }
    }
}

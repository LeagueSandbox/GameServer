using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.Enet;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Projectile : GameObject
    {
        protected List<GameObject> objectsHit = new List<GameObject>();
        protected Spell originSpell;
        protected Unit owner;
        protected float moveSpeed;
        protected int projectileId;
        protected int flags;

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
            this.originSpell = originSpell;
            this.moveSpeed = moveSpeed;
            this.owner = owner;
            this.projectileId = projectileId;
            this.flags = flags;

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
                var objects = _game.GetMap().GetObjects();
                foreach (var it in objects)
                {
                    if (isToRemove())
                        return;

                    if (collide(it.Value))
                    {
                        if (objectsHit.Contains(it.Value))
                            continue;

                        var u = it.Value as Unit;
                        if (u == null)
                            continue;

                        if (u.Team == owner.Team
                            && !((flags & (int)SpellFlag.SPELL_FLAG_AffectFriends) > 0))
                            continue;

                        if (u.Team == TeamId.TEAM_NEUTRAL
                            && !((flags & (int)SpellFlag.SPELL_FLAG_AffectNeutral) > 0))
                            continue;

                        if (u.Team != owner.Team
                            && u.Team != TeamId.TEAM_NEUTRAL
                            && !((flags & (int)SpellFlag.SPELL_FLAG_AffectEnemies) > 0))
                            continue;


                        if (u.isDead() && !((flags & (int)SpellFlag.SPELL_FLAG_AffectDead) > 0))
                            continue;

                        var m = u as Minion;
                        if (m != null && !((flags & (int)SpellFlag.SPELL_FLAG_AffectMinions) > 0))
                            continue;

                        var p = u as Placeable;
                        if (p != null && !((flags & (int)SpellFlag.SPELL_FLAG_AffectUseable) > 0))
                            continue;

                        var t = u as LaneTurret;
                        if (t != null && !((flags & (int)SpellFlag.SPELL_FLAG_AffectTurrets) > 0))
                            continue;

                        var i = u as Inhibitor;
                        if (i != null && !((flags & (int)SpellFlag.SPELL_FLAG_AffectBuildings) > 0))
                            continue;

                        var n = u as Nexus;
                        if (n != null && !((flags & (int)SpellFlag.SPELL_FLAG_AffectBuildings) > 0))
                            continue;

                        var c = u as Champion;
                        if (c != null && !((flags & (int)SpellFlag.SPELL_FLAG_AffectHeroes) > 0))
                            continue;

                        objectsHit.Add(u);
                        originSpell.applyEffects(u, this);
                    }
                }
            }
            else
            {
                var u = Target as Unit;
                if (u != null && collide(u))
                { // Autoguided spell
                    if (originSpell != null)
                    {
                        originSpell.applyEffects(u, this);
                    }
                    else
                    { // auto attack
                        owner.autoAttackHit(u);
                        setToRemove();
                    }
                }
            }

            base.update(diff);
        }

        public override float getMoveSpeed()
        {
            return moveSpeed;
        }

        public Unit getOwner()
        {
            return owner;
        }

        public List<GameObject> getObjectsHit()
        {
            return objectsHit;
        }

        public override void setToRemove()
        {
            if (Target != null && !Target.isSimpleTarget())
                (Target as GameObject).decrementAttackerCount();

            owner.decrementAttackerCount();
            base.setToRemove();
            _game.PacketNotifier.notifyProjectileDestroy(this);
        }

        public int getProjectileId()
        {
            return projectileId;
        }
    }
}

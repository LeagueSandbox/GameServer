using IntWarsSharp.Logic.Packets;
using IntWarsSharp.Logic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic.GameObjects
{
    public class Projectile : GameObject
    {

        protected List<GameObject> objectsHit = new List<GameObject>();
        protected Spell originSpell;
        protected Unit owner;
        protected float moveSpeed;
        protected int projectileId;
        protected int flags;


        public Projectile(Map map, int id, float x, float y, int collisionRadius, Unit owner, Target target, Spell originSpell, float moveSpeed, int projectileId, int flags = 0) : base(map, id, x, y, collisionRadius)
        {
            this.originSpell = originSpell;
            this.moveSpeed = moveSpeed;
            this.owner = owner;
            this.projectileId = projectileId;
            this.flags = flags;

            setTarget(target);

            if (!target.isSimpleTarget())
                ((GameObject)target).incrementAttackerCount();

            owner.incrementAttackerCount();
        }

        public override void update(long diff)
        {
            if (target == null)
            {
                setToRemove();
                return;
            }

            if (target.isSimpleTarget())
            { // Skillshot
                var objects = map.getObjects();
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

                        if (u.getTeam() == owner.getTeam() && !((flags & (int)SpellFlag.SPELL_FLAG_AffectFriends) > 0))
                            continue;

                        if (u.getTeam() != owner.getTeam() && !((flags & (int)SpellFlag.SPELL_FLAG_AffectEnemies) > 0))
                            continue;


                        if (u.isDead() && !((flags & (int)SpellFlag.SPELL_FLAG_AffectDead) > 0))
                            continue;

                        var m = u as Minion;
                        if (m != null && !((flags & (int)SpellFlag.SPELL_FLAG_AffectMinions) > 0))
                            continue;

                        var t = u as Turret;
                        if (t != null && !((flags & (int)SpellFlag.SPELL_FLAG_AffectTurrets) > 0))
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
                var u = target as Unit;
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
            if (target != null && !target.isSimpleTarget())
                (target as GameObject).decrementAttackerCount();

            owner.decrementAttackerCount();
            base.setToRemove();
            PacketNotifier.notifyProjectileDestroy(this);
        }

        public int getProjectileId()
        {
            return projectileId;
        }
    }
}

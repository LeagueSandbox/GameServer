using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Laser : GameObject
    {
        public List<GameObject> ObjectsHit { get; private set; }
        public Unit Owner { get; private set; }
        private int _flags;
        private Spell _originSpell;
        protected Logger _logger = Program.ResolveDependency<Logger>();
        protected PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();
        private List<PieceOfLaser> _piecesOfLaser = new List<PieceOfLaser>();

        public Laser(
            float x,
            float y,
            int collisionRadius,
            Unit owner,
            Target target,
            Spell originSpell,
            int flags = 0,
            uint netId = 0) : base(x, y, collisionRadius, 0, netId)
        {
            Owner = owner;
            Target = target;
            _originSpell = originSpell;
            _flags = flags;
            ObjectsHit = new List<GameObject>();

            if (!target.IsSimpleTarget)
            {
                ((GameObject)target).incrementAttackerCount();
            }

            owner.incrementAttackerCount();

            while (GetDistanceTo(Target) > CollisionRadius)
            {
                var to = new Vector2(Target.X, Target.Y);
                var cur = new Vector2(X, Y); //?
                var goingTo = to - cur;
                _direction = Vector2.Normalize(goingTo);
                if (float.IsNaN(_direction.X) || float.IsNaN(_direction.Y))
                {
                    _direction = new Vector2(0, 0);
                }
                X += _direction.X * CollisionRadius;
                Y += _direction.Y * CollisionRadius;
                _piecesOfLaser.Add(new PieceOfLaser(X, Y));
            }

            foreach (var piece in _piecesOfLaser)
            {
                var units = _game.Map.GetObjects().Values;
                foreach (var unit in units)
                {
                    CheckFlagsForUnit(unit as Unit);
                }
            }
			
            setToRemove();
        }

        public override float getMoveSpeed()
        {
            return float.MaxValue;
        }

        protected void CheckFlagsForUnit(Unit unit)
        {
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
                _originSpell.applyEffects(unit, null, this);
            }
            else
            {
                var u = Target as Unit;
                if (u != null && Collide(u))
                { // Autoguided spell
                    if (_originSpell != null)
                    {
                        _originSpell.applyEffects(u, null, this);
                    }
                    else
                    { // auto attack
                        Owner.autoAttackHit(u);
                        setToRemove();
                    }
                }
            }
        }

        private class PieceOfLaser
        {
            public float X;
            public float Y;

            internal PieceOfLaser(float x, float y)
            {
                X = x;
                Y = y;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;

namespace LeagueSandbox.GameServer.GameObjects.Spell.Missile
{
    public class SpellChainMissile : SpellMissile
    {
        public override MissileType Type { get; protected set; } = MissileType.Chained;

        public SpellChainMissile(
            Game game,
            int collisionRadius,
            ISpell originSpell,
            ICastInfo castInfo,
            float moveSpeed,
            SpellDataFlags overrideFlags = 0, // TODO: Find a use for these
            uint netId = 0,
            bool serverOnly = false
        ) : base(game, collisionRadius, originSpell, castInfo, moveSpeed)
        {
            // TODO: Implemented full support for multiple targets.
            if (!castInfo.Targets.Exists(t =>
            {
                if (t.Unit != null)
                {
                    TargetUnit = t.Unit;
                    return true;
                }
                return false;
            }))
            {
                Position = new Vector2(castInfo.SpellCastLaunchPosition.X, castInfo.SpellCastLaunchPosition.Z);

                var goingTo = new Vector2(castInfo.TargetPositionEnd.X, castInfo.TargetPositionEnd.Z) - Position;
                var dirTemp = Vector2.Normalize(goingTo);
                var endPos = Position + (dirTemp * SpellOrigin.SpellData.CastRangeDisplayOverride);

                // usually doesn't happen
                if (float.IsNaN(dirTemp.X) || float.IsNaN(dirTemp.Y))
                {
                    if (float.IsNaN(CastInfo.Owner.Direction.X) || float.IsNaN(CastInfo.Owner.Direction.Y))
                    {
                        dirTemp = new Vector2(1, 0);
                    }
                    else
                    {
                        dirTemp = new Vector2(CastInfo.Owner.Direction.X, CastInfo.Owner.Direction.Z);
                    }

                    endPos = Position + (dirTemp * SpellOrigin.SpellData.CastRangeDisplayOverride);
                    CastInfo.TargetPositionEnd = new Vector3(endPos.X, 0, endPos.Y);
                }
            }
        }

        public override void Update(float diff)
        {
            if (!HasTarget())
            {
                SetToRemove();
                return;
            }

            base.Update(diff);
        }

        public override void OnCollision(IGameObject collider, bool isTerrain = false)
        {
            if (IsToRemove() || (TargetUnit != null && collider != TargetUnit))
            {
                return;
            }

            if (isTerrain)
            {
                // TODO: Implement methods for isTerrain for projectiles such as Nautilus Q, ShyvanaDragon Q, or Ziggs Q.
                return;
            }
        }

        /// <summary>
        /// Whether or not this projectile has a target unit or a destination; if it is a valid projectile.
        /// </summary>
        /// <returns>True/False.</returns>
        public bool HasTarget()
        {
            return TargetUnit != null;
        }
    }
}

using System;
using System.Numerics;
using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    public class Target : ITarget
    {
        public Vector2 Position { get; protected set; }

        public virtual bool IsSimpleTarget => true;

        public Target(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public Target(Vector2 position)
        {
            Position = position;
        }

        public float GetDistanceTo(Target target)
        {
            return GetDistanceTo(target.Position);
        }

        public float GetDistanceTo(Vector2 position)
        {
            return (Position - position).Length();
        }

        public float GetDistanceToSqr(Target target)
        {
            return GetDistanceToSqr(target.Position);
        }

        public float GetDistanceToSqr(Vector2 position)
        {
            return (Position - position).LengthSquared();
        }
    }
}

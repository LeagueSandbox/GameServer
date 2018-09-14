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
            return GetDistanceTo(position.X, position.Y);
        }

        public float GetDistanceTo(float x, float y)
        {
            return (float)Math.Sqrt(GetDistanceToSqr(x, y));
        }

        public float GetDistanceToSqr(Target target)
        {
            return GetDistanceToSqr(target.Position);
        }

        public float GetDistanceToSqr(Vector2 position)
        {
            return GetDistanceToSqr(position.X, position.Y);
        }

        public float GetDistanceToSqr(float x, float y)
        {
            return (Position.X - x) * (Position.X - x) + (Position.Y - y) * (Position.Y - y);
        }
    }
}

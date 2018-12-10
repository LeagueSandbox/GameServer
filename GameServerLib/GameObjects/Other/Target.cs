using System;
using System.Numerics;
using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    public class Target : ITarget
    {
        public float X { get; protected set; }
        public float Y { get; protected set; }

        public virtual bool IsSimpleTarget => true;

        public Target(float x, float y)
        {
            X = x;
            Y = y;
        }

        public Target(Vector2 vec)
        {
            X = vec.X;
            Y = vec.Y;
        }

        public float GetDistanceTo(ITarget target)
        {
            return GetDistanceTo(target.X, target.Y);
        }
        
        public float GetDistanceTo(float xtarget, float ytarget)
        {
            return (float)Math.Sqrt(GetDistanceToSqr(xtarget, ytarget));
        }

        public float GetDistanceToSqr(ITarget target)
        {
            return GetDistanceToSqr(target.X, target.Y);
        }

        public float GetDistanceToSqr(float xtarget, float ytarget)
        {
            return (X - xtarget) * (X - xtarget) + (Y - ytarget) * (Y - ytarget);
        }

        public Vector2 GetPosition()
        {
            return new Vector2(X, Y);
        }

        public bool WithinRange(Vector2 from, Vector2 to, float range)
        {
            float v1 = from.X - to.X, v2 = from.Y - to.Y;
            return ((v1 * v1) + (v2 * v2)) <= (range * range);
        }
    }
}

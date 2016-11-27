using System;
using System.Numerics;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Target
    {
        public float X { get; protected set; }
        public float Y { get; protected set; }

        public virtual bool IsSimpleTarget { get { return true; } }

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

        public float GetDistanceTo(Target target)
        {
            return GetDistanceTo(target.X, target.Y);
        }

        public float GetDistanceTo(float xtarget, float ytarget)
        {
            return (float)Math.Sqrt(GetDistanceToSqr(xtarget, ytarget));
        }

        public float GetDistanceToSqr(Target target)
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
    }
}

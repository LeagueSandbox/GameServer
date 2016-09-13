using System;
using System.Numerics;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Target
    {
        public float X { get; protected set; }
        public float Y { get; protected set; }

        public Target(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public Target(Vector2 vec)
        {
            this.X = vec.X;
            this.Y = vec.Y;
        }

        public float distanceWith(Target target)
        {
            return distanceWith(target.X, target.Y);
        }
        public float distanceWith(float xtarget, float ytarget)
        {
            return (float)Math.Sqrt((X - xtarget) * (X - xtarget) + (Y - ytarget) * (Y - ytarget));
        }

        public float distanceWithSqr(Target target)
        {
            return distanceWithSqr(target.X, target.Y);
        }

        public float distanceWithSqr(float xtarget, float ytarget)
        {
            return ((X - xtarget) * (X - xtarget) + (Y - ytarget) * (Y - ytarget));
        }

        public virtual void setPosition(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
        public Vector2 getPosition()
        {
            return new Vector2(X, Y);
        }

        public virtual bool isSimpleTarget()
        {
            return true;
        }
    }
}

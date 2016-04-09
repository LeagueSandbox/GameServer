using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Target
    {
        protected float x;
        protected float y;

        public Target(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Target(Vector2 vec)
        {
            this.x = vec.X;
            this.y = vec.Y;
        }

        public float distanceWith(Target target)
        {
            return distanceWith(target.getX(), target.getY());
        }
        public float distanceWith(float xtarget, float ytarget)
        {
            return (float)Math.Sqrt((x - xtarget) * (x - xtarget) + (y - ytarget) * (y - ytarget));
        }

        public float distanceWithSqr(Target target)
        {
            return distanceWithSqr(target.getX(), target.getY());
        }

        public float distanceWithSqr(float xtarget, float ytarget)
        {
            return ((x - xtarget) * (x - xtarget) + (y - ytarget) * (y - ytarget));
        }

        public float getX()
        {
            return x;
        }
        public float getY()
        {
            return y;
        }

        public virtual void setPosition(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public Vector2 getPosition()
        {
            return new Vector2(x, y);
        }

        public virtual bool isSimpleTarget()
        {
            return true;
        }
    }
}

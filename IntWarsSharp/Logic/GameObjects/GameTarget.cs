using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic
{
    class GameTarget
    {
        internal float x;
        internal float y;

        public GameTarget(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public GameTarget(Vector2 vec)
        {
            x = vec.X;
            y = vec.Y;
        }

        public float distanceWith(GameTarget target)
        {
            return distanceWith(target.getX(), target.getY());
        }

        public float distanceWith(float xtarget, float ytarget)
        {
            return (float)Math.Sqrt((x - xtarget) * (x - xtarget) + (y - ytarget) * (y - ytarget));
        }

        public float distanceWithSqr(GameTarget target)
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

        public  Vector2 getPosition()
        {
            return new Vector2(x, y);
        }

        public virtual bool isSimpleTarget()
        {
            return true;
        }
    }
}

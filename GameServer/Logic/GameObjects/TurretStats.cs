using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    class TurretStats : Stats
    {
        protected float range;

        public override void setAttackSpeedMultiplier(float multiplier)
        {
        }

        public override float getAttackSpeedMultiplier()
        {
            return 1.0f;
        }

        public override void setRange(float range)
        {
            this.range = range;
        }

        public override float getRange()
        {
            return range;
        }
    }
}

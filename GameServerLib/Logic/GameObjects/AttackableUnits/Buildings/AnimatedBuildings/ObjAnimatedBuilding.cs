using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class ObjAnimatedBuilding : ObjBuilding
    {
        public ObjAnimatedBuilding(string model, int collisionRadius = 40, 
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0) : 
            base(model, collisionRadius, x, y, visionRadius, netId)
        {
        }
    }
}

using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class LevelProp : GameObject
    {
        private string name;
        private string type;
        private float z, dirX, dirY, dirZ, unk1, unk2;

        public LevelProp(Game game, uint id, float x, float y, float z, float dirX, float dirY, float dirZ, float unk1, float unk2, string name, string type) : base(game, id, x, y, 0)
        {
            this.z = z;
            this.dirX = dirX;
            this.dirY = dirY;
            this.dirZ = dirZ;
            this.unk1 = unk1;
            this.unk2 = unk2;
            this.name = name;
            this.type = type;
        }

        public override float GetZ()
        {
            return z;
        }

        public float getDirectionX()
        {
            return dirX;
        }

        public float getDirectionY()
        {
            return dirY;
        }

        public float getDirectionZ()
        {
            return dirZ;
        }

        public float getUnk1()
        {
            return unk1;
        }

        public float getUnk2()
        {
            return unk2;
        }

        public override float getMoveSpeed()
        {
            return 0.0f;
        }

        public string getName()
        {
            return name;
        }
        public string getType()
        {
            return type;
        }

    }
}

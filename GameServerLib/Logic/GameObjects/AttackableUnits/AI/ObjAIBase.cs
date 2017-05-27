using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class ObjAIBase : AttackableUnit
    {
        public ObjAIBase(string model, Stats stats, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0) :
            base(model, stats, collisionRadius, x, y, visionRadius, netId)
        {
            if(!string.IsNullOrEmpty(model))
            {
                AASpellData = _game.Config.ContentManager.GetSpellData(model + "BasicAttack");
                AutoAttackDelay = AASpellData.CastFrame / 30.0f;
                AutoAttackProjectileSpeed = AASpellData.MissileSpeed;
            }
        }
    }
}

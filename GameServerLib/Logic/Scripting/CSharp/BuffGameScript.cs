using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Scripting
{
    public interface IBuffGameScript
    {
        void OnUpdate(double diff);

        void OnActivate(ObjAiBase unit, Spell ownerSpell);

        void OnDeactivate(ObjAiBase unit);
    }
}

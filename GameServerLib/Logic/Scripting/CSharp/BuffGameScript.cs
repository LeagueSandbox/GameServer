using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Scripting
{
    public interface BuffGameScript
    {
        void OnUpdate(double diff);

        void OnActivate(Unit unit, Spell ownerSpell);

        void OnDeactivate(Unit unit);
    }
}

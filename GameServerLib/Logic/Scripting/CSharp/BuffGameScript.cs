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
        void OnActivate(Champion owner, Spell ownerSpell);

        void OnDeactivate(Champion owner);

        void OnUpdate(double diff);
    }
}

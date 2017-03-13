using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    public interface GameScript
    {
        void OnActivate(Champion owner);

        void OnDeactivate(Champion owner);

        void OnStartCasting(Champion owner, Spell spell, Unit target);

        void OnFinishCasting(Champion owner, Spell spell, Unit target);

        void ApplyEffects(Champion owner, Unit target, Spell spell, Projectile projectile);
    }
}

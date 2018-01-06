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

        void OnStartCasting(Champion owner, Spell spell, ObjAIBase target);

        void OnFinishCasting(Champion owner, Spell spell, ObjAIBase target);

        void ApplyEffects(Champion owner, ObjAIBase target, Spell spell, Projectile projectile);
    }
}

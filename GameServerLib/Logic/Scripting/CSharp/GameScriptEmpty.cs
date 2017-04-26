using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.GameObjects;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    public class GameScriptEmpty : GameScript
    {
        public void OnActivate(Champion owner)
        {
        }

        public void OnDeactivate(Champion owner)
        {
        }

        public void OnStartCasting(Champion owner, Spell spell, Unit target)
        {
        }

        public void OnFinishCasting(Champion owner, Spell spell, Unit target)
        {
        }

        public void ApplyEffects(Champion owner, Unit target, Spell spell, Projectile projectile)
        {
        }
    }
}

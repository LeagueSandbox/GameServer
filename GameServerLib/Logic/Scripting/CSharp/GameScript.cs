using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    public interface GameScript
    {
        void OnActivate(Champion owner);

        void OnDeactivate(Champion owner);

        void OnStartCasting(Champion owner, Spell spell, AttackableUnit target);

        void OnFinishCasting(Champion owner, Spell spell, AttackableUnit target);

        void ApplyEffects(Champion owner, AttackableUnit target, Spell spell, Projectile projectile);
    }
}

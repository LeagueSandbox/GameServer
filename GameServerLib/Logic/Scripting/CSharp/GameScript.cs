using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.Missiles;
using LeagueSandbox.GameServer.Logic.GameObjects.Spells;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    public interface IGameScript
    {
        void OnActivate(Champion owner);

        void OnDeactivate(Champion owner);

        void OnStartCasting(Champion owner, Spell spell, AttackableUnit target);

        void OnFinishCasting(Champion owner, Spell spell, AttackableUnit target);

        void ApplyEffects(Champion owner, AttackableUnit target, Spell spell, Projectile projectile);
    }
}

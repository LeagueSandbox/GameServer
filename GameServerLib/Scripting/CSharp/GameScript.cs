using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Missiles;
using LeagueSandbox.GameServer.GameObjects.Spells;

namespace LeagueSandbox.GameServer.Scripting.CSharp
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

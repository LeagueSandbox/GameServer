using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public interface IGameScript
    {
        void OnActivate(IObjAiBase owner, ISpell spell);

        void OnDeactivate(IObjAiBase owner, ISpell spell);

        void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target);

        void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target);

        void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile);

        void OnUpdate(float diff);
    }
}

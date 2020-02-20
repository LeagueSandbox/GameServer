using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public interface IGameScript
    {
        void OnActivate(IObjAiBase owner);

        void OnDeactivate(IObjAiBase owner);

        void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target);

        void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target);

        void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile);
    }
}

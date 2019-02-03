using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public interface IGameScript
    {
        void OnActivate(IChampion owner);

        void OnDeactivate(IChampion owner);

        void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target);

        void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target);

        void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile);
    }
}

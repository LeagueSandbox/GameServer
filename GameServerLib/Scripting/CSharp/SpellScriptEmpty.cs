using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    class SpellScriptEmpty : ISpellScript
    {
        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void CooldownStarted(IChampion owner, ISpell spell)
        {

        }

        public void CooldownEnded(IChampion owner, ISpell spell)
        {

        }
    }
}

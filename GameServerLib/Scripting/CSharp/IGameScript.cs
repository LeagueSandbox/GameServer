using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public interface IGameScript
    {
        void OnActivate(IObjAiBase owner);

        void OnDeactivate(IObjAiBase owner);

        void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target);

        void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target);

        void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile);
    }
}

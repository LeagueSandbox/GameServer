using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class RegenerationPotion : IGameScript
    {
        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            AddBuff("RegenerationPotion", 15.0f, 1, spell, owner, owner);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }

        public void OnActivate(IObjAiBase owner)
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }
    }
}

using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace NidaleeEBuff
{
    class NidaleeEBuff : IBuffGameScript
    {

        private IBuff _visualBuff;
        private Particle _createdParticle;
        private StatsModifier _statsModifier;

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            _visualBuff = AddBuffHudVisual("PrimalSurge", 7f, 1, BuffType.COMBAT_ENCHANCER, unit);
            _createdParticle = AddParticleTarget(ownerSpell.Owner, "Nidalee_Base_E_buf.troy", unit);

            _statsModifier = new StatsModifier();
            _statsModifier.AttackSpeed.PercentBonus = _statsModifier.AttackSpeed.PercentBonus + (0.1f + (0.1f * ownerSpell.Level));

            unit.AddStatModifier(_statsModifier);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            RemoveBuffHudVisual(_visualBuff);
            RemoveParticle(_createdParticle);

            unit.RemoveStatModifier(_statsModifier);
        }

        public void OnUpdate(double diff)
        {
            
        }
    }
}

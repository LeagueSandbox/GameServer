using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Recall
{
    class Recall : IBuffGameScript
    {
        private IBuff _visualBuff;
        private Particle _createdParticle;

        private bool _canRecall;

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            IChampion champion = unit as IChampion;

            _visualBuff = AddBuffHudVisual("Recall", 8.0f, 1, BuffType.COMBAT_ENCHANCER, champion);
            _createdParticle = AddParticleTarget(champion, "TeleportHome.troy", champion);

            // @TODO Change to a less hacky way of implementing recall checking
            CreateTimer(7.9f, () => 
            {
                _canRecall = true;
            });
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            RemoveBuffHudVisual(_visualBuff);
            RemoveParticle(_createdParticle);

            if (_canRecall)
            {
                ((IChampion)unit).Recall();
            }
        }

        public void OnUpdate(double diff)
        {

        }
    }
}

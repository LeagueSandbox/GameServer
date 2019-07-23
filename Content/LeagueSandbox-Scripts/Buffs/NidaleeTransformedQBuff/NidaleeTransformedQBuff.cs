using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace NidaleeTransformedQBuff
{
    class NidaleeTransformedQBuff : IBuffGameScript
    {
        private IBuff _visualBuff;
        private Particle _spawnedParticle;

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            unit.Stats.Range.FlatBonus += 75;
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            unit.Stats.Range.FlatBonus -=  75;
        }

        public void OnUpdate(double diff)
        {
            
        }
    }
}

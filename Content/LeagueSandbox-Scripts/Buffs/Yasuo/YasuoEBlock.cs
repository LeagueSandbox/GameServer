using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace YasuoEBlock
{
    internal class YasuoEBlock : IBuffGameScript
    {
        private readonly IChampion owner = Spells.YasuoDashWrapper._owner;
        private IBuff _visualBuff;

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            var time = 11f - ownerSpell.Level * 1f;
            _visualBuff = AddBuffHudVisual("YasuoDashScalar", time, 1, BuffType.COMBAT_DEHANCER, unit);
            AddParticleTarget(owner, "Yasuo_base_E_timer1.troy", unit);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            RemoveBuffHudVisual(_visualBuff);
        }

        public void OnUpdate(double diff)
        {
            //empty!
        }
    }
}

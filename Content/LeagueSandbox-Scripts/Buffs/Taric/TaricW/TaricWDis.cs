using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace TaricWDis
{
    internal class TaricWDis : IBuffGameScript
    {
        private IBuff _visualBuff;        
        public void OnActivate(IObjAiBase unit,ISpell spell)
        {
            _visualBuff = AddBuffHudVisual("Shatter", 4f, 1, BuffType.COMBAT_DEHANCER, unit);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            RemoveBuffHudVisual(_visualBuff);
        }

        public void OnUpdate(double diff)
        {
            
        }
    }
}

using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace YasuoQ01
{
    internal class YasuoQ01 : IBuffGameScript
    {
        private IBuff _visualBuff;

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            ((IChampion)unit).SetSpell("YasuoQ2W", 0, true);
            _visualBuff = AddBuffHudVisual("YasuoQ", 6f, 1, BuffType.COMBAT_ENCHANCER, unit);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            if (((IChampion)unit).Spells[0].SpellName == "YasuoQ2W")
            {
                ((IChampion)unit).SetSpell("YasuoQW", 0, true);
            }
            RemoveBuffHudVisual(_visualBuff);
        }

        public void OnUpdate(double diff)
        {
            //nothing!
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace EzrealWBuff
{
    class EzrealWBuff : IBuffGameScript
    {
        private StatsModifier _statMod;
        private IBuff _visualBuff;

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            _statMod = new StatsModifier();

            _statMod.AttackSpeed.PercentBonus = _statMod.AttackSpeed.PercentBonus + (0.15f + 0.05f * ownerSpell.Level);

            unit.AddStatModifier(_statMod);

            _visualBuff = AddBuffHudVisual("EzrealEssenceFluxBuff", 5f, 1, BuffType.COMBAT_ENCHANCER, unit);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            RemoveBuffHudVisual(_visualBuff);

            unit.RemoveStatModifier(_statMod);
        }

        public void OnUpdate(double diff)
        {
            
        }
    }
}

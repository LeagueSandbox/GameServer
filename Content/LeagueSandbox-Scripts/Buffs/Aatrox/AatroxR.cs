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

namespace AatroxR
{
    class AatroxR : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_ENCHANCER;
        public BuffAddType BuffAddType => BuffAddType.REPLACE_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        string pmodelname;
        IParticle pmodel;

        public void OnActivate(IObjAiBase unit, IBuff buff, ISpell ownerSpell)
        {
            if (unit is IChampion c)
            {
                // TODO: Implement Animation Overrides for spells like these
                if (c.Skin == 0)
                {
                    pmodelname = "Aatrox_Base_RModel.troy";
                }
                else if (c.Skin == 1)
                {
                    pmodelname = "Aatrox_Skin01_RModel.troy";
                }
                else if (c.Skin == 2)
                {
                    pmodelname = "Aatrox_Skin02_RModel.troy";
                }
                pmodel = AddParticleTarget(c, pmodelname, c);
                pmodel.SetToRemove();

                StatsModifier.AttackSpeed.PercentBonus = (0.4f + (0.1f * (ownerSpell.Level - 1))) * buff.StackCount; // StackCount included here as an example
                StatsModifier.Range.FlatBonus = 175f * buff.StackCount;

                unit.AddStatModifier(StatsModifier);
            }
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            RemoveParticle(pmodel);
            unit.RemoveStatModifier(StatsModifier);
        }

        public void OnUpdate(double diff)
        {

        }
    }
}

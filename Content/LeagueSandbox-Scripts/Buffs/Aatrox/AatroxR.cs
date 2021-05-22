using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
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

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
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
                pmodel = AddParticleTarget(c, c, pmodelname, c);
                pmodel.SetToRemove();

                StatsModifier.AttackSpeed.PercentBonus = (0.4f + (0.1f * (ownerSpell.CastInfo.SpellLevel - 1))) * buff.StackCount; // StackCount included here as an example
                StatsModifier.Range.FlatBonus = 175f * buff.StackCount;

                unit.AddStatModifier(StatsModifier);
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(pmodel);
        }

        public void OnUpdate(float diff)
        {

        }
    }
}

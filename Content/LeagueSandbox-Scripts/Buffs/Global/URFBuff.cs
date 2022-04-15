using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    internal class InternalTestBuff : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.AURA,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell = null)
        {
            //TODO: Set up NotifyS2C_HandleTipUpdate in order to update the stats in the buff's tooltip
            if (unit is IChampion champion)
            {
                StatsModifier.CooldownReduction.FlatBonus += 0.8f;
                StatsModifier.Tenacity.FlatBonus += 0.25f;
                StatsModifier.MoveSpeed.FlatBonus += 60;
                //TODO: Add +35% Resistance Against Dinosaurs
                if (!champion.IsMelee)
                {
                    StatsModifier.CriticalDamage.FlatBonus += 0.25f;
                    //TODO: Add +100% attack speed multiplier here
                }

                unit.AddStatModifier(StatsModifier);

                // Mana/Energy cost reduction, CDR, Tenacity, Movespeed, Dinosaur Resist 
                SetBuffToolTipVar(buff, 0, 100);
                SetBuffToolTipVar(buff, 1, StatsModifier.CooldownReduction.FlatBonus * 100);
                SetBuffToolTipVar(buff, 2, StatsModifier.Tenacity.FlatBonus * 100);
                SetBuffToolTipVar(buff, 3, StatsModifier.MoveSpeed.FlatBonus);
                SetBuffToolTipVar(buff, 4, 35);
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }

        public void OnUpdate(float diff)
        {

        }
    }
}

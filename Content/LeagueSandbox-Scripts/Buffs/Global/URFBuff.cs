using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Buffs
{
    internal class InternalTestBuff : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.AURA,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell = null)
        {
            //TODO: Set up NotifyS2C_HandleTipUpdate in order to update the stats in the buff's tooltip
            if (unit is Champion champion)
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

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
        }

        public void OnUpdate(float diff)
        {

        }
    }
}

using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class MordekaiserCOTGPetBuff : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            //Stats applied here might not be notified to the clients, even though all necessary packets are sent, i was unsuccessful on pinpointing the cause (cabeca143).
            if (unit is Pet pet)
            {
                StatsModifier.AbilityPower.FlatBonus = pet.ClonedUnit.Stats.AbilityPower.Total + pet.Owner.Stats.AbilityPower.Total * 0.75f;
                StatsModifier.AttackDamage.FlatBonus = pet.Owner.Stats.AttackDamage.Total * 0.75f;
                StatsModifier.HealthPoints.FlatBonus = pet.Owner.Stats.HealthPoints.Total * 0.15f;

                while (pet.Stats.Level < pet.ClonedUnit.Stats.Level)
                {
                    pet.LevelUp();
                }
                pet.Stats.AttackDamage.BaseValue = pet.ClonedUnit.CharData.BaseDamage;
                
                pet.AddStatModifier(StatsModifier);
                pet.Stats.CurrentHealth = pet.Stats.HealthPoints.Total;
            }
        }
    }
}

using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiEventManager;
using GameServerCore.Domain;

namespace Buffs
{
    internal class MordekaiserCOTGPetBuff : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            //Stats applied here might not be notified to the clients, even though all necessary packets are sent, i was unsuccessful on pinpointing the cause (cabeca143).
            if (unit is IPet pet)
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

using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace Buffs
{
    internal class InfernalGuardianBurning : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IPet Pet;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (unit is IPet pet)
            {
                Pet = pet;

                StatsModifier.Armor.FlatBonus = 20.0f * (ownerSpell.CastInfo.SpellLevel - 1);
                StatsModifier.MagicResist.FlatBonus = 20.0f * (ownerSpell.CastInfo.SpellLevel - 1);
                StatsModifier.AttackDamage.FlatBonus = 25.0f * (ownerSpell.CastInfo.SpellLevel - 1);
                StatsModifier.HealthPoints.FlatBonus = 900.0f * (ownerSpell.CastInfo.SpellLevel - 1);
                pet.AddStatModifier(StatsModifier);
                pet.Stats.CurrentHealth = pet.Stats.HealthPoints.Total;
                ExecuteTick();
            }
        }
        public void ExecuteTick()
        {
            var totalDamage = 35.0f + Pet.Owner.Stats.AbilityPower.Total * 0.20f;

            var targets = GetUnitsInRange(Pet.Position, 250.0f, true);
            targets.RemoveAll(x => x.Team == Pet.Team || x is IObjBuilding);
            foreach (var target in targets)
            {
                target.TakeDamage(Pet.Owner, totalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }

        float timer = 1000.0f;
        public void OnUpdate(float diff)
        {
            if (Pet != null)
            {
                timer -= diff;
                if (timer <= 0.0f)
                {
                    ExecuteTick();
                    timer = 1000.0f;
                }
            }
        }
    }
}
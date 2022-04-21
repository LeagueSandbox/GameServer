using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Domain.GameObjects.Spell.Missile;

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
        ISpellSector burnSector;
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

                burnSector = ownerSpell.CreateSpellSector(new SectorParameters
                {
                    BindObject = pet,
                    Length = 350.0f,
                    Lifetime = buff.Duration,
                    Tickrate = 1,
                    CanHitSameTarget = true,
                    CanHitSameTargetConsecutively = true,
                    MaximumHits = 0,
                    OverrideFlags = SpellDataFlags.AffectEnemies | SpellDataFlags.AffectNeutral | SpellDataFlags.AffectMinions | SpellDataFlags.AffectHeroes,
                    Type = SectorType.Area
                });

                ApiEventManager.OnSpellSectorHit.AddListener(this, burnSector, TargetExecute, false);
            }
        }
        public void TargetExecute(IAttackableUnit target, ISpellSector sector)
        {
            if (Pet != null && sector.Parameters.BindObject != null)
            {
                var totalDamage = 35.0f + Pet.Owner.Stats.AbilityPower.Total * 0.20f;
                target.TakeDamage(Pet.Owner, totalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            unit.Die(CreateDeathData(false, 0, unit, unit, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, 0.0f));
            RemoveBuff(buff.SourceUnit, "InfernalGuardianTimer");

            if (burnSector != null)
            {
                burnSector.SetToRemove();
            }
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Buffs
{
    internal class InfernalGuardianBurning : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Pet Pet;
        SpellSector burnSector;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            if (unit is Pet pet)
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
        public void TargetExecute(SpellSector sector, AttackableUnit target)
        {
            if (Pet != null && sector.Parameters.BindObject != null)
            {
                var totalDamage = 35.0f + Pet.Owner.Stats.AbilityPower.Total * 0.20f;
                target.TakeDamage(Pet.Owner, totalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
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
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;

namespace Spells
{
    public class InfernalGuardian : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true,
            IsPetDurationBuff = true,
            NotSingleTargetSpell = true,
            SpellDamageRatio = 0.5f,
        };

        public void OnSpellPostCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner as Champion;
            var tibbers = CreatePet
            (
                owner,
                spell,
                new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z),
                "Tibbers",
                "AnnieTibbers",
                "InfernalGuardian",
                45.0f,
                showMinimapIfClone: false,
                isClone: false
            );
            var guideSpell = SetSpell(owner, "InfernalGuardianGuide", SpellSlotType.SpellSlots, 3);

            AddBuff("InfernalGuardianBurning", 45.0f, 1, spell, tibbers, owner);
            AddBuff("InfernalGuardianTimer", 45.0f, 1, spell, owner, owner);

            // Pyromania stuff here

            string particles;
            switch (owner.SkinID)
            {
                case 1:
                    particles = "Annie_skin02_R_cas";
                    break;
                case 4:
                    particles = "Annie_skin05_R_cas";
                    break;
                case 8:
                    particles = "Annie_skin09_R_cas";
                    break;
                default:
                    particles = "Annie_R_cas";
                    break;
            }
            AddParticle(owner, null, particles, tibbers.Position);

            var sector = spell.CreateSpellSector(new SectorParameters
            {
                Length = spell.SpellData.CastRadius[0],
                SingleTick = true,
                Type = SectorType.Area
            });

            ApiEventManager.OnSpellSectorHit.AddListener(this, sector, TargetExecute, false);
        }

        public void TargetExecute(SpellSector sector, AttackableUnit target)
        {
            var spell = sector.SpellOrigin;

            // Pyromania stun here

            var Ap = spell.CastInfo.Owner.Stats.AbilityPower.Total * 0.8f;
            var totalDamage = 50 + 125 * spell.CastInfo.SpellLevel + Ap;
            target.TakeDamage(spell.CastInfo.Owner, totalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);
        }
    }

    public class InfernalGuardianGuide : BasePetController
    {
    }
}

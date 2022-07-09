using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class InfernalGuardian : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true,
            IsDamagingSpell = true,
            IsPetDurationBuff = true,
            NotSingleTargetSpell = true,
            SpellDamageRatio = 0.5f,
        };

        public void OnActivate(IObjAIBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAIBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAIBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner as IChampion;
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

        public void TargetExecute(ISpellSector sector, IAttackableUnit target)
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

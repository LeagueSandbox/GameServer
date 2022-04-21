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

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
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
                owner.SkinID,
                showMinimapIfClone: false,
                isClone: false
            );

            var guideSpell = SetSpell(owner, "InfernalGuardianGuide", SpellSlotType.SpellSlots, 3);
            (guideSpell.Script as InfernalGuardianGuide).Tibbers = tibbers;

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

        public void TargetExecute(IAttackableUnit target, ISpellSector sector)
        {
            var spell = sector.SpellOrigin;

            // Pyromania stun here

            var Ap = spell.CastInfo.Owner.Stats.AbilityPower.Total * 0.8f;
            var totalDamage = 50 + 125 * spell.CastInfo.SpellLevel + Ap;
            target.TakeDamage(spell.CastInfo.Owner, totalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);
        }

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }

    public class InfernalGuardianGuide : ISpellScript
    {
        public IPet Tibbers;
        public ISpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            NotSingleTargetSpell = true,
            DoesntBreakShields = true,
            TriggersSpellCasts = false,
            IsDamagingSpell = true,
            SpellDamageRatio = 0.5f,
            IsPetDurationBuff = true
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            if (Tibbers != null)
            {
                // likely AddBuff("PetCommandParticle") here (refer to preload for particles)
                AddParticle(owner, null, "cursor_moveto", start);

                //TODO: Instead of baking AI here, make a general Pet AI script and set it as the default AI for Pet class.
                var unitsInRage = GetUnitsInRange(end, 100.0f, true);
                unitsInRage.RemoveAll(x => x.Team == spell.CastInfo.Owner.Team);
                if (unitsInRage.Count > 0)
                {
                    Tibbers.UpdateMoveOrder(OrderType.PetHardAttack);
                    Tibbers.SetTargetUnit(unitsInRage[0]);
                    for(int i = 0; i < unitsInRage.Count; i++)
                    {
                        spell.CastInfo.SetTarget(unitsInRage[i], i);
                    }
                }
                else
                {
                    Tibbers.SetTargetUnit(null, true);
                    Tibbers.UpdateMoveOrder(OrderType.PetHardMove);
                    Tibbers.SetWaypoints(GetPath(Tibbers.Position, end));
                }
            }
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        public void TargetExecute(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
        }

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

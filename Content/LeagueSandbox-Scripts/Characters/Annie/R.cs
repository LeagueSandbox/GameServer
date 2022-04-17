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
            NotSingleTargetSpell = true,
            TriggersSpellCasts = true,
            IsDamagingSpell = true,
            SpellDamageRatio = 0.5f,
            IsPetDurationBuff = true
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, OnSpellHit, false);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellHit(ISpell spell, IAttackableUnit target, ISpellMissile missle, ISpellSector sector)
        {
            var Ap = spell.CastInfo.Owner.Stats.AbilityPower.Total * 0.8f;
            var totalDamage = 50 + 125 * spell.CastInfo.SpellLevel + Ap;
            target.TakeDamage(spell.CastInfo.Owner, totalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner as IChampion;
            var tibbers = CreatePet(owner, spell, new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z), "Tibbers", "AnnieTibbers", "InfernalGuardian", 45.0f, owner.SkinID, showMinimapIfClone: false, isClone: false);
            (spell.CastInfo.Owner.GetSpell("InfernalGuardianGuide").Script as InfernalGuardianGuide).Tibbers = tibbers;

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

            spell.CreateSpellSector(new SectorParameters
            {
                Length = 250.0f,
                SingleTick = true,
                Type = SectorType.Area
            });
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
                //TODO: Validade all this section regarding Targeting enemies
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

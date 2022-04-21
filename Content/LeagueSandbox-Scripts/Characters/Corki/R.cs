using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Spells
{
    public class MissileBarrage : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            PersistsThroughDeath = true,
            IsNonDispellable = true,
            TriggersSpellCasts = true,
            CooldownIsAffectedByCDR = false,
            SpellFXOverrideSkins = new string[]
            {
                "FireworksCorki"
            }
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
            var owner = spell.CastInfo.Owner;
            FaceDirection(new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z), owner, true);
            var targetPos = GetPointFromUnit(spell.CastInfo.Owner, spell.SpellData.CastRange[0]);
            SpellCast(owner, 1, SpellSlotType.ExtraSlots, owner.Position, targetPos, false, Vector2.Zero);
        }

        public void OnSpellPostCast(ISpell spell)
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

    public class MissileBarrageMissile : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Circle
            },
            DoesntBreakShields = false,
            TriggersSpellCasts = false,
            SpellFXOverrideSkins = new string[]
            {
                "FireworksCorki"
            }
        };


        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, OnSpellHit, false);
        }

        public void OnSpellHit(ISpell spell, IAttackableUnit target, ISpellMissile missile, ISpellSector sector)
        {
            var owner = spell.CastInfo.Owner;

            var spellLevel = owner.Spells[3].CastInfo.SpellLevel;
            float AP = owner.Stats.AbilityPower.Total * 0.3f;
            float AD = owner.Stats.AttackDamage.Total * (0.1f + (0.1f * spellLevel));
            float totalDamage = 20 + (80 * spellLevel) + AP + AD;

            var targets = GetUnitsInRange(target.Position, 75.0f, true);
            foreach (var unit in targets)
            {
                unit.TakeDamage(owner, totalDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);
            }

            AddParticle(owner, null, "corki_misslebarrage_std_tar", target.Position);

            missile.SetToRemove();
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            FaceDirection(end, owner, true);
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
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

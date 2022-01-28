using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Scripting.CSharp;

namespace Spells
{
    public class YasuoQW : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        private Vector2 trueCoords;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            // here's nothing
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
            // here's empty
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
            var current = new Vector2(spell.CastInfo.Owner.Position.X, spell.CastInfo.Owner.Position.Y);
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var to = Vector2.Normalize(spellPos - current);
            var range = to * spell.SpellData.CastRangeDisplayOverride;
            trueCoords = current + range;

            FaceDirection(trueCoords, spell.CastInfo.Owner, true);
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            if (HasBuff(owner, "YasuoE"))
            {
                //spell.CastInfo.Owner.SpellAnimation("SPELL3b");
                AddParticleTarget(owner, owner, "Yasuo_Base_EQ_cas", owner);
                AddParticleTarget(owner, owner, "Yasuo_Base_EQ_SwordGlow", owner, 0, 1, "C_BUFFBONE_GLB_Weapon_1");
                foreach (var affectEnemys in GetUnitsInRange(spell.CastInfo.Owner.Position, 270f, true))
                {
                    if (affectEnemys is IAttackableUnit && affectEnemys.Team != spell.CastInfo.Owner.Team)
                    {
                        affectEnemys.TakeDamage(spell.CastInfo.Owner, spell.CastInfo.SpellLevel * 20f + spell.CastInfo.Owner.Stats.AttackDamage.Total, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                        AddParticleTarget(owner, affectEnemys, "Yasuo_Base_Q_hit_tar", affectEnemys);
                    }
                }
                AddBuff("YasuoQ01", 6f, 1, spell, spell.CastInfo.Owner, spell.CastInfo.Owner);
            }
            else
            {
                //spell.CastInfo.Owner.SpellAnimation("SPELL1A");
                //spell.AddLaser("YasuoQ", trueCoords);
                AddParticleTarget(owner, owner, "Yasuo_Q_Hand", owner);
                AddParticleTarget(owner, owner, "Yasuo_Base_Q1_cast_sound", owner);
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile missile)
        {
            AddParticleTarget(owner, target, "Yasuo_Base_Q_hit_tar", target);
            target.TakeDamage(owner, spell.CastInfo.SpellLevel * 20f + owner.Stats.AttackDamage.Total, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            if (!HasBuff(owner, "YasuoQ01"))
            {
                AddBuff("YasuoQ01", 6f, 1, spell, owner, owner);
            }
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
            //here's nothing
        }
    }
}

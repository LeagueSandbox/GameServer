using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;

namespace Spells
{
    public class YasuoQ2W : ISpellScript
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
                foreach (var affectEnemys in GetUnitsInRange(owner.Position, 270f, true))
                {
                    if (affectEnemys is IAttackableUnit && affectEnemys.Team != owner.Team)
                    {
                        affectEnemys.TakeDamage(owner, spell.CastInfo.SpellLevel * 20f + owner.Stats.AttackDamage.Total, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                        AddParticleTarget(owner, owner, "Yasuo_Base_Q_hit_tar", affectEnemys);
                    }
                }
                AddBuff("YasuoQ02", 6f, 1, spell, owner, owner);
                RemoveBuff(owner, "YasuoQ01");
            }
            else
            {
                //spell.CastInfo.Owner.SpellAnimation("SPELL1B");
                //spell.AddLaser("YasuoQ", trueCoords);
                AddParticleTarget(owner, owner, "Yasuo_Q_Hand", owner);
                AddParticleTarget(owner, owner, "Yasuo_Base_Q2_cast_sound", owner);
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile missile)
        {
            AddParticleTarget(owner, owner, "Yasuo_Base_Q_hit_tar", target);
            target.TakeDamage(owner, spell.CastInfo.SpellLevel * 20f + owner.Stats.AttackDamage.Total,DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            if (!HasBuff(owner, "YasuoQ02"))
            {
                AddBuff("YasuoQ02", 6f, 1, spell, owner, owner);
                RemoveBuff(owner, "YasuoQ01");
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
            //empty!
        }
    }
}

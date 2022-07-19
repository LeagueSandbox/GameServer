using System.Numerics;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;

namespace Spells
{
    public class YasuoQ2W : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        private Vector2 trueCoords;

        public void OnSpellCast(Spell spell)
        {
            var current = spell.CastInfo.Owner.Position;
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var to = Vector2.Normalize(spellPos - current);
            var range = to * spell.SpellData.CastRangeDisplayOverride;
            trueCoords = current + range;

            FaceDirection(trueCoords, spell.CastInfo.Owner, true);
        }

        public void OnSpellPostCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            if (HasBuff(owner, "YasuoE"))
            {
                //spell.CastInfo.Owner.SpellAnimation("SPELL3b");
                AddParticleTarget(owner, owner, "Yasuo_Base_EQ_cas", owner);
                AddParticleTarget(owner, owner, "Yasuo_Base_EQ_SwordGlow", owner, 0, 1, "C_BUFFBONE_GLB_Weapon_1");
                foreach (var affectEnemys in GetUnitsInRange(owner.Position, 270f, true))
                {
                    if (affectEnemys is AttackableUnit && affectEnemys.Team != owner.Team)
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

        public void ApplyEffects(ObjAIBase owner, AttackableUnit target, Spell spell, SpellMissile missile)
        {
            AddParticleTarget(owner, owner, "Yasuo_Base_Q_hit_tar", target);
            target.TakeDamage(owner, spell.CastInfo.SpellLevel * 20f + owner.Stats.AttackDamage.Total,DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            if (!HasBuff(owner, "YasuoQ02"))
            {
                AddBuff("YasuoQ02", 6f, 1, spell, owner, owner);
                RemoveBuff(owner, "YasuoQ01");
            }
        }
    }
}

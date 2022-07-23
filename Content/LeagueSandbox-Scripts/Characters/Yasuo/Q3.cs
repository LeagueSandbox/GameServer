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
    public class YasuoQ3W : ISpellScript
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
            if (HasBuff(spell.CastInfo.Owner, "YasuoE"))
            {
                //spell.CastInfo.Owner.SpellAnimation("SPELL3b");
                AddParticleTarget(owner, owner, "Yasuo_Base_EQ3_cas", owner);
                AddParticleTarget(owner, owner, "Yasuo_Base_EQ_SwordGlow", owner, bone: "C_BUFFBONE_GLB_Weapon_1");
                foreach (var affectEnemys in GetUnitsInRange(spell.CastInfo.Owner.Position, 270f, true))
                {
                    if (affectEnemys is AttackableUnit && affectEnemys.Team != owner.Team)
                    {
                        affectEnemys.TakeDamage(owner, spell.CastInfo.SpellLevel * 20f + owner.Stats.AttackDamage.Total, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                        AddParticleTarget(owner, owner, "Yasuo_Base_Q_WindStrike", affectEnemys);
                        AddParticleTarget(owner, owner, "Yasuo_Base_Q_windstrike_02", affectEnemys);
                        AddParticleTarget(owner, owner, "Yasuo_Base_Q_hit_tar", affectEnemys);
                        ForceMovement(affectEnemys, "RUN", new Vector2(affectEnemys.Position.X + 10f, affectEnemys.Position.Y + 10f), 13f, 0, 16.5f, 0, movementOrdersFacing: ForceMovementOrdersFacing.KEEP_CURRENT_FACING);
                    }
                }
            }
            else
            {
                //spell.AddProjectile("YasuoQ3Mis", spell.CastInfo.Owner.Position, spell.CastInfo.Owner.Position, trueCoords);
                //spell.CastInfo.Owner.SpellAnimation("SPELL1C");
                SetSpell(owner, "YasuoQW", SpellSlotType.SpellSlots, 0);
                AddParticleTarget(owner, owner, "Yasuo_Base_Q3_Hand", owner);
                AddParticleTarget(owner, owner, "Yasuo_Base_Q3_cast_sound", owner);
            }
            if (HasBuff(owner, "YasuoQ02"))
            {
                RemoveBuff(owner, "YasuoQ02");
            }
        }

        public void ApplyEffects(ObjAIBase owner, AttackableUnit target, Spell spell, SpellMissile missile)
        {
            AddParticleTarget(owner, owner, "Yasuo_Base_Q_WindStrike", target);
            AddParticleTarget(owner, owner, "Yasuo_Base_Q_windstrike_02", target);
            AddParticleTarget(owner, owner, "Yasuo_Base_Q_hit_tar", target);
            target.TakeDamage(owner, spell.CastInfo.SpellLevel * 20f + owner.Stats.AttackDamage.Total,DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            ForceMovement(target, "RUN", new Vector2(target.Position.X + 10f, target.Position.Y + 10f), 13f, 0, 16.5f, 0);
        }
    }
}

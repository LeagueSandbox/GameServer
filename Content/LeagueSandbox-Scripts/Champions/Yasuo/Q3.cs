using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class YasuoQ3W : IGameScript
    {
        private Vector2 trueCoords;
        public void OnActivate(IObjAiBase owner)
        {
            //empty
        }

        public void OnDeactivate(IObjAiBase owner)
        {
            //empty
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var current = new Vector2(owner.Position.X, owner.Position.Y);
            var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
            var range = to * spell.SpellData.CastRangeDisplayOverride;
            trueCoords = current + range;

            FaceDirection(trueCoords, owner, true, 0f);
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            if (HasBuff(owner, "YasuoE"))
            {
                spell.SpellAnimation("SPELL3b", owner);
                AddParticleTarget(owner, "Yasuo_Base_EQ3_cas.troy", owner);
                AddParticleTarget(owner, "Yasuo_Base_EQ_SwordGlow.troy", owner, bone: "C_BUFFBONE_GLB_Weapon_1");
                foreach (var affectEnemys in GetUnitsInRange(owner.Position, 270f, true))
                {
                    if (affectEnemys is IAttackableUnit && affectEnemys.Team != owner.Team)
                    {
                        affectEnemys.TakeDamage(owner, spell.Level * 20f + owner.Stats.AttackDamage.Total, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                        AddParticleTarget(owner, "Yasuo_Base_Q_WindStrike.troy", affectEnemys);
                        AddParticleTarget(owner, "Yasuo_Base_Q_windstrike_02.troy", affectEnemys);
                        AddParticleTarget(owner, "Yasuo_Base_Q_hit_tar.troy", affectEnemys);
                        ForceMovement(affectEnemys, "RUN", new Vector2(affectEnemys.Position.X + 10f, affectEnemys.Position.Y + 10f), 13f, 0, 16.5f, 0, movementOrdersFacing: ForceMovementOrdersFacing.KEEP_CURRENT_FACING);
                    }
                }
            }
            else
            {
                spell.AddProjectile("YasuoQ3Mis", owner.Position, trueCoords);
                spell.SpellAnimation("SPELL1C", owner);
                (owner as IChampion).SetSpell("YasuoQW", 0, true);
                AddParticleTarget(owner, "Yasuo_Base_Q3_Hand.troy", owner);
                AddParticleTarget(owner, "Yasuo_Base_Q3_cast_sound.troy", owner);
            }
            if (HasBuff(owner, "YasuoQ02"))
            {
                RemoveBuff(owner, "YasuoQ02");
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            AddParticleTarget(owner, "Yasuo_Base_Q_WindStrike.troy", target);
            AddParticleTarget(owner, "Yasuo_Base_Q_windstrike_02.troy", target);
            AddParticleTarget(owner, "Yasuo_Base_Q_hit_tar.troy", target);
            target.TakeDamage(owner, spell.Level * 20f + owner.Stats.AttackDamage.Total,DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            ForceMovement(target, "RUN", new Vector2(target.Position.X + 10f, target.Position.Y + 10f), 13f, 0, 16.5f, 0);
        }

        public void OnUpdate(double diff)
        {
            //empty
        }
    }
}

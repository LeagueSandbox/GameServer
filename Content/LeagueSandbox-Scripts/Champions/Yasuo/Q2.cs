using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class YasuoQ2W : IGameScript
    {
        private Vector2 trueCoords;
        public void OnActivate(IObjAiBase owner)
        {
            // here's nothing
        }

        public void OnDeactivate(IObjAiBase owner)
        {
            // here's empty
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var current = new Vector2(owner.X, owner.Y);
            var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
            var range = to * spell.SpellData.CastRangeDisplayOverride[0];
            trueCoords = current + range;

            FaceDirection(owner, trueCoords, true, 0f);
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            if (HasBuff(owner, "YasuoE"))
            {
                spell.SpellAnimation("SPELL3b", owner);
                AddParticleTarget(owner, "Yasuo_Base_EQ_cas.troy", owner);
                AddParticleTarget(owner, "Yasuo_Base_EQ_SwordGlow.troy", owner,1, "C_BUFFBONE_GLB_Weapon_1");
                foreach (var affectEnemys in GetUnitsInRange(owner, 270f, true))
                {
                    if (affectEnemys is IAttackableUnit && affectEnemys.Team != owner.Team)
                    {
                        affectEnemys.TakeDamage(owner, spell.Level * 20f + owner.Stats.AttackDamage.Total, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                        AddParticleTarget(owner, "Yasuo_Base_Q_hit_tar.troy", affectEnemys);
                    }
                }
                AddBuff("YasuoQ02", 6f, 1, spell, owner, owner);
                RemoveBuff(owner, "YasuoQ01");
            }
            else
            {
                spell.SpellAnimation("SPELL1B", owner);
                spell.AddLaser("YasuoQ", trueCoords.X, trueCoords.Y);
                AddParticleTarget(owner, "Yasuo_Q_Hand.troy", owner);
                AddParticleTarget(owner, "Yasuo_Base_Q2_cast_sound.troy", owner);
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            AddParticleTarget(owner, "Yasuo_Base_Q_hit_tar.troy", target);
            target.TakeDamage(owner, spell.Level * 20f + owner.Stats.AttackDamage.Total,DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            if (!HasBuff(owner, "YasuoQ02"))
            {
                AddBuff("YasuoQ02", 6f, 1, spell, owner, owner);
                RemoveBuff(owner, "YasuoQ01");
            }
        }

        public void OnUpdate(double diff)
        {
            //empty!
        }
    }
}

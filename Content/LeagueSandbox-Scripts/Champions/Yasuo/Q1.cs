using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class YasuoQW : IGameScript
    {
        private Vector2 trueCoords;
        public void OnActivate(IChampion owner)
        {
            // here's nothing
        }

        public void OnDeactivate(IChampion owner)
        {
            // here's empty
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var current = new Vector2(owner.X, owner.Y);
            var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
            var range = to * spell.SpellData.CastRangeDisplayOverride[0];
            trueCoords = current + range;

            FaceDirection(owner, trueCoords, true, 0f);
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var _hasbuff = owner.HasBuffGameScriptActive("YasuoE", "YasuoE");
            if (_hasbuff)
            {
                spell.SpellAnimation("SPELL3b", owner);
                AddParticleTarget(owner, "Yasuo_Base_EQ_cas.troy", owner);
                AddParticleTarget(owner, "Yasuo_Base_EQ_SwordGlow.troy", owner,1,"C_BUFFBONE_GLB_Weapon_1");
                foreach (var affectEnemys in GetUnitsInRange(owner, 270f, true))
                {
                    if (affectEnemys is IAttackableUnit && affectEnemys.Team != owner.Team)
                    {
                        affectEnemys.TakeDamage(owner, spell.Level * 20f + owner.Stats.AttackDamage.Total, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                        AddParticleTarget(owner, "Yasuo_Base_Q_hit_tar.troy", affectEnemys);
                    }
                }
                AddBuffGameScript("YasuoQ01", 1, spell, BuffType.COMBAT_ENCHANCER, owner, 6f, true);
            }
            else
            {
                spell.SpellAnimation("SPELL1A", owner);
                spell.AddLaser("YasuoQ", trueCoords.X, trueCoords.Y);
                AddParticleTarget(owner, "Yasuo_Q_Hand.troy", owner);
                AddParticleTarget(owner, "Yasuo_Base_Q1_cast_sound.troy", owner);
            }
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            var _hasbuff = owner.HasBuffGameScriptActive("YasuoQ01", "YasuoQ01");
            AddParticleTarget(owner, "Yasuo_Base_Q_hit_tar.troy", target);
            target.TakeDamage(owner, spell.Level * 20f + owner.Stats.AttackDamage.Total,DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            if (!_hasbuff)
            {
                AddBuffGameScript("YasuoQ01",1,spell, BuffType.COMBAT_ENCHANCER, owner,6f,true);
            }
        }

        public void OnUpdate(double diff)
        {
            //here's nothing
        }
    }
}

using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Linq;
using GameServerCore;

namespace Spells
{
    public class YasuoQ2W : IGameScript
    {
        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var current = new Vector2(owner.X, owner.Y);
            var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
            var range = to * 475;
            var trueCoords = current + range;

            FaceDirection(owner, trueCoords, true, 0f);
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            var _hasbuff = owner.HasBuffGameScriptActive("YasuoE", "YasuoE");
            if (_hasbuff)
            {
                spell.SpellAnimation("SPELL3b", owner);
                AddParticleTarget(owner, "Yasuo_Base_EQ_cas.troy", owner);
                AddParticleTarget(owner, "Yasuo_Base_EQ_SwordGlow.troy", owner, bone: "C_BUFFBONE_GLB_Weapon_1");
                foreach (var units in GetUnitsInRange(owner, 375f, true).Where(x => x.Team == CustomConvert.GetEnemyTeam(owner.Team)))
                {
                    if (units is IAttackableUnit)
                    {
                        units.TakeDamage(owner, spell.Level * 20f + owner.Stats.AttackDamage.Total, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                        AddParticleTarget(owner, "Yasuo_Base_Q_hit_tar.troy", units);
                    }
                }
                AddBuffGameScript("YasuoQ02", 1, spell, BuffType.COMBAT_ENCHANCER, owner, 10f, true);
                ((ObjAiBase)owner).RemoveBuffGameScriptsWithName("YasuoQ01", "YasuoQ01");
            }
            else
            {
                spell.SpellAnimation("SPELL1B", owner);
                AddParticleTarget(owner, "Yasuo_Base_Q2_cast_sound.troy", owner);
                var current = new Vector2(owner.X, owner.Y);
                var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
                var range = to * 475;
                var trueCoords = current + range;

                spell.AddLaser("YasuoQ", trueCoords.X, trueCoords.Y);
                AddParticleTarget(owner, "Yasuo_Q_Hand.troy", owner);
            }
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            var _hasbuff = owner.HasBuffGameScriptActive("YasuoQ02", "YasuoQ02");
            AddParticleTarget(owner, "Yasuo_Base_Q_hit_tar.troy", target);
            target.TakeDamage(owner, spell.Level * 20f + owner.Stats.AttackDamage.Total,DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            if (!_hasbuff)
            {
                AddBuffGameScript("YasuoQ02", 1, spell, BuffType.COMBAT_ENCHANCER, owner, 10f, true);
                ((ObjAiBase)owner).RemoveBuffGameScriptsWithName("YasuoQ01", "YasuoQ01");
            }
        }

        public void OnUpdate(double diff)
        {
        }
    }
}

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
    public class YasuoQ3W : IGameScript
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
                AddParticleTarget(owner, "Yasuo_Base_EQ3_cas.troy", owner);
                AddParticleTarget(owner, "Yasuo_Base_EQ_SwordGlow.troy", owner, bone: "C_BUFFBONE_GLB_Weapon_1");
                foreach (var units in GetUnitsInRange(owner, 375f, true).Where(x => x.Team == CustomConvert.GetEnemyTeam(owner.Team)))
                {
                    if (units is IAttackableUnit)
                    {
                        units.TakeDamage(owner, spell.Level * 20f + owner.Stats.AttackDamage.Total, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
                        AddParticleTarget(owner, "Yasuo_Base_Q_WindStrike.troy", units);
                        AddParticleTarget(owner, "Yasuo_Base_Q_windstrike_02.troy", units);
                        AddParticleTarget(owner, "Yasuo_Base_Q_hit_tar.troy", units);
                        DashToLocation((ObjAiBase)units, units.X + 10f, units.Y + 10f, 13f, true, "RUN", 16.5f, travelTime: 1.15f);
                    }
                }
            }
            else
            {
                spell.SpellAnimation("SPELL1C", owner);
                AddParticleTarget(owner, "Yasuo_Base_Q3_cast_sound.troy", owner);
                var current = new Vector2(owner.X, owner.Y);
                var to = Vector2.Normalize(new Vector2(spell.X, spell.Y) - current);
                var range = to * 1100;
                var trueCoords = current + range;

                spell.AddProjectile("YasuoQ3Mis", owner.X, owner.Y, trueCoords.X, trueCoords.Y);
                AddParticleTarget(owner, "Yasuo_Base_Q3_Hand.troy", owner);
                owner.SetSpell("YasuoQW", 0, true);                
            }
            if (((ObjAiBase)owner).HasBuffGameScriptActive("YasuoQ02", "YasuoQ02"))
            {
                ((ObjAiBase)owner).RemoveBuffGameScriptsWithName("YasuoQ02", "YasuoQ02");
            }
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            AddParticleTarget(owner, "Yasuo_Base_Q_WindStrike.troy", target);
            AddParticleTarget(owner, "Yasuo_Base_Q_windstrike_02.troy", target);
            AddParticleTarget(owner, "Yasuo_Base_Q_hit_tar.troy", target);
            target.TakeDamage(owner, spell.Level * 20f + owner.Stats.AttackDamage.Total,DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            DashToLocation((ObjAiBase)target, target.X+10f, target.Y+10f, 13f, true, "RUN", 16.5f, travelTime:1.15f);
        }

        public void OnUpdate(double diff)
        {
        }
    }
}

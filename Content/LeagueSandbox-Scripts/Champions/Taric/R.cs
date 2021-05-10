using System.Linq;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;

namespace Spells
{
    public class TaricHammerSmash : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
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
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var p1 = AddParticleTarget(spell.CastInfo.Owner, "TaricHammerSmash_nova.troy", spell.CastInfo.Owner);
            var p2 = AddParticleTarget(spell.CastInfo.Owner, "TaricHammerSmash_shatter.troy", spell.CastInfo.Owner);
            var hasbuff = spell.CastInfo.Owner.HasBuff("Radiance");
            var ap = spell.CastInfo.Owner.Stats.AbilityPower.Total * 0.5f;
            var damage = 50 + spell.CastInfo.SpellLevel * 100 + ap;

            foreach (var enemyTarget in GetUnitsInRange(spell.CastInfo.Owner.Position, 375, true)
                .Where(x => x.Team == CustomConvert.GetEnemyTeam(spell.CastInfo.Owner.Team)))
            {
                if (enemyTarget is IAttackableUnit)
                {
                    enemyTarget.TakeDamage(spell.CastInfo.Owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                    var ep1 = AddParticleTarget(spell.CastInfo.Owner, "Taric_GemStorm_Tar.troy", enemyTarget, 1.25f);
                    var ep2 = AddParticleTarget(spell.CastInfo.Owner, "Taric_GemStorm_Aura.troy", enemyTarget, 1.25f);
                    var ep3 = AddParticleTarget(spell.CastInfo.Owner, "Taric_ShoulderFlare.troy", enemyTarget, 1.25f);
                    CreateTimer(1f, () =>
                    {
                        RemoveParticle(ep1);
                        RemoveParticle(ep2);
                        RemoveParticle(ep3);
                    });
                }
            }

            foreach (var allyTarget in GetUnitsInRange(spell.CastInfo.Owner.Position, 1100, true)
                .Where(x => x.Team != CustomConvert.GetEnemyTeam(spell.CastInfo.Owner.Team)))
            {
                if (allyTarget is IObjAiBase && spell.CastInfo.Owner != allyTarget && hasbuff == false)
                {
                    AddBuff("Radiance_ally", 10.0f, 1, spell, allyTarget, spell.CastInfo.Owner);
                }
            }
            if (spell.CastInfo.Owner == spell.CastInfo.Targets[0].Unit && hasbuff == false)
            {
                var p3 = AddParticleTarget(spell.CastInfo.Owner, "taricgemstorm.troy", spell.CastInfo.Owner);
                AddBuff("Radiance", 10.0f, 1, spell, spell.CastInfo.Owner, spell.CastInfo.Owner);
                CreateTimer(10.0f, () =>
                {
                    RemoveParticle(p3);
                });
            }

            CreateTimer(10.0f, () =>
            {
                RemoveParticle(p1);
                RemoveParticle(p2);
            }
            );
        }

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell)
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

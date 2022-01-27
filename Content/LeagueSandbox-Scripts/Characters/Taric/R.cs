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
            var owner = spell.CastInfo.Owner;
            var p1 = AddParticleTarget(owner, owner, "TaricHammerSmash_nova", owner);
            var p2 = AddParticleTarget(owner, owner, "TaricHammerSmash_shatter", owner);
            var hasbuff = owner.HasBuff("Radiance");
            var ap = owner.Stats.AbilityPower.Total * 0.5f;
            var damage = 50 + spell.CastInfo.SpellLevel * 100 + ap;

            foreach (var enemyTarget in GetUnitsInRange(owner.Position, 375, true)
                .Where(x => x.Team == CustomConvert.GetEnemyTeam(owner.Team)))
            {
                if (enemyTarget is IAttackableUnit)
                {
                    enemyTarget.TakeDamage(spell.CastInfo.Owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                    var ep1 = AddParticleTarget(owner, owner, "Taric_GemStorm_Tar", enemyTarget, size: 1.25f);
                    var ep2 = AddParticleTarget(owner, owner, "Taric_GemStorm_Aura", enemyTarget, size: 1.25f);
                    var ep3 = AddParticleTarget(owner, owner, "Taric_ShoulderFlare", enemyTarget, size: 1.25f);
                    CreateTimer(1f, () =>
                    {
                        RemoveParticle(ep1);
                        RemoveParticle(ep2);
                        RemoveParticle(ep3);
                    });
                }
            }

            foreach (var allyTarget in GetUnitsInRange(owner.Position, 1100, true)
                .Where(x => x.Team != CustomConvert.GetEnemyTeam(owner.Team)))
            {
                if (allyTarget is IObjAiBase && owner != allyTarget && hasbuff == false)
                {
                    AddBuff("Radiance_ally", 10.0f, 1, spell, allyTarget, owner);
                }
            }
            if (owner == spell.CastInfo.Targets[0].Unit && hasbuff == false)
            {
                var p3 = AddParticleTarget(owner, owner, "taricgemstorm", owner);
                AddBuff("Radiance", 10.0f, 1, spell, owner, owner);
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

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
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

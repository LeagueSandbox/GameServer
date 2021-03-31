﻿using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class DeathfireGrasp : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            spell.AddProjectileTarget("DeathfireGraspSpell", spell.CastInfo.SpellCastLaunchPosition, target);
            var p1 = AddParticleTarget(owner, "deathFireGrasp_tar.troy", target);
            var p2 = AddParticleTarget(owner, "obj_DeathfireGrasp_debuff.troy", target);
            AddBuff("DeathfireGraspSpell", 4.0f, 1, spell, target, owner);
            CreateTimer(4.0f, () =>
            {
                RemoveParticle(p1);
                RemoveParticle(p2);
            });
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            var damage = target.Stats.HealthPoints.Total * 0.15f;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_DEFAULT, false);
            projectile.SetToRemove();
        }

        public void OnUpdate(float diff)
        {
        }

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }
    }
}

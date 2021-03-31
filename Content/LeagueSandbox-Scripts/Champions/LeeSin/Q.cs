﻿using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class BlindMonkQOne : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var current = new Vector2(spell.CastInfo.SpellCastLaunchPosition.X, spell.CastInfo.SpellCastLaunchPosition.Z);
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var to = Vector2.Normalize(spellPos - current);
            var range = to * spell.SpellData.CastRangeDisplayOverride;
            var trueCoords = current + range;
            //LogDebug("BlindMonkQOne going from (" + current.X + ", " + current.Y + ") -> (" + trueCoords.X + ", " + trueCoords.Y + ")");
            spell.AddProjectile("BlindMonkQOne", new Vector2(spell.CastInfo.SpellCastLaunchPosition.X, spell.CastInfo.SpellCastLaunchPosition.Z), current, trueCoords, HitResult.HIT_Normal, true);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            var ad = owner.Stats.AttackDamage.Total * 0.9f;
            var damage = 50 + (spell.CastInfo.SpellLevel * 30) + ad;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            AddParticleTarget(owner, "blindMonk_Q_resonatingStrike_tar.troy", target, 1, "C_BuffBone_Glb_Center_Loc");
            AddParticleTarget(owner, "blindMonk_Q_tar.troy", target, 1, "C_BuffBone_Glb_Center_Loc");
            if (target is IObjAiBase u)
            {
                AddBuff("BlindMonkSonicWave", 3f, 1, spell, u, owner);
            }

            projectile.SetToRemove();

            // TODO: SetSpell("BlindMonkQTwo")
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

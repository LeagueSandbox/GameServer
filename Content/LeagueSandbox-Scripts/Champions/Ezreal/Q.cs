using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using LeagueSandbox.GameServer.GameObjects.Spells;
using LeagueSandbox.GameServer.API;

namespace Spells
{
    public class EzrealMysticShot : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        Vector2 direction;

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
            var current = new Vector2(spell.CastInfo.Owner.Position.X, spell.CastInfo.Owner.Position.Y);
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            direction = Vector2.Normalize(spellPos - current);
            if (float.IsNaN(direction.X) || float.IsNaN(direction.Y))
            {
                direction = new Vector2(1.0f, 0.0f);
            }
            spell.CastInfo.Owner.FaceDirection(new Vector3(direction.X, 0.0f, direction.Y), false);

            AddParticleTarget(spell.CastInfo.Owner, "ezreal_bow.troy", spell.CastInfo.Owner, 1, "L_HAND", lifetime: 1.0f);
        }

        public void OnSpellPostCast(ISpell spell)
        {
            spell.CastInfo.Owner.FaceDirection(new Vector3(direction.X, 0.0f, direction.Y));
            //spell.AddProjectile("EzrealMysticShotMissile", current, current, trueCoords);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            var ad = spell.CastInfo.Owner.Stats.AttackDamage.Total * spell.SpellData.AttackDamageCoefficient;
            var ap = spell.CastInfo.Owner.Stats.AbilityPower.Total * spell.SpellData.MagicDamageCoefficient;
            var damage = 15 + spell.CastInfo.SpellLevel * 20 + ad + ap;
            target.TakeDamage(spell.CastInfo.Owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            for (byte i = 0; i < 4; i++)
            {
                spell.CastInfo.Owner.Spells[i].LowerCooldown(1);
            }
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

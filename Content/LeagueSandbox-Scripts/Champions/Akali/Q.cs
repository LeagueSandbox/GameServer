using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class AkaliMota : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
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
            var current = new Vector2(owner.Position.X, owner.Position.Y);
            var to = Vector2.Normalize(new Vector2(target.Position.X, target.Position.Y) - current);
            var range = to * 1150;
            var trueCoords = current + range;
            spell.AddProjectile("AkaliMota", current, current, trueCoords);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            var ap = owner.Stats.AbilityPower.Total * 0.4f;
            var damage = 15 + spell.CastInfo.SpellLevel * 20 + ap;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
            AddParticleTarget(owner, "akali_markOftheAssasin_marker_tar_02.troy", target, 1, "");
            projectile.SetToRemove();
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

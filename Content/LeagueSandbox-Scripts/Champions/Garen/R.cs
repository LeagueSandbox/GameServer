using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;

namespace Spells
{
    public class GarenR : ISpellScript
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

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            var target = spell.CastInfo.Targets[0].Unit;
            AddParticleTarget(owner, "Garen_Base_R_Tar_Impact.troy", target, 1);
            AddParticleTarget(owner, "Garen_Base_R_Sword_Tar.troy", target, 1);
            var missinghealth = target.Stats.HealthPoints.Total - target.Stats.CurrentHealth;
            var damageperc = missinghealth * new[] { 0.28f, 0.33f, 0.40f }[spell.CastInfo.SpellLevel - 1];
            var damage = spell.CastInfo.SpellLevel * 175 + damageperc;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            if (target.IsDead)
            {
                AddParticleTarget(owner, "Garen_Base_R_Champ_Kill.troy", target, 1);
                AddParticleTarget(owner, "Garen_Base_R_Champ_Death.troy", target, 1);
            }
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

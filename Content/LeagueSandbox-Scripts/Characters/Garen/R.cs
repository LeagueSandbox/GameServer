using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class GarenR : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnSpellPostCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            var target = spell.CastInfo.Targets[0].Unit;
            AddParticleTarget(owner, target, "Garen_Base_R_Tar_Impact", target);
            AddParticleTarget(owner, target, "Garen_Base_R_Sword_Tar", target);
            var missinghealth = target.Stats.HealthPoints.Total - target.Stats.CurrentHealth;
            var damageperc = missinghealth * new[] { 0.28f, 0.33f, 0.40f }[spell.CastInfo.SpellLevel - 1];
            var damage = spell.CastInfo.SpellLevel * 175 + damageperc;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            if (target.IsDead)
            {
                AddParticleTarget(owner, target, "Garen_Base_R_Champ_Kill", target);
                AddParticleTarget(owner, target, "Garen_Base_R_Champ_Death", target);
            }
        }
    }
}

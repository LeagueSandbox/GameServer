using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;

namespace Spells
{
    public class BlindingDart : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnSpellPostCast(Spell spell)
        {
            //spell.AddProjectileTarget("ToxicShot", spell.CastInfo.SpellCastLaunchPosition, spell.CastInfo.Targets[0].Unit);
        }

        public void ApplyEffects(ObjAIBase owner, AttackableUnit target, Spell spell, SpellMissile missile)
        {
            var ap = owner.Stats.AbilityPower.Total * 0.8f;
            var damage = 35 + spell.CastInfo.SpellLevel * 45 + ap;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
            var time = 1.25f + 0.25f * spell.CastInfo.SpellLevel;
            AddBuff("Blind", time, 1, spell, target, owner);
            missile.SetToRemove();
        }
    }
}

using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;

namespace Spells
{
    public class AkaliShadowSwipe : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void OnSpellPostCast(Spell spell)
        {
            var sector = spell.CreateSpellSector(new SectorParameters
            {
                Length = 300f,
                SingleTick = true,
                Type = SectorType.Area
            });
        }
        public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
        {
            var owner = spell.CastInfo.Owner;
            var AP = spell.CastInfo.Owner.Stats.AbilityPower.Total * 0.3f;
            var AD = spell.CastInfo.Owner.Stats.AttackDamage.Total * 0.6f;
            var damage = 40 + spell.CastInfo.SpellLevel * 30 + AP + AD;
            var MarkAPratio = spell.CastInfo.Owner.Stats.AbilityPower.Total * 0.5f;
            var MarkDamage = 45 + 25 * (owner.GetSpell("AkaliMota").CastInfo.SpellLevel - 1) + MarkAPratio;

            if (target.HasBuff("AkaliMota"))
            {
                target.TakeDamage(owner, MarkDamage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_PROC, false);
                AddParticleTarget(owner, target, "akali_mark_impact_tar", target, 1f);
                RemoveBuff(target, "AkaliMota");
            }
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_SPELLAOE, false);
            AddParticleTarget(owner, target, "akali_shadowSwipe_tar", target, 1f);
        }
    }
}

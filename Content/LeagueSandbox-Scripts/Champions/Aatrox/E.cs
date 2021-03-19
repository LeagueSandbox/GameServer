using System.Numerics;
using GameServerCore;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Collections.Generic;

namespace Spells
{
    public class AatroxE : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        List<uint> UnitsHit;

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
            UnitsHit = new List<uint>();
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var ownerPos = spell.CastInfo.Owner.Position;
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            var to = Vector2.Normalize(spellPos - ownerPos);
            if (float.IsNaN(to.X) || float.IsNaN(to.Y))
            {
                to = new Vector2(spell.CastInfo.Owner.Direction.X, spell.CastInfo.Owner.Direction.Z);
            }
            var coneRange = to * spell.SpellData.CastRangeDisplayOverride;
            var missileRange = to * spell.CastInfo.Owner.GetSpell("AatroxEConeMissile").SpellData.CastRangeDisplayOverride;

            var coneWidth = spell.SpellData.LineWidth/*owner.GetSpell("AatroxEConeMissile").SpellData.LineWidth * 2 + owner.GetSpell("AatroxEConeMissile2").SpellData.LineWidth*/;

            // Calculate start for each missile.
            // Left
            var toMissile1Start = Extensions.Rotate(to, 270.0f) * coneWidth;
            var missile1Start = ownerPos + toMissile1Start;

            // Right
            var toMissile2Start = Extensions.Rotate(to, 90.0f) * coneWidth;
            var missile2Start = ownerPos + toMissile2Start;
            
            // Due to Riot Spaghetti, even when overriding cast position, it is required that the target position be different from the target end position,
            // hence the two different end points. Otherwise, the projectile spawns and is immediately destroyed (client-side) as it thinks it has already reached its destination.
            var coneEnd = ownerPos + coneRange;
            // Both missiles triangulate onto one position.
            var missileEnd = ownerPos + missileRange;

            AddParticleTarget(spell.CastInfo.Owner, "Aatrox_Base_E_Glow.troy", spell.CastInfo.Owner, bone: "C_BUFFBONE_GLB_CHEST_LOC");

            // ConeMissile (middle)
            spell.AddProjectile("AatroxEConeMissile2", ownerPos, ownerPos, coneEnd);
            // With override cast position, the angle is determined by: cast position -> start position, rather than start -> end.
            // The end position will then only be used for the distance 
            // ConeMissile1 (left)
            spell.AddProjectile("AatroxEConeMissile", missile1Start, coneEnd, missileEnd, overrideCastPosition: true);
            // ConeMissile2 (right)
            spell.AddProjectile("AatroxEConeMissile", missile2Start, coneEnd, missileEnd, overrideCastPosition: true);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            if (UnitsHit.Contains(target.NetId))
            {
                return;
            }

            AddParticleTarget(owner, "Aatrox_Base_EMissile_Hit.troy", target, bone: "C_BUFFBONE_GLB_CHEST_LOC");

            var ad = owner.Stats.AttackDamage.Total * spell.SpellData.AttackDamageCoefficient;
            var ap = owner.Stats.AbilityPower.Total * spell.SpellData.MagicDamageCoefficient;
            var damage = 75 + (spell.CastInfo.SpellLevel - 1) * 35 + ad + ap;
            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);

            //var slowDuration = 1.75f + (spell.CastInfo.SpellLevel - 1) * 0.25f;
            //AddBuff("AatroxEslow", slowDuration, 1, spell, target, owner);

            UnitsHit.Add(target.NetId);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

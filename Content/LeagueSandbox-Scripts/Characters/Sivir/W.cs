using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Sector;

namespace Spells
{
    public class SivirW : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            AddBuff("SivirW", 6.0f, 3, spell, owner, owner);
        }
    }

    public class SivirWAttack : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Target
            }
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, true);
        }

        public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
        {
            var owner = spell.CastInfo.Owner;
            owner.AutoAttackHit(target);
            AddParticleTarget(owner, target, "Sivir_Base_W_Tar.troy", target, bone: "C_BUFFBONE_GLB_CHEST_LOC");
            SpellCast(owner, 2, SpellSlotType.ExtraSlots, false, target, missile.Position);
        }
    }

    public class SivirWAttackBounce : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = false,
            MissileParameters = new MissileParameters
            {
                Type = MissileType.Chained,
                // Sivir W bounces until all units in bounce range have been hit.
                MaximumHits = int.MaxValue
            }
        };

        AttackableUnit firstTarget;

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, spell, TargetExecute, false);
        }

        public void TargetExecute(Spell spell, AttackableUnit target, SpellMissile missile, SpellSector sector)
        {
            // Skip the first target because we already hit it in SivirWAttack
            if (firstTarget == target)
            {
                return;
            }

            var owner = spell.CastInfo.Owner;

            AddParticleTarget(owner, target, "Sivir_Base_W_Tar.troy", target);

            var damage = owner.Stats.AttackDamage.Total * (0.5f + (0.05f * (spell.CastInfo.SpellLevel - 1)));

            target.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);
        }

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            firstTarget = target;
        }
    }
}


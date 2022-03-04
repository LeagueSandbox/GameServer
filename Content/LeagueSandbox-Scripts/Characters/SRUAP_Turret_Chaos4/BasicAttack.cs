using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Spells
{
    public class SRUAP_Turret_Chaos4BasicAttack : ISpellScript
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
            AddParticleTarget(owner, owner, "SRU_Order_Laser_Turret_cas", target, bone: "joint2");
            AddParticleTarget(owner, target, "SRU_Inhibitor_chaos_Tower_Beam_Lvl1", owner, 0.5f, bone: "ROOT", targetBone: "joint2");
            AddParticleTarget(owner, target, "SRU_Chaos_Laser_Turret_Tar", target, 0.5f, bone: "ROOT", targetBone: "ROOT");

            var dmg = owner.Stats.AttackDamage.Total;
            if(target is IMinion)
            {
                dmg *= 0.7f;
            }

            owner.TargetUnit.TakeDamage(owner, dmg, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, false);

            AddBuff("S5Test_TowerWrath", 0.5f, 1, spell, target, owner);
            AddBuff("SRU_Turret_Laser_Audio", 0.5f, 1, spell, owner, owner);
        }

        public void OnLaunchAttack(ISpell spell)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
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


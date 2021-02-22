using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class NasusQ : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        IBuff buffQ;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            if (!owner.HasBuff("NasusQStacks"))
            {
                AddBuff("NasusQStacks", 20000f, 1, null, owner, owner, true);
            }
            buffQ = AddBuff("NasusQ", 10f, 1, spell, owner, owner);
            owner.SetAutoAttackSpell("NasusQAttack", true);
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
            buffQ.DeactivateBuff();
            target.TakeDamage(owner, (30f + (20f * (spell.CastInfo.SpellLevel - 1))) + owner.GetBuffWithName("NasusQStacks").StackCount, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, owner.IsNextAutoCrit);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}


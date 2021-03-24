using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Collections.Generic;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using System.Numerics;
using LeagueSandbox.GameServer.API;
using GameServerCore.Enums;

namespace Spells
{
    public class NasusBasicAttack : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
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
            spell.CastInfo.Owner.SetAutoAttackSpell("NasusBasicAttack2", false);
        }

        public void OnUpdate(float diff)
        {
        }
    }

    public class NasusBasicAttack2 : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
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
            spell.CastInfo.Owner.SetAutoAttackSpell("NasusBasicAttack", false);
        }

        public void OnUpdate(float diff)
        {
        }
    }

    public class NasusQAttack : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            ApiEventManager.OnSpellHit.AddListener(this, new KeyValuePair<ISpell, IObjAiBase>(spell, owner), ApplyEffects, false);
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
        }

        public void ApplyEffects(ISpell spell, IAttackableUnit target, IProjectile projectile)
        {
            //RemoveParticleSilent(pTar);
            target.TakeDamage(spell.CastInfo.Owner, 30f + (20f * (spell.CastInfo.SpellLevel - 1)) + spell.CastInfo.Owner.GetBuffWithName("NasusQStacks").StackCount, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, spell.CastInfo.Owner.IsNextAutoCrit);
            spell.CastInfo.Owner.GetBuffWithName("NasusQ").DeactivateBuff();

            ApiEventManager.OnSpellHit.RemoveListener(this, new KeyValuePair<ISpell, IObjAiBase>(spell, spell.CastInfo.Owner));
        }

        public void OnUpdate(float diff)
        {
        }
    }
}


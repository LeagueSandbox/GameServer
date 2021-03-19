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

    public class NasusCritAttack : ISpellScript
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

        IParticle pTar;
        ISpell ownerSpell;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            ownerSpell = spell;

            spell.CastInfo.UseAttackCastTime = true;

            ApiEventManager.OnHitUnit.AddListener(this, owner, ApplyOnHitEffects);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
            var animPairs = new Dictionary<string, string> { { "Attack1", "Spell1" } };
            SetAnimStates(spell.CastInfo.Owner, animPairs);
        }

        public void OnSpellPostCast(ISpell spell)
        {
            pTar = AddParticleTarget(spell.CastInfo.Owner, "Nasus_Base_Q_Tar.troy", spell.CastInfo.Targets[0].Unit);

            var animPairs = new Dictionary<string, string> { { "Spell1", "Attack1" } };
            SetAnimStates(spell.CastInfo.Owner, animPairs);
            spell.CastInfo.Owner.SetAutoAttackSpell("NasusBasicAttack", false);
        }

        public void ApplyOnHitEffects(IAttackableUnit target, bool isCrit)
        {
            RemoveParticleSilent(pTar);
            target.TakeDamage(ownerSpell.CastInfo.Owner, 30f + (20f * (ownerSpell.CastInfo.SpellLevel - 1)) + ownerSpell.CastInfo.Owner.GetBuffWithName("NasusQStacks").StackCount, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_ATTACK, ownerSpell.CastInfo.Owner.IsNextAutoCrit);
            ownerSpell.CastInfo.Owner.GetBuffWithName("NasusQ").DeactivateBuff();
        }

        public void OnUpdate(float diff)
        {
        }
    }
}


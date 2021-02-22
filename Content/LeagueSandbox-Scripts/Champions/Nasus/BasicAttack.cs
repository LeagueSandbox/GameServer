using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Collections.Generic;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

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

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            owner.SetAutoAttackSpell("NasusBasicAttack2", false);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile p)
        {
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

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            owner.SetAutoAttackSpell("NasusBasicAttack", false);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile p)
        {
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

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile p)
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
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            spell.CastInfo.UseAttackCastTime = true;
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var animPairs = new Dictionary<string, string> { { "Attack1", "Spell1" } };
            SetAnimStates(owner, animPairs);
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            pTar = AddParticleTarget(owner, "Nasus_Base_Q_Tar.troy", target);

            var animPairs = new Dictionary<string, string> { { "Spell1", "Attack1" } };
            SetAnimStates(owner, animPairs);
            owner.SetAutoAttackSpell("NasusBasicAttack", false);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, IProjectile p)
        {
            RemoveParticleSilent(pTar);
            owner.GetSpell("NasusQ").ApplyEffects(target, p);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}


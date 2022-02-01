using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;

namespace Spells
{
    public class OdinRecall : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            CastingBreaksStealth = true,
            ChannelDuration = 4.5f,
            TriggersSpellCasts = false,
            NotSingleTargetSpell = true
        };

        IParticle recallParticle;

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnTakeDamage(IAttackableUnit unit, IAttackableUnit attacker)
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

        public void OnSpellChannel(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            recallParticle = AddParticleTarget(owner, owner, "teleporthome_shortimproved", owner, 4.5f, flags: 0);
            //TODO: Find out what's the proper buff name, i couldnt find it. But i at least found it's icon, "RecallHome.dds"
            AddBuff("RecallHome", 4.4f, 1, spell, owner, owner);
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
            recallParticle.SetToRemove();
            RemoveBuff(spell.CastInfo.Owner, "RecallHome");
        }

        public void OnSpellPostChannel(ISpell spell)
        {
            var owner = spell.CastInfo.Owner as IChampion;

            owner.Recall();

            AddParticleTarget(owner, owner, "TeleportArrive", owner, flags: 0);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}


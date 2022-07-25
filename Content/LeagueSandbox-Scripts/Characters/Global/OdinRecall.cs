using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects;

namespace Spells
{
    public class OdinRecall : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            CastingBreaksStealth = true,
            ChannelDuration = 4.5f,
            TriggersSpellCasts = false,
            NotSingleTargetSpell = true
        };

        Particle recallParticle;

        public void OnSpellChannel(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            recallParticle = AddParticleTarget(owner, owner, "teleporthome_shortimproved", owner, 4.5f, flags: 0);
            //TODO: Find out what's the proper buff name, i couldnt find it. But i at least found it's icon, "RecallHome.dds"
            AddBuff("RecallHome", 4.4f, 1, spell, owner, owner);
        }

        public void OnSpellChannelCancel(Spell spell, ChannelingStopSource reason)
        {
            recallParticle.SetToRemove();
            RemoveBuff(spell.CastInfo.Owner, "RecallHome");
        }

        public void OnSpellPostChannel(Spell spell)
        {
            var owner = spell.CastInfo.Owner as Champion;

            owner.Recall();

            AddParticleTarget(owner, owner, "TeleportArrive", owner, flags: 0);
        }
    }
}


using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects;

namespace Spells
{
    public class Recall : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            CastingBreaksStealth = true,
            ChannelDuration = 8.0f,
            TriggersSpellCasts = false,
            NotSingleTargetSpell = true
        };

        Particle recallParticle;

        public void OnSpellChannel(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            recallParticle = AddParticleTarget(owner, owner, "TeleportHome", owner, 8.0f, flags: 0);
            AddBuff("Recall", 7.9f, 1, spell, owner, owner);
            owner.IconInfo.ChangeBorder("Recall", "recall");
        }

        public void OnSpellChannelCancel(Spell spell, ChannelingStopSource reason)
        {
            var owner = spell.CastInfo.Owner;
            recallParticle.SetToRemove();
            RemoveBuff(owner, "Recall");
            owner.IconInfo.ResetBorder();
        }

        public void OnSpellPostChannel(Spell spell)
        {
            var owner = spell.CastInfo.Owner as Champion;
            owner.Recall();
            AddParticleTarget(owner, owner, "TeleportArrive", owner, flags: 0);
            owner.IconInfo.ResetBorder();
        }
    }
}

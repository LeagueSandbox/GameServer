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
    public class Recall : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            CastingBreaksStealth = true,
            ChannelDuration = 8.0f,
            TriggersSpellCasts = false,
            NotSingleTargetSpell = true
        };

        IParticle recallParticle;

        public void OnSpellChannel(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            recallParticle = AddParticleTarget(owner, owner, "TeleportHome", owner, 8.0f, flags: 0);
            AddBuff("Recall", 7.9f, 1, spell, owner, owner);
            owner.IconInfo.ChangeBorder("Recall", "recall");
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
            var owner = spell.CastInfo.Owner;
            recallParticle.SetToRemove();
            RemoveBuff(owner, "Recall");
            owner.IconInfo.ResetBorder();
        }

        public void OnSpellPostChannel(ISpell spell)
        {
            var owner = spell.CastInfo.Owner as IChampion;
            owner.Recall();
            AddParticleTarget(owner, owner, "TeleportArrive", owner, flags: 0);
            owner.IconInfo.ResetBorder();
        }
    }
}

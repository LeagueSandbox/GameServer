using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class SionPassiveSpeed : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnSpellCast(Spell spell)
        {
            AddBuff("SionPassiveSpeed", 1.5f, 1, spell, spell.CastInfo.Owner, spell.CastInfo.Owner);
        }
    }
}


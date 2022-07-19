using GameServerCore.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public class SpellScriptEmpty : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata();
    }
}

using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public class BuffScriptEmpty : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING,
            MaxStacks = 0
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();
    }
}

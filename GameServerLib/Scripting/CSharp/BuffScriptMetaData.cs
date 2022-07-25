using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public class BuffScriptMetaData
    {
        public BuffType BuffType { get; set; } = BuffType.INTERNAL;
        public BuffAddType BuffAddType { get; set; } = BuffAddType.RENEW_EXISTING;
        public int MaxStacks { get; set; } = 1;
        public bool IsHidden { get; set; } = false;
    }
}

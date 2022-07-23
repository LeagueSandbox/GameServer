using GameServerCore.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public class AIScriptMetaData
    {
        public byte BehaviorTree { get; set; } = 0;
        public uint MinionRoamState { get; set; } = 0;
        public bool HandlesCallsForHelp { get; set; } = false;
    }
}
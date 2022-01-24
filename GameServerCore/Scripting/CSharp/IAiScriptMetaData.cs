namespace GameServerCore.Scripting.CSharp
{
    public interface IAiScriptMetaData
    {
        public byte BehaviorTree { get; }
        public uint MinionRoamState { get; }
    }
}
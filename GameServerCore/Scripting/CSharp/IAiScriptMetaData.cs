namespace GameServerCore.Scripting.CSharp
{
    public interface IAIScriptMetaData
    {
        public byte BehaviorTree { get; }
        public uint MinionRoamState { get; }
    }
}
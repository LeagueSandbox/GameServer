namespace GameServerCore.Scripting.CSharp
{
    public interface IAIScriptMetaData
    {
        byte BehaviorTree { get; }
        uint MinionRoamState { get; }
        bool HandlesCallsForHelp { get; }
    }
}
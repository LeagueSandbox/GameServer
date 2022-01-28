namespace GameServerCore.Scripting.CSharp
{
    public interface IAIScriptMetaData
    {
        byte BehaviorTree { get; }
        uint MinionRoamState { get; }
        /// AIScript should set this to true if it can handle incoming calls for help.
        /// To have callers populate a list of units attacking allies.
        /// </summary>
        bool HandlesCallsForHelp { get; }        
    }
}
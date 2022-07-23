namespace GameServerCore.Scripting.CSharp
{
    public class EventSource: IEventSource
    {
        public uint ScriptNameHash { get; private set; }
        public IEventSource ParentScript { get; private set; }
        public EventSource(uint hash, IEventSource parent)
        {
            ScriptNameHash = hash;
            ParentScript = parent;
        }
        public EventSource(uint hash, uint parentHash)
        {
            ScriptNameHash = hash;
            ParentScript = new EventSource(parentHash, null);
        }
    }
}
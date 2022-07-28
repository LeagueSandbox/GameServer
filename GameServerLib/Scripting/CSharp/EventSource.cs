using GameServerCore.Enums;

namespace GameServerCore.Scripting.CSharp
{
    public class AbilityInfo : IEventSource
    {
        public uint ScriptNameHash { get; private set; }
        public EventSource EventSource;
        public float PhysicalDamage { get; private set; }
        public float MagicalDamage { get; private set; }
        public float TrueDamage { get; private set; }
        public float TotalDamage => PhysicalDamage + MagicalDamage + TrueDamage;
        public float Heal { get; private set; }
        public int Priority { get; private set; }
        public int Count { get; private set; }

        public IEventSource ParentScript { get; private set; }
        public AbilityInfo(uint hash, IEventSource parent)
        {
            ScriptNameHash = hash;
            ParentScript = parent;
        }
        public AbilityInfo(uint hash, uint parentHash)
        {
            ScriptNameHash = hash;
            ParentScript = new AbilityInfo(parentHash, null);
        }
    }
}
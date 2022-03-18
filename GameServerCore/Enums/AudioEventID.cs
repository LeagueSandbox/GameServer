using System;
namespace GameServerCore.Enums
{
    public struct AudioEventID
    {
        public uint ID { get; private set; }
        public static explicit operator AudioEventID(uint id) => new AudioEventID { ID = id };
        public static explicit operator uint(AudioEventID id) => id.ID;
        public static bool operator ==(AudioEventID a, AudioEventID b) => a.ID == b.ID;
        public static bool operator !=(AudioEventID a, AudioEventID b) => !(a == b);
        public override bool Equals(Object obj) => (obj is AudioEventID b) && this == b;
        public override int GetHashCode() => ID.GetHashCode();
    }
}

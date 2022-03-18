using System;
namespace GameServerCore.Enums
{
    public struct MusicID
    {
        public uint ID { get; private set; }
        public static explicit operator MusicID(uint id) => new MusicID { ID = id };
        public static explicit operator uint(MusicID id) => id.ID;
        public static bool operator ==(MusicID a, MusicID b) => a.ID == b.ID;
        public static bool operator !=(MusicID a, MusicID b) => !(a == b);
        public override bool Equals(Object obj) => (obj is MusicID b) && this == b;
        public override int GetHashCode() => ID.GetHashCode();
    }
}

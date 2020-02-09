using System.IO;
using System.Numerics;

namespace GameServerLib.Extensions
{
    public static class BinaryReaderExtensions
    {
        public static Vector2 ReadVector2(this BinaryReader br)
        {
            return new Vector2(br.ReadSingle(), br.ReadSingle());
        }
        public static Vector3 ReadVector3(this BinaryReader br)
        {
            return new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }
    }
}
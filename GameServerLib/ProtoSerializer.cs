using System.IO;

namespace LeagueSandbox.GameServer
{
    public class ProtoSerializer
    {
        public static byte[] Serialize<T>(T obj)
        {
            using (var stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize<T>(stream, obj);
                return stream.ToArray();
            }
        }
        public static void Serialize<T>(Stream stream, T obj)
        {
            ProtoBuf.Serializer.Serialize<T>(stream, obj);
        }
        public static T Deserialize<T>(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, 0, bytes.Length))
            {
                return ProtoBuf.Serializer.Deserialize<T>(stream);
            }
        }
        public static T Deserialize<T>(Stream stream)
        {
            return ProtoBuf.Serializer.Deserialize<T>(stream);
        }
    }
}
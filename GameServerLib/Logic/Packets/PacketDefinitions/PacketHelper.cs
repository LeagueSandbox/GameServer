using System;
using System.Linq;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public static class PacketHelper
    {
        public static byte[] intToByteArray(int i)
        {
            var ret = BitConverter.GetBytes(i);
            if (BitConverter.IsLittleEndian)
                return ret.Reverse().ToArray();
            return ret;
        }
    }
}
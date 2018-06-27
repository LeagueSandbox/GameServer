using System;
using System.Linq;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions
{
    public static class PacketHelper
    {
        public static byte[] IntToByteArray(int i)
        {
            var ret = BitConverter.GetBytes(i);
            if (BitConverter.IsLittleEndian)
            {
                return ret.Reverse().ToArray();
            }

            return ret;
        }
    }
}
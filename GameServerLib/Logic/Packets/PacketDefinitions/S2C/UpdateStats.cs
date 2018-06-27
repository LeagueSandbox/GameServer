using System;
using System.IO;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class UpdateStats : BasePacket
    {
        public UpdateStats(Replication r, bool partial = true)
            : base(PacketCmd.PKT_S2_C_CHAR_STATS)
        {
            _buffer.Write(Environment.TickCount); // syncID
            _buffer.Write((byte)1); // updating 1 unit

            var values = r.Values;

            uint masterMask = 0;
            for (var i = 0; i < 6; i++)
            {
                for (var j = 0; j < 32; j++)
                {
                    var rep = values[i, j];
                    if (rep == null || (!rep.Changed && partial))
                    {
                        continue;
                    }

                    masterMask |= 1u << i;
                    break;
                }
            }

            _buffer.Write((byte)masterMask);
            _buffer.Write(r.NetId);

            for (var i = 0; i < 6; i++)
            {
                uint fieldMask = 0;
                var stream = new MemoryStream();
                var writer = new BinaryWriter(stream);
                for (var j = 0; j < 32; j++)
                {
                    var rep = values[i, j];
                    if (rep == null || (!rep.Changed && partial))
                    {
                        continue;
                    }

                    fieldMask |= 1u << j;
                    if (rep.IsFloat)
                    {
                        var source = BitConverter.GetBytes(rep.Value);

                        if (source[0] >= 0xFE)
                        {
                            writer.Write((byte)0xFE);
                        }

                        writer.Write(source);
                    }
                    else
                    {
                        var num = rep.Value;
                        while (num >= 0x80)
                        {
                            writer.Write((byte)(num | 0x80));
                            num >>= 7;
                        }

                        writer.Write((byte)num);
                    }
                }

                var data = stream.ToArray();
                if (data.Length > 0)
                {
                    _buffer.Write(fieldMask);
                    _buffer.Write((byte)data.Length);
                    _buffer.Write(data);
                }
            }
        }
    }
}
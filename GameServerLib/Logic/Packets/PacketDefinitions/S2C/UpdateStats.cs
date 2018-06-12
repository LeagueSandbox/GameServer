using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class UpdateStats : BasePacket
    {
        public UpdateStats(AttackableUnit u, bool partial = true)
            : base(PacketCmd.PKT_S2C_CharStats)
        {
            buffer.Write(Environment.TickCount); // syncID
            buffer.Write((byte)1); // updating 1 unit

            var values = u.ReplicationManager.Values;

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

            buffer.Write((byte)masterMask);
            buffer.Write((uint)u.NetId);

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
                        uint num = rep.Value;
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
                    buffer.Write(fieldMask);
                    buffer.Write((byte)data.Length);
                    buffer.Write(data);
                }
            }

            if (partial)
            {
                foreach (var x in values)
                {
                    if (x != null)
                    {
                        x.Changed = false;
                    }
                }

                u.ReplicationManager.Changed = false;
            }
        }
    }
}
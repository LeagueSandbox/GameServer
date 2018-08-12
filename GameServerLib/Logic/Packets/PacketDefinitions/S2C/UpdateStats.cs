using System;
using System.Collections.Generic;
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
            var stats = new Dictionary<MasterMask, Dictionary<FieldMask, float>>();

            if (partial)
                stats = u.GetStats().GetUpdatedStats();
            else
                stats = u.GetStats().GetAllStats();
            var orderedStats = stats.OrderBy(x => x.Key);

            buffer.Write(Environment.TickCount); // syncID
            buffer.Write((byte)1); // updating 1 unit

            byte masterMask = 0;
            foreach (var p in orderedStats)
                masterMask |= (byte)p.Key;

            buffer.Write((byte)masterMask);
            buffer.Write((uint)u.NetId);

            foreach (var group in orderedStats)
            {
                var orderedGroup = group.Value.OrderBy(x => x.Key);
                uint fieldMask = 0;
                byte size = 0;
                foreach (var stat in orderedGroup)
                {
                    fieldMask |= (uint)stat.Key;
                    size += u.GetStats().getSize(group.Key, stat.Key);
                }
                buffer.Write((uint)fieldMask);
                buffer.Write((byte)size);
                foreach (var stat in orderedGroup)
                {
                    size = u.GetStats().getSize(group.Key, stat.Key);
                    switch (size)
                    {
                        case 1:
                            buffer.Write((byte)Convert.ToByte(stat.Value));
                            break;
                        case 2:
                            buffer.Write((short)Convert.ToInt16(stat.Value));
                            break;
                        case 4:
                            var bytes = BitConverter.GetBytes(stat.Value);
                            if (bytes[0] >= 0xFE)
                                bytes[0] = 0xFD;
                            buffer.Write((float)BitConverter.ToSingle(bytes, 0));
                            break;
                    }
                }
            }
        }
    }
}
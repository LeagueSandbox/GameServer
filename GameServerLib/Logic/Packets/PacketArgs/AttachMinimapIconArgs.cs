using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.GameObjects;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct AttachMinimapIconArgs
    {
        public uint UnitNetId { get; }
        public byte Unk1 { get; }
        public string IconName { get; }
        public byte Unk2 { get; }
        public string Unk3 { get; }
        public string Unk4 { get; }

        public AttachMinimapIconArgs(uint unitNetId, byte unk1, string iconName, byte unk2, string unk3, string unk4)
        {
            UnitNetId = unitNetId;
            Unk1 = unk1;
            IconName = iconName;
            Unk2 = unk2;
            Unk3 = unk3;
            Unk4 = unk4;
        }
    }
}

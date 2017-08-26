using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.Providers
{
    public interface IClientPacketProvider
    {
        ClientPacketBase ProvideClientPacket(IPacketHandler handler, byte[] data);
    }
}

using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class AttentionPingAns : BasePacket
    {
        public AttentionPingAns(ClientInfo player, AttentionPing ping) : base(PacketCmd.PKT_S2C_AttentionPing)
        {
            buffer.Write((float)ping.x);
            buffer.Write((float)ping.y);
            buffer.Write((int)ping.targetNetId);
            buffer.Write((int)player.Champion.NetId);
            buffer.Write((byte)ping.type);
            buffer.Write((byte)0xFB); // 4.18
            /*
                                      switch (ping.type)
                                      {
                                         case 0:
                                            buffer.Write((short)0xb0;
                                            break;
                                         case 1:
                                            buffer.Write((short)0xb1;
                                            break;
                                         case 2:
                                            buffer.Write((short)0xb2; // Danger
                                            break;
                                         case 3:
                                            buffer.Write((short)0xb3; // Enemy Missing
                                            break;
                                         case 4:
                                            buffer.Write((short)0xb4; // On My Way
                                            break;
                                         case 5:
                                            buffer.Write((short)0xb5; // Retreat / Fall Back
                                            break;
                                         case 6:
                                            buffer.Write((short)0xb6; // Assistance Needed
                                            break;
                                      }
                                      */
        }
    }
}
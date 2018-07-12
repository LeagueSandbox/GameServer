using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleEmotion : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_EMOTION;
        public override Channel PacketChannel => Channel.CHL_C2_S;
        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var emotion = new EmotionPacketRequest(data);
            //for later use -> tracking, etc.
            var playerName = PlayerManager.GetPeerInfo(peer).Champion.Model;
            switch (emotion.Id)
            {
                case (byte)Emotions.DANCE:
                    Logger.LogCoreInfo("Player " + playerName + " is dancing.");
                    break;
                case (byte)Emotions.TAUNT:
                    Logger.LogCoreInfo("Player " + playerName + " is taunting.");
                    break;
                case (byte)Emotions.LAUGH:
                    Logger.LogCoreInfo("Player " + playerName + " is laughing.");
                    break;
                case (byte)Emotions.JOKE:
                    Logger.LogCoreInfo("Player " + playerName + " is joking.");
                    break;
            }

            var response = new EmotionPacketResponse(emotion.Id, emotion.NetId);
            return Game.PacketHandlerManager.BroadcastPacket(response, Channel.CHL_S2_C);
        }
    }

    //TODO: move to separate file
    public enum Emotions : byte
    {
        DANCE = 0,
        TAUNT = 1,
        LAUGH = 2,
        JOKE = 3
    }
}

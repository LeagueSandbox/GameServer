using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleEmotion : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var emotion = new EmotionPacket(data);
            //for later use -> tracking, etc.
            switch (emotion.id)
            {
                case 0:
                    //dance
                    //Logging->writeLine("dance");
                    break;
                case 1:
                    //taunt
                    //Logging->writeLine("taunt");
                    break;
                case 2:
                    //laugh
                    //Logging->writeLine("laugh");
                    break;
                case 3:
                    //joke
                    //Logging->writeLine("joke");
                    break;
            }
            var response = new EmotionPacket(emotion.id, emotion.netId);
            return _game.PacketHandlerManager.broadcastPacket(response, Channel.CHL_S2C);
        }
    }
}

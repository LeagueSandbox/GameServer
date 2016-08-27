using ENet;
using System;
using System.Threading;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.GameObjects;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class RainbowCommand : ChatCommand
    {
        public RainbowCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        private static Champion me;
        private static ChatboxManager owner;

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            me = _owner.GetGame().GetPeerInfo(peer).GetChampion();
            owner = _owner;
            Task.Run(() => RAINBOW());
        }

        public void RAINBOW()
        {
            while (true)
            {
                try
                {
                    //sending the actual Thread of execution to sleep X milliseconds
                    float a = 0.5f;
                    byte[] rainBow = new byte[4];
                    Random random = new Random();
                    random.NextBytes(rainBow);
                    Thread.Sleep(500);
                    SetScreenTint s1 = new SetScreenTint(me, false, 0.0f, 0, 0, 0, 1f);
                    owner.GetGame().PacketHandlerManager.broadcastPacket(s1, Core.Logic.PacketHandlers.Channel.CHL_S2C);
                    SetScreenTint sst = new SetScreenTint(me, true, 0.6f, rainBow[1], rainBow[2], rainBow[3], a);
                    owner.GetGame().PacketNotifier.notifyDebugMessage("R: " + rainBow[1] + " G: " + rainBow[2] + " B: " + rainBow[3]);
                    owner.GetGame().PacketHandlerManager.broadcastPacket(sst, Core.Logic.PacketHandlers.Channel.CHL_S2C);
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}
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
        private static bool run = false;
        private static float a = 0.5f;
        private static float speed = 0.25f;
        private static int delay = 250;

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            me = _owner.GetGame().GetPeerInfo(peer).GetChampion();
            owner = _owner;

            if (split.Length > 1)
            {
                float.TryParse(split[1], out a);
            }

            if(split.Length > 2)
            {
                speed = float.Parse(split[2]);
                delay = (int)(speed * 1000);
            }

            if (!run)
            {
                run = true;
                Task.Run(() => rainbow());
            }
            else
            {
                run = false;
                SetScreenTint tintOff = new SetScreenTint(me, false, 0.0f, 0, 0, 0, 1f);
                owner.GetGame().PacketHandlerManager.broadcastPacket(tintOff, Core.Logic.PacketHandlers.Channel.CHL_S2C);
            }
        }

        public void rainbow()
        {
            while (run)
            {
                try
                {
                    byte[] rainbow = new byte[4];
                    new Random().NextBytes(rainbow);
                    Thread.Sleep(delay);
                    SetScreenTint tintOff = new SetScreenTint(me, false, 0.0f, 0, 0, 0, 1f);
                    owner.GetGame().PacketHandlerManager.broadcastPacket(tintOff, Core.Logic.PacketHandlers.Channel.CHL_S2C);
                    if (run)
                    {
                        SetScreenTint tintOn = new SetScreenTint(me, true, speed, rainbow[1], rainbow[2], rainbow[3], a);
                        owner.GetGame().PacketHandlerManager.broadcastPacket(tintOn, Core.Logic.PacketHandlers.Channel.CHL_S2C);
                    }
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}

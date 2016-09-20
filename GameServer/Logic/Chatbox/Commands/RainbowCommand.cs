using ENet;
using System;
using System.Threading;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.GameObjects;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Players;
using LeagueSandbox.GameServer.Logic.Enet;

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
            PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

            var split = arguments.ToLower().Split(' ');

            me = _playerManager.GetPeerInfo(peer).Champion;
            owner = _owner;

            if (split.Length > 1)
            {
                float.TryParse(split[1], out a);
            }

            if(split.Length > 2)
            {
                float.TryParse(split[2], out speed);
                delay = (int)(speed * 1000);
            }

            run = !run;
            if (run)
            {
                Task.Run(() => TaskRainbow());
            }
        }

        public void TaskRainbow()
        {
            while (run)
            {
                byte[] rainbow = new byte[4];
                new Random().NextBytes(rainbow);
                Thread.Sleep(delay);
                BroadcastTint(me.Team, false, 0.0f, 0, 0, 0, 1f);
                BroadcastTint(me.Team, true, speed, rainbow[1], rainbow[2], rainbow[3], a);
            }
            Thread.Sleep(delay);
            BroadcastTint(me.Team, false, 0.0f, 0, 0, 0, 1f);
        }

        public void BroadcastTint(TeamId team, bool enable, float speed, byte r, byte g, byte b, float a)
        {
            Game _game = Program.ResolveDependency<Game>();
            var tint = new SetScreenTint(team, enable, speed, r, g, b, a);
            _game.PacketHandlerManager.broadcastPacket(tint, Core.Logic.PacketHandlers.Channel.CHL_S2C);
        }
    }
}

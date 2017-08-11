using ENet;
using System;
using System.Threading;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Players;
using LeagueSandbox.GameServer.Logic.Enet;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class RainbowCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        private Champion _me;
        private bool _run = false;
        private float _a = 0.5f;
        private float _speed = 0.25f;
        private int _delay = 250;

        public override string Command => $"rainbow";
        public override string Syntax => $"{Command} alpha speed";

        public RainbowCommand(ChatCommandManager chatCommandManager, PlayerManager playerManager) : base(chatCommandManager)
        {
            _playerManager = playerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            _me = _playerManager.GetPeerInfo(peer).Champion;

            if (split.Length > 1)
            {
                float.TryParse(split[1], out _a);
            }

            if (split.Length > 2)
            {
                float.TryParse(split[2], out _speed);
                _delay = (int)(_speed * 1000);
            }

            _run = !_run;
            if (_run)
            {
                Task.Run(() => TaskRainbow());
            }
        }

        public void TaskRainbow()
        {
            while (_run)
            {
                byte[] rainbow = new byte[4];
                new Random().NextBytes(rainbow);
                Thread.Sleep(_delay);
                BroadcastTint(_me.Team, false, 0.0f, 0, 0, 0, 1f);
                BroadcastTint(_me.Team, true, _speed, rainbow[1], rainbow[2], rainbow[3], _a);
            }
            Thread.Sleep(_delay);
            BroadcastTint(_me.Team, false, 0.0f, 0, 0, 0, 1f);
        }

        public void BroadcastTint(TeamId team, bool enable, float speed, byte r, byte g, byte b, float a)
        {
            Game _game = Program.ResolveDependency<Game>();
            var tint = new SetScreenTint(team, enable, speed, r, g, b, a);
            _game.PacketHandlerManager.broadcastPacket(tint, Core.Logic.PacketHandlers.Channel.CHL_S2C);
        }
    }
}

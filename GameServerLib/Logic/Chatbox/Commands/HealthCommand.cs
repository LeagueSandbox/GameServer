﻿using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class HealthCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "health";
        public override string Syntax => $"{Command} maxHealth";

        public HealthCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.GetPlayerManager();
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (float.TryParse(split[1], out var hp))
            {
                _playerManager.GetPeerInfo(peer).Champion.Stats.HealthPoints.FlatBonus += hp;
                _playerManager.GetPeerInfo(peer).Champion.Stats.CurrentHealth += hp;
            }
        }
    }
}

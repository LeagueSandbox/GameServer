using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class SkillpointsCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;
        private readonly IPacketNotifier _packetNotifier;

        public override string Command => "skillpoints";
        public override string Syntax => $"{Command}";

        public SkillpointsCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
            _packetNotifier = game.PacketNotifier;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var champion = _playerManager.GetPeerInfo(peer).Champion;
            champion.SetSkillPoints(17);

            _packetNotifier.NotifySkillUp(peer, champion.NetId, 0, 0, 17);
        }
    }
}

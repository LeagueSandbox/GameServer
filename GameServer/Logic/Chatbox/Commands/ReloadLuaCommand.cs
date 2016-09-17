using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class ReloadLuaCommand : ChatCommand
    {
        public ReloadLuaCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();
            var champ = _playerManager.GetPeerInfo(peer).Champion;
            foreach (var spell in champ.Spells)
            {
                spell.ReloadLua();
            }
            champ.LoadLua();
            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "Successfully reloaded luas for your champion!");
        }
    }
}

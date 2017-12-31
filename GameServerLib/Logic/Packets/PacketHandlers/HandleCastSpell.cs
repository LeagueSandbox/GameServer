using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleCastSpell : PacketHandlerBase<CastSpellRequest>
    {
        private readonly Game _game;
        private readonly NetworkIdManager _networkIdManager;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CastSpell;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleCastSpell(Game game, NetworkIdManager networkIdManager, PlayerManager playerManager)
        {
            _game = game;
            _networkIdManager = networkIdManager;
            _playerManager = playerManager;
        }

        public override bool HandlePacketInternal(Peer peer, CastSpellRequest data)
        {
            var targetObj = _game.ObjectManager.GetObjectById(data.TargetNetId);
            var TargetUnit = targetObj as Unit;
            var owner = _playerManager.GetPeerInfo(peer).Champion;
            if (owner == null)
            {
                return false;
            }
            if (!owner.CanCast())
            {
                return false;
            }

            var s = owner.GetSpell(data.SpellSlot);
            if (s == null)
            {
                return false;
            }
            return s.cast(data.X, data.Y, data.X2, data.Y2, TargetUnit);
        }
    }
}

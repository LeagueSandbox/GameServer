using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleCastSpell : PacketHandlerBase
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

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var spell = new CastSpell(data);

            var targetObj = _game.ObjectManager.GetObjectById(spell.targetNetId);
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
            var s = owner.GetSpell(spell.spellSlot);
            if (s == null)
            {
                return false;
            }
            return s.cast(spell.x, spell.y, spell.x2, spell.y2, TargetUnit);
        }
    }
}

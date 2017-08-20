using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Players;
using LeagueSandbox.GameServer.Logic.API;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleCastSpell : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private NetworkIdManager _networkIdManager = Program.ResolveDependency<NetworkIdManager>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
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

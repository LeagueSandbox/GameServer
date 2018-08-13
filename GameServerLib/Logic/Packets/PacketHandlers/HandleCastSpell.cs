using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.Spells;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleCastSpell : PacketHandlerBase
    {
        private readonly IPacketReader _packetReader;
        private readonly Game _game;
        private readonly NetworkIdManager _networkIdManager;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CAST_SPELL;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleCastSpell(Game game)
        {
            _packetReader = game.PacketReader;
            _game = game;
            _networkIdManager = game.NetworkIdManager;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _packetReader.ReadCastSpellRequest(data);
            var targetObj = _game.ObjectManager.GetObjectById(request.TargetNetId);
            var targetUnit = targetObj as AttackableUnit;
            var owner = _playerManager.GetPeerInfo(peer).Champion;
            if (owner == null || !owner.CanCast())
            {
                return false;
            }

            var s = owner.GetSpell(request.SpellSlot) as Spell;
            if (s == null)
            {
                return false;
            }

            return s.Cast(request.X, request.Y, request.X2, request.Y2, targetUnit);
        }
    }
}

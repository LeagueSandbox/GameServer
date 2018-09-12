using GameServerCore;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.Spells;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleCastSpell : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly NetworkIdManager _networkIdManager;
        private readonly IPlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CAST_SPELL;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleCastSpell(Game game)
        {
            _game = game;
            _networkIdManager = game.NetworkIdManager;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, byte[] data)
        {
            var request = _game.PacketReader.ReadCastSpellRequest(data);
            var targetObj = _game.ObjectManager.GetObjectById(request.TargetNetId);
            var targetUnit = targetObj as AttackableUnit;
            var owner = _playerManager.GetPeerInfo(userId).Champion;
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

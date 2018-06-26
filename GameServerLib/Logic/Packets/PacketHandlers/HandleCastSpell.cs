using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleCastSpell : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly NetworkIdManager _networkIdManager;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2_S_CAST_SPELL;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public HandleCastSpell(Game game, NetworkIdManager networkIdManager, PlayerManager playerManager)
        {
            _game = game;
            _networkIdManager = networkIdManager;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var spell = new CastSpellRequest(data);

            var targetObj = _game.ObjectManager.GetObjectById(spell.TargetNetId);
            var targetUnit = targetObj as AttackableUnit;
            var owner = _playerManager.GetPeerInfo(peer).Champion;
            if (owner == null)
            {
                return false;
            }
            if (!owner.CanCast())
            {
                return false;
            }
            var s = owner.GetSpell(spell.SpellSlot);
            if (s == null)
            {
                return false;
            }
            return s.Cast(spell.X, spell.Y, spell.X2, spell.Y2, targetUnit);
        }
    }
}

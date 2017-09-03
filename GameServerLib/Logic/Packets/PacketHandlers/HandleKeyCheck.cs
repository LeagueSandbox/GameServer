using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;
using System;
using System.Globalization;
using System.Threading;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleKeyCheck : PacketHandlerBase
    {
        private readonly Logger _logger;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_KeyCheck;
        public override Channel PacketChannel => Channel.CHL_HANDSHAKE;

        public HandleKeyCheck(Logger logger, Game game, PlayerManager playerManager)
        {
            _logger = logger;
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var keyCheck = new KeyCheckRequest(data);
            var userId = _game.Blowfish.Decrypt(keyCheck.checkId);
            _logger.LogCoreInfo("new player with id " + userId + " connected.");

            if (userId != keyCheck.userId)
                return false;

            var playerNo = 0;

            foreach (var p in _playerManager.GetPlayers())
            {
                var player = p.Item2;
                if (player.UserId == userId)
                {
                    if (player.Peer != null)
                    {
                        if (!player.IsDisconnected)
                        {
                            _logger.LogCoreWarning("Ignoring new player " + userId + ", already connected!");
                            return false;
                        }
                    }
                    //TODO: add at least port or smth
                    p.Item1 = peer.Address.port;
                    player.Peer = peer;
                    var response = new KeyCheckResponse(keyCheck.userId, playerNo);
                    _game.PacketHandlerManager.sendPacket(peer, response, Channel.CHL_HANDSHAKE);
                    var _string = "23010000000023265794D75B5BD55765D3513BD6E0D5E0200505050505050505050505AB";
                    _game.PacketHandlerManager.sendPacket(peer, ConvertHexStringToByteArray(_string), Channel.CHL_HANDSHAKE);

                    return true;
                }
                ++playerNo;
            }
            return false;
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return HexAsBytes;
        }
    }
}
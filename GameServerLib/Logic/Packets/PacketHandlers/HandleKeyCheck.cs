using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleKeyCheck : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_KEY_CHECK;
        public override Channel PacketChannel => Channel.CHL_HANDSHAKE;
        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var keyCheck = new KeyCheckRequest(data);
            var userId = Game.Blowfish.Decrypt(keyCheck.CheckId);

            if (userId != keyCheck.UserId)
            {
                Logger.LogCoreWarning("Client has sent wrong blowfish data.");
                return false;
            }

            if (keyCheck.VersionNo != Config.VERSION_NUMBER)
            {
                Logger.LogCoreWarning("Client version doesn't match server's. " +
                                       $"(C:{keyCheck.VersionNo}, S:{Config.VERSION_NUMBER})");
                return false;
            }

            var playerNo = 0;

            foreach (var p in PlayerManager.GetPlayers())
            {
                var player = p.Item2;
                if (player.UserId == userId)
                {
                    if (player.Peer != null)
                    {
                        if (!player.IsDisconnected)
                        {
                            Logger.LogCoreWarning($"Ignoring new player {userId}, already connected!");
                            return false;
                        }
                    }

                    //TODO: add at least port or smth
                    p.Item1 = peer.Address.port;
                    player.Peer = peer;
                    player.PlayerNo = playerNo;
                    var response = new KeyCheckResponse(keyCheck.UserId, playerNo);
                    Game.PacketHandlerManager.BroadcastPacket(response, Channel.CHL_HANDSHAKE);


                    foreach(var p2 in PlayerManager.GetPlayers())
                    {
                        if (p2.Item2.Peer != null && p2.Item2.UserId != player.UserId)
                        {
                            var response2 = new KeyCheckResponse(p2.Item2.UserId, p2.Item2.PlayerNo);
                            Game.PacketHandlerManager.SendPacket(player.Peer, response2, Channel.CHL_HANDSHAKE);
                        }
                    }

                    return true;
                }
                ++playerNo;
            }
            return false;
        }
    }
}

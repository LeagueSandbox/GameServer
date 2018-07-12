using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSync : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SYNCH_VERSION;
        public override Channel PacketChannel => Channel.CHL_C2_S;
        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var version = new SynchVersionRequest(data);
            //Logging->writeLine("Client version: %s", version->version);

            var mapId = Game.Config.GameConfig.Map;
            Logger.LogCoreInfo("Current map: " + mapId);

            var versionMatch = true;
            // Version might be an invalid value, currently it trusts the client
            if (version.Version != Config.VERSION_STRING)
            {
                versionMatch = false;
                Logger.LogCoreWarning($"Client's version ({version.Version}) does not match server's {Config.VERSION}");
            }
            else
            {
                Logger.LogCoreInfo("Accepted client version (" + version.Version + ")");
            }

            foreach (var player in PlayerManager.GetPlayers())
            {
                if (player.Item1 == peer.Address.port)
                {
                    player.Item2.IsMatchingVersion = versionMatch;
                    break;
                }
            }
            var answer = new SynchVersionResponse(PlayerManager.GetPlayers(), Config.VERSION_STRING, "CLASSIC", mapId);

            return Game.PacketHandlerManager.SendPacket(peer, answer, Channel.CHL_S2_C);
        }
    }
}

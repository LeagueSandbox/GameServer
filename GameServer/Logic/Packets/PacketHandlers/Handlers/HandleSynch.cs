using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSynch : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var version = new SynchVersion(data);
            //Logging->writeLine("Client version: %s", version->version);

            var mapId = Config.gameConfig.map;
            Logger.LogCoreInfo("Current map: " + mapId);

            bool versionMatch = true;
            // Version might be an invalid value, currently it trusts the client
            if (version.version != Config.version)
            {
                versionMatch = false;
                Logger.LogCoreWarning("Client " + version.version + " does not match Server " + Config.version);
            }
            else
            {
                Logger.LogCoreInfo("Accepted client version (" + version.version + ")");
            }

            foreach (var player in game.getPlayers())
            {
                if (player.Item1 == peer.Address.port)
                {
                    player.Item2.setVersionMatch(versionMatch);
                    break;
                }
            }
            var answer = new SynchVersionAns(game.getPlayers(), Config.version, "CLASSIC", mapId);

            return PacketHandlerManager.getInstace().sendPacket(peer, answer, Channel.CHL_S2C);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BlowFishCS;
using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Handlers;
using LeagueSandbox.GameServer.Logic.Players;
using System.Globalization;
using System.Threading;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class PacketHandlerManager
    {
        private readonly Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>> _handlerTable;
        private readonly List<TeamId> _teamsEnumerator;
        private readonly Logger _logger;
        private readonly BlowFish _blowfish;
        private readonly Host _server;
        private readonly PlayerManager _playerManager;
        private readonly IHandlersProvider _packetHandlerProvider;
        private bool firstLoadingScreenResponse = false;

        public PacketHandlerManager(Logger logger, BlowFish blowfish, Host server, PlayerManager playerManager,
            IHandlersProvider handlersProvider)
        {
            _logger = logger;
            _blowfish = blowfish;
            _server = server;
            _playerManager = playerManager;
            _packetHandlerProvider = handlersProvider;
            _teamsEnumerator = Enum.GetValues(typeof(TeamId)).Cast<TeamId>().ToList();

            var loadFrom = new[] { ServerLibAssemblyDefiningType.Assembly };
            _handlerTable = _packetHandlerProvider.GetAllPacketHandlers(loadFrom);
        }

        internal IPacketHandler GetHandler(PacketCmd cmd, Channel channelID)
        {
            var game = Program.ResolveDependency<Game>();
            var packetsHandledWhilePaused = new List<PacketCmd>
            {
                PacketCmd.PKT_UnpauseGame,
                PacketCmd.PKT_C2S_CharLoaded,
                PacketCmd.PKT_C2S_Click,
                PacketCmd.PKT_C2S_ClientReady,
                PacketCmd.PKT_C2S_Exit,
                PacketCmd.PKT_C2S_HeartBeat,
                PacketCmd.PKT_C2S_QueryStatusReq,
                PacketCmd.PKT_C2S_StartGame,
                PacketCmd.PKT_C2S_World_SendGameNumber,
                PacketCmd.PKT_ChatBoxMessage,
                PacketCmd.PKT_KeyCheck
            };
            if (game.IsPaused && !packetsHandledWhilePaused.Contains(cmd))
            {
                return null;
            }
            if (_handlerTable.ContainsKey(cmd))
            {
                var handlers = _handlerTable[cmd];
                if (handlers.ContainsKey(channelID))
                    return handlers[channelID];
            }
            return null;
        }
        public bool sendPacket(Peer peer, GameServer.Logic.Packets.Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return sendPacket(peer, packet.GetBytes(), channelNo, flag);
        }

        private IntPtr allocMemory(byte[] data)
        {
            var unmanagedPointer = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, unmanagedPointer, data.Length);
            return unmanagedPointer;
        }

        private void releaseMemory(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }

        public void printPacket(byte[] buffer, string str)
        {
            //string hex = BitConverter.ToString(buffer);
            // System.Diagnostics.Debug.WriteLine(str + hex.Replace("-", " "));
            lock (Program.ExecutingDirectory)
            {
                Console.Write(str);
                foreach (var b in buffer)
                    Console.Write(b.ToString("X2") + " ");

                Console.WriteLine("");
                Console.WriteLine("--------");
            }
        }
        public bool sendPacket(Peer peer, byte[] source, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            ////PDEBUG_LOG_LINE(Logging," Sending packet:\n");
            //if(length < 300)
            printPacket(source, "Sent: ");
            byte[] temp;
            if (source.Length >= 8)
                temp = _blowfish.Encrypt(source);
            else
                temp = source;

            return peer.Send((byte)channelNo, temp);
        }

        public bool broadcastPacket(byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable)
        {
            ////PDEBUG_LOG_LINE(Logging," Broadcast packet:\n");
            //printPacket(data, "Broadcast: ");
            byte[] temp;
            if (data.Length >= 8)
                temp = _blowfish.Encrypt(data);
            else
                temp = data;

            var packet = new ENet.Packet();
            packet.Create(temp);
            _server.Broadcast((byte)channelNo, ref packet);
            return true;
        }

        public bool broadcastPacket(GameServer.Logic.Packets.Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return broadcastPacket(packet.GetBytes(), channelNo, flag);
        }


        public bool broadcastPacketTeam(TeamId team, byte[] data, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            foreach (var ci in _playerManager.GetPlayers())
                if (ci.Item2.Peer != null && ci.Item2.Team == team)
                    sendPacket(ci.Item2.Peer, data, channelNo, flag);
            return true;
        }

        public bool broadcastPacketTeam(TeamId team, GameServer.Logic.Packets.Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return broadcastPacketTeam(team, packet.GetBytes(), channelNo, flag);
        }

        public bool broadcastPacketVision(GameObject o, GameServer.Logic.Packets.Packet packet, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            return broadcastPacketVision(o, packet.GetBytes(), channelNo, flag);
        }

        public bool broadcastPacketVision(GameObject o, byte[] data, Channel channelNo,
            PacketFlags flag = PacketFlags.Reliable)
        {
            var game = Program.ResolveDependency<Game>();
            foreach (var team in _teamsEnumerator)
            {
                if (team == TeamId.TEAM_NEUTRAL)
                    continue;

                if (game.ObjectManager.TeamHasVisionOn(team, o))
                {
                    broadcastPacketTeam(team, data, channelNo, flag);
                }
            }

            return true;
        }

        public bool handlePacket(Peer peer, byte[] data, Channel channelID)
        {
            var header = new GameServer.Logic.Packets.PacketHeader(data);
            var handler = GetHandler(header.cmd, channelID);

            switch (header.cmd)
            {
                case PacketCmd.PKT_C2S_StatsConfirm:
                case PacketCmd.PKT_C2S_MoveConfirm:
                case PacketCmd.PKT_C2S_ViewReq:
                    break;
            }
            printPacket(data, "Recv: ");
            if (header.cmd == PacketCmd.PKT_RESPONSE)
            {
                //WORLD INFO, like what mode, etc. etc.
                var x = "030000000000402612A5424BF9AAACF9B913AAF5ECF80B61F1BBA4F59F543B6930F52D10DC98F8F18C6EF0FDF5579C847C70F2C5628B9DABFA27095E8C87F92A4D4022F821E7C75770F2CAE340BB78F25707E7C5B3FADCCF8C85BEFA05E75785BEF926F92A36B3F421FFF503ABF84F18356E0FF384D121F20FF23BC2D7A1F41FB4180DB3F8FFFF3F81B3F8CF2A3F4730FA313D83F5CCF82D950B85B3F845DDD75478FB58A39396FDF336F140E178F4014D9C7D30F2DD62467C70F2F81FF39630FAC53DC618DFF9F12D628478F8BB54D9C40FFB904DEFA078F336B8AC090FF2E09C588334FA612DC6F5ABF563C5C43B87F9835785BDFA65508369FDF52A69136D70FBBBD99D6E30F4B4E3CAB8BEF93FC2E718ABF2E06469C530F51A33F2B6A4FB10368C3D0FFAE38B3AEDDFF451514645A4F261F971F9CCF3654FF99470F2F5AC4FD1AEF51AF62A94CCF34701BAE587F827643A11B3F85A6272C4870482FE556767A0FEEEFE6675F21901BAD775B55148F285A3015160D71521BC725F669EBD765A7E5A6ACE5A225A965A2F5A4F5A0F5A9E5A475F366757D71F5F3F62575A575A765A435A3F5A3F5A2F5ADE5A775A4F5A5A5AA293825A5A5A5A5A5A61CA03EA13BE7AFA7B915A665ABD5A5A825A6BCE5A2AE796EF2FF64F2F0F779E1E47EF361E570F1FF73F7E575F5767762F43373F2F3F472F27DE2777294F525A61A20382135A7A5AA25A5ACA5AEA5ABE82FAD3915A667EBD5F5A675A2FCE371A2F96472F5F4F2F0FE69EBF472F3626572F1F3F3F8257B857F3764243613F033F132F7ADE83775A4F5A5A5AA28282735A5A5A6E5A1ECA1EEAEFBEF7FA17917766DEBD2F5A2F5A1FCE5FF22F96E62FBF4F2F0F269E2F473F365A575A1F5A3F5E576157037613437A3F6B3F5A2F5ADE5A77824F825A5AA27E825F5A5F5A2F5AE6CABFEA2FBE26FA2F913F665A805A5A1C5A66CE91E2FA96BE2FEA4FCA0F5A9E5A475A368257A21F5A3F4F577757DE762F433F3F3F3F432F76DE5777574F3F5A1FA25782365A475A9E5A0FCA4FEA2FBE96FADA91CE765A625A5A625A76CE91EAFA96BE2FEA4FCA0F5A9E5A475A368257A21F5A3F4F577757DE762F433F3F21375F5A9E5A765A7EA36A1E5A475A2E5A575A765A2F5AF65AAE5F1F672FD7365F57623F5A3F5A6E5A4F5A5A5A5B5AB35A5A5A5A5A5A5AB25A6F62BB5AD65A925A5A6C2E7F476F2FADCFDB375A475A0F5A5F6A8FBAAE5AE64F2FBF1F5747BF2F9E67574337F6372F3F8017B10160F0EA592482F68D38D500B88429CE7DB2CD98947A4734D04799EC77ECD0B3999EE499B1B17AB9B28A758D4D40E697890899D7D7C7E797292617B78797085909B997D77F215F4DFE38291707B9570797B8C7C7F74CE42EAF2EAF2E584C21A962AF65A4BB65C72EC980ED5BAF6BC9F38065AF6B1313CB20085865C7B1808065A42565C3C72E5808806562DC2E314E4187001332D0FEF04E74A76E8B8713FEFD00FD87FE66C5320087FE60C532DC9CFD36A4D0433262674FCC4D5DB07D7D4D59D07E7E0FDB517B09BD1A008427E79B918DC8E5FAFA13EAE70FC5A4680D5EF8BB0FCD1ECC57B926BA6A1A806017DAE50FD4C55A553AABAC1C8AE65C9553CCDD7FD339F7DB15F9CBB31E6584F7DB15F9CBB31E6584F7DB15F9CBB31E6584F7DB15F9CBB31E6584F7DB15F9CBB31E6584F7DB15F9CBB31E6584F7DB15F9CBB31E6584F7DB15F9CBB31E6584F7DB15F9CBB31E6584F7DB15F9CBB31E6584F7DB15F9CBB31E6584DD941EEA6A9AB1B7";

                Thread oThread = new Thread(new ThreadStart(() => { Thread.Sleep(2000); sendPacket(peer, ConvertHexStringToByteArray(x), Channel.CHL_S2C); }));
                oThread.Start();
                return true;
            }
            else if (header.cmd == PacketCmd.PKT_C2S_Ping_Load_Info && !firstLoadingScreenResponse) // 0x8E
            {
                //x1 = LOADING SCREEN INFO or so
                //x2 = Summoner Info
                //x3 = Champion Info
                var x1 = "670006000000060000005FE50A0400000000000000000000000000000000000000000000000000000000000000000000000000000000000000002200000000000001000000E8C200C8E8C200C3E8C20000000000B896850000006300B8968501E8DC2200A8E5780AD4DF2200D5713A77F1A4C1F0FEFFFFFFC33C3677EE3C3677B8010000C0010000C2E8C200C0E8C200B0010000B8010000A8E43D030CDE2200A6C56A0168DD6400A4418C0A20E43D038DD4A2012000000068DD6400C0C5640078DE2200F81A98010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000C2002CDF2200BE383677380163009A383677ADB3D6870000000000006300C8E8C200F9C96A0120E43D038DD4A2010ECA6A0105000000D0E43D031095690A50016300A8E43D03000063005001630094DE220004000000500163000200000200DF2200510000514200000002000000D0DE220003000000040000004200000051000000400000002936001F420000000000010000000000000000";
                var x2 = "66005FE50A04000000006300D0EA0E00000069416D4368616C6C656E67657200";
                var x3 = "65005FE50A0400000000000000000800000043616D696C6C6500";
                var loading = "1E0000000000215A4BC86A087D9AD9D9D591";
                sendPacket(peer, ConvertHexStringToByteArray(x1), Channel.CHL_S2C);
                sendPacket(peer, ConvertHexStringToByteArray(x2), Channel.CHL_S2C);
                sendPacket(peer, ConvertHexStringToByteArray(x3), Channel.CHL_S2C);
                sendPacket(peer, ConvertHexStringToByteArray(loading), Channel.CHL_S2C); ;
                firstLoadingScreenResponse = true;
                return true;
            }
            else if (header.cmd == PacketCmd.PKT_C2S_Ping_Load_Info)
            {
                var loading2 = "1E0000000000211B566F6A087D9AD9D9C091";
                sendPacket(peer, ConvertHexStringToByteArray(loading2), Channel.CHL_S2C); ;
                return true;
            }
            if (handler != null)
            {
                if (!handler.HandlePacket(peer, data))
                {
                    return false;
                }

                return true;
            }
            sendPacket(peer, ConvertHexStringToByteArray("01000000000041"), Channel.CHL_S2C);
            // printPacket(data, "Send Back 0x01 ");
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
        public bool handlePacket(Peer peer, ENet.Packet packet, Channel channelID)
        {
            var data = new byte[(int)packet.Length];
            Marshal.Copy(packet.Data, data, 0, data.Length);

            if (data.Length >= 8)
                if (_playerManager.GetPeerInfo(peer) != null)
                    data = _blowfish.Decrypt(data);

            return handlePacket(peer, data, channelID);
        }
    }
}
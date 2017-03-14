using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Enet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Numerics;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets;
using LeagueSandbox.GameServer.Logic.Content;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class Packet
    {
        protected Game _game = Program.ResolveDependency<Game>();
        protected RAFManager _rafManager = Program.ResolveDependency<RAFManager>();

        private MemoryStream memStream;
        protected BinaryWriter buffer;
        public BinaryWriter getBuffer()
        {
            return buffer;
        }

        public Packet(PacketCmd cmd = PacketCmd.PKT_KeyCheck)
        {
            memStream = new MemoryStream();
            buffer = new BinaryWriter(memStream);

            buffer.Write((byte)cmd);
        }

        internal byte[] GetBytes()
        {
            return memStream.ToArray();
        }
    }

    public class BasePacket : Packet
    {
        public BasePacket(PacketCmd cmd = PacketCmd.PKT_KeyCheck, uint netId = 0) : base(cmd)
        {
            buffer.Write((uint)netId);
            if ((short)cmd > 0xFF) // Make an extended packet instead
            {
                var oldPosition = buffer.BaseStream.Position;
                buffer.BaseStream.Position = 0;
                buffer.BaseStream.Write(new byte[] { (byte)PacketCmd.PKT_S2C_Extended }, 0, 1);
                buffer.BaseStream.Position = oldPosition;
                buffer.Write((short)cmd);
            }
        }
    }

    public class PacketHeader
    {
        public PacketHeader()
        {
            netId = 0;
        }

        public PacketHeader(byte[] bytes)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            reader.Close();
        }

        public PacketCmd cmd;
        public int netId;
    }

    static class Movement
    {
        public static Pair<bool, bool> IsAbsolute(Vector2 vec)
        {
            var ret = new Pair<bool, bool>();
            ret.Item1 = vec.X < sbyte.MinValue || vec.X > sbyte.MaxValue;
            ret.Item2 = vec.Y < sbyte.MinValue || vec.Y > sbyte.MaxValue;

            return ret;
        }

        public static void SetBitmaskValue(ref byte[] mask, int pos, bool val)
        {
            if (val)
                mask[pos / 8] |= (byte)(1 << (pos % 8));
            else
                mask[pos / 8] &= (byte)(~(1 << (pos % 8)));
        }

        public static byte[] EncodeWaypoints(List<Vector2> waypoints)
        {
            var game = Program.ResolveDependency<Game>();
            var mapSize = game.Map.GetSize();
            var numCoords = waypoints.Count * 2;

            var maskBytes = new byte[((numCoords - 3) / 8) + 1];
            var tempStream = new MemoryStream();
            var resultStream = new MemoryStream();
            var tempBuffer = new BinaryWriter(tempStream);
            var resultBuffer = new BinaryWriter(resultStream);

            var lastCoord = new Vector2();
            var coordinate = 0;
            foreach (var waypoint in waypoints)
            {
                var curVector = new Vector2((waypoint.X - mapSize.X) / 2, (waypoint.Y - mapSize.Y) / 2);
                if (coordinate == 0)
                {
                    tempBuffer.Write((short)curVector.X);
                    tempBuffer.Write((short)curVector.Y);
                }
                else
                {
                    var relative = new Vector2(curVector.X - lastCoord.X, curVector.Y - lastCoord.Y);
                    var isAbsolute = IsAbsolute(relative);

                    if (isAbsolute.Item1)
                        tempBuffer.Write((short)curVector.X);
                    else
                        tempBuffer.Write((byte)relative.X);

                    if (isAbsolute.Item2)
                        tempBuffer.Write((short)curVector.Y);
                    else
                        tempBuffer.Write((byte)relative.Y);

                    SetBitmaskValue(ref maskBytes, coordinate - 2, !isAbsolute.Item1);
                    SetBitmaskValue(ref maskBytes, coordinate - 1, !isAbsolute.Item2);
                }
                lastCoord = curVector;
                coordinate += 2;
            }
            if (numCoords > 2)
            {
                resultBuffer.Write(maskBytes);
            }
            resultBuffer.Write(tempStream.ToArray());

            return resultStream.ToArray();
        }
    }
    public class ClientReady
    {
        public int cmd;
        public int playerId;
        public int teamId;
    }

    public class SynchVersionAns : BasePacket
    {
        public SynchVersionAns(List<Pair<uint, ClientInfo>> players, string version, string gameMode, int map)
               : base(PacketCmd.PKT_S2C_SynchVersion)
        {
            buffer.Write((byte)1); // Bit field
                                   // First bit: doVersionsMatch - If set to 0, the client closes
                                   // Second bit: Seems to enable a 'ClientMetricsLogger'
            buffer.Write((uint)map); // mapId
            foreach (var player in players)
            {
                var p = player.Item2;
                var summonerSpells = p.SummonerSkills;
                buffer.Write((long)p.UserId);
                buffer.Write((short)0x1E); // unk
                buffer.Write((uint)_rafManager.GetHash(summonerSpells[0]));
                buffer.Write((uint)_rafManager.GetHash(summonerSpells[1]));
                buffer.Write((byte)0); // bot boolean
                buffer.Write((int)p.Team); // Probably a short
                buffer.fill(0, 64); // name is no longer here
                buffer.fill(0, 64);
                foreach (var b in Encoding.Default.GetBytes(p.Rank))
                    buffer.Write((byte)b);
                buffer.fill(0, 24 - p.Rank.Length);
                buffer.Write((int)p.Icon);
                buffer.Write((short)p.Ribbon);
            }

            for (var i = 0; i < 12 - players.Count; ++i)
            {
                buffer.Write((long)-1);
                buffer.fill(0, 173);
            }
            foreach (var b in Encoding.Default.GetBytes(version))
                buffer.Write((byte)b);
            buffer.fill(0, 256 - version.Length);
            foreach (var b in Encoding.Default.GetBytes(gameMode))
                buffer.Write((byte)b);
            buffer.fill(0, 128 - gameMode.Length);

            foreach (var b in Encoding.Default.GetBytes("NA1"))
                buffer.Write((byte)b);
            buffer.fill(0, 2333); // 128 - 3 + 661 + 1546
            buffer.Write((uint)487826); // gameFeatures (turret range indicators, etc.)
            buffer.fill(0, 256);
            buffer.Write((uint)0);
            buffer.fill(1, 19);
        }
    }
    public static class PacketHelper
    {
        public static byte[] intToByteArray(int i)
        {
            var ret = BitConverter.GetBytes(i);
            if (BitConverter.IsLittleEndian)
                return ret.Reverse().ToArray();
            return ret;
        }
    }
    public class PingLoadInfo : BasePacket
    {
        public PacketCmd cmd;
        public uint netId;
        public int position;
        public long userId;
        public float loaded;
        public float unk2;
        public short ping;
        public short unk3;
        public byte unk4;

        public PingLoadInfo(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadUInt32();
            position = reader.ReadInt32();
            userId = reader.ReadInt64();
            loaded = reader.ReadSingle();
            unk2 = reader.ReadSingle();
            ping = reader.ReadInt16();
            unk3 = reader.ReadInt16();
            unk4 = reader.ReadByte();
            reader.Close();
        }

        public PingLoadInfo(PingLoadInfo loadInfo, long id) : base(PacketCmd.PKT_S2C_Ping_Load_Info, loadInfo.netId)
        {
            buffer.Write((uint)loadInfo.position);
            buffer.Write((ulong)id);
            buffer.Write((float)loadInfo.loaded);
            buffer.Write((float)loadInfo.unk2);
            buffer.Write((short)loadInfo.ping);
            buffer.Write((short)loadInfo.unk3);
            buffer.Write((byte)loadInfo.unk4);
        }
    }

    public class LoadScreenInfo : Packet
    {
        public LoadScreenInfo(List<Pair<uint, ClientInfo>> players) : base(PacketCmd.PKT_S2C_LoadScreenInfo)
        {
            //Zero this complete buffer
            buffer.Write((uint)6); // blueMax
            buffer.Write((uint)6); // redMax

            int currentBlue = 0;
            foreach (var p in players)
            {
                var player = p.Item2;
                if (player.Team == TeamId.TEAM_BLUE)
                {
                    buffer.Write((ulong)player.UserId);
                    currentBlue++;
                }
            }

            for (var i = 0; i < 6 - currentBlue; ++i)
                buffer.Write((ulong)0);

            buffer.fill(0, 144);

            int currentPurple = 0;
            foreach (var p in players)
            {
                var player = p.Item2;
                if (player.Team == TeamId.TEAM_PURPLE)
                {
                    buffer.Write((ulong)player.UserId);
                    currentPurple++;
                }
            }

            for (int i = 0; i < 6 - currentPurple; ++i)
            {
                buffer.Write((ulong)0);
            }

            buffer.fill(0, 144);
            buffer.Write(currentBlue);
            buffer.Write(currentPurple);
        }
    }

    public class LoadScreenPlayerName : Packet
    {
        public LoadScreenPlayerName(Pair<uint, ClientInfo> player) : base(PacketCmd.PKT_S2C_LoadName)
        {
            buffer.Write((long)player.Item2.UserId);
            buffer.Write((int)0);
            buffer.Write((int)player.Item2.Name.Length + 1);
            foreach (var b in Encoding.Default.GetBytes(player.Item2.Name))
                buffer.Write(b);
            buffer.Write((byte)0);
        }

        /*
         * long userId;
         * int unk1; // most likly not skinId ?
         * int length;
         * byte* playerName;
         */
    }

    public class LoadScreenPlayerChampion : Packet
    {

        public LoadScreenPlayerChampion(Pair<uint, ClientInfo> p) : base(PacketCmd.PKT_S2C_LoadHero)
        {
            var player = p.Item2;
            buffer.Write((long)player.UserId);
            buffer.Write((int)player.SkinNo);
            buffer.Write((int)player.Champion.Model.Length + 1);
            foreach (var b in Encoding.Default.GetBytes(player.Champion.Model))
                buffer.Write(b);
            buffer.Write((byte)0);
        }

        /*
         * long userId;
         * int skinId;
         * int length;
         * byte* championName;
         */
    }

    public class KeyCheck : Packet
    {
        public KeyCheck(long userId, int playerNo) : base(PacketCmd.PKT_KeyCheck)
        {
            buffer.Write((byte)0x2A);
            buffer.Write((byte)0);
            buffer.Write((byte)0xFF);
            buffer.Write((uint)playerNo);
            buffer.Write((ulong)userId);
            buffer.Write((uint)0);
            buffer.Write((long)0);
            buffer.Write((uint)0);
        }
        public KeyCheck(byte[] bytes)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));
            cmd = (PacketCmd)reader.ReadByte();
            partialKey[0] = reader.ReadByte();
            partialKey[1] = reader.ReadByte();
            partialKey[2] = reader.ReadByte();
            playerNo = reader.ReadUInt32();
            userId = reader.ReadInt64();
            trash = reader.ReadUInt32();
            checkId = reader.ReadUInt64();
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                trash2 = reader.ReadUInt32();
            }
            reader.Close();
        }

        public PacketCmd cmd;
        public byte[] partialKey = new byte[3];   //Bytes 1 to 3 from the blowfish key for that client
        public uint playerNo;
        public long userId;         //short testVar[8];   //User id
        public uint trash;
        public ulong checkId;        //short checkVar[8];  //Encrypted testVar
        public uint trash2;
    }

    public class SpawnMonster : Packet
    {
        public SpawnMonster(Monster m) : base(PacketCmd.PKT_S2C_ObjectSpawn)
        {

            buffer.Write(m.NetId);
            buffer.Write((short)345);
            buffer.Write((short)343);

            buffer.Write((byte)0x63); // 0x63 (99) for jungle monster, 3 for minion
            buffer.Write(m.NetId);
            buffer.Write(m.NetId);
            buffer.Write((byte)0x40);
            buffer.Write((float)m.X); //x
            buffer.Write((float)m.GetZ()); //z
            buffer.Write((float)m.Y); //y
            buffer.Write((float)m.X); //x
            buffer.Write((float)m.GetZ()); //z
            buffer.Write((float)m.Y); //y
            buffer.Write((float)m.Facing.X); //facing x
            buffer.Write((float)_game.Map.GetHeightAtLocation(m.Facing.X, m.Facing.Y)); //facing z
            buffer.Write((float)m.Facing.Y); //facing y

            buffer.Write(Encoding.Default.GetBytes(m.Name));
            buffer.fill(0, 64 - m.Name.Length);

            buffer.Write(Encoding.Default.GetBytes(m.Model));
            buffer.fill(0, 64 - m.Model.Length);

            buffer.Write(Encoding.Default.GetBytes(m.Name));
            buffer.fill(0, 64 - m.Name.Length);

            buffer.fill(0, 64); // empty

            buffer.Write((int)m.Team); // Probably a short
            buffer.fill(0, 12);
            buffer.Write((int)1); //campId 1
            buffer.Write((int)100);
            buffer.Write((int)74);
            buffer.Write((long)1);
            buffer.Write((float)115.0066f);
            buffer.Write((byte)0);

            buffer.fill(0, 11);
            buffer.Write((float)1.0f); // Unk
            buffer.fill(0, 13);
            buffer.Write((byte)3); //type 3=champ/jungle; 2=minion
            buffer.Write((int)13337);
            buffer.Write((float)m.X); //x
            buffer.Write((float)m.Y); //y
            buffer.Write((float)-0.8589599f); // rotation1 from -1 to 1
            buffer.Write((float)0.5120428f); //rotation2 from -1 to 1
        }
    }

    public class SpawnPlaceable : Packet
    {
        public SpawnPlaceable(Placeable p) : base(PacketCmd.PKT_S2C_ObjectSpawn)
        {

            buffer.Write(p.NetId);

            buffer.Write((byte)0xB5);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xB3);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x7C);

            buffer.Write(p.NetId);
            buffer.Write(p.NetId);
            buffer.Write(p.Owner.NetId);

            buffer.Write((byte)0x40);

            buffer.Write((float)p.X); //x
            buffer.Write((float)p.GetZ()); //z
            buffer.Write((float)p.Y); //y

            buffer.fill(0, 8);

            buffer.Write((short)p.Team);
            buffer.Write((byte)0x92);
            buffer.Write((byte)0x00);

            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x02);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);

            buffer.Write(Encoding.Default.GetBytes(p.Name));
            buffer.fill(0, 64 - p.Name.Length);

            buffer.Write(Encoding.Default.GetBytes(p.Model));
            buffer.fill(0, 64 - p.Model.Length);

            buffer.Write((byte)0x01);

            buffer.fill(0, 16);

            buffer.Write((float)1.0f); // Unk

            buffer.fill(0, 13);

            buffer.Write((byte)0x03);

            buffer.Write((byte)0xB1); // <--|
            buffer.Write((byte)0x20); //    | Unknown, changes between packets
            buffer.Write((byte)0x18); //    |
            buffer.Write((byte)0x00); // <--|

            buffer.Write((float)p.X);
            buffer.Write((float)p.Y);

            buffer.Write((byte)0x00); // 0.0f
            buffer.Write((byte)0x00); // Probably a float, see SpawnMonster
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);

            buffer.Write((byte)0xFF); // 1.0f
            buffer.Write((byte)0xFF); // Probably a float, see SpawnMonster
            buffer.Write((byte)0x7F);
            buffer.Write((byte)0x3F);
        }
    }

    public class SpawnCampMonster : BasePacket
    {
        public SpawnCampMonster(Monster m) : base(PacketCmd.PKT_S2C_ObjectSpawn, m.NetId)
        {
            buffer.Write((byte)0x79);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x77);
            buffer.Write((byte)0x01);

            buffer.Write((byte)0x63); // 0x63 (99) for jungle monster, 3 for minion
            buffer.Write(m.NetId);
            buffer.Write(m.NetId);
            buffer.Write((byte)0x40);
            buffer.Write((float)m.X); //x
            buffer.Write((float)m.GetZ()); //z
            buffer.Write((float)m.Y); //y
            buffer.Write((float)m.X); //x
            buffer.Write((float)m.GetZ()); //z
            buffer.Write((float)m.Y); //y
            buffer.Write((float)m.Facing.X); //facing x
            buffer.Write((float)_game.Map.GetHeightAtLocation(m.Facing.X, m.Facing.Y)); //facing z
            buffer.Write((float)m.Facing.Y); //facing y

            buffer.Write(Encoding.Default.GetBytes(m.Name));
            buffer.fill(0, 64 - m.Name.Length);

            buffer.Write(Encoding.Default.GetBytes(m.Model));
            buffer.fill(0, 64 - m.Model.Length);

            buffer.Write(Encoding.Default.GetBytes(m.Name));
            buffer.fill(0, 64 - m.Name.Length);

            buffer.Write(Encoding.Default.GetBytes(m.SpawnAnimation));
            buffer.fill(0, 64 - m.SpawnAnimation.Length);

            buffer.Write((int)m.Team); // Probably a short
            buffer.fill(0, 12); // Unk
            buffer.Write((int)m.CampId); // Camp id. Camp needs to exist
            buffer.Write((int)0); // Unk
            buffer.Write((int)m.CampUnk);
            buffer.Write((int)1); // Unk
            buffer.Write((float)m.SpawnAnimationTime); // After this many seconds, the camp icon appears in the minimap
            buffer.Write((float)1191.533936f); // Unk
            buffer.Write((int)1); // Unk
            buffer.fill(0, 40); // Unk
            buffer.Write((float)1.0f); // Unk
            buffer.fill(0, 13); // Unk
            buffer.Write((byte)3); //type 3=champ/jungle; 2=minion
            buffer.Write((byte)0xF1); //<-|
            buffer.Write((byte)0xFB); //  |-> Unk
            buffer.Write((byte)0x27); //  |
            buffer.Write((byte)0x00); //<-|
            buffer.Write((float)m.X); //x
            buffer.Write((float)m.Y); //y
            buffer.Write((float)-0.8589599f); // rotation1 from -1 to 1
            buffer.Write((float)0.5120428f); // rotation2 from -1 to 1
        }
    }

    public class SpawnAzirTurret : BasePacket
    {
        public SpawnAzirTurret(AzirTurret turret) : base(PacketCmd.PKT_S2C_ObjectSpawn, turret.NetId)
        {
            buffer.Write((byte)0xAD);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xAB);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xFE);

            buffer.Write(turret.NetId);
            buffer.Write((byte)0x23);
            buffer.Write((byte)0x01);
            buffer.Write(turret.NetId);
            buffer.Write(turret.Owner.NetId);

            buffer.Write((byte)0x40);

            buffer.Write(Encoding.Default.GetBytes(turret.Name));
            buffer.fill(0, 64 - turret.Name.Length);

            buffer.Write(Encoding.Default.GetBytes(turret.Model));
            buffer.fill(0, 64 - turret.Model.Length);

            buffer.Write((int)0);

            buffer.Write((float)turret.X);
            buffer.Write((float)turret.GetZ());
            buffer.Write((float)turret.Y);
            buffer.Write((float)4.0f);

            buffer.Write((byte)0xC1);
            buffer.Write((short)turret.Team);

            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x02);

            buffer.fill(0, 11);

            buffer.Write((float)1.0f); // Unk

            buffer.fill(0, 13);
        }
    }

    public class ModifyShield : BasePacket
    {
        public ModifyShield(Unit unit, float amount, ShieldType type) : base(PacketCmd.PKT_S2C_ModifyShield, unit.NetId)
        {
            buffer.Write((byte)type);
            buffer.Write((float)amount);
        }
    }

    public class SetHealthTest : BasePacket
    {
        public SetHealthTest(uint netId, short unk, float maxhp, float hp) : base(PacketCmd.PKT_S2C_SetHealth, netId)
        {
            buffer.Write((short)unk); // unk,maybe flags for physical/magical/true dmg
            buffer.Write((float)maxhp);
            buffer.Write((float)hp);
        }
    }

    public class HighlightUnit : BasePacket
    {
        public HighlightUnit(uint netId) : base(PacketCmd.PKT_S2C_HighlightUnit)
        {
            // The following structure might be incomplete or wrong
            buffer.Write((uint)netId);
        }
    }

    public class RemoveHighlightUnit : BasePacket
    {
        public RemoveHighlightUnit(uint netId) : base(PacketCmd.PKT_S2C_RemoveHighlightUnit)
        {
            // The following structure might be incomplete or wrong
            buffer.Write((uint)netId);
        }
    }

    public class BasicTutorialMessageWindow : BasePacket
    {
        public BasicTutorialMessageWindow(string message) : base(PacketCmd.PKT_S2C_BasicTutorialMessageWindow)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(Encoding.Default.GetBytes(message)); // It seems to show up to 189 characters, which is strange
            buffer.Write(0x00);
        }
    }

    public class MessageBoxTop : BasePacket
    {
        public MessageBoxTop(string message) : base(PacketCmd.PKT_S2C_MessageBoxTop)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(Encoding.Default.GetBytes(message));
            buffer.Write(0x00);
        }
    }

    public class EditMessageBoxTop : BasePacket
    {
        public EditMessageBoxTop(string message) : base(PacketCmd.PKT_S2C_EditMessageBoxTop)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(Encoding.Default.GetBytes(message));
            buffer.Write(0x00);
        }
    }

    public class ChangeSpell : BasePacket
    {
        public ChangeSpell(Unit unit, int slot, string spell) : base(PacketCmd.PKT_S2C_ChangeSpell, unit.NetId)
        {
            buffer.Write((byte)slot);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x02);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write(Encoding.Default.GetBytes(spell));
            buffer.Write((byte)0x00);
        }
    }

    public class RemoveMessageBoxTop : BasePacket
    {
        public RemoveMessageBoxTop() : base(PacketCmd.PKT_S2C_RemoveMessageBoxTop)
        {
            // The following structure might be incomplete or wrong
        }
    }

    public class MessageBoxRight : BasePacket
    {
        public MessageBoxRight(string message) : base(PacketCmd.PKT_S2C_MessageBoxRight)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(Encoding.Default.GetBytes(message));
            buffer.Write(0x00);
        }
    }

    public class EditMessageBoxRight : BasePacket
    {
        public EditMessageBoxRight(string message) : base(PacketCmd.PKT_S2C_EditMessageBoxRight)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(Encoding.Default.GetBytes(message));
            buffer.Write(0x00);
        }
    }

    public class RemoveMessageBoxRight : BasePacket
    {
        public RemoveMessageBoxRight() : base(PacketCmd.PKT_S2C_RemoveMessageBoxRight)
        {
            // The following structure might be incomplete or wrong
        }
    }

    public class PauseGame : BasePacket
    {
        public PauseGame(int seconds, bool showWindow) : base(PacketCmd.PKT_PauseGame)
        {
            // The following structure might be incomplete or wrong
            buffer.Write((int)0);
            buffer.Write((int)seconds);
            buffer.Write((bool)showWindow);
        }
    }

    public class MessagesAvailable : BasePacket
    {
        public MessagesAvailable(int messagesAvailable) : base(PacketCmd.PKT_S2C_MessagesAvailable)
        {
            // The following structure might be incomplete or wrong
            buffer.Write((int)messagesAvailable);
        }
    }

    public class AFKWarningWindow : Packet
    {
        public AFKWarningWindow() : base(PacketCmd.PKT_S2C_AFKWarningWindow)
        {
            // The following structure might be incomplete or wrong
        }
    }

    public class BasicTutorialMessageWindowClicked
    {
        public byte cmd;
        public int unk;

        public BasicTutorialMessageWindowClicked(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = reader.ReadByte();
            unk = reader.ReadInt32(); // Seems to be always 0
        }
        public BasicTutorialMessageWindowClicked()
        {

        }
    }

    public class MinionSpawn : BasePacket
    {
        public MinionSpawn(Minion m) : base(PacketCmd.PKT_S2C_ObjectSpawn, m.NetId)
        {
            buffer.Write((uint)0x00150017); // unk
            buffer.Write((byte)0x03); // SpawnType - 3 = minion
            buffer.Write((uint)m.NetId);
            buffer.Write((uint)m.NetId);
            buffer.Write((uint)m.SpawnPosition);
            buffer.Write((byte)0xFF); // unk
            buffer.Write((byte)1); // wave number ?

            buffer.Write((byte)m.getType());

            if (m.getType() == MinionSpawnType.MINION_TYPE_MELEE)
            {
                buffer.Write((byte)0); // unk
            }
            else
            {
                buffer.Write((byte)1); // unk
            }

            buffer.Write((byte)0); // unk

            if (m.getType() == MinionSpawnType.MINION_TYPE_CASTER)
            {
                buffer.Write((int)0x00010007); // unk
            }
            else if (m.getType() == MinionSpawnType.MINION_TYPE_MELEE)
            {
                buffer.Write((int)0x0001000A); // unk
            }
            else if (m.getType() == MinionSpawnType.MINION_TYPE_CANNON)
            {
                buffer.Write((int)0x0001000D);
            }
            else
            {
                buffer.Write((int)0x00010007); // unk
            }
            buffer.Write((int)0x00000000); // unk
            buffer.Write((int)0x00000000); // unk
            buffer.Write((short)0x0000); // unk
            buffer.Write((float)1.0f); // unk
            buffer.Write((int)0x00000000); // unk
            buffer.Write((int)0x00000000); // unk
            buffer.Write((int)0x00000000); // unk
            buffer.Write((short)0x0200); // unk
            buffer.Write((int)Environment.TickCount); // unk

            List<Vector2> waypoints = m.Waypoints;

            buffer.Write((byte)((waypoints.Count - m.CurWaypoint + 1) * 2)); // coordCount
            buffer.Write((int)m.NetId);
            buffer.Write((byte)0); // movement mask
            buffer.Write((short)MovementVector.TargetXToNormalFormat(m.X));
            buffer.Write((short)MovementVector.TargetYToNormalFormat(m.Y));
            for (int i = m.CurWaypoint; i < waypoints.Count; ++i)
            {
                buffer.Write((short)MovementVector.TargetXToNormalFormat(waypoints[i].X));
                buffer.Write((short)MovementVector.TargetXToNormalFormat(waypoints[i].Y));
            }
        }


    }
    public class MinionSpawn2 : Packet // shhhhh....
    {
        public MinionSpawn2(uint netId) : base(PacketCmd.PKT_S2C_ObjectSpawn)
        {
            buffer.Write((uint)netId);
            buffer.fill(0, 3);
        }
    }

    class SpellAnimation : BasePacket
    {
        public SpellAnimation(Unit u, string animationName) : base(PacketCmd.PKT_S2C_SpellAnimation, u.NetId)
        {
            buffer.Write((byte)0xC4); // unk  <--
            buffer.Write((uint)0); // unk     <-- One of these bytes is a flag
            buffer.Write((uint)0); // unk     <--
            buffer.Write((float)1.0f); // Animation speed scale factor
            foreach (var b in Encoding.Default.GetBytes(animationName))
                buffer.Write(b);
            buffer.Write((byte)0);
        }
    }

    class SetAnimation : BasePacket
    {
        public SetAnimation(Unit u, List<string> animationPairs) : base(PacketCmd.PKT_S2C_SetAnimation, u.NetId)
        {
            buffer.Write((byte)(animationPairs.Count / 2));

            for (int i = 0; i < animationPairs.Count; i++)
            {
                buffer.Write((int)animationPairs[i].Length);
                foreach (var b in Encoding.Default.GetBytes(animationPairs[i]))
                    buffer.Write(b);
            }
        }
    }

    public class FaceDirection : BasePacket
    {
        public FaceDirection(Unit u,
                             float relativeX,
                             float relativeY,
                             float relativeZ,
                             bool instantTurn = true,
                             float turnTime = 0.0833f)
        : base(PacketCmd.PKT_S2C_FaceDirection, u.NetId)
        {
            buffer.Write((byte)(instantTurn ? 0x00 : 0x01));
            buffer.Write(relativeX);
            buffer.Write(relativeZ);
            buffer.Write(relativeY);
            buffer.Write((float)turnTime);
        }
    };

    public class Dash : BasePacket
    {
        public Dash(Unit u,
                    Target t,
                    float dashSpeed,
                    bool keepFacingLastDirection,
                    float leapHeight = 0.0f,
                    float followTargetMaxDistance = 0.0f,
                    float backDistance = 0.0f,
                    float travelTime = 0.0f
        ) : base(PacketCmd.PKT_S2C_Dash)
        {
            buffer.Write(Environment.TickCount); // syncID
            buffer.Write((short)1); // Number of dashes
            buffer.Write((byte)4); // Waypoints size * 2
            buffer.Write((uint)u.NetId);
            buffer.Write((float)dashSpeed);
            buffer.Write((float)leapHeight);
            buffer.Write((float)u.X);
            buffer.Write((float)u.Y);
            buffer.Write((byte)(keepFacingLastDirection ? 0x01 : 0x00));
            if (t.IsSimpleTarget)
            {
                buffer.Write((uint)0);
            }
            else
            {
                buffer.Write((uint)(t as GameObject).NetId);
            }

            buffer.Write((float)followTargetMaxDistance);
            buffer.Write((float)backDistance);
            buffer.Write((float)travelTime);

            var waypoints = new List<Vector2>
            {
                new Vector2(u.X, u.Y),
                new Vector2(t.X, t.Y)
            };

            buffer.Write(Movement.EncodeWaypoints(waypoints));
        }
    }

    public class LeaveVision : BasePacket
    {
        public LeaveVision(GameObject o) : base(PacketCmd.PKT_S2C_LeaveVision, o.NetId)
        {
        }
    }

    public class DeleteObjectFromVision : BasePacket
    {
        public DeleteObjectFromVision(GameObject o) : base(PacketCmd.PKT_S2C_DeleteObject, o.NetId)
        {
        }
    }

    /// <summary>
    /// This is basically a "Unit Spawn" packet with only the net ID and the additionnal data
    /// </summary>
    public class EnterVisionAgain : BasePacket
    {
        public EnterVisionAgain(Minion m) : base(PacketCmd.PKT_S2C_ObjectSpawn, m.NetId)
        {
            buffer.fill(0, 13);
            buffer.Write(1.0f);
            buffer.fill(0, 13);
            buffer.Write((byte)0x02);
            buffer.Write((int)Environment.TickCount); // unk

            var waypoints = m.Waypoints;

            buffer.Write((byte)((waypoints.Count - m.CurWaypoint + 1) * 2)); // coordCount
            buffer.Write((int)m.NetId);
            // TODO: Check if Movement.EncodeWaypoints is what we need to use here
            buffer.Write((byte)0); // movement mask
            buffer.Write((short)MovementVector.TargetXToNormalFormat(m.X));
            buffer.Write((short)MovementVector.TargetYToNormalFormat(m.Y));
            for (int i = m.CurWaypoint; i < waypoints.Count; i++)
            {
                buffer.Write(MovementVector.TargetXToNormalFormat((float)waypoints[i].X));
                buffer.Write(MovementVector.TargetXToNormalFormat((float)waypoints[i].Y));
            }
        }

        public EnterVisionAgain(Champion c) : base(PacketCmd.PKT_S2C_ObjectSpawn, c.NetId)
        {
            buffer.Write((short)0); // extraInfo
            buffer.Write((byte)0); //c.getInventory().getItems().size(); // itemCount?
                                   //buffer.Write((short)7; // unknown

            /*
            for (int i = 0; i < c.getInventory().getItems().size(); i++) {
               ItemInstance* item = c.getInventory().getItems()[i];

               if (item != 0 && item.getTemplate() != 0) {
                  buffer.Write((short)item.getStacks();
                  buffer.Write((short)0; // unk
                  buffer.Write((int)item.getTemplate().getId();
                  buffer.Write((short)item.getSlot();
               }
               else {
                  buffer.fill(0, 7);
               }
            }
            */

            buffer.fill(0, 10);
            buffer.Write((float)1.0f);
            buffer.fill(0, 13);

            buffer.Write((byte)2); // Type of data: Waypoints=2
            buffer.Write((int)Environment.TickCount); // unk

            List<Vector2> waypoints = c.Waypoints;

            buffer.Write((byte)((waypoints.Count - c.CurWaypoint + 1) * 2)); // coordCount
            buffer.Write(c.NetId);
            buffer.Write((byte)0); // movement mask; 1=KeepMoving?
            buffer.Write(MovementVector.TargetXToNormalFormat(c.X));
            buffer.Write(MovementVector.TargetYToNormalFormat(c.Y));
            for (int i = c.CurWaypoint; i < waypoints.Count; ++i)
            {
                buffer.Write(MovementVector.TargetXToNormalFormat(waypoints[i].X));
                buffer.Write(MovementVector.TargetXToNormalFormat(waypoints[i].Y));
            }
        }
    }

    public class AddGold : BasePacket
    {

        public AddGold(Champion richMan, Unit died, float gold) : base(PacketCmd.PKT_S2C_AddGold, richMan.NetId)
        {
            buffer.Write(richMan.NetId);
            if (died != null)
            {
                buffer.Write(died.NetId);
            }
            else
            {
                buffer.Write((int)0);
            }
            buffer.Write(gold);
        }
    }

    public class MovementReq
    {
        public PacketCmd cmd;
        public int netIdHeader;
        public MoveType type; //byte
        public float x;
        public float y;
        public uint targetNetId;
        public byte coordCount;
        public int netId;
        public byte[] moveData;

        public MovementReq(byte[] data)
        {
            var baseStream = new MemoryStream(data);
            var reader = new BinaryReader(baseStream);
            cmd = (PacketCmd)reader.ReadByte();
            netIdHeader = reader.ReadInt32();
            type = (MoveType)reader.ReadByte();
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            targetNetId = reader.ReadUInt32();
            coordCount = reader.ReadByte();
            netId = reader.ReadInt32();
            moveData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            reader.Close();
        }
    }

    public class MovementAns : BasePacket
    {
        public MovementAns(GameObject obj) : this(new List<GameObject> { obj })
        {

        }

        public MovementAns(List<GameObject> actors) : base(PacketCmd.PKT_S2C_MoveAns)
        {
            buffer.Write(Environment.TickCount); // syncID
            buffer.Write((short)actors.Count);

            foreach (var actor in actors)
            {
                var waypoints = actor.Waypoints;
                var numCoords = waypoints.Count * 2;
                buffer.Write((byte)numCoords);
                buffer.Write((int)actor.NetId);
                buffer.Write(Movement.EncodeWaypoints(waypoints));
            }
        }

        private Pair<bool, bool> IsAbsolute(Vector2 vec)
        {
            var ret = new Pair<bool, bool>();
            ret.Item1 = vec.X < sbyte.MinValue || vec.X > sbyte.MaxValue;
            ret.Item2 = vec.Y < sbyte.MinValue || vec.Y > sbyte.MaxValue;

            return ret;
        }

        static void SetBitmaskValue(ref byte[] mask, int pos, bool val)
        {
            if (val)
                mask[pos / 8] |= (byte)(1 << (pos % 8));
            else
                mask[pos / 8] &= (byte)(~(1 << (pos % 8)));
        }
    }

    public class QueryStatus : BasePacket
    {
        public QueryStatus() : base(PacketCmd.PKT_S2C_QueryStatusAns)
        {
            buffer.Write((byte)1); //ok
        }
    }

    public class Quest : BasePacket
    {
        public Quest(string title, string description, byte type, byte command, uint netid, byte questEvent = 0)
               : base(PacketCmd.PKT_S2C_Quest)
        {
            buffer.Write(Encoding.Default.GetBytes(title));
            buffer.fill(0, 256 - title.Length);
            buffer.Write(Encoding.Default.GetBytes(description));
            buffer.fill(0, 256 - description.Length);
            buffer.Write((byte)type); // 0 : Primary quest, 1 : Secondary quest
            buffer.Write((byte)command); // 0 : Activate quest, 1 : Complete quest, 2 : Remove quest
            buffer.Write((byte)questEvent); // 0 : Roll over, 1 : Roll out, 2 : Mouse down, 3 : Mouse up
            buffer.Write((int)netid);
        }
    }

    public class ChangeCrystalScarNexusHP : BasePacket
    {
        public ChangeCrystalScarNexusHP(TeamId team, int hp) : base(PacketCmd.PKT_S2C_ChangeCrystalScarNexusHP)
        {
            buffer.Write((uint)team);
            buffer.Write(hp);
        }
    }

    public class SynchVersion : BasePacket
    {
        public PacketCmd cmd;
        public int netId;
        public int unk1;
        private byte[] _version = new byte[256]; // version string might be shorter?
        public string version
        {
            get
            {
                var s = Encoding.Default.GetString(_version);
                var idx = s.IndexOf('\0');
                if (idx > 0)
                    return s.Substring(0, idx);
                else
                    return s;
            }
            private set
            {

            }
        }
        public SynchVersion(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            unk1 = reader.ReadInt32();
            _version = reader.ReadBytes(256);
            reader.Close();
        }
    }

    public class WorldSendGameNumber : BasePacket
    {
        public WorldSendGameNumber(long gameId, string name) : base(PacketCmd.PKT_S2C_World_SendGameNumber)
        {
            var data = Encoding.Default.GetBytes(name);
            buffer.Write((long)gameId);
            foreach (var d in data)
                buffer.Write((byte)d);
            buffer.fill(0, 128 - data.Length);
        }
    }

    public class InhibitorStateUpdate : BasePacket
    {
        public InhibitorStateUpdate(Inhibitor inhi) : base(PacketCmd.PKT_S2C_InhibitorState, inhi.NetId)
        {
            buffer.Write((byte)inhi.getState());
            buffer.Write((byte)0);
            buffer.Write((byte)0);
        }
    }

    public class FloatingText : BasePacket
    {
        public FloatingText(Unit u, string text) : base(PacketCmd.PKT_S2C_FloatingText, u.NetId)
        {
            buffer.Write((int)0); // netid?
            buffer.fill(0, 10);
            buffer.Write((int)0); // netid?
            buffer.Write(Encoding.Default.GetBytes(text));
            buffer.Write((byte)0x00);
        }
    }

    public class FloatingText2 : BasePacket
    {
        public FloatingText2(Unit u, string text, byte type, int unk) : base(PacketCmd.PKT_S2C_FloatingText, u.NetId)
        {
            buffer.fill(0, 10);
            buffer.Write((byte)type); // From 0x00 to 0x1B, 0x1C shows nothing and 0x1D bugsplats
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)unk);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write(Encoding.Default.GetBytes(text));
            buffer.Write((byte)0x00);
        }
    }

    public class SetSpellActiveState : BasePacket
    {
        public SetSpellActiveState(Unit u, byte slot, byte state)
               : base(PacketCmd.PKT_S2C_SetSpellActiveState, u.NetId)
        {
            buffer.Write((byte)slot);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)state);
        }
    }

    public class MoveChampionCameraCenter : BasePacket
    {
        public MoveChampionCameraCenter(Champion c, bool enable, byte mode, float distance)
               : base(PacketCmd.PKT_S2C_MoveChampionCameraCenter, c.NetId)
        {
            byte state = 0x00;
            if (enable)
            {
                state = 0x01;
            }
            buffer.Write((byte)state);
            buffer.Write((float)distance); // How much it's moved towards
                                           // where the champion is facing
                                           // (Can be a negative value; ends up behind the champion)
            buffer.fill(0, 8);
            buffer.Write((byte)mode); // Seems to be a bit field.
                                      // First bit 1 : Always in front (or back) of the player
                                      // First bit 0 : Doesn't move when the champion faces another direction
        }
    }

    public class SoundSettings : BasePacket
    {
        public SoundSettings(byte soundCategory, bool mute) : base(PacketCmd.PKT_S2C_SoundSettings)
        {
            buffer.Write((byte)soundCategory);
            buffer.Write(mute);
        }
    }

    public class ForceTargetSpell : BasePacket
    {
        public ForceTargetSpell(Unit u, byte slot, float time)
               : base(PacketCmd.PKT_S2C_ForceTargetSpell, u.NetId)
        {
            buffer.Write((byte)slot);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((float)time);
        }
    }

    public class ReplaceStoreItem : BasePacket
    {
        public ReplaceStoreItem(Unit u, uint replacedItemHash, uint newItemHash)
               : base(PacketCmd.PKT_S2C_ReplaceStoreItem, u.NetId)
        {
            buffer.Write((uint)replacedItemHash);
            buffer.Write((uint)newItemHash);
        }
    }

    public class InhibitorDeathAnimation : BasePacket
    {
        public InhibitorDeathAnimation(Inhibitor inhi, GameObject killer)
               : base(PacketCmd.PKT_S2C_InhibitorDeathAnimation, inhi.NetId)
        {
            if (killer != null)
                buffer.Write((uint)killer.NetId);
            else
                buffer.Write((int)0);
            buffer.Write((int)0); //unk
        }
    }

    public class UnitAnnounce : BasePacket
    {
        public UnitAnnounce(UnitAnnounces id, Unit target, GameObject killer = null, List<Champion> assists = null)
               : base(PacketCmd.PKT_S2C_Announce2, target.NetId)
        {
            if (assists == null)
                assists = new List<Champion>();

            buffer.Write((byte)id);
            if (killer != null)
            {
                buffer.Write((long)killer.NetId);
                buffer.Write((int)assists.Count);
                foreach (var a in assists)
                    buffer.Write((uint)a.NetId);
                for (int i = 0; i < 12 - assists.Count; i++)
                    buffer.Write((int)0);
            }
        }
    }

    public class HideUi : BasePacket
    {
        public HideUi() : base(PacketCmd.PKT_S2C_HideUi)
        {

        }
    }

    public class UnlockCamera : BasePacket
    {
        public UnlockCamera() : base(PacketCmd.PKT_S2C_UnlockCamera)
        {

        }
    }

    public class MoveCamera : BasePacket
    {
        public MoveCamera(Champion champ, float x, float y, float z, float seconds)
               : base(PacketCmd.PKT_S2C_MoveCamera, champ.NetId)
        {
            // Unk, if somebody figures out let @horato know
            buffer.Write((byte)0x97);
            buffer.Write((byte)0xD4);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x58);
            buffer.Write((byte)0xD7);
            buffer.Write((byte)0x17);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xCD);
            buffer.Write((byte)0xED);
            buffer.Write((byte)0x13);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0xA0);
            buffer.Write((byte)0x96);

            buffer.Write((float)x);
            buffer.Write((float)z); // I think this coordinate is ignored
            buffer.Write((float)y);
            buffer.Write((float)seconds);
        }
    }

    public class SetCameraPosition : BasePacket
    {
        public SetCameraPosition(Champion champ, float x, float y, float z)
               : base(PacketCmd.PKT_S2C_SetCameraPosition, champ.NetId)
        {
            buffer.Write((float)x);
            buffer.Write((float)z); // Doesn't seem to matter
            buffer.Write((float)y);
        }
    }

    public class RestrictCameraMovement : BasePacket
    {
        public RestrictCameraMovement(float x, float y, float z, float radius, bool enable)
               : base(PacketCmd.PKT_S2C_RestrictCameraMovement)
        {
            buffer.Write((float)x);
            buffer.Write((float)z);
            buffer.Write((float)y);
            buffer.Write((float)radius);
            buffer.Write(enable);
        }
    }

    public class DisconnectedAnnouncement : BasePacket
    {
        public DisconnectedAnnouncement(Unit unit) : base(PacketCmd.PKT_S2C_DisconnectedAnnouncement)
        {
            buffer.Write(unit.NetId);
        }
    }

    public class SetCapturePoint : BasePacket
    {
        public SetCapturePoint(Unit unit, byte capturePointId) : base(PacketCmd.PKT_S2C_SetCapturePoint)
        {
            buffer.Write((byte)capturePointId);
            buffer.Write(unit.NetId);
            buffer.fill(0, 6);
        }
    }

    public class SetTeam : BasePacket
    {
        public SetTeam(Unit unit, TeamId team) : base(PacketCmd.PKT_S2C_SetTeam)
        {
            buffer.Write(unit.NetId);
            buffer.Write((int)team);
        }
    }

    public class PlaySound : BasePacket
    {
        public PlaySound(Unit unit, string soundName) : base(PacketCmd.PKT_S2C_PlaySound, unit.NetId)
        {
            buffer.Write(Encoding.Default.GetBytes(soundName));
            buffer.fill(0, 1024 - soundName.Length);
            buffer.Write(unit.NetId); // audioEventNetworkID?
        }
    }

    public class CloseGame : BasePacket
    {
        public CloseGame() : base(PacketCmd.PKT_S2C_CloseGame)
        {
        }
    }

    public class AttachMinimapIcon : BasePacket
    {
        public AttachMinimapIcon(Unit unit, byte unk1, string iconName, byte unk2, string unk3, string unk4)
               : base(PacketCmd.PKT_S2C_AttachMinimapIcon)
        {
            buffer.Write(unit.NetId);
            buffer.Write((byte)unk1);
            buffer.Write(Encoding.Default.GetBytes(iconName)); // This is probably the icon name, but sometimes it's empty
            buffer.fill(0, 64 - iconName.Length);              // Example: "Quest"
            buffer.Write((byte)unk2);
            buffer.Write(Encoding.Default.GetBytes(unk3));
            buffer.fill(0, 64 - unk3.Length); // Example: "Recall"
            buffer.Write(Encoding.Default.GetBytes(unk4));
            buffer.fill(0, 64 - unk4.Length); // Example "OdinRecall", "odinrecallimproved"
        }
    }

    public class ShowHPAndName : BasePacket
    {
        public ShowHPAndName(Unit unit, bool show) : base(PacketCmd.PKT_S2C_ShowHPAndName, unit.NetId)
        {
            buffer.Write(show);
            buffer.Write((byte)0x00);
        }
    }

    public class SpellEmpower : BasePacket
    {
        public SpellEmpower(Unit unit, byte slot, byte empowerLevel) : base(PacketCmd.PKT_S2C_SpellEmpower, unit.NetId)
        {
            buffer.Write((byte)slot);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x06); // Unknown
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)empowerLevel); // 0 - normal, 1 - empowered (for Rengar)
        }
    }

    public class ExplodeNexus : BasePacket
    {
        public ExplodeNexus(Nexus nexus) : base(PacketCmd.PKT_S2C_ExplodeNexus, nexus.NetId)
        {
            // animation ID?
            buffer.Write((byte)0xE7);
            buffer.Write((byte)0xF9);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x40);
            // unk
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
        }
    }

    public class ChatMessage
    {
        public PacketCmd cmd;
        public int playerId;
        public int botNetId;
        public byte isBotMessage;

        public ChatType type;
        public int unk1; // playerNo?
        public int length;
        public byte[] unk2 = new byte[32];
        public string msg;

        public ChatMessage(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            playerId = reader.ReadInt32();
            botNetId = reader.ReadInt32();
            isBotMessage = reader.ReadByte();
            type = (ChatType)reader.ReadInt32();
            unk1 = reader.ReadInt32();
            length = reader.ReadInt32();
            unk2 = reader.ReadBytes(32);

            var bytes = new List<byte>();
            for (var i = 0; i < length; i++)
                bytes.Add(reader.ReadByte());
            msg = Encoding.Default.GetString(bytes.ToArray());
        }
    }

    public class UpdateModel : BasePacket
    {
        public UpdateModel(uint netID, string modelName, bool useSpells = true)
               : base(PacketCmd.PKT_S2C_UpdateModel, netID)
        {
            buffer.Write(useSpells); // Use spells from the new model
            buffer.Write((byte)0x00); // <-- These three bytes most likely form
            buffer.Write((byte)0x00); // <-- an int with the useSpells byte, but
            buffer.Write((byte)0x00); // <-- they don't seem to affect anything
            buffer.Write((byte)1); // Bit field with bits 1 and 2. Unk
            buffer.Write((int)-1); // SkinID (Maybe -1 means keep using current one?)
            foreach (var b in Encoding.Default.GetBytes(modelName))
                buffer.Write((byte)b);
            if (modelName.Length < 32)
                buffer.fill(0, 32 - modelName.Length);
        }
    }

    public class ChangeCharacterVoice : BasePacket
    {
        public ChangeCharacterVoice(uint netID, string voiceOverride, bool resetOverride = false)
               : base(PacketCmd.PKT_S2C_ChangeCharacterVoice, netID)
        {
            buffer.Write(resetOverride); // If this is 1, resets voice to default state and ignores voiceOverride
            foreach (var b in Encoding.Default.GetBytes(voiceOverride))
                buffer.Write((byte)b);
            if (voiceOverride.Length < 32)
                buffer.fill(0, 32 - voiceOverride.Length);
        }
    }

    public class StatePacket : BasePacket
    {
        public StatePacket(PacketCmd state) : base(state)
        {

        }
    }
    public class StatePacket2 : BasePacket
    {
        public StatePacket2(PacketCmd state) : base(state)
        {
            buffer.Write((short)0); //unk
        }
    }

    public class Click
    {
        PacketCmd cmd;
        int netId;
        public int zero;
        public uint targetNetId; // netId on which the player clicked

        public Click(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            zero = reader.ReadInt32();
            targetNetId = reader.ReadUInt32();
        }
    }

    public class UseObject
    {
        PacketCmd cmd;
        int netId;
        public uint targetNetId; // netId of the object used

        public UseObject(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            targetNetId = reader.ReadUInt32();
        }
    }

    public class HeroSpawn : BasePacket
    {

        public HeroSpawn(ClientInfo player, int playerId) : base(PacketCmd.PKT_S2C_HeroSpawn)
        {
            buffer.Write((int)player.Champion.NetId);
            buffer.Write((int)playerId); // player Id
            buffer.Write((byte)40); // netNodeID ?
            buffer.Write((byte)0); // botSkillLevel Beginner=0 Intermediate=1
            if (player.Team == TeamId.TEAM_BLUE)
            {
                buffer.Write((byte)1); // teamNumber BotTeam=2,3 Blue=Order=1 Purple=Chaos=0
            }
            else
            {
                buffer.Write((byte)0); // teamNumber BotTeam=2,3 Blue=Order=1 Purple=Chaos=0
            }
            buffer.Write((byte)0); // isBot
                                   //buffer.Write((short)0; // botRank (deprecated as of 4.18)
            buffer.Write((byte)0); // spawnPosIndex
            buffer.Write((int)player.SkinNo);
            foreach (var b in Encoding.Default.GetBytes(player.Name))
                buffer.Write((byte)b);
            buffer.fill(0, 128 - player.Name.Length);
            foreach (var b in Encoding.Default.GetBytes(player.Champion.Model))
                buffer.Write((byte)b);
            buffer.fill(0, 40 - player.Champion.Model.Length);
            buffer.Write((float)0.0f); // deathDurationRemaining
            buffer.Write((float)0.0f); // timeSinceDeath
            buffer.Write((int)0); // UNK (4.18)
            buffer.Write((byte)0); // bitField
        }
    }

    public class HeroSpawn2 : BasePacket
    {
        public HeroSpawn2(Champion p) : base(PacketCmd.PKT_S2C_ObjectSpawn, p.NetId)
        {
            buffer.fill(0, 15);
            buffer.Write((byte)0x80); // unk
            buffer.Write((byte)0x3F); // unk
            buffer.fill(0, 13);
            buffer.Write((byte)3); // unk
            buffer.Write((uint)1); // unk
            buffer.Write(p.X);
            buffer.Write(p.Y);
            buffer.Write((float)0x3F441B7D); // z ?
            buffer.Write((float)0x3F248DBB); // Rotation ?
        }
    }

    public class TurretSpawn : BasePacket //TODO: check
    {
        public TurretSpawn(BaseTurret t) : base(PacketCmd.PKT_S2C_TurretSpawn)
        {
            buffer.Write((int)t.NetId);
            buffer.Write((byte)0x40);
            foreach (var b in Encoding.Default.GetBytes(t.Name))
                buffer.Write((byte)b);
            buffer.fill(0, 64 - t.Name.Length);
            buffer.Write((byte)0x0C);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x80);
            buffer.Write((byte)0x01);
        }
    }

    public class BlueTip : BasePacket
    {
        public BlueTip(string title,
                       string text,
                       string imagePath,
                       byte tipCommand,
                       uint playernetid,
                       uint netid)
               : base(PacketCmd.PKT_S2C_BlueTip, playernetid)
        {
            foreach (var b in Encoding.Default.GetBytes(text))
                buffer.Write(b);
            buffer.fill(0, 128 - text.Length);
            foreach (var b in Encoding.Default.GetBytes(title))
                buffer.Write(b);
            buffer.fill(0, 128 - title.Length);
            foreach (var b in Encoding.Default.GetBytes(imagePath))
                buffer.Write(b);
            buffer.fill(0, 128 - imagePath.Length);
            buffer.Write((byte)tipCommand); /* ACTIVATE_TIP     = 0
                                               REMOVE_TIP       = 1
                                               ENABLE_TIP_EVENTS  = 2
                                               DISABLE_TIP_EVENTS  = 3
                                               ACTIVATE_TIP_DIALOGUE  = 4
                                               ENABLE_TIP_DIALOGUE_EVENTS  = 5
                                               DISABLE_TIP_DIALOGUE_EVENTS  = 6 */
            buffer.Write((int)netid);
        }
    }

    public class BlueTipClicked
    {
        public byte cmd;
        public uint playernetid;
        public byte unk;
        public uint netid;

        public BlueTipClicked(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = reader.ReadByte();
            playernetid = reader.ReadUInt32();
            unk = reader.ReadByte();
            netid = reader.ReadUInt32();
        }
        public BlueTipClicked()
        {

        }
    }

    public class QuestClicked
    {
        public byte cmd;
        public uint playernetid;
        public byte unk;
        public uint netid;

        public QuestClicked(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = reader.ReadByte();
            playernetid = reader.ReadUInt32();
            unk = reader.ReadByte();
            netid = reader.ReadUInt32();
        }
        public QuestClicked()
        {

        }
    }

    public class AutoAttackOption
    {
        public byte cmd;
        public int netid;
        public byte activated;

        public AutoAttackOption(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = reader.ReadByte();
            netid = reader.ReadInt32();
            activated = reader.ReadByte();
        }
        public AutoAttackOption()
        {

        }
    }

    public class GameTimer : BasePacket
    {
        public GameTimer(float fTime) : base(PacketCmd.PKT_S2C_GameTimer, 0)
        {
            buffer.Write((float)fTime);
        }
    }

    public class GameTimerUpdate : BasePacket
    {
        public GameTimerUpdate(float fTime) : base(PacketCmd.PKT_S2C_GameTimerUpdate, 0)
        {
            buffer.Write((float)fTime);
        }
    }

    public class HeartBeat
    {
        public PacketCmd cmd;
        public int netId;
        public float receiveTime;
        public float ackTime;
        public HeartBeat(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            receiveTime = reader.ReadSingle();
            ackTime = reader.ReadSingle();
            reader.Close();
        }
    }

    public class SkillUpPacket : BasePacket
    {
        public PacketCmd cmd;
        public uint netId;
        public byte skill;
        public SkillUpPacket(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadUInt32();
            skill = reader.ReadByte();
        }
        public SkillUpPacket(uint netId, byte skill, byte level, byte pointsLeft)
               : base(PacketCmd.PKT_S2C_SkillUp, netId)
        {
            buffer.Write(skill);
            buffer.Write(level);
            buffer.Write(pointsLeft);
        }
    }

    public class BuyItemReq
    {
        PacketCmd cmd;
        int netId;
        public int id;
        public BuyItemReq(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            id = reader.ReadInt32();
        }
    }

    public class BuyItemAns : BasePacket
    {
        public BuyItemAns(Unit actor, Item item, byte unk = 0x29) : base(PacketCmd.PKT_S2C_BuyItemAns, actor.NetId)
        {
            buffer.Write((int)item.ItemType.ItemId);
            buffer.Write((byte)actor.Inventory.GetItemSlot(item));
            buffer.Write((byte)item.StackSize);
            buffer.Write((byte)0); //unk or stacks => short
            buffer.Write((byte)unk); //unk (turret 0x01 and champions 0x29)
        }
    }

    public class SellItem
    {
        public PacketCmd cmd;
        public int netId;
        public byte slotId;

        public SellItem(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            slotId = reader.ReadByte();
        }
    }


    public class RemoveItem : BasePacket
    {
        public RemoveItem(Unit u, byte slot, short remaining) : base(PacketCmd.PKT_S2C_RemoveItem, u.NetId)
        {
            buffer.Write(slot);
            buffer.Write(remaining);
        }
    }

    public class EmotionPacket : BasePacket
    {
        public PacketCmd cmd;
        public uint netId;
        public byte id;

        public EmotionPacket(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadUInt32();
            id = reader.ReadByte();
        }

        public EmotionPacket(byte id, uint netId) : base(PacketCmd.PKT_S2C_Emotion, netId)
        {
            buffer.Write((byte)id);
        }
    }

    public class SwapItems : BasePacket
    {
        public PacketCmd cmd;
        public int netId;
        public byte slotFrom;
        public byte slotTo;
        public SwapItems(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            slotFrom = reader.ReadByte();
            slotTo = reader.ReadByte();
        }

        public SwapItems(Champion c, byte slotFrom, byte slotTo) : base(PacketCmd.PKT_S2C_SwapItems, c.NetId)
        {
            buffer.Write((byte)slotFrom);
            buffer.Write((byte)slotTo);
        }
    }

    class Announce : BasePacket
    {
        public Announce(Announces messageId, int mapId = 0) : base(PacketCmd.PKT_S2C_Announce)
        {
            buffer.Write((byte)messageId);
            buffer.Write((long)0);

            if (mapId > 0)
            {
                buffer.Write(mapId);
            }
        }
    }

    public class AddBuff : BasePacket
    {
        public AddBuff(Unit u, Unit source, int stacks, float time, BuffType buffType, string name, byte slot)
               : base(PacketCmd.PKT_S2C_AddBuff, u.NetId)
        {
            buffer.Write((byte)slot); //Slot
            buffer.Write((byte)buffType); //Type
            buffer.Write((byte)stacks); // stacks
            buffer.Write((byte)0x00); // Visible was (byte)0x00
            buffer.Write(_rafManager.GetHash(name)); //Buff id
            
            buffer.Write((int)0); // <-- Probably runningTime

            buffer.Write((float)0); // <- ProgressStartPercent

            buffer.Write((float)time);

            if (source != null)
            {
                buffer.Write(source.NetId); //source
            }
            else
            {
                buffer.Write((int)0);
            }
        }
    }

    public class AddXP : BasePacket
    {
        public AddXP(Unit u, float xp) : base(PacketCmd.PKT_S2C_AddXP)
        {
            buffer.Write(u.NetId);
            buffer.Write(xp);
        }
    }

    public class EditBuff : BasePacket
    {
        public EditBuff(Unit u, byte slot, byte stacks) : base(PacketCmd.PKT_S2C_EditBuff, u.NetId)
        {
            buffer.Write(slot);//slot
            buffer.Write(stacks);//stacks
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x50);
            buffer.Write((byte)0xC3);
            buffer.Write((byte)0x46);
            buffer.Write(0);
            buffer.Write(u.NetId);

        }
    }

    public class RemoveBuff : BasePacket
    {
        public RemoveBuff(Unit u, string name, byte slot) : base(PacketCmd.PKT_S2C_RemoveBuff, u.NetId)
        {
            buffer.Write((byte)slot);
            buffer.Write(_rafManager.GetHash(name));
            buffer.Write((int)0x0);
            //buffer.Write(u.NetId);//source?
        }
    }

    public class DamageDone : BasePacket
    {
        public DamageDone(Unit source, Unit target, float amount, DamageType type, DamageText damageText)
               : base(PacketCmd.PKT_S2C_DamageDone, target.NetId)
        {
            buffer.Write((byte)damageText);
            buffer.Write((short)((short)type << 8));
            buffer.Write((float)amount);
            buffer.Write((int)target.NetId);
            buffer.Write((int)source.NetId);
        }
    }

    public class NpcDie : BasePacket
    {
        public NpcDie(Unit die, Unit killer) : base(PacketCmd.PKT_S2C_NPC_Die, die.NetId)
        {
            buffer.Write((int)0);
            buffer.Write((byte)0);
            buffer.Write(killer.NetId);
            buffer.Write((byte)0); // unk
            buffer.Write((byte)7); // unk
            buffer.Write((int)0); // Flags?
        }
    }

    public class AttentionPing
    {
        public byte cmd;
        public int unk1;
        public float x;
        public float y;
        public int targetNetId;
        public Pings type;
        public AttentionPing(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = reader.ReadByte();
            unk1 = reader.ReadInt32();
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            targetNetId = reader.ReadInt32();
            type = (Pings)reader.ReadByte();
        }
        public AttentionPing()
        {

        }
    }

    public class CursorPositionOnWorld
    {
        public byte Cmd;
        public uint NetId;
        public short Unk1; // Maybe 2 bytes instead of 1 short?
        public float X;
        public float Z;
        public float Y;
        public CursorPositionOnWorld(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            Cmd = reader.ReadByte();
            NetId = reader.ReadUInt32();
            Unk1 = reader.ReadInt16();
            X = reader.ReadSingle();
            Z = reader.ReadSingle();
            Y = reader.ReadSingle();
        }
    }

    public class AttentionPingAns : BasePacket
    {
        public AttentionPingAns(ClientInfo player, AttentionPing ping) : base(PacketCmd.PKT_S2C_AttentionPing)
        {
            buffer.Write((float)ping.x);
            buffer.Write((float)ping.y);
            buffer.Write((int)ping.targetNetId);
            buffer.Write((int)player.Champion.NetId);
            buffer.Write((byte)ping.type);
            buffer.Write((byte)0xFB); // 4.18
                                      /*
                                      switch (ping.type)
                                      {
                                         case 0:
                                            buffer.Write((short)0xb0;
                                            break;
                                         case 1:
                                            buffer.Write((short)0xb1;
                                            break;
                                         case 2:
                                            buffer.Write((short)0xb2; // Danger
                                            break;
                                         case 3:
                                            buffer.Write((short)0xb3; // Enemy Missing
                                            break;
                                         case 4:
                                            buffer.Write((short)0xb4; // On My Way
                                            break;
                                         case 5:
                                            buffer.Write((short)0xb5; // Retreat / Fall Back
                                            break;
                                         case 6:
                                            buffer.Write((short)0xb6; // Assistance Needed
                                            break;
                                      }
                                      */
        }
    }

    public class BeginAutoAttack : BasePacket
    {
        public BeginAutoAttack(Unit attacker, Unit attacked, uint futureProjNetId, bool isCritical)
               : base(PacketCmd.PKT_S2C_BeginAutoAttack, attacker.NetId)
        {
            buffer.Write(attacked.NetId);
            buffer.Write((byte)0x80); // extraTime
            buffer.Write(futureProjNetId); // Basic attack projectile ID, to be spawned later
            if (isCritical)
                buffer.Write((byte)0x49); // attackSlot
            else
                buffer.Write((byte)0x40); // attackSlot

            buffer.Write((byte)0x80); // not sure what this is, but it should be correct (or maybe attacked x z y?) - 4.18
            buffer.Write((byte)0x01);
            buffer.Write(MovementVector.TargetXToNormalFormat(attacked.X));
            buffer.Write((byte)0x80);
            buffer.Write((byte)0x01);
            buffer.Write(MovementVector.TargetYToNormalFormat(attacked.Y));
            buffer.Write((byte)0xCC);
            buffer.Write((byte)0x35);
            buffer.Write((byte)0xC4);
            buffer.Write((byte)0xD1);
            buffer.Write(attacker.X);
            buffer.Write(attacker.Y);
        }
    }

    public class NextAutoAttack : BasePacket
    {

        public NextAutoAttack(Unit attacker, Unit attacked, uint futureProjNetId, bool isCritical, bool initial)
               : base(PacketCmd.PKT_S2C_NextAutoAttack, attacker.NetId)
        {
            buffer.Write(attacked.NetId);
            if (initial)
                buffer.Write((byte)0x80); // extraTime
            else
                buffer.Write((byte)0x7F); // extraTime

            buffer.Write(futureProjNetId);
            if (isCritical)
                buffer.Write((byte)0x49); // attackSlot
            else
                buffer.Write((byte)0x40); // attackSlot

            buffer.Write((byte)0x40);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x7B);
            buffer.Write((byte)0xEF);
            buffer.Write((byte)0xEF);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x2E);
            buffer.Write((byte)0x55);
            buffer.Write((byte)0x55);
            buffer.Write((byte)0x35);
            buffer.Write((byte)0x94);
            buffer.Write((byte)0xD3);
        }
    }

    public class StopAutoAttack : BasePacket
    {

        public StopAutoAttack(Unit attacker) : base(PacketCmd.PKT_S2C_StopAutoAttack, attacker.NetId)
        {
            buffer.Write((byte)0); // Flag
            buffer.Write((int)0); // A netId
        }
    }

    public class OnAttack : BasePacket
    {
        public OnAttack(Unit attacker, Unit attacked, AttackType attackType)
               : base(PacketCmd.PKT_S2C_OnAttack, attacker.NetId)
        {
            buffer.Write((byte)attackType);
            buffer.Write(attacked.X);
            buffer.Write(attacked.GetZ());
            buffer.Write(attacked.Y);
            buffer.Write(attacked.NetId);
        }
    }

    public class Surrender : BasePacket
    {
        public Surrender(Unit starter, byte flag, byte yesVotes, byte noVotes, byte maxVotes, TeamId team, float timeOut)
        : base(PacketCmd.PKT_S2C_Surrender)
        {
            buffer.Write((byte)flag); // Flag. 2 bits
            buffer.Write((uint)starter.NetId);
            buffer.Write((byte)yesVotes);
            buffer.Write((byte)noVotes);
            buffer.Write((byte)maxVotes);
            buffer.Write((int)team);
            buffer.Write((float)timeOut);
        }
    }

    public class UnpauseGame : BasePacket
    {
        public UnpauseGame(uint unpauserNetId, bool showWindow) : base(PacketCmd.PKT_UnpauseGame)
        {
            buffer.Write((uint)unpauserNetId);
            buffer.Write(showWindow);
        }
    }

    public class GameSpeed : BasePacket
    {
        public GameSpeed(float gameSpeed) : base(PacketCmd.PKT_S2C_GameSpeed)
        {
            buffer.Write((float)gameSpeed);
        }
    }

    public class SurrenderState : BasePacket
    {
        public SurrenderState(uint playernetid, byte state) : base(PacketCmd.PKT_S2C_SurrenderState, playernetid)
        {
            buffer.Write((byte)state);
        }
    }

    public class ResourceType : BasePacket
    {
        public ResourceType(uint playernetid, byte resourceType) : base(PacketCmd.PKT_S2C_ResourceType, playernetid)
        {
            buffer.Write((byte)resourceType);
        }
    }

    public class CreateMonsterCamp : BasePacket
    {
        public CreateMonsterCamp(float x, float y, float z, string iconName, byte campId, byte campUnk, float unk)
               : base(PacketCmd.PKT_S2C_CreateMonsterCamp)
        {
            buffer.Write((float)x);
            buffer.Write((float)z);
            buffer.Write((float)y);
            buffer.Write(Encoding.Default.GetBytes(iconName));
            buffer.fill(0, 64 - iconName.Length);
            buffer.Write((byte)campId);
            buffer.Write((byte)campUnk);

            /*buffer.Write((byte)0x64); // <-|
            buffer.Write((byte)0x15); //   |
            buffer.Write((byte)0xFB); //   |-> Unk
            buffer.Write((byte)0x41); //   |
            buffer.Write((byte)0x0C); // <-|*/
            buffer.fill(0, 5);
            buffer.Write((float)unk);
        }
    }

    public class SurrenderResult : BasePacket
    {
        public SurrenderResult(bool reason, int yes, int no, TeamId team) : base(PacketCmd.PKT_S2C_SurrenderResult)
        {
            buffer.Write(reason); //surrendererNetworkId
            buffer.Write((byte)yes); //yesVotes
            buffer.Write((byte)no); //noVotes
            buffer.Write((int)team); //team
        }
    }

    public class GameEnd : BasePacket
    {
        public GameEnd(bool winningTeamIsBlue) : base(PacketCmd.PKT_S2C_GameEnd)
        {
            buffer.Write(winningTeamIsBlue ? (byte)1 : (byte)0);
        }
    }

    public class SetTarget : BasePacket
    {

        public SetTarget(Unit attacker, Unit attacked) : base(PacketCmd.PKT_S2C_SetTarget, attacker.NetId)
        {
            if (attacked != null)
            {
                buffer.Write(attacked.NetId);
            }
            else
            {
                buffer.Write((int)0);
            }
        }

    }

    public class SetTarget2 : BasePacket
    {

        public SetTarget2(Unit attacker, Unit attacked) : base(PacketCmd.PKT_S2C_SetTarget2, attacker.NetId)
        {
            if (attacked != null)
            {
                buffer.Write(attacked.NetId);
            }
            else
            {
                buffer.Write((int)0);
            }
        }

    }

    public class ChampionDie : BasePacket
    {

        public ChampionDie(Champion die, Unit killer, int goldFromKill) : base(PacketCmd.PKT_S2C_ChampionDie, die.NetId)
        {
            buffer.Write(goldFromKill); // Gold from kill?
            buffer.Write((byte)0);
            if (killer != null)
                buffer.Write(killer.NetId);
            else
                buffer.Write((int)0);

            buffer.Write((byte)0);
            buffer.Write((byte)7);
            buffer.Write(die.RespawnTimer / 1000.0f); // Respawn timer, float
        }
    }

    public class ChampionDeathTimer : BasePacket
    {

        public ChampionDeathTimer(Champion die) : base(PacketCmd.PKT_S2C_ChampionDeathTimer, die.NetId)
        {
            buffer.Write(die.RespawnTimer / 1000.0f); // Respawn timer, float
        }
    }

    public class ChampionRespawn : BasePacket
    {
        public ChampionRespawn(Champion c) : base(PacketCmd.PKT_S2C_ChampionRespawn, c.NetId)
        {
            buffer.Write(c.X);
            buffer.Write(c.Y);
            buffer.Write(c.GetZ());
        }
    }

    public class ShowProjectile : BasePacket
    {

        public ShowProjectile(Projectile p) : base(PacketCmd.PKT_S2C_ShowProjectile, p.Owner.NetId)
        {
            buffer.Write(p.NetId);
        }
    }

    public class SetHealth : BasePacket
    {
        public SetHealth(Unit u) : base(PacketCmd.PKT_S2C_SetHealth, u.NetId)
        {
            buffer.Write((short)0x0000); // unk,maybe flags for physical/magical/true dmg
            buffer.Write((float)u.GetStats().HealthPoints.Total);
            buffer.Write((float)u.GetStats().CurrentHealth);
        }

        public SetHealth(uint itemHash) : base(PacketCmd.PKT_S2C_SetHealth, itemHash)
        {
            buffer.Write((short)0);
        }

    }

    public class SetScreenTint : BasePacket
    {
        public SetScreenTint(TeamId team, bool enable, float transitionTime, byte red, byte green, byte blue, float alpha)
               : base(PacketCmd.PKT_S2C_SetScreenTint)
        {
            buffer.Write(enable);
            buffer.Write((float)transitionTime); // Transition time in seconds
            buffer.Write((int)team);
            buffer.Write((byte)blue);
            buffer.Write((byte)green);
            buffer.Write((byte)red);
            buffer.Write((byte)0xFF); // Unk
            buffer.Write((float)alpha);
        }
    }

    public class SetModelTransparency : BasePacket
    {
        public SetModelTransparency(Unit u, float transparency, float transitionTime)
               : base(PacketCmd.PKT_S2C_SetModelTransparency, u.NetId)
        {
            // Applied to Teemo's mushrooms for example
            buffer.Write((byte)0xDB); // Unk
            buffer.Write((byte)0x00); // Unk
            buffer.Write((float)transitionTime);
            buffer.Write((float)transparency); // 0.0 : fully transparent, 1.0 : fully visible
        }
    }

    public class TeleportRequest : BasePacket
    {
        static short a = 0x01;
        public TeleportRequest(uint netId, float x, float y, bool first) : base(PacketCmd.PKT_S2C_MoveAns)
        {
            buffer.Write((int)Environment.TickCount); // syncID
            buffer.Write((byte)0x01); // Unk
            buffer.Write((byte)0x00); // Unk
            if (first == true) //seems to be id, 02 = before teleporting, 03 = do teleport
                buffer.Write((byte)0x02);
            else
                buffer.Write((byte)0x03);
            buffer.Write((int)netId);
            if (first == false)
            {
                buffer.Write((byte)a); // if it is the second part, send 0x01 before coords
                a++;
            }
            buffer.Write((short)x);
            buffer.Write((short)y);
        }

    }

    public class CastSpell
    {
        public PacketCmd cmd;
        public int netId;
        public byte spellSlotType; // 4.18 [deprecated? . 2 first(highest) bits: 10 - ability or item, 01 - summoner spell]
        public byte spellSlot; // 0-3 [0-1 if spellSlotType has summoner spell bits set]
        public float x; // Initial point
        public float y; // (e.g. Viktor E laser starting point)
        public float x2; // Final point
        public float y2; // (e.g. Viktor E laser final point)
        public uint targetNetId; // If 0, use coordinates, else use target net id

        public CastSpell(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            spellSlotType = reader.ReadByte();
            spellSlot = reader.ReadByte();
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            x2 = reader.ReadSingle();
            y2 = reader.ReadSingle();
            targetNetId = reader.ReadUInt32();
        }
    }

    public class CastSpellAns : BasePacket
    {
        public CastSpellAns(Spell s, float x, float y, float xDragEnd, float yDragEnd, uint futureProjNetId, uint spellNetId)
               : base(PacketCmd.PKT_S2C_CastSpellAns, s.Owner.NetId)
        {
            var m = _game.Map;

            buffer.Write(Environment.TickCount); // syncID
            buffer.Write((byte)0); // Unk
            buffer.Write((short)0x66); // Buffer size from here
            buffer.Write((int)s.getId()); // Spell hash, for example hash("EzrealMysticShot")
            buffer.Write((uint)spellNetId); // Spell net ID
            buffer.Write((byte)(s.Level - 1));
            buffer.Write((float)1.0f); // attackSpeedMod
            buffer.Write((uint)s.Owner.NetId);
            buffer.Write((uint)s.Owner.NetId);
            buffer.Write((int)s.Owner.getChampionHash());
            buffer.Write((uint)futureProjNetId); // The projectile ID that will be spawned
            buffer.Write((float)x);
            buffer.Write((float)m.GetHeightAtLocation(x, y));
            buffer.Write((float)y);
            buffer.Write((float)xDragEnd);
            buffer.Write((float)m.GetHeightAtLocation(xDragEnd, yDragEnd));
            buffer.Write((float)yDragEnd);
            buffer.Write((byte)0); // numTargets (if >0, what follows is a list of {uint32 targetNetId, uint8 hitResult})
            buffer.Write((float)s.CastTime); // designerCastTime
            buffer.Write((float)0.0f); // extraTimeForCast
            buffer.Write((float)s.CastTime /*+ s.ChannelTime*/); // designerTotalTime
            buffer.Write((float)s.getCooldown());
            buffer.Write((float)0.0f); // startCastTime
            buffer.Write((byte)0); // flags (isAutoAttack, secondAttack, forceCastingOrChannelling, mShouldOverrideCastPosition)
            buffer.Write((byte)s.Slot);
            buffer.Write((float)s.getCost());
            buffer.Write((float)s.Owner.X);
            buffer.Write((float)s.Owner.GetZ());
            buffer.Write((float)s.Owner.Y);
            buffer.Write((long)1); // Unk
        }
    }

    public class AvatarInfo : BasePacket
    {
        public AvatarInfo(ClientInfo player) : base(PacketCmd.PKT_S2C_AvatarInfo, player.Champion.NetId)
        {
            int runesRequired = 30;
            foreach (var rune in player.Champion.RuneList._runes)
            {
                buffer.Write((short)rune.Value);
                buffer.Write((short)0x00);
                runesRequired--;
            }
            for (int i = 1; i <= runesRequired; i++)
            {
                buffer.Write((short)0);
                buffer.Write((short)0);
            }

            var summonerSpells = player.SummonerSkills;
            buffer.Write((uint)_rafManager.GetHash(summonerSpells[0]));
            buffer.Write((uint)_rafManager.GetHash(summonerSpells[1]));

            int talentsRequired = 80;
            var talentsHashes = new Dictionary<int, byte>(){
                { 0, 0 } // hash, level
            };

            foreach (var talent in talentsHashes)
            {
                buffer.Write((int)talent.Key); // hash
                buffer.Write((byte)talent.Value); // level
                talentsRequired--;
            }
            for (int i = 1; i <= talentsRequired; i++)
            {
                buffer.Write((int)0);
                buffer.Write((byte)0);
            }

            buffer.Write((short)30); // avatarLevel
        }
    }

    public class SpawnProjectile : BasePacket
    {
        public SpawnProjectile(Projectile p) : base(PacketCmd.PKT_S2C_SpawnProjectile, p.NetId)
        {
            float targetZ = _game.Map.GetHeightAtLocation(p.Target.X, p.Target.Y);

            buffer.Write((float)p.X);
            buffer.Write((float)p.GetZ()+100.0f);
            buffer.Write((float)p.Y);
            buffer.Write((float)p.X);
            buffer.Write((float)p.GetZ());
            buffer.Write((float)p.Y);
            buffer.Write((float)-0.992436f); // Rotation X
            buffer.Write((int)0); // Rotation Z
            buffer.Write((float)-0.122766f); // Rotation Y
            buffer.Write((float)-1984.871338f); // Unk
            buffer.Write((float)-166.666656f); // Unk
            buffer.Write((float)-245.531418f); // Unk
            buffer.Write((float)p.X);
            buffer.Write((float)p.GetZ()+100.0f);
            buffer.Write((float)p.Y);
            buffer.Write((float)p.Target.X);
            buffer.Write((float)_game.Map.GetHeightAtLocation(p.Target.X, p.Target.Y));
            buffer.Write((float)p.Target.Y);
            buffer.Write((float)p.X);
            buffer.Write((float)p.GetZ());
            buffer.Write((float)p.Y);
            buffer.Write((int)0); // Unk ((float)castDelay ?)
            buffer.Write((float)p.getMoveSpeed()); // Projectile speed
            buffer.Write((int)0); // Unk
            buffer.Write((int)0); // Unk
            buffer.Write((int)0x7f7fffff); // Unk
            buffer.Write((byte)0); // Unk
            if (!p.Target.IsSimpleTarget)
            {
                buffer.Write((short)0x6B); // Buffer size from here
            }
            else
            {
                buffer.Write((short)0x66); // Buffer size from here
            }
            buffer.Write((int)p.ProjectileId); // projectile ID (hashed name)
            buffer.Write((int)0); // Second net ID
            buffer.Write((byte)0); // spellLevel
            buffer.Write((float)1.0f); // attackSpeedMod
            buffer.Write((int)p.Owner.NetId);
            buffer.Write((int)p.Owner.NetId);

            var c = p.Owner as Champion;
            if (c != null)
            {
                buffer.Write((int)c.getChampionHash());
            }
            else
            {
                buffer.Write((int)0);
            }

            buffer.Write((int)p.NetId);
            buffer.Write((float)p.Target.X);
            buffer.Write((float)_game.Map.GetHeightAtLocation(p.Target.X, p.Target.Y));
            buffer.Write((float)p.Target.Y);
            buffer.Write((float)p.Target.X);
            buffer.Write((float)_game.Map.GetHeightAtLocation(p.Target.X, p.Target.Y)+100.0f);
            buffer.Write((float)p.Target.Y);
            if (!p.Target.IsSimpleTarget)
            {
                buffer.Write((byte)0x01); // numTargets
                buffer.Write((p.Target as Unit).NetId);
                buffer.Write((byte)0); // hitResult
            }
            else
            {
                buffer.Write((byte)0); // numTargets
            }
            buffer.Write((float)1.0f); // designerCastTime -- Doesn't seem to matter
            buffer.Write((int)0); // extraTimeForCast -- Doesn't seem to matter
            buffer.Write((float)1.0f); // designerTotalTime -- Doesn't seem to matter
            buffer.Write((float)0.0f); // cooldown -- Doesn't seem to matter
            buffer.Write((float)0.0f); // startCastTime -- Doesn't seem to matter
            buffer.Write((byte)0x00); // flags?
            buffer.Write((byte)0x30); // slot?
            buffer.Write((float)0.0f); // manaCost?
            buffer.Write((float)p.X);
            buffer.Write((float)p.GetZ());
            buffer.Write((float)p.Y);
            buffer.Write((int)0); // Unk
            buffer.Write((int)0); // Unk
        }

    }

    public class FogUpdate2 : BasePacket
    {
        private NetworkIdManager _networkManager = Program.ResolveDependency<NetworkIdManager>();

        public FogUpdate2(Unit unit) : base(PacketCmd.PKT_S2C_FogUpdate2, 0)
        {
            buffer.Write((int)unit.Team);
            buffer.Write((byte)0xFE);
            buffer.Write((byte)0xFF);
            buffer.Write((byte)0xFF);
            buffer.Write((byte)0xFF);
            buffer.Write((int)0);
            buffer.Write((uint)unit.NetId); // Fog Attached, when unit dies it disappears
            buffer.Write((uint)_networkManager.GetNewNetID()); //Fog NetID
            buffer.Write((int)0);
            buffer.Write((float)unit.X);
            buffer.Write((float)unit.Y);
            buffer.Write((float)2500);
            buffer.Write((float)88.4f);
            buffer.Write((float)130);
            buffer.Write((float)1.0f);
            buffer.Write((int)0);
            buffer.Write((byte)199);
            buffer.Write((float)unit.VisionRadius);
        }
    }

    public class SpawnParticle : BasePacket
    {
        public SpawnParticle(Particle particle) : base(PacketCmd.PKT_S2C_SpawnParticle, particle.Owner.NetId)
        {
            buffer.Write((byte)1); // number of particles
            buffer.Write((uint)particle.Owner.getChampionHash());
            buffer.Write((uint)_rafManager.GetHash(particle.Name));
            buffer.Write((int)0x00000020); // flags ?

            buffer.Write((short)0); // Unk
            buffer.Write((uint)_rafManager.GetHash(particle.BoneName));

            buffer.Write((byte)1); // number of targets ?
            buffer.Write((uint)particle.Owner.NetId);
            buffer.Write((uint)particle.NetId); // Particle net id ?
            buffer.Write((uint)particle.Owner.NetId);

            if (particle.Target.IsSimpleTarget)
                buffer.Write((int)0);
            else
                buffer.Write((particle.Target as GameObject).NetId);

            buffer.Write((int)0); // unk

            for (var i = 0; i < 3; ++i)
            {
                var map = _game.Map;
                var ownerHeight = map.GetHeightAtLocation(particle.Owner.X, particle.Owner.Y);
                var particleHeight = map.GetHeightAtLocation(particle.X, particle.Y);
                var higherValue = Math.Max(ownerHeight, particleHeight);
                buffer.Write((short)((particle.Target.X - _game.Map.GetWidth() / 2) / 2));
                buffer.Write((float)higherValue);
                buffer.Write((short)((particle.Target.Y - _game.Map.GetHeight() / 2) / 2));
            }

            buffer.Write((uint)0); // unk
            buffer.Write((uint)0); // unk
            buffer.Write((uint)0); // unk
            buffer.Write((uint)0); // unk
            buffer.Write((float)particle.Size); // Particle size
        }
    }

    public class DestroyParticle : BasePacket
    {
        public DestroyParticle(Particle p) : base(PacketCmd.PKT_S2C_DestroyObject, p.NetId)
        {
            buffer.Write((uint)p.NetId);
        }
    }

    public class DestroyObject : BasePacket
    {
        public DestroyObject(Unit destroyer, Unit destroyed) : base(PacketCmd.PKT_S2C_DestroyObject, destroyer.NetId)
        {
            buffer.Write((uint)destroyed.NetId);
        }
    }

    public class DestroyProjectile : BasePacket
    {
        public DestroyProjectile(Projectile p) : base(PacketCmd.PKT_S2C_DestroyProjectile, p.NetId)
        {

        }
    }

    public class UpdateStats : BasePacket
    {
        public UpdateStats(Unit u, bool partial = true) : base(PacketCmd.PKT_S2C_CharStats)
        {
            var stats = new Dictionary<MasterMask, Dictionary<FieldMask, float>>();

            if (partial)
                stats = u.GetStats().GetUpdatedStats();
            else
                stats = u.GetStats().GetAllStats();
            var orderedStats = stats.OrderBy(x => x.Key);

            buffer.Write(Environment.TickCount); // syncID
            buffer.Write((byte)1); // updating 1 unit

            byte masterMask = 0;
            foreach (var p in orderedStats)
                masterMask |= (byte)p.Key;

            buffer.Write((byte)masterMask);
            buffer.Write((uint)u.NetId);

            foreach (var group in orderedStats)
            {
                var orderedGroup = group.Value.OrderBy(x => x.Key);
                uint fieldMask = 0;
                byte size = 0;
                foreach (var stat in orderedGroup)
                {
                    fieldMask |= (uint)stat.Key;
                    size += u.GetStats().getSize(group.Key, stat.Key);
                }
                buffer.Write((uint)fieldMask);
                buffer.Write((byte)size);
                foreach (var stat in orderedGroup)
                {
                    size = u.GetStats().getSize(group.Key, stat.Key);
                    switch (size)
                    {
                        case 1:
                            buffer.Write((byte)Convert.ToByte(stat.Value));
                            break;
                        case 2:
                            buffer.Write((short)Convert.ToInt16(stat.Value));
                            break;
                        case 4:
                            var bytes = BitConverter.GetBytes(stat.Value);
                            if (bytes[0] >= 0xFE)
                                bytes[0] = 0xFD;
                            buffer.Write((float)BitConverter.ToSingle(bytes, 0));
                            break;
                    }
                }
            }
        }
    }

    public class LevelPropSpawn : BasePacket
    {
        public LevelPropSpawn(LevelProp lp) : base(PacketCmd.PKT_S2C_LevelPropSpawn)
        {
            buffer.Write((int)lp.NetId);
            buffer.Write((byte)0x40); // unk
            buffer.Write((byte)lp.SkinId);
            buffer.Write((byte)0);
            buffer.Write((byte)0);
            buffer.Write((byte)0); // Unk
            buffer.Write((float)lp.X);
            buffer.Write((float)lp.Z);
            buffer.Write((float)lp.Y);
            buffer.Write((float)0.0f); // Rotation Y

            buffer.Write((float)lp.DirX);
            buffer.Write((float)lp.DirZ);
            buffer.Write((float)lp.DirY);
            buffer.Write((float)lp.Unk1);
            buffer.Write((float)lp.Unk2);

            buffer.Write((float)1.0f);
            buffer.Write((float)1.0f);
            buffer.Write((float)1.0f); // Scaling
            buffer.Write((int)lp.Team); // Probably a short
            buffer.Write((int)2); // nPropType [size 1 . 4] (4.18) -- if is a prop, become unselectable and use direction params

            foreach (var b in Encoding.Default.GetBytes(lp.Name))
                buffer.Write((byte)b);
            buffer.fill(0, 64 - lp.Name.Length);
            foreach (var b in Encoding.Default.GetBytes(lp.Model))
                buffer.Write(b);
            buffer.fill(0, 64 - lp.Model.Length);
        }
    }

    public class LevelPropAnimation : BasePacket
    {
        public LevelPropAnimation(LevelProp lp,
                                  string animationName,
                                  float unk1 = 0.0f,
                                  float animationTime = 0.0f,
                                  int unk2 = 1,
                                  int unk3 = 1,
                                  bool deletePropAfterAnimationFinishes = false)

               : base(PacketCmd.PKT_S2C_LevelPropAnimation)
        {
            buffer.Write(Encoding.Default.GetBytes(animationName));
            buffer.fill(0, 64 - animationName.Length);

            buffer.Write((float)unk1);
            buffer.Write((float)animationTime);

            buffer.Write((uint)lp.NetId);

            buffer.Write((int)unk2);
            buffer.Write((int)unk3);

            byte delete = 0x00;
            if (deletePropAfterAnimationFinishes)
            {
                delete = 0x01;
            }
            buffer.Write((byte)delete); // Most likely deletes prop after animation ends when set to 1
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
        }
    }

    public class FloatingTextWithValue : BasePacket
    {
        public FloatingTextWithValue(Unit u, int value, string text) : base(PacketCmd.PKT_S2C_FloatingTextWithValue)
        {
            buffer.Write(u.NetId);
            buffer.Write((int)15); // Unk
            buffer.Write(value); // Example -3
            buffer.Write(Encoding.Default.GetBytes(text));
            buffer.Write((byte)0x00);
        }
    }

    public class ChampionDie2 : BasePacket
    {
        public ChampionDie2(Champion die, float deathTimer) : base(PacketCmd.PKT_S2C_ChampionDie, die.NetId)
        {
            // Not sure what the whole purpose of that packet is
            buffer.Write(deathTimer);
        }
    }

    public class ToggleInputLockingFlag : BasePacket
    {

        public ToggleInputLockingFlag(byte bitField, bool locked) : base(PacketCmd.PKT_S2C_ToggleInputLockingFlag)
        {
            byte toggle = 0xFE;
            if (locked)
                toggle = 0xFF;
            buffer.Write((byte)bitField); // 0x01 = centerCamera; 0x02 = movement; 0x04 = spells; etc
            buffer.Write((byte)00);
            buffer.Write((byte)00);
            buffer.Write((byte)00);
            buffer.Write((byte)toggle); // FE(nabled); FD(isabled);
        }
    }

    public class AddUnitFOW : BasePacket
    {
        public AddUnitFOW(Unit u) : base(PacketCmd.PKT_S2C_AddUnitFOW)
        {
            buffer.Write((int)u.NetId);
        }
    }

    public class FreezeUnitAnimation : BasePacket
    {
        public FreezeUnitAnimation(Unit u, bool freeze) : base(PacketCmd.PKT_S2C_FreezeUnitAnimation, u.NetId)
        {
            byte flag = 0xDE;
            if (freeze)
                flag = 0xDD;
            buffer.Write(flag);
        }
    }

    public class ViewRequest
    {
        public PacketCmd cmd;
        public int netId;
        public float x;
        public float zoom;
        public float y;
        public float y2;       //Unk
        public int width;  //Unk
        public int height; //Unk
        public int unk2;   //Unk
        public byte requestNo;

        public ViewRequest(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            x = reader.ReadSingle();
            zoom = reader.ReadSingle();
            y = reader.ReadSingle();
            y2 = reader.ReadSingle();
            width = reader.ReadInt32();
            height = reader.ReadInt32();
            unk2 = reader.ReadInt32();
            requestNo = reader.ReadByte();

            reader.Close();
        }
    }

    public class LevelUp : BasePacket
    {
        public LevelUp(Champion c) : base(PacketCmd.PKT_S2C_LevelUp, c.NetId)
        {
            buffer.Write(c.GetStats().Level);
            buffer.Write(c.getSkillPoints());
        }
    }

    public class ViewAnswer : Packet
    {
        public ViewAnswer(ViewRequest request) : base(PacketCmd.PKT_S2C_ViewAns)
        {
            buffer.Write(request.netId);
        }
        public void setRequestNo(byte requestNo)
        {
            buffer.Write(requestNo);
        }
    }

    public class DebugMessage : BasePacket
    {

        public DebugMessage(string message) : base(PacketCmd.PKT_S2C_DebugMessage)
        {
            buffer.Write((int)0);
            foreach (var b in Encoding.Default.GetBytes(message))
                buffer.Write((byte)b);
            buffer.fill(0, 512 - message.Length);
        }
    }

    public class SetCooldown : BasePacket
    {
        public SetCooldown(uint netId, byte slotId, float currentCd, float totalCd = 0.0f)
               : base(PacketCmd.PKT_S2C_SetCooldown, netId)
        {
            buffer.Write(slotId);
            buffer.Write((byte)0xF8); // 4.18
            buffer.Write(currentCd);
            buffer.Write(totalCd);
        }
    }

    public class SetItemStacks : BasePacket
    {
        public SetItemStacks(Unit unit, byte slot, byte stack1, byte stack2)
               : base(PacketCmd.PKT_S2C_SetItemStacks, unit.NetId)
        {
            buffer.Write(slot);
            buffer.Write((byte)stack1); // Needs more research
            buffer.Write((byte)stack2); //
        }
    }

    public class SetItemStacks2 : BasePacket
    {
        public SetItemStacks2(Unit unit, byte slot, byte stack) : base(PacketCmd.PKT_S2C_SetItemStacks2, unit.NetId)
        {
            buffer.Write(slot);
            buffer.Write((byte)stack); // Needs more research
        }
    }

    public class EnableFOW : BasePacket
    {
        public EnableFOW(bool activate) : base(PacketCmd.PKT_S2C_EnableFOW)
        {
            buffer.Write(activate ? 0x01 : 0x00);
        }
    }
}

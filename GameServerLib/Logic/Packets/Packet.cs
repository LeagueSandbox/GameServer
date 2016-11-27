using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Enet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer;
using System.IO;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class Packet
    {
        private MemoryStream memStream;
        protected BinaryWriter buffer;
        public BinaryWriter getBuffer()
        {
            return buffer;
        }

        public Packet(PacketCmdS2C cmd = PacketCmdS2C.PKT_S2C_KeyCheck)
        {
            memStream = new MemoryStream();
            buffer = new BinaryWriter(memStream);

            buffer.Write((byte)cmd);
        }

        internal byte[] GetBytes()
        {
            return memStream.ToArray();
        }

        internal void Change(byte[] bytes)
        {
            memStream = new MemoryStream();
            buffer = new BinaryWriter(memStream);

            foreach (var b in bytes)
                buffer.Write(b);
        }
    }

    public class BasePacket : Packet
    {
        public BasePacket(PacketCmdS2C cmd = PacketCmdS2C.PKT_S2C_KeyCheck, int netId = 0) : base(cmd)
        {
            buffer.Write(netId);
        }
    }

    public class GamePacket : BasePacket
    {
        public GamePacket(PacketCmdS2C cmd = PacketCmdS2C.PKT_S2C_KeyCheck, int netId = 0) : base(cmd, netId)
        {
            buffer.Write(Environment.TickCount);
        }
    }

    public class ExtendedPacket : BasePacket
    {
        public ExtendedPacket(ExtendedPacketCmd ecmd = (ExtendedPacketCmd)0, int netId = 0) : base(PacketCmdS2C.PKT_S2C_Extended, netId)
        {
            buffer.Write((byte)ecmd);
            buffer.Write((byte)1);
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
            cmd = (PacketCmdC2S)reader.ReadByte();
            netId = reader.ReadInt32();
            reader.Close();
        }

        public PacketCmdC2S cmd;
        public int netId;
    }

    public class GameHeader
    {
        public GameCmd cmd;
        public int netId;
        public int ticks;
        public GameHeader()
        {
            netId = ticks = 0;
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
        public SynchVersionAns(List<Pair<uint, ClientInfo>> players, string version, string gameMode, int map) : base(PacketCmdS2C.PKT_S2C_SynchVersion)
        {
            buffer.Write((byte)9); // unk
            buffer.Write((int)map); // mapId
            foreach (var player in players)
            {
                var p = player.Item2;
                buffer.Write((long)p.userId);
                buffer.Write((short)0x1E); // unk
                buffer.Write((int)p.summonerSkills[0]);
                buffer.Write((int)p.summonerSkills[1]);
                buffer.Write((byte)0); // bot boolean
                buffer.Write((int)p.getTeam());
                buffer.fill(0, 64); // name is no longer here
                buffer.fill(0, 64);
                //buffer.Write(p.getRank());
                foreach (var b in Encoding.Default.GetBytes(p.getRank()))
                    buffer.Write((byte)b);
                buffer.fill(0, 24 - p.getRank().Length);
                buffer.Write((int)p.getIcon());
                buffer.Write((short)p.getRibbon());
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
            buffer.Write((int)487826); // gameFeatures (turret range indicators, etc.)
            buffer.fill(0, 256);
            buffer.Write((int)0);
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
        public PacketCmdS2C cmd;
        public int netId;
        public int unk1;
        public long userId;
        public float loaded;
        public float ping;
        public short unk2;
        public short unk3;
        public byte unk4;

        public PingLoadInfo(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmdS2C)reader.ReadByte();
            netId = reader.ReadInt32();
            unk1 = reader.ReadInt32();
            userId = reader.ReadInt64();
            loaded = reader.ReadSingle();
            ping = reader.ReadSingle();
            unk2 = reader.ReadInt16();
            unk3 = reader.ReadInt16();
            unk4 = reader.ReadByte();
            reader.Close();
        }

        public PingLoadInfo(PingLoadInfo loadInfo, long id) : base(PacketCmdS2C.PKT_S2C_Ping_Load_Info, loadInfo.netId)
        {
            buffer.Write((int)loadInfo.unk1);
            buffer.Write((long)id);
            buffer.Write((float)loadInfo.loaded);
            buffer.Write((float)loadInfo.ping);
            buffer.Write((short)loadInfo.unk2);
            buffer.Write((short)loadInfo.unk3);
            buffer.Write((byte)loadInfo.unk4);
        }
    }

    public class LoadScreenInfo : Packet
    {
        public LoadScreenInfo(List<Pair<uint, ClientInfo>> players) : base(PacketCmdS2C.PKT_S2C_LoadScreenInfo)
        {
            //Zero this complete buffer
            buffer.Write((int)6); // blueMax
            buffer.Write((int)6); // redMax

            int currentBlue = 0;
            foreach (var p in players)
            {
                var player = p.Item2;
                if (player.getTeam() == TeamId.TEAM_BLUE)
                {
                    buffer.Write((long)player.userId);
                    currentBlue++;
                }
            }

            for (var i = 0; i < 6 - currentBlue; ++i)
                buffer.Write((long)0);

            buffer.fill(0, 144);

            int currentPurple = 0;
            foreach (var p in players)
            {
                var player = p.Item2;
                if (player.getTeam() == TeamId.TEAM_PURPLE)
                {
                    buffer.Write((long)player.userId);
                    currentPurple++;
                }
            }

            for (int i = 0; i < 6 - currentPurple; ++i)
            {
                buffer.Write((long)0);
            }

            buffer.fill(0, 144);
            buffer.Write(currentBlue);
            buffer.Write(currentPurple);
        }
    }

    public class KeyCheck : Packet
    {
        public KeyCheck(long userId, int playerNo) : base(PacketCmdS2C.PKT_S2C_KeyCheck)
        {
            buffer.Write((byte)0x2A);
            buffer.Write((byte)0);
            buffer.Write((byte)0xFF);
            buffer.Write((int)playerNo);
            buffer.Write((long)userId);
            buffer.Write((int)0);
            buffer.Write((long)0);
            buffer.Write((int)0);
        }
        public KeyCheck(byte[] bytes) : base(PacketCmdS2C.PKT_S2C_KeyCheck)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));
            cmd = (PacketCmdS2C)reader.ReadByte();
            partialKey[0] = reader.ReadByte();
            partialKey[1] = reader.ReadByte();
            partialKey[2] = reader.ReadByte();
            playerNo = reader.ReadInt32();
            userId = reader.ReadInt64();
            trash = reader.ReadInt32();
            checkId = reader.ReadInt64();
            trash2 = reader.ReadInt32();
            reader.Close();
        }

        public PacketCmdS2C cmd;
        public byte[] partialKey = new byte[3];   //Bytes 1 to 3 from the blowfish key for that client
        public int playerNo;
        public long userId;         //short testVar[8];   //User id
        public int trash;
        public long checkId;        //short checkVar[8];  //Encrypted testVar
        public int trash2;
    }

    public class CameraLock
    {
        PacketHeader header;
        short isLock;
        int padding;
    }

    /*typedef struct _ViewReq {
        short cmd;
        int unk1;
        float x;
        float zoom;
        float y;
        float y2;		//Unk
        int width;	//Unk
        int height;	//Unk
        int unk2;	//Unk
        short requestNo;
    } ViewReq;*/

    public class MinionSpawn : BasePacket
    {
        public MinionSpawn(Minion m) : base(PacketCmdS2C.PKT_S2C_ObjectSpawn, m.getNetId())
        {
            buffer.Write((int)0x00150017); // unk
            buffer.Write((byte)0x03); // SpawnType - 3 = minion
            buffer.Write((int)m.getNetId());
            buffer.Write((int)m.getNetId());
            buffer.Write((int)m.getSpawnPosition());
            buffer.Write((byte)0xFF); // unk
            buffer.Write((byte)1); // wave number ?

            buffer.Write((byte)m.getType());

            if (m.getType() == MinionSpawnType.MINION_TYPE_MELEE)
            {
                buffer.Write((byte)0); // unk
            }
            else {
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
            else {
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

            List<Vector2> waypoints = m.getWaypoints();

            buffer.Write((byte)((waypoints.Count - m.getCurWaypoint() + 1) * 2)); // coordCount
            buffer.Write((int)m.getNetId());
            buffer.Write((byte)0); // movement mask
            buffer.Write((short)MovementVector.targetXToNormalFormat(m.getX()));
            buffer.Write((short)MovementVector.targetYToNormalFormat(m.getY()));
            for (int i = m.getCurWaypoint(); i < waypoints.Count; ++i)
            {
                buffer.Write((short)MovementVector.targetXToNormalFormat(waypoints[i].X));
                buffer.Write((short)MovementVector.targetXToNormalFormat(waypoints[i].Y));
            }
        }


    }
    public class MinionSpawn2 : Packet // shhhhh....
    {
        public MinionSpawn2(uint netId) : base(PacketCmdS2C.PKT_S2C_ObjectSpawn)
        {
            buffer.Write((uint)netId);
            buffer.fill(0, 3);
        }
    }
    class SpellAnimation : BasePacket
    {

        public SpellAnimation(Unit u, string animationName) : base(PacketCmdS2C.PKT_S2C_SpellAnimation, u.getNetId())
        {
            buffer.Write((int)0x00000005); // unk
            buffer.Write((int)0x00000000); // unk
            buffer.Write((int)0x00000000); // unk
            buffer.Write(1.0f); // unk
            foreach (var b in Encoding.Default.GetBytes(animationName))
                buffer.Write(b);
            buffer.Write((byte)0);
        }
    }

    class SetAnimation : BasePacket
    {
        public SetAnimation(Unit u, List<Tuple<string, string>> animationPairs) : base(PacketCmdS2C.PKT_S2C_SetAnimation, u.getNetId())
        {
            buffer.Write((byte)animationPairs.Count);

            for (int i = 0; i < animationPairs.Count; i++)
            {
                buffer.Write((int)animationPairs[i].Item1.Length);
                foreach (var b in Encoding.Default.GetBytes(animationPairs[i].Item1))
                    buffer.Write(b);

                buffer.Write((int)animationPairs[i].Item2.Length);
                foreach (var b in Encoding.Default.GetBytes(animationPairs[i].Item2))
                    buffer.Write(b);
            }
        }
    }

    public class FaceDirection : BasePacket
    {
        public FaceDirection(Unit u, float relativeX, float relativeY, float relativeZ) : base(PacketCmdS2C.PKT_S2C_FaceDirection, u.getNetId())
        {
            buffer.Write(relativeX);
            buffer.Write(relativeZ);
            buffer.Write(relativeY);
            buffer.Write((byte)0);
            buffer.Write((float)0.0833); // Time to turn ?
        }
    };

    public class Dash : GamePacket
    {
        public Dash(Unit u, float toX, float toY, float dashSpeed) : base(PacketCmdS2C.PKT_S2C_Dash, 0)
        {
            buffer.Write((short)1); // nb updates ?
            buffer.Write((byte)5); // unk
            buffer.Write((int)u.getNetId());
            buffer.Write((byte)0); // unk
            buffer.Write((float)dashSpeed); // Dash speed
            buffer.Write((int)0); // unk
            buffer.Write((float)u.getX());
            buffer.Write((float)u.getY());
            buffer.Write((int)0); // unk
            buffer.Write((byte)0);

            buffer.Write((int)0x4c079bb5); // unk
            buffer.Write((uint)0xa30036df); // unk
            buffer.Write((int)0x200168c2); // unk

            buffer.Write((byte)0x00); // Vector bitmask on whether they're int16 or byte

            MovementVector from = u.getMap().toMovementVector(u.getX(), u.getY());
            MovementVector to = u.getMap().toMovementVector(toX, toY);

            buffer.Write((short)from.x);
            buffer.Write((short)from.y);
            buffer.Write((short)to.x);
            buffer.Write((short)to.y);
        }
    }

    public class LeaveVision : BasePacket
    {
        public LeaveVision(GameObject o) : base(PacketCmdS2C.PKT_S2C_LeaveVision, o.getNetId())
        {
        }
    }

    public class DeleteObjectFromVision : BasePacket
    {
        public DeleteObjectFromVision(GameObject o) : base(PacketCmdS2C.PKT_S2C_DeleteObject, o.getNetId())
        {
        }
    }

    /**
     * This is basically a "Unit Spawn" packet with only the net ID and the additionnal data
     */
    public class EnterVisionAgain : BasePacket
    {

        public EnterVisionAgain(Minion m) : base(PacketCmdS2C.PKT_S2C_ObjectSpawn, m.getNetId())
        {
            buffer.fill(0, 13);
            buffer.Write(1.0f);
            buffer.fill(0, 13);
            buffer.Write((byte)0x02);
            buffer.Write((int)Environment.TickCount); // unk

            var waypoints = m.getWaypoints();

            buffer.Write((byte)((waypoints.Count - m.getCurWaypoint() + 1) * 2)); // coordCount
            buffer.Write((int)m.getNetId());
            buffer.Write((byte)0); // movement mask
            buffer.Write((short)MovementVector.targetXToNormalFormat(m.getX()));
            buffer.Write((short)MovementVector.targetYToNormalFormat(m.getY()));
            for (int i = m.getCurWaypoint(); i < waypoints.Count; i++)
            {
                buffer.Write(MovementVector.targetXToNormalFormat((float)waypoints[i].X));
                buffer.Write(MovementVector.targetXToNormalFormat((float)waypoints[i].Y));
            }
        }

        public EnterVisionAgain(Champion c) : base(PacketCmdS2C.PKT_S2C_ObjectSpawn, c.getNetId())
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

            List<Vector2> waypoints = c.getWaypoints();

            buffer.Write((byte)((waypoints.Count - c.getCurWaypoint() + 1) * 2)); // coordCount
            buffer.Write(c.getNetId());
            buffer.Write((byte)0); // movement mask; 1=KeepMoving?
            buffer.Write(MovementVector.targetXToNormalFormat(c.getX()));
            buffer.Write(MovementVector.targetYToNormalFormat(c.getY()));
            for (int i = c.getCurWaypoint(); i < waypoints.Count; ++i)
            {
                buffer.Write(MovementVector.targetXToNormalFormat(waypoints[i].X));
                buffer.Write(MovementVector.targetXToNormalFormat(waypoints[i].Y));
            }
        }
    }

    public class AddGold : BasePacket
    {

        public AddGold(Champion richMan, Unit died, float gold) : base(PacketCmdS2C.PKT_S2C_AddGold, richMan.getNetId())
        {
            buffer.Write(richMan.getNetId());
            if (died != null)
            {
                buffer.Write(died.getNetId());
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
        public PacketCmdC2S cmd;
        public int netIdHeader;
        public MoveType type; //byte
        public float x;
        public float y;
        public int targetNetId;
        public byte coordCount;
        public int netId;
        public byte[] moveData;

        public MovementReq(byte[] data)
        {
            var baseStream = new MemoryStream(data);
            var reader = new BinaryReader(baseStream);
            cmd = (PacketCmdC2S)reader.ReadByte();
            netIdHeader = reader.ReadInt32();
            type = (MoveType)reader.ReadByte();
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            targetNetId = reader.ReadInt32();
            coordCount = reader.ReadByte();
            netId = reader.ReadInt32();
            moveData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            reader.Close();
        }
    }

    public class MovementAns : GamePacket
    {
        //see PKT_S2C_CharStats mask
        //TODO movement
        public MovementAns(GameObject actor, Map map) : base(PacketCmdS2C.PKT_S2C_MoveAns)
        {
            var waypoints = actor.getWaypoints();
            buffer.Write((short)1);              //count
            buffer.Write((byte)(2 * waypoints.Count));
            buffer.Write((int)actor.getNetId());

            int startPos = (int)buffer.BaseStream.Position;
            int coordCount = 2 * waypoints.Count();
            var maskSize = (coordCount + 5) / 8; //mask size
            //buffer.reserve(pos + maskSize + coordCount * sizeof(short)); //reserve max total size
            for (uint i = 0; i < maskSize; i++)
            {
                buffer.Write((byte)0);
            }
            var lastCoord = new Vector2();
            for (int i = 0; i < waypoints.Count; i++)
            {
                var mapSize = map.getSize();
                var curVector = new Vector2((waypoints[i].X - mapSize.X) / 2, (waypoints[i].Y - mapSize.Y) / 2);
                var relative = new Vector2(curVector.X - lastCoord.X, curVector.Y - lastCoord.Y);
                var isAbsolute = new Tuple<bool, bool>(
                      i == 0 || relative.X < sbyte.MinValue || relative.X > sbyte.MaxValue,
                      i == 0 || relative.Y < sbyte.MinValue || relative.Y > sbyte.MaxValue);

                Change(SetBitmaskValue(GetBytes(), startPos, (2 * i - 2), !isAbsolute.Item1));
                if (isAbsolute.Item1)
                    buffer.Write((short)curVector.X);
                else
                    buffer.Write((byte)relative.X);

                Change(SetBitmaskValue(GetBytes(), startPos, (2 * i - 1), !isAbsolute.Item2));
                if (isAbsolute.Item2)
                    buffer.Write((short)curVector.Y);
                else
                    buffer.Write((byte)relative.Y);

                lastCoord = curVector;
            }
        }
        static byte[] SetBitmaskValue(byte[] mask, int startPos, int pos, bool val)
        {
            if (pos < 0)
                return mask;

            if (val)
                mask[startPos + (pos / 8)] |= (byte)(1 << (pos % 8));
            else
                mask[startPos + (pos / 8)] &= (byte)(~(1 << (pos % 8)));

            return mask;
        }
    }

    /*typedef struct _ViewAns {
        _ViewAns() {
            cmd = PKT_S2C_ViewAns;
            unk1 = 0;
        }

        short cmd;
        int unk1;
        short requestNo;
    } ViewAns;*/


    public class QueryStatus : BasePacket
    {
        public QueryStatus() : base(PacketCmdS2C.PKT_S2C_QueryStatusAns)
        {
            buffer.Write((byte)1); //ok
        }
    }

    public class SynchVersion : BasePacket
    {
        public PacketCmdS2C cmd;
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
            cmd = (PacketCmdS2C)reader.ReadByte();
            netId = reader.ReadInt32();
            unk1 = reader.ReadInt32();
            _version = reader.ReadBytes(256);
            reader.Close();
        }
    }

    public class WorldSendGameNumber : BasePacket
    {
        public WorldSendGameNumber(long gameId, string name) : base(PacketCmdS2C.PKT_S2C_World_SendGameNumber)
        {
            var data = Encoding.Default.GetBytes(name);
            buffer.Write((long)gameId);
            foreach (var d in data)
                buffer.Write((byte)d);
            buffer.fill(0, 128 - data.Length);
        }
    }


    public class CharacterStats
    {
        GameHeader header;
        short updateNo = 1;
        short masterMask;
        int netId;
        int mask;
        short size;
        Value value;
        public CharacterStats(short masterMask, int netId, int mask, float value)
        {
            this.masterMask = masterMask;
            this.netId = netId;
            this.mask = mask;
            this.size = 4;
            header = new GameHeader();
            this.value = new Value();
            header.cmd = GameCmd.PKT_S2C_CharStats;
            header.ticks = Environment.TickCount;
            this.value.fValue = value;
        }

        public CharacterStats(short masterMask, int netId, int mask, short value)
        {
            this.masterMask = masterMask;
            this.netId = netId;
            this.mask = mask;
            this.size = 2;
            header = new GameHeader();
            this.value = new Value();
            header.cmd = GameCmd.PKT_S2C_CharStats;
            header.ticks = Environment.TickCount;
            this.value.sValue = value;
        }


        public class Value
        {
            public short sValue;
            public float fValue;
        }
    }

    public class ChatMessage
    {
        public PacketCmdC2S cmd;
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
            cmd = (PacketCmdC2S)reader.ReadByte();
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
        public UpdateModel(int netID, string szModel) : base(PacketCmdS2C.PKT_S2C_UpdateModel, netID)
        {
            buffer.Write((int)netID & ~0x40000000); //id
            buffer.Write((byte)1); //bOk
            buffer.Write((int)-1); //unk1
            foreach (var b in Encoding.Default.GetBytes(szModel))
                buffer.Write((byte)b);
            if (szModel.Length < 32)
                buffer.fill(0, 32 - szModel.Length);
        }
    }

    public class StatePacket : BasePacket
    {
        public StatePacket(PacketCmdS2C state) : base(state)
        {

        }
    }
    public class StatePacket2 : BasePacket
    {
        public StatePacket2(PacketCmdS2C state) : base(state)
        {
            buffer.Write((short)0); //unk
        }
    }
    /*
    public class FogUpdate2
    {
        PacketHeader header;
        float x;
        float y;
        int radius;
        short unk1;
        public FogUpdate2()
        {
            header = new PacketHeader();
            header.cmd = PacketCmdS2C.PKT_S2C_FogUpdate2;
            header.netId = 0x40000019;
        }
    }*/

    public class Click
    {
        PacketCmdC2S cmd;
        int netId;
        public int zero;
        public int targetNetId; // netId on which the player clicked

        public Click(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmdC2S)reader.ReadByte();
            netId = reader.ReadInt32();
            zero = reader.ReadInt32();
            targetNetId = reader.ReadInt32();
        }
    }

    public class HeroSpawn : Packet
    {

        public HeroSpawn(ClientInfo player, int playerId) : base(PacketCmdS2C.PKT_S2C_HeroSpawn)
        {
            buffer.Write((int)0); // ???
            buffer.Write((int)player.getChampion().getNetId());
            buffer.Write((int)playerId); // player Id
            buffer.Write((byte)40); // netNodeID ?
            buffer.Write((byte)0); // botSkillLevel Beginner=0 Intermediate=1
            if (player.getTeam() == TeamId.TEAM_BLUE)
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
            buffer.Write((int)player.getSkinNo());
            foreach (var b in Encoding.Default.GetBytes(player.getName()))
                buffer.Write((byte)b);
            buffer.fill(0, 128 - player.getName().Length);
            foreach (var b in Encoding.Default.GetBytes(player.getChampion().getType()))
                buffer.Write((byte)b);
            buffer.fill(0, 40 - player.getChampion().getType().Length);
            buffer.Write((float)0.0f); // deathDurationRemaining
            buffer.Write((float)0.0f); // timeSinceDeath
            buffer.Write((int)0); // UNK (4.18)
            buffer.Write((byte)0); // bitField
        }
    }

    public class HeroSpawn2 : BasePacket
    {
        public HeroSpawn2(Champion p) : base(PacketCmdS2C.PKT_S2C_ObjectSpawn, p.getNetId())
        {
            buffer.fill(0, 15);
            buffer.Write((byte)0x80); // unk
            buffer.Write((byte)0x3F); // unk
            buffer.fill(0, 13);
            buffer.Write((byte)3); // unk
            buffer.Write((int)1); // unk
            buffer.Write((float)p.getX());
            buffer.Write((float)p.getY());
            buffer.Write((float)0x3F441B7D); // z ?
            buffer.Write((float)0x3F248DBB); // Rotation ?
        }
    }

    public class TurretSpawn : BasePacket //TODO: check
    {
        public TurretSpawn(Turret t) : base(PacketCmdS2C.PKT_S2C_TurretSpawn)
        {
            buffer.Write((int)t.getNetId());
            foreach (var b in Encoding.Default.GetBytes(t.getName()))
                buffer.Write((byte)b);
            buffer.fill(0, 64 - t.getName().Length);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x22);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x80);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x00);
        }

        /*PacketHeader header;
        int tID;
        short name[28];
        short type[42];*/
    }

    public class GameTimer : BasePacket
    {
        public GameTimer(float fTime) : base(PacketCmdS2C.PKT_S2C_GameTimer, 0)
        {
            buffer.Write((float)fTime);
        }
    }

    public class GameTimerUpdate : BasePacket
    {
        public GameTimerUpdate(float fTime) : base(PacketCmdS2C.PKT_S2C_GameTimerUpdate, 0)
        {
            buffer.Write((float)fTime);
        }
    }

    public class HeartBeat
    {
        public PacketCmdC2S cmd;
        public int netId;
        public float receiveTime;
        public float ackTime;
        public HeartBeat(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmdC2S)reader.ReadByte();
            netId = reader.ReadInt32();
            receiveTime = reader.ReadSingle();
            ackTime = reader.ReadSingle();
            reader.Close();
        }
    }

    /* public class SpellSet
     {
         public PacketHeader header;
         public int spellID;
         public int level;
         public SpellSet(int netID, int _spellID, int _level)
         {
             header = new PacketHeader();
             header.cmd = (PacketCmdS2C)0x5A;
             header.netId = netID;
             spellID = _spellID;
             level = _level;
         }
     }*/

    public class SkillUpPacket : BasePacket
    {
        public PacketCmdC2S cmd;
        public int netId;
        public byte skill;
        public SkillUpPacket(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmdC2S)reader.ReadByte();
            netId = reader.ReadInt32();
            skill = reader.ReadByte();
        }
        public SkillUpPacket(int netId, byte skill, byte level, byte pointsLeft) : base(PacketCmdS2C.PKT_S2C_SkillUp, netId)
        {
            buffer.Write(skill);
            buffer.Write(level);
            buffer.Write(pointsLeft);
        }
    }

    public class BuyItemReq
    {
        PacketCmdC2S cmd;
        int netId;
        public int id;
        public BuyItemReq(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmdC2S)reader.ReadByte();
            netId = reader.ReadInt32();
            id = reader.ReadInt32();
        }
    }

    public class BuyItemAns : BasePacket
    {
        public BuyItemAns(Champion actor, ItemInstance item) : base(PacketCmdS2C.PKT_S2C_BuyItemAns, actor.getNetId())
        {
            buffer.Write((int)item.getTemplate().getId());
            buffer.Write((byte)item.getSlot());
            buffer.Write((byte)item.getStacks());
            buffer.Write((byte)0); //unk or stacks => short
            buffer.Write((byte)0x40); //unk
        }
    }

    public class SellItem
    {
        public PacketCmdC2S cmd;
        public int netId;
        public byte slotId;

        public SellItem(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmdC2S)reader.ReadByte();
            netId = reader.ReadInt32();
            slotId = reader.ReadByte();
        }
    }


    public class RemoveItem : BasePacket
    {
        public RemoveItem(Unit u, short slot, short remaining) : base(PacketCmdS2C.PKT_S2C_RemoveItem, u.getNetId())
        {
            buffer.Write(slot);
            buffer.Write(remaining);
        }
    }

    public class EmotionPacket : BasePacket
    {
        public PacketCmdC2S cmd;
        public int netId;
        public byte id;

        public EmotionPacket(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmdC2S)reader.ReadByte();
            netId = reader.ReadInt32();
            id = reader.ReadByte();
        }

        public EmotionPacket(byte id, int netId) : base(PacketCmdS2C.PKT_S2C_Emotion, netId)
        {
            buffer.Write((byte)id);
        }
    }

    public class SwapItems : BasePacket
    {
        public PacketCmdC2S cmd;
        public int netId;
        public byte slotFrom;
        public byte slotTo;
        public SwapItems(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmdC2S)reader.ReadByte();
            netId = reader.ReadInt32();
            slotFrom = reader.ReadByte();
            slotTo = reader.ReadByte();
        }

        public SwapItems(Champion c, byte slotFrom, byte slotTo) : base(PacketCmdS2C.PKT_S2C_SwapItems, c.getNetId())
        {
            buffer.Write((byte)slotFrom);
            buffer.Write((byte)slotTo);
        }
    }

    /* New Style Packets */

    class Announce : BasePacket
    {
        public Announce(byte messageId, int mapId = 0) : base(PacketCmdS2C.PKT_S2C_Announce)
        {
            buffer.Write((byte)messageId);
            buffer.Write((long)0);

            if (mapId > 0)
            {
                buffer.Write(mapId);
            }
        }
    }

    public class AddBuff : Packet
    {
        public AddBuff(Unit u, Unit source, int stacks, string name) : base(PacketCmdS2C.PKT_S2C_AddBuff)
        {
            buffer.Write(u.getNetId());//target

            buffer.Write((short)0x05); //maybe type?
            buffer.Write((short)0x02);
            buffer.Write((short)0x01); // stacks
            buffer.Write((short)0x00); // bool value
            buffer.Write(RAFManager.getInstance().getHash(name));
            buffer.Write((short)0xde);
            buffer.Write((short)0x88);
            buffer.Write((short)0xc6);
            buffer.Write((short)0xee);
            buffer.Write((short)0x00);
            buffer.Write((short)0x00);
            buffer.Write((short)0x00);
            buffer.Write((short)0x00);
            buffer.Write((short)0x00);
            buffer.Write((short)0x50);
            buffer.Write((short)0xc3);
            buffer.Write((short)0x46);

            if (source != null)
            {
                buffer.Write(source.getNetId()); //source
            }
            else
            {
                buffer.Write((int)0);
            }
        }
    }

    public class RemoveBuff : BasePacket
    {
        public RemoveBuff(Unit u, string name) : base(PacketCmdS2C.PKT_S2C_RemoveBuff, u.getNetId())
        {
            buffer.Write((short)0x05);
            buffer.Write(RAFManager.getInstance().getHash(name));
            buffer.Write((int)0x0);
            //buffer.Write(u.getNetId());//source?
        }
    }

    public class DamageDone : BasePacket
    {
        public DamageDone(Unit source, Unit target, float amount, DamageType type) : base(PacketCmdS2C.PKT_S2C_DamageDone, target.getNetId())
        {
            buffer.Write((short)((((short)type) << 4) | 0x04));
            buffer.Write((short)0x4B); // 4.18
            buffer.Write((float)amount); // 4.18
            buffer.Write((int)target.getNetId());
            buffer.Write((int)source.getNetId());
        }
    }

    public class NpcDie : ExtendedPacket
    {
        public NpcDie(Unit die, Unit killer) : base(ExtendedPacketCmd.EPKT_S2C_NPC_Die, die.getNetId())
        {
            buffer.Write((int)0);
            buffer.Write((short)0);
            buffer.Write(killer.getNetId());
            buffer.Write((short)0); // unk
            buffer.Write((short)7); // unk
            buffer.Write((int)0); // Flags?
        }
    }

    public class LoadScreenPlayerName : Packet
    {
        public LoadScreenPlayerName(Pair<uint, ClientInfo> player) : base(PacketCmdS2C.PKT_S2C_LoadName)
        {
            buffer.Write((long)player.Item2.userId);
            buffer.Write((int)0);
            buffer.Write((int)player.Item2.getName().Length + 1);
            foreach (var b in Encoding.Default.GetBytes(player.Item2.getName()))
                buffer.Write(b);
            buffer.Write((byte)0);
        }

        /*byte cmd;
        long userId;
        int skinId;
        int length;
        byte* description;*/
    }

    public class LoadScreenPlayerChampion : Packet
    {

        public LoadScreenPlayerChampion(Pair<uint, ClientInfo> p) : base(PacketCmdS2C.PKT_S2C_LoadHero)
        {
            var player = p.Item2;
            buffer.Write((long)player.userId);
            buffer.Write((int)player.skinNo);
            buffer.Write((int)player.getChampion().getType().Length + 1);
            foreach (var b in Encoding.Default.GetBytes(player.getChampion().getType()))
                buffer.Write(b);
            buffer.Write((byte)0);
        }

        /*byte cmd;
        long userId;
        int skinId;
        int length;
        byte* description;*/
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

    public class AttentionPingAns : Packet
    {
        public AttentionPingAns(ClientInfo player, AttentionPing ping) : base(PacketCmdS2C.PKT_S2C_AttentionPing)
        {
            buffer.Write((int)0); //unk1
            buffer.Write((float)ping.x);
            buffer.Write((float)ping.y);
            buffer.Write((int)ping.targetNetId);
            buffer.Write((int)player.getChampion().getNetId());
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
        public BeginAutoAttack(Unit attacker, Unit attacked, int futureProjNetId, bool isCritical) : base(PacketCmdS2C.PKT_S2C_BeginAutoAttack, attacker.getNetId())
        {
            buffer.Write(attacked.getNetId());
            buffer.Write((short)0x80); // unk
            buffer.Write(futureProjNetId); // Basic attack projectile ID, to be spawned later
            if (isCritical)
                buffer.Write((short)0x49);
            else
                buffer.Write((short)0x40); // unk -- seems to be flags related to things like critical strike (0x49)
                                           // not sure what this is, but it should be correct (or maybe attacked x z y?) - 4.18
            buffer.Write((short)0x80);
            buffer.Write((short)0x01);
            buffer.Write(MovementVector.targetXToNormalFormat(attacked.getX()));
            buffer.Write((short)0x80);
            buffer.Write((short)0x01);
            buffer.Write(MovementVector.targetYToNormalFormat(attacked.getY()));
            buffer.Write((short)0xCC);
            buffer.Write((short)0x35);
            buffer.Write((short)0xC4);
            buffer.Write((short)0xD1);
            buffer.Write(attacker.getX());
            buffer.Write(attacker.getY());
        }
    }

    public class NextAutoAttack : BasePacket
    {

        public NextAutoAttack(Unit attacker, Unit attacked, int futureProjNetId, bool isCritical, bool initial) : base(PacketCmdS2C.PKT_S2C_NextAutoAttack, attacker.getNetId())
        {
            buffer.Write(attacked.getNetId());
            if (initial)
                buffer.Write((short)0x80); // These flags appear to change only to 0x80 and 0x7F after the first autoattack.
            else
                buffer.Write((short)0x7F);

            buffer.Write(futureProjNetId);
            if (isCritical)
                buffer.Write((short)0x49);
            else
                buffer.Write((short)0x40); // unk -- seems to be flags related to things like critical strike (0x49)

            // not sure what this is, but it should be correct (or maybe attacked x z y?) - 4.18
            buffer.Write("\x40\x01\x7B\xEF\xEF\x01\x2E\x55\x55\x35\x94\xD3");
        }
    }

    public class StopAutoAttack : BasePacket
    {

        public StopAutoAttack(Unit attacker) : base(PacketCmdS2C.PKT_S2C_StopAutoAttack, attacker.getNetId())
        {
            buffer.Write((int)0); // Unk. Rarely, this is a net ID. Dunno what for.
            buffer.Write((short)3); // Unk. Sometimes "2", sometimes "11" when the above netId is not 0.
        }
    }

    public class OnAttack : ExtendedPacket
    {
        public OnAttack(Unit attacker, Unit attacked, AttackType attackType) : base(ExtendedPacketCmd.EPKT_S2C_OnAttack, attacker.getNetId())
        {
            buffer.Write((short)attackType);
            buffer.Write(attacked.getX());
            buffer.Write(attacked.getZ());
            buffer.Write(attacked.getY());
            buffer.Write(attacked.getNetId());
        }
    }

    public class SetTarget : BasePacket
    {

        public SetTarget(Unit attacker, Unit attacked) : base(PacketCmdS2C.PKT_S2C_SetTarget, attacker.getNetId())
        {
            if (attacked != null)
            {
                buffer.Write(attacked.getNetId());
            }
            else
            {
                buffer.Write((int)0);
            }
        }

    }

    public class SetTarget2 : BasePacket
    {

        public SetTarget2(Unit attacker, Unit attacked) : base(PacketCmdS2C.PKT_S2C_SetTarget2, attacker.getNetId())
        {
            if (attacked != null)
            {
                buffer.Write(attacked.getNetId());
            }
            else
            {
                buffer.Write((int)0);
            }
        }

    }

    public class ChampionDie : BasePacket
    {

        public ChampionDie(Champion die, Unit killer, int goldFromKill) : base(PacketCmdS2C.PKT_S2C_ChampionDie, die.getNetId())
        {
            buffer.Write(goldFromKill); // Gold from kill?
            buffer.Write((short)0);
            if (killer != null)
                buffer.Write(killer.getNetId());
            else
                buffer.Write((int)0);

            buffer.Write((short)0);
            buffer.Write((short)7);
            buffer.Write(die.getRespawnTimer() / 1000.0f); // Respawn timer, float
        }
    }

    public class ChampionDeathTimer : ExtendedPacket
    {

        public ChampionDeathTimer(Champion die) : base(ExtendedPacketCmd.EPKT_S2C_ChampionDeathTimer, die.getNetId())
        {
            buffer.Write(die.getRespawnTimer() / 1000.0f); // Respawn timer, float
        }
    }

    public class ChampionRespawn : BasePacket
    {
        public ChampionRespawn(Champion c) : base(PacketCmdS2C.PKT_S2C_ChampionRespawn, c.getNetId())
        {
            buffer.Write(c.getX());
            buffer.Write(c.getY());
            buffer.Write(c.getZ());
        }
    }

    public class ShowProjectile : BasePacket
    {

        public ShowProjectile(Projectile p) : base(PacketCmdS2C.PKT_S2C_ShowProjectile, p.getOwner().getNetId())
        {
            buffer.Write(p.getNetId());
        }
    }

    public class SetHealth : BasePacket
    {
        public SetHealth(Unit u) : base(PacketCmdS2C.PKT_S2C_SetHealth, u.getNetId())
        {
            buffer.Write((short)0x0000); // unk,maybe flags for physical/magical/true dmg
            buffer.Write((float)u.getStats().getMaxHealth());
            buffer.Write((float)u.getStats().getCurrentHealth());
        }

        public SetHealth(int itemHash) : base(PacketCmdS2C.PKT_S2C_SetHealth, itemHash)
        {
            buffer.Write((short)0);
        }

    }
    public class SetHealth2 : Packet //shhhhh...
    {
        public SetHealth2(uint itemHash) : base(PacketCmdS2C.PKT_S2C_SetHealth)
        {
            buffer.Write((uint)itemHash);
            buffer.Write((short)0);
        }
    }

    public class TeleportRequest : BasePacket
    {
        short a = 0x01;
        public TeleportRequest(int netId, float x, float y, bool first) : base(PacketCmdS2C.PKT_S2C_MoveAns)
        {
            buffer.Write((int)Environment.TickCount);//not 100% sure
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x00);
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
        public PacketCmdC2S cmd;
        public int netId;
        public byte spellSlotType; // 4.18 [deprecated? . 2 first(highest) bits: 10 - ability or item, 01 - summoner spell]
        public byte spellSlot; // 0-3 [0-1 if spellSlotType has summoner spell bits set]
        public float x;
        public float y;
        public float x2;
        public float y2;
        public int targetNetId; // If 0, use coordinates, else use target net id

        public CastSpell(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmdC2S)reader.ReadByte();
            netId = reader.ReadInt32();
            spellSlotType = reader.ReadByte();
            spellSlot = reader.ReadByte();
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            x2 = reader.ReadSingle();
            y2 = reader.ReadSingle();
            targetNetId = reader.ReadInt32();
        }
    }

    public class CastSpellAns : GamePacket
    {

        public CastSpellAns(Spell s, float x, float y, int futureProjNetId, int spellNetId) : base(PacketCmdS2C.PKT_S2C_CastSpellAns, s.getOwner().getNetId())
        {
            Map m = s.getOwner().getMap();

            buffer.Write((byte)0);
            buffer.Write((byte)0x66);
            buffer.Write((byte)0x00); // unk
            buffer.Write((int)s.getId()); // Spell hash, for example hash("EzrealMysticShot")
            buffer.Write((int)spellNetId); // Spell net ID
            buffer.Write((byte)0); // unk
            buffer.Write((float)1.0f); // unk
            buffer.Write((int)s.getOwner().getNetId());
            buffer.Write((int)s.getOwner().getNetId());
            buffer.Write((int)s.getOwner().getChampionHash());
            buffer.Write((int)futureProjNetId); // The projectile ID that will be spawned
            buffer.Write((float)x);
            buffer.Write((float)m.getHeightAtLocation(x, y));
            buffer.Write((float)y);
            buffer.Write((float)x);
            buffer.Write((float)m.getHeightAtLocation(x, y));
            buffer.Write((float)y);
            buffer.Write((byte)0); // unk
            buffer.Write(s.getCastTime());
            buffer.Write((float)0.0f); // unk
            buffer.Write((float)1.0f); // unk
            buffer.Write(s.getCooldown());
            buffer.Write((float)0.0f); // unk
            buffer.Write((byte)0); // unk
            buffer.Write((byte)s.getSlot());
            buffer.Write((float)s.getCost());
            buffer.Write((float)s.getOwner().getX());
            buffer.Write((float)s.getOwner().getZ());
            buffer.Write((float)s.getOwner().getY());
            buffer.Write((long)1); // unk
        }
    }

    public class PlayerInfo : BasePacket
    {
        public PlayerInfo(ClientInfo player) : base(PacketCmdS2C.PKT_S2C_PlayerInfo, player.getChampion().getNetId())
        {
            #region wtf
            buffer.Write((byte)0x7D);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x7D);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x7D);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x7D);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x7D);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x7D);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x7D);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x7D);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x83);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xA9);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xA9);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xA9);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xA9);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xA9);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xA9);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xA9);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xA9);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xA9);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xC5);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xC5);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xC5);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xC5);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xC5);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xC5);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xC5);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xC5);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xC5);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xD7);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xD7);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xD7);
            buffer.Write((byte)0x14);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);

            buffer.Write((int)player.summonerSkills[0]);
            buffer.Write((int)player.summonerSkills[1]);

            buffer.Write((byte)0x41);
            buffer.Write((byte)0x74);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x42);
            buffer.Write((byte)0x74);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x04);
            buffer.Write((byte)0x52);
            buffer.Write((byte)0x74);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x61);
            buffer.Write((byte)0x74);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x62);
            buffer.Write((byte)0x74);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x64);
            buffer.Write((byte)0x74);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x71);
            buffer.Write((byte)0x74);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x72);
            buffer.Write((byte)0x74);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x82);
            buffer.Write((byte)0x74);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x92);
            buffer.Write((byte)0x74);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x41);
            buffer.Write((byte)0x75);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x42);
            buffer.Write((byte)0x75);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x02);
            buffer.Write((byte)0x43);
            buffer.Write((byte)0x75);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x02);
            buffer.Write((byte)0x52);
            buffer.Write((byte)0x75);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x62);
            buffer.Write((byte)0x75);
            buffer.Write((byte)0x03);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x1E);
            buffer.Write((byte)0x00);

            #endregion
        }


    }

    public class SpawnProjectile : BasePacket
    {

        public SpawnProjectile(Projectile p) : base(PacketCmdS2C.PKT_S2C_SpawnProjectile, p.getNetId())
        {
            float targetZ = p.getMap().getHeightAtLocation(p.getTarget().getX(), p.getTarget().getY());

            buffer.Write((float)p.getX());
            buffer.Write((float)p.getZ());
            buffer.Write((float)p.getY());
            buffer.Write((float)p.getX());
            buffer.Write((float)p.getZ());
            buffer.Write((float)p.getY());
            buffer.Write((long)0x000000003f510fe2); // unk
            buffer.Write((float)0.577f); // unk
            buffer.Write((float)p.getTarget().getX());
            buffer.Write((float)targetZ);
            buffer.Write((float)p.getTarget().getY());
            buffer.Write((float)p.getX());
            buffer.Write((float)p.getZ());
            buffer.Write((float)p.getY());
            buffer.Write((float)p.getTarget().getX());
            buffer.Write((float)targetZ);
            buffer.Write((float)p.getTarget().getY());
            buffer.Write((float)p.getX());
            buffer.Write((float)p.getZ());
            buffer.Write((float)p.getY());
            buffer.Write((int)0); // unk
            buffer.Write((float)p.getMoveSpeed()); // Projectile speed
            buffer.Write((long)0x00000000d5002fce); // unk
            buffer.Write((int)0x7f7fffff); // unk
            buffer.Write((byte)0);
            buffer.Write((byte)0x66);
            buffer.Write((byte)0);
            buffer.Write((int)p.getProjectileId()); // unk (projectile ID)
            buffer.Write((int)0); // Second net ID
            buffer.Write((byte)0); // unk
            buffer.Write(1.0f);
            buffer.Write((int)p.getOwner().getNetId());
            buffer.Write((int)p.getOwner().getNetId());

            var c = p.getOwner() as Champion;
            if (c != null)
                buffer.Write((int)c.getChampionHash());
            else
                buffer.Write((int)0);

            buffer.Write((int)p.getNetId());
            buffer.Write((float)p.getTarget().getX());
            buffer.Write((float)targetZ);
            buffer.Write((float)p.getTarget().getY());
            buffer.Write((float)p.getTarget().getX());
            buffer.Write((float)targetZ);
            buffer.Write((float)p.getTarget().getY());
            buffer.Write((uint)0x80000000); // unk
            buffer.Write((int)0x000000bf); // unk
            buffer.Write((uint)0x80000000); // unk
            buffer.Write((int)0x2fd5843f); // unk
            buffer.Write((int)0x00000000); // unk
            buffer.Write((short)0x0000); // unk
            buffer.Write((byte)0x2f); // unk
            buffer.Write((int)0x00000000); // unk
            buffer.Write((float)p.getX());
            buffer.Write((float)p.getZ());
            buffer.Write((float)p.getY());
            buffer.Write((long)0x0000000000000000); // unk
        }

    }

    public class SpawnParticle : BasePacket
    {
        const short MAP_WIDTH = (13982 / 2);
        const short MAP_HEIGHT = (14446 / 2);

        public SpawnParticle(Champion owner, GameObjects.Target t, string particle, int netId) : base(PacketCmdS2C.PKT_S2C_SpawnParticle, owner.getNetId())
        {
            buffer.Write((short)1); // number of particles
            buffer.Write(owner.getChampionHash());
            buffer.Write(RAFManager.getInstance().getHash(particle));
            buffer.Write((int)0x00000020); // flags ?
            buffer.Write((int)0); // unk
            buffer.Write((short)0); // unk
            buffer.Write((short)1); // number of targets ?
            buffer.Write(owner.getNetId());
            buffer.Write(netId); // Particle net id ?
            buffer.Write(owner.getNetId());

            if (t.isSimpleTarget())
                buffer.Write((int)0);
            else
                buffer.Write((t as GameObject).getNetId());

            buffer.Write((int)0); // unk

            for (var i = 0; i < 3; ++i)
            {

                buffer.Write((short)((t.getX() - MAP_WIDTH) / 2));
                buffer.Write(50.0f);
                buffer.Write((short)((t.getY() - MAP_HEIGHT) / 2));
            }

            buffer.Write((int)0); // unk
            buffer.Write((int)0); // unk
            buffer.Write((int)0); // unk
            buffer.Write((int)0); // unk
            buffer.Write(1.0f); // unk

        }

        public class DestroyProjectile : BasePacket
        {
            public DestroyProjectile(Projectile p) : base(PacketCmdS2C.PKT_S2C_DestroyProjectile, p.getNetId())
            {

            }
        }

        public class UpdateStats : GamePacket
        {
            public UpdateStats(Unit u, bool partial = true) : base(PacketCmdS2C.PKT_S2C_CharStats, 0)
            {
                var stats = new PairList<byte, List<int>>();

                if (partial)
                    stats = u.getStats().getUpdatedStats();
                else
                    stats = u.getStats().getAllStats();

                var masks = new List<byte>();
                byte masterMask = 0;

                foreach (var p in stats)
                {
                    masterMask |= p.Item1;
                    masks.Add(p.Item1);
                }

                masks.Sort();

                buffer.Write((byte)1);
                buffer.Write((byte)masterMask);
                buffer.Write((int)u.getNetId());


                foreach (var m in masks)
                {
                    int mask = 0;
                    byte size = 0;

                    var updatedStats = stats[m];
                    updatedStats.Sort();
                    foreach (var it in updatedStats)
                    {
                        size += u.getStats().getSize(m, it);
                        mask |= it;
                    }
                    //if (updatedStats.Contains((int)FieldMask.FM1_SummonerSpells_Enabled))
                    //  System.Diagnostics.Debugger.Break();
                    buffer.Write((int)mask);
                    buffer.Write((byte)size);

                    for (int i = 0; i < 32; i++)
                    {
                        int tmpMask = (1 << i);
                        if ((tmpMask & mask) > 0)
                        {
                            if (u.getStats().getSize(m, tmpMask) == 4)
                            {
                                float f = u.getStats().getStat(m, tmpMask);
                                var c = BitConverter.GetBytes(f);
                                if (c[0] >= 0xFE)
                                {
                                    c[0] = (byte)0xFD;
                                }
                                buffer.Write(BitConverter.ToSingle(c, 0));
                            }
                            else if (u.getStats().getSize(m, tmpMask) == 2)
                            {
                                short stat = (short)Math.Floor(u.getStats().getStat(m, tmpMask) + 0.5);
                                buffer.Write(stat);
                            }
                            else
                            {
                                byte stat = (byte)Math.Floor(u.getStats().getStat(m, tmpMask) + 0.5);
                                buffer.Write(stat);
                            }
                        }
                    }
                }
            }
        }

        public class LevelPropSpawn : BasePacket
        {
            public LevelPropSpawn(LevelProp lp) : base(PacketCmdS2C.PKT_S2C_LevelPropSpawn)
            {
                buffer.Write((int)lp.getNetId());
                buffer.Write((int)0x00000040); // unk
                buffer.Write((byte)0); // unk
                buffer.Write((float)lp.getX());
                buffer.Write((float)lp.getZ());
                buffer.Write((float)lp.getY());
                buffer.Write((float)0.0f); // Rotation Y

                buffer.Write((float)lp.getDirectionX());
                buffer.Write((float)lp.getDirectionZ());
                buffer.Write((float)lp.getDirectionY());
                buffer.Write((float)lp.getUnk1());
                buffer.Write((float)lp.getUnk2());

                buffer.Write((float)1.0f);
                buffer.Write((float)1.0f);
                buffer.Write((float)1.0f); // Scaling
                buffer.Write((int)300); // unk
                buffer.Write((int)2); // nPropType [size 1 . 4] (4.18) -- if is a prop, become unselectable and use direction params

                foreach (var b in Encoding.Default.GetBytes(lp.getName()))
                    buffer.Write((byte)b);
                buffer.fill(0, 64 - lp.getName().Length);
                foreach (var b in Encoding.Default.GetBytes(lp.getType()))
                    buffer.Write(b);
                buffer.fill(0, 64 - lp.getType().Length);
            }

            // TODO : remove this once we find a better solution for jungle camp spawning command
            public LevelPropSpawn(int netId, string name, string type, float x, float y, float z, float dirX, float dirY, float dirZ, float unk1, float unk2) : base(PacketCmdS2C.PKT_S2C_LevelPropSpawn)
            {
                buffer.Write(netId);
                buffer.Write((int)0x00000040); // unk
                buffer.Write((short)0); // unk
                buffer.Write(x);
                buffer.Write(z);
                buffer.Write(y);
                buffer.Write(0.0f); // Rotation Y
                buffer.Write(dirX);
                buffer.Write(dirZ);
                buffer.Write(dirY); // Direction
                buffer.Write(unk1);
                buffer.Write(unk2);
                buffer.Write(1.0f);
                buffer.Write(1.0f);
                buffer.Write(1.0f); // Scaling
                buffer.Write((int)300); // unk
                buffer.Write((short)1); // bIsProp -- if is a prop, become unselectable and use direction params
                buffer.Write(name);
                buffer.fill(0, 64 - name.Length);
                buffer.Write(type);
                buffer.fill(0, 64 - type.Length);
            }
        }

        public class ViewRequest
        {
            public PacketCmdC2S cmd;
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
                cmd = (PacketCmdC2S)reader.ReadByte();
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
            public LevelUp(Champion c) : base(PacketCmdS2C.PKT_S2C_LevelUp, c.getNetId())
            {
                buffer.Write(c.getStats().getLevel());
                buffer.Write(c.getSkillPoints());
            }
        }

        public class ViewAnswer : Packet
        {
            public ViewAnswer(ViewRequest request) : base(PacketCmdS2C.PKT_S2C_ViewAns)
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

            public DebugMessage(string message) : base(PacketCmdS2C.PKT_S2C_DebugMessage)
            {
                buffer.Write((int)0);
                foreach (var b in Encoding.Default.GetBytes(message))
                    buffer.Write((byte)b);
                buffer.fill(0, 512 - message.Length);
            }
        }

        public class SetCooldown : BasePacket
        {
            public SetCooldown(int netId, byte slotId, float currentCd, float totalCd = 0.0f) : base(PacketCmdS2C.PKT_S2C_SetCooldown, netId)
            {
                buffer.Write(slotId);
                buffer.Write((short)0xF8); // 4.18
                buffer.Write(totalCd);
                buffer.Write(currentCd);
            }
        }
    }
}

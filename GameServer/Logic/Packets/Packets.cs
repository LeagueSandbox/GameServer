using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Enet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox;
using System.IO;
using System.Numerics;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets;
using LeagueSandbox.GameServer.Logic.Content;

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
    }

    public class BasePacket : Packet
    {
        public BasePacket(PacketCmdS2C cmd = PacketCmdS2C.PKT_S2C_KeyCheck, uint netId = 0) : base(cmd)
        {
            buffer.Write((uint)netId);
        }
    }

    public class GamePacket : BasePacket
    {
        public GamePacket(PacketCmdS2C cmd = PacketCmdS2C.PKT_S2C_KeyCheck, uint netId = 0) : base(cmd, netId)
        {
            buffer.Write(Environment.TickCount);
        }
    }

    public class ExtendedPacket : BasePacket
    {
        public ExtendedPacket(ExtendedPacketCmd ecmd = (ExtendedPacketCmd)0, uint netId = 0) : base(PacketCmdS2C.PKT_S2C_Extended, netId)
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
            buffer.Write((uint)map); // mapId
            foreach (var player in players)
            {
                var p = player.Item2;
                var summonerSpells = p.getSummoners();
                buffer.Write((long)p.UserId);
                buffer.Write((short)0x1E); // unk
                buffer.Write((int)summonerSpells[0]);
                buffer.Write((int)summonerSpells[1]);
                buffer.Write((byte)0); // bot boolean
                buffer.Write((int)p.GetTeam());
                buffer.fill(0, 64); // name is no longer here
                buffer.fill(0, 64);
                //buffer.Write(p.getRank());
                foreach (var b in Encoding.Default.GetBytes(p.GetRank()))
                    buffer.Write((byte)b);
                buffer.fill(0, 24 - p.GetRank().Length);
                buffer.Write((int)p.GetIcon());
                buffer.Write((short)p.GetRibbon());
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
        public PacketCmdS2C cmd;
        public uint netId;
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
            netId = reader.ReadUInt32();
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
            buffer.Write((uint)loadInfo.unk1);
            buffer.Write((ulong)id);
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
            buffer.Write((uint)6); // blueMax
            buffer.Write((uint)6); // redMax

            int currentBlue = 0;
            foreach (var p in players)
            {
                var player = p.Item2;
                if (player.GetTeam() == TeamId.TEAM_BLUE)
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
                if (player.GetTeam() == TeamId.TEAM_PURPLE)
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

    public class KeyCheck : Packet
    {
        public KeyCheck(long userId, int playerNo) : base(PacketCmdS2C.PKT_S2C_KeyCheck)
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
            cmd = (PacketCmdS2C)reader.ReadByte();
            partialKey[0] = reader.ReadByte();
            partialKey[1] = reader.ReadByte();
            partialKey[2] = reader.ReadByte();
            playerNo = reader.ReadUInt32();
            userId = reader.ReadInt64();
            trash = reader.ReadUInt32();
            checkId = reader.ReadUInt64();
            trash2 = reader.ReadUInt32();
            reader.Close();
        }

        public PacketCmdS2C cmd;
        public byte[] partialKey = new byte[3];   //Bytes 1 to 3 from the blowfish key for that client
        public uint playerNo;
        public long userId;         //short testVar[8];   //User id
        public uint trash;
        public ulong checkId;        //short checkVar[8];  //Encrypted testVar
        public uint trash2;
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
    public class SpawnMonster : Packet
    {
        public SpawnMonster(Monster m) : base(PacketCmdS2C.PKT_S2C_ObjectSpawn)
        {

            buffer.Write(m.getNetId());
            buffer.Write((short)345);
            //
            buffer.Write((short)343);

            buffer.Write((byte)99);// 99 for jungle monster, 3 for minion
            buffer.Write(m.getNetId());
            buffer.Write(m.getNetId());
            buffer.Write((byte)64);
            buffer.Write((float)m.getX()); //x
            buffer.Write((float)m.GetZ()); //z
            buffer.Write((float)m.getY()); //y
            buffer.Write((float)m.getX()); //x
            buffer.Write((float)m.GetZ()); //z
            buffer.Write((float)m.getY()); //y
            buffer.Write((float)m.getFacing().X); //facing x
            buffer.Write((float)m.GetGame().GetMap().GetHeightAtLocation(m.getFacing().X, m.getFacing().Y)); //facing z
            buffer.Write((float)m.getFacing().Y); //facing y

            var str = m.getName();
            foreach (var b in Encoding.Default.GetBytes(str)) // starting with a string -> Dragon6.1.1
                buffer.Write(b);
            buffer.fill(0, 64 - str.Length);

            foreach (var b in Encoding.Default.GetBytes(m.getModel())) // starting with a string -> Dragon
                buffer.Write(b);
            buffer.fill(0, 64 - m.getModel().Length);

            str = m.getName();
            foreach (var b in Encoding.Default.GetBytes(str)) // starting with a string -> Dragon6.1.1
                buffer.Write(b);
            buffer.fill(0, 64 - str.Length);

            buffer.fill(0, 64); // empty


            buffer.Write((int)300);
            buffer.fill(0, 12);
            buffer.Write((int)1); //campId 1
            buffer.Write((int)100);
            buffer.Write((int)74);
            buffer.Write((long)1);
            buffer.Write((float)115.0066f);
            buffer.Write((byte)0);

            //
            buffer.fill(0, 13);
            buffer.Write((sbyte)-128); // always 0x80/-128
            buffer.Write((byte)63); // always 0x3F/63
            buffer.fill(0, 13);
            buffer.Write((byte)3); //type 3=champ/jungle; 2=minion
            buffer.Write((int)13337);
            buffer.Write((float)m.getX()); //x
            buffer.Write((float)m.getY());  //y
            buffer.Write((float)-0.8589599f);  // rotation1 from -1 to 1
            buffer.Write((float)0.5120428f); //rotation2 from -1 to 1
        }
    }

    public class SpawnPlaceable : Packet
    {
        public SpawnPlaceable(Placeable p) : base(PacketCmdS2C.PKT_S2C_ObjectSpawn)
        {

            buffer.Write(p.getNetId());

            buffer.Write((byte)0xB5);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xB3);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x7C);

            buffer.Write(p.getNetId());
            buffer.Write(p.getNetId());

            buffer.Write((byte)0x20);

            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x40);
            buffer.Write((byte)0x40);

            buffer.Write((float)p.getX()); //x
            buffer.Write((float)p.GetZ()); //z
            buffer.Write((float)p.getY()); //y

            buffer.fill(0, 8);

            buffer.Write((byte)p.getTeam());
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x92);
            buffer.Write((byte)0x00);

            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x02);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);

            var name = p.getName();
            foreach (var b in Encoding.Default.GetBytes(name))
                buffer.Write(b);
            buffer.fill(0, 64 - name.Length);

            foreach (var b in Encoding.Default.GetBytes(p.getModel()))
                buffer.Write(b);
            buffer.fill(0, 64 - p.getModel().Length);

            buffer.Write((byte)0x01);

            buffer.fill(0, 18);

            buffer.Write((byte)0x80);
            buffer.Write((byte)0x3F);

            buffer.fill(0, 13);

            buffer.Write((byte)0x03);

            buffer.Write((byte)0xB1); // <--|
            buffer.Write((byte)0x20); //    | Unknown, changes between packets
            buffer.Write((byte)0x18); //    |
            buffer.Write((byte)0x00); // <--|

            buffer.Write((float)p.getX());
            buffer.Write((float)p.getY());

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

    public class SetHealthTest : BasePacket
    {
        public SetHealthTest(uint netId, short unk, float maxhp, float hp) : base(PacketCmdS2C.PKT_S2C_SetHealth, netId)
        {
            buffer.Write((short)unk); // unk,maybe flags for physical/magical/true dmg
            buffer.Write((float)maxhp);
            buffer.Write((float)hp);
        }
    }

    public class HighlightUnit : BasePacket
    {
        public HighlightUnit(uint netId) : base(PacketCmdS2C.PKT_S2C_HighlightUnit)
        {
            // The following structure might be incomplete or wrong
            buffer.Write((uint)netId);
        }
    }

    public class RemoveHighlightUnit : BasePacket
    {
        public RemoveHighlightUnit(uint netId) : base(PacketCmdS2C.PKT_S2C_RemoveHighlightUnit)
        {
            // The following structure might be incomplete or wrong
            buffer.Write((uint)netId);
        }
    }

    public class BasicTutorialMessageWindow : BasePacket
    {
        public BasicTutorialMessageWindow(string message) : base(PacketCmdS2C.PKT_S2C_BasicTutorialMessageWindow)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(Encoding.Default.GetBytes(message)); // It seems to show up to 189 characters, which is strange
            buffer.Write(0x00);
        }
    }

    public class MessageBoxTop : BasePacket
    {
        public MessageBoxTop(string message) : base(PacketCmdS2C.PKT_S2C_MessageBoxTop)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(Encoding.Default.GetBytes(message));
            buffer.Write(0x00);
        }
    }

    public class EditMessageBoxTop : BasePacket
    {
        public EditMessageBoxTop(string message) : base(PacketCmdS2C.PKT_S2C_EditMessageBoxTop)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(Encoding.Default.GetBytes(message));
            buffer.Write(0x00);
        }
    }

    public class ChangeSpell : BasePacket
    {
        public ChangeSpell(Unit unit, int slot, string spell) : base(PacketCmdS2C.PKT_S2C_ChangeSpell, unit.getNetId())
        {
            buffer.Write((byte)slot);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x02);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write(Encoding.Default.GetBytes(spell));
            buffer.Write(0x00);
        }
    }

    public class RemoveMessageBoxTop : BasePacket
    {
        public RemoveMessageBoxTop() : base(PacketCmdS2C.PKT_S2C_RemoveMessageBoxTop)
        {
            // The following structure might be incomplete or wrong
        }
    }

    public class MessageBoxRight : BasePacket
    {
        public MessageBoxRight(string message) : base(PacketCmdS2C.PKT_S2C_MessageBoxRight)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(Encoding.Default.GetBytes(message));
            buffer.Write(0x00);
        }
    }

    public class EditMessageBoxRight : BasePacket
    {
        public EditMessageBoxRight(string message) : base(PacketCmdS2C.PKT_S2C_EditMessageBoxRight)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(Encoding.Default.GetBytes(message));
            buffer.Write(0x00);
        }
    }

    public class RemoveMessageBoxRight : BasePacket
    {
        public RemoveMessageBoxRight() : base(PacketCmdS2C.PKT_S2C_RemoveMessageBoxRight)
        {
            // The following structure might be incomplete or wrong
        }
    }

    public class PauseGame : Packet
    {
        public PauseGame(byte seconds) : base(PacketCmdS2C.PKT_S2C_PauseGame)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(0x00);
            buffer.Write(0x00);
            buffer.Write((byte)seconds);
        }
    }

    public class MessagesAvailable : Packet
    {
        public MessagesAvailable(byte messagesAvailable) : base(PacketCmdS2C.PKT_S2C_MessagesAvailable)
        {
            // The following structure might be incomplete or wrong
            buffer.Write(0x00);
            buffer.Write((byte)messagesAvailable);
        }
    }

    public class AFKWarningWindow : Packet
    {
        public AFKWarningWindow() : base(PacketCmdS2C.PKT_S2C_AFKWarningWindow)
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
        public MinionSpawn(Minion m) : base(PacketCmdS2C.PKT_S2C_ObjectSpawn, m.getNetId())
        {
            buffer.Write((uint)0x00150017); // unk
            buffer.Write((byte)0x03); // SpawnType - 3 = minion
            buffer.Write((uint)m.getNetId());
            buffer.Write((uint)m.getNetId());
            buffer.Write((uint)m.getSpawnPosition());
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
            buffer.Write((byte)0xC4); // unk
            buffer.Write((uint)0); // unk
            buffer.Write((uint)0); // unk
            buffer.Write((ushort)0); // unk
            buffer.Write((byte)0x80); // unk
            buffer.Write((byte)0x3F); // unk
            foreach (var b in Encoding.Default.GetBytes(animationName))
                buffer.Write(b);
            buffer.Write((byte)0);
        }
    }

    class SetAnimation : BasePacket
    {
        public SetAnimation(Unit u, List<string> animationPairs) : base(PacketCmdS2C.PKT_S2C_SetAnimation, u.getNetId())
        {
            buffer.Write((byte)(animationPairs.Count/2));

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
            buffer.Write((uint)u.getNetId());
            buffer.Write((byte)0); // unk
            buffer.Write((float)dashSpeed); // Dash speed
            buffer.Write((int)0); // unk
            buffer.Write((float)u.getX());
            buffer.Write((float)u.getY());
            buffer.Write((int)0); // unk
            buffer.Write((byte)0);

            buffer.Write((uint)0x4c079bb5); // unk
            buffer.Write((uint)0xa30036df); // unk
            buffer.Write((uint)0x200168c2); // unk

            buffer.Write((byte)0x00); // Vector bitmask on whether they're int16 or byte

            MovementVector from = u.GetGame().GetMap().ToMovementVector(u.getX(), u.getY());
            MovementVector to = u.GetGame().GetMap().ToMovementVector(toX, toY);

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
        public uint targetNetId;
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
            targetNetId = reader.ReadUInt32();
            coordCount = reader.ReadByte();
            netId = reader.ReadInt32();
            moveData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            reader.Close();
        }
    }

    public class MovementAns : GamePacket
    {
        public MovementAns(GameObject obj, Map map) : this(new List<GameObject> { obj }, map)
        {

        }

        public MovementAns(List<GameObject> actors, Map map) : base(PacketCmdS2C.PKT_S2C_MoveAns)
        {
            buffer.Write((short)actors.Count);

            foreach (var actor in actors)
            {
                var mapSize = map.GetSize();
                var waypoints = actor.getWaypoints();
                var numCoords = waypoints.Count * 2;
                buffer.Write((byte)numCoords);
                buffer.Write((int)actor.getNetId());

                var maskBytes = new byte[((numCoords - 3) / 8) + 1];
                var memStream = new MemoryStream();
                var tempBuffer = new BinaryWriter(memStream);

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
                    buffer.Write(maskBytes);
                buffer.Write(memStream.ToArray());
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

    public class Quest : BasePacket
    {
        public Quest(string questName, string questDescription, byte type, byte remove, uint netid,  byte success = 0) : base(PacketCmdS2C.PKT_S2C_Quest)
        {
            buffer.Write(Encoding.Default.GetBytes(questName));
            buffer.fill(0, 256 - questName.Length);
            buffer.Write(Encoding.Default.GetBytes(questDescription));
            buffer.fill(0, 256 - questDescription.Length);
            buffer.Write((byte)type);
            buffer.Write((byte)remove);
            buffer.Write((byte)success); // 252 to 255: Success, anything else: Fail
            buffer.Write((int)netid);
        }
    }

    public class ChangeCrystalScarNexusHP : BasePacket
    {
        public ChangeCrystalScarNexusHP(TeamId team, int hp) : base(PacketCmdS2C.PKT_S2C_ChangeCrystalScarNexusHP)
        {
            buffer.Write((byte)team);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write(hp);
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

    public class InhibitorStateUpdate : BasePacket
    {
        public InhibitorStateUpdate(Inhibitor inhi) : base(PacketCmdS2C.PKT_S2C_InhibitorState, inhi.getNetId())
        {
            buffer.Write((byte)inhi.getState());
            buffer.Write((byte)0);
            buffer.Write((byte)0);
        }
    }
    public class InhibitorDeathAnimation : BasePacket
    {
        public InhibitorDeathAnimation(Inhibitor inhi, GameObject killer) : base(PacketCmdS2C.PKT_S2C_InhibitorDeathAnimation, inhi.getNetId())
        {
            if (killer != null)
                buffer.Write((uint)killer.getNetId());
            else
                buffer.Write((int)0);
            buffer.Write((int)0); //unk
        }
    }

    public class UnitAnnounce : BasePacket
    {
        public UnitAnnounce(UnitAnnounces id, Unit target, GameObject killer = null, List<Champion> assists = null) : base(PacketCmdS2C.PKT_S2C_Announce2, target.getNetId())
        {
            if (assists == null)
                assists = new List<Champion>();

            buffer.Write((byte)id);
            if (killer != null)
            {
                buffer.Write((long)killer.getNetId());
                buffer.Write((int)assists.Count);
                foreach (var a in assists)
                    buffer.Write((uint)a.getNetId());
                for (int i = 0; i < 12 - assists.Count; i++)
                    buffer.Write((int)0);
            }
        }
    }

    public class HideUi : BasePacket
    {
        public HideUi() : base(PacketCmdS2C.PKT_S2C_HideUi)
        {

        }
    }

    public class UnlockCamera : BasePacket
    {
        public UnlockCamera() : base(PacketCmdS2C.PKT_S2C_UnlockCamera)
        {

        }
    }

    public class MoveCamera : BasePacket
    {
        public MoveCamera(Champion champ, float x, float y, float z, float seconds) : base(PacketCmdS2C.PKT_S2C_MoveCamera, champ.getNetId())
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
            buffer.Write((float)y);
            buffer.Write((float)y);
            buffer.Write((float)seconds);
        }
    }

    public class ExplodeNexus : BasePacket
    {
        public ExplodeNexus(Nexus nexus) : base(PacketCmdS2C.PKT_S2C_ExplodeNexus, nexus.getNetId())
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

    public class CharacterStats
    {
        GameHeader header;
        byte updateNo = 1;
        byte masterMask;
        int netId;
        int mask;
        byte size;
        Value value;
        public CharacterStats(byte masterMask, int netId, int mask, float value)
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

        public CharacterStats(byte masterMask, int netId, int mask, short value)
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
        public UpdateModel(uint netID, string szModel) : base(PacketCmdS2C.PKT_S2C_UpdateModel, netID)
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
        public uint targetNetId; // netId on which the player clicked

        public Click(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmdC2S)reader.ReadByte();
            netId = reader.ReadInt32();
            zero = reader.ReadInt32();
            targetNetId = reader.ReadUInt32();
        }
    }

    public class UseObject
    {
        PacketCmdC2S cmd;
        int netId;
        public uint targetNetId; // netId of the object used

        public UseObject(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmdC2S)reader.ReadByte();
            netId = reader.ReadInt32();
            targetNetId = reader.ReadUInt32();
        }
    }

    public class HeroSpawn : Packet
    {

        public HeroSpawn(ClientInfo player, int playerId) : base(PacketCmdS2C.PKT_S2C_HeroSpawn)
        {
            buffer.Write((int)0); // ???
            buffer.Write((int)player.GetChampion().getNetId());
            buffer.Write((int)playerId); // player Id
            buffer.Write((byte)40); // netNodeID ?
            buffer.Write((byte)0); // botSkillLevel Beginner=0 Intermediate=1
            if (player.GetTeam() == TeamId.TEAM_BLUE)
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
            buffer.Write((int)player.GetSkinNo());
            foreach (var b in Encoding.Default.GetBytes(player.GetName()))
                buffer.Write((byte)b);
            buffer.fill(0, 128 - player.GetName().Length);
            foreach (var b in Encoding.Default.GetBytes(player.GetChampion().getType()))
                buffer.Write((byte)b);
            buffer.fill(0, 40 - player.GetChampion().getType().Length);
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
            buffer.Write((uint)1); // unk
            buffer.Write(p.getX());
            buffer.Write(p.getY());
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

    public class BlueTip : BasePacket
    {
        public BlueTip(string title, string text, uint playernetid, uint netid, bool delete = false) : base(PacketCmdS2C.PKT_S2C_BlueTip, playernetid)
        {
            foreach (var b in Encoding.Default.GetBytes(text))
                buffer.Write(b);
            buffer.fill(0, 128 - text.Length);
            foreach (var b in Encoding.Default.GetBytes(title))
                buffer.Write(b);
            buffer.fill(0, 256 - title.Length);
            if (delete)
            {
                buffer.Write((byte)0x01);
            }
            else
            {
                buffer.Write((byte)0x00);
            }
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
        public uint netId;
        public byte skill;
        public SkillUpPacket(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmdC2S)reader.ReadByte();
            netId = reader.ReadUInt32();
            skill = reader.ReadByte();
        }
        public SkillUpPacket(uint netId, byte skill, byte level, byte pointsLeft) : base(PacketCmdS2C.PKT_S2C_SkillUp, netId)
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
        public BuyItemAns(Champion actor, Item item) : base(PacketCmdS2C.PKT_S2C_BuyItemAns, actor.getNetId())
        {
            buffer.Write((int)item.ItemType.ItemId);
            buffer.Write((byte)actor.Inventory.GetItemSlot(item));
            buffer.Write((byte)item.StackSize);
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
        public RemoveItem(Unit u, byte slot, short remaining) : base(PacketCmdS2C.PKT_S2C_RemoveItem, u.getNetId())
        {
            buffer.Write(slot);
            buffer.Write(remaining);
        }
    }

    public class EmotionPacket : BasePacket
    {
        public PacketCmdC2S cmd;
        public uint netId;
        public byte id;

        public EmotionPacket(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmdC2S)reader.ReadByte();
            netId = reader.ReadUInt32();
            id = reader.ReadByte();
        }

        public EmotionPacket(byte id, uint netId) : base(PacketCmdS2C.PKT_S2C_Emotion, netId)
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
        public Announce(Announces messageId, int mapId = 0) : base(PacketCmdS2C.PKT_S2C_Announce)
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
        public AddBuff(Unit u, Unit source, int stacks, float time, BuffType buffType, string name, int slot) : base(PacketCmdS2C.PKT_S2C_AddBuff)
        {
            buffer.Write(u.getNetId());//target
            buffer.Write((byte)slot); //Slot
            buffer.Write((byte)buffType); //Type
            buffer.Write((byte)stacks); // stacks
            buffer.Write((byte)0x00); // Visible
            buffer.Write(RAFManager.getInstance().getHash(name)); //Buff id
            buffer.Write((byte)0xde);
            buffer.Write((byte)0x88);
            buffer.Write((byte)0xc6);
            buffer.Write((byte)0xee);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);

            buffer.Write((float)time);

            buffer.Write((byte)0x00);
            buffer.Write((byte)0x50);
            buffer.Write((byte)0xc3);
            buffer.Write((byte)0x46);

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

    public class AddXP : BasePacket
    {
        public AddXP(Unit u, float xp) : base(PacketCmdS2C.PKT_S2C_AddXP)
        {
            buffer.Write(u.getNetId());
            buffer.Write(xp);
        }
    }

    public class EditBuff : BasePacket
    {
        public EditBuff(Unit u, byte slot, byte stacks) : base(PacketCmdS2C.PKT_S2C_EditBuff, u.getNetId())
        {
            buffer.Write(slot);//slot
            buffer.Write(stacks);//stacks
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x50);
            buffer.Write((byte)0xC3);
            buffer.Write((byte)0x46);
            buffer.Write(0);
            buffer.Write(u.getNetId());

        }
    }

    public class RemoveBuff : BasePacket
    {
        public RemoveBuff(Unit u, string name) : base(PacketCmdS2C.PKT_S2C_RemoveBuff, u.getNetId())
        {
            buffer.Write((byte)0x01);
            buffer.Write(RAFManager.getInstance().getHash(name));
            buffer.Write((int)0x0);
            //buffer.Write(u.getNetId());//source?
        }
    }

    public class DamageDone : BasePacket
    {
        public DamageDone(Unit source, Unit target, float amount, DamageType type) : base(PacketCmdS2C.PKT_S2C_DamageDone, target.getNetId())
        {
            buffer.Write((byte)(((byte)type << 4) | 0x04));
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
            buffer.Write((long)player.Item2.UserId);
            buffer.Write((int)0);
            buffer.Write((int)player.Item2.GetName().Length + 1);
            foreach (var b in Encoding.Default.GetBytes(player.Item2.GetName()))
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
            buffer.Write((long)player.UserId);
            buffer.Write((int)player.GetSkinNo());
            buffer.Write((int)player.GetChampion().getType().Length + 1);
            foreach (var b in Encoding.Default.GetBytes(player.GetChampion().getType()))
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
            buffer.Write((int)player.GetChampion().getNetId());
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
        public BeginAutoAttack(Unit attacker, Unit attacked, uint futureProjNetId, bool isCritical) : base(PacketCmdS2C.PKT_S2C_BeginAutoAttack, attacker.getNetId())
        {
            buffer.Write(attacked.getNetId());
            buffer.Write((byte)0x80); // unk
            buffer.Write(futureProjNetId); // Basic attack projectile ID, to be spawned later
            if (isCritical)
                buffer.Write((byte)0x49);
            else
                buffer.Write((byte)0x40); // unk -- seems to be flags related to things like critical strike (0x49)
                                          // not sure what this is, but it should be correct (or maybe attacked x z y?) - 4.18
            buffer.Write((byte)0x80);
            buffer.Write((byte)0x01);
            buffer.Write(MovementVector.targetXToNormalFormat(attacked.getX()));
            buffer.Write((byte)0x80);
            buffer.Write((byte)0x01);
            buffer.Write(MovementVector.targetYToNormalFormat(attacked.getY()));
            buffer.Write((byte)0xCC);
            buffer.Write((byte)0x35);
            buffer.Write((byte)0xC4);
            buffer.Write((byte)0xD1);
            buffer.Write(attacker.getX());
            buffer.Write(attacker.getY());
        }
    }

    public class NextAutoAttack : BasePacket
    {

        public NextAutoAttack(Unit attacker, Unit attacked, uint futureProjNetId, bool isCritical, bool initial) : base(PacketCmdS2C.PKT_S2C_NextAutoAttack, attacker.getNetId())
        {
            buffer.Write(attacked.getNetId());
            if (initial)
                buffer.Write((byte)0x80); // These flags appear to change only to 0x80 and 0x7F after the first autoattack.
            else
                buffer.Write((byte)0x7F);

            buffer.Write(futureProjNetId);
            if (isCritical)
                buffer.Write((byte)0x49);
            else
                buffer.Write((byte)0x40); // unk -- seems to be flags related to things like critical strike (0x49)

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

        public StopAutoAttack(Unit attacker) : base(PacketCmdS2C.PKT_S2C_StopAutoAttack, attacker.getNetId())
        {
            buffer.Write((int)0); // Unk. Rarely, this is a net ID. Dunno what for.
            buffer.Write((byte)3); // Unk. Sometimes "2", sometimes "11" when the above netId is not 0.
        }
    }

    public class StopAutoAttackUnk : BasePacket
    {

        public StopAutoAttackUnk(Unit attacker) : base(PacketCmdS2C.PKT_S2C_StopAutoAttack, attacker.getNetId())
        {
            // Same number of bytes as StopAutoAttack, but might have a different structure
            // It could also just be the same packet
            buffer.Write((byte)0x43);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
        }
    }

    public class OnAttack : ExtendedPacket
    {
        public OnAttack(Unit attacker, Unit attacked, AttackType attackType) : base(ExtendedPacketCmd.EPKT_S2C_OnAttack, attacker.getNetId())
        {
            buffer.Write((byte)attackType);
            buffer.Write(attacked.getX());
            buffer.Write(attacked.GetZ());
            buffer.Write(attacked.getY());
            buffer.Write(attacked.getNetId());
        }
    }

    public class Surrender : BasePacket
    {
        public Surrender() : base(PacketCmdS2C.PKT_S2C_Surrender)
        {
            buffer.Write((byte)0); //unk
            buffer.Write(0); //surrendererNetworkId
            buffer.Write((byte)0); //yesVotes
            buffer.Write((byte)0); //noVotes
            buffer.Write((byte)4); //maxVotes
            buffer.Write((byte)TeamId.TEAM_BLUE); //team
        }
    }

    public class UnpauseGame : BasePacket
    {
        public UnpauseGame(uint unpauserNetId) : base(PacketCmdS2C.PKT_S2C_UnpauseGame)
        {
            buffer.Write(unpauserNetId);
        }
    }

    public class GameSpeed : BasePacket
    {
        public GameSpeed(float gameSpeed) : base(PacketCmdS2C.PKT_S2C_GameSpeed)
        {
            buffer.Write((float)gameSpeed);
        }
    }

    public class SurrenderState : ExtendedPacket
    {
        public SurrenderState(uint playernetid, byte state) : base(ExtendedPacketCmd.EPKT_S2C_SurrenderState, playernetid)
        {
            buffer.Write((byte)state);
        }
    }

    public class ResourceType : ExtendedPacket
    {
        public ResourceType(uint playernetid, byte resourceType) : base(ExtendedPacketCmd.EPKT_S2C_SurrenderState, playernetid)
        {
            buffer.Write((byte)resourceType);
        }
    }

    public class SurrenderResult : BasePacket
    {
        public SurrenderResult(bool early, int yes, int no, TeamId team) : base(PacketCmdS2C.PKT_S2C_SurrenderResult)
        {
            buffer.Write(BitConverter.GetBytes(early)); //surrendererNetworkId
            buffer.Write((byte)yes); //yesVotes
            buffer.Write((byte)no); //noVotes
            buffer.Write((byte)team); //team
        }
    }

    public class GameEnd : BasePacket
    {
        public GameEnd(bool win) : base(PacketCmdS2C.PKT_S2C_GameEnd)
        {
            buffer.Write(win ? (byte)1 : (byte)0); //0 : lose 1 : Win
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
            buffer.Write((byte)0);
            if (killer != null)
                buffer.Write(killer.getNetId());
            else
                buffer.Write((int)0);

            buffer.Write((byte)0);
            buffer.Write((byte)7);
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
            buffer.Write(c.GetZ());
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
            buffer.Write((float)u.GetStats().HealthPoints.Total);
            buffer.Write((float)u.GetStats().CurrentHealth);
        }

        public SetHealth(uint itemHash) : base(PacketCmdS2C.PKT_S2C_SetHealth, itemHash)
        {
            buffer.Write((short)0);
        }

    }

    public class SetModelTransparency : BasePacket
    {
        public SetModelTransparency(Unit u, float transparency) : base(PacketCmdS2C.PKT_S2C_SetModelTransparency, u.getNetId())
        {
            // Applied to Teemo's mushrooms for example
            buffer.Write((byte)0xDB);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0xC0);
            buffer.Write((byte)0x3F);
            buffer.Write(transparency); // 0.0 : fully transparent, 1.0 : fully visible
        }
    }

    public class TeleportRequest : BasePacket
    {
        static short a = 0x01;
        public TeleportRequest(uint netId, float x, float y, bool first) : base(PacketCmdS2C.PKT_S2C_MoveAns)
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
        public uint targetNetId; // If 0, use coordinates, else use target net id

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
            targetNetId = reader.ReadUInt32();
        }
    }

    public class CastSpellAns : GamePacket
    {

        public CastSpellAns(Spell s, float x, float y, uint futureProjNetId, uint spellNetId) : base(PacketCmdS2C.PKT_S2C_CastSpellAns, s.getOwner().getNetId())
        {
            var m = s.getOwner().GetGame().GetMap();

            buffer.Write((byte)0);
            buffer.Write((byte)0x66);
            buffer.Write((byte)0x00); // unk
            buffer.Write((int)s.getId()); // Spell hash, for example hash("EzrealMysticShot")
            buffer.Write((int)spellNetId); // Spell net ID
            buffer.Write((byte)1); // unk
            buffer.Write((float)1.0f); // unk
            buffer.Write((int)s.getOwner().getNetId());
            buffer.Write((int)s.getOwner().getNetId());
            buffer.Write((int)s.getOwner().getChampionHash());
            buffer.Write((int)futureProjNetId); // The projectile ID that will be spawned
            buffer.Write((float)x);
            buffer.Write((float)m.GetHeightAtLocation(x, y));
            buffer.Write((float)y);
            buffer.Write((float)x);
            buffer.Write((float)m.GetHeightAtLocation(x, y));
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
            buffer.Write((float)s.getOwner().GetZ());
            buffer.Write((float)s.getOwner().getY());
            buffer.Write((long)1); // unk
        }
    }

    public class PlayerInfo : BasePacket
    {
        public PlayerInfo(ClientInfo player) : base(PacketCmdS2C.PKT_S2C_PlayerInfo, player.GetChampion().getNetId())
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

            var summonerSpells = player.getSummoners();
            buffer.Write((int)summonerSpells[0]);
            buffer.Write((int)summonerSpells[1]);

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
            float targetZ = p.GetGame().GetMap().GetHeightAtLocation(p.getTarget().getX(), p.getTarget().getY());

            buffer.Write((float)p.getX());
            buffer.Write((float)p.GetZ());
            buffer.Write((float)p.getY());
            buffer.Write((float)p.getX());
            buffer.Write((float)p.GetZ());
            buffer.Write((float)p.getY());
            buffer.Write((long)0x000000003f510fe2); // unk
            buffer.Write((float)0.577f); // unk
            buffer.Write((float)p.getTarget().getX());
            buffer.Write((float)targetZ);
            buffer.Write((float)p.getTarget().getY());
            buffer.Write((float)p.getX());
            buffer.Write((float)p.GetZ());
            buffer.Write((float)p.getY());
            buffer.Write((float)p.getTarget().getX());
            buffer.Write((float)targetZ);
            buffer.Write((float)p.getTarget().getY());
            buffer.Write((float)p.getX());
            buffer.Write((float)p.GetZ());
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
            buffer.Write((float)p.GetZ());
            buffer.Write((float)p.getY());
            buffer.Write((long)0x0000000000000000); // unk
        }

    }

    public class SpawnParticle : BasePacket
    {
        const short MAP_WIDTH = (13982 / 2);
        const short MAP_HEIGHT = (14446 / 2);

        public SpawnParticle(Champion owner, GameObjects.Target t, string particle, uint netId) : base(PacketCmdS2C.PKT_S2C_SpawnParticle, owner.getNetId())
        {
            buffer.Write((byte)1); // number of particles
            buffer.Write((uint)owner.getChampionHash());
            buffer.Write(RAFManager.getInstance().getHash(particle));
            buffer.Write((int)0x00000020); // flags ?
            buffer.Write((int)0); // unk
            buffer.Write((short)0); // unk
            buffer.Write((byte)1); // number of targets ?
            buffer.Write((uint)owner.getNetId());
            buffer.Write((uint)netId); // Particle net id ?
            buffer.Write((uint)owner.getNetId());

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

            buffer.Write((uint)0); // unk
            buffer.Write((uint)0); // unk
            buffer.Write((uint)0); // unk
            buffer.Write((uint)0); // unk
            buffer.Write(BitConverter.GetBytes(1.0f)); // unk

        }
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
            var stats = new Dictionary<MasterMask, Dictionary<FieldMask, float>>();

            if (partial)
                stats = u.GetStats().GetUpdatedStats();
            else
                stats = u.GetStats().GetAllStats();
            var orderedStats = stats.OrderBy(x => x.Key);

            buffer.Write((byte)1); // updating 1 unit

            byte masterMask = 0;
            foreach (var p in orderedStats)
                masterMask |= (byte)p.Key;

            buffer.Write((byte)masterMask);
            buffer.Write((uint)u.getNetId());

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
        public LevelPropSpawn(LevelProp lp) : base(PacketCmdS2C.PKT_S2C_LevelPropSpawn)
        {
            buffer.Write((int)lp.getNetId());
            buffer.Write((int)0x00000040); // unk
            buffer.Write((byte)0); // unk
            buffer.Write((float)lp.getX());
            buffer.Write((float)lp.GetZ());
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
            buffer.Write((byte)0); // unk
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
            buffer.Write((byte)1); // bIsProp -- if is a prop, become unselectable and use direction params
            foreach (var b in Encoding.Default.GetBytes(name))
                buffer.Write((byte)b);
            buffer.fill(0, 64 - name.Length);
            foreach (var b in Encoding.Default.GetBytes(type))
                buffer.Write(b);
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
            buffer.Write(c.GetStats().Level);
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
        public SetCooldown(uint netId, byte slotId, float currentCd, float totalCd = 0.0f) : base(PacketCmdS2C.PKT_S2C_SetCooldown, netId)
        {
            buffer.Write(slotId);
            buffer.Write((byte)0xF8); // 4.18
            buffer.Write(totalCd);
            buffer.Write(currentCd);
        }
    }

    public class EnableFOW : BasePacket
    {
        public EnableFOW(bool activate) : base(PacketCmdS2C.PKT_S2C_EnableFOW)
        {
            buffer.Write(activate ? 0x01 : 0x00);
        }
    }
}

using IntWarsSharp.Core.Logic.PacketHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IntWarsSharp;
using System.IO;

namespace SnifferApp.net.Packets
{
    public class Packets
    {
        private readonly byte[] bytes;
        internal BinaryReader reader;
        internal List<Tuple<string, string, object>> data = new List<Tuple<string, string, object>>();

        public Packets(byte[] bytes)
        {
            reader = new BinaryReader(new MemoryStream(bytes));
            this.bytes = bytes;
        }
        internal byte readByte(string name)
        {
            byte b = reader.ReadByte();
            data.Add(new Tuple<string, string, object>("b", name, b));
            return b;
        }
        internal void readShort(string name)
        {
            data.Add(new Tuple<string, string, object>("s", name, reader.ReadInt16()));
        }
        internal int readInt(string name)
        {
            int i = reader.ReadInt32();
            data.Add(new Tuple<string, string, object>("d", name, i));
            return i;
        }
        internal void readUInt(string name)
        {
            data.Add(new Tuple<string, string, object>("d+", name, reader.ReadUInt32()));
        }
        internal void readLong(string name)
        {
            data.Add(new Tuple<string, string, object>("l", name, reader.ReadInt64()));
        }
        internal void readFloat(string name)
        {
            data.Add(new Tuple<string, string, object>("f", name, reader.ReadSingle()));
        }
        internal void readFill(int len, string name)
        {
            data.Add(new Tuple<string, string, object>("fill", name, reader.ReadBytes(len)));
        }
        internal void readString(int len, string name)
        {
            var buff = new List<byte>();
            for (var i = 0; i < len; i++)
                buff.Add(reader.ReadByte());

            data.Add(new Tuple<string, string, object>("str", name, Encoding.Default.GetString(buff.ToArray())));
        }
        internal void readZeroTerminatedString(string name)
        {
            var buff = new List<byte>();
            byte b = 0;
            do
            {
                b = reader.ReadByte();
                buff.Add(b);
            } while (b != 0);

            data.Add(new Tuple<string, string, object>("str", name, Encoding.Default.GetString(buff.ToArray())));
        }
        internal void close()
        {
            if (reader.BaseStream.Position < reader.BaseStream.Length)
                readFill((int)(reader.BaseStream.Length - reader.BaseStream.Position), "unk(Not defined)");
            reader.Close();
        }
        internal int getBufferLength()
        {
            return (int)reader.BaseStream.Length;
        }

        //lol
        internal bool isEnterVisionPacket()
        {
            if (bytes[0] != (byte)PacketCmdS2C.PKT_S2C_ObjectSpawn)
                return false;

            bool isEnterVision = true;
            for (var i = 5; i < 18; i++)
                if (bytes[i] != 0)
                    isEnterVision = false;
            isEnterVision = isEnterVision && BitConverter.ToSingle(bytes.Skip(18).Take(4).ToArray(), 0) == 1.0f;
            for (var i = 22; i < 35; i++)
                if (bytes[i] != 0)
                    isEnterVision = false;

            return isEnterVision;
        }

        internal bool isHeroSpawn()
        {
            bool isHeroSpawn = true;
            for (int i = 5; i < 20; i++)
                if (bytes[i] != 0)
                    isHeroSpawn = false;
            isHeroSpawn = isHeroSpawn && bytes[20] == 0x80;
            isHeroSpawn = isHeroSpawn && bytes[21] == 0x3F;
            for (int i = 22; i < 35; i++)
                if (bytes[i] != 0)
                    isHeroSpawn = false;

            return isHeroSpawn;
        }
    }
    public class PKT_C2S_ClientReady : Packets
    {
        public PKT_C2S_ClientReady(byte[] data) : base(data)
        {
            readByte("cmd");
            readInt("playerId");
            readInt("teamId");
            close();
        }
    }

    public class PKT_S2C_SynchVersion : Packets
    {

        public PKT_S2C_SynchVersion(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readByte("unk(9)");
            readInt("mapId");
            for (var i = 0; i < 12; i++)
            {
                readLong("userId");
                readShort("unk(0x1E)");
                readInt("summonerSpell1");
                readInt("summonerSpell2");
                readByte("isBot");
                readInt("teamId");
                readFill(64, "formerName");
                readFill(64, "none");
                readString(24, "rank");
                readInt("icon");
                readShort("ribbon");
            }

            readString(256, "version");
            readString(128, "gameMode");
            readString(3, "serverLocale");
            readFill(2333, "unk");
            readInt("gameFeatures"); // gameFeatures (turret range indicators, etc.)
            readFill(256, "unk");
            readInt("unk");
            readFill(19, "unk(1)");
            close();
        }
    }

    public class PKT_S2C_Ping_Load_Info : Packets
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

        public PKT_S2C_Ping_Load_Info(byte[] data) : base(data)
        {
            readByte("cmd");
            readInt("netId");
            readInt("unk");
            readLong("userId");
            readFloat("loaded");
            readFloat("ping");
            readShort("unk");
            readShort("unk");
            readByte("unk");
            close();
        }
    }
    public class PKT_C2S_Ping_Load_Info : PKT_S2C_Ping_Load_Info
    {
        public PKT_C2S_Ping_Load_Info(byte[] data) : base(data)
        {

        }
    }

    public class PKT_S2C_LoadScreenInfo : Packets
    {
        public PKT_S2C_LoadScreenInfo(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            //Zero this complete buffer
            readInt("blueMax");
            readInt("blueMax");

            for (var i = 0; i < 6; i++)
                readLong("userId");

            readFill(144, "unk");

            for (int i = 0; i < 6; i++)
                readLong("userId");

            readFill(144, "unk");
            readInt("currentBlue");
            readInt("currentPurple");
            close();
        }
    }

    public class PKT_S2C_KeyCheck : Packets
    {
        public PKT_S2C_KeyCheck(byte[] data) : base(data)
        {
            readByte("cmd");
            readByte("unk(0x2A?)");
            readByte("unk");
            readByte("unk(0xFF?)");
            readInt("playerNo");
            readLong("userId");
            readInt("unk(0)");
            readLong("unk(0)");
            readInt("unk(0)");
            close();
        }
    }

    public class PKT_C2S_KeyCheck : PKT_S2C_KeyCheck
    {
        public PKT_C2S_KeyCheck(byte[] data) : base(data)
        {

        }
    }

    public class PKT_C2S_LockCamera : Packets
    {
        public PKT_C2S_LockCamera(byte[] data) : base(data)
        {
            readByte("cmd");
            readInt("netId");
            readByte("isLock");
            readInt("padding");
            close();
        }
    }

    /*typedef struct _ViewReq {
        byte cmd;
        int unk1;
        float x;
        float zoom;
        float y;
        float y2;		//Unk
        int width;	//Unk
        int height;	//Unk
        int unk2;	//Unk
        byte requestNo;
    } ViewReq;*/
    public class PKT_C2S_ViewReq : Packets
    {
        public PKT_C2S_ViewReq(byte[] data) : base(data)
        {
            readByte("cmd");
            readInt("netId");
            readFloat("x");
            readFloat("zoom");
            readFloat("y");
            readFloat("y2");
            readInt("width");
            readInt("height");
            readInt("unk");
            readByte("requestNo");
            close();
        }
    }

    public class PKT_S2C_ObjectSpawn : Packets //Minion Spawn
    {
        public PKT_S2C_ObjectSpawn(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            if (getBufferLength() == 8) //MinionSpawn2
            {
                readUInt("netId");
                readFill(3, "unk");
                close();
                return;
            }
            if (isHeroSpawn()) //HeroSpawn2
            {
                readFill(15, "unk");
                readByte("unk(0x80)");
                readByte("unk(0x3F)");
                readFill(13, "unk");
                readByte("unk(3)");
                readInt("unk(1)");
                readFloat("x");
                readFloat("y");
                readFloat("z?");
                readFloat("rotation?");
                close();
                return;
            }
            if (isEnterVisionPacket())
            {
                readFill(13, "unk");
                readFloat("unk(1)");
                readFill(13, "unk");
                readByte("unk(0x02)");
                readInt("tickCount");

                var coordsCount = readByte("coordCount");
                readInt("netId");
                readByte("movementMask(0)");
                for (int i = 0; i < coordsCount / 2; i++)
                {
                    readShort("x");
                    readShort("y");
                }
                close();
                return;
            }
            readInt("unk"); // unk
            readByte("spawnType(3)"); // SpawnType - 3 = minion
            readInt("netId");
            readInt("netId");
            readInt("spawnPos");
            readByte("unk(255)");
            readByte("unk(1)");
            readByte("type");
            readByte("minionType(0=melee)");
            readByte("unk");
            readInt("minionSpawnType");
            readInt("unk");
            readInt("unk");
            readShort("unk");
            readFloat("unk(1)");
            readInt("unk");
            readInt("unk");
            readInt("unk");
            readShort("unk(512)");
            readInt("tickCount");

            var count = readByte("coordCount");
            readInt("netId");
            readByte("movementMask(0)");
            for (int i = 0; i < count / 2; i++)
            {
                readShort("x");
                readShort("y");
            }
            close();
        }
    }

    class PKT_S2C_SpellAnimation : Packets
    {
        public PKT_S2C_SpellAnimation(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readInt("unk(5)");
            readInt("unk");
            readInt("unk");
            readFloat("unk(1)");
            readZeroTerminatedString("animationName");
            close();
        }
    }

    class PKT_S2C_SetAnimation : Packets
    {
        public PKT_S2C_SetAnimation(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            var count = readByte("animationPairsCount");

            for (int i = 0; i < count; i++)
            {
                var strLen = readInt("animationPair1Length");
                readString(strLen, "animationPair1");
                strLen = readInt("animationPair2Length");
                readString(strLen, "animationPair2");
            }
            close();
        }
    }

    public class PKT_S2C_FaceDirection : Packets
    {
        public PKT_S2C_FaceDirection(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readFloat("relativeX");
            readFloat("relativeZ");
            readFloat("relativeY");
            readByte("unk");
            readFloat("unk(0.0833)");
            close();
        }
    };

    public class PKT_S2C_Dash : Packets
    {
        public PKT_S2C_Dash(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readInt("tickCount");
            readShort("numUpdates?(1)");
            readByte("unk(5)");
            readInt("netId");
            readByte("unk(0)");
            readFloat("dashSpeed");
            readInt("unk");
            readFloat("x");
            readFloat("y");
            readInt("unk(0)");
            readByte("unk(0)");
            readInt("unk");
            readUInt("unk");
            readInt("unk");
            readByte("vectorBitmask"); // Vector bitmask on whether they're int16 or byte
            readShort("fromX");
            readShort("fromY");
            readShort("toX");
            readShort("toY");
            close();
        }
    }

    public class PKT_S2C_LeaveVision : Packets
    {
        public PKT_S2C_LeaveVision(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            close();
        }
    }

    public class PKT_S2C_DeleteObject : Packets
    {
        public PKT_S2C_DeleteObject(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            close();
        }
    }

    public class AddGold : Packets
    {
        public AddGold(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readInt("killerNetId");
            readInt("killedNetId");
            readFloat("gold");
            close();
        }
    }

    public class PKT_C2S_MoveReq : Packets
    {
        public PKT_C2S_MoveReq(byte[] data) : base(data)
        {
            readByte("cmd");
            readInt("netId");
            readByte("moveType");
            readFloat("x");
            readFloat("y");
            readInt("targetNetId");
            readByte("coordsCount");
            readInt("netId");
            readFill((int)(reader.BaseStream.Length - reader.BaseStream.Position), "moveData");
            close();
        }
    }

    public class PKT_S2C_MoveAns : Packets
    {
        public PKT_S2C_MoveAns(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readInt("tickCount");
            readShort("updatesCount");          //count
            int coordsCount = readByte("coordsCount");
            readInt("actorNetId");
            for (var i = 0; i < (coordsCount + 5) / 8; i++)
                readByte("coordsMask");

            for (int i = 0; i < coordsCount / 2; i++)
            {
                readShort("coord" + i + "x");
                readShort("coord" + i + "y");
            }
            close();
        }
    }

    public class PKT_S2C_ViewAns : Packets
    {
        public PKT_S2C_ViewAns(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readByte("requestNo");
            close();
        }
    }


    public class PKT_S2C_QueryStatusAns : Packets
    {
        public PKT_S2C_QueryStatusAns(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readByte("status(1=ok)");
            close();
        }
    }

    public class PKT_C2S_SynchVersion : Packets
    {
        public PacketCmdS2C cmd;
        public int netId;
        public int unk1;
        private byte[] _version = new byte[256]; // version string might be shorter?

        public PKT_C2S_SynchVersion(byte[] data) : base(data)
        {
            readByte("cmd");
            readInt("netId");
            readInt("unk");
            readString(256, "versionString");
            close();
        }
    }

    public class PKT_S2C_World_SendGameNumber : Packets
    {
        public PKT_S2C_World_SendGameNumber(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readLong("gameId");
            readString(128, "clientName");
            close();
        }
    }

    //app crash inc
    public class PKT_C2S_StatsConfirm : Packets
    {
        public PKT_C2S_StatsConfirm(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readInt("tickCount");
            readByte("updateNo");
            readByte("masterMask");
            readInt("netId");
            var mask = readInt("mask");
            var size = readByte("size");
            for (int i = 0; i < size; i++)
            {
                if (mask == 0)
                    readShort("value");
                else
                    readByte("value");
            }
            close();
        }
    }

    public class PKT_C2S_ChatBoxMessage : Packets
    {
        public PKT_C2S_ChatBoxMessage(byte[] data) : base(data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            readByte("cmd");
            readInt("playerNetId");
            readInt("botNetId");
            readByte("idBotMsg");
            readInt("msgType");
            readInt("unk");
            var len = readInt("msgLen");
            readFill(32, "unk");
            readString(len, "msg");
            close();
        }
    }

    public class PKT_S2C_UpdateModel : Packets
    {
        public PKT_S2C_UpdateModel(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readInt("netId");
            readByte("bOk");
            readInt("unk(-1)");
            readString(32, "modelName");
            close();
        }
    }

    public class PKT_S2C_EndSpawn : Packets
    {
        public PKT_S2C_EndSpawn(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            close();
        }
    }
    public class PKT_S2C_StartGame : PKT_S2C_EndSpawn
    {
        public PKT_S2C_StartGame(byte[] bytes) : base(bytes)
        {
        }
    }

    public class PKT_S2C_StartSpawn : Packets
    {
        public PKT_S2C_StartSpawn(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readShort("unk");
            close();
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

    public class Click : Packets
    {
        public Click(byte[] data) : base(data)
        {
            readByte("cmd");
            readInt("netId");
            readInt("zero");
            readInt("targetNetId");
            close();
        }
    }

    public class PKT_S2C_HeroSpawn : Packets
    {
        public PKT_S2C_HeroSpawn(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readInt("playerNetId");
            readInt("playerId");
            readByte("netNodeID?(40)");
            readByte("botSkillLevel");
            readByte("teamId");
            readByte("isBot");
            readByte("spawnPosIndex");
            readInt("skinNo");
            readString(128, "playerName");
            readString(40, "championType");
            readFloat("deathDurationRemaining");
            readFloat("timeSinceDeath");
            readInt("unk");
            readByte("bitField");
            close();
        }
    }

    public class PKT_S2C_TurretSpawn : Packets
    {
        public PKT_S2C_TurretSpawn(byte[] b) : base(b)
        {
            readByte("cmd");
            readInt("netId");
            readInt("netId");
            readString(64, "name");
            readByte("unk");
            readByte("unk");
            readByte("unk");
            readByte("unk");
            readByte("unk");
            readByte("unk");
            readByte("unk");
            close();
        }
    }

    public class PKT_S2C_GameTimer : Packets
    {
        public PKT_S2C_GameTimer(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readFloat("time");
            close();
        }
    }

    public class PKT_S2C_GameTimerUpdate : Packets
    {
        public PKT_S2C_GameTimerUpdate(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readFloat("time");
        }
    }

    public class PKT_C2S_HeartBeat : Packets
    {
        public PKT_C2S_HeartBeat(byte[] data) : base(data)
        {
            readByte("cmd");
            readInt("netId");
            readFloat("receiveTime");
            readFloat("ackTime");
            close();
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

    public class PKT_C2S_SkillUp : Packets
    {
        public PacketCmdC2S cmd;
        public int netId;
        public byte skill;
        public PKT_C2S_SkillUp(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readByte("skillId");
        }
    }
    public class PKT_S2C_SkillUp : Packets
    {
        public PKT_S2C_SkillUp(byte[] bytes) : base(bytes)
        {
            readByte("cmd");
            readInt("netId");
            readByte("skill");
            readByte("level");
            readByte("pointsLeft");
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

    public class AddBuff : Packets
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

    public class LoadScreenPlayerName : Packets
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

    public class LoadScreenPlayerChampion : Packets
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

    public class AttentionPingAns : Packets
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
    public class SetHealth2 : Packets //shhhhh...
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

        public class ViewAnswer : Packets
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

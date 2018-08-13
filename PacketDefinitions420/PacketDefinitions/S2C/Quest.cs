using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class Quest : BasePacket
    {
        public Quest(string title, string description, byte type, byte command, uint netid, byte questEvent = 0)
            : base(PacketCmd.PKT_S2C_QUEST)
        {
			WriteConstLengthString(title, 256);
			WriteConstLengthString(description, 256);
            Write(type); // 0 : Primary quest, 1 : Secondary quest
            Write(command); // 0 : Activate quest, 1 : Complete quest, 2 : Remove quest
            Write(questEvent); // 0 : Roll over, 1 : Roll out, 2 : Mouse down, 3 : Mouse up
            Write((int)netid);
        }
    }
}
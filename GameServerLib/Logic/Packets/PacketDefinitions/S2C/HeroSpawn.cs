using System.Text;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class HeroSpawn : BasePacket
    {
        public HeroSpawn(HeroSpawnArgs args)
            : base(PacketCmd.PKT_S2C_HeroSpawn)
        {
            buffer.Write((uint)args.PlayerNetId);
            buffer.Write((int)args.PlayerId); // player Id
            buffer.Write((byte)40); // netNodeID ?
            buffer.Write((byte)0); // botSkillLevel Beginner=0 Intermediate=1

            if (args.PlayerTeam == TeamId.TEAM_BLUE)
                buffer.Write((byte)1); // teamNumber BotTeam=2,3 Blue=Order=1 Purple=Chaos=0
            else
                buffer.Write((byte)0); // teamNumber BotTeam=2,3 Blue=Order=1 Purple=Chaos=0

            buffer.Write((byte)0); // isBot
            buffer.Write((byte)0); // spawnPosIndex
            buffer.Write((int)args.SkinNo);
            foreach (var b in Encoding.Default.GetBytes(args.PlayerName))
                buffer.Write((byte)b);
            buffer.fill(0, 128 - args.PlayerName.Length);
            foreach (var b in Encoding.Default.GetBytes(args.ChampionName))
                buffer.Write((byte)b);
            buffer.fill(0, 40 - args.ChampionName.Length);
            buffer.Write((float)0.0f); // deathDurationRemaining
            buffer.Write((float)0.0f); // timeSinceDeath
            buffer.Write((int)0); // UNK (4.18)
            buffer.Write((byte)0); // bitField
        }
    }
}
using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AvatarInfo : BasePacket
    {
        public AvatarInfo(ClientInfo player)
            : base(PacketCmd.PKT_S2C_AvatarInfo, player.Champion.NetId)
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
            buffer.Write((uint)HashFunctions.HashString(summonerSpells[0]));
            buffer.Write((uint)HashFunctions.HashString(summonerSpells[1]));

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
}
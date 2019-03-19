using System.Collections.Generic;
using GameServerCore.NetInfo;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class AvatarInfo : BasePacket
    {
        public AvatarInfo(ClientInfo player)
            : base(PacketCmd.PKT_S2C_AVATAR_INFO, player.Champion.NetId)
        {
            var runesRequired = 30;
            foreach (var rune in player.Champion.RuneList.Runes)
            {
                Write((short)rune.Value);
                Write((short)0x00);
                runesRequired--;
            }

            for (var i = 1; i <= runesRequired; i++)
            {
                Write((short)0);
                Write((short)0);
            }

            var summonerSpells = player.SummonerSkills;
            WriteStringHash(summonerSpells[0]);
            WriteStringHash(summonerSpells[1]);

            var talentsRequired = 80;
            var talentsHashes = new Dictionary<int, byte>
            {
                { 0, 0 } // hash, level
            };

            foreach (var talent in talentsHashes)
            {
                Write(talent.Key); // hash
                Write(talent.Value); // level
                talentsRequired--;
            }

            for (var i = 1; i <= talentsRequired; i++)
            {
                Write(0);
                Write((byte)0);
            }

            Write((short)30); // avatarLevel
        }
    }
}
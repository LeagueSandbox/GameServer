using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AvatarInfo : BasePacket
    {
        public AvatarInfo(ClientInfo player)
            : base(PacketCmd.PKT_S2C_AVATAR_INFO, player.Champion.NetId)
        {
            var runesRequired = 30;
            foreach (var rune in player.Champion.RuneList.Runes)
            {
                _buffer.Write((short)rune.Value);
                _buffer.Write((short)0x00);
                runesRequired--;
            }

            for (var i = 1; i <= runesRequired; i++)
            {
                _buffer.Write((short)0);
                _buffer.Write((short)0);
            }

            var summonerSpells = player.SummonerSkills;
            _buffer.Write(HashFunctions.HashString(summonerSpells[0]));
            _buffer.Write(HashFunctions.HashString(summonerSpells[1]));

            var talentsRequired = 80;
            var talentsHashes = new Dictionary<int, byte>
            {
                { 0, 0 } // hash, level
            };

            foreach (var talent in talentsHashes)
            {
                _buffer.Write(talent.Key); // hash
                _buffer.Write(talent.Value); // level
                talentsRequired--;
            }

            for (var i = 1; i <= talentsRequired; i++)
            {
                _buffer.Write(0);
                _buffer.Write((byte)0);
            }

            _buffer.Write((short)30); // avatarLevel
        }
    }
}
using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AvatarInfo : BasePacket
    {
        public AvatarInfo(ClientInfo player)
            : base(PacketCmd.PKT_S2_C_AVATAR_INFO, player.Champion.NetId)
        {
            int runesRequired = 30;
            foreach (var rune in player.Champion.RuneList.Runes)
            {
                _buffer.Write((short)rune.Value);
                _buffer.Write((short)0x00);
                runesRequired--;
            }
            for (int i = 1; i <= runesRequired; i++)
            {
                _buffer.Write((short)0);
                _buffer.Write((short)0);
            }

            var summonerSpells = player.SummonerSkills;
            _buffer.Write((uint)HashFunctions.HashString(summonerSpells[0]));
            _buffer.Write((uint)HashFunctions.HashString(summonerSpells[1]));

            int talentsRequired = 80;
            var talentsHashes = new Dictionary<int, byte>(){
                { 0, 0 } // hash, level
            };

            foreach (var talent in talentsHashes)
            {
                _buffer.Write((int)talent.Key); // hash
                _buffer.Write((byte)talent.Value); // level
                talentsRequired--;
            }
            for (int i = 1; i <= talentsRequired; i++)
            {
                _buffer.Write((int)0);
                _buffer.Write((byte)0);
            }

            _buffer.Write((short)30); // avatarLevel
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Content;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct AvatarInfoArgs
    {
        public uint PlayerNetId { get; }
        public RuneCollection Runes { get; }
        public string SummonerSpell1 { get; }
        public string SummonerSpell2 { get; }

        public AvatarInfoArgs(uint playerNetId, RuneCollection runes, string summonerSpell1, string summonerSpell2)
        {
            PlayerNetId = playerNetId;
            Runes = runes;
            SummonerSpell1 = summonerSpell1;
            SummonerSpell2 = summonerSpell2;
        }
    }
}

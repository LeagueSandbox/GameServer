using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class SkillUpRequest
    {
        public uint NetId { get; }
        public byte Skill { get; }

        public SkillUpRequest(uint netId, byte skill)
        {
            NetId = netId;
            Skill = skill;
        }
    }
}

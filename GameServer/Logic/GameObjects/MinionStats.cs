using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum MinionFieldMask : uint
    {
        Minion_FM2_CurrentHp = 0x00000001,
        Minion_FM2_MaxHp = 0x00000002,
        Minion_FM2_Ad = 0x00001000,
        Minion_FM2_Atks = 0x00004000,
        Minion_FM4_MoveSpeed = 0x0000004
    };

    public class MinionStats : Stats
    {
        protected float range;
        public MinionStats()
        {
            range = 0;
        }

        public override byte getSize(MasterMask blockId, FieldMask stat)
        {
            return 4;
        }
    }
}

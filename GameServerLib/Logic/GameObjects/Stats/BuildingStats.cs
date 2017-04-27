using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using System.Collections.Generic;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum BuildingFieldMask : uint
    {
        Building_FM2_CurrentHp = 0x00000001,
    };

    public class BuildingStats : Stats
    {
        public override float CurrentHealth
        {
            get
            {
                return _currentHealth;
            }
            set
            {
                _currentHealth = value;
                appendStat(_updatedStats, MasterMask.MM_Two, (FieldMask) BuildingFieldMask.Building_FM2_CurrentHp, CurrentHealth);
            }
        }

        protected float range;
        public BuildingStats()
        {
            range = 0;
        }

        public override byte getSize(MasterMask blockId, FieldMask stat)
        {
            return 4;
        }

        public override Dictionary<MasterMask, Dictionary<FieldMask, float>> GetAllStats()
        {
            var toReturn = new Dictionary<MasterMask, Dictionary<FieldMask, float>>();
            Dictionary<MasterMask, Dictionary<FieldMask, float>> stats = new Dictionary<MasterMask, Dictionary<FieldMask, float>>();
            appendStat(stats, MasterMask.MM_Two, (FieldMask)BuildingFieldMask.Building_FM2_CurrentHp, CurrentHealth);

            foreach (var block in stats)
            {
                if (!toReturn.ContainsKey(block.Key))
                    toReturn.Add(block.Key, new Dictionary<FieldMask, float>());
                foreach (var stat in block.Value)
                    toReturn[block.Key].Add(stat.Key, stat.Value);
            }
            return toReturn;
        }

        public override float GetStat(MasterMask blockId, FieldMask stat)
        {
            BuildingFieldMask minionStat = (BuildingFieldMask) stat;
            if (blockId == MasterMask.MM_One)
            {
            }
            else if (blockId == MasterMask.MM_Two)
            {
                switch (minionStat)
                {
                    case BuildingFieldMask.Building_FM2_CurrentHp:
                        return CurrentHealth;
                }
            }
            else if (blockId == MasterMask.MM_Three)
            {
            }
            else if (blockId == MasterMask.MM_Four)
            {
            }
            return 0;
        }
    }
}

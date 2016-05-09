using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Core.Logic;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum BuildingFieldMask : uint
    {
        Building_FM2_CurrentHp = 0x00000001,
        Building_FM2_MaxHp = 0x00000002,
        Building_FM2_Ad = 0x00001000,
        Building_FM2_Atks = 0x00004000,
        Building_FM4_MoveSpeed = 0x0000004
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
            appendStat(stats, MasterMask.MM_Two, (FieldMask)BuildingFieldMask.Building_FM2_MaxHp, HealthPoints.Total);
            appendStat(stats, MasterMask.MM_Two, (FieldMask)BuildingFieldMask.Building_FM2_Ad, AttackDamage.Total);
            appendStat(stats, MasterMask.MM_Two, (FieldMask)BuildingFieldMask.Building_FM2_Atks, AttackSpeedMultiplier.Total);
            appendStat(stats, MasterMask.MM_Four, FieldMask.FM4_ModelSize, Size.Total);

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
                    case BuildingFieldMask.Building_FM2_MaxHp:
                        return HealthPoints.Total;
                    case BuildingFieldMask.Building_FM2_Ad:
                        return AttackDamage.Total;
                    case BuildingFieldMask.Building_FM2_Atks:
                        return AttackSpeedMultiplier.Total;
                    case BuildingFieldMask.Building_FM4_MoveSpeed:
                        return MoveSpeed.Total;
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

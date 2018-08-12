using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public enum TurretFieldMask : uint
    {
        Turret_FM4_CurrentHp = 0x00000001,
        Turret_FM4_MaxHp = 0x00000002,
    };

    public class TurretStats : Stats
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
                appendStat(_updatedStats, MasterMask.MM_Four, (FieldMask) TurretFieldMask.Turret_FM4_CurrentHp, CurrentHealth);
            }
        }

        protected float range;
        public TurretStats()
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
            appendStat(stats, MasterMask.MM_Four, (FieldMask)TurretFieldMask.Turret_FM4_CurrentHp, CurrentHealth);
            appendStat(stats, MasterMask.MM_Four, (FieldMask)TurretFieldMask.Turret_FM4_MaxHp, HealthPoints.Total);

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
            TurretFieldMask turretStat = (TurretFieldMask) stat;
            if (blockId == MasterMask.MM_One)
            {
            }
            else if (blockId == MasterMask.MM_Two)
            {
            }
            else if (blockId == MasterMask.MM_Three)
            {
            }
            else if (blockId == MasterMask.MM_Four)
            {
                switch (turretStat)
                {
                    case TurretFieldMask.Turret_FM4_CurrentHp:
                        return CurrentHealth;
                    case TurretFieldMask.Turret_FM4_MaxHp:
                        return HealthPoints.Total;
                }
            }
            return 0;
        }
    }
}

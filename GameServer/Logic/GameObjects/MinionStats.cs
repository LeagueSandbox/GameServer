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

        public override float getMaxHealth()
        {
            return getStat(MasterMask.MM_Two, MinionFieldMask.Minion_FM2_MaxHp);
        }

        public override float getCurrentHealth()
        {
            return getStat(MasterMask.MM_Two, MinionFieldMask.Minion_FM2_CurrentHp);
        }

        public override float getBaseAd()
        {
            return getStat(MasterMask.MM_Two, MinionFieldMask.Minion_FM2_Ad);
        }

        public override float getRange()
        {
            return range;
        }

        public override float getBaseAttackSpeed()
        {
            return getStat(MasterMask.MM_Two, MinionFieldMask.Minion_FM2_Atks);
        }

        public override float getMovementSpeed()
        {
            return getStat(MasterMask.MM_Four, MinionFieldMask.Minion_FM4_MoveSpeed);
        }

        public override float getAttackSpeedMultiplier()
        {
            return 1.0f;
        }

        public override void setMaxHealth(float health)
        {
            setStat(MasterMask.MM_Two, MinionFieldMask.Minion_FM2_MaxHp, health);
            updatedHealth = true;
        }

        public override void setCurrentHealth(float health)
        {
            setStat(MasterMask.MM_Two, MinionFieldMask.Minion_FM2_CurrentHp, health);
            updatedHealth = true;
        }

        public override void setBaseAd(float ad)
        {
            setStat(MasterMask.MM_Two, MinionFieldMask.Minion_FM2_Ad, ad);
        }

        public override void setRange(float range)
        {
            this.range = range;
        }

        public override void setBaseAttackSpeed(float speed)
        {
            setStat(MasterMask.MM_Two, MinionFieldMask.Minion_FM2_Atks, speed);
        }

        public override void setMovementSpeed(float speed)
        {
            setStat(MasterMask.MM_Four, MinionFieldMask.Minion_FM4_MoveSpeed, speed);
        }

        public override void setAttackSpeedMultiplier(float multiplier)
        {
        }

        public override byte getSize(MasterMask blockId, FieldMask stat)
        {
            return 4;
        }

    }
}

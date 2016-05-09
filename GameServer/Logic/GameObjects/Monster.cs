using InibinSharp;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Monster : Unit
    {
        private Vector2 facing;
        private string name;

        public Monster(Game game, uint id, float x, float y, float facingX, float facingY, string model, string name) : base(game, id, model, new Stats(), 40, x, y)
        {
            setTeam(TeamId.TEAM_NEUTRAL);

            var teams = Enum.GetValues(typeof(TeamId)).Cast<TeamId>();
            foreach (var team in teams)
                setVisibleByTeam(team, true);

            setMoveOrder(MoveOrder.MOVE_ORDER_MOVE);
            facing = new Vector2(facingX, facingY);
            stats.setAttackSpeedMultiplier(1.0f);
            this.name = name;

            Inibin inibin;
            if (!RAFManager.getInstance().readInibin("DATA/Characters/" + model + "/" + model + ".inibin", out inibin))
            {
                Logger.LogCoreError("couldn't find monster stats for " + model);
                return;
            }

            stats.setCurrentHealth(inibin.getFloatValue("Data", "BaseHP"));
            stats.setMaxHealth(inibin.getFloatValue("Data", "BaseHP"));
            // stats.setCurrentMana(inibin.getFloatValue("Data", "BaseMP"));
            // stats.setMaxMana(inibin.getFloatValue("Data", "BaseMP"));
            stats.setBaseAd(inibin.getFloatValue("DATA", "BaseDamage"));
            stats.setRange(inibin.getFloatValue("DATA", "AttackRange"));
            stats.setBaseMovementSpeed(inibin.getFloatValue("DATA", "MoveSpeed"));
            stats.setArmor(inibin.getFloatValue("DATA", "Armor"));
            stats.setMagicArmor(inibin.getFloatValue("DATA", "SpellBlock"));
            stats.setHp5(inibin.getFloatValue("DATA", "BaseStaticHPRegen"));
            stats.setMp5(inibin.getFloatValue("DATA", "BaseStaticMPRegen"));

            stats.setHealthPerLevel(inibin.getFloatValue("DATA", "HPPerLevel"));
            stats.setManaPerLevel(inibin.getFloatValue("DATA", "MPPerLevel"));
            stats.setAdPerLevel(inibin.getFloatValue("DATA", "DamagePerLevel"));
            stats.setArmorPerLevel(inibin.getFloatValue("DATA", "ArmorPerLevel"));
            stats.setMagicArmorPerLevel(inibin.getFloatValue("DATA", "SpellBlockPerLevel"));
            stats.setHp5RegenPerLevel(inibin.getFloatValue("DATA", "HPRegenPerLevel"));
            stats.setMp5RegenPerLevel(inibin.getFloatValue("DATA", "MPRegenPerLevel"));
            stats.setBaseAttackSpeed(0.625f / (1 + inibin.getFloatValue("DATA", "AttackDelayOffsetPercent")));

            setMelee(inibin.getBoolValue("DATA", "IsMelee"));
            setCollisionRadius(inibin.getIntValue("DATA", "PathfindingCollisionRadius"));

            Inibin autoAttack;
            if (!RAFManager.getInstance().readInibin("DATA/Characters/" + model + "/Spells/" + model + "BasicAttack.inibin", out autoAttack))
            {
                if (!RAFManager.getInstance().readInibin("DATA/Spells/" + model + "BasicAttack.inibin", out autoAttack))
                {
                    Logger.LogCoreError("Couldn't find monster auto-attack data for " + model);
                    return;
                }
            }

            autoAttackDelay = autoAttack.getFloatValue("SpellData", "castFrame") / 30.0f;
            autoAttackProjectileSpeed = autoAttack.getFloatValue("SpellData", "MissileSpeed");
        }

        public Vector2 getFacing()
        {
            return facing;
        }

        public override void update(long diff)
        {
            base.update(diff);
        }

        public string getName()
        {
            return name;
        }

        public override bool isInDistress()
        {
            return distressCause != null;
        }
    }
}

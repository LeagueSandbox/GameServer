using IntWarsSharp.Core.Logic;
using IntWarsSharp.Core.Logic.RAF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic.GameObjects
{
    class Champion : Unit
    {
        protected string type;
        protected List<Spell> spells = new List<Spell>();
        protected short skillPoints = 0;
        protected short skin;
        protected long respawnTimer = 0;
        protected Inventory inventory;
        protected float championGoldFromMinions = 0;
        protected long championHitFlagTimer = 0;
        public int playerId;
        public int playerHitId;

        public Spell getSpell(int index)
        {
            return spells[index];
        }
        public Champion(string type, Map map, int id, int playerId) : base(map, id, type, new Stats(), 30, 0, 0, 1200)
        {
            this.type = type;
            this.playerId = playerId;

            stats.setGold(475.0f);
            stats.setAttackSpeedMultiplier(1.0f);
            stats.setGoldPerSecond(map.getGoldPerSecond());
            stats.setGeneratingGold(false);

            List<char> iniFile = new List<char>();
            if (!RAFManager.getInstance().readFile("DATA/Characters/" + type + "/" + type + ".inibin", iniFile))
            {
                Logger.LogCoreError("couldn't find champion stats for " + type);
                return;
            }

            var inibin = getInibin(iniFile);

            stats->setCurrentHealth(inibin.getFloatValue("Data", "BaseHP"));
            stats->setMaxHealth(inibin.getFloatValue("Data", "BaseHP"));
            stats->setCurrentMana(inibin.getFloatValue("Data", "BaseMP"));
            stats->setMaxMana(inibin.getFloatValue("Data", "BaseMP"));
            stats->setBaseAd(inibin.getFloatValue("DATA", "BaseDamage"));
            stats->setRange(inibin.getFloatValue("DATA", "AttackRange"));
            stats->setBaseMovementSpeed(inibin.getFloatValue("DATA", "MoveSpeed"));
            stats->setArmor(inibin.getFloatValue("DATA", "Armor"));
            stats->setMagicArmor(inibin.getFloatValue("DATA", "SpellBlock"));
            stats->setHp5(inibin.getFloatValue("DATA", "BaseStaticHPRegen"));
            stats->setMp5(inibin.getFloatValue("DATA", "BaseStaticMPRegen"));

            stats->setHealthPerLevel(inibin.getFloatValue("DATA", "HPPerLevel"));
            stats->setManaPerLevel(inibin.getFloatValue("DATA", "MPPerLevel"));
            stats->setAdPerLevel(inibin.getFloatValue("DATA", "DamagePerLevel"));
            stats->setArmorPerLevel(inibin.getFloatValue("DATA", "ArmorPerLevel"));
            stats->setMagicArmorPerLevel(inibin.getFloatValue("DATA", "SpellBlockPerLevel"));
            stats->setHp5RegenPerLevel(inibin.getFloatValue("DATA", "HPRegenPerLevel"));
            stats->setMp5RegenPerLevel(inibin.getFloatValue("DATA", "MPRegenPerLevel"));
            stats->setBaseAttackSpeed(0.625f / (1 + inibin.getFloatValue("DATA", "AttackDelayOffsetPercent")));

            spells.Add(new Spell(this, inibin.getStringValue("Data", "Spell1"), 0));
            spells.Add(new Spell(this, inibin.getStringValue("Data", "Spell2"), 1));
            spells.Add(new Spell(this, inibin.getStringValue("Data", "Spell3"), 2));
            spells.Add(new Spell(this, inibin.getStringValue("Data", "Spell4"), 3));

            setMelee(inibin.getBoolValue("DATA", "IsMelee"));
            setCollisionRadius(inibin.getIntValue("DATA", "PathfindingCollisionRadius"));

            iniFile.Clear();
            if (!RAFManager.getInstance().readFile("DATA/Characters/" + type + "/Spells/" + type + "BasicAttack.inibin", iniFile))
            {
                if (!RAFManager.getInstance().readFile("DATA/Spells/" + type + "BasicAttack.inibin", iniFile))
                {
                    Logger.LogCoreError("Couldn't find champion auto-attack data for " + type);
                    return;
                }
            }

            var autoAttack = getAutoAttack(iniFile);

            autoAttackDelay = autoAttack.getFloatValue("SpellData", "castFrame") / 30.f;
            autoAttackProjectileSpeed = autoAttack.getFloatValue("SpellData", "MissileSpeed");

            //Fuck LUA
            /* var scriptloc = "../../lua/champions/" + getType() + "/Passive.lua";
             Logger.LogCoreInfo("Loading " + scriptloc);
             try
             {
                 unitScript = LuaScript(true);//fix

                 unitScript.lua.set("me", this);

                 unitScript.loadScript(scriptloc);

                 unitScript.lua.set_function("dealMagicDamage", [this](Unit * target, float amount) { this->dealDamageTo(target, amount, DAMAGE_TYPE_MAGICAL, DAMAGE_SOURCE_SPELL); });
                 unitScript.lua.set_function("addBuff", [this](Buff b, Unit * target){
                     target->addBuff(new Buff(b));
                     return;
                 });

                 unitScript.lua.set_function("addParticleTarget", [this](const std::string&particle, Target* u) {
                     this->getMap()->getGame()->notifyParticleSpawn(this, u, particle);
                     return;
                 });

                 // unitScript.lua.set ("me", this);
             }
             catch
             {

             }*/
        }
        public string getType()
        {
            return type;
        }

        public int getTeamSize()
        {
            int blueTeamSize = 0;
            int purpTeamSize = 0;

            foreach (var player in Config.players.Values)
            {
                switch (player.team.ToLower())
                {
                    case "blue":
                        blueTeamSize++;
                        break;
                    case "purple":
                        purpTeamSize++;
                        break;
                    default:
                        break;
                }
            }

            var playerIndex = "player" + playerId;
            if (Config.players.ContainsKey(playerIndex))
            {
                switch (Config.players[playerIndex].name.ToLower())
                {
                    case "blue":
                        return blueTeamSize;
                    case "purple":
                        return purpTeamSize;
                }
            }

            return 0;
        }

        public Tuple<float, float> getRespawnPosition()
        {
            var mapId = Config.gameConfig.map;
            var playerIndex = "player" + playerId;
            var playerTeam = "";
            var teamSize = getTeamSize();

            if (teamSize > 6) //???
                teamSize = 6;

            if (Config.players.ContainsKey(playerIndex))
            {
                var p = Config.players[playerIndex];
                playerTeam = p.team;
            }
            var x = 0;
            var y = 0;
            switch (playerTeam.ToLower())
            {
                case "blue":
                    x = Config.mapSpawns.blue[teamSize].getXForPlayer(playerId);
                    y = Config.mapSpawns.blue[teamSize].getYForPlayer(playerId);
                    break;
                case "purple":
                    x = Config.mapSpawns.purple[teamSize].getXForPlayer(playerId);
                    y = Config.mapSpawns.purple[teamSize].getYForPlayer(playerId);
                    break;
            }

            return new Tuple<float, float>(x, y);
        }

        public Spell castSpell(short slot, float x, float y, Unit target, int futureProjNetId, int spellNetId)
        {
            if (slot >= spells.Count)
                return null;

            Spell s = spells[slot];

            s.setSlot(slot);//temporary hack until we redo spells to be almost fully lua-based

            if ((s.getCost() * (1 - stats.getSpellCostReduction())) > stats.getCurrentMana() || s.getState() != SpellState.STATE_READY)
            {
                return 0;
            }

            s.cast(x, y, target, futureProjNetId, spellNetId);
            stats.setCurrentMana(stats.getCurrentMana() - (s.getCost() * (1 - stats.getSpellCostReduction())));
            return s;
        }
        public Spell levelUpSpell(uint8 slot);

        public virtual void update(int64 diff);

        public void setSkillPoints(int _skillPoints)
        {
            skillPoints = (uint8)_skillPoints;
        }

        public void setSkin(uint8 skin) { this->skin = skin; }
        public uint32 getChampionHash();

        public virtual bool isInDistress() const override { return distressCause!=0; }

    public uint8 getSkillPoints() const { return skillPoints; }
public void levelUp();

public Inventory& getInventory() { return inventory; }

public virtual void die(Unit* killer) override;
  public int64 getRespawnTimer() const { return respawnTimer; }

 public void onCollision(Object* collider);

public float getChampionGoldFromMinions() { return championGoldFromMinions; }
public void setChampionGoldFromMinions(float gold) { this->championGoldFromMinions = gold; }

public void setChampionHitFlagTimer(int64 time) { this->championHitFlagTimer = time; }

public virtual void dealDamageTo(Unit* target, float damage, DamageType type, DamageSource source) override;
    }
}

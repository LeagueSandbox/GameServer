using InibinSharp;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Champion : Unit
    {
        protected string type;
        protected List<Spell> spells = new List<Spell>();
        protected short skillPoints = 0;
        protected int skin;
        protected long respawnTimer = 0;
        protected Inventory inventory = new Inventory();
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

            Inibin inibin;
            if (!RAFManager.getInstance().readInibin("DATA/Characters/" + type + "/" + type + ".inibin", out inibin))
            {
                Logger.LogCoreError("couldn't find champion stats for " + type);
                return;
            }
            
            stats.setCurrentHealth(inibin.getFloatValue("Data", "BaseHP"));
            stats.setMaxHealth(inibin.getFloatValue("Data", "BaseHP"));
            stats.setCurrentMana(inibin.getFloatValue("Data", "BaseMP"));
            stats.setMaxMana(inibin.getFloatValue("Data", "BaseMP"));
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

            spells.Add(new Spell(this, inibin.getStringValue("Data", "Spell1"), 0));
            spells.Add(new Spell(this, inibin.getStringValue("Data", "Spell2"), 1));
            spells.Add(new Spell(this, inibin.getStringValue("Data", "Spell3"), 2));
            spells.Add(new Spell(this, inibin.getStringValue("Data", "Spell4"), 3));
            spells.Add(new Spell(this, "SummonerHeal", 4));
            spells.Add(new Spell(this, "SummonerFlash", 5));
            spells.Add(new Spell(this, "Recall", 13));

            setMelee(inibin.getBoolValue("DATA", "IsMelee"));
            setCollisionRadius(inibin.getIntValue("DATA", "PathfindingCollisionRadius"));

            Inibin autoAttack;
            if (!RAFManager.getInstance().readInibin("DATA/Characters/" + type + "/Spells/" + type + "BasicAttack.inibin", out autoAttack))
            {
                if (!RAFManager.getInstance().readInibin("DATA/Spells/" + type + "BasicAttack.inibin", out autoAttack))
                {
                    Logger.LogCoreError("Couldn't find champion auto-attack data for " + type);
                    return;
                }
            }

            autoAttackDelay = autoAttack.getFloatValue("SpellData", "castFrame") / 30.0f;
            autoAttackProjectileSpeed = autoAttack.getFloatValue("SpellData", "MissileSpeed");

            //Fuck LUA
            /* var scriptloc = "../../lua/champions/" + getType() + "/Passive.lua";
             Logger.LogCoreInfo("Loading " + scriptloc);
             try
             {
                 unitScript = LuaScript(true);//fix

                 unitScript.lua.set("me", this);

                 unitScript.loadScript(scriptloc);

                 unitScript.lua.set_function("dealMagicDamage", [this](Unit * target, float amount) { this.dealDamageTo(target, amount, DAMAGE_TYPE_MAGICAL, DAMAGE_SOURCE_SPELL); });
                 unitScript.lua.set_function("addBuff", [this](Buff b, Unit * target){
                     target.addBuff(new Buff(b));
                     return;
                 });

                 unitScript.lua.set_function("addParticleTarget", [this](const std::string&particle, Target* u) {
                     this.getMap().getGame().notifyParticleSpawn(this, u, particle);
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
                switch (Config.players[playerIndex].team.ToLower())
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
                //TODO : repair function
                case "blue":
                    x = Config.mapSpawns.blue[teamSize].getXForPlayer(0);
                    y = Config.mapSpawns.blue[teamSize].getYForPlayer(0);
                    break;
                case "purple":
                    x = Config.mapSpawns.purple[teamSize].getXForPlayer(0);
                    y = Config.mapSpawns.purple[teamSize].getYForPlayer(0);
                    break;
            }

            return new Tuple<float, float>(x, y);
        }

        public Spell castSpell(byte slot, float x, float y, Unit target, int futureProjNetId, int spellNetId)
        {
            Spell s = null;
            foreach (Spell t in spells)
            {
                if (t.getSlot() == slot)
                {
                    s = t;
                }
            }

            if (s == null)
            {
                return null;
            }

            s.setSlot(slot);//temporary hack until we redo spells to be almost fully lua-based

            if ((s.getCost() * (1 - stats.getSpellCostReduction())) > stats.getCurrentMana() || s.getState() != SpellState.STATE_READY)
                return null;

            s.cast(x, y, target, futureProjNetId, spellNetId);
            stats.setCurrentMana(stats.getCurrentMana() - (s.getCost() * (1 - stats.getSpellCostReduction())));
            return s;
        }
        public Spell levelUpSpell(short slot)
        {
            if (slot >= spells.Count)
                return null;

            if (skillPoints == 0)
                return null;

            spells[slot].levelUp();
            --skillPoints;

            return spells[slot];
        }

        public override void update(long diff)
        {
            base.update(diff);

            if (!isDead() && moveOrder == MoveOrder.MOVE_ORDER_ATTACKMOVE && targetUnit != null)
            {
                Dictionary<int, GameObject> objects = map.getObjects();
                float distanceToTarget = 9000000.0f;
                Unit nextTarget = null;
                float range = Math.Max(stats.getRange(), DETECT_RANGE);

                foreach (var it in objects)
                {
                    var u = it.Value as Unit;

                    if (u == null || u.isDead() || u.getTeam() == getTeam() || distanceWith(u) > range)
                        continue;

                    if (distanceWith(u) < distanceToTarget)
                    {
                        distanceToTarget = distanceWith(u);
                        nextTarget = u;
                    }
                }

                if (nextTarget != null)
                {
                    setTargetUnit(nextTarget);
                    PacketNotifier.notifySetTarget(this, nextTarget);
                }
            }

            if (!stats.isGeneratingGold() && map.getGameTime() >= map.getFirstGoldTime())
            {
                stats.setGeneratingGold(true);
                Logger.LogCoreInfo("Generating Gold!");
            }

            if (respawnTimer > 0)
            {
                respawnTimer -= diff;
                if (respawnTimer <= 0)
                {
                    var spawnPos = getRespawnPosition();
                    float respawnX = spawnPos.Item1;
                    float respawnY = spawnPos.Item2;
                    setPosition(respawnX, respawnY);
                    PacketNotifier.notifyChampionRespawn(this);
                    getStats().setCurrentHealth(getStats().getMaxHealth());
                    getStats().setCurrentMana(getStats().getMaxMana());
                    deathFlag = false;
                }
            }

            bool levelup = false;

            while (getStats().getLevel() < map.getExperienceToLevelUp().Count && getStats().getExperience() >= map.getExperienceToLevelUp()[getStats().getLevel()])
            {
                levelUp();
                levelup = true;
            }

            if (levelup)
                PacketNotifier.notifyLevelUp(this);

            foreach (var s in spells)
                s.update(diff);

            if (championHitFlagTimer > 0)
            {
                championHitFlagTimer -= diff;
                if (championHitFlagTimer <= 0)
                    championHitFlagTimer = 0;
            }
        }

        public void setSkillPoints(int _skillPoints)
        {
            skillPoints = (short)_skillPoints;
        }

        public void setSkin(int skin)
        {
            this.skin = skin;
        }
        public int getChampionHash()
        {
            var szSkin = "";

            if (skin < 10)
                szSkin = "0" + skin;
            else
                szSkin = skin.ToString();

            int hash = 0;
            var gobj = "[Character]";
            for (var i = 0; i < gobj.Length; i++)
            {
                hash = Char.ToLower(gobj[i]) + (0x1003F * hash);
            }
            for (var i = 0; i < type.Length; i++)
            {
                hash = Char.ToLower(type[i]) + (0x1003F * hash);
            }
            for (var i = 0; i < szSkin.Length; i++)
            {
                hash = Char.ToLower(szSkin[i]) + (0x1003F * hash);
            }
            return hash;
        }

        public override bool isInDistress()
        {
            return distressCause != null;
        }

        public short getSkillPoints()
        {
            return skillPoints;
        }
        public void levelUp()
        {
            Logger.LogCoreInfo("Champion " + getType() + "Levelup to " + getStats().getLevel() + 1);
            getStats().levelUp();
            ++skillPoints;
        }

        public Inventory getInventory()
        {
            return inventory;
        }

        public override void die(Unit killer)
        {
            respawnTimer = 5000 + getStats().getLevel() * 2500;
            map.stopTargeting(this);

            var cKiller = killer as Champion;

            if (cKiller == null && championHitFlagTimer > 0)
            {
                cKiller = map.getObjectById(playerHitId) as Champion;
                Logger.LogCoreInfo("Killed by turret, minion or monster, but still  give gold to the enemy.");
            }

            if (cKiller == null)
            {
                PacketNotifier.notifyChampionDie(this, killer, 0);
                return;
            }

            cKiller.setChampionGoldFromMinions(0);

            float gold = map.getGoldFor(this);
            Logger.LogCoreInfo("Before: getGoldFromChamp: " + gold + " Killer:" + cKiller.killDeathCounter + "Victim: " + killDeathCounter);

            if (cKiller.killDeathCounter < 0)
                cKiller.killDeathCounter = 0;

            if (cKiller.killDeathCounter >= 0)
                cKiller.killDeathCounter += 1;

            if (killDeathCounter > 0)
                killDeathCounter = 0;

            if (killDeathCounter <= 0)
                killDeathCounter -= 1;

            if (gold > 0)
            {
                PacketNotifier.notifyChampionDie(this, cKiller, 0);
                return;
            }

            if (map.getKillReduction() && !map.getFirstBlood())
            {
                gold -= gold * 0.25f;
                //CORE_INFO("Still some minutes for full gold reward on champion kills");
            }

            if (map.getFirstBlood())
            {
                gold += 100;
                map.setFirstBlood(false);
            }

            PacketNotifier.notifyChampionDie(this, cKiller, (int)gold);

            cKiller.getStats().setGold(cKiller.getStats().getGold() + gold);
            PacketNotifier.notifyAddGold(cKiller, this, gold);

            //CORE_INFO("After: getGoldFromChamp: %f Killer: %i Victim: %i", gold, cKiller.killDeathCounter,this.killDeathCounter);

            map.stopTargeting(this);
        }
        public long getRespawnTimer()
        {
            return respawnTimer;
        }

        public override void onCollision(GameObject collider)
        {
            if (collider == null)
            {
                //CORE_INFO("I bumped into a wall!");
            }
            else
            {
                //CORE_INFO("I bumped into someone else!");
            }
        }

        public float getChampionGoldFromMinions()
        {
            return championGoldFromMinions;
        }
        public void setChampionGoldFromMinions(float gold)
        {
            championGoldFromMinions = gold;
        }

        public void setChampionHitFlagTimer(long time)
        {
            championHitFlagTimer = time;
        }

        public override void dealDamageTo(Unit target, float damage, DamageType type, DamageSource source)
        {
            base.dealDamageTo(target, damage, type, source);

            var cTarget = target as Champion;
            if (cTarget == null)
                return;

            cTarget.setChampionHitFlagTimer(15 * 1000); //15 seconds timer, so when you get executed the last enemy champion who hit you gets the gold
            cTarget.playerHitId = id;
            //CORE_INFO("15 second execution timer on you. Do not get killed by a minion, turret or monster!");
        }
    }
}

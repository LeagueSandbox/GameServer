using InibinSharp;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Items;
using System;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.Content;
using NLua.Exceptions;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Champion : Unit
    {
        private RAFManager _rafManager = Program.ResolveDependency<RAFManager>();

        public Shop Shop { get; protected set; }
        public long RespawnTimer { get; private set; }
        public float ChampionGoldFromMinions { get; set; }
        public RuneCollection RuneList { get; set; }
        public List<Spell> Spells { get; private set; }

        private short _skillPoints;
        public int Skin { get; set; }
        private long _championHitFlagTimer;
        private uint _playerId;
        private uint _playerHitId;

        public Champion(string model, uint playerId, RuneCollection runeList, uint netId = 0)
            : base(model, new Stats(), 30, 0, 0, 1200, netId)
        {
            this._playerId = playerId;
            this.RuneList = runeList;

            Inventory = InventoryManager.CreateInventory(this);
            Shop = Shop.CreateShop(this);

            stats.Gold = 475.0f;
            stats.GoldPerSecond.BaseValue = _game.Map.GetGoldPerSecond();
            stats.SetGeneratingGold(false);

            Inibin inibin;
            if (!_rafManager.readInibin("DATA/Characters/" + Model + "/" + Model + ".inibin", out inibin))
            {
                _logger.LogCoreError("couldn't find champion stats for " + Model);
                return;
            }

            stats.HealthPoints.BaseValue = inibin.getFloatValue("Data", "BaseHP");
            stats.CurrentHealth = stats.HealthPoints.Total;
            stats.ManaPoints.BaseValue = inibin.getFloatValue("Data", "BaseMP");
            stats.CurrentMana = stats.ManaPoints.Total;
            stats.AttackDamage.BaseValue = inibin.getFloatValue("DATA", "BaseDamage");
            stats.Range.BaseValue = inibin.getFloatValue("DATA", "AttackRange");
            stats.MoveSpeed.BaseValue = inibin.getFloatValue("DATA", "MoveSpeed");
            stats.Armor.BaseValue = inibin.getFloatValue("DATA", "Armor");
            stats.MagicResist.BaseValue = inibin.getFloatValue("DATA", "SpellBlock");
            stats.HealthRegeneration.BaseValue = inibin.getFloatValue("DATA", "BaseStaticHPRegen");
            stats.ManaRegeneration.BaseValue = inibin.getFloatValue("DATA", "BaseStaticMPRegen");
            stats.AttackSpeedFlat = 0.625f/(1 + inibin.getFloatValue("DATA", "AttackDelayOffsetPercent"));
            stats.AttackSpeedMultiplier.BaseValue = 1.0f;

            stats.HealthPerLevel = inibin.getFloatValue("DATA", "HPPerLevel");
            stats.ManaPerLevel = inibin.getFloatValue("DATA", "MPPerLevel");
            stats.AdPerLevel = inibin.getFloatValue("DATA", "DamagePerLevel");
            stats.ArmorPerLevel = inibin.getFloatValue("DATA", "ArmorPerLevel");
            stats.MagicResistPerLevel = inibin.getFloatValue("DATA", "SpellBlockPerLevel");
            stats.HealthRegenerationPerLevel = inibin.getFloatValue("DATA", "HPRegenPerLevel");
            stats.ManaRegenerationPerLevel = inibin.getFloatValue("DATA", "MPRegenPerLevel");
            stats.GrowthAttackSpeed = inibin.getFloatValue("DATA", "AttackSpeedPerLevel");

            Spells = new List<Spell>();
            Spells.Add(new Spell(this, inibin.getStringValue("Data", "Spell1"), 0));
            Spells.Add(new Spell(this, inibin.getStringValue("Data", "Spell2"), 1));
            Spells.Add(new Spell(this, inibin.getStringValue("Data", "Spell3"), 2));
            Spells.Add(new Spell(this, inibin.getStringValue("Data", "Spell4"), 3));
            Spells.Add(new Spell(this, "SummonerHeal", 4));
            Spells.Add(new Spell(this, "SummonerFlash", 5));
            Spells.Add(new Spell(this, "Recall", 13));

            IsMelee = inibin.getBoolValue("DATA", "IsMelee");
            CollisionRadius = inibin.getIntValue("DATA", "PathfindingCollisionRadius");

            var autoAttack = _rafManager.GetAutoAttackData(Model);
            if (autoAttack != null)
            {
                AutoAttackDelay = autoAttack.getFloatValue("SpellData", "castFrame") / 30.0f;
                AutoAttackProjectileSpeed = autoAttack.getFloatValue("SpellData", "MissileSpeed");
            }

            LoadLua();
        }

        public override void LoadLua()
        {
            base.LoadLua();
            var scriptloc = _game.Config.ContentManager.GetSpellScriptPath(Model, "Passive");
            _scriptEngine.SetGlobalVariable("me", this);
            _scriptEngine.Execute(@"
                function getOwner()
                    return me
                end");
            _scriptEngine.Execute(@"
                function onSpellCast(slot, target)
                end");
            _scriptEngine.Load(scriptloc);
        }

        private string GetPlayerIndex()
        {
            return string.Format("player{0}", _playerId);
        }

        public int getTeamSize()
        {
            int blueTeamSize = 0;
            int purpTeamSize = 0;

            foreach (var player in _game.Config.Players.Values)
            {
                switch (player.Team.ToLower())
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

            var playerIndex = GetPlayerIndex();
            if (_game.Config.Players.ContainsKey(playerIndex))
            {
                switch (_game.Config.Players[playerIndex].Team.ToLower())
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
            var config = _game.Config;
            var mapId = config.GameConfig.Map;
            var playerIndex = GetPlayerIndex();
            var playerTeam = "";
            var teamSize = getTeamSize();

            if (teamSize > 6) //???
                teamSize = 6;

            if (config.Players.ContainsKey(playerIndex))
            {
                var p = config.Players[playerIndex];
                playerTeam = p.Team;
            }
            var x = 0;
            var y = 0;
            switch (playerTeam.ToLower())
            {
                //TODO : repair function
                case "blue":
                    x = config.MapSpawns.Blue[teamSize - 1].GetXForPlayer(0);
                    y = config.MapSpawns.Blue[teamSize - 1].GetYForPlayer(0);
                    break;
                case "purple":
                    x = config.MapSpawns.Purple[teamSize - 1].GetXForPlayer(0);
                    y = config.MapSpawns.Purple[teamSize - 1].GetYForPlayer(0);
                    break;
            }

            return new Tuple<float, float>(x, y);
        }

        public Spell castSpell(byte slot, float x, float y, Unit target, uint futureProjNetId, uint spellNetId)
        {
            try
            {
                _scriptEngine.SetGlobalVariable("slot", slot);
                _scriptEngine.SetGlobalVariable("target", target);
                _scriptEngine.Execute("onSpellCast(slot, target)");
            }
            catch (LuaScriptException e)
            {
                _logger.LogCoreError("LUA ERROR : " + e.Message);
            }
            Spell s = null;
            foreach (Spell t in Spells)
            {
                if (t.Slot == slot)
                {
                    s = t;
                }
            }

            if (s == null)
            {
                return null;
            }

            s.Slot = slot;//temporary hack until we redo spells to be almost fully lua-based

            if ((s.getCost() * (1 - stats.getSpellCostReduction())) > stats.CurrentMana || s.getState() != SpellState.STATE_READY)
                return null;

            s.cast(x, y, target, futureProjNetId, spellNetId);
            stats.CurrentMana = stats.CurrentMana - (s.getCost() * (1 - stats.getSpellCostReduction()));
            return s;
        }
        public Spell levelUpSpell(short slot)
        {
            if (slot >= Spells.Count)
                return null;

            if (_skillPoints == 0)
                return null;

            Spells[slot].levelUp();
            _skillPoints--;

            return Spells[slot];
        }

        public override void update(long diff)
        {
            base.update(diff);

            if (!IsDead && MoveOrder == MoveOrder.MOVE_ORDER_ATTACKMOVE && TargetUnit != null)
            {
                Dictionary<uint, GameObject> objects = _game.Map.GetObjects();
                float distanceToTarget = 9000000.0f;
                Unit nextTarget = null;
                float range = Math.Max(stats.Range.Total, DETECT_RANGE);

                foreach (var it in objects)
                {
                    var u = it.Value as Unit;

                    if (u == null || u.IsDead || u.Team == this.Team || GetDistanceTo(u) > range)
                        continue;

                    if (GetDistanceTo(u) < distanceToTarget)
                    {
                        distanceToTarget = GetDistanceTo(u);
                        nextTarget = u;
                    }
                }

                if (nextTarget != null)
                {
                    TargetUnit = nextTarget;
                    _game.PacketNotifier.notifySetTarget(this, nextTarget);
                }
            }

            if (!stats.IsGeneratingGold() && _game.Map.GameTime >= _game.Map.FirstGoldTime)
            {
                stats.SetGeneratingGold(true);
                _logger.LogCoreInfo("Generating Gold!");
            }

            if (RespawnTimer > 0)
            {
                RespawnTimer -= diff;
                if (RespawnTimer <= 0)
                {
                    var spawnPos = getRespawnPosition();
                    float respawnX = spawnPos.Item1;
                    float respawnY = spawnPos.Item2;
                    setPosition(respawnX, respawnY);
                    _game.PacketNotifier.notifyChampionRespawn(this);
                    GetStats().CurrentHealth = GetStats().HealthPoints.Total;
                    GetStats().CurrentMana = GetStats().HealthPoints.Total;
                    IsDead = false;
                }
            }

            var isLevelup = LevelUp();
            if (isLevelup)
            {
                _game.PacketNotifier.notifyLevelUp(this);
                _game.PacketNotifier.notifyUpdatedStats(this, false);
            }

            foreach (var s in Spells)
                s.update(diff);

            if (_championHitFlagTimer > 0)
            {
                _championHitFlagTimer -= diff;
                if (_championHitFlagTimer <= 0)
                {
                    _championHitFlagTimer = 0;
                }
            }
        }

        public void setSkillPoints(int _skillPoints)
        {
            _skillPoints = (short)_skillPoints;
        }

        public int getChampionHash()
        {
            var szSkin = "";

            if (Skin < 10)
                szSkin = "0" + Skin;
            else
                szSkin = Skin.ToString();

            int hash = 0;
            var gobj = "[Character]";
            for (var i = 0; i < gobj.Length; i++)
            {
                hash = Char.ToLower(gobj[i]) + (0x1003F * hash);
            }
            for (var i = 0; i < Model.Length; i++)
            {
                hash = Char.ToLower(Model[i]) + (0x1003F * hash);
            }
            for (var i = 0; i < szSkin.Length; i++)
            {
                hash = Char.ToLower(szSkin[i]) + (0x1003F * hash);
            }
            return hash;
        }

        public override bool isInDistress()
        {
            return DistressCause != null;
        }

        public short getSkillPoints()
        {
            return _skillPoints;
        }

        public bool LevelUp()
        {
            var stats = GetStats();
            var expMap = _game.Map.ExpToLevelUp;
            if (stats.GetLevel() >= expMap.Count)
                return false;
            if (stats.Experience < expMap[stats.Level])
                return false;

            while (stats.Level < expMap.Count && stats.Experience >= expMap[stats.Level])
            {
                GetStats().LevelUp();
                _logger.LogCoreInfo("Champion " + Model + " leveled up to " + stats.Level);
                _skillPoints++;
            }
            return true;
        }

        public InventoryManager getInventory()
        {
            return Inventory;
        }

        public override void die(Unit killer)
        {
            RespawnTimer = 5000 + GetStats().Level * 2500;
            _game.Map.StopTargeting(this);

            var cKiller = killer as Champion;

            if (cKiller == null && _championHitFlagTimer > 0)
            {
                cKiller = _game.Map.GetObjectById(_playerHitId) as Champion;
                _logger.LogCoreInfo("Killed by turret, minion or monster, but still  give gold to the enemy.");
            }

            if (cKiller == null)
            {
                _game.PacketNotifier.notifyChampionDie(this, killer, 0);
                return;
            }

            cKiller.ChampionGoldFromMinions = 0;

            float gold = _game.Map.GetGoldFor(this);
            _logger.LogCoreInfo(
                "Before: getGoldFromChamp: {0} Killer: {1} Victim {2}",
                gold,
                cKiller.KillDeathCounter,
                KillDeathCounter
            );

            if (cKiller.KillDeathCounter < 0)
                cKiller.KillDeathCounter = 0;

            if (cKiller.KillDeathCounter >= 0)
                cKiller.KillDeathCounter += 1;

            if (KillDeathCounter > 0)
                KillDeathCounter = 0;

            if (KillDeathCounter <= 0)
                KillDeathCounter -= 1;

            if (gold > 0)
            {
                _game.PacketNotifier.notifyChampionDie(this, cKiller, 0);
                return;
            }

            if (_game.Map.IsKillGoldRewardReductionActive && _game.Map.HasFirstBloodHappened)
            {
                gold -= gold * 0.25f;
                //CORE_INFO("Still some minutes for full gold reward on champion kills");
            }

            if (!_game.Map.HasFirstBloodHappened)
            {
                gold += 100;
                _game.Map.HasFirstBloodHappened = true;
            }

            _game.PacketNotifier.notifyChampionDie(this, cKiller, (int)gold);

            cKiller.GetStats().Gold = cKiller.GetStats().Gold + gold;
            _game.PacketNotifier.notifyAddGold(cKiller, this, gold);

            //CORE_INFO("After: getGoldFromChamp: %f Killer: %i Victim: %i", gold, cKiller.killDeathCounter,this.killDeathCounter);

            _game.Map.StopTargeting(this);
        }

        public override void onCollision(GameObject collider)
        {
            base.onCollision(collider);
            if (collider == null)
            {
                //CORE_INFO("I bumped into a wall!");
            }
            else
            {
                //CORE_INFO("I bumped into someone else!");
            }
        }

        public override void dealDamageTo(Unit target, float damage, DamageType type, DamageSource source, bool isCrit)
        {
            base.dealDamageTo(target, damage, type, source, isCrit);

            var cTarget = target as Champion;
            if (cTarget == null)
                return;

            cTarget._championHitFlagTimer = 15 * 1000; //15 seconds timer, so when you get executed the last enemy champion who hit you gets the gold
            cTarget._playerHitId = NetId;
            //CORE_INFO("15 second execution timer on you. Do not get killed by a minion, turret or monster!");
        }
    }
}

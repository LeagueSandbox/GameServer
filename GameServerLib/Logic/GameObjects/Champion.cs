using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Items;
using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using Newtonsoft.Json.Linq;
using NLua.Exceptions;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Champion : Unit
    {
        public Shop Shop { get; protected set; }
        public float RespawnTimer { get; private set; }
        public float ChampionGoldFromMinions { get; set; }
        public RuneCollection RuneList { get; set; }
        public Dictionary<short, Spell> Spells { get; private set; }
        public List<string> ExtraSpells { get; private set; }

        private short _skillPoints;
        public int Skin { get; set; }
        private float _championHitFlagTimer;
        /// <summary>
        /// Player number ordered by the config file.
        /// </summary>
        private uint _playerId;
        /// <summary>
        /// Player number in the team ordered by the config file.
        /// Used in nowhere but to set spawnpoint at the game start.
        /// </summary>
        private uint _playerTeamSpecialId;
        private uint _playerHitId;

        public Champion(string model,
                        uint playerId,
                        uint playerTeamSpecialId,
                        RuneCollection runeList,
                        ClientInfo clientInfo,
                        uint netId = 0)
            : base(model, new Stats(), 30, 0, 0, 1200, netId)
        {
            _playerId = playerId;
            _playerTeamSpecialId = playerTeamSpecialId;
            RuneList = runeList;

            Inventory = InventoryManager.CreateInventory(this);
            Shop = Shop.CreateShop(this);

            stats.Gold = 475.0f;
            stats.GoldPerSecond.BaseValue = _game.Map.GetGoldPerSecond();
            stats.SetGeneratingGold(false);

            JObject data;
            if (!_rafManager.ReadUnitStats(model, out data))
            {
                _logger.LogCoreError("Couldn't find champion stats for " + Model);
                return;
            }

            stats.HealthPoints.BaseValue = _rafManager.GetFloatValue(data, "Data", "BaseHP");
            stats.CurrentHealth = stats.HealthPoints.Total;
            stats.ManaPoints.BaseValue = _rafManager.GetFloatValue(data, "Data", "BaseMP");
            stats.CurrentMana = stats.ManaPoints.Total;
            stats.AttackDamage.BaseValue = _rafManager.GetFloatValue(data, "Data", "BaseDamage");
            stats.Range.BaseValue = _rafManager.GetFloatValue(data, "Data", "AttackRange");
            stats.MoveSpeed.BaseValue = _rafManager.GetFloatValue(data, "Data", "MoveSpeed");
            stats.Armor.BaseValue = _rafManager.GetFloatValue(data, "Data", "Armor");
            stats.MagicResist.BaseValue = _rafManager.GetFloatValue(data, "Data", "SpellBlock");
            stats.HealthRegeneration.BaseValue = _rafManager.GetFloatValue(data, "Data", "BaseStaticHPRegen");
            stats.ManaRegeneration.BaseValue = _rafManager.GetFloatValue(data, "Data", "BaseStaticMPRegen");
            stats.AttackSpeedFlat = 0.625f / (1 + _rafManager.GetFloatValue(data, "Data", "AttackDelayOffsetPercent"));
            stats.AttackSpeedMultiplier.BaseValue = 1.0f;

            stats.HealthPerLevel = _rafManager.GetFloatValue(data, "Data", "HPPerLevel");
            stats.ManaPerLevel = _rafManager.GetFloatValue(data, "Data", "MPPerLevel");
            stats.AdPerLevel = _rafManager.GetFloatValue(data, "Data", "DamagePerLevel");
            stats.ArmorPerLevel = _rafManager.GetFloatValue(data, "Data", "ArmorPerLevel");
            stats.MagicResistPerLevel = _rafManager.GetFloatValue(data, "Data", "SpellBlockPerLevel");
            stats.HealthRegenerationPerLevel = _rafManager.GetFloatValue(data, "Data", "HPRegenPerLevel");
            stats.ManaRegenerationPerLevel = _rafManager.GetFloatValue(data, "Data", "MPRegenPerLevel");
            stats.GrowthAttackSpeed = _rafManager.GetFloatValue(data, "Data", "AttackSpeedPerLevel");

            Spells = new Dictionary<short, Spell>
            {
                { 0, new Spell(this, _rafManager.GetStringValue(data, "Data", "Spell1"), 0) },
                { 1, new Spell(this, _rafManager.GetStringValue(data, "Data", "Spell2"), 1) },
                { 2, new Spell(this, _rafManager.GetStringValue(data, "Data", "Spell3"), 2) },
                { 3, new Spell(this, _rafManager.GetStringValue(data, "Data", "Spell4"), 3) },
                { 4, new Spell(this, clientInfo.SummonerSkills[0], 4) },
                { 5, new Spell(this, clientInfo.SummonerSkills[1], 5) },
                { 13, new Spell(this, "Recall", 13) }
            };

            Spells[4].levelUp();
            Spells[5].levelUp();

            ExtraSpells = new List<string>();

            for (var i = 1; true; i++)
            {
                if (string.IsNullOrEmpty(_rafManager.GetStringValue(data, "Data", "ExtraSpell" + i)))
                {
                    break;
                }

                ExtraSpells.Add(_rafManager.GetStringValue(data, "Data", "ExtraSpell" + i));
            }

            IsMelee = _rafManager.GetBoolValue(data, "Data", "IsMelee");
            CollisionRadius = _rafManager.GetIntValue(data, "Data", "PathfindingCollisionRadius");

            JObject autoAttack;
            if (_rafManager.ReadAutoAttackData(model, out autoAttack))
            {
                AutoAttackDelay = _rafManager.GetFloatValue(autoAttack, "SpellData", "CastFrame") / 30.0f;
                AutoAttackProjectileSpeed = _rafManager.GetFloatValue(autoAttack, "SpellData", "MissileSpeed");
            }

            LoadLua();
            foreach (var spell in Spells.Values)
            {
                spell.LoadExtraSpells(this);
            }
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
                function onSpellCast(x, y, slot, target)
                end");
            _scriptEngine.Load(scriptloc);
        }

        private string GetPlayerIndex()
        {
            return $"player{_playerId}";
        }

        public int GetTeamSize()
        {
            var blueTeamSize = 0;
            var purpTeamSize = 0;

            foreach (var player in _game.Config.Players.Values)
            {
                if (player.Team.ToLower() == "blue")
                {
                    blueTeamSize++;
                }
                else
                {
                    purpTeamSize++;
                }
            }

            var playerIndex = GetPlayerIndex();
            if (_game.Config.Players.ContainsKey(playerIndex))
            {
                switch (_game.Config.Players[playerIndex].Team.ToLower())
                {
                    case "blue":
                        return blueTeamSize;
                    default:
                        return purpTeamSize;
                }
            }

            return 0;
        }

        public Vector2 GetSpawnPosition()
        {
            var config = _game.Config;
            var playerIndex = GetPlayerIndex();
            var playerTeam = "";
            var teamSize = GetTeamSize();

            if (teamSize > 6) //???
                teamSize = 6;

            if (config.Players.ContainsKey(playerIndex))
            {
                var p = config.Players[playerIndex];
                playerTeam = p.Team;
            }

            var spawnsByTeam = new Dictionary<TeamId, Dictionary<int, PlayerSpawns>>
            {
                {TeamId.TEAM_BLUE, config.MapSpawns.Blue},
                {TeamId.TEAM_PURPLE, config.MapSpawns.Purple}
            };

            var spawns = spawnsByTeam[Team];
            return spawns[teamSize - 1].GetCoordsForPlayer((int)_playerTeamSpecialId);
        }

        public Vector2 GetRespawnPosition()
        {
            var config = _game.Config;
            var playerIndex = GetPlayerIndex();

            if (config.Players.ContainsKey(playerIndex))
            {
                var p = config.Players[playerIndex];
            }
            var coords = new Vector2
            {
                X = _game.Map.GetRespawnLocation(Team).X,
                Y = _game.Map.GetRespawnLocation(Team).Y
            };
            return new Vector2(coords.X, coords.Y);
        }

        public Spell castSpell(byte slot, float x, float y, Unit target, uint futureProjNetId, uint spellNetId)
        {
            Spell s = null;
            foreach (var t in Spells.Values)
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

            if ((s.getCost() * (1 - stats.getSpellCostReduction())) > stats.CurrentMana || s.state != SpellState.STATE_READY)
                return null;

            if (s.cast(x, y, target, futureProjNetId, spellNetId))
            {
                stats.CurrentMana = stats.CurrentMana - s.getCost() * (1 - stats.getSpellCostReduction());
                try
                {
                    _scriptEngine.RunFunction("onSpellCast", x, y, slot, target);
                }
                catch (LuaScriptException e)
                {
                    _logger.LogCoreError("LUA ERROR : " + e.Message);
                }
                return s;
            }
            return null;
        }

        public Spell levelUpSpell(short slot)
        {
            var _udyrModels = new List<string>
            {
                "Udyr",
                "UdyrPhoenix",
                "UdyrPhoenixUlt",
                "UdyrTiger",
                "UdyrTigerUlt",
                "UdyrTurtle",
                "UdyrTurtleUlt",
                "UdyrUlt"
            };

            var _specificModels = new List<string>
            {
                "Elise",
                "EliseSpider",
                "Karma",
                "Nidalee",
                "NidaleeCougar"
            };
            if (slot >= Spells.Count)
                return null;

            if (_skillPoints == 0)
                return null;

            if ((!_udyrModels.Contains(Model) && slot != 3) || _udyrModels.Contains(Model))
            {
                if (Spells[slot].Level >= Math.Ceiling((decimal)GetStats().Level))
                {
                    return null;
                }
            }
            else
            {
                if ((!_specificModels.Contains(Model) && GetStats().Level < 1 + Spells[slot].Level * 5) ||
                    (_specificModels.Contains(Model) && GetStats().Level < 1 + (Spells[slot].Level - 1) * 5))
                {
                    return null;
                }
            }

            Spells[slot].levelUp();
            _skillPoints--;

            return Spells[slot];
        }

        public override void update(float diff)
        {
            base.update(diff);

            if (!IsDead && MoveOrder == MoveOrder.MOVE_ORDER_ATTACKMOVE && TargetUnit != null)
            {
                var objects = _game.Map.GetObjects();
                var distanceToTarget = 9000000.0f;
                Unit nextTarget = null;
                var range = Math.Max(stats.Range.Total, DETECT_RANGE);

                foreach (var it in objects)
                {
                    var u = it.Value as Unit;

                    if (u == null || u.IsDead || u.Team == Team || GetDistanceTo(u) > range)
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
                    _game.PacketNotifier.NotifySetTarget(this, nextTarget);
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
                    Respawn();
                }
            }

            var isLevelup = LevelUp();
            if (isLevelup)
            {
                _game.PacketNotifier.NotifyLevelUp(this);
                _game.PacketNotifier.NotifyUpdatedStats(this, false);
            }

            foreach (var s in Spells.Values)
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

        public void Respawn()
        {
            var spawnPos = GetRespawnPosition();
            setPosition(spawnPos.X, spawnPos.Y);
            _game.PacketNotifier.NotifyChampionRespawn(this);
            GetStats().CurrentHealth = GetStats().HealthPoints.Total;
            GetStats().CurrentMana = GetStats().HealthPoints.Total;
            IsDead = false;
            RespawnTimer = -1;
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

            _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.Death, this, killer);

            var cKiller = killer as Champion;

            if (cKiller == null && _championHitFlagTimer > 0)
            {
                cKiller = _game.Map.GetObjectById(_playerHitId) as Champion;
                _logger.LogCoreInfo("Killed by turret, minion or monster, but still  give gold to the enemy.");
            }

            if (cKiller == null)
            {
                _game.PacketNotifier.NotifyChampionDie(this, killer, 0);
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
                _game.PacketNotifier.NotifyChampionDie(this, cKiller, 0);
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

            _game.PacketNotifier.NotifyChampionDie(this, cKiller, (int)gold);

            cKiller.GetStats().Gold = cKiller.GetStats().Gold + gold;
            _game.PacketNotifier.NotifyAddGold(cKiller, this, gold);

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

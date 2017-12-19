using LeagueSandbox.GameServer.Logic.Items;
using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using Newtonsoft.Json.Linq;
using LeagueSandbox.GameServer.Logic.Scripting;
using LeagueSandbox.GameServer.Logic.API;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Champion : ObjAIBase
    {
        public Shop Shop { get; protected set; }
        public float RespawnTimer { get; private set; }
        public float ChampionGoldFromMinions { get; set; }
        public RuneCollection RuneList { get; set; }
        public Dictionary<short, Spell> Spells { get; private set; } = new Dictionary<short, Spell>();

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

            Stats.Gold = 475.0f;
            Stats.GoldPerSecond.BaseValue = _game.Map.MapGameScript.GoldPerSecond;
            Stats.SetGeneratingGold(false);

            //TODO: automaticaly rise spell levels with CharData.SpellLevelsUp
            for(short i = 0; i<CharData.SpellNames.Length;i++)
            {
                if(CharData.SpellNames[i] != "")
                {
                    Spells[i] = new Spell(this, CharData.SpellNames[i], (byte)(i));
                }
            }
            Spells[4] = new Spell(this, clientInfo.SummonerSkills[0], 4);
            Spells[5] = new Spell(this, clientInfo.SummonerSkills[1], 5);
            Spells[13] = new Spell(this, "Recall", 13);

            for(short i = 0; i<CharData.Passives.Length; i++)
            {
                if (CharData.Passives[i].PassiveLuaName != "")
                {
                    Spells[(byte)(i + 14)] = new Spell(this, CharData.Passives[i].PassiveLuaName, (byte)(i + 14));
                }
            }

            for (short i = 0; i < CharData.ExtraSpells.Length; i++)
            {
                if (CharData.ExtraSpells[i] != "")
                {
                    var spell = new Spell(this, CharData.ExtraSpells[i], (byte)(i + 45));
                    Spells[(byte)(i + 45)] = spell;
                    spell.levelUp();
                }
            }
            Spells[4].levelUp();
            Spells[5].levelUp();
        }
        private string GetPlayerIndex()
        {
            return $"player{_playerId}";
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.ObjectManager.AddChampion(this);
            _game.PacketNotifier.NotifyChampionSpawned(this, Team);
        }

        public override void OnRemoved()
        {
            base.OnRemoved();
            _game.ObjectManager.RemoveChampion(this);
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

        public void AddStatModifier(ChampionStatModifier statModifier)
        {
            Stats.AddModifier(statModifier);
        }

        public void UpdateStatModifier(ChampionStatModifier statModifier)
        {
            Stats.UpdateModifier(statModifier);
        }

        public void RemoveStatModifier(ChampionStatModifier statModifier)
        {
            Stats.RemoveModifier(statModifier);
        }
        public bool CanMove()
        {
            return !this.HasCrowdControl(CrowdControlType.Stun) &&
                !this.IsDashing &&
                !this.IsCastingSpell &&
                !this.IsDead &&
                !this.HasCrowdControl(CrowdControlType.Root);
        }
        public bool CanCast()
        {
            return !this.HasCrowdControl(CrowdControlType.Stun) &&
                !this.HasCrowdControl(CrowdControlType.Silence);
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
                X = _game.Map.MapGameScript.GetRespawnLocation(Team).X,
                Y = _game.Map.MapGameScript.GetRespawnLocation(Team).Y
            };
            return new Vector2(coords.X, coords.Y);
        }

        public Spell GetSpell(byte slot)
        {
            return Spells[slot];
        }

        public Spell GetSpellByName(string name)
        {
            foreach(var s in Spells.Values)
            {
                if (s == null)
                    continue;
                if (s.SpellName == name)
                    return s;
            }
            return null;
        }

        public Spell LevelUpSpell(short slot)
        {
            if (_skillPoints == 0)
                return null;

            var s = GetSpell((byte) slot);

            if (s == null)
                return null;

            s.levelUp();
            _skillPoints--;

            return s;
        }

        public override void update(float diff)
        {
            base.update(diff);

            if (!IsDead && MoveOrder == MoveOrder.MOVE_ORDER_ATTACKMOVE && TargetUnit != null)
            {
                var objects = _game.ObjectManager.GetObjects();
                var distanceToTarget = 9000000.0f;
                Unit nextTarget = null;
                var range = Math.Max(Stats.Range.Total, DETECT_RANGE);

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

            if (!Stats.IsGeneratingGold() && _game.GameTime >= _game.Map.MapGameScript.FirstGoldTime)
            {
                Stats.SetGeneratingGold(true);
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

	    public void Recall(Unit owner)
        {
            var spawnPos = GetRespawnPosition();
            _game.PacketNotifier.NotifyTeleport(owner, spawnPos.X, spawnPos.Y);
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
            var expMap = _game.Map.MapGameScript.ExpToLevelUp;
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
            _game.ObjectManager.StopTargeting(this);

            _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.Death, this, killer);

            var cKiller = killer as Champion;

            if (cKiller == null && _championHitFlagTimer > 0)
            {
                cKiller = _game.ObjectManager.GetObjectById(_playerHitId) as Champion;
                _logger.LogCoreInfo("Killed by turret, minion or monster, but still  give gold to the enemy.");
            }

            if (cKiller == null)
            {
                _game.PacketNotifier.NotifyChampionDie(this, killer, 0);
                return;
            }

            cKiller.ChampionGoldFromMinions = 0;

            float gold = _game.Map.MapGameScript.GetGoldFor(this);
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

            if (_game.Map.MapGameScript.IsKillGoldRewardReductionActive
                && _game.Map.MapGameScript.HasFirstBloodHappened)
            {
                gold -= gold * 0.25f;
                //CORE_INFO("Still some minutes for full gold reward on champion kills");
            }

            if (!_game.Map.MapGameScript.HasFirstBloodHappened)
            {
                gold += 100;
                _game.Map.MapGameScript.HasFirstBloodHappened = true;
            }

            _game.PacketNotifier.NotifyChampionDie(this, cKiller, (int)gold);

            cKiller.GetStats().Gold = cKiller.GetStats().Gold + gold;
            _game.PacketNotifier.NotifyAddGold(cKiller, this, gold);

            //CORE_INFO("After: getGoldFromChamp: %f Killer: %i Victim: %i", gold, cKiller.killDeathCounter,this.killDeathCounter);

            _game.ObjectManager.StopTargeting(this);
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

        public override void DealDamageTo(Unit target, float damage, DamageType type, DamageSource source, bool isCrit)
        {
            base.DealDamageTo(target, damage, type, source, isCrit);

            var cTarget = target as Champion;
            if (cTarget == null)
                return;

            cTarget._championHitFlagTimer = 15 * 1000; //15 seconds timer, so when you get executed the last enemy champion who hit you gets the gold
            cTarget._playerHitId = NetId;
            //CORE_INFO("15 second execution timer on you. Do not get killed by a minion, turret or monster!");
        }
    }
}

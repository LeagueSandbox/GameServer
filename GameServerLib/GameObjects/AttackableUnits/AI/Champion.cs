using System.Collections.Generic;
using System.Numerics;
using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.NetInfo;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Inventory;
using LeagueSandbox.GameServer.API;
using LeaguePackets.Game.Events;
using System;
using GameServerLib.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class Champion : ObjAiBase, IChampion
    {
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
        private List<IToolTipData> _tipsChanged;
        public IShop Shop { get; protected set; }
        public float RespawnTimer { get; private set; }
        public int DeathSpree { get; set; } = 0;
        public int KillSpree { get; set; } = 0;
        public float GoldFromMinions { get; set; }
        public IRuneCollection RuneList { get; }
        public ITalentInventory TalentInventory { get; set; }
        public IChampionStats ChampStats { get; private set; } = new ChampionStats();

        public byte SkillPoints { get; set; }

        public Champion(Game game,
                        string model,
                        uint playerId,
                        uint playerTeamSpecialId,
                        IRuneCollection runeList,
                        ITalentInventory talentInventory,
                        ClientInfo clientInfo,
                        uint netId = 0,
                        TeamId team = TeamId.TEAM_BLUE)
            : base(game, model, new Stats.Stats(), 30, new Vector2(), 1200, clientInfo.SkinNo, netId, team)
        {
            _playerId = playerId;
            _playerTeamSpecialId = playerTeamSpecialId;
            RuneList = runeList;

            Inventory = InventoryManager.CreateInventory(game.PacketNotifier);
            TalentInventory = talentInventory;
            Shop = GameServer.Inventory.Shop.CreateShop(this, game);

            Stats.Gold = _game.Map.MapScript.MapScriptMetadata.AIVars.StartingGold;
            Stats.GoldPerGoldTick.BaseValue = _game.Map.MapScript.MapScriptMetadata.BaseGoldPerGoldTick;
            Stats.IsGeneratingGold = false;

            //TODO: automaticaly rise spell levels with CharData.SpellLevelsUp

            Spells[(int)SpellSlotType.SummonerSpellSlots] = new Spell.Spell(game, this, clientInfo.SummonerSkills[0], (int)SpellSlotType.SummonerSpellSlots);
            Spells[(int)SpellSlotType.SummonerSpellSlots].LevelUp();
            Spells[(int)SpellSlotType.SummonerSpellSlots + 1] = new Spell.Spell(game, this, clientInfo.SummonerSkills[1], (int)SpellSlotType.SummonerSpellSlots + 1);
            Spells[(int)SpellSlotType.SummonerSpellSlots + 1].LevelUp();

            Spells[(int)SpellSlotType.BluePillSlot] = new Spell.Spell(game, this,
                _game.ItemManager.GetItemType(_game.Map.MapScript.MapScriptMetadata.RecallSpellItemId).SpellName, (int)SpellSlotType.BluePillSlot);
            Stats.SetSpellEnabled((byte)SpellSlotType.BluePillSlot, true);

            Replication = new ReplicationHero(this);

            _tipsChanged = new List<IToolTipData>();

            if (clientInfo.PlayerId == -1)
            {
                IsBot = true;
            }
        }

        public void AddGold(IAttackableUnit source, float gold, bool notify = true)
        {
            Stats.Gold += gold;
            if (notify)
            {
                _game.PacketNotifier.NotifyUnitAddGold(this, source, gold);
            }
        }

        private string GetPlayerIndex()
        {
            return $"player{_playerId}";
        }

        public override void OnAdded()
        {
            _game.ObjectManager.AddChampion(this);
            base.OnAdded();
            TalentInventory.Initialize(this);
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
                if (player.Team.ToLower().Equals("blue"))
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
        public uint GetPlayerId()
        {
            return _playerId;
        }

        public Vector2 GetSpawnPosition(int index)
        {
            var teamSize = GetTeamSize();

            if (_game.Map.PlayerSpawnPoints[Team].ContainsKey(teamSize))
            {
                return _game.Map.PlayerSpawnPoints[Team][teamSize][index];
            }

            if (_game.Map.PlayerSpawnPoints[Team].ContainsKey(1) && _game.Map.PlayerSpawnPoints[Team][1][1] != null)
            {
                return _game.Map.PlayerSpawnPoints[Team][1][1];
            }

            return _game.Map.MapScript.GetFountainPosition(Team);
        }

        public Vector2 GetRespawnPosition()
        {
            return _game.Map.MapScript.GetFountainPosition(Team);
        }

        public override ISpell LevelUpSpell(byte slot)
        {
            if (SkillPoints == 0)
            {
                return null;
            }

            SkillPoints--;

            return base.LevelUpSpell(slot);
        }

        public void AddToolTipChange(IToolTipData data)
        {
            if (!_tipsChanged.Contains(data))
            {
                _tipsChanged.Add(data);
            }
        }

        public void ClearToolTipsChanged()
        {
            _tipsChanged.Clear();
        }

        float goldTimer;
        public override void Update(float diff)
        {
            base.Update(diff);

            if (Stats.IsGeneratingGold && Stats.GoldPerGoldTick.Total > 0)
            {
                goldTimer -= diff;

                if (goldTimer <= 0)
                {
                    Stats.Gold += Stats.GoldPerGoldTick.Total;
                    goldTimer = _game.Map.MapScript.MapScriptMetadata.GoldTickSpeed;
                }
            }
            else if (!Stats.IsGeneratingGold && _game.GameTime >= _game.Map.MapScript.MapScriptMetadata.AIVars.AmbientGoldDelay * 1000f)
            {
                Stats.IsGeneratingGold = true;
                Logger.Debug("Generating Gold!");
            }

            if (RespawnTimer > 0)
            {
                RespawnTimer -= diff;
                if (RespawnTimer <= 0)
                {
                    Respawn();
                }
            }

            if (_championHitFlagTimer > 0)
            {
                _championHitFlagTimer -= diff;
                if (_championHitFlagTimer <= 0)
                {
                    _championHitFlagTimer = 0;
                }
            }

            // TODO: Find out the best way to bulk send these for all champions (tool tip handler?).
            // League sends a single packet detailing every champion's tool tip changes.
            if (_tipsChanged.Count > 0)
            {
                _game.PacketNotifier.NotifyS2C_ToolTipVars(_tipsChanged);
                ClearToolTipsChanged();
            }
        }

        public void Respawn()
        {
            var spawnPos = GetRespawnPosition();
            SetPosition(spawnPos.X, spawnPos.Y);
            float parToRestore = 0;
            // TODO: Find a better way to do this, perhaps through scripts. Otherwise, make sure all types are accounted for.
            if (Stats.ParType == PrimaryAbilityResourceType.MANA || Stats.ParType == PrimaryAbilityResourceType.Energy || Stats.ParType == PrimaryAbilityResourceType.Wind)
            {
                parToRestore = Stats.ManaPoints.Total;
            }
            Stats.CurrentMana = parToRestore;
            _game.PacketNotifier.NotifyHeroReincarnateAlive(this, parToRestore);
            Stats.CurrentHealth = Stats.HealthPoints.Total;
            IsDead = false;
            RespawnTimer = -1;
            ApiEventManager.OnResurrect.Publish(this);
        }

        public bool OnDisconnect()
        {
            this.StopMovement();
            this.SetWaypoints(_game.Map.PathingHandler.GetPath(Position, GetRespawnPosition()));
            this.UpdateMoveOrder(OrderType.MoveTo, true);

            return true;
        }

        public void Recall()
        {
            var spawnPos = GetRespawnPosition();
            TeleportTo(spawnPos.X, spawnPos.Y);
        }

        public void AddExperience(float experience, bool notify = true)
        {
            if (experience > 0)
            {
                Stats.Experience += experience;

                if (notify)
                {
                    _game.PacketNotifier.NotifyUnitAddEXP(this, experience);
                }

                while (Stats.Experience >= _game.Map.MapData.ExpCurve[Stats.Level - 1] && LevelUp()) ;
            }
        }

        public override bool LevelUp(bool force = false)
        {
            var stats = Stats;
            var expMap = _game.Map.MapData.ExpCurve;

            if (force && stats.Level > 0)
            {
                Stats.Experience = expMap[Stats.Level - 1];
            }

            if (stats.Level < _game.Map.MapScript.MapScriptMetadata.MaxLevel && (stats.Level < 1 || (stats.Experience >= expMap[stats.Level - 1]))) //The - 1s is there because the XP files don't have level 1
            {
                Logger.Debug("Champion " + Model + " leveled up to " + stats.Level);
                if (stats.Level <= 18)
                {
                    SkillPoints++;
                }
                base.LevelUp(force);

                return true;
            }

            return false;
        }

        public void OnKill(IDeathData deathData)
        {
            ApiEventManager.OnKillUnit.Publish(deathData);

            if (deathData.Unit is IMinion)
            {
                ChampStats.MinionsKilled += 1;
                if (deathData.Unit.Team == TeamId.TEAM_NEUTRAL)
                {
                    ChampStats.NeutralMinionsKilled += 1;
                }

                var gold = deathData.Unit.Stats.GoldGivenOnDeath.Total;
                if (gold <= 0)
                {
                    return;
                }

                AddGold(deathData.Unit, gold);

                if (DeathSpree > 0)
                {
                    GoldFromMinions += gold;
                }

                if (GoldFromMinions >= 1000)
                {
                    GoldFromMinions -= 1000;
                    DeathSpree -= 1;
                }
            }
        }

        public override void Die(IDeathData data)
        {
            var mapScript = _game.Map.MapScript;
            var mapScriptMetaData = mapScript.MapScriptMetadata;
            var mapData = _game.Map.MapData;

            ApiEventManager.OnDeath.Publish(data);

            RespawnTimer = _game.Map.MapData.DeathTimes[Stats.Level] * 1000.0f;
            ChampStats.Deaths += 1;

            var cKiller = data.Killer as IChampion;

            if (cKiller == null && _championHitFlagTimer > 0)
            {
                cKiller = _game.ObjectManager.GetObjectById(_playerHitId) as Champion;
                Logger.Debug("Killed by turret, minion or monster, but still  give gold to the enemy.");
            }

            if (cKiller == null)
            {
                _game.PacketNotifier.NotifyNPC_Hero_Die(data);
                return;
            }

            ApiEventManager.OnKill.Publish(data);

            // TODO: Find out if we can unhardcode some of the fractions used here.
            var gold = mapScriptMetaData.ChampionBaseGoldValue;
            if (KillSpree > 1)
            {
                gold = Math.Min(gold * (float)Math.Pow(7f / 6f, KillSpree - 1), mapScriptMetaData.ChampionMaxGoldValue);
            }
            else if (KillSpree == 0 & DeathSpree >= 1)
            {
                gold *= (11f / 12f);

                if (DeathSpree > 1)
                {
                    gold = Math.Max(gold * (float)Math.Pow(0.8f, DeathSpree / 2), mapScriptMetaData.ChampionMinGoldValue);
                }
                DeathSpree++;
            }

            if (mapScript.HasFirstBloodHappened)
            {
                var onKill = new OnChampionKill { OtherNetID = NetId };
                _game.PacketNotifier.NotifyS2C_OnEventWorld(onKill, data.Killer.NetId);
            }
            else
            {
                gold += mapScript.MapScriptMetadata.FirstBloodExtraGold;
                mapScript.HasFirstBloodHappened = true;
            }

            var EXP = (mapData.ExpCurve[Stats.Level - 1]) * mapData.BaseExpMultiple;
            if (cKiller.Stats.Level != Stats.Level)
            {
                var levelDifference = Math.Abs(cKiller.Stats.Level - Stats.Level);
                float EXPDiff = EXP * Math.Min(mapData.LevelDifferenceExpMultiple * levelDifference, mapData.MinimumExpMultiple);
                if (cKiller.Stats.Level > Stats.Level)
                {
                    EXPDiff = -EXPDiff;
                }
                EXP += EXPDiff;
            }

            cKiller.AddGold(this, gold);
            cKiller.AddExperience(EXP);

            var worldEvent = new OnChampionDie
            {
                GoldGiven = gold,
                OtherNetID = data.Killer.NetId,
                AssistCount = 0
                //Todo: implement assists when an assist system gets implemented
            };

            cKiller.GoldFromMinions = 0;
            cKiller.ChampStats.Kills++;
            cKiller.KillSpree++;
            cKiller.DeathSpree = 0;

            KillSpree = 0;
            DeathSpree++;

            _game.PacketNotifier.NotifyS2C_OnEventWorld(worldEvent, NetId);

            _game.PacketNotifier.NotifyDeath(data);
            //CORE_INFO("After: getGoldFromChamp: %f Killer: %i Victim: %i", gold, cKiller.killDeathCounter,this.killDeathCounter);

            _game.ObjectManager.StopTargeting(this);
        }

        public override void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source, bool isCrit)
        {
            base.TakeDamage(attacker, damage, type, source, isCrit);

            _championHitFlagTimer = 15 * 1000; //15 seconds timer, so when you get executed the last enemy champion who hit you gets the gold
            _playerHitId = attacker.NetId;
            //CORE_INFO("15 second execution timer on you. Do not get killed by a minion, turret or monster!");
        }

        public void UpdateSkin(int skinNo)
        {
            SkinID = skinNo;
        }

        public void IncrementScore(float points, ScoreCategory scoreCategory, ScoreEvent scoreEvent, bool doCallOut, bool notifyText = true)
        {
            Stats.Points += points;
            var scoreData = new ScoreData(this, points, scoreCategory, scoreEvent, doCallOut);
            _game.PacketNotifier.NotifyS2C_IncrementPlayerScore(scoreData);

            if (notifyText)
            {
                //TODO: Figure out what "Params" is exactly
                _game.PacketNotifier.NotifyDisplayFloatingText(new FloatingTextData(this, $"+{(int)points} Points", FloatTextType.Score, 1073741833), Team);
            }

            ApiEventManager.OnIncrementChampionScore.Publish(scoreData);
        }
    }
}

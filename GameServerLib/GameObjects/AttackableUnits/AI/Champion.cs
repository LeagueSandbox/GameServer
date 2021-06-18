using System.Collections.Generic;
using System.Numerics;
using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.NetInfo;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Items;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class Champion : ObjAiBase, IChampion
    {
        public IShop Shop { get; protected set; }
        public float RespawnTimer { get; private set; }
        public float ChampionGoldFromMinions { get; set; }
        public IRuneCollection RuneList { get; }
        public IChampionStats ChampStats { get; private set; } = new ChampionStats();

        public byte SkillPoints { get; set; }
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

        public Champion(Game game,
                        string model,
                        uint playerId,
                        uint playerTeamSpecialId,
                        IRuneCollection runeList,
                        ClientInfo clientInfo,
                        uint netId = 0,
                        TeamId team = TeamId.TEAM_BLUE)
            : base(game, model, new Stats.Stats(), 30, new Vector2(), 1200, netId, team)
        {
            _playerId = playerId;
            _playerTeamSpecialId = playerTeamSpecialId;
            RuneList = runeList;

            Inventory = InventoryManager.CreateInventory();
            Shop = Items.Shop.CreateShop(this, game);

            Stats.Gold = _game.Map.MapProperties.StartingGold;
            Stats.GoldPerSecond.BaseValue = _game.Map.MapProperties.GoldPerSecond;
            Stats.IsGeneratingGold = false;

            //TODO: automaticaly rise spell levels with CharData.SpellLevelsUp

            Spells[(int)SpellSlotType.SummonerSpellSlots] = new Spell.Spell(game, this, clientInfo.SummonerSkills[0], (int)SpellSlotType.SummonerSpellSlots);
            Spells[(int)SpellSlotType.SummonerSpellSlots].LevelUp();
            Spells[(int)SpellSlotType.SummonerSpellSlots + 1] = new Spell.Spell(game, this, clientInfo.SummonerSkills[1], (int)SpellSlotType.SummonerSpellSlots + 1);
            Spells[(int)SpellSlotType.SummonerSpellSlots + 1].LevelUp();

            Spells[(int)SpellSlotType.BluePillSlot] = new Spell.Spell(game, this, "Recall", (int)SpellSlotType.BluePillSlot);
            Stats.SetSpellEnabled((byte)SpellSlotType.BluePillSlot, true);

            Replication = new ReplicationHero(this);
        }

        private string GetPlayerIndex()
        {
            return $"player{_playerId}";
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.ObjectManager.AddChampion(this);
            _game.PacketNotifier.NotifySpawn(this);
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

        public Vector2 GetSpawnPosition()
        {
            var config = _game.Config;
            var playerIndex = GetPlayerIndex();
            var playerTeam = "";
            var teamSize = GetTeamSize();

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

            if (teamSize > config.MapSpawns.Blue.Count || teamSize > config.MapSpawns.Purple.Count)
            {
                var spawns1 = spawnsByTeam[Team];
                return spawns1[0].GetCoordsForPlayer(0);
            }

            var spawns = spawnsByTeam[Team];
            return spawns[teamSize - 1].GetCoordsForPlayer((int)_playerTeamSpecialId);
        }

        public Vector2 GetRespawnPosition()
        {
            var config = _game.Config;

            var spawnsByTeam = new Dictionary<TeamId, Dictionary<int, PlayerSpawns>>
            {
                {TeamId.TEAM_BLUE, config.MapSpawns.Blue},
                {TeamId.TEAM_PURPLE, config.MapSpawns.Purple}
            };
            var spawns1 = spawnsByTeam[Team];
            return spawns1[0].GetCoordsForPlayer(0);
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

        public override void Update(float diff)
        {
            base.Update(diff);

            if (!Stats.IsGeneratingGold && _game.GameTime >= _game.Map.MapProperties.FirstGoldTime)
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

            if (LevelUp())
            {
                _game.PacketNotifier.NotifyNPC_LevelUp(this);
                // TODO: send this in one place only
                _game.PacketNotifier.NotifyUpdatedStats(this, false);
            }

            if (_championHitFlagTimer > 0)
            {
                _championHitFlagTimer -= diff;
                if (_championHitFlagTimer <= 0)
                {
                    _championHitFlagTimer = 0;
                }
            }
            Replication.Update();
        }

        public void Respawn()
        {
            var spawnPos = GetRespawnPosition();
            SetPosition(spawnPos.X, spawnPos.Y);
            _game.PacketNotifier.NotifyChampionRespawn(this);
            Stats.CurrentHealth = Stats.HealthPoints.Total;
            Stats.CurrentMana = Stats.HealthPoints.Total;
            IsDead = false;
            RespawnTimer = -1;
        }

        public bool OnDisconnect()
        {
            this.StopMovement();
            this.SetWaypoints(_game.Map.NavigationGrid.GetPath(Position, GetRespawnPosition()));
            this.UpdateMoveOrder(OrderType.MoveTo, true);

            return true;
        }

        public void Recall()
        {
            var spawnPos = GetRespawnPosition();
            TeleportTo(spawnPos.X, spawnPos.Y);
        }

        public bool LevelUp()
        {
            var stats = Stats;
            var expMap = _game.Config.MapData.ExpCurve;

            //Ideally we'd use "stats.Level < expMap.Count + 1", but since we still don't have gamemodes implemented yet, i'll be hardcoding the EXP level to cap at lvl 18,
            //Since the SR Map has 30 levels in total because of URF
            if (stats.Level < 18 && (stats.Level < 1 || stats.Experience >= expMap[stats.Level - 1])) //The + and - 1s are there because the XP files don't have level 1
            {
                Stats.LevelUp();
                Logger.Debug("Champion " + Model + " leveled up to " + stats.Level);
                if (stats.Level <= 18)
                {
                    SkillPoints++;
                }
                return true;
            }
            return false;
        }

        public void OnKill(IAttackableUnit killed)
        {
            if (killed is IMinion)
            {
                ChampStats.MinionsKilled += 1;
                if (killed.Team == TeamId.TEAM_NEUTRAL)
                {
                    ChampStats.NeutralMinionsKilled += 1;
                }

                var gold = _game.Map.MapProperties.GetGoldFor(killed);
                if (gold <= 0)
                {
                    return;
                }

                Stats.Gold += gold;
                _game.PacketNotifier.NotifyAddGold(this, killed, gold);

                if (KillDeathCounter < 0)
                {
                    ChampionGoldFromMinions += gold;
                    Logger.Debug($"Adding gold form minions to reduce death spree: {ChampionGoldFromMinions}");
                }

                if (ChampionGoldFromMinions >= 50 && KillDeathCounter < 0)
                {
                    ChampionGoldFromMinions = 0;
                    KillDeathCounter += 1;
                }
            }
        }

        public override void Die(IAttackableUnit killer)
        {
            RespawnTimer = _game.Config.MapData.DeathTimes[Stats.Level] * 1000.0f;
            ChampStats.Deaths += 1;

            _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.DEATH, this, killer);

            var cKiller = killer as IChampion;

            if (cKiller == null && _championHitFlagTimer > 0)
            {
                cKiller = _game.ObjectManager.GetObjectById(_playerHitId) as IChampion;
                Logger.Debug("Killed by turret, minion or monster, but still  give gold to the enemy.");
            }

            if (cKiller == null)
            {
                _game.PacketNotifier.NotifyChampionDie(this, killer, 0);
                return;
            }

            cKiller.ChampionGoldFromMinions = 0;
            cKiller.ChampStats.Kills += 1;
            // TODO: add assists

            var gold = _game.Map.MapProperties.GetGoldFor(this);
            Logger.Debug($"Before: getGoldFromChamp: {gold} Killer: {cKiller.KillDeathCounter} Victim {KillDeathCounter}");

            if (cKiller.KillDeathCounter < 0)
                cKiller.KillDeathCounter = 0;

            if (cKiller.KillDeathCounter >= 0)
                cKiller.KillDeathCounter += 1;

            if (KillDeathCounter > 0)
                KillDeathCounter = 0;

            if (KillDeathCounter <= 0)
                KillDeathCounter -= 1;

            if (gold < 0)
            {
                _game.PacketNotifier.NotifyChampionDie(this, cKiller, 0);
                return;
            }

            if (_game.Map.MapProperties.IsKillGoldRewardReductionActive
                && _game.Map.MapProperties.HasFirstBloodHappened)
            {
                gold -= gold * 0.25f;
                //CORE_INFO("Still some minutes for full gold reward on champion kills");
            }

            if (!_game.Map.MapProperties.HasFirstBloodHappened)
            {
                gold += 100;
                _game.Map.MapProperties.HasFirstBloodHappened = true;
            }

            _game.PacketNotifier.NotifyChampionDie(this, cKiller, (int)gold);

            cKiller.Stats.Gold = cKiller.Stats.Gold + gold;
            _game.PacketNotifier.NotifyAddGold(cKiller, this, gold);
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
            Skin = skinNo;
        }
    }
}

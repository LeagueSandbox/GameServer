using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.NetInfo;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.Spells;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Items;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class Champion : ObjAiBase, IChampion
    {
        public IGame RefGame { get; }
        public IShop Shop { get; protected set; }
        public float RespawnTimer { get; private set; }
        public float ChampionGoldFromMinions { get; set; }
        public IRuneCollection RuneList { get; }
        public Dictionary<short, ISpell> Spells { get; }
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
                        uint netId = 0)
            : base(game, model, new Stats.Stats(), 30, 0, 0, 1200, netId)
        {
            RefGame = game;
            _playerId = playerId;
            _playerTeamSpecialId = playerTeamSpecialId;
            RuneList = runeList;

            Spells = new Dictionary<short, ISpell>();

            Inventory = InventoryManager.CreateInventory();
            Shop = Items.Shop.CreateShop(this, game);

            Stats.Gold = _game.Map.MapProperties.StartingGold;
            Stats.GoldPerSecond.BaseValue = _game.Map.MapProperties.GoldPerSecond;
            Stats.IsGeneratingGold = false;

            //TODO: automaticaly rise spell levels with CharData.SpellLevelsUp

            for (short i = 0; i < CharData.SpellNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(CharData.SpellNames[i]))
                {
                    Spells[i] = new Spell(game, this, CharData.SpellNames[i], (byte)i);
                }
            }

            Spells[4] = new Spell(game, this, clientInfo.SummonerSkills[0], 4);
            Spells[5] = new Spell(game, this, clientInfo.SummonerSkills[1], 5);

            for (byte i = 6; i < 13; i++)
            {
                Spells[i] = new Spell(game, this, "BaseSpell", i);
            }

            Spells[13] = new Spell(game, this, "Recall", 13);

            for (short i = 0; i < CharData.Passives.Length; i++)
            {
                if (!string.IsNullOrEmpty(CharData.Passives[i].PassiveLuaName))
                {
                    Spells[(byte)(i + 14)] = new Spell(game, this, CharData.Passives[i].PassiveLuaName, (byte)(i + 14));
                }
            }

            for (short i = 0; i < CharData.ExtraSpells.Length; i++)
            {
                if (!string.IsNullOrEmpty(CharData.ExtraSpells[i]))
                {
                    var spell = new Spell(game, this, CharData.ExtraSpells[i], (byte)(i + 45));
                    Spells[(byte)(i + 45)] = spell;
                    spell.LevelUp();
                }
            }

            Spells[4].LevelUp();
            Spells[5].LevelUp();
            Replication = new ReplicationHero(this);
            Stats.SetSpellEnabled(13, true);
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

        public bool CanMove()
        {
            return !HasCrowdControl(CrowdControlType.STUN) &&
                !IsDashing &&
                !IsCastingSpell &&
                !IsDead &&
                !HasCrowdControl(CrowdControlType.ROOT);
        }

        public void UpdateMoveOrder(MoveOrder order)
        {
            MoveOrder = order;
        }

        public bool CanCast()
        {
            return !HasCrowdControl(CrowdControlType.STUN) && !HasCrowdControl(CrowdControlType.SILENCE);
        }

        public Vector2 GetSpawnPosition()
        {
            var config = _game.Config;
            var playerIndex = GetPlayerIndex();
            var playerTeam = "";
            var teamSize = GetTeamSize();

            if (teamSize > 6) //???
            {
                teamSize = 6;
            }

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
                X = _game.Map.MapProperties.GetRespawnLocation(Team).X,
                Y = _game.Map.MapProperties.GetRespawnLocation(Team).Y
            };

            return new Vector2(coords.X, coords.Y);
        }

        public void StopChampionMovement()
        {
            List<Vector2> l = new List<Vector2>();
            l.Add(new Vector2(this.X, this.Y));
            this.SetWaypoints(l);
            _game.PacketNotifier.NotifyMovement(this);
        }

        public ISpell GetSpellBySlot(byte slot)
        {
            return Spells[slot];
        }

        public ISpell GetSpellByName(string name)
        {
            foreach (var s in Spells.Values)
            {
                if (s == null)
                {
                    continue;
                }

                if (s.SpellName == name)
                {
                    return s;
                }
            }

            return null;
        }

        public ISpell LevelUpSpell(byte slot)
        {
            if (SkillPoints == 0)
            {
                return null;
            }

            var s = Spells[slot];

            if (s == null || !CanSpellBeLeveledUp(s))
            {
                return null;
            }

            s.LevelUp();
            SkillPoints--;

            return s;
        }

        public bool CanSpellBeLeveledUp(ISpell s)
        {
            return CharData.SpellsUpLevels[s.Slot][s.Level] <= Stats.Level;
        }

        public override void Update(float diff)
        {
            base.Update(diff);

            if (!IsDead && MoveOrder == MoveOrder.MOVE_ORDER_ATTACKMOVE && TargetUnit != null)
            {
                var objects = _game.ObjectManager.GetObjects();
                var distanceToTarget = 25000f;
                IAttackableUnit nextTarget = null;
                var range = Math.Max(Stats.Range.Total, DETECT_RANGE);

                foreach (var it in objects)
                {
                    if (!(it.Value is IAttackableUnit u) ||
                        u.IsDead ||
                        u.Team == Team ||
                        GetDistanceTo(u) > range)
                        continue;

                    if (!(GetDistanceTo(u) < distanceToTarget))
                        continue;
                    distanceToTarget = GetDistanceTo(u);
                    nextTarget = u;
                }

                if (nextTarget != null)
                {
                    TargetUnit = nextTarget;
                    _game.PacketNotifier.NotifySetTarget(this, nextTarget);
                }
            }

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

            var isLevelup = LevelUp();
            if (isLevelup)
            {
                _game.PacketNotifier.NotifyLevelUp(this);
                // TODO: send this in one place only
                _game.PacketNotifier.NotifyUpdatedStats(this, false);
            }

            foreach (var s in Spells.Values)
            {
                s.Update(diff);
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

        public void Recall()
        {
            var spawnPos = GetRespawnPosition();
            TeleportTo(spawnPos.X, spawnPos.Y);
        }

        public int GetChampionHash()
        {
            var szSkin = "";

            if (Skin < 10)
            {
                szSkin = "0" + Skin;
            }
            else
            {
                szSkin = Skin.ToString();
            }

            var hash = 0;
            var gobj = "[Character]";

            for (var i = 0; i < gobj.Length; i++)
            {
                hash = char.ToLower(gobj[i]) + 0x1003F * hash;
            }

            for (var i = 0; i < Model.Length; i++)
            {
                hash = char.ToLower(Model[i]) + 0x1003F * hash;
            }

            for (var i = 0; i < szSkin.Length; i++)
            {
                hash = char.ToLower(szSkin[i]) + 0x1003F * hash;
            }

            return hash;
        }

        public bool LevelUp()
        {
            var stats = Stats;
            var expMap = _game.Map.MapProperties.ExpToLevelUp;
            if (stats.Level >= expMap.Count)
            {
                return false;
            }

            if (stats.Experience < expMap[stats.Level])
            {
                return false;
            }

            while (stats.Level < expMap.Count && stats.Experience >= expMap[stats.Level])
            {
                Stats.LevelUp();
                Logger.Debug("Champion " + Model + " leveled up to " + stats.Level);
                SkillPoints++;
            }

            return true;
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
            RespawnTimer = 5000 + Stats.Level * 2500;
            _game.ObjectManager.StopTargeting(this);
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

        public override void OnCollision(IGameObject collider)
        {
            base.OnCollision(collider);
            if (collider == null)
            {
                //CORE_INFO("I bumped into a wall!");
            }
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

        public void SetSpell(string name, byte slot, bool enabled)
        {
            Spells[slot] = new Spell(_game, this, name, slot);
            Stats.SetSpellEnabled(slot, enabled);
        }

        public void SwapSpells(byte slot1, byte slot2)
        {
            var enabledBuffer = Stats.GetSpellEnabled(slot1);
            var buffer = Spells[slot1];
            Spells[slot1] = Spells[slot2];
            Spells[slot2] = buffer;
            Stats.SetSpellEnabled(slot1, Stats.GetSpellEnabled(slot2));
            Stats.SetSpellEnabled(slot2, enabledBuffer);
        }

        public void RemoveSpell(byte slot)
        {
            Spells[slot].Deactivate();
            Spells[slot] = new Spell(_game, this, "BaseSpell", slot); // Replace previous spell with empty spell.
            Stats.SetSpellEnabled(slot, false);
        }

        ISpell IChampion.GetSpell(byte slot)
        {
            return Spells[slot];
        }

        ISpell IChampion.LevelUpSpell(byte slot)
        {
            return LevelUpSpell(slot);
        }
    }
}

using LeagueSandbox.GameServer.Logic.Items;
using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Champion : ObjAIBase
    {
        public Shop Shop { get; protected set; }
        public float RespawnTimer { get; private set; }
        public float ChampionGoldFromMinions { get; set; }
        public RuneCollection RuneList { get; set; }
        public Dictionary<short, Spell> Spells { get; private set; } = new Dictionary<short, Spell>();
        public float GoldPerSecond { get; set; }
        public bool IsGeneratingGold { get; set; }

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

        public Champion(string model,
                        uint playerId,
                        uint playerTeamSpecialId,
                        RuneCollection runeList,
                        ClientInfo clientInfo,
                        uint netId = 0)
            : base(model, 30, 0, 0, 1200, netId)
        {
            Stats.LoadStats(model, CharData, Skin);
            _playerId = playerId;
            _playerTeamSpecialId = playerTeamSpecialId;
            RuneList = runeList;

            Inventory = InventoryManager.CreateInventory(this);
            Shop = Shop.CreateShop(this);

            Stats.Gold += 475.0f;
            Stats.TotalGold += 475.0f;
            GoldPerSecond = _game.Map.MapGameScript.GoldPerSecond;
            IsGeneratingGold = false;

            //TODO: automaticaly rise spell levels with CharData.SpellLevelsUp
            for (short i = 0; i < CharData.SpellNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(CharData.SpellNames[i]))
                {
                    Spells[i] = new Spell(this, CharData.SpellNames[i], (byte)i);
                }
            }

            Spells[4] = new Spell(this, clientInfo.SummonerSkills[0], 4);
            Spells[5] = new Spell(this, clientInfo.SummonerSkills[1], 5);
            Spells[13] = new Spell(this, "Recall", 13);

            for (short i = 0; i < CharData.Passives.Length; i++)
            {
                if (!string.IsNullOrEmpty(CharData.Passives[i].PassiveLuaName))
                {
                    Spells[(byte)(i + 14)] = new Spell(this, CharData.Passives[i].PassiveLuaName, (byte)(i + 14));
                }
            }

            for (short i = 0; i < CharData.ExtraSpells.Length; i++)
            {
                if (!string.IsNullOrEmpty(CharData.ExtraSpells[i]))
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

        public override void UpdateReplication()
        {
            ReplicationManager.UpdateFloat(Stats.Gold, 0, 0);
            ReplicationManager.UpdateFloat(Stats.TotalGold, 0, 1);
            ReplicationManager.UpdateUint(Stats.SpellEnabledBitFieldLower1, 0, 2);
            ReplicationManager.UpdateUint(Stats.SpellEnabledBitFieldUpper1, 0, 3);
            ReplicationManager.UpdateUint(Stats.SpellEnabledBitFieldLower2, 0, 4);
            ReplicationManager.UpdateUint(Stats.SpellEnabledBitFieldUpper2, 0, 5);
            ReplicationManager.UpdateUint(Stats.EvolvePoints, 0, 6);
            ReplicationManager.UpdateUint(Stats.EvolveFlags, 0, 7);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(0) ? Spells[0]?.CurrentManaCost ?? 0f : 0, 0, 8);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(1) ? Spells[1]?.CurrentManaCost ?? 0f : 0, 0, 9);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(2) ? Spells[2]?.CurrentManaCost ?? 0f : 0, 0, 10);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(3) ? Spells[3]?.CurrentManaCost ?? 0f : 0, 0, 11);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(45) ? Spells[45]?.CurrentManaCost ?? 0f : 0, 0, 12);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(46) ? Spells[46]?.CurrentManaCost ?? 0f : 0, 0, 13);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(47) ? Spells[47]?.CurrentManaCost ?? 0f : 0, 0, 14);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(48) ? Spells[48]?.CurrentManaCost ?? 0f : 0, 0, 15);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(49) ? Spells[49]?.CurrentManaCost ?? 0f : 0, 0, 16);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(50) ? Spells[50]?.CurrentManaCost ?? 0f : 0, 0, 17);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(51) ? Spells[51]?.CurrentManaCost ?? 0f : 0, 0, 18);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(52) ? Spells[52]?.CurrentManaCost ?? 0f : 0, 0, 19);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(53) ? Spells[53]?.CurrentManaCost ?? 0f : 0, 0, 20);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(54) ? Spells[54]?.CurrentManaCost ?? 0f : 0, 0, 21);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(55) ? Spells[55]?.CurrentManaCost ?? 0f : 0, 0, 22);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(56) ? Spells[56]?.CurrentManaCost ?? 0f : 0, 0, 23);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(57) ? Spells[57]?.CurrentManaCost ?? 0f : 0, 0, 24);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(58) ? Spells[58]?.CurrentManaCost ?? 0f : 0, 0, 25);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(59) ? Spells[59]?.CurrentManaCost ?? 0f : 0, 0, 26);
            ReplicationManager.UpdateFloat(Spells.ContainsKey(60) ? Spells[60]?.CurrentManaCost ?? 0f : 0, 0, 27);
            ReplicationManager.UpdateUint((uint)Stats.ActionState, 1, 0);
            ReplicationManager.UpdateBool(Stats.IsMagicImmune, 1, 1);
            ReplicationManager.UpdateBool(Stats.IsInvulnerable, 1, 2);
            ReplicationManager.UpdateBool(Stats.IsPhysicalImmune, 1, 3);
            ReplicationManager.UpdateBool(Stats.IsLifestealImmune, 1, 4);
            ReplicationManager.UpdateFloat(Stats.BaseAttackDamage, 1, 5);
            ReplicationManager.UpdateFloat(0, 1, 6); // Base ability power?
            ReplicationManager.UpdateFloat(Stats.DodgeChance, 1, 7);
            ReplicationManager.UpdateFloat(Stats.CriticalChance, 1, 8);
            ReplicationManager.UpdateFloat(Stats.TotalArmor, 1, 9);
            ReplicationManager.UpdateFloat(Stats.TotalMagicResist, 1, 10);
            ReplicationManager.UpdateFloat(Stats.TotalHealthRegen, 1, 11);
            ReplicationManager.UpdateFloat(Stats.TotalParRegen, 1, 12);
            ReplicationManager.UpdateFloat(Stats.TotalAttackRange, 1, 13);
            ReplicationManager.UpdateFloat(Stats.FlatAttackDamageMod, 1, 14);
            ReplicationManager.UpdateFloat(Stats.PercentAttackDamageMod, 1, 15);
            ReplicationManager.UpdateFloat(Stats.FlatAbilityPower, 1, 16);
            ReplicationManager.UpdateFloat(Stats.FlatMagicReduction, 1, 17);
            ReplicationManager.UpdateFloat(1 - Stats.PercentMagicReduction, 1, 18);
            ReplicationManager.UpdateFloat(Stats.PercentAttackSpeedMod + Stats.PercentAttackSpeedDebuff, 1, 19);
            ReplicationManager.UpdateFloat(Stats.FlatAttackRangeMod, 1, 20);
            ReplicationManager.UpdateFloat(Stats.CooldownReduction, 1, 21);
            ReplicationManager.UpdateFloat(Stats.PassiveCooldownEndTime, 1, 22);
            ReplicationManager.UpdateFloat(Stats.PassiveCooldownTotalTime, 1, 23);
            ReplicationManager.UpdateFloat(Stats.FlatArmorPenetration, 1, 24);
            ReplicationManager.UpdateFloat(1 - Stats.PercentArmorPenetration, 1, 25);
            ReplicationManager.UpdateFloat(Stats.FlatMagicPenetration, 1, 26);
            ReplicationManager.UpdateFloat(1 - Stats.PercentMagicPenetration, 1, 27);
            ReplicationManager.UpdateFloat(Stats.LifeSteal, 1, 28);
            ReplicationManager.UpdateFloat(Stats.SpellVamp, 1, 29);
            ReplicationManager.UpdateFloat(1 - Stats.Tenacity, 1, 30);
            ReplicationManager.UpdateFloat(1 - Stats.PercentBonusArmorPenetration, 2, 0);
            ReplicationManager.UpdateFloat(1 - Stats.PercentBonusMagicPenetration, 2, 1);
            ReplicationManager.UpdateFloat(Stats.BaseHealthRegen, 2, 2);
            ReplicationManager.UpdateFloat(Stats.BaseParRegen, 2, 3);
            ReplicationManager.UpdateFloat(Stats.CurrentHealth, 3, 0);
            ReplicationManager.UpdateFloat(Stats.CurrentPar, 3, 1);
            ReplicationManager.UpdateFloat(Stats.TotalHealth, 3, 2);
            ReplicationManager.UpdateFloat(Stats.TotalPar, 3, 3);
            ReplicationManager.UpdateFloat(Stats.Experience, 3, 4);
            ReplicationManager.UpdateFloat(Stats.LifeTime, 3, 5);
            ReplicationManager.UpdateFloat(Stats.MaxLifeTime, 3, 6);
            ReplicationManager.UpdateFloat(Stats.LifeTimeTicks, 3, 7);
            ReplicationManager.UpdateFloat(0, 3, 8); // FlatSightRangeMod
            ReplicationManager.UpdateFloat(0, 3, 9); // PercentSightRangeMod
            ReplicationManager.UpdateFloat(Stats.TotalMovementSpeed, 3, 10);
            ReplicationManager.UpdateFloat(Stats.TotalSize, 3, 11);
            ReplicationManager.UpdateFloat(Stats.FlatPathfindingRadiusMod, 3, 12);
            ReplicationManager.UpdateUint(Stats.Level, 3, 13);
            ReplicationManager.UpdateUint(Stats.NumberOfNeutralMinionsKilled, 3, 14);
            ReplicationManager.UpdateBool(Stats.IsTargetable, 3, 15);
            ReplicationManager.UpdateUint((uint)Stats.IsTargetableToTeam, 3, 16);
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

        public Spell LevelUpSpell(short slot)
        {
            if (SkillPoints == 0)
            {
                return null;
            }

            var s = GetSpell((byte)slot);

            if (s == null)
            {
                return null;
            }

            if (CharData.SpellsUpLevels[slot][s.Level] > Stats.Level)
            {
                return null;
            }

            s.levelUp();
            SkillPoints--;

            return s;
        }

        public override void update(float diff)
        {
            base.update(diff);

            if (!IsDead && MoveOrder == MoveOrder.MOVE_ORDER_ATTACKMOVE && TargetUnit != null)
            {
                var objects = _game.ObjectManager.GetObjects();
                var distanceToTarget = 9000000.0f;
                AttackableUnit nextTarget = null;
                var range = Math.Max(Stats.TotalAttackRange, DETECT_RANGE);

                foreach (var it in objects)
                {
                    var u = it.Value as AttackableUnit;

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

            if (!IsGeneratingGold && _game.GameTime >= _game.Map.MapGameScript.FirstGoldTime)
            {
                IsGeneratingGold = true;
                _logger.LogCoreInfo("Generating Gold!");
            }

            if (IsGeneratingGold)
            {
                var deltaGold = GoldPerSecond * 0.001f * diff;
                Stats.Gold += deltaGold;
                Stats.TotalGold += deltaGold;
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
                _game.PacketNotifier.NotifyLevelUp(this);
                _game.PacketNotifier.NotifyUpdatedStats(this, false);
            }

            foreach (var s in Spells.Values)
            {
                s.update(diff);
            }

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
            Stats.CurrentHealth = Stats.TotalHealth;
            Stats.CurrentPar = Stats.TotalPar;
            IsDead = false;
            RespawnTimer = -1;
        }

	    public void Recall(ObjAIBase owner)
        {
            var spawnPos = GetRespawnPosition();
            _game.PacketNotifier.NotifyTeleport(owner, spawnPos.X, spawnPos.Y);
        }

        public int getChampionHash()
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

            int hash = 0;
            var gobj = "[Character]";
            for (var i = 0; i < gobj.Length; i++)
            {
                hash = char.ToLower(gobj[i]) + (0x1003F * hash);
            }
            for (var i = 0; i < Model.Length; i++)
            {
                hash = char.ToLower(Model[i]) + (0x1003F * hash);
            }
            for (var i = 0; i < szSkin.Length; i++)
            {
                hash = char.ToLower(szSkin[i]) + (0x1003F * hash);
            }
            return hash;
        }

        public bool LevelUp()
        {
            var expMap = _game.Map.MapGameScript.ExpToLevelUp;
            if (Stats.Level >= expMap.Count)
            {
                return false;
            }

            if (Stats.Experience < expMap[(int)Stats.Level])
            {
                return false;
            }

            while (Stats.Level < expMap.Count && Stats.Experience >= expMap[(int)Stats.Level])
            {
                Stats.Level++;
                Stats.CurrentHealth = (Stats.TotalHealth / (Stats.TotalHealth - Stats.HealthGrowth)) * Stats.CurrentHealth;
                Stats.CurrentPar = (Stats.TotalPar / (Stats.TotalPar - Stats.ParGrowth)) * Stats.CurrentPar;
                _logger.LogCoreInfo("Champion " + Model + " leveled up to " + Stats.Level);
                SkillPoints++;
            }

            return true;
        }

        public override void die(ObjAIBase killer)
        {
            RespawnTimer = 5000 + Stats.Level * 2500;
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
            {
                cKiller.KillDeathCounter = 0;
            }

            if (cKiller.KillDeathCounter >= 0)
            {
                cKiller.KillDeathCounter += 1;
            }

            if (KillDeathCounter > 0)
            {
                KillDeathCounter = 0;
            }

            if (KillDeathCounter <= 0)
            {
                KillDeathCounter -= 1;
            }

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

            cKiller.Stats.Gold += gold;
            if (gold > 0)
            {
                cKiller.Stats.TotalGold += gold;
            }

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

        public override void TakeDamage(ObjAIBase attacker, float damage, DamageType type, DamageSource source,
            bool isCrit)
        {
            base.TakeDamage(attacker, damage, type, source, isCrit);

            _championHitFlagTimer = 15 * 1000; //15 seconds timer, so when you get executed the last enemy champion who hit you gets the gold
            _playerHitId = NetId;
            //CORE_INFO("15 second execution timer on you. Do not get killed by a minion, turret or monster!");
        }
    }
}

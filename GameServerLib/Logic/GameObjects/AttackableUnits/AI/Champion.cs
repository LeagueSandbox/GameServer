using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.Spells;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;
using LeagueSandbox.GameServer.Logic.Items;

namespace LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI
{
    public class Champion : ObjAiBase
    {
        public Shop Shop { get; protected set; }
        public float RespawnTimer { get; private set; }
        public float ChampionGoldFromMinions { get; set; }
        public RuneCollection RuneList { get; set; }
        public Dictionary<short, Spell> Spells { get; } = new Dictionary<short, Spell>();

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
            : base(model, new Stats.Stats(), 30, 0, 0, 1200, netId)
        {
            _playerId = playerId;
            _playerTeamSpecialId = playerTeamSpecialId;
            RuneList = runeList;

            Inventory = InventoryManager.CreateInventory(this);
            Shop = Shop.CreateShop(this);

            Stats.Gold = 475.0f;
            Stats.GoldPerSecond.BaseValue = Game.Map.MapGameScript.GoldPerSecond;
            Stats.IsGeneratingGold = false;

            //TODO: automaticaly rise spell levels with CharData.SpellLevelsUp

            for (short i = 0; i<CharData.SpellNames.Length; i++)
            {
                if (!string.IsNullOrEmpty(CharData.SpellNames[i]))
                {
                    Spells[i] = new Spell(this, CharData.SpellNames[i], (byte)i);
                }
            }

            Spells[4] = new Spell(this, clientInfo.SummonerSkills[0], 4);
            Spells[5] = new Spell(this, clientInfo.SummonerSkills[1], 5);

            for (byte i = 6; i < 13; i++)
            {
                Spells[i] = new Spell(this, "BaseSpell", i);
            }

            Spells[13] = new Spell(this, "Recall", 13);

            for (short i = 0; i<CharData.Passives.Length; i++)
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
            Game.ObjectManager.AddChampion(this);
            Game.PacketNotifier.NotifyChampionSpawned(this, Team);
        }

        public override void OnRemoved()
        {
            base.OnRemoved();
            Game.ObjectManager.RemoveChampion(this);
        }

        public int GetTeamSize()
        {
            var blueTeamSize = 0;
            var purpTeamSize = 0;

            foreach (var player in Game.Config.Players.Values)
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
            if (Game.Config.Players.ContainsKey(playerIndex))
            {
                switch (Game.Config.Players[playerIndex].Team.ToLower())
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

        public bool CanCast()
        {
            return !HasCrowdControl(CrowdControlType.STUN) &&
                !HasCrowdControl(CrowdControlType.SILENCE);
        }

        public Vector2 GetSpawnPosition()
        {
            var config = Game.Config;
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
            var config = Game.Config;
            var playerIndex = GetPlayerIndex();

            if (config.Players.ContainsKey(playerIndex))
            {
                var p = config.Players[playerIndex];
            }

            var coords = new Vector2
            {
                X = Game.Map.MapGameScript.GetRespawnLocation(Team).X,
                Y = Game.Map.MapGameScript.GetRespawnLocation(Team).Y
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
            if (_skillPoints == 0)
            {
                return null;
            }

            var s = GetSpell((byte)slot);

            if (s == null)
            {
                return null;
            }

            s.LevelUp();
            _skillPoints--;

            return s;
        }

        public override void Update(float diff)
        {
            base.Update(diff);

            if (!IsDead && MoveOrder == MoveOrder.MOVE_ORDER_ATTACKMOVE && TargetUnit != null)
            {
                var objects = Game.ObjectManager.GetObjects();
                var distanceToTarget = 25000f;
                AttackableUnit nextTarget = null;
                var range = Math.Max(Stats.Range.Total, DETECT_RANGE);

                foreach (var it in objects)
                {
                    if (!(it.Value is AttackableUnit u) || u.IsDead || u.Team == Team || GetDistanceTo(u) > range)
                    {
                        continue;
                    }

                    if (GetDistanceTo(u) < distanceToTarget)
                    {
                        distanceToTarget = GetDistanceTo(u);
                        nextTarget = u;
                    }
                }

                if (nextTarget != null)
                {
                    TargetUnit = nextTarget;
                    Game.PacketNotifier.NotifySetTarget(this, nextTarget);
                }
            }

            if (!Stats.IsGeneratingGold && Game.GameTime >= Game.Map.MapGameScript.FirstGoldTime)
            {
                Stats.IsGeneratingGold = true;
                Logger.LogCoreInfo("Generating Gold!");
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
                Game.PacketNotifier.NotifyLevelUp(this);
                // TODO: send this in one place only
                Game.PacketNotifier.NotifyUpdatedStats(this, false);
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
            Game.PacketNotifier.NotifyChampionRespawn(this);
            Stats.CurrentHealth = Stats.HealthPoints.Total;
            Stats.CurrentMana = Stats.HealthPoints.Total;
            IsDead = false;
            RespawnTimer = -1;
        }

	    public void Recall(ObjAiBase owner)
        {
            var spawnPos = GetRespawnPosition();
            Game.PacketNotifier.NotifyTeleport(owner, spawnPos.X, spawnPos.Y);
        }

        public void SetSkillPoints(int skillPoints)
        {
            skillPoints = (short)skillPoints;
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

        public short GetSkillPoints()
        {
            return _skillPoints;
        }

        public bool LevelUp()
        {
            var stats = Stats;
            var expMap = Game.Map.MapGameScript.ExpToLevelUp;
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
                Logger.LogCoreInfo("Champion " + Model + " leveled up to " + stats.Level);
                _skillPoints++;
            }

            return true;
        }

        public InventoryManager GetInventory()
        {
            return Inventory;
        }

        public override void Die(AttackableUnit killer)
        {
            RespawnTimer = 5000 + Stats.Level * 2500;
            Game.ObjectManager.StopTargeting(this);

            Game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.DEATH, this, killer);

            var cKiller = killer as Champion;

            if (cKiller == null && _championHitFlagTimer > 0)
            {
                cKiller = Game.ObjectManager.GetObjectById(_playerHitId) as Champion;
                Logger.LogCoreInfo("Killed by turret, minion or monster, but still  give gold to the enemy.");
            }

            if (cKiller == null)
            {
                Game.PacketNotifier.NotifyChampionDie(this, killer, 0);
                return;
            }

            cKiller.ChampionGoldFromMinions = 0;

            var gold = Game.Map.MapGameScript.GetGoldFor(this);
            Logger.LogCoreInfo(
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
                Game.PacketNotifier.NotifyChampionDie(this, cKiller, 0);
                return;
            }

            if (Game.Map.MapGameScript.IsKillGoldRewardReductionActive
                && Game.Map.MapGameScript.HasFirstBloodHappened)
            {
                gold -= gold * 0.25f;
                //CORE_INFO("Still some minutes for full gold reward on champion kills");
            }

            if (!Game.Map.MapGameScript.HasFirstBloodHappened)
            {
                gold += 100;
                Game.Map.MapGameScript.HasFirstBloodHappened = true;
            }

            Game.PacketNotifier.NotifyChampionDie(this, cKiller, (int)gold);

            cKiller.Stats.Gold = cKiller.Stats.Gold + gold;
            Game.PacketNotifier.NotifyAddGold(cKiller, this, gold);

            //CORE_INFO("After: getGoldFromChamp: %f Killer: %i Victim: %i", gold, cKiller.killDeathCounter,this.killDeathCounter);

            Game.ObjectManager.StopTargeting(this);
        }

        public override void OnCollision(GameObject collider)
        {
            base.OnCollision(collider);
            if (collider == null)
            {
                //CORE_INFO("I bumped into a wall!");
            }
        }

        public override void TakeDamage(AttackableUnit attacker, float damage, DamageType type, DamageSource source, bool isCrit)
        {
            base.TakeDamage(attacker, damage, type, source, isCrit);

            _championHitFlagTimer = 15 * 1000; //15 seconds timer, so when you get executed the last enemy champion who hit you gets the gold
            _playerHitId = NetId;
            //CORE_INFO("15 second execution timer on you. Do not get killed by a minion, turret or monster!");
        }
    }
}

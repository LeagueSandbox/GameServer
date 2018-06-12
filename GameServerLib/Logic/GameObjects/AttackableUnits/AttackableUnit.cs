﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;
using LeagueSandbox.GameServer.Logic.Items;

namespace LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits
{
    public class AttackableUnit : GameObject
    {
        internal const float DETECT_RANGE = 475.0f;
        internal const int EXP_RANGE = 1400;

        private float _healthUpdateTimer;
        private float _statUpdateTimer;
        public bool IsModelUpdated { get; set; }
        public bool IsDead { get; protected set; }
        private string _model;
        public string Model
        {
            get => _model;
            set
            {
                _model = value;
                IsModelUpdated = true;
            }
        }
        public ReplicationManager ReplicationManager { get; private set; }
        protected Logger _logger = Program.ResolveDependency<Logger>();
        public InventoryManager Inventory { get; protected set; }
        public int KillDeathCounter { get; protected set; }
        public Stats.Stats Stats { get; protected set; }

        public AttackableUnit(
            string model,
            int collisionRadius = 40,
            float x = 0,
            float y = 0,
            int visionRadius = 0,
            uint netId = 0
        ) : base(x, y, collisionRadius, visionRadius, netId)
        {
            Stats = new Stats.Stats();
            Model = model;
            CollisionRadius = 40;
            ReplicationManager = new ReplicationManager();
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.ObjectManager.AddVisionUnit(this);
        }

        public override void OnRemoved()
        {
            base.OnRemoved();
            _game.ObjectManager.RemoveVisionUnit(this);
        }

        public override void update(float diff)
        {
            base.update(diff);

            _healthUpdateTimer += diff;
            _statUpdateTimer += diff;
            if (_healthUpdateTimer >= 500)
            {
                if (Stats.TotalHealthRegen > 0 && Stats.CurrentHealth < Stats.TotalHealth && Stats.CurrentHealth > 0)
                {
                    Stats.CurrentHealth = Math.Min(Stats.TotalHealth,
                        Stats.CurrentHealth + Stats.TotalHealthRegen * diff * 0.001f);
                }

                if (Stats.TotalParRegen > 0 && Stats.CurrentPar < Stats.TotalPar && Stats.CurrentPar > 0)
                {
                    Stats.CurrentPar = Math.Min(Stats.TotalPar,
                        Stats.CurrentPar + Stats.TotalParRegen * diff * 0.001f);
                }

                UpdateReplication();
                _healthUpdateTimer = 0;
            }

            if (_statUpdateTimer >= 100)
            {
                UpdateReplication();
                _statUpdateTimer = 0;
            }
        }

        public virtual void UpdateReplication()
        {

        }

        public virtual bool GetTargetableToTeam(TeamId team)
        {
            if (Stats.IsTargetableToTeam.HasFlag(IsTargetableToTeamFlags.TargetableToAll))
            {
                return true;
            }

            if (team == TeamId.TEAM_NEUTRAL)
            {
                return true;
            }

            if (!Stats.IsTargetable)
            {
                return false;
            }

            if (team == Team)
            {
                return !Stats.IsTargetableToTeam.HasFlag(IsTargetableToTeamFlags.NonTargetableAlly);
            }

            return !Stats.IsTargetableToTeam.HasFlag(IsTargetableToTeamFlags.NonTargetableEnemy);
        }

        public virtual void SetTargetableToTeam(TeamId team, bool targetable)
        {
            var dictionary = new Dictionary<TeamId, bool>
            {
                {TeamId.TEAM_NEUTRAL, true},
                {TeamId.TEAM_BLUE, GetTargetableToTeam(TeamId.TEAM_BLUE)},
                {TeamId.TEAM_PURPLE, GetTargetableToTeam(TeamId.TEAM_PURPLE)}
            };

            dictionary[team] = targetable;

            Stats.IsTargetableToTeam = 0;
            if (dictionary[TeamId.TEAM_BLUE] && dictionary[TeamId.TEAM_PURPLE])
            {
                Stats.IsTargetableToTeam = IsTargetableToTeamFlags.TargetableToAll;
                return;
            }

            if (!dictionary[Team])
            {
                Stats.IsTargetableToTeam |= IsTargetableToTeamFlags.NonTargetableAlly;
            }

            if (!dictionary[CustomConvert.GetEnemyTeam(Team)])
            {
                Stats.IsTargetableToTeam |= IsTargetableToTeamFlags.NonTargetableEnemy;
            }
        }

        public virtual void die(ObjAIBase killer)
        {
            setToRemove();
            _game.ObjectManager.StopTargeting(this);

            _game.PacketNotifier.NotifyNpcDie(this, killer);

            var exp = _game.Map.MapGameScript.GetExperienceFor(this);
            var champs = _game.ObjectManager.GetChampionsInRange(this, EXP_RANGE, true);
            //Cull allied champions
            champs.RemoveAll(l => l.Team == Team);

            if (champs.Count > 0)
            {
                float expPerChamp = exp / champs.Count;
                foreach (var c in champs)
                {
                    c.Stats.Experience += expPerChamp;
                    _game.PacketNotifier.NotifyAddXP(c, expPerChamp);
                }
            }

            if (killer != null)
            {
                var cKiller = killer as Champion;

                if (cKiller == null)
                {
                    return;
                }

                var gold = _game.Map.MapGameScript.GetGoldFor(this);
                if (gold <= 0)
                {
                    return;
                }

                cKiller.Stats.Gold += gold;
                if (gold > 0)
                {
                    cKiller.Stats.TotalGold += gold;
                }

                _game.PacketNotifier.NotifyAddGold(cKiller, this, gold);

                if (cKiller.KillDeathCounter < 0)
                {
                    cKiller.ChampionGoldFromMinions += gold;
                    _logger.LogCoreInfo($"Adding gold form minions to reduce death spree: {cKiller.ChampionGoldFromMinions}");
                }

                if (cKiller.ChampionGoldFromMinions >= 50 && cKiller.KillDeathCounter < 0)
                {
                    cKiller.ChampionGoldFromMinions = 0;
                    cKiller.KillDeathCounter += 1;
                }
            }
        }

        public virtual bool isInDistress()
        {
            return false; //return DistressCause;
        }

        public virtual void TakeDamage(ObjAIBase attacker, float damage, DamageType type, DamageSource source,
            DamageText damageText)
        {
            if (Stats.IsInvulnerable)
            {
                damage = 0;
                damageText = DamageText.DAMAGE_TEXT_INVULNERABLE;
            }

            ApiEventManager.OnUnitDamageTaken.Publish(this);

            Stats.CurrentHealth = Math.Max(0.0f, Stats.CurrentHealth - damage);

            if (!IsDead && Stats.CurrentHealth <= 0)
            {
                IsDead = true;
                die(attacker);
            }

            _game.PacketNotifier.NotifyDamageDone(attacker, this, damage, type, damageText);
        }

        public virtual void TakeDamage(ObjAIBase attacker, float damage, DamageType type, DamageSource source,
            bool isCrit)
        {
            var text = DamageText.DAMAGE_TEXT_NORMAL;

            if (isCrit)
            {
                text = DamageText.DAMAGE_TEXT_CRITICAL;
            }

            TakeDamage(attacker, damage, type, source, text);
        }
    }

    public enum UnitAnnounces : byte
    {
        Death = 0x04,
        InhibitorDestroyed = 0x1F,
        InhibitorAboutToSpawn = 0x20,
        InhibitorSpawned = 0x21,
        TurretDestroyed = 0x24,
        SummonerDisconnected = 0x47,
        SummonerReconnected = 0x48
    }

    public enum ClassifyUnit
    {
        ChampionAttackingChampion = 1,
        MinionAttackingChampion = 2,
        MinionAttackingMinion = 3,
        TurretAttackingMinion = 4,
        ChampionAttackingMinion = 5,
        Placeable = 6,
        MeleeMinion = 7,
        CasterMinion = 8,
        SuperOrCannonMinion = 9,
        Turret = 10,
        Champion = 11,
        Inhibitor = 12,
        Nexus = 13,
        Default = 14
    }

    public enum DamageType : byte
    {
        DAMAGE_TYPE_PHYSICAL = 0x0,
        DAMAGE_TYPE_MAGICAL = 0x1,
        DAMAGE_TYPE_TRUE = 0x2
    }

    public enum DamageText : byte
    {
        DAMAGE_TEXT_INVULNERABLE = 0x0,
        DAMAGE_TEXT_DODGE = 0x2,
        DAMAGE_TEXT_CRITICAL = 0x3,
        DAMAGE_TEXT_NORMAL = 0x4,
        DAMAGE_TEXT_MISS = 0x5
    }

    public enum DamageSource
    {
        DAMAGE_SOURCE_RAW,
        DAMAGE_SOURCE_INTERNALRAW,
        DAMAGE_SOURCE_PERIODIC,
        DAMAGE_SOURCE_PROC,
        DAMAGE_SOURCE_REACTIVE,
        DAMAGE_SOURCE_ON_DEATH,
        DAMAGE_SOURCE_SPELL,
        DAMAGE_SOURCE_ATTACK,
        DAMAGE_SOURCE_SPELL_AOE,
        DAMAGE_SOURCE_SPELL_PERSIST
    }

    public enum AttackType : byte
    {
        ATTACK_TYPE_RADIAL,
        ATTACK_TYPE_MELEE,
        ATTACK_TYPE_TARGETED
    }

    public enum MoveOrder
    {
        MOVE_ORDER_MOVE,
        MOVE_ORDER_ATTACKMOVE
    }

    public enum ShieldType : byte
    {
        GreenShield = 0x01,
        MagicShield = 0x02,
        NormalShield = 0x03
    }
}

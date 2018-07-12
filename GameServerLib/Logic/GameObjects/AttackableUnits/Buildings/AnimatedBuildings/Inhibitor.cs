﻿using System;
using System.Timers;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings
{
    public class Inhibitor : ObjAnimatedBuilding
    {
        private Timer _respawnTimer;
        public InhibitorState InhibitorState { get; private set; }
        private const double RESPAWN_TIMER = 5 * 60 * 1000;
        private const double RESPAWN_ANNOUNCE = 1 * 60 * 1000;
        private const float GOLD_WORTH = 50.0f;
        private DateTime _timerStartTime;
        public bool RespawnAnnounced { get; private set; } = true;

        // TODO assists
        public Inhibitor(
            string model,
            TeamId team,
            int collisionRadius = 40,
            float x = 0,
            float y = 0,
            int visionRadius = 0,
            uint netId = 0
        ) : base(model, new Stats.Stats(), collisionRadius, x, y, visionRadius, netId)
        {
            Stats.CurrentHealth = 4000;
            Stats.HealthPoints.BaseValue = 4000;
            InhibitorState = InhibitorState.ALIVE;
            SetTeam(team);
        }

        public override void OnAdded()
        {
            base.OnAdded();
            Game.ObjectManager.AddInhibitor(this);
        }

        public override void Die(AttackableUnit killer)
        {
            var objects = Game.ObjectManager.GetObjects().Values;
            foreach (var obj in objects)
            {
                var u = obj as ObjAiBase;
                if (u != null && u.TargetUnit == this)
                {
                    u.SetTargetUnit(null);
                    u.AutoAttackTarget = null;
                    u.IsAttacking = false;
                    Game.PacketNotifier.NotifySetTarget(u, null);
                    u.HasMadeInitialAttack = false;
                }
            }

            _respawnTimer?.Stop();
            _respawnTimer = new Timer(RESPAWN_TIMER) {AutoReset = false};

            _respawnTimer.Elapsed += (a, b) =>
            {
                Stats.CurrentHealth = Stats.HealthPoints.Total;
                SetState(InhibitorState.ALIVE);
                IsDead = false;
            };
            _respawnTimer.Start();
            _timerStartTime = DateTime.Now;

            if (killer is Champion c)
            {
                c.Stats.Gold += GOLD_WORTH;
                Game.PacketNotifier.NotifyAddGold(c, this, GOLD_WORTH);
            }

            SetState(InhibitorState.DEAD, killer);
            RespawnAnnounced = false;

            base.Die(killer);
        }

        public void SetState(InhibitorState state, GameObject killer = null)
        {
            if (_respawnTimer != null && state == InhibitorState.ALIVE)
            {
                _respawnTimer.Stop();
            }

            InhibitorState = state;
            Game.PacketNotifier.NotifyInhibitorState(this, killer);
        }

        public double GetRespawnTimer()
        {
            var diff = DateTime.Now - _timerStartTime;
            return RESPAWN_TIMER - diff.TotalMilliseconds;
        }

        public override void Update(float diff)
        {
            if (!RespawnAnnounced && InhibitorState == InhibitorState.DEAD && GetRespawnTimer() <= RESPAWN_ANNOUNCE)
            {
                Game.PacketNotifier.NotifyInhibitorSpawningSoon(this);
                RespawnAnnounced = true;
            }

            base.Update(diff);
        }

        public override void SetToRemove()
        {

        }

        public override float GetMoveSpeed()
        {
            return 0;
        }

    }

    public enum InhibitorState : byte
    {
        DEAD = 0x00,
        ALIVE = 0x01
    }
}

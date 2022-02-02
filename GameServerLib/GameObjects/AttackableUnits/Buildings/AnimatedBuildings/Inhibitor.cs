﻿using System;
using System.Numerics;
using System.Timers;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings
{
    public class Inhibitor : ObjAnimatedBuilding, IInhibitor
    {
        private Timer _respawnTimer;
        public LaneID Lane { get; private set; }
        public InhibitorState InhibitorState { get; private set; }
        private const double RESPAWN_TIMER = 5 * 60 * 1000;
        private const double RESPAWN_ANNOUNCE = 1 * 60 * 1000;
        private const float GOLD_WORTH = 50.0f;
        private DateTime _timerStartTime;
        public bool RespawnAnnounced { get; private set; } = true;

        // TODO assists
        public Inhibitor(
            Game game,
            string model,
            LaneID laneId,
            TeamId team,
            int collisionRadius = 40,
            Vector2 position = new Vector2(),
            int visionRadius = 0,
            uint netId = 0
        ) : base(game, model, new Stats.Stats(), collisionRadius, position, visionRadius, netId, team)
        {
            Stats.CurrentHealth = 4000;
            Stats.HealthPoints.BaseValue = 4000;
            InhibitorState = InhibitorState.ALIVE;
            Lane = laneId;
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.ObjectManager.AddInhibitor(this);
        }

        public override void Die(IDeathData data)
        {
            var objects = _game.ObjectManager.GetObjects().Values;
            foreach (var obj in objects)
            {
                var u = obj as IObjAiBase;
                if (u != null && u.TargetUnit == this)
                {
                    u.SetTargetUnit(null, true);
                }
            }

            _respawnTimer?.Stop();
            _respawnTimer = new Timer(RESPAWN_TIMER) { AutoReset = false };

            _respawnTimer.Elapsed += (a, b) =>
            {
                //Todo: Handle inhibitor respawn on MapScript
                Stats.CurrentHealth = Stats.HealthPoints.Total;
                SetState(InhibitorState.ALIVE);
                IsDead = false;
            };
            _respawnTimer.Start();
            _timerStartTime = DateTime.Now;

            if (data.Killer is IChampion c)
            {
                c.Stats.Gold += GOLD_WORTH;
                _game.PacketNotifier.NotifyUnitAddGold(c, this, GOLD_WORTH);
            }

            SetState(InhibitorState.DEAD, data);
            RespawnAnnounced = false;

            base.Die(data);
        }

        public void SetState(InhibitorState state, IDeathData data = null)
        {
            if (_respawnTimer != null && state == InhibitorState.ALIVE)
            {
                _respawnTimer.Stop();
            }

            var opposingTeam = Team == TeamId.TEAM_BLUE ? TeamId.TEAM_PURPLE : TeamId.TEAM_BLUE;
            SetIsTargetableToTeam(opposingTeam, state == InhibitorState.ALIVE);

            InhibitorState = state;
            _game.PacketNotifier.NotifyInhibitorState(this, data);
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
                _game.PacketNotifier.NotifyInhibitorSpawningSoon(this);
                RespawnAnnounced = true;
            }

            base.Update(diff);
        }

        public override void SetToRemove()
        {

        }

    }
}

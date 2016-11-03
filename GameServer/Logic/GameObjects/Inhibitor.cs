using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Inhibitor : Unit
    {
        private System.Timers.Timer RespawnTimer;
        private InhibitorState State;
        private const double RESPAWN_TIMER = 5 * 60 * 1000;
        private const double RESPAWN_ANNOUNCE = 1 * 60 * 1000;
        private const float GOLD_WORTH = 50.0f;
        private DateTime TimerStartTime;
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
        ) : base(model, new BuildingStats(), collisionRadius, x, y, visionRadius, netId)
        {
            stats.CurrentHealth = 4000;
            stats.HealthPoints.BaseValue = 4000;
            State = InhibitorState.Alive;
            SetTeam(team);
        }

        public override void die(Unit killer)
        {
            var objects = _game.Map.GetObjects().Values;
            foreach (var obj in objects)
            {
                var u = obj as Unit;
                if (u != null && u.TargetUnit == this)
                {
                    u.SetTargetUnit(null);
                    u.AutoAttackTarget = null;
                    u.IsAttacking = false;
                    _game.PacketNotifier.notifySetTarget(u, null);
                    u._hasMadeInitialAttack = false;
                }
            }

            if (RespawnTimer != null) //?
                RespawnTimer.Stop();

            RespawnTimer = new System.Timers.Timer(RESPAWN_TIMER) {AutoReset = false};

            RespawnTimer.Elapsed += (a, b) =>
            {
                GetStats().CurrentHealth = GetStats().HealthPoints.Total;
                setState(InhibitorState.Alive);
                IsDead = false;
            };
            RespawnTimer.Start();
            TimerStartTime = DateTime.Now;

            if (killer != null && killer is Champion)
            {
                var c = (Champion)killer;
                c.GetStats().Gold += GOLD_WORTH;
                _game.PacketNotifier.notifyAddGold(c, this, GOLD_WORTH);
            }

            setState(InhibitorState.Dead, killer);
            RespawnAnnounced = false;

            base.die(killer);
        }

        public void setState(InhibitorState state, GameObject killer = null)
        {
            if (RespawnTimer != null && state == InhibitorState.Alive)
                RespawnTimer.Stop();

            State = state;
            _game.PacketNotifier.NotifyInhibitorState(this, killer);
        }

        public InhibitorState getState()
        {
            return State;
        }

        public double getRespawnTimer()
        {
            var diff = DateTime.Now - TimerStartTime;
            return RESPAWN_TIMER - diff.TotalMilliseconds;
        }

        public override void update(long diff)
        {
            if (!RespawnAnnounced && getState() == InhibitorState.Dead && getRespawnTimer() <= RESPAWN_ANNOUNCE)
            {
                _game.PacketNotifier.NotifyInhibitorSpawningSoon(this);
                RespawnAnnounced = true;
            }

            base.update(diff);
        }

        public override void refreshWaypoints()
        {

        }

        public override void setToRemove()
        {

        }

        public override float getMoveSpeed()
        {
            return 0;
        }

    }

    public enum InhibitorState : byte
    {
        Dead = 0x00,
        Alive = 0x01
    }
}

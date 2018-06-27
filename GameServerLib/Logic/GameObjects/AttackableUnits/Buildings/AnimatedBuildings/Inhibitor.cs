using System;
using System.Timers;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Inhibitor : ObjAnimatedBuilding
    {
        private Timer _respawnTimer;
        private InhibitorState _state;
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
        ) : base(model, new Stats(), collisionRadius, x, y, visionRadius, netId)
        {
            Stats.CurrentHealth = 4000;
            Stats.HealthPoints.BaseValue = 4000;
            _state = InhibitorState.ALIVE;
            SetTeam(team);
        }
        public override void OnAdded()
        {
            base.OnAdded();
            _game.ObjectManager.AddInhibitor(this);
        }

        public override void Die(AttackableUnit killer)
        {
            var objects = _game.ObjectManager.GetObjects().Values;
            foreach (var obj in objects)
            {
                var u = obj as ObjAiBase;
                if (u != null && u.TargetUnit == this)
                {
                    u.SetTargetUnit(null);
                    u.AutoAttackTarget = null;
                    u.IsAttacking = false;
                    _game.PacketNotifier.NotifySetTarget(u, null);
                    u.HasMadeInitialAttack = false;
                }
            }

            if (_respawnTimer != null) //?
                _respawnTimer.Stop();

            _respawnTimer = new Timer(RESPAWN_TIMER) {AutoReset = false};

            _respawnTimer.Elapsed += (a, b) =>
            {
                Stats.CurrentHealth = Stats.HealthPoints.Total;
                SetState(InhibitorState.ALIVE);
                IsDead = false;
            };
            _respawnTimer.Start();
            _timerStartTime = DateTime.Now;

            if (killer != null && killer is Champion)
            {
                var c = (Champion)killer;
                c.Stats.Gold += GOLD_WORTH;
                _game.PacketNotifier.NotifyAddGold(c, this, GOLD_WORTH);
            }

            SetState(InhibitorState.DEAD, killer);
            RespawnAnnounced = false;

            base.Die(killer);
        }

        public void SetState(InhibitorState state, GameObject killer = null)
        {
            if (_respawnTimer != null && state == InhibitorState.ALIVE)
                _respawnTimer.Stop();

            _state = state;
            _game.PacketNotifier.NotifyInhibitorState(this, killer);
        }

        public InhibitorState GetState()
        {
            return _state;
        }

        public double GetRespawnTimer()
        {
            var diff = DateTime.Now - _timerStartTime;
            return RESPAWN_TIMER - diff.TotalMilliseconds;
        }

        public override void Update(float diff)
        {
            if (!RespawnAnnounced && GetState() == InhibitorState.DEAD && GetRespawnTimer() <= RESPAWN_ANNOUNCE)
            {
                _game.PacketNotifier.NotifyInhibitorSpawningSoon(this);
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

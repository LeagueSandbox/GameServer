using System;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Inhibitor : ObjAnimatedBuilding
    {
        private System.Timers.Timer RespawnTimer;
        public InhibitorState State { get; private set; }
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
        ) : base(model, collisionRadius, x, y, visionRadius, netId)
        {
            State = InhibitorState.Alive;
            SetTeam(team);
            Stats.IsInvulnerable = true;
            Stats.IsTargetableToTeam = IsTargetableToTeamFlags.NonTargetableEnemy;
        }

        public override void OnAdded()
        {
            base.OnAdded();
            _game.ObjectManager.AddInhibitor(this);
        }

        public override void die(ObjAIBase killer)
        {
            var objects = _game.ObjectManager.GetObjects().Values;
            foreach (var obj in objects)
            {
                var u = obj as ObjAIBase;
                if (u != null && u.TargetUnit == this)
                {
                    u.SetTargetUnit(null);
                    u.AutoAttackTarget = null;
                    u.IsAttacking = false;
                    _game.PacketNotifier.NotifySetTarget(u, null);
                    u._hasMadeInitialAttack = false;
                }
            }

            RespawnTimer?.Stop();

            RespawnTimer = new System.Timers.Timer(RESPAWN_TIMER) {AutoReset = false};

            RespawnTimer.Elapsed += (a, b) =>
            {
                Stats.CurrentHealth = Stats.TotalHealth;
                setState(InhibitorState.Alive);
                IsDead = false;
            };
            RespawnTimer.Start();
            TimerStartTime = DateTime.Now;

            if (killer is Champion c)
            {
                c.Stats.Gold += GOLD_WORTH;
                c.Stats.TotalGold += GOLD_WORTH;

                _game.PacketNotifier.NotifyAddGold(c, this, GOLD_WORTH);
            }

            setState(InhibitorState.Dead, killer);
            RespawnAnnounced = false;

            base.die(killer);
        }

        public override void UpdateReplication()
        {
            ReplicationManager.UpdateFloat(Stats.CurrentHealth, 1, 0);
            ReplicationManager.UpdateBool(Stats.IsInvulnerable, 1, 1);
            ReplicationManager.UpdateBool(Stats.IsTargetable, 5, 0);
            ReplicationManager.UpdateUint((uint)Stats.IsTargetableToTeam, 5, 1);
        }

        public void setState(InhibitorState state, GameObject killer = null)
        {
            if (RespawnTimer != null && state == InhibitorState.Alive)
            {
                RespawnTimer.Stop();
            }

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

        public override void update(float diff)
        {
            if (!RespawnAnnounced && getState() == InhibitorState.Dead && getRespawnTimer() <= RESPAWN_ANNOUNCE)
            {
                _game.PacketNotifier.NotifyInhibitorSpawningSoon(this);
                RespawnAnnounced = true;
            }

            base.update(diff);
        }


        public override void setToRemove()
        {

        }

        public override void TakeDamage(ObjAIBase attacker, float damage, DamageType type, DamageSource source, DamageText damageText)
        {
            if (attacker is Champion)
            {
                damage *= 0.85f;
            }

            base.TakeDamage(attacker, damage, type, source, damageText);
        }
    }

    public enum InhibitorState : byte
    {
        Dead = 0x00,
        Alive = 0x01
    }
}

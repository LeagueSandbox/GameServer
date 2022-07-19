using System;
using System.Numerics;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class Fountain
    {
        private Game _game;
        private const float PERCENT_MAX_HEALTH_HEAL = 0.15f;
        private const float PERCENT_MAX_MANA_HEAL = 0.15f;
        private const float HEAL_FREQUENCY = 1000f;
        private float _fountainSize;
        private float _healTickTimer;
        public Vector2 Position { get; set; }
        public TeamId Team { get; set; }

        public Fountain(Game game, TeamId team, Vector2 position, float size)
        {
            _game = game;
            Position = position;
            _fountainSize = size;
            _healTickTimer = 0;
            Team = team;
        }

        public void Update(float diff)
        {
            _healTickTimer += diff;
            if (_healTickTimer < HEAL_FREQUENCY)
            {
                return;
            }

            _healTickTimer = 0;

            var champions = _game.ObjectManager.GetChampionsInRange(Position, _fountainSize, true);
            foreach (var champion in champions)
            {
                if (champion.Team != Team)
                {
                    continue;
                }

                var hp = champion.Stats.CurrentHealth;
                var maxHp = champion.Stats.HealthPoints.Total;
                champion.Stats.CurrentHealth = Math.Min(hp + maxHp * PERCENT_MAX_HEALTH_HEAL, maxHp);

                if ((byte)champion.Stats.ParType > 1)
                {
                    continue;
                }

                var mp = champion.Stats.CurrentMana;
                var maxMp = champion.Stats.ManaPoints.Total;
                champion.Stats.CurrentMana = Math.Min(mp + maxMp * PERCENT_MAX_MANA_HEAL, maxMp);
                _game.ProtectionManager.HandleFountainProtection(champion);
            }
        }
    }
}

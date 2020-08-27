using System;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class Fountain
    {
        private const float PERCENT_MAX_HEALTH_HEAL = 0.15f;
        private const float PERCENT_MAX_MANA_HEAL = 0.15f;
        private const float HEAL_FREQUENCY = 1000f;
        private float _x;
        private float _y;
        private float _fountainSize;
        private float _healTickTimer;
        private TeamId _team;
        private Game _game;

        public Fountain(Game game, TeamId team, float x, float y, float size)
        {
            _game = game;
            _x = x;
            _y = y;
            _fountainSize = size;
            _healTickTimer = 0;
            _team = team;
        }

        internal void Update(float diff)
        {
            _healTickTimer += diff;
            if (_healTickTimer < HEAL_FREQUENCY)
            {
                return;
            }

            _healTickTimer = 0;

            var champions = _game.ObjectManager.GetChampionsInRange(_x, _y, _fountainSize, true);
            foreach (var champion in champions)
            {
                if (champion.Team != _team)
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
            }
        }
    }
}

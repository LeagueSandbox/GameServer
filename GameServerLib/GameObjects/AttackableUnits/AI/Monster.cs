using System;
using System.Linq;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace LeagueSandbox.GameServer.GameObjects
{
    public class Monster : Minion, IMonster
    {
        public IMonsterCamp Camp { get; private set; }
        public string SpawnAnimation { get; private set; }
        
        private bool _hasBeenAdded;
        private float _additionTimer;

        public Monster(
            Game game,
            string name,
            string model,
            Vector2 position,
            Vector3 faceDirection,
            IMonsterCamp monsterCamp,
            TeamId team = TeamId.TEAM_NEUTRAL,
            uint netId = 0,
            string spawnAnimation = "",
            bool isTargetable = true,
            bool ignoresCollision = false,
            string aiScript = "",
            int damageBonus = 0,
            int healthBonus = 0,
            int initialLevel = 1
        ) : base
            (
                game, null, position, model, name,
                netId, team, 0, ignoresCollision, isTargetable,
                null, aiScript, damageBonus, healthBonus, initialLevel
            )
        {
            _additionTimer = monsterCamp.SpawnDuration;
            _hasBeenAdded = false;
            SetStatus(StatusFlags.Targetable, false);
            SetStatus(StatusFlags.Invulnerable, true);
            SetStatus(StatusFlags.Ghosted, true);
            SetStatus(StatusFlags.CanMove, false);
            SetStatus(StatusFlags.CanAttack, false);
            SetStatus(StatusFlags.CanCast, false);

            Camp = monsterCamp;
            Team = team;
            SpawnAnimation = spawnAnimation;
            IsTargetable = true;
            IgnoresCollision = false;
            FaceDirection(faceDirection);
            IsTargetable = isTargetable;
            IgnoresCollision = ignoresCollision;
        }

        public override void Update(float diff)
        {
            if (!_hasBeenAdded)
            {
                _additionTimer -= diff;
                if(_additionTimer <= 0)
                {
                    SetStatus(StatusFlags.Targetable, true);
                    SetStatus(StatusFlags.Invulnerable, false);
                    SetStatus(StatusFlags.Ghosted, false);
                    SetStatus(StatusFlags.CanMove, true);
                    SetStatus(StatusFlags.CanAttack, true);
                    SetStatus(StatusFlags.CanCast, true);
                    _hasBeenAdded = true;
                }
            }
            base.Update(diff);
        }
    }
}


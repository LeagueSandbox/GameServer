using System.Linq;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Stats;
using GameServerCore.Enums;
using System.Numerics;
using LeagueSandbox.GameServer.Logging;
using GameServerCore.Scripting.CSharp;
using System;
using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class Minion : ObjAiBase, IMinion
    {
        /// <summary>
        /// Unit which spawned this minion.
        /// </summary>
        public IObjAiBase Owner { get; }
        /// <summary>
        /// Whether or not this minion is considered a clone of its owner.
        /// </summary>
        public bool IsClone { get; protected set; }
        /// <summary>
        /// Whether or not this minion should ignore collisions.
        /// </summary>
        public bool IgnoresCollision { get; protected set; }
        /// <summary>
        /// Whether or not this minion is considered a ward.
        /// </summary>
        public bool IsWard { get; protected set; }
        /// <summary>
        /// Whether or not this minion is a LaneMinion.
        /// </summary>
        public bool IsLaneMinion { get; protected set; }
        /// <summary>
        /// Whether or not this minion is considered a pet.
        /// </summary>
        public bool IsPet { get; protected set; }
        /// <summary>
        /// Whether or not this minion is targetable at all.
        /// </summary>
        public bool IsTargetable { get; protected set; }
        /// <summary>
        /// Internal name of the minion.
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// Only unit which is allowed to see this minion.
        /// </summary>
        public IObjAiBase VisibilityOwner { get; }

        //TODO: Implement these variables
        public int DamageBonus { get; protected set; }
        public int HealthBonus { get; protected set; }
        public int InitialLevel { get; protected set; }

        public Minion(
            Game game,
            IObjAiBase owner,
            Vector2 position,
            string model,
            string name,
            uint netId = 0,
            TeamId team = TeamId.TEAM_NEUTRAL,
            int skinId = 0,
            bool ignoreCollision = false,
            bool targetable = true,
            bool isWard = false,
            IObjAiBase visibilityOwner = null,
            string aiScript = "",
            int damageBonus = 0,
            int healthBonus = 0,
            int initialLevel = 1
        ) : base(game, model, new Stats.Stats(), 40, position, 1100, skinId, netId, team, aiScript)
        {
            Name = name;
            Owner = owner;

            IsPet = false;
            if (Owner != null)
            {
                IsPet = true;
                if (model == Owner.Model) // Placeholder, should be changed
                {
                    IsClone = true;
                }
            }

            IsLaneMinion = false;
            IsWard = isWard;
            IgnoresCollision = ignoreCollision;
            if (IgnoresCollision)
            {
                SetStatus(StatusFlags.Ghosted, true);
            }

            IsTargetable = targetable;
            if (!IsTargetable)
            {
                SetStatus(StatusFlags.Targetable, false);
            }

            VisibilityOwner = visibilityOwner;
            DamageBonus = damageBonus;
            HealthBonus = healthBonus;
            InitialLevel = initialLevel;
            MoveOrder = OrderType.Stop;

            Replication = new ReplicationMinion(this);
        }
    }
}
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.Stats;
using LeagueSandbox.GameServer.Logic.Items;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class BaseTurret : ObjAIBase
    {
        public string Name { get; private set; }
        protected float globalGold = 250.0f;
        protected float globalExp = 0.0f;

        public BaseTurret(
            string name,
            string model,
            float x = 0,
            float y = 0,
            TeamId team = TeamId.TEAM_BLUE,
            uint netId = 0
        ) : base(model, 50, x, y, 1200, netId)
        {
            Name = name;
            SetTeam(team);
            Inventory = InventoryManager.CreateInventory(this);
            Stats.LoadStats(model, CharData);
        }

        public override void UpdateReplication()
        {
            ReplicationManager.UpdateFloat(Stats.TotalPar, 1, 0);
            ReplicationManager.UpdateFloat(Stats.CurrentPar, 1, 1);
            ReplicationManager.UpdateUint((uint)Stats.ActionState, 1, 2);
            ReplicationManager.UpdateBool(Stats.IsMagicImmune, 1, 3);
            ReplicationManager.UpdateBool(Stats.IsInvulnerable, 1, 4);
            ReplicationManager.UpdateBool(Stats.IsPhysicalImmune, 1, 5);
            ReplicationManager.UpdateBool(Stats.IsLifestealImmune, 1, 6);
            ReplicationManager.UpdateFloat(Stats.BaseAttackDamage, 1, 7);
            ReplicationManager.UpdateFloat(Stats.TotalArmor, 1, 9);
            ReplicationManager.UpdateFloat(Stats.TotalMagicResist, 1, 10);
            ReplicationManager.UpdateFloat(Stats.PercentAttackSpeedMod + Stats.PercentAttackSpeedDebuff, 1, 11);
            ReplicationManager.UpdateFloat(Stats.FlatAttackDamageMod, 1, 12);
            ReplicationManager.UpdateFloat(Stats.PercentAttackDamageMod, 1, 13);
            ReplicationManager.UpdateFloat(Stats.TotalAbilityPower, 1, 14);
            ReplicationManager.UpdateFloat(Stats.TotalHealthRegen, 1, 15);
            ReplicationManager.UpdateFloat(Stats.CurrentHealth, 3, 0);
            ReplicationManager.UpdateFloat(Stats.TotalHealth, 3, 1);
            ReplicationManager.UpdateFloat(0, 3, 2); // FlatSightRangeMod
            ReplicationManager.UpdateFloat(0, 3, 3); // PercentSightRangeMod
            ReplicationManager.UpdateFloat(Stats.TotalMovementSpeed, 3, 4);
            ReplicationManager.UpdateFloat(Stats.TotalSize, 3, 5);
            ReplicationManager.UpdateBool(Stats.IsTargetable, 5, 0);
            ReplicationManager.UpdateUint((uint)Stats.IsTargetableToTeam, 5, 1);
        }

        public void CheckForTargets()
        {
            var objects = _game.ObjectManager.GetObjects();
            ObjAIBase nextTarget = null;
            var nextTargetPriority = 14;

            foreach (var it in objects)
            {
                var u = it.Value as ObjAIBase;

                if (u == null || u is BaseTurret || u.IsDead || u.Team == Team || GetDistanceTo(u) > Stats.TotalAttackRange)
                {
                    continue;
                }

                // Note: this method means that if there are two champions within turret range,
                // The player who was added to the game first will always be targeted before the others
                if (TargetUnit == null)
                {
                    var priority = (int)ClassifyTarget(u);
                    if (priority < nextTargetPriority)
                    {
                        nextTarget = u;
                        nextTargetPriority = priority;
                    }
                }
                else
                {
                    var targetIsChampion = TargetUnit as Champion;

                    // Is the current target a champion? If it is, don't do anything
                    if (targetIsChampion != null)
                    {
                        // Find the next champion in range targeting an enemy champion who is also in range
                        var enemyChamp = u as Champion;
                        if (enemyChamp != null && enemyChamp.TargetUnit != null)
                        {
                            var enemyChampTarget = enemyChamp.TargetUnit as Champion;
                            if (enemyChampTarget != null && // Enemy Champion is targeting an ally
                                enemyChamp.GetDistanceTo(enemyChampTarget) <= enemyChamp.Stats.TotalAttackRange && // Enemy within range of ally
                                GetDistanceTo(enemyChampTarget) <= Stats.TotalAttackRange) // Enemy within range of this turret
                            {
                                nextTarget = enemyChamp; // No priority required
                                break;
                            }
                        }
                    }
                }
            }
            if (nextTarget != null)
            {
                TargetUnit = nextTarget;
                _game.PacketNotifier.NotifySetTarget(this, nextTarget);
            }
        }

        public override void update(float diff)
        {
            if (!IsAttacking)
            {
                CheckForTargets();
            }

            // Lose focus of the unit target if the target is out of range
            if (TargetUnit != null && GetDistanceTo(TargetUnit) > Stats.TotalAttackRange)
            {
                TargetUnit = null;
                _game.PacketNotifier.NotifySetTarget(this, null);
            }

            base.update(diff);
        }

        public override void die(ObjAIBase killer)
        {
            foreach (var player in _game.ObjectManager.GetAllChampionsFromTeam(killer.Team))
            {
                var goldEarn = globalGold;

                // Champions in Range within TURRET_RANGE * 1.5f will gain 150% more (obviously)
                if (player.GetDistanceTo(this) <= Stats.TotalAttackRange * 1.5f && !player.IsDead)
                {
                    goldEarn = globalGold * 2.5f;
                    if (globalExp > 0)
                    {
                        player.Stats.Experience += globalExp;
                    }
                }


                player.Stats.Gold += goldEarn;
                if (goldEarn > 0)
                {
                    player.Stats.TotalGold += goldEarn;
                }
                _game.PacketNotifier.NotifyAddGold(player, this, goldEarn);
            }
            _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.TurretDestroyed, this, killer);
            base.die(killer);
        }

        public override void refreshWaypoints()
        {
        }
    }
}

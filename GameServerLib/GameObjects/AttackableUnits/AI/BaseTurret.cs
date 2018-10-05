using System.Text;
using Force.Crc32;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Items;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class BaseTurret : ObjAiBase, IBaseTurret
    {
        public string Name { get; private set; }
        protected float _globalGold = 250.0f;
        protected float _globalExp = 0.0f;

        public uint ParentNetId { get; private set; }

        public BaseTurret(
            Game game,
            string name,
            string model,
            float x = 0,
            float y = 0,
            TeamId team = TeamId.TEAM_BLUE,
            uint netId = 0
        ) : base(game, model, new Stats.Stats(), 50, x, y, 1200, netId)
        {
            ParentNetId = Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(name)) | 0xFF000000;
            Name = name;
            SetTeam(team);
            Inventory = InventoryManager.CreateInventory();
            Replication = new ReplicationAiTurret(this);
        }

        public void CheckForTargets()
        {
            var objects = _game.ObjectManager.GetObjects();
            IAttackableUnit nextTarget = null;
            var nextTargetPriority = 14;

            foreach (var it in objects)
            {
                if (!(it.Value is IAttackableUnit u) ||
                    u.IsDead ||
                    u.Team == Team || 
                    GetDistanceTo(u) > Stats.Range.Total)
                    continue;

                // Note: this method means that if there are two champions within turret range,
                // The player to have been added to the game first will always be targeted before the others
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
                    // Is the current target a champion? If it is, don't do anything
                    if (!(TargetUnit is IChampion))
                        continue;
                    // Find the next champion in range targeting an enemy champion who is also in range
                    if (!(u is IChampion enemyChamp) || enemyChamp.TargetUnit == null)
                        continue;
                    if (!(enemyChamp.TargetUnit is IChampion enemyChampTarget) ||
                        enemyChamp.GetDistanceTo(enemyChampTarget) > enemyChamp.Stats.Range.Total ||
                        GetDistanceTo(enemyChampTarget) > Stats.Range.Total)
                        continue;
                    nextTarget = enemyChamp; // No priority required
                    break;
                }
            }

            if (nextTarget == null)
                return;
            TargetUnit = nextTarget;
            _game.PacketNotifier.NotifySetTarget(this, nextTarget);
        }

        public override void Update(float diff)
        {
            if (!IsAttacking)
            {
                CheckForTargets();
            }

            // Lose focus of the unit target if the target is out of range
            if (TargetUnit != null && GetDistanceTo(TargetUnit) > Stats.Range.Total)
            {
                TargetUnit = null;
                _game.PacketNotifier.NotifySetTarget(this, null);
            }

            base.Update(diff);
            Replication.Update();
        }

        public override void Die(IAttackableUnit killer)
        {
            foreach (var player in _game.ObjectManager.GetAllChampionsFromTeam(killer.Team))
            {
                var goldEarn = _globalGold;

                // Champions in Range within TURRET_RANGE * 1.5f will gain 150% more (obviously)
                if (player.GetDistanceTo(this) <= Stats.Range.Total * 1.5f && !player.IsDead)
                {
                    goldEarn = _globalGold * 2.5f;
                    if(_globalExp > 0)
                        player.Stats.Experience += _globalExp;
                }


                player.Stats.Gold += goldEarn;
                _game.PacketNotifier.NotifyAddGold(player, this, goldEarn);
            }

            _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.TURRET_DESTROYED, this, killer);
            base.Die(killer);
        }

        public override void RefreshWaypoints()
        {
        }

        public override float GetMoveSpeed()
        {
            return 0;
        }
    }
}

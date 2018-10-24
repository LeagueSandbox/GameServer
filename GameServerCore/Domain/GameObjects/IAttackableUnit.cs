using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IAttackableUnit : IGameObject
    {
        bool IsModelUpdated { get; set; }
        bool IsDead { get; }
        string Model { get; set; }
        int KillDeathCounter { get; }
        int MinionCounter { get; }
        IReplication Replication { get; }
        IStats Stats { get; }
        IInventoryManager Inventory { get; }

        void TakeDamage(IAttackableUnit attacker, Damage damage);
        void TakeDamage(IAttackableUnit attacker, Damage damage, DamageText damageText);
        void Die(IAttackableUnit killer);
    }
}

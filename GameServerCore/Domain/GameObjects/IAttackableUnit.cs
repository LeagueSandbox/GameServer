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

        void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source, bool isCrit);
        void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source, DamageText damageText);
        void Die(IAttackableUnit killer);
    }
}

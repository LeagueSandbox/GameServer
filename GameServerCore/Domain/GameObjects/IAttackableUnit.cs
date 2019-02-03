using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IAttackableUnit : IGameObject
    {
        bool IsDead { get; }
        bool IsModelUpdated { get; set; }
        int KillDeathCounter { get; set; }
        int MinionCounter { get; }
        string Model { get; }
        IInventoryManager Inventory { get; }
        IReplication Replication { get; }
        IStats Stats { get; }

        void Die(IAttackableUnit killer);
        void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source, bool isCrit);
        void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source, DamageText damageText);

        bool IsInDistress();
        bool ChangeModel(string model);
    }
}

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
        IReplication Replication { get; }
        IStats Stats { get; }
        void Die(IAttackableUnit killer);
        int[] ShieldAmount { get; }
        
        void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source, bool isCrit);
        void TakeDamage(IAttackableUnit attacker, float damage, DamageType type, DamageSource source, DamageText damageText);
        public void SetIsTargetableToTeam(TeamId team, bool targetable);
        void ApplyShield(float amount, ShieldType shieldType, bool noFade);

        bool IsInDistress();
        bool ChangeModel(string model);
    }
}

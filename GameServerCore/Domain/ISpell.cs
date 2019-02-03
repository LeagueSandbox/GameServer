using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface ISpell: IUpdate
    {
        IChampion Owner { get; }
        byte Level { get; }
        byte Slot { get; }
        float CastTime { get; }
        string SpellName { get; }
        SpellState State { get; }
        float CurrentCooldown { get; }
        float CurrentCastTime { get; }
        float CurrentChannelDuration { get; }
        uint FutureProjNetId { get; }
        uint SpellNetId { get; }
        IAttackableUnit Target { get; }
        float X { get; }
        float Y { get; }
        float X2 { get; }
        float Y2 { get; }
        ISpellData SpellData { get; }

        bool Cast(float x, float y, float x2, float y2, IAttackableUnit u);
        int GetId();
        float GetCooldown();
        void LowerCooldown(float lowerValue);
        void Deactivate();
        void ApplyEffects(IAttackableUnit u, IProjectile p);
        void LevelUp();
        void AddProjectile(string nameMissile, float toX, float toY, bool isServerOnly = false);
        void AddProjectileTarget(string nameMissile, ITarget target, bool isServerOnly = false);
        void AddLaser(string effectName, float toX, float toY, bool affectAsCastIsOver = true);
        void AddCone(string effectName, float toX, float toY, float angleDeg, bool affectAsCastIsOver = true);
        void SpellAnimation(string animName, IAttackableUnit target);
    }
}

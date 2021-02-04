using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects.Spell
{
    public interface ISpell: IUpdate
    {
        ICastInfo CastInfo { get; }
        float CurrentCooldown { get; }
        float CurrentCastTime { get; }
        float CurrentChannelDuration { get; }
        float CurrentDelayTime { get; }
        bool HasEmptyScript { get; }
        Dictionary<uint, IProjectile> Projectiles { get; }
        bool Toggle { get; }
        ISpellData SpellData { get; }
        string SpellName { get; }
        SpellState State { get; }

        void AddCone(string effectName, Vector2 targetPos, float angleDeg, HitResult hitResult = HitResult.HIT_Normal, bool affectAsCastIsOver = true);
        void AddLaser(string effectName, Vector2 targetPos, HitResult hitResult = HitResult.HIT_Normal, bool affectAsCastIsOver = true);
        void AddProjectile(string nameMissile, Vector2 startPos, Vector2 endPos, HitResult hitResult = HitResult.HIT_Normal, bool isServerOnly = false);
        void AddProjectileTarget(string nameMissile, IAttackableUnit target, HitResult hitResult = HitResult.HIT_Normal, bool isServerOnly = false);
        void ApplyEffects(IAttackableUnit u, IProjectile p);
        bool Cast(Vector2 start, Vector2 end, IAttackableUnit unit = null);
        void Deactivate();
        int GetId();
        float GetCooldown();
        string GetStringForSlot();
        void LevelUp();
        void LowerCooldown(float lowerValue);
        void ResetSpellDelay();
        void SetCooldown(float newCd);
        void SetLevel(byte toLevel);
        void SetSpellState(SpellState state);
        void SpellAnimation(string animName, IAttackableUnit target);
    }
}

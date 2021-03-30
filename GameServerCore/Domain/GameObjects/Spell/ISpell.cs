using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects.Spell
{
    public interface ISpell: IUpdate
    {
        IObjAiBase Owner { get; }
        byte Level { get; }
        byte Slot { get; }
        float CastTime { get; }
        string SpellName { get; }
        SpellState State { get; }
        float CurrentCooldown { get; }
        float CurrentCastTime { get; }
        float CurrentChannelDuration { get; }
        Dictionary<uint, ISpellMissile> Projectiles { get; }
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
        void ApplyEffects(IAttackableUnit u, ISpellMissile p);
        void LevelUp();
        void SetLevel(byte toLevel);
        void AddProjectile(string nameMissile, Vector2 startPos, Vector2 endPos, bool isServerOnly = false);
        void AddProjectileTarget(string nameMissile, IAttackableUnit target, bool isServerOnly = false);
        void AddLaser(string effectName, Vector2 targetPos, bool affectAsCastIsOver = true);
        void AddCone(string effectName, Vector2 targetPos, float angleDeg, bool affectAsCastIsOver = true);
        void SpellAnimation(string animName, IAttackableUnit target);
    }
}

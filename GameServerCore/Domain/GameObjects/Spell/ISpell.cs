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

        /// <summary>
        /// Creates a area of effect cone at this spell's owner position pointing towards the given target position that will act as follows:
        /// ApplyEffect for each unit that has not been affected yet within the area.
        /// If affectAsCastIsOver = true: when the spell origin has finished casting, despawns after doing one area of effect check.
        /// If false: performs continuous area of effect checks until manually SetToRemove.
        /// </summary>
        /// <param name="effectName">Internal name of the cone to spawn. Required for cone features.</param>
        /// <param name="target">Position the area of effect will point towards (from the owner's position).</param>
        /// <param name="hitResult">How the damage applied by this area of effect should be shown to clients.</param>
        /// <param name="affectAsCastIsOver">Whether or not the area of effect will last until its origin spell is finished casting. False = lasts forever (or until something calls SetToRemove for it manually, likely via spell script).</param>
        /// <returns>Newly created area of effect cone with the given functionality.</returns>
        IProjectile AddCone(string effectName, Vector2 target, float angleDeg, HitResult hitResult = HitResult.HIT_Normal, bool affectAsCastIsOver = true);
        /// <summary>
        /// Creates a area of effect rectangle at this spell's owner position pointing towards the given target position that will act as follows:
        /// ApplyEffect for each unit that has not been affected yet within the area.
        /// If affectAsCastIsOver = true: when the spell origin has finished casting, despawns after doing one area of effect check.
        /// If false: performs continuous area of effect checks until manually SetToRemove.
        /// </summary>
        /// <param name="effectName">Internal name of the laser to spawn. Required for laser features.</param>
        /// <param name="target">Position the area of effect will point towards (from the owner's position).</param>
        /// <param name="hitResult">How the damage applied by this area of effect should be shown to clients.</param>
        /// <param name="affectAsCastIsOver">Whether or not the area of effect will last until its origin spell is finished casting. False = lasts forever (or until something calls SetToRemove for it manually, likely via spell script).</param>
        /// <returns>Newly created area of effect rectangle with the given functionality.</returns>
        IProjectile AddLaser(string effectName, Vector2 target, HitResult hitResult = HitResult.HIT_Normal, bool affectAsCastIsOver = true);
        /// <summary>
        /// Creates a line missile with the specified properties.
        /// </summary>
        IProjectile AddProjectile(ICastInfo castInfo);
        /// <summary>
        /// Creates a single-target missile at the specified cast position that will move as follows: Owner.Position -> target.Position. Despawns when Position = target.Position
        /// </summary>
        /// <param name="nameMissile">Internal name of the missile to spawn. Required for missile features.</param>
        /// <param name="castPos">Position the missile will spawn at.</param>
        /// <param name="target">Unit the missile will move towards. Once hit, this missile will despawn (unless it has bounces left).</param>
        /// <param name="hitResult">How the damage applied by this projectile should be shown to clients.</param>
        /// <param name="isServerOnly">Whether or not this missile will only spawn server-side.</param>
        /// <param name="overrideCastPosition">Whether or not to override default cast position behavior with the given cast position.</param>
        /// <returns>Newly created missile with the given functionality.</returns>
        IProjectile AddProjectileTarget(string nameMissile, Vector3 castPos, IAttackableUnit target, HitResult hitResult = HitResult.HIT_Normal, bool isServerOnly = false, bool overrideCastPosition = false);
        void ApplyEffects(IAttackableUnit u, IProjectile p);
        /// <summary>
        /// Removes the given projectile instance from this spell's dictionary of projectiles. Will automatically SetToRemove the projectile.
        /// </summary>
        /// <param name="p">Projectile to remove.</param>
        void RemoveProjectile(IProjectile p);
        /// <summary>
        /// Called when the character casts this spell. Initializes the CastInfo for this spell and begins casting.
        /// </summary>
        bool Cast(Vector2 start, Vector2 end, IAttackableUnit unit = null);
        /// <summary>
        /// Called when a script manually casts this spell.
        /// </summary>
        bool Cast(ICastInfo castInfo, bool cast);
        void Deactivate();
        int GetId();
        float GetCooldown();
        /// <summary>
        /// Gets the cast range for this spell (based on level).
        /// </summary>
        /// <returns>Cast range based on level.</returns>
        float GetCurrentCastRange();
        string GetStringForSlot();
        void LevelUp();
        void LowerCooldown(float lowerValue);
        void ResetSpellDelay();
        void SetCooldown(float newCd);
        void SetLevel(byte toLevel);
        void SetSpellState(SpellState state);
        void SetSpellToggle(bool toggle);
    }
}

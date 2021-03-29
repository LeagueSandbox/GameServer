using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public class SpellScriptMetadata : ISpellScriptMetadata
    {
        public string AutoAuraBuffName { get; set; } = "";

        // TODO: Replace string with empty event class.
        public string AutoBuffActivateEvent { get; set; } = "";

        public float[] AutoCooldownByLevel { get; set; } = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

        public string AutoItemActivateEffect { get; set; } = "";

        public float[] AutoTargetDamageByLevel { get; set; } = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };

        public float CastTime { get; set; } = 0.0f;
        public bool CastingBreaksStealth { get; set; } = false;

        public IMissileParameters MissileParameters { get; set; } = null;

        public ISectorParameters SectorParameters { get; set; } = null;

        /// <summary>
        /// Determines how how long the spell should be channeled (overrides content based channel duration). Triggers on channel (and post) if value is above 0.
        /// </summary>
        public float ChannelDuration { get; set; } = 0.0f;

        public bool DoOnPreDamageInExpirationOrder { get; set; } = false;
        public bool DoesntBreakShields { get; set; } = false;
        // TODO: Find a use for this.
        public bool IsDamagingSpell { get; set; } = false;
        public bool IsDeathRecapSource { get; set; } = false;
        public bool IsDebugMode { get; set; } = false;
        public bool IsPetDurationBuff { get; set; } = false;
        public bool IsNonDispellable { get; set; } = false;
        public bool NotSingleTargetSpell { get; set; } = false;

        // Never appears below 2?
        public int OnPreDamagePriority { get; set; } = 0;

        public bool OverrideCooldownCheck { get; set; } = false;
        public bool PermeatesThroughDeath { get; set; } = false;
        public bool PersistsThroughDeath { get; set; } = false;

        public string PopupMessage1 { get; set; } = "";

        public float SetSpellDamageRatio { get; set; } = 0.0f;
        public float SpellDamageRatio { get; set; } = 0.0f;

        // TODO: Verify if this can be removed.
        public string[] SpellFXOverrideSkins { get; set; } = { "", "" };

        public int SpellToggleSlot { get; set; } = 0;

        // TODO: Verify if this can be removed.
        public string[] SpellVOOverrideSkins { get; set; } = { "" };

        /// <summary>
        /// Determines whether or not the spell stops movement and triggers spell casts (and post).
        /// Usually should not be true if the spell is an item active, summoner spell, missile spell, or otherwise purely buff related spell.
        /// </summary>
        public bool TriggersSpellCasts { get; set; } = false;
    }

    public class MissileParameters : IMissileParameters
    {
        public bool CanHitSameTarget { get; set; } = false;
        public bool CanHitSameTargetConsecutively { get; set; } = false;

        public int MaximumHits { get; set; } = 0;

        public MissileType Type { get; set; } = MissileType.None;
    }

    public class SectorParameters : ISectorParameters
    {
        public bool CanHitSameTarget { get; set; } = false;
        public bool CanHitSameTargetConsecutively { get; set; } = false;

        public int MaximumHits { get; set; } = 0;

        public SectorType Type { get; set; } = SectorType.None;
    }
}
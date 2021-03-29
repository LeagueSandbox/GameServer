using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public interface ISpellScriptMetadata
    {
        string AutoAuraBuffName { get; }

        string AutoBuffActivateAttachBoneName { get; }
        string AutoBuffActivateAttachBoneName2 { get; }
        string AutoBuffActivateAttachBoneName3 { get; }
        string AutoBuffActivateAttachBoneName4 { get; }

        string AutoBuffActivateEffect { get; }
        string AutoBuffActivateEffect2 { get; }
        string AutoBuffActivateEffect3 { get; }
        string AutoBuffActivateEffect4 { get; }
        string AutoBuffActivateEffectFlags { get; }

        string AutoBuffActivateEvent { get; }

        float[] AutoCooldownByLevel { get; }

        string AutoItemActivateEffect { get; }

        float[] AutoTargetDamageByLevel { get; }

        float CastTime { get; }
        bool CastingBreaksStealth { get; }

        IMissileParameters MissileParameters { get; }

        ISectorParameters SectorParameters { get; }

        /// <summary>
        /// Determines how how long the spell should be channeled (overrides content based channel duration). Triggers on channel (and post) if value is above 0.
        /// </summary>
        float ChannelDuration { get; }

        bool DoOnPreDamageInExpirationOrder { get; }
        bool DoesntBreakShields { get; }
        bool IsDamagingSpell { get; }
        bool IsDeathRecapSource { get; }
        bool IsDebugMode { get; }
        bool IsPetDurationBuff { get; }
        bool IsNonDispellable { get; }

        bool NotSingleTargetSpell { get; }

        int OnPreDamagePriority { get; }

        bool OverrideCooldownCheck { get; }
        bool PermeatesThroughDeath { get; }
        bool PersistsThroughDeath { get; }

        string PopupMessage1 { get; }

        float SetSpellDamageRatio { get; }
        float SpellDamageRatio { get; }

        string[] SpellFXOverrideSkins { get; }

        int SpellToggleSlot { get; }

        string[] SpellVOOverrideSkins { get; }

        /// <summary>
        /// Determines whether or not the spell stops movement and triggers spell casts (and post).
        /// Usually should not be true if the spell is an item active, summoner spell, missile spell, or otherwise purely buff related spell.
        /// </summary>
        bool TriggersSpellCasts { get; }
    }
}
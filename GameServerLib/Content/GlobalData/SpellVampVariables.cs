using GameServerCore.Enums;
using System.Collections.Generic;

namespace LeagueSandbox.GameServer.Content
{
    public class SpellVampVariables
    {
        public Dictionary<DamageSource, float> SpellVampRatios = new Dictionary<DamageSource, float>
        {
            // Spell Vamp ratio for DAMAGESOURCE_SPELL
            {DamageSource.DAMAGE_SOURCE_SPELL, 1.0f },

            // Spell Vamp ratio for DAMAGESOURCE_SPELLAOE
            {DamageSource.DAMAGE_SOURCE_SPELLAOE, 0.334f },

            // Spell Vamp ratio for DAMAGESOURCE_SPELLPERSIST
            {DamageSource.DAMAGE_SOURCE_SPELLPERSIST, 1.0f },

            // Spell Vamp ratio for DAMAGESOURCE_PERIODIC
            {DamageSource.DAMAGE_SOURCE_PERIODIC, 0.0f },

            // Spell Vamp ratio for DAMAGESOURCE_PROC
            {DamageSource.DAMAGE_SOURCE_PROC, 0.0f },

            // Spell Vamp ratio for DAMAGESOURCE_REACTIVE
            {DamageSource.DAMAGE_SOURCE_REACTIVE, 0.0f },

            // Spell Vamp ratio for DAMAGESOURCE_ONDEATH
            {DamageSource.DAMAGE_SOURCE_ONDEATH, 0.0f },

            // Spell Vamp ratio for DAMAGESOURCE_PET
            {DamageSource.DAMAGE_SOURCE_PET, 0.0f },
        };
    }
}

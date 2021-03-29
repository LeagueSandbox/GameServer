namespace GameServerCore.Enums
{
    public enum OrderType : byte
    {
        /// <summary>
        /// Unknown. Current usage assumes it is for the beginning of the game.
        /// </summary>
        /// TODO: Verify
        OrderNone = 0x0,
        /// <summary>
        /// Used when a unit is postponing further movements (can still target).
        /// </summary>
        /// TODO: Verify
        Hold = 0x1,
        /// <summary>
        /// Used when a unit has started moving to a location (non-object target).
        /// </summary>
        MoveTo = 0x2,
        /// <summary>
        /// Used when a unit has targeted and is moving towards, a unit.
        /// </summary>
        AttackTo = 0x3,
        /// <summary>
        /// Unknown. Current usage assumes it is for when a spell uses auto attack cast delay.
        /// </summary>
        /// TODO: Verify
        TempCastSpell = 0x4,
        /// <summary>
        /// Used when a pet is forced to target and move towards a unit.
        /// </summary>
        PetHardAttack = 0x5,
        /// <summary>
        /// Used when a pet is forced to move to a location.
        /// </summary>
        PetHardMove = 0x6,
        /// <summary>
        /// Used when a unit has started moving to a location and is actively searching its area for a target to attack.
        /// </summary>
        AttackMove = 0x7,
        /// <summary>
        /// Used when a unit has started a taunt. Normally stops movement.
        /// </summary>
        Taunt = 0x8,
        /// <summary>
        /// Used when a unit is forced to return to a location. Perhaps related to Malzahar pets losing focus of a target, and thus moving back to their owner?
        /// </summary>
        /// TODO: Verify
        PetHardReturn = 0x9,
        /// <summary>
        /// Used when a unit performs a stop movement.
        /// </summary>
        Stop = 0xA,
        /// <summary>
        /// Used when a pet is forced to perform a stop movement.
        /// </summary>
        /// TODO: Verify
        PetHardStop = 0xB,
        /// <summary>
        /// Used when a unit tries to use an object. Perhaps related to Dominion capture points?
        /// </summary>
        /// TODO: Verify
        Use = 0xC,
        /// <summary>
        /// Unknown.
        /// </summary>
        /// TODO: Verify
        AttackTerrainSustained = 0xD,
        /// <summary>
        /// Unknown.
        /// </summary>
        /// TODO: Verify
        AttackTerrainOnce = 0xE,
        /// <summary>
        /// Used when a unit has begun casting a spell.
        /// </summary>
        /// TODO: Verify
        CastSpell = 0xF,
    }
}
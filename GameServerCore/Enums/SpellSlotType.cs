namespace GameServerCore.Enums
{
    public enum SpellSlotType
    {
        SpellSlots,
        SummonerSpellSlots = 4,
        InventorySlots = 6,
        BluePillSlot = 13,
        TempItemSlot = 14,
        RuneSlots = 15,
        ExtraSlots = 45,
        RespawnSpellSlot = 60,
        UseSpellSlot = 61,
        PassiveSpellSlot = 62,
        BasicAttackNormalSlots = 64,
        // TODO: Verify if we need this
        BasicAttackCriticalSlots = 73
    }
}
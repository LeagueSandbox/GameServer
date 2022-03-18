using System;
namespace GameServerCore.Enums
{
    public enum ContextualEmoteID : byte
    {
        DefaultRun = 0x0,
        DefaultIdle = 0x1,
        Flee = 0x2,
        Chase = 0x3,
        GameStart = 0x4,
        KilledChampionGeneric = 0x5,
        KilledChampionMultiKill = 0x6,
        KilledChampionSpecific = 0x7,
        KilledNeutralMinion = 0x8,
        KilledTurret = 0x9,
        ProximityFirendlyMinion = 0xA,
        ProximityEnemyMinion = 0xB,
        ProximityNeutralMinion = 0xC,
        ProximityTurret = 0xD,
        LeaveCombatLowHealth = 0xE,
        InBrush = 0xF,
        ChampionSpell1 = 0x10,
        ChampionSpell2 = 0x11,
        ChampionSpell3 = 0x12,
        ChampionSpell4 = 0x13,
        CustomEvent = 0x14,
    }
}

using System;

namespace GameServerCore.Enums
{
    /// <summary>
    /// Enumerator containing all(?) possible spell flags found in spell data. Used in determining how a spell functions.
    /// </summary>
    /// TODO: Verify if this is finished.
    [Flags]
    public enum SpellDataFlags
    {
        AutoCast = 1 << 1,
        InstantCast = 1 << 2,
        PersistThroughDeath = 1 << 3,
        NonDispellable = 1 << 4,
        NoClick = 1 << 5,
        AffectImportantBotTargets = 1 << 6,
        AllowWhileTaunted = 1 << 7,
        NotAffectZombie = 1 << 8,
        AffectUntargetable = 1 << 9,
        AffectEnemies = 1 << 10,
        AffectFriends = 1 << 11,
        AffectNeutral = 1 << 14,
        // 4000 + C00: 4C00
        AffectAllSides = AffectEnemies + AffectFriends + AffectNeutral,
        AffectBuildings = 1 << 12,
        AffectMinions = 1 << 15,
        AffectHeroes = 1 << 16,
        AffectTurrets = 1 << 17,
        // 20000 + 18000: 38000
        AffectAllUnitTypes = AffectMinions + AffectHeroes + AffectTurrets,
        NotAffectSelf = 1 << 13,
        AlwaysSelf = 1 << 18,
        AffectDead = 1 << 19,
        AffectNotPet = 1 << 20,
        AffectBarracksOnly = 1 << 21,
        IgnoreVisibilityCheck = 1 << 22,
        NonTargetableAlly = 1 << 23,
        NonTargetableEnemy = 1 << 24,
        // 1000000 + 800000: 1800000
        NonTargetableAll = NonTargetableAlly + NonTargetableEnemy,
        TargetableToAll = 1 << 25,
        AffectWards = 1 << 26,
        AffectUseable = 1 << 27,
        IgnoreAllyMinion = 1 << 28,
        IgnoreEnemyMinion = 1 << 29,
        IgnoreLaneMinion = 1 << 30,
        IgnoreClones = 1 << 31,
    }
}
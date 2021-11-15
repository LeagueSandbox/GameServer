namespace GameServerCore.Enums
{
    public enum GameObjectTypes
    {
        // Most commented types are client-side.

        // LampBulb
        InfoPoint = 1,
        // EffectEmitter
        LevelProp = 2,
        // LevelPropSpawnerPoint
        // GrassObject
        // Spell_Targeting_UnrevealedTarget
        // Spell_SpellCaster
        // Spell_DrawFX
        // Missile is also part of Spell_
        Missile,
        ChainMissile = Missile << 1,
        CircleMissile = Missile << 2,
        LineMissile = Missile << 3,
        NeutralMinionCamp,
        AttackableUnit,
        ObjAIBase = AttackableUnit << 1,
        ObjAIBase_Hero = ObjAIBase + 1,
        ObjAIBase_Turret = ObjAIBase + 2,
        ObjAIBase_Minion = ObjAIBase + 3,
        ObjAIBase_Marker = ObjAIBase + 4,
        ObjAIBase_LevelProp = ObjAIBase + 5,
        ObjAIBase_FollowerObject = ObjAIBase + 6,
        ObjBuilding,
        ObjBuilding_Shop = ObjBuilding << 1,
        // ObjBuilding_Levelsizer
        ObjBuilding_NavPoint = ObjBuilding << 2,
        // ObjBuilding_Lake
        ObjBuilding_SpawnPoint = ObjBuilding << 3,
        ObjBuilding_Animated = ObjBuilding << 4,
        // ObjAnimated_Turret
        ObjAnimated_HQ = ObjBuilding_Animated + 1,
        ObjAnimated_BarracksDampener = ObjBuilding_Animated + 2,
        ObjBuildingBarracks = ObjBuilding << 5
    }
}
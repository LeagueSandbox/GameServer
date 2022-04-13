using System;

namespace GameServerCore.Enums
{
    [Flags]
    public enum UnitTag
    {
        Champion,
        Champion_Clone,
        Minion,
        Minion_Lane,
        Minion_Lane_Siege,
        Minion_Lane_Super,
        Minion_Summon,
        Monster,
        Monster_Epic,
        Monster_Large,
        Special,
        Special_SyndraSphere,
        Special_TeleportTarget,
        Structure,
        Structure_Inhibitor,
        Structure_Nexus,
        Structure_Turret,
        Structure_Turret_Inhib,
        Structure_Turret_Inner,
        Structure_Turret_Nexus,
        Structure_Turret_Outer,
        Structure_Turret_Shrine,
        Ward
    }
}

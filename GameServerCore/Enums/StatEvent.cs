namespace GameServerCore.Enums
{
    public enum StatEvent : byte
    {
        NodeCapture = 0x0,
        NodeNeutralize = 0x1,
        NodeKillOffense = 0x2,
        TeamObjective = 0x3,
        DefendPointNeutralize = 0x4,
        NodeKillDefense = 0x5,
        NodeTimeDefense = 0x6,
        LastStand = 0x7,
        NodeCaptureAssist = 0x8,
        NodeNeutralizeAssist = 0x9,
    }
}

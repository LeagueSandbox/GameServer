namespace GameServerCore.Enums
{
    public enum TargetingType : byte
    {
        Self = 0x0,
        Target = 0x1,
        Area = 0x2,
        Cone = 0x3,
        SelfAOE = 0x4,
        TargetOrLocation = 0x5,
        Location = 0x6,
        Direction = 0x7,
        DragDirection = 0x8,
        // TODO: Find out what this is used for.
        LineTargetToCaster = 0x9,
        Invalid = 0xFF,
    }
}
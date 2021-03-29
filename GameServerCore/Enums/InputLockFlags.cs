using System;

namespace GameServerCore.Enums
{
    /// <summary>
    /// Enumerator containing all(?) possible input lock flags.
    /// </summary>
    /// TODO: Verify if this is finished.
    [Flags]
    public enum InputLockFlags
    {
        None = 0x0,
        CameraLocking = 0x1,
        Movement = 0x2,
        Abilities = 0x4,
        SummonerSpells = 0x8,
        Shop = 0x10,
        Chat = 0x20,
        MinimapMovement = 0x40,
        CameraMovement = 0x80,
    }
}
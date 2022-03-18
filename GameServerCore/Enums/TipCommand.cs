using System;
namespace GameServerCore.Enums
{
    public enum TipCommand : byte
    {
        Activate = 0x0,
        Remove = 0x1,
        EnableTipEvents = 0x2,
        DisableTipEvents = 0x3,
        ActivateTipDialogue = 0x4,
        EnableTipDialogueEvents = 0x5,
        DisableTipDialogueEvents = 0x6,
    }
}

using System;
namespace GameServerCore.Enums
{
    public enum AudioVOEventType : byte
    {
        Announcer = 0x0,
        Conversation = 0x1,
        Shop = 0x2,
        Command = 0x3,
        Death = 0x4,
        ImportantCallout = 0x5,
        SpecialSpell = 0x6,
        SpellUlti = 0x7,
        ItemPurchase = 0x8,
        InteractiveEvent = 0x9,
        LastHit = 0xA,
        Emote = 0xB,
        FirstMove = 0xC,
        OrderConfirmation = 0xD,
        Spell = 0xE,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct CastSpellResponseArgs
    {
        public int Hash { get; }
        public byte Level { get; }
        public float CastTime { get; }
        public float CoolDown { get; }
        public byte Slot { get; }
        public float ManaCost { get; }
        public ChampionAtLocation Owner { get; }
        public float X { get; }
        public float Y { get; }
        public float XDragEnd { get; }
        public float YDragEnd { get; }
        public uint FutureProjNetId { get; }
        public uint SpellNetId { get; }

        public CastSpellResponseArgs(int hash, byte level, float castTime, float coolDown, byte slot, float manaCost, ChampionAtLocation owner, float x, float y, float xDragEnd, float yDragEnd, uint futureProjNetId, uint spellNetId)
        {
            Hash = hash;
            Level = level;
            CastTime = castTime;
            CoolDown = coolDown;
            Slot = slot;
            ManaCost = manaCost;
            Owner = owner;
            X = x;
            Y = y;
            XDragEnd = xDragEnd;
            YDragEnd = yDragEnd;
            FutureProjNetId = futureProjNetId;
            SpellNetId = spellNetId;
        }
    }
}

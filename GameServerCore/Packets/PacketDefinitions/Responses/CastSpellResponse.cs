using GameServerCore.Content;
using GameServerCore.Domain;
using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class CastSpellResponse : ICoreResponse
    {
        public INavGrid NavGrid { get; }
        public ISpell Spell { get; }
        public Vector2 Start { get; }
        public Vector2 End{ get;}
        public uint FutureProjNetId { get; }
        public uint SpellNetId { get; }
        public CastSpellResponse(INavGrid navGrid, ISpell s, Vector2 start, Vector2 end, uint futureProjNetId, uint spellNetId)
        {
            NavGrid = navGrid;
            Spell = s;
            Start = start;
            End = end;
            FutureProjNetId = futureProjNetId;
            SpellNetId = spellNetId;
        }
    }
}
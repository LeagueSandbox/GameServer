using GameServerCore.Domain.GameObjects;
using GameServerCore.Scripting.CSharp;
using System.Collections.Generic;

namespace GameServerCore.Domain
{
    public interface ITalentInventory
    {
        List<ITalent> Talents { get; }
        void Add(string talentId, byte level);
        void Initialize(IObjAiBase owner);
    }
}

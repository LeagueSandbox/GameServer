using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;

namespace GameServerCore.Scripting.CSharp
{
    public interface ITalentScript
    {
        void OnActivate(IObjAiBase owner, byte rank)
        {
        }
    }
}
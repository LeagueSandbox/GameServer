using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;

namespace GameServerCore.Scripting.CSharp
{
    public interface ITalentScript
    {
        void OnActivate(IObjAIBase owner, byte rank)
        {
        }
    }
}
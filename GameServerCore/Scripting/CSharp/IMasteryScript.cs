using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;

namespace GameServerCore.Scripting.CSharp
{
    public interface IMasteryScript
    {
        void OnActivate(IObjAiBase owner, byte level)
        {
        }
        void OnUpdate(float diff)
        {
        }
    }
}
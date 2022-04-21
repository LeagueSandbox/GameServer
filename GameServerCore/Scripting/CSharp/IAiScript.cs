using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;

namespace GameServerCore.Scripting.CSharp
{
    public interface IAIScript
    {
        IAIScriptMetaData AIScriptMetaData { get; set; }
        void OnActivate(IObjAiBase owner)
        {
        }

        void OnUpdate(float diff)
        {
        }

        void OnCallForHelp(IAttackableUnit attacker, IAttackableUnit victium)
        {
        }
    }
}
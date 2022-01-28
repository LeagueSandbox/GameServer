using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;

namespace GameServerCore.Scripting.CSharp
{
    public interface IAIScript
    {
        IAIScriptMetaData AIScriptMetaData { get; set; }

        /// <summary>
        /// List of attacking allied units that are asking for help and their priorities.
        /// Must be non-null to receive calls for help.
        /// Must be manually cleaned when not needed to avoid overgrowth.
        /// </summary>
        Dictionary<IAttackableUnit, int> unitsAttackingAllies { get; }
        void OnActivate(IObjAiBase owner)
        {
        }
        void OnUpdate(float diff)
        {
        }
    }
}
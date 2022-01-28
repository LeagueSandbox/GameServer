using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;

namespace GameServerCore.Scripting.CSharp
{
    public interface IAIScriptHearingCallsForHelp: IAIScript
    {
        /// <summary>
        /// List of attacking allied units that are asking for help and their priorities.
        /// </summary>
        Dictionary<IAttackableUnit, int> unitsAttackingAllies { get; }
    }
}
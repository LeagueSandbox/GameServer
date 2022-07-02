using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Scripting.CSharp
{
    public interface IAIScript
    {
        IAIScriptMetaData AIScriptMetaData { get; set; }
        void OnActivate(IObjAIBase owner)
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
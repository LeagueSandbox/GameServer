using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace GameServerCore.Scripting.CSharp
{
    public interface IAIScript
    {
        AIScriptMetaData AIScriptMetaData { get; set; }
        void OnActivate(ObjAIBase owner)
        {
        }

        void OnUpdate(float diff)
        {
        }

        void OnCallForHelp(AttackableUnit attacker, AttackableUnit victium)
        {
        }
    }
}
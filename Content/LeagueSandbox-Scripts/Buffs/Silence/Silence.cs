using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Silence
{
    internal class Silence : IBuffGameScript
    {
        private UnitCrowdControl _crowd = new UnitCrowdControl(CrowdControlType.SILENCE);

        public void OnActivate(IObjAiBase unit, ISpell ownerSpell)
        {
            unit.ApplyCrowdControl(_crowd);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            unit.RemoveCrowdControl(_crowd);
        }

        public void OnUpdate(double diff)
        {

        }
    }
}

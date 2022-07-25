using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace CharScripts
{
    internal class CharScriptOdinCenterRelic : ICharScript
    {
        public void OnActivate(ObjAIBase owner, Spell spell = null)
        {
            SetStatus(owner, StatusFlags.MagicImmune, true);
            SetStatus(owner, StatusFlags.PhysicalImmune, true);
            SetStatus(owner, StatusFlags.CanAttack, false);
            SetStatus(owner, StatusFlags.CanMove, false);

            AddBuff("OdinBombBuff", 25000.0f, 1, null, owner, owner, false);
        }
    }
}

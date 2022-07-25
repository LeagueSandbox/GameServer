using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace CharScripts
{
    internal class CharScriptAscXerath : ICharScript
    {
        public void OnActivate(ObjAIBase owner, Spell spell = null)
        {
            AddBuff("ResistantSkinDragon", 25000.0f, 1, null, owner, owner);
            var buff = AddBuff("AscBuffTransfer", 5.7f, 1, null, owner, owner);
            ApiEventManager.OnBuffDeactivated.AddListener(this, buff, OnBuffDeactivation, true);
        }

        public void OnBuffDeactivation(Buff buff)
        {
            AddBuff("AscXerathControl", 999999.0f, 1, null, buff.TargetUnit, buff.TargetUnit as ObjAIBase);
        }
        public void OnDeactivate(ObjAIBase owner, Spell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}

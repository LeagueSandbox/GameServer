using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.API;


namespace CharScripts
{
    internal class CharScriptAscXerath : ICharScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            AddBuff("ResistantSkinDragon", 25000.0f, 1, null, owner, owner);
            var buff = AddBuff("AscBuffTransfer", 5.7f, 1, null, owner, owner);
            ApiEventManager.OnBuffDeactivated.AddListener(this, buff, OnBuffDeactivation, true);
        }

        public void OnBuffDeactivation(IBuff buff)
        {
            AddBuff("AscXerathControl", 999999.0f, 1, null, buff.TargetUnit, buff.TargetUnit as IObjAiBase);
        }
        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}

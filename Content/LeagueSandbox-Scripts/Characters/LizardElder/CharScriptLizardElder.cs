using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace CharScripts
{
    internal class CharScriptLizardElder : ICharScript
    {
        public void OnActivate(ObjAIBase owner, Spell spell = null)
        {
            AddBuff("GlobalMonsterBuff", 25000.0f, 1, spell, owner, owner, true);
            AddBuff("BlessingoftheLizardElder", 25000.0f, 1, null, owner, owner, true);
        }

        public void OnHitUnit(DamageData data)
        {
            // TODO: Multiply damage in data (currently unsupported).
        }
    }
}

using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Spells;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public class BuffGameScriptController
    {
        public IObjAiBase Unit { get; private set; }
        public IBuffGameScript GameScript { get; private set; }
        public string BuffNamespace { get; private set; }
        public string BuffClass { get; private set; }
        public ISpell OwnerSpell { get; private set; }
        private bool _remove;
        private float _duration = -1f;
        protected CSharpScriptEngine _scriptEngine;

        public BuffGameScriptController(Game game, IObjAiBase unit, string buffNamespace, string buffClass, ISpell ownerSpell, float duration = -1f)
        {
            _scriptEngine = game.ScriptEngine;
            BuffNamespace = buffNamespace;
            BuffClass = buffClass;
            OwnerSpell = ownerSpell;
            Unit = unit;
            _duration = duration;
            GameScript = _scriptEngine.CreateObject<IBuffGameScript>(buffNamespace, buffClass);

            if (_duration >= 0)
            {
                ApiFunctionManager.CreateTimer(_duration, DeactivateBuff);
            }
        }

        public void ActivateBuff()
        {
            GameScript.OnActivate(Unit, OwnerSpell);
            _remove = false;
        }

        public void DeactivateBuff()
        {
            if (_remove)
            {
                return;
            }

            GameScript.OnDeactivate(Unit);
            _remove = true;
        }

        public bool NeedsRemoved()
        {
            return _remove;
        }

        public bool IsBuffSame(string buffNamespace, string buffClass)
        {
            return buffNamespace == BuffNamespace && buffClass == BuffClass;
        }
    }
}

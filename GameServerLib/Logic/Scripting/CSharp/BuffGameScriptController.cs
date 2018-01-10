using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    public class BuffGameScriptController
    {
        ObjAIBase _unit;
        BuffGameScript _gameScript;
        String _buffNamespace;
        String _buffClass;
        Spell _ownerSpell;
        bool _remove = false;
        float _duration = -1f;
        protected CSharpScriptEngine _scriptEngine = Program.ResolveDependency<CSharpScriptEngine>();

        public BuffGameScriptController(ObjAIBase unit, String buffNamespace, String buffClass, Spell ownerSpell, float duration = -1f)
        {
            _buffNamespace = buffNamespace;
            _buffClass = buffClass;
            _ownerSpell = ownerSpell;
            _unit = unit;
            _duration = duration;
            _gameScript = _scriptEngine.CreateObject<BuffGameScript>(buffNamespace, buffClass);

            if (_duration >= 0)
            {
                ApiFunctionManager.CreateTimer(_duration, ()=> {
                    DeactivateBuff();
                });
            }
        }
        public void ActivateBuff()
        {
            _gameScript.OnActivate(_unit, _ownerSpell);
            _remove = false;
        }
        public void DeactivateBuff()
        {
            if (_remove == true) return;
            _gameScript.OnDeactivate(_unit);
            _remove = true;
        }
        public bool NeedsRemoved()
        {
            return _remove;
        }
        public Unit GetUnit() { return _unit; }
        public Spell GetOwnerSpell() { return _ownerSpell; }
        public BuffGameScript GetBuffGameScript() { return _gameScript; }
        public String GetBuffNamespace() { return _buffNamespace; }
        public String GetBuffClass() { return _buffClass; }
        public bool IsBuffSame(String buffNamespace, String buffClass)
        {
            return buffNamespace == _buffNamespace && buffClass == _buffClass;
        }
    }
}

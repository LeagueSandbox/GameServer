using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    public class GameScriptBuffController
    {
        Unit _unit, _target;
        IGameScript _gameScript;
        String _buffNamespace;
        String _buffClass;
        Spell _ownerSpell;
        bool _remove = false;
        float _duration = -1f;
        protected GameScriptEngine _scriptEngine = Program.ResolveDependency<GameScriptEngine>();

        public GameScriptBuffController(Unit owner, Unit target, String buffNamespace, String buffClass, Spell ownerSpell, float duration = -1f)
        {
            _buffNamespace = buffNamespace;
            _buffClass = buffClass;
            _ownerSpell = ownerSpell;
            _unit = owner;
            _target = target;
            _duration = duration;
            _gameScript = _scriptEngine.GetGameScript(buffNamespace, buffClass);

            if (_duration >= 0)
            {
                ApiFunctionManager.CreateTimer(_duration, ()=> {
                    DeactivateBuff();
                });
            }
        }
        public void ActivateBuff()
        {
            _gameScript.OnActivate(
                new GameScriptInformation
                {
                    Namespace = _buffNamespace,
                    Name = _buffClass,
                    OwnerUnit = _unit,
                    OwnerSpell = _ownerSpell,
                    TargetUnit = _target
                });
            _remove = false;
        }
        public void DeactivateBuff()
        {
            if (_remove == true) return;
            _gameScript.OnDeactivate();
            _remove = true;
        }
        public bool NeedsRemoved()
        {
            return _remove;
        }
        public Unit GetUnit() { return _unit; }
        public Spell GetOwnerSpell() { return _ownerSpell; }
        public IGameScript GetBuffGameScript() { return _gameScript; }
        public String GetBuffNamespace() { return _buffNamespace; }
        public String GetBuffClass() { return _buffClass; }
        public bool IsBuffSame(String buffNamespace, String buffClass)
        {
            return buffNamespace == _buffNamespace && buffClass == _buffClass;
        }
    }
}

using LeagueSandbox.GameServer.Logic.Scripting.Engine.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    public class GameScriptEngine
    {
        CSharpScriptEngine _scriptingEngine;
        public GameScriptEngine()
        {
            _scriptingEngine = new CSharpScriptEngine();
        }
        public IGameScript GetGameScript(string namespaceName, string className)
        {
            var gameScript = _scriptingEngine.CreateObject<IGameScript>(namespaceName, className);
            if (gameScript == null) gameScript = new GenericGameScript();
            return gameScript;
        }
    }
}

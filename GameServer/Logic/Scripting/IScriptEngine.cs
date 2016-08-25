using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Scripting
{
    public interface IScriptEngine
    {
        bool IsLoaded();
        void Load(string location);
        void RegisterFunction(string functionName, object target, MethodBase function);
        void Execute(string script);
        void SetGlobalVariable(string name, object value);
    }
}

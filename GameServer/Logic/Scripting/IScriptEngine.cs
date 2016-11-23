using System.Reflection;

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

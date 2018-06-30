using System.Reflection;

namespace LeagueSandbox.GameServer
{
    public static class ServerLibAssemblyDefiningType
    {
        public static Assembly Assembly => typeof(ServerLibAssemblyDefiningType).Assembly;
    }
}

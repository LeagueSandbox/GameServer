using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer
{
    public static class ServerLibAssemblyDefiningType
    {
        public static Assembly Assembly => typeof(ServerLibAssemblyDefiningType).Assembly;
    }
}

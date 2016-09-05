using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer
{
    public class ServerContext
    {
        public string ExecutingDirectory { get; private set; }

        public ServerContext()
        {
            ExecutingDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}

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
        private string _executingDirectory;

        public ServerContext()
        {
            _executingDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public string GetExecutingDirectory()
        {
            return _executingDirectory;
        }
    }
}

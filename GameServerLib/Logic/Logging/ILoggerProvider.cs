using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Logging
{
    public interface ILoggerProvider
    {
        ILogger GetLogger();
    }
}

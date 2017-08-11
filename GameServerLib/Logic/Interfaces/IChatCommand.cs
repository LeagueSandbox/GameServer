using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;

namespace LeagueSandbox.GameServer.Logic.Interfaces
{
    public interface IChatCommand
    {
        string Command { get; }
        string Syntax { get; }
        void Execute(Peer peer, bool hasReceivedArguments, string arguments = "");
        void ShowSyntax();
    }
}

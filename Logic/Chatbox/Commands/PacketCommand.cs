using System;
using System.Collections.Generic;
using ENet;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class PacketCommand : ChatCommand
    {
        public PacketCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            try
            {
                var s = arguments.Split(' ');
                if (s.Length < 2)
                {
                    _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    ShowSyntax();
                    return;
                }

                var bytes = new List<byte>();

                for (var i = 1; i < s.Length; i++)
                {
                    var ss = s[i].Split(':');
                    var type = ss[0];
                    dynamic num;
                    if (ss[1] == "netid")
                        num = _owner.GetGame().GetPeerInfo(peer).GetChampion().getNetId();
                    else
                        num = System.Convert.ChangeType(int.Parse(ss[1]), Type.GetType("System." + type));
                    var d = BitConverter.GetBytes(num);
                    if (num.GetType() == typeof(byte))
                        bytes.Add(num);
                    else
                        bytes.AddRange(d);
                }

                _owner.GetGame().PacketHandlerManager.sendPacket(peer, bytes.ToArray(), Channel.CHL_S2C);
            }
            catch { }
        }
    }
}

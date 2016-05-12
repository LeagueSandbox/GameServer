using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Chatbox;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleChatBoxMessage : IPacketHandler
    {
        #region Command classes
        /*class AdCommand : ChatCommand
        {
            public AdCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float ad;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                }
                else if (float.TryParse(split[1], out ad))
                    game.GetPeerInfo(peer).GetChampion().GetStats().AttackDamage.FlatBonus = ad;
            }
        }

        class ApCommand : ChatCommand
        {
            public ApCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float ap;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                }
                else if (float.TryParse(split[1], out ap))
                    game.GetPeerInfo(peer).GetChampion().GetStats().AbilityPower.FlatBonus = ap;
            }
        }

        class ChCommand : ChatCommand
        {
            public ChCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                    return;
                }
                new System.Threading.Thread(new System.Threading.ThreadStart(() =>
                {
                    var c = new Champion(game, split[1], game.GetPeerInfo(peer).GetChampion().getNetId(), (uint)game.GetPeerInfo(peer).UserId);
                    c.setPosition(game.GetPeerInfo(peer).GetChampion().getX(), game.GetPeerInfo(peer).GetChampion().getY());
                    c.setModel(split[1]); // trigger the "modelUpdate" proc
                    c.setTeam(game.GetPeerInfo(peer).GetChampion().getTeam());
                    game.GetMap().RemoveObject(game.GetPeerInfo(peer).GetChampion());
                    game.GetMap().AddObject(c);
                    game.GetPeerInfo(peer).SetChampion(c);
                })).Start();
            }
        }

        class CoordsCommand : ChatCommand
        {
            public CoordsCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                Logger.LogCoreInfo("At " + game.GetPeerInfo(peer).GetChampion().getX() + ";" + game.GetPeerInfo(peer).GetChampion().getY());
                StringBuilder debugMsg = new StringBuilder();
                debugMsg.Append("At Coords - X: ");
                debugMsg.Append(game.GetPeerInfo(peer).GetChampion().getX());
                debugMsg.Append(" Y: ");
                debugMsg.Append(game.GetPeerInfo(peer).GetChampion().getY());
                debugMsg.Append(" Z: ");
                debugMsg.Append(game.GetPeerInfo(peer).GetChampion().GetZ());
                SendDebugMsgFormatted(DebugMsgType.NORMAL, game, debugMsg.ToString());
            }
        }

        class GoldCommand : ChatCommand
        {
            public GoldCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float gold;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                }
                else if (float.TryParse(split[1], out gold))
                    game.GetPeerInfo(peer).GetChampion().GetStats().Gold = gold;
            }
        }

        class HealthCommand : ChatCommand
        {
            public HealthCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float hp;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                }
                else if (float.TryParse(split[1], out hp))
                {
                    game.GetPeerInfo(peer).GetChampion().GetStats().HealthPoints.FlatBonus = hp;
                    game.GetPeerInfo(peer).GetChampion().GetStats().CurrentHealth = hp;
                }
            }
        }

        class NewCommand : ChatCommand
        {
            public NewCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var packet = new GameServer.Logic.Packets.Packet((PacketCmdS2C)0xB7);
                var netid = game.GetPeerInfo(peer).GetChampion().getNetId();
                var buffer = packet.getBuffer();

                buffer.Write(netid);//target
                buffer.Write((byte)0x01); //Slot
                buffer.Write((byte)0x01); //Type
                buffer.Write((byte)0x01); // stacks
                buffer.Write((byte)0x01); // Visible
                buffer.Write((int)17212821); //Buff id
                buffer.Write((byte)0x56);
                buffer.Write((byte)0xD0);
                buffer.Write((byte)0xF2);
                buffer.Write((byte)0xDF);
                buffer.Write((byte)0x00);
                buffer.Write((byte)0x00);
                buffer.Write((byte)0x00);
                buffer.Write((byte)0x00);

                buffer.Write((float)25000.0f);

                buffer.Write((byte)0x20);
                buffer.Write((byte)0x00);
                buffer.Write((byte)0x00);
                buffer.Write((byte)0x40);
                buffer.Write((int)0);

                game.PacketHandlerManager.sendPacket(peer, packet, Channel.CHL_S2C);
                SendDebugMsgFormatted(DebugMsgType.INFO, game, "The new command added by .help has been executed");
                ChatCommandsDictionary.Remove(Command);
            }
        }

        class HelpCommand : ChatCommand
        {
            public HelpCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                string commands = "";
                int count = 0;
                foreach (KeyValuePair<string, ChatCommand> keyValuePair in ChatCommandsDictionary)
                {
                    if (!(keyValuePair.Value.IsHidden || keyValuePair.Value.IsDisabled)) // We could also show disabled commands
                    {                                  // (with a different color, saying they're disabled, etc.)
                        count += 1;
                        commands = commands
                                   + "<font color =\"#E175FF\"><b>"
                                   + keyValuePair.Value.Command
                                   + "</b><font color =\"#FFB145\">, ";
                    }
                }
                SendDebugMsgFormatted(DebugMsgType.INFO, game, "List of available commands: ");
                SendDebugMsgFormatted(DebugMsgType.INFO, game, commands);
                SendDebugMsgFormatted(DebugMsgType.INFO, game, "There are " + count.ToString() + " commands");

                if (!ChatCommandsDictionary.ContainsKey(".newcommand"))
                {
                    var NewCommandCmd = new NewCommand(".newcommand", "");
                    ChatCommandsDictionary.Add(".newcommand", NewCommandCmd);
                }
            }
        }

        class InhibCommand : ChatCommand
        {
            public InhibCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var sender = game.GetPeerInfo(peer);
                var min = new Monster(game, game.GetNewNetID(), sender.GetChampion().getX(), sender.GetChampion().getY(), sender.GetChampion().getX(), sender.GetChampion().getY(), "Worm", "Worm");//"AncientGolem", "AncientGolem1.1.1");
                game.GetMap().AddObject(min);
            }
        }

        class JunglespawnCommand : ChatCommand
        {
            public JunglespawnCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                Logger.LogCoreInfo(".junglespawn command not implemented");
                SendDebugMsgFormatted(DebugMsgType.INFO, game, "Command not implemented");
            }
        }

        class LevelCommand : ChatCommand
        {
            public LevelCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                byte lvl;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                }
                else if (byte.TryParse(split[1], out lvl))
                {
                    if (lvl < 1 || lvl > 18)
                        return;

                    game.GetPeerInfo(peer).GetChampion().GetStats().Level = lvl;
                    game.GetPeerInfo(peer).GetChampion().LevelUp();
                }
            }
        }

        class ManaCommand : ChatCommand
        {
            public ManaCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float mp;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                }
                else if (float.TryParse(split[1], out mp))
                {
                    game.GetPeerInfo(peer).GetChampion().GetStats().ManaPoints.FlatBonus = mp;
                    game.GetPeerInfo(peer).GetChampion().GetStats().CurrentMana = mp;
                }
            }
        }

        class MobsCommand : ChatCommand
        {
            public MobsCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                    return;
                }
                int team;
                if (!int.TryParse(split[1], out team))
                    return;
                var units = game.GetMap().GetObjects().Where(xx => xx.Value.getTeam() == CustomConvert.toTeamId(team)).Where(xx => xx.Value is Minion);
                foreach (var unit in units)
                {
                    var response = new AttentionPingAns(game.GetPeerInfo(peer), new AttentionPing { x = unit.Value.getX(), y = unit.Value.getY(), targetNetId = 0, type = Pings.Ping_Danger });
                    game.PacketHandlerManager.broadcastPacketTeam(game.GetPeerInfo(peer).GetTeam(), response, Channel.CHL_S2C);
                }
            }
        }

        class ModelCommand : ChatCommand
        {
            public ModelCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                if (split.Length >= 2)
                    game.GetPeerInfo(peer).GetChampion().setModel(split[1]);
                else
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                }
            }
        }

        class PacketCommand : ChatCommand
        {
            public PacketCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                try
                {
                    var s = arguments.Split(' ');
                    if (s.Length < 2)
                    {
                        SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                        ShowSyntax(game);
                        return;
                    }

                    var bytes = new List<byte>();

                    for (var i = 1; i < s.Length; i++)
                    {
                        var ss = s[i].Split(':');
                        var type = ss[0];
                        dynamic num;
                        if (ss[1] == "netid")
                            num = game.GetPeerInfo(peer).GetChampion().getNetId();
                        else
                            num = System.Convert.ChangeType(int.Parse(ss[1]), Type.GetType("System." + type));
                        var d = BitConverter.GetBytes(num);
                        if (num.GetType() == typeof(byte))
                            bytes.Add(num);
                        else
                            bytes.AddRange(d);
                    }

                    game.PacketHandlerManager.sendPacket(peer, bytes.ToArray(), Channel.CHL_S2C);
                }
                catch { }
            }
        }

        class SetCommand : ChatCommand
        {
            public SetCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                if (split.Length < 4)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                    return;
                }
                int blockNo, fieldNo = 0;
                float value = 0;
                if (int.TryParse(split[1], out blockNo))
                    if (int.TryParse(split[2], out fieldNo))
                        if (float.TryParse(split[3], out value))
                        {
                            //game.GetPeerInfo(peer).GetChampion().GetStats().setStat((MasterMask)blockNo, (FieldMask)fieldNo, value);
                        }
            }
        }

        class SizeCommand : ChatCommand
        {
            public SizeCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float size;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                    return;
                }
                else if (float.TryParse(split[1], out size))
                    game.GetPeerInfo(peer).GetChampion().GetStats().Size.BaseValue = size;
            }
        }

        class SkillpointsCommand : ChatCommand
        {
            public SkillpointsCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                game.GetPeerInfo(peer).GetChampion().setSkillPoints(17);
                var skillUpResponse = new SkillUpPacket(game.GetPeerInfo(peer).GetChampion().getNetId(), 0, 0, 17);
                game.PacketHandlerManager.sendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
            }
        }

        class SpawnCommand : ChatCommand
        {
            public SpawnCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                Logger.LogCoreInfo(".spawn command not implemented");
                SendDebugMsgFormatted(DebugMsgType.INFO, game, "Command not implemented");
            }
        }

        class SpeedCommand : ChatCommand
        {
            public SpeedCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float speed;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                }
                if (float.TryParse(split[1], out speed))
                    game.GetPeerInfo(peer).GetChampion().GetStats().MoveSpeed.FlatBonus = speed;
                else
                    SendDebugMsgFormatted(DebugMsgType.ERROR, game, "Incorrect parameter");
            }
        }

        class TpCommand : ChatCommand
        {
            public TpCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float x, y;
                if (split.Length < 3)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                }
                if (float.TryParse(split[1], out x))
                    if (float.TryParse(split[2], out y))
                        game.PacketNotifier.notifyTeleport(game.GetPeerInfo(peer).GetChampion(), x, y);
            }
        }

        class XpCommand : ChatCommand
        {
            public XpCommand(string command, string syntax) : base(command, syntax) { }

            public override void Execute(Peer peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float xp;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR, game);
                    ShowSyntax(game);
                }
                if (float.TryParse(split[1], out xp))
                    game.GetPeerInfo(peer).GetChampion().GetStats().Experience = xp;
            }
        }*/
        #endregion

        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var message = new ChatMessage(data);
            var split = message.msg.Split(' ');
            if (split.Length > 1)
            {
                int x, y = 0;
                if (int.TryParse(split[0], out x))
                {
                    if (int.TryParse(split[1], out y))
                    {
                        var response = new AttentionPingAns(game.GetPeerInfo(peer), new AttentionPing { x = x, y = y, targetNetId = 0, type = 0 });
                        game.PacketHandlerManager.broadcastPacketTeam(game.GetPeerInfo(peer).GetTeam(), response, Channel.CHL_S2C);
                    }
                }
            }

            #region Commands
            // Execute commands
            var CommandStarterCharacter = game.ChatboxManager.CommandStarterCharacter;
            if (message.msg.StartsWith(CommandStarterCharacter))
            {
                message.msg = message.msg.Remove(0, 1);
                split = message.msg.ToLower().Split(' ');

                ChatCommand command = game.ChatboxManager.GetCommand(split[0]);
                if (command != null)
                {
                    command.Execute(peer, true, message.msg);
                    return true;
                }
                else
                {
                    game.ChatboxManager.SendDebugMsgFormatted(DebugMsgType.ERROR, "<font color =\"#E175FF\"><b>" + game.ChatboxManager.CommandStarterCharacter + split[0] + "</b><font color =\"#AFBF00\"> is not a valid command.");
                    game.ChatboxManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Type <font color =\"#E175FF\"><b>" + game.ChatboxManager.CommandStarterCharacter + "help</b><font color =\"#AFBF00\"> for a list of available commands");
                    return true;
                }
            }
            #endregion
            switch (message.type)
            {
                case ChatType.CHAT_ALL:
                    return game.PacketHandlerManager.broadcastPacket(data, Channel.CHL_COMMUNICATION);
                case ChatType.CHAT_TEAM:
                    return game.PacketHandlerManager.broadcastPacketTeam(game.GetPeerInfo(peer).GetTeam(), data, Channel.CHL_COMMUNICATION);
                default:
                    //Logging.errorLine("Unknown ChatMessageType");
                    return game.PacketHandlerManager.sendPacket(peer, data, Channel.CHL_COMMUNICATION);
            }
        }
    }
}

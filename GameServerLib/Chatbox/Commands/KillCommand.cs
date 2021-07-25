using GameServerCore;
using GameServerCore.Enums;
using GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class KillCommand : ChatCommandBase
    {
        private readonly IPlayerManager _playerManager;

        public override string Command => "kill";
        public override string Syntax => $"{Command} minions";

        public KillCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');

            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (split[1] == "minions")
            {
                var objects = Game.ObjectManager.GetObjects();
                foreach (var o in objects)
                {
                    if (o.Value is Minion minion)
                    {
                        minion.Die(new DeathData
                        {
                            BecomeZombie = false,
                            DieType = 0,
                            Unit = minion,
                            Killer = minion,
                            DamageType = (byte)DamageType.DAMAGE_TYPE_PHYSICAL,
                            DamageSource = (byte)DamageSource.DAMAGE_SOURCE_RAW,
                            DeathDuration = 0
                        }); // :(
                    }
                }
            }
            else
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
        }
    }
}

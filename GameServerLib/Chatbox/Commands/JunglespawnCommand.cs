using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class JunglespawnCommand : ChatCommandBase
    {
        private readonly ILog _logger;

        public override string Command => "junglespawn";
        public override string Syntax => $"{Command} 1(normal camp) 2(Epic) 3(normal+Epic)";

        public JunglespawnCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _logger = LoggerProvider.GetLogger();
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }

            if (!int.TryParse(split[1], out var number))
            {
                return;
            }
            switch (number)
            {
                case 1: //normal only
                    BlueTeam01();
                    BlueTeam02();
                    RedTeam01();
                    RedTeam02();
                    break;
                case 2: // Epic only
                    Dragon();
                    Baron();
                    break;
                case 3: // normal + Epic
                    BlueTeam01();
                    BlueTeam02();
                    RedTeam01();
                    RedTeam02();
                    Dragon();
                    Baron();
                    break;
                default:
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    ShowSyntax();
                    break;
            }
            //_logger.Warn($"{ChatCommandManager.CommandStarterCharacter}{Command} command not implemented");
            //ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Command not implemented");
        }

        private void BlueTeam01()
        {
            var BlueWolfBig = new Monster(
                Game,
                3373.805f,
                6210.78f,
                3372.805f,
                6209.78f,
                "GiantWolf",
                "GiantWolf"
                );
            var BlueWolf01 = new Monster(
                Game,
                3352.034f,
                6357.328f,
                3351.034f,
                6356.328f,
                "Wolf",
                "Wolf"
                );
            var BlueWolf02 = new Monster(
                Game,
                3564.353f,
                6198.197f,
                3563.353f,
                6197.197f,
                "Wolf",
                "Wolf"
                );
            var BTblueBig = new Monster(
                Game,
                3447.734f,
                7665.903f,
                3446.734f,
                7666.903f,
                "Golem",
                "Golem"
                );
            var BTblue01 = new Monster(
                Game,
                3402.417f,
                7860.728f,
                3401.417f,
                7861.728f,
                "SmallGolem",
                "SmallGolem"
                );
            var BTblue02 = new Monster(
                Game,
                3243.181f,
                7642.048f,
                3242.181f,
                7643.048f,
                "SmallGolem",
                "SmallGolem"
                );
            var BlueGhost = new Monster(
                Game,
                1827.713f,
                8128.047f,
                1828.713f,
                8129.047f,
                "GreatWraith",
                "GreatWraith"
                );

            Game.ObjectManager.AddObject(BlueWolfBig);
            Game.ObjectManager.AddObject(BlueWolf01);
            Game.ObjectManager.AddObject(BlueWolf02);
            Game.ObjectManager.AddObject(BTblueBig);
            Game.ObjectManager.AddObject(BTblue01);
            Game.ObjectManager.AddObject(BTblue02);
            Game.ObjectManager.AddObject(BlueGhost);
        }

        private void BlueTeam02()
        {
            var RedGhostBig = new Monster(
                Game,
                6426.537f,
                5324.14f,
                6427.537f,
                5323.14f,
                "Wraith",
                "Wraith"
                );
            var RedGhost01 = new Monster(
                Game,
                6661f,
                5312.133f,
                6660f,
                5311.133f,
                "LesserWraith",
                "LesserWraith"
                );
            var RedGhost02 = new Monster(
                Game,
                6447.12f,
                5064.347f,
                6448.12f,
                5065.347f,
                "LesserWraith",
                "LesserWraith"
                );
            var RedGhost03 = new Monster(
                Game,
                6695.407f,
                5100.09f,
                6694.407f,
                5101.09f,
                "LesserWraith",
                "LesserWraith"
                );
            var BTredBig = new Monster(
                Game,
                7339.877f,
                3782.32f,
                7338.877f,
                3781.32f,
                "LizardElder",
                "LizardElder"
                );
            var BTred01 = new Monster(
                Game,
                7109.72f,
                3746.528f,
                7108.72f,
                3745.528f,
                "YoungLizard",
                "YoungLizard"
                );
            var BTred02 = new Monster(
                Game,
                7361.778f,
                3577.811f,
                7360.778f,
                3576.811f,
                "YoungLizard",
                "YoungLizard"
                );
            var Golem01 = new Monster(
                Game,
                8144.904f,
                2466.169f,
                8144.104f,
                2465.169f,
                "Golem",
                "Golem"
                );
            var Golem02 = new Monster(
                Game,
                7872.973f,
                2428.22f,
                7873.773f,
                2427.22f,
                "Golem",
                "Golem"
                );
            Game.ObjectManager.AddObject(RedGhostBig);
            Game.ObjectManager.AddObject(RedGhost01);
            Game.ObjectManager.AddObject(RedGhost02);
            Game.ObjectManager.AddObject(RedGhost03);
            Game.ObjectManager.AddObject(BTredBig);
            Game.ObjectManager.AddObject(BTred01);
            Game.ObjectManager.AddObject(BTred02);
            Game.ObjectManager.AddObject(Golem01);
            Game.ObjectManager.AddObject(Golem02);
        }

        private void RedTeam01()
        {
            var RedWolfBig = new Monster(
                Game,
                10569.55f,
                8125.38f,
                10570.55f,
                8126.38f,
                "GiantWolf",
                "GiantWolf"
                );
            var RedWolf01 = new Monster(
                Game,
                10570.42f,
                8326.041f,
                10571.42f,
                8327.041f,
                "Wolf",
                "Wolf"
                );
            var RedWolf02 = new Monster(
                Game,
                10770.59f,
                8190.568f,
                10771.59f,
                8191.568f,
                "Wolf",
                "Wolf"
                );
            var RTblueBig = new Monster(
                Game,
                10507.09f,
                6745.472f,
                10508.09f,
                6744.472f,
                "Golem",
                "Golem"
                );
            var RTblue01 = new Monster(
                Game,
                10614.14f,
                6471.359f,
                10615.14f,
                6470.359f,
                "SmallGolem",
                "SmallGolem"
                );
            var RTblue02 = new Monster(
                Game,
                10772.15f,
                6771.886f,
                10773.15f,
                6770.886f,
                "SmallGolem",
                "SmallGolem"
                );
            var RedGhost = new Monster(
                Game,
                12337f,
                6263f,
                12336f,
                6262f,
                "GreatWraith",
                "GreatWraith"
                );

            Game.ObjectManager.AddObject(RedWolfBig);
            Game.ObjectManager.AddObject(RedWolf01);
            Game.ObjectManager.AddObject(RedWolf02);
            Game.ObjectManager.AddObject(RTblueBig);
            Game.ObjectManager.AddObject(RTblue01);
            Game.ObjectManager.AddObject(RTblue02);
            Game.ObjectManager.AddObject(RedGhost);
        }

        private void RedTeam02()
        {
            var RedGhostBig = new Monster(
                Game,
                7480.368f,
                9091.41f,
                7480.368f,
                9092.41f,
                "Wraith",
                "Wraith"
                );
            var RedGhost01 = new Monster(
                Game,
                7350.368f,
                9230.41f,
                7351.368f,
                9230.41f,
                "LesserWraith",
                "LesserWraith"
                );
            var RedGhost02 = new Monster(
                Game,
                7450.368f,
                9350.41f,
                7450.368f,
                9349.41f,
                "LesserWraith",
                "LesserWraith"
                );
            var RedGhost03 = new Monster(
                Game,
                7580.368f,
                9250.41f,
                7579.368f,
                9250.41f,
                "LesserWraith",
                "LesserWraith"
                );
            var RTredBig = new Monster(
                Game,
                6566.17f,
                10534.22f,
                6567.17f,
                10535.22f,
                "LizardElder",
                "LizardElder"
                );
            var RTred01 = new Monster(
                Game,
                6504.241f,
                10784.563f,
                6505.241f,
                10785.563f,
                "YoungLizard",
                "YoungLizard"
                );
            var RTred02 = new Monster(
                Game,
                6792.664f,
                10606.5f,
                6793.664f,
                10607.5f,
                "YoungLizard",
                "YoungLizard"
                );
            var Golem01 = new Monster(
                Game,
                5865.4f,
                11915.05f,
                5866.4f,
                11916.05f,
                "Golem",
                "Golem"
                );
            var Golem02 = new Monster(
                Game,
                6140.464f,
                11935.5f,
                6139.264f,
                11936.3f,
                "Golem",
                "Golem"
                );
            Game.ObjectManager.AddObject(RedGhostBig);
            Game.ObjectManager.AddObject(RedGhost01);
            Game.ObjectManager.AddObject(RedGhost02);
            Game.ObjectManager.AddObject(RedGhost03);
            Game.ObjectManager.AddObject(RTred01);
            Game.ObjectManager.AddObject(RTred02);
            Game.ObjectManager.AddObject(RTredBig);
            Game.ObjectManager.AddObject(Golem01);
            Game.ObjectManager.AddObject(Golem02);
        }

        private void Dragon()
        {
            var dragon = new Monster(
                Game,
                9459.52f,
                4193.03f,
                9460.52f,
                4194.03f,
                "Dragon",
                "Dragon"
                );
            Game.ObjectManager.AddObject(dragon);
        }

        private void Baron()
        {
            var Baron = new Monster(
                Game,
                4600.495f,
                10250.462f,
                4599.495f,
                10249.462f,
                "Worm",
                "Worm"
                );
            Game.ObjectManager.AddObject(Baron);
        }
    }
}

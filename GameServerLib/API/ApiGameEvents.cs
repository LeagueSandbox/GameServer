﻿using GameServerCore.Domain.GameObjects;
using LeaguePackets.Game.Events;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.API
{
    public static class ApiGameEvents
    {
        private static Game _game;
        private static ILog _logger;
        public static void SetGame(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
        }

        public static void AnnounceCaptureAltar(IMinion altar, byte index)
        {
            _game.PacketNotifier.NotifyS2C_OnEventWorld(new OnCaptureAltar() { CapturePoint = index, OtherNetID = altar.NetId });
        }

        public static void AnnounceCapturePointCaptured(ILaneTurret turret, char point, IChampion captor = null)
        {
            IEvent captured;
            switch (char.ToUpper(point))
            {
                case 'A':
                    captured = new OnCapturePointCaptured_A();
                    break;
                case 'B':
                    captured = new OnCapturePointCaptured_B();
                    break;
                case 'C':
                    captured = new OnCapturePointCaptured_C();
                    break;
                case 'D':
                    captured = new OnCapturePointCaptured_D();
                    break;
                case 'E':
                    captured = new OnCapturePointCaptured_E();
                    break;
                default:
                    _logger.Warn($"Announcement with Id {point} doesn't exist! Please use numbers between 1 and 5");
                    return;
            }

            if (captor != null)
            {
                captured.OtherNetID = captor.NetId;
            }

            _game.PacketNotifier.NotifyOnEvent(captured, turret);
        }

        public static void AnnounceCapturePointNeutralized(ILaneTurret turret, char point)
        {
            IEvent neutralized;
            switch (char.ToUpper(point))
            {
                case 'A':
                    neutralized = new OnCapturePointNeutralized_A();
                    break;
                case 'B':
                    neutralized = new OnCapturePointNeutralized_B();
                    break;
                case 'C':
                    neutralized = new OnCapturePointNeutralized_C();
                    break;
                case 'D':
                    neutralized = new OnCapturePointNeutralized_D();
                    break;
                case 'E':
                    neutralized = new OnCapturePointNeutralized_E();
                    break;
                default:
                    _logger.Warn($"Announcement with Id {point} doesn't exist! Please use numbers between 1 and 5");
                    return;
            }

            _game.PacketNotifier.NotifyOnEvent(neutralized, turret);
        }

        public static void AnnounceChampionAscended(IChampion champion)
        {
            _game.PacketNotifier.NotifyS2C_OnEventWorld(new OnChampionAscended() { OtherNetID = champion.NetId }, champion);
        }

        public static void AnnounceClearAscended()
        {
            _game.PacketNotifier.NotifyS2C_OnEventWorld(new OnClearAscended());
            ApiMapFunctionManager.NotifyAscendant();
        }

        public static void AnnounceMinionAscended(IMinion minion)
        {
            _game.PacketNotifier.NotifyS2C_OnEventWorld(new OnMinionAscended() { OtherNetID = minion.NetId }, minion);
        }

        public static void AnnounceMinionsSpawn()
        {
            _game.PacketNotifier.NotifyS2C_OnEventWorld(new OnMinionsSpawn());
        }

        public static void AnnouceNexusCrystalStart()
        {
            _game.PacketNotifier.NotifyS2C_OnEventWorld(new OnNexusCrystalStart());
        }

        public static void AnnounceStartGameMessage(int message, int map = 0)
        {
            IEventEmptyHistory annoucement;
            switch (message)
            {
                case 1:
                    annoucement = new OnStartGameMessage1() { MapNumber = map };
                    break;
                case 2:
                    annoucement = new OnStartGameMessage2() { MapNumber = map };
                    break;
                case 3:
                    annoucement = new OnStartGameMessage3() { MapNumber = map };
                    break;
                case 4:
                    annoucement = new OnStartGameMessage4() { MapNumber = map };
                    break;
                case 5:
                    annoucement = new OnStartGameMessage5() { MapNumber = map };
                    break;
                default:
                    _logger.Warn($"Announcement with Id {message} doesn't exist! Please use numbers between 1 and 5");
                    return;
            }

            _game.PacketNotifier.NotifyS2C_OnEventWorld(annoucement);
        }

        public static void AnnounceVictoryPointThreshold(ILaneTurret turret, int index)
        {
            IEventEmptyHistory pointThreshHold;
            switch (index)
            {
                case 1:
                    pointThreshHold = new OnVictoryPointThreshold1();
                    break;
                case 2:
                    pointThreshHold = new OnVictoryPointThreshold2();
                    break;
                case 3:
                    pointThreshHold = new OnVictoryPointThreshold3();
                    break;
                case 4:
                    pointThreshHold = new OnVictoryPointThreshold4();
                    break;
                default:
                    _logger.Warn($"Announcement with Id {index} doesn't exist! Please use numbers between 1 and 5");
                    return;
            }

            _game.PacketNotifier.NotifyOnEvent(pointThreshHold, turret);
        }
    }
}

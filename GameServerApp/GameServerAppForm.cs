using GameServerApp.Configs;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using log4net;
using log4net.Appender;
using log4net.Core;
using System.Timers;
using Timer = System.Timers.Timer;
using log4net.Layout;
using System.Collections;

namespace GameServerApp
{
    public partial class GameServerAppForm : Form
    {
        LeagueSandbox.GameServer.Logging.ILogger logger = LoggerProvider.GetLogger();
        const string CONFIG_PATH = "./GameServerAppConfig.json";
        MemoryAppender memoryAppender;
        List<LoggingEvent> logEvents = new List<LoggingEvent>();
        GameServerAppConfig gameServerAppConfig;
        string blowfishKey = "17BLOhi6KZsTtldTsizvHg==";
        GameServerLauncher gameServerLauncher;
        Thread gameServerThread = null;
        Timer consoleTimer;

        public GameServerAppForm()
        {
            InitializeComponent();
            gameServerAppConfig = new GameServerAppConfig();
            gameServerAppConfig.LoadFromFile(CONFIG_PATH);
            gamePathTxt.Text = gameServerAppConfig.GamePath;
            gameModeCombo.Text = gameServerAppConfig.GameMode;
            contentPathTxt.Text = gameServerAppConfig.ContentPath;
            rankCombo.Text = gameServerAppConfig.Rank;
            championCombo.Text = gameServerAppConfig.Champion;
            nameTxt.Text = gameServerAppConfig.Name;
            mapCombo.Text = gameServerAppConfig.Map.ToString();
            teamCombo.Text = gameServerAppConfig.Team;
            iconCombo.Text = gameServerAppConfig.Icon.ToString();
            skinCombo.Text = gameServerAppConfig.Skin.ToString();
            ribbonCombo.Text = gameServerAppConfig.Ribbon.ToString();
            summoner1Combo.Text = gameServerAppConfig.Summoner1;
            summoner2Combo.Text = gameServerAppConfig.Summoner2;
            cooldownsCheckBox.Checked = gameServerAppConfig.EnableCooldowns;
            spawnMinionsCheckBox.Checked = gameServerAppConfig.EnableSpawnMinions;
            manaCostsCheckBox.Checked = gameServerAppConfig.EnableManaCosts;
            enableCheatsCheckBox.Checked = gameServerAppConfig.EnableCheats;
            consoleInfoLogsCheckBox.Checked = gameServerAppConfig.DisplayInfoLogs;
            consoleDebugLogsCheckBox.Checked = gameServerAppConfig.DisplayDebugLogs;
            consoleWarningLogsCheckBox.Checked = gameServerAppConfig.DisplayWarningLogs;
            consoleErrorLogsCheckBox.Checked = gameServerAppConfig.DisplayErrorLogs;
            autoStartServerOnLaunchCheckBox.Checked = gameServerAppConfig.AutoStartServerOnLaunch;
            autoStartGameWithServerCheckBox.Checked = gameServerAppConfig.AutoStartGameWithServer;

            memoryAppender = new MemoryAppender();
            memoryAppender.Layout = new PatternLayout("%date %message%newline");
            var repository = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
            repository.Root.AddAppender(memoryAppender);
            
            consoleTimer = new Timer(50);
            consoleTimer.SynchronizingObject = this;
            consoleTimer.Elapsed += (object o, ElapsedEventArgs e) => GetNewLogEvents();
            consoleTimer.AutoReset = true;
            consoleTimer.Start();
        }

        private void GetNewLogEvents()
        {
            var events = memoryAppender.GetEvents();
            if (events.Count() == 0)
            {
                return;
            }
            logEvents.AddRange(events);
            consoleTextBox.AppendText(GetLogEventsAsString(events));
            memoryAppender.Clear();
        }

        private void RedrawConsoleText()
        {
            var result = GetLogEventsAsString(logEvents);
            consoleTextBox.Text = "";
            consoleTextBox.AppendText(result);
        }

        private string GetLogEventsAsString(IEnumerable<LoggingEvent> list)
        {
            using (var writer = new System.IO.StringWriter())
            {
                foreach (var ev in list)
                {
                    if (ev.Level == Level.Debug && consoleDebugLogsCheckBox.Checked ||
                        ev.Level == Level.Info && consoleInfoLogsCheckBox.Checked ||
                        ev.Level == Level.Warn && consoleWarningLogsCheckBox.Checked ||
                        ev.Level == Level.Error && consoleErrorLogsCheckBox.Checked)
                    {
                        memoryAppender.Layout.Format(writer, ev);
                    }
                }
                return writer.ToString();
            }
        }

        private void GameServerAppForm_Load(object sender, EventArgs e)
        {

        }

        private void gamePathTxt_TextChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.GamePath = gamePathTxt.Text;
            SaveConfigSettings();
        }

        private void SaveConfigSettings()
        {
            gameServerAppConfig.SaveToFile(CONFIG_PATH);
        }

        private void gameModeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.GameMode = gameModeCombo.Text;
            SaveConfigSettings();
        }

        private void contentPathTxt_TextChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.ContentPath = contentPathTxt.Text;
            SaveConfigSettings();
        }

        private void rankCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.Rank = rankCombo.Text;
            SaveConfigSettings();
        }

        private void championCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.Champion = championCombo.Text;
            SaveConfigSettings();
        }

        private void nameTxt_TextChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.Name = nameTxt.Text;
            SaveConfigSettings();
        }

        private void mapCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(mapCombo.Text, out int value))
            {
                gameServerAppConfig.Map = value;
                SaveConfigSettings();
            }
        }

        private void teamCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.Team = teamCombo.Text;
            SaveConfigSettings();
        }

        private void cooldownsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.EnableCooldowns = cooldownsCheckBox.Checked;
            SaveConfigSettings();
        }

        private void spawnMinionsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.EnableSpawnMinions = spawnMinionsCheckBox.Checked;
            SaveConfigSettings();
        }

        private void manaCostsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.EnableManaCosts = manaCostsCheckBox.Checked;
            SaveConfigSettings();
        }

        private void enableCheatsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.EnableCheats = enableCheatsCheckBox.Checked;
            SaveConfigSettings();
        }

        private void iconCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(iconCombo.Text, out int value))
            {
                gameServerAppConfig.Icon = value;
                SaveConfigSettings();
            }
        }

        private void skinCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(skinCombo.Text, out int value))
            {
                gameServerAppConfig.Skin = value;
                SaveConfigSettings();
            }
        }

        private void ribbonCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(ribbonCombo.Text, out int value))
            {
                gameServerAppConfig.Ribbon = value;
                SaveConfigSettings();
            }
        }

        private void summoner1Combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.Summoner1 = summoner1Combo.Text;
            SaveConfigSettings();
        }

        private void summoner2Combo_SelectedIndexChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.Summoner2 = summoner2Combo.Text;
            SaveConfigSettings();
        }

        private void consoleInfoLogsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.DisplayInfoLogs = consoleInfoLogsCheckBox.Checked;
            RedrawConsoleText();
            SaveConfigSettings();
        }

        private void consoleDebugLogsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.DisplayDebugLogs = consoleDebugLogsCheckBox.Checked;
            RedrawConsoleText();
            SaveConfigSettings();
        }

        private void consoleWarningLogsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.DisplayWarningLogs = consoleWarningLogsCheckBox.Checked;
            RedrawConsoleText();
            SaveConfigSettings();
        }

        private void consoleErrorLogsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.DisplayErrorLogs = consoleErrorLogsCheckBox.Checked;
            RedrawConsoleText();
            SaveConfigSettings();
        }

        private void autoStartServerOnLaunchCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.AutoStartGameWithServer = autoStartServerOnLaunchCheckBox.Checked;
            SaveConfigSettings();
        }

        private void autoStartGameWithServerCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.AutoStartGameWithServer = autoStartGameWithServerCheckBox.Checked;
            SaveConfigSettings();
        }

        private void startServerButton_Click(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                gameServerLauncher = new GameServerLauncher(5119, gameServerAppConfig.CreateGameServerConfig(), blowfishKey);
                gameServerThread = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    gameServerLauncher.StartNetworkLoop();
                });
                gameServerThread.Start();
            }).Start();
        }
    }
}

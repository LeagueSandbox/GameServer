using GameServerApp.Configs;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using log4net;
using log4net.Appender;
using log4net.Core;
using System.Timers;
using Timer = System.Timers.Timer;
using log4net.Layout;
using System.IO;

namespace GameServerApp
{
    public partial class GameServerAppForm : Form
    {
        LeagueSandbox.GameServer.Logging.ILogger logger = LoggerProvider.GetLogger();
        const string CONFIG_PATH = "./GameServerAppConfig.json";
        const int SERVER_PORT = 5119;
        MemoryAppender memoryAppender;
        List<LoggingEvent> logEvents = new List<LoggingEvent>();
        GameServerAppConfig gameServerAppConfig;
        string blowfishKey = "17BLOhi6KZsTtldTsizvHg==";
        GameServerLauncher gameServerLauncher;
        Thread gameServerThread = null;
        Timer consoleTimer;
        Font boldFont = new Font("Verdana", 10, FontStyle.Bold);
        Font regularFont = new Font("Verdana", 10, FontStyle.Regular);
        Process leagueClientProcess = null;

        public GameServerAppForm()
        {
            InitializeComponent();
            rankCombo.TextChanged += rankCombo_SelectedIndexChanged;
            championCombo.TextChanged += championCombo_SelectedIndexChanged;
            gameModeCombo.TextChanged += gameModeCombo_SelectedIndexChanged;
            mapCombo.TextChanged += mapCombo_SelectedIndexChanged;
            teamCombo.TextChanged += teamCombo_SelectedIndexChanged;
            iconCombo.TextChanged += iconCombo_SelectedIndexChanged;
            skinCombo.TextChanged += skinCombo_SelectedIndexChanged;
            ribbonCombo.TextChanged += ribbonCombo_SelectedIndexChanged;
            summoner1Combo.TextChanged += summoner1Combo_SelectedIndexChanged;
            summoner2Combo.TextChanged += summoner2Combo_SelectedIndexChanged;

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

            FormClosed += (o, e) =>
            {
                stopClient();
            };
        }

        private void GameServerAppForm_Shown(object sender, EventArgs e)
        {
            memoryAppender = new MemoryAppender();
            memoryAppender.Layout = new PatternLayout("%date %message%newline");
            var repository = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
            repository.Root.AddAppender(memoryAppender);

            consoleTimer = new Timer(50);
            consoleTimer.SynchronizingObject = this;
            consoleTimer.Elapsed += (object o, ElapsedEventArgs e2) => GetNewLogEvents();
            consoleTimer.AutoReset = true;
            consoleTimer.Start();

            if (autoStartServerOnLaunchCheckBox.Checked)
            {
                startServer();
            }
        }

        private void GetNewLogEvents()
        {
            var events = memoryAppender.GetEvents();
            if (events.Count() == 0)
            {
                return;
            }
            memoryAppender.Clear();
            this.Invoke((MethodInvoker)delegate
            {
                logEvents.AddRange(events);
                WriteEventsToRichTextBox(events);
            });
        }

        private void RedrawConsoleText()
        {
            consoleTextBox.Text = "";
            WriteEventsToRichTextBox(logEvents);
        }

        private void WriteEventsToRichTextBox(IEnumerable<LoggingEvent> list)
        {
            foreach (var ev in list)
            {
                if (ev.Level == Level.Debug && consoleDebugLogsCheckBox.Checked ||
                    ev.Level == Level.Info && consoleInfoLogsCheckBox.Checked ||
                    ev.Level == Level.Warn && consoleWarningLogsCheckBox.Checked ||
                    ev.Level == Level.Error && consoleErrorLogsCheckBox.Checked)
                {
                    consoleTextBox.SelectionColor = Color.Black;
                    consoleTextBox.SelectionFont = regularFont;
                    consoleTextBox.AppendText(ev.TimeStamp.ToLongTimeString() + " " + GetStackFrameString(ev.LocationInformation.StackFrames[1]));
                    consoleTextBox.AppendText(Environment.NewLine);
                    if (ev.Level == Level.Debug)
                    {
                        consoleTextBox.SelectionColor = Color.Blue;
                    }
                    else
                    if (ev.Level == Level.Info)
                    {
                        consoleTextBox.SelectionColor = Color.Green;
                    }
                    else
                    if (ev.Level == Level.Warn)
                    {
                        consoleTextBox.SelectionColor = Color.DarkOrange;
                    }
                    else
                    if (ev.Level == Level.Error)
                    {
                        consoleTextBox.SelectionColor = Color.Red;
                    }
                    consoleTextBox.SelectionFont = boldFont;
                    consoleTextBox.AppendText("[" + ev.Level.DisplayName + "] " + ev.RenderedMessage);
                    consoleTextBox.AppendText(Environment.NewLine);
                }
            }
            consoleTextBox.SelectionStart = consoleTextBox.Text.Length;
            consoleTextBox.ScrollToCaret();
        }

        private string GetStackFrameString(StackFrameItem item)
        {
            return "(" + item.ClassName + "." + item.Method.Name + ":" + item.LineNumber + ")";
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
            if (gameServerLauncher == null)
            {
                startServer();
            }
            else
            {
                stopServer();
            }
        }

        private void startServer()
        {
            startServerButton.Text = "Stop Server";
            startServerButton.BackColor = Color.LightBlue;
            new Thread(() =>
            {
                gameServerLauncher = new GameServerLauncher(SERVER_PORT, gameServerAppConfig.CreateGameServerConfig(), blowfishKey, () =>
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (gameServerAppConfig.AutoStartGameWithServer)
                        {
                            startClient();
                        }
                    });
                });
                gameServerThread = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    gameServerLauncher.StartNetworkLoop();
                });
                gameServerThread.Start();
            }).Start();
        }

        private void stopServer()
        {
            startServerButton.Text = "Start Server";
            startServerButton.BackColor = Color.LightCoral;
            gameServerLauncher.Stop();
            gameServerLauncher = null;
            stopClient();
        }

        private void startGameButton_Click(object sender, EventArgs e)
        {
            if (leagueClientProcess == null)
            {
                startClient();
            }
            else
            {
                stopClient();
            }
        }

        private void startClient()
        {
            stopClient();
            string leaguePath = gameServerAppConfig.GamePath;
            if (Directory.Exists(leaguePath))
            {
                leaguePath = Path.Combine(leaguePath, "League of Legends.exe");
            }
            if (File.Exists(leaguePath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(leaguePath);
                startInfo.Arguments = String.Format("\"8394\" \"LoLLauncher.exe\" \"\" \"127.0.0.1 {0} {1} 1\"", SERVER_PORT, blowfishKey);
                startInfo.WorkingDirectory = Path.GetDirectoryName(leaguePath);
                leagueClientProcess = Process.Start(startInfo);
                logger.Info("Launching League of Legends.");

                startGameButton.Text = "Stop Client";
                startGameButton.BackColor = Color.LightBlue;
            }
            else
            {
                logger.Info("Unable to find League of Legends.exe. Check the GameServerSettings.json settings and your League location.");
            }
        }

        private void stopClient()
        {
            if (leagueClientProcess != null && !leagueClientProcess.HasExited)
            {
                leagueClientProcess.Kill();
                leagueClientProcess = null;
                startGameButton.Text = "Start Client";
                startGameButton.BackColor = Color.LightCoral;
                logger.Info("Closed League of Legends.");
            }
        }
    }
}

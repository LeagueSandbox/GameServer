using GameServerApp.Configs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameServerApp
{
    public partial class GameServerAppForm : Form
    {
        const string CONFIG_PATH = "./GameServerAppConfig.json";
        GameServerAppConfig gameServerAppConfig;

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
        }

        private void GameServerAppForm_Load(object sender, EventArgs e)
        {

        }
    }
}

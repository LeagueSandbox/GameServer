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
            SaveConfigSettings();
        }

        private void consoleDebugLogsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.DisplayDebugLogs = consoleDebugLogsCheckBox.Checked;
            SaveConfigSettings();
        }

        private void consoleWarningLogsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.DisplayWarningLogs = consoleWarningLogsCheckBox.Checked;
            SaveConfigSettings();
        }

        private void consoleErrorLogsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            gameServerAppConfig.DisplayErrorLogs = consoleErrorLogsCheckBox.Checked;
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
    }
}

namespace GameServerApp
{
    partial class GameServerAppForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameServerAppForm));
            this.autoStartServerOnLaunchCheckBox = new System.Windows.Forms.CheckBox();
            this.startServerButton = new System.Windows.Forms.Button();
            this.autoStartGameWithServerCheckBox = new System.Windows.Forms.CheckBox();
            this.startGameButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.gamePathTxt = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.gameModeCombo = new System.Windows.Forms.ComboBox();
            this.mapCombo = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.manaCostsCheckBox = new System.Windows.Forms.CheckBox();
            this.cooldownsCheckBox = new System.Windows.Forms.CheckBox();
            this.enableCheatsCheckBox = new System.Windows.Forms.CheckBox();
            this.spawnMinionsCheckBox = new System.Windows.Forms.CheckBox();
            this.consoleTextBox = new System.Windows.Forms.TextBox();
            this.consoleInfoLogsCheckBox = new System.Windows.Forms.CheckBox();
            this.label12 = new System.Windows.Forms.Label();
            this.consoleDebugLogsCheckBox = new System.Windows.Forms.CheckBox();
            this.consoleWarningLogsCheckBox = new System.Windows.Forms.CheckBox();
            this.consoleErrorLogsCheckBox = new System.Windows.Forms.CheckBox();
            this.nameTxt = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.serverHostCombo = new System.Windows.Forms.ComboBox();
            this.allowOnlineConnectionsCheckBox = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.connectedIPsListBox = new System.Windows.Forms.ListBox();
            this.label15 = new System.Windows.Forms.Label();
            this.addIPButton = new System.Windows.Forms.Button();
            this.removeIPButton = new System.Windows.Forms.Button();
            this.blockIPToggleButton = new System.Windows.Forms.Button();
            this.requestOtherConnectionsButton = new System.Windows.Forms.Button();
            this.label16 = new System.Windows.Forms.Label();
            this.lobbyListBox = new System.Windows.Forms.ListBox();
            this.closeLobbyCheckBox = new System.Windows.Forms.CheckBox();
            this.label17 = new System.Windows.Forms.Label();
            this.onlineChatTextBox = new System.Windows.Forms.TextBox();
            this.chatMessageTextBox = new System.Windows.Forms.TextBox();
            this.sendChatButton = new System.Windows.Forms.Button();
            this.championCombo = new System.Windows.Forms.ComboBox();
            this.rankCombo = new System.Windows.Forms.ComboBox();
            this.teamCombo = new System.Windows.Forms.ComboBox();
            this.ribbonCombo = new System.Windows.Forms.ComboBox();
            this.summoner1Combo = new System.Windows.Forms.ComboBox();
            this.summoner2Combo = new System.Windows.Forms.ComboBox();
            this.iconCombo = new System.Windows.Forms.ComboBox();
            this.skinCombo = new System.Windows.Forms.ComboBox();
            this.label18 = new System.Windows.Forms.Label();
            this.contentPathTxt = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // autoStartServerOnLaunchCheckBox
            // 
            this.autoStartServerOnLaunchCheckBox.AutoSize = true;
            this.autoStartServerOnLaunchCheckBox.Checked = true;
            this.autoStartServerOnLaunchCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoStartServerOnLaunchCheckBox.Location = new System.Drawing.Point(222, 873);
            this.autoStartServerOnLaunchCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.autoStartServerOnLaunchCheckBox.Name = "autoStartServerOnLaunchCheckBox";
            this.autoStartServerOnLaunchCheckBox.Size = new System.Drawing.Size(315, 29);
            this.autoStartServerOnLaunchCheckBox.TabIndex = 0;
            this.autoStartServerOnLaunchCheckBox.Text = "Auto Start Server on Launch";
            this.autoStartServerOnLaunchCheckBox.UseVisualStyleBackColor = true;
            this.autoStartServerOnLaunchCheckBox.CheckedChanged += new System.EventHandler(this.autoStartServerOnLaunchCheckBox_CheckedChanged);
            // 
            // startServerButton
            // 
            this.startServerButton.BackColor = System.Drawing.Color.LightCoral;
            this.startServerButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startServerButton.Location = new System.Drawing.Point(556, 865);
            this.startServerButton.Margin = new System.Windows.Forms.Padding(6);
            this.startServerButton.Name = "startServerButton";
            this.startServerButton.Size = new System.Drawing.Size(234, 98);
            this.startServerButton.TabIndex = 1;
            this.startServerButton.Text = "Start Server";
            this.startServerButton.UseVisualStyleBackColor = false;
            this.startServerButton.Click += new System.EventHandler(this.startServerButton_Click);
            // 
            // autoStartGameWithServerCheckBox
            // 
            this.autoStartGameWithServerCheckBox.AutoSize = true;
            this.autoStartGameWithServerCheckBox.Checked = true;
            this.autoStartGameWithServerCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoStartGameWithServerCheckBox.Location = new System.Drawing.Point(1080, 873);
            this.autoStartGameWithServerCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.autoStartGameWithServerCheckBox.Name = "autoStartGameWithServerCheckBox";
            this.autoStartGameWithServerCheckBox.Size = new System.Drawing.Size(315, 29);
            this.autoStartGameWithServerCheckBox.TabIndex = 2;
            this.autoStartGameWithServerCheckBox.Text = "Auto Start Game with Server";
            this.autoStartGameWithServerCheckBox.UseVisualStyleBackColor = true;
            this.autoStartGameWithServerCheckBox.CheckedChanged += new System.EventHandler(this.autoStartGameWithServerCheckBox_CheckedChanged);
            // 
            // startGameButton
            // 
            this.startGameButton.BackColor = System.Drawing.Color.LightCoral;
            this.startGameButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startGameButton.Location = new System.Drawing.Point(802, 865);
            this.startGameButton.Margin = new System.Windows.Forms.Padding(6);
            this.startGameButton.Name = "startGameButton";
            this.startGameButton.Size = new System.Drawing.Size(234, 98);
            this.startGameButton.TabIndex = 3;
            this.startGameButton.Text = "Start Game";
            this.startGameButton.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 17);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 25);
            this.label1.TabIndex = 4;
            this.label1.Text = "Game Path";
            // 
            // gamePathTxt
            // 
            this.gamePathTxt.Location = new System.Drawing.Point(136, 12);
            this.gamePathTxt.Margin = new System.Windows.Forms.Padding(4);
            this.gamePathTxt.Name = "gamePathTxt";
            this.gamePathTxt.Size = new System.Drawing.Size(518, 31);
            this.gamePathTxt.TabIndex = 5;
            this.gamePathTxt.Text = "C:\\LeagueSandbox\\League_Sandbox_Client";
            this.gamePathTxt.TextChanged += new System.EventHandler(this.gamePathTxt_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 65);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 25);
            this.label2.TabIndex = 6;
            this.label2.Text = "Rank";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1004, 65);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 25);
            this.label3.TabIndex = 8;
            this.label3.Text = "Team";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 117);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 25);
            this.label4.TabIndex = 10;
            this.label4.Text = "Icon";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(246, 65);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(109, 25);
            this.label5.TabIndex = 12;
            this.label5.Text = "Champion";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(580, 117);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(133, 25);
            this.label6.TabIndex = 14;
            this.label6.Text = "Summoner 1";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(904, 117);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(133, 25);
            this.label7.TabIndex = 16;
            this.label7.Text = "Summoner 2";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(196, 117);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(54, 25);
            this.label8.TabIndex = 18;
            this.label8.Text = "Skin";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(374, 117);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(80, 25);
            this.label9.TabIndex = 20;
            this.label9.Text = "Ribbon";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(666, 17);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(129, 25);
            this.label10.TabIndex = 22;
            this.label10.Text = "Game Mode";
            // 
            // gameModeCombo
            // 
            this.gameModeCombo.FormattingEnabled = true;
            this.gameModeCombo.Location = new System.Drawing.Point(800, 12);
            this.gameModeCombo.Margin = new System.Windows.Forms.Padding(4);
            this.gameModeCombo.Name = "gameModeCombo";
            this.gameModeCombo.Size = new System.Drawing.Size(292, 33);
            this.gameModeCombo.TabIndex = 23;
            this.gameModeCombo.Text = "LeagueSandbox-Default";
            this.gameModeCombo.SelectedIndexChanged += new System.EventHandler(this.gameModeCombo_SelectedIndexChanged);
            // 
            // mapCombo
            // 
            this.mapCombo.FormattingEnabled = true;
            this.mapCombo.Location = new System.Drawing.Point(886, 60);
            this.mapCombo.Margin = new System.Windows.Forms.Padding(4);
            this.mapCombo.Name = "mapCombo";
            this.mapCombo.Size = new System.Drawing.Size(106, 33);
            this.mapCombo.TabIndex = 25;
            this.mapCombo.Text = "1";
            this.mapCombo.SelectedIndexChanged += new System.EventHandler(this.mapCombo_SelectedIndexChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(828, 65);
            this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(54, 25);
            this.label11.TabIndex = 24;
            this.label11.Text = "Map";
            // 
            // manaCostsCheckBox
            // 
            this.manaCostsCheckBox.AutoSize = true;
            this.manaCostsCheckBox.Location = new System.Drawing.Point(1240, 115);
            this.manaCostsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.manaCostsCheckBox.Name = "manaCostsCheckBox";
            this.manaCostsCheckBox.Size = new System.Drawing.Size(159, 29);
            this.manaCostsCheckBox.TabIndex = 26;
            this.manaCostsCheckBox.Text = "Mana Costs";
            this.manaCostsCheckBox.UseVisualStyleBackColor = true;
            this.manaCostsCheckBox.CheckedChanged += new System.EventHandler(this.manaCostsCheckBox_CheckedChanged);
            // 
            // cooldownsCheckBox
            // 
            this.cooldownsCheckBox.AutoSize = true;
            this.cooldownsCheckBox.Location = new System.Drawing.Point(1240, 65);
            this.cooldownsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.cooldownsCheckBox.Name = "cooldownsCheckBox";
            this.cooldownsCheckBox.Size = new System.Drawing.Size(150, 29);
            this.cooldownsCheckBox.TabIndex = 27;
            this.cooldownsCheckBox.Text = "Cooldowns";
            this.cooldownsCheckBox.UseVisualStyleBackColor = true;
            this.cooldownsCheckBox.CheckedChanged += new System.EventHandler(this.cooldownsCheckBox_CheckedChanged);
            // 
            // enableCheatsCheckBox
            // 
            this.enableCheatsCheckBox.AutoSize = true;
            this.enableCheatsCheckBox.Location = new System.Drawing.Point(1412, 115);
            this.enableCheatsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.enableCheatsCheckBox.Name = "enableCheatsCheckBox";
            this.enableCheatsCheckBox.Size = new System.Drawing.Size(185, 29);
            this.enableCheatsCheckBox.TabIndex = 28;
            this.enableCheatsCheckBox.Text = "Enable Cheats";
            this.enableCheatsCheckBox.UseVisualStyleBackColor = true;
            this.enableCheatsCheckBox.CheckedChanged += new System.EventHandler(this.enableCheatsCheckBox_CheckedChanged);
            // 
            // spawnMinionsCheckBox
            // 
            this.spawnMinionsCheckBox.AutoSize = true;
            this.spawnMinionsCheckBox.Location = new System.Drawing.Point(1412, 63);
            this.spawnMinionsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.spawnMinionsCheckBox.Name = "spawnMinionsCheckBox";
            this.spawnMinionsCheckBox.Size = new System.Drawing.Size(190, 29);
            this.spawnMinionsCheckBox.TabIndex = 29;
            this.spawnMinionsCheckBox.Text = "Spawn Minions";
            this.spawnMinionsCheckBox.UseVisualStyleBackColor = true;
            this.spawnMinionsCheckBox.CheckedChanged += new System.EventHandler(this.spawnMinionsCheckBox_CheckedChanged);
            // 
            // consoleTextBox
            // 
            this.consoleTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.consoleTextBox.Location = new System.Drawing.Point(18, 206);
            this.consoleTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.consoleTextBox.Multiline = true;
            this.consoleTextBox.Name = "consoleTextBox";
            this.consoleTextBox.Size = new System.Drawing.Size(1544, 646);
            this.consoleTextBox.TabIndex = 30;
            // 
            // consoleInfoLogsCheckBox
            // 
            this.consoleInfoLogsCheckBox.AutoSize = true;
            this.consoleInfoLogsCheckBox.Checked = true;
            this.consoleInfoLogsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.consoleInfoLogsCheckBox.Location = new System.Drawing.Point(136, 163);
            this.consoleInfoLogsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.consoleInfoLogsCheckBox.Name = "consoleInfoLogsCheckBox";
            this.consoleInfoLogsCheckBox.Size = new System.Drawing.Size(132, 29);
            this.consoleInfoLogsCheckBox.TabIndex = 31;
            this.consoleInfoLogsCheckBox.Text = "Info Logs";
            this.consoleInfoLogsCheckBox.UseVisualStyleBackColor = true;
            this.consoleInfoLogsCheckBox.CheckedChanged += new System.EventHandler(this.consoleInfoLogsCheckBox_CheckedChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(18, 162);
            this.label12.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(103, 29);
            this.label12.TabIndex = 32;
            this.label12.Text = "Console";
            // 
            // consoleDebugLogsCheckBox
            // 
            this.consoleDebugLogsCheckBox.AutoSize = true;
            this.consoleDebugLogsCheckBox.Location = new System.Drawing.Point(296, 163);
            this.consoleDebugLogsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.consoleDebugLogsCheckBox.Name = "consoleDebugLogsCheckBox";
            this.consoleDebugLogsCheckBox.Size = new System.Drawing.Size(160, 29);
            this.consoleDebugLogsCheckBox.TabIndex = 33;
            this.consoleDebugLogsCheckBox.Text = "Debug Logs";
            this.consoleDebugLogsCheckBox.UseVisualStyleBackColor = true;
            this.consoleDebugLogsCheckBox.CheckedChanged += new System.EventHandler(this.consoleDebugLogsCheckBox_CheckedChanged);
            // 
            // consoleWarningLogsCheckBox
            // 
            this.consoleWarningLogsCheckBox.AutoSize = true;
            this.consoleWarningLogsCheckBox.Location = new System.Drawing.Point(488, 163);
            this.consoleWarningLogsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.consoleWarningLogsCheckBox.Name = "consoleWarningLogsCheckBox";
            this.consoleWarningLogsCheckBox.Size = new System.Drawing.Size(177, 29);
            this.consoleWarningLogsCheckBox.TabIndex = 34;
            this.consoleWarningLogsCheckBox.Text = "Warning Logs";
            this.consoleWarningLogsCheckBox.UseVisualStyleBackColor = true;
            this.consoleWarningLogsCheckBox.CheckedChanged += new System.EventHandler(this.consoleWarningLogsCheckBox_CheckedChanged);
            // 
            // consoleErrorLogsCheckBox
            // 
            this.consoleErrorLogsCheckBox.AutoSize = true;
            this.consoleErrorLogsCheckBox.Location = new System.Drawing.Point(700, 163);
            this.consoleErrorLogsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.consoleErrorLogsCheckBox.Name = "consoleErrorLogsCheckBox";
            this.consoleErrorLogsCheckBox.Size = new System.Drawing.Size(144, 29);
            this.consoleErrorLogsCheckBox.TabIndex = 35;
            this.consoleErrorLogsCheckBox.Text = "Error Logs";
            this.consoleErrorLogsCheckBox.UseVisualStyleBackColor = true;
            this.consoleErrorLogsCheckBox.CheckedChanged += new System.EventHandler(this.consoleErrorLogsCheckBox_CheckedChanged);
            // 
            // nameTxt
            // 
            this.nameTxt.Location = new System.Drawing.Point(622, 58);
            this.nameTxt.Margin = new System.Windows.Forms.Padding(4);
            this.nameTxt.Name = "nameTxt";
            this.nameTxt.Size = new System.Drawing.Size(194, 31);
            this.nameTxt.TabIndex = 37;
            this.nameTxt.Text = "Gan";
            this.nameTxt.TextChanged += new System.EventHandler(this.nameTxt_TextChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(550, 65);
            this.label13.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(68, 25);
            this.label13.TabIndex = 36;
            this.label13.Text = "Name";
            // 
            // serverHostCombo
            // 
            this.serverHostCombo.Enabled = false;
            this.serverHostCombo.FormattingEnabled = true;
            this.serverHostCombo.Location = new System.Drawing.Point(1754, 706);
            this.serverHostCombo.Margin = new System.Windows.Forms.Padding(6);
            this.serverHostCombo.Name = "serverHostCombo";
            this.serverHostCombo.Size = new System.Drawing.Size(426, 33);
            this.serverHostCombo.TabIndex = 38;
            this.serverHostCombo.Text = "You";
            // 
            // allowOnlineConnectionsCheckBox
            // 
            this.allowOnlineConnectionsCheckBox.AutoSize = true;
            this.allowOnlineConnectionsCheckBox.Enabled = false;
            this.allowOnlineConnectionsCheckBox.Location = new System.Drawing.Point(1622, 19);
            this.allowOnlineConnectionsCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.allowOnlineConnectionsCheckBox.Name = "allowOnlineConnectionsCheckBox";
            this.allowOnlineConnectionsCheckBox.Size = new System.Drawing.Size(289, 29);
            this.allowOnlineConnectionsCheckBox.TabIndex = 39;
            this.allowOnlineConnectionsCheckBox.Text = "Allow Online Connections";
            this.allowOnlineConnectionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(1616, 712);
            this.label14.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(125, 25);
            this.label14.TabIndex = 40;
            this.label14.Text = "Server Host";
            // 
            // connectedIPsListBox
            // 
            this.connectedIPsListBox.Enabled = false;
            this.connectedIPsListBox.FormattingEnabled = true;
            this.connectedIPsListBox.ItemHeight = 25;
            this.connectedIPsListBox.Items.AddRange(new object[] {
            "17.281.281.1 - Some Guy - Online",
            "83.88.27.34 - Cheating Hacker - Blocked",
            "28.84.99.938 - Mr. Unreliable - Offline"});
            this.connectedIPsListBox.Location = new System.Drawing.Point(1622, 112);
            this.connectedIPsListBox.Margin = new System.Windows.Forms.Padding(6);
            this.connectedIPsListBox.Name = "connectedIPsListBox";
            this.connectedIPsListBox.Size = new System.Drawing.Size(558, 479);
            this.connectedIPsListBox.TabIndex = 41;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(1802, 67);
            this.label15.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(152, 25);
            this.label15.TabIndex = 42;
            this.label15.Text = "Connected IPs";
            // 
            // addIPButton
            // 
            this.addIPButton.Enabled = false;
            this.addIPButton.Location = new System.Drawing.Point(1622, 606);
            this.addIPButton.Margin = new System.Windows.Forms.Padding(6);
            this.addIPButton.Name = "addIPButton";
            this.addIPButton.Size = new System.Drawing.Size(150, 44);
            this.addIPButton.TabIndex = 43;
            this.addIPButton.Text = "Add";
            this.addIPButton.UseVisualStyleBackColor = true;
            // 
            // removeIPButton
            // 
            this.removeIPButton.Enabled = false;
            this.removeIPButton.Location = new System.Drawing.Point(2034, 606);
            this.removeIPButton.Margin = new System.Windows.Forms.Padding(6);
            this.removeIPButton.Name = "removeIPButton";
            this.removeIPButton.Size = new System.Drawing.Size(150, 44);
            this.removeIPButton.TabIndex = 44;
            this.removeIPButton.Text = "Remove";
            this.removeIPButton.UseVisualStyleBackColor = true;
            // 
            // blockIPToggleButton
            // 
            this.blockIPToggleButton.Enabled = false;
            this.blockIPToggleButton.Location = new System.Drawing.Point(1832, 606);
            this.blockIPToggleButton.Margin = new System.Windows.Forms.Padding(6);
            this.blockIPToggleButton.Name = "blockIPToggleButton";
            this.blockIPToggleButton.Size = new System.Drawing.Size(150, 44);
            this.blockIPToggleButton.TabIndex = 45;
            this.blockIPToggleButton.Text = "(Un) block";
            this.blockIPToggleButton.UseVisualStyleBackColor = true;
            // 
            // requestOtherConnectionsButton
            // 
            this.requestOtherConnectionsButton.Enabled = false;
            this.requestOtherConnectionsButton.Location = new System.Drawing.Point(1622, 652);
            this.requestOtherConnectionsButton.Margin = new System.Windows.Forms.Padding(6);
            this.requestOtherConnectionsButton.Name = "requestOtherConnectionsButton";
            this.requestOtherConnectionsButton.Size = new System.Drawing.Size(562, 44);
            this.requestOtherConnectionsButton.TabIndex = 46;
            this.requestOtherConnectionsButton.Text = "Request Other Connections (2 min)";
            this.requestOtherConnectionsButton.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(1802, 756);
            this.label16.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(148, 25);
            this.label16.TabIndex = 48;
            this.label16.Text = "Current Lobby";
            // 
            // lobbyListBox
            // 
            this.lobbyListBox.Enabled = false;
            this.lobbyListBox.FormattingEnabled = true;
            this.lobbyListBox.ItemHeight = 25;
            this.lobbyListBox.Items.AddRange(new object[] {
            "127.0.0.1 - Gan - Blue, Ezreal",
            "17.281.281.1 - Some Guy - Purple, Ezreal"});
            this.lobbyListBox.Location = new System.Drawing.Point(1622, 800);
            this.lobbyListBox.Margin = new System.Windows.Forms.Padding(6);
            this.lobbyListBox.Name = "lobbyListBox";
            this.lobbyListBox.Size = new System.Drawing.Size(558, 479);
            this.lobbyListBox.TabIndex = 47;
            // 
            // closeLobbyCheckBox
            // 
            this.closeLobbyCheckBox.AutoSize = true;
            this.closeLobbyCheckBox.Enabled = false;
            this.closeLobbyCheckBox.Location = new System.Drawing.Point(1622, 1290);
            this.closeLobbyCheckBox.Margin = new System.Windows.Forms.Padding(6);
            this.closeLobbyCheckBox.Name = "closeLobbyCheckBox";
            this.closeLobbyCheckBox.Size = new System.Drawing.Size(216, 29);
            this.closeLobbyCheckBox.TabIndex = 49;
            this.closeLobbyCheckBox.Text = "Close Lobby to all";
            this.closeLobbyCheckBox.UseVisualStyleBackColor = true;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(24, 960);
            this.label17.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(125, 25);
            this.label17.TabIndex = 50;
            this.label17.Text = "Online Chat";
            // 
            // onlineChatTextBox
            // 
            this.onlineChatTextBox.Enabled = false;
            this.onlineChatTextBox.Location = new System.Drawing.Point(24, 990);
            this.onlineChatTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.onlineChatTextBox.Multiline = true;
            this.onlineChatTextBox.Name = "onlineChatTextBox";
            this.onlineChatTextBox.Size = new System.Drawing.Size(1538, 289);
            this.onlineChatTextBox.TabIndex = 51;
            this.onlineChatTextBox.Text = "[9:46pm Aug 19, 2018] 17.281.281.1 - Some Guy: Hey Guys\r\n[9:47pm Aug 19, 2018] 12" +
    "7.0.0.1 - Gan: Hi\r\n";
            // 
            // chatMessageTextBox
            // 
            this.chatMessageTextBox.Enabled = false;
            this.chatMessageTextBox.Location = new System.Drawing.Point(22, 1290);
            this.chatMessageTextBox.Margin = new System.Windows.Forms.Padding(6);
            this.chatMessageTextBox.Name = "chatMessageTextBox";
            this.chatMessageTextBox.Size = new System.Drawing.Size(1384, 31);
            this.chatMessageTextBox.TabIndex = 52;
            // 
            // sendChatButton
            // 
            this.sendChatButton.Enabled = false;
            this.sendChatButton.Location = new System.Drawing.Point(1418, 1287);
            this.sendChatButton.Margin = new System.Windows.Forms.Padding(6);
            this.sendChatButton.Name = "sendChatButton";
            this.sendChatButton.Size = new System.Drawing.Size(150, 44);
            this.sendChatButton.TabIndex = 53;
            this.sendChatButton.Text = "Send";
            this.sendChatButton.UseVisualStyleBackColor = true;
            // 
            // championCombo
            // 
            this.championCombo.FormattingEnabled = true;
            this.championCombo.Location = new System.Drawing.Point(354, 60);
            this.championCombo.Margin = new System.Windows.Forms.Padding(4);
            this.championCombo.Name = "championCombo";
            this.championCombo.Size = new System.Drawing.Size(180, 33);
            this.championCombo.TabIndex = 54;
            this.championCombo.Text = "Ezreal";
            this.championCombo.SelectedIndexChanged += new System.EventHandler(this.championCombo_SelectedIndexChanged);
            // 
            // rankCombo
            // 
            this.rankCombo.FormattingEnabled = true;
            this.rankCombo.Location = new System.Drawing.Point(80, 60);
            this.rankCombo.Margin = new System.Windows.Forms.Padding(4);
            this.rankCombo.Name = "rankCombo";
            this.rankCombo.Size = new System.Drawing.Size(152, 33);
            this.rankCombo.TabIndex = 55;
            this.rankCombo.Text = "Diamond";
            this.rankCombo.SelectedIndexChanged += new System.EventHandler(this.rankCombo_SelectedIndexChanged);
            // 
            // teamCombo
            // 
            this.teamCombo.FormattingEnabled = true;
            this.teamCombo.Location = new System.Drawing.Point(1072, 62);
            this.teamCombo.Margin = new System.Windows.Forms.Padding(4);
            this.teamCombo.Name = "teamCombo";
            this.teamCombo.Size = new System.Drawing.Size(138, 33);
            this.teamCombo.TabIndex = 56;
            this.teamCombo.Text = "Blue";
            this.teamCombo.SelectedIndexChanged += new System.EventHandler(this.teamCombo_SelectedIndexChanged);
            // 
            // ribbonCombo
            // 
            this.ribbonCombo.FormattingEnabled = true;
            this.ribbonCombo.Location = new System.Drawing.Point(456, 112);
            this.ribbonCombo.Margin = new System.Windows.Forms.Padding(4);
            this.ribbonCombo.Name = "ribbonCombo";
            this.ribbonCombo.Size = new System.Drawing.Size(106, 33);
            this.ribbonCombo.TabIndex = 57;
            this.ribbonCombo.Text = "2";
            this.ribbonCombo.SelectedIndexChanged += new System.EventHandler(this.ribbonCombo_SelectedIndexChanged);
            // 
            // summoner1Combo
            // 
            this.summoner1Combo.FormattingEnabled = true;
            this.summoner1Combo.Location = new System.Drawing.Point(710, 112);
            this.summoner1Combo.Margin = new System.Windows.Forms.Padding(4);
            this.summoner1Combo.Name = "summoner1Combo";
            this.summoner1Combo.Size = new System.Drawing.Size(170, 33);
            this.summoner1Combo.TabIndex = 58;
            this.summoner1Combo.Text = "Smite";
            this.summoner1Combo.SelectedIndexChanged += new System.EventHandler(this.summoner1Combo_SelectedIndexChanged);
            // 
            // summoner2Combo
            // 
            this.summoner2Combo.FormattingEnabled = true;
            this.summoner2Combo.Location = new System.Drawing.Point(1044, 112);
            this.summoner2Combo.Margin = new System.Windows.Forms.Padding(4);
            this.summoner2Combo.Name = "summoner2Combo";
            this.summoner2Combo.Size = new System.Drawing.Size(166, 33);
            this.summoner2Combo.TabIndex = 59;
            this.summoner2Combo.Text = "Ignite";
            this.summoner2Combo.SelectedIndexChanged += new System.EventHandler(this.summoner2Combo_SelectedIndexChanged);
            // 
            // iconCombo
            // 
            this.iconCombo.FormattingEnabled = true;
            this.iconCombo.Location = new System.Drawing.Point(80, 112);
            this.iconCombo.Margin = new System.Windows.Forms.Padding(4);
            this.iconCombo.Name = "iconCombo";
            this.iconCombo.Size = new System.Drawing.Size(104, 33);
            this.iconCombo.TabIndex = 60;
            this.iconCombo.Text = "0";
            this.iconCombo.SelectedIndexChanged += new System.EventHandler(this.iconCombo_SelectedIndexChanged);
            // 
            // skinCombo
            // 
            this.skinCombo.FormattingEnabled = true;
            this.skinCombo.Location = new System.Drawing.Point(252, 112);
            this.skinCombo.Margin = new System.Windows.Forms.Padding(4);
            this.skinCombo.Name = "skinCombo";
            this.skinCombo.Size = new System.Drawing.Size(104, 33);
            this.skinCombo.TabIndex = 61;
            this.skinCombo.Text = "0";
            this.skinCombo.SelectedIndexChanged += new System.EventHandler(this.skinCombo_SelectedIndexChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(1104, 17);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(137, 25);
            this.label18.TabIndex = 62;
            this.label18.Text = "Content Path";
            // 
            // contentPathTxt
            // 
            this.contentPathTxt.Location = new System.Drawing.Point(1248, 12);
            this.contentPathTxt.Margin = new System.Windows.Forms.Padding(4);
            this.contentPathTxt.Name = "contentPathTxt";
            this.contentPathTxt.Size = new System.Drawing.Size(314, 31);
            this.contentPathTxt.TabIndex = 63;
            this.contentPathTxt.Text = "../../../..";
            this.contentPathTxt.TextChanged += new System.EventHandler(this.contentPathTxt_TextChanged);
            // 
            // GameServerAppForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2208, 1342);
            this.Controls.Add(this.contentPathTxt);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.skinCombo);
            this.Controls.Add(this.iconCombo);
            this.Controls.Add(this.summoner2Combo);
            this.Controls.Add(this.summoner1Combo);
            this.Controls.Add(this.ribbonCombo);
            this.Controls.Add(this.teamCombo);
            this.Controls.Add(this.rankCombo);
            this.Controls.Add(this.championCombo);
            this.Controls.Add(this.sendChatButton);
            this.Controls.Add(this.chatMessageTextBox);
            this.Controls.Add(this.onlineChatTextBox);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.closeLobbyCheckBox);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.lobbyListBox);
            this.Controls.Add(this.requestOtherConnectionsButton);
            this.Controls.Add(this.blockIPToggleButton);
            this.Controls.Add(this.removeIPButton);
            this.Controls.Add(this.addIPButton);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.connectedIPsListBox);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.allowOnlineConnectionsCheckBox);
            this.Controls.Add(this.serverHostCombo);
            this.Controls.Add(this.nameTxt);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.consoleErrorLogsCheckBox);
            this.Controls.Add(this.consoleWarningLogsCheckBox);
            this.Controls.Add(this.consoleDebugLogsCheckBox);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.consoleInfoLogsCheckBox);
            this.Controls.Add(this.consoleTextBox);
            this.Controls.Add(this.spawnMinionsCheckBox);
            this.Controls.Add(this.enableCheatsCheckBox);
            this.Controls.Add(this.cooldownsCheckBox);
            this.Controls.Add(this.manaCostsCheckBox);
            this.Controls.Add(this.mapCombo);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.gameModeCombo);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.gamePathTxt);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.startGameButton);
            this.Controls.Add(this.autoStartGameWithServerCheckBox);
            this.Controls.Add(this.startServerButton);
            this.Controls.Add(this.autoStartServerOnLaunchCheckBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6);
            this.Name = "GameServerAppForm";
            this.Text = "League Sandbox Game Server App";
            this.Load += new System.EventHandler(this.GameServerAppForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox autoStartServerOnLaunchCheckBox;
        private System.Windows.Forms.Button startServerButton;
        private System.Windows.Forms.CheckBox autoStartGameWithServerCheckBox;
        private System.Windows.Forms.Button startGameButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox gamePathTxt;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox gameModeCombo;
        private System.Windows.Forms.ComboBox mapCombo;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox manaCostsCheckBox;
        private System.Windows.Forms.CheckBox cooldownsCheckBox;
        private System.Windows.Forms.CheckBox enableCheatsCheckBox;
        private System.Windows.Forms.CheckBox spawnMinionsCheckBox;
        private System.Windows.Forms.TextBox consoleTextBox;
        private System.Windows.Forms.CheckBox consoleInfoLogsCheckBox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox consoleDebugLogsCheckBox;
        private System.Windows.Forms.CheckBox consoleWarningLogsCheckBox;
        private System.Windows.Forms.CheckBox consoleErrorLogsCheckBox;
        private System.Windows.Forms.TextBox nameTxt;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox serverHostCombo;
        private System.Windows.Forms.CheckBox allowOnlineConnectionsCheckBox;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ListBox connectedIPsListBox;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Button addIPButton;
        private System.Windows.Forms.Button removeIPButton;
        private System.Windows.Forms.Button blockIPToggleButton;
        private System.Windows.Forms.Button requestOtherConnectionsButton;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ListBox lobbyListBox;
        private System.Windows.Forms.CheckBox closeLobbyCheckBox;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox onlineChatTextBox;
        private System.Windows.Forms.TextBox chatMessageTextBox;
        private System.Windows.Forms.Button sendChatButton;
        private System.Windows.Forms.ComboBox championCombo;
        private System.Windows.Forms.ComboBox rankCombo;
        private System.Windows.Forms.ComboBox teamCombo;
        private System.Windows.Forms.ComboBox ribbonCombo;
        private System.Windows.Forms.ComboBox summoner1Combo;
        private System.Windows.Forms.ComboBox summoner2Combo;
        private System.Windows.Forms.ComboBox iconCombo;
        private System.Windows.Forms.ComboBox skinCombo;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox contentPathTxt;
    }
}


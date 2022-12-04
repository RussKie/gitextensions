using System.Drawing;
using System.Windows.Forms;

namespace GitUI
{
    partial class FormShell
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
            System.Windows.Forms.ToolStripMenuItem toolStripMenuItem4;
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initNewRepositoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiFavouriteRepositories = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRecentRepositories = new System.Windows.Forms.ToolStripMenuItem();
            this.clearRecentRepositoriesListToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.tsmiRecentRepositoriesClear = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.cloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitBashToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitGUIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kGitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator46 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator41 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.PuTTYToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startAuthenticationAgentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateOrImportKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.userManualToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changelogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.translateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.donateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiTelemetryEnabled = new System.Windows.Forms.ToolStripMenuItem();
            this.reportAnIssueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkForUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gitcommandLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.mainMenuStrip = new GitUI.MenuStripEx();
            this.pnlContent = new System.Windows.Forms.Panel();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.mainMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(119, 22);
            toolStripMenuItem2.Text = "...";
            // 
            // toolStripMenuItem4
            // 
            toolStripMenuItem4.Name = "toolStripMenuItem4";
            toolStripMenuItem4.Size = new System.Drawing.Size(83, 22);
            toolStripMenuItem4.Text = "...";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.initNewRepositoryToolStripMenuItem,
            this.openToolStripMenuItem,
            this.tsmiFavouriteRepositories,
            this.tsmiRecentRepositories,
            this.toolStripSeparator12,
            this.cloneToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(43, 19);
            this.fileToolStripMenuItem.Text = "&Start";
            // 
            // initNewRepositoryToolStripMenuItem
            // 
            this.initNewRepositoryToolStripMenuItem.Image = global::GitUI.Properties.Images.RepoCreate;
            this.initNewRepositoryToolStripMenuItem.Name = "initNewRepositoryToolStripMenuItem";
            this.initNewRepositoryToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.initNewRepositoryToolStripMenuItem.Text = "&Create new repository...";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::GitUI.Properties.Images.RepoOpen;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.openToolStripMenuItem.Text = "&Open...";
            // 
            // tsmiFavouriteRepositories
            // 
            this.tsmiFavouriteRepositories.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            toolStripMenuItem4});
            this.tsmiFavouriteRepositories.Image = global::GitUI.Properties.Images.Star;
            this.tsmiFavouriteRepositories.Name = "tsmiFavouriteRepositories";
            this.tsmiFavouriteRepositories.Size = new System.Drawing.Size(198, 22);
            this.tsmiFavouriteRepositories.Text = "&Favorite repositories";
            // 
            // tsmiRecentRepositories
            // 
            this.tsmiRecentRepositories.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            toolStripMenuItem2,
            this.clearRecentRepositoriesListToolStripMenuItem,
            this.tsmiRecentRepositoriesClear});
            this.tsmiRecentRepositories.Image = global::GitUI.Properties.Images.RecentRepositories;
            this.tsmiRecentRepositories.Name = "tsmiRecentRepositories";
            this.tsmiRecentRepositories.Size = new System.Drawing.Size(198, 22);
            this.tsmiRecentRepositories.Text = "&Recent repositories";
            // 
            // clearRecentRepositoriesListToolStripMenuItem
            // 
            this.clearRecentRepositoriesListToolStripMenuItem.Name = "clearRecentRepositoriesListToolStripMenuItem";
            this.clearRecentRepositoriesListToolStripMenuItem.Size = new System.Drawing.Size(116, 6);
            // 
            // tsmiRecentRepositoriesClear
            // 
            this.tsmiRecentRepositoriesClear.Name = "tsmiRecentRepositoriesClear";
            this.tsmiRecentRepositoriesClear.Size = new System.Drawing.Size(119, 22);
            this.tsmiRecentRepositoriesClear.Text = "Clear list";
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(195, 6);
            // 
            // cloneToolStripMenuItem
            // 
            this.cloneToolStripMenuItem.Image = global::GitUI.Properties.Images.CloneRepoGit;
            this.cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
            this.cloneToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.cloneToolStripMenuItem.Text = "C&lone repository...";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(195, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            // 
            // gitBashToolStripMenuItem
            // 
            this.gitBashToolStripMenuItem.Image = global::GitUI.Properties.Images.GitForWindows;
            this.gitBashToolStripMenuItem.Name = "gitBashToolStripMenuItem";
            this.gitBashToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.gitBashToolStripMenuItem.Text = "Git &bash";
            // 
            // gitGUIToolStripMenuItem
            // 
            this.gitGUIToolStripMenuItem.Image = global::GitUI.Properties.Images.GitGui;
            this.gitGUIToolStripMenuItem.Name = "gitGUIToolStripMenuItem";
            this.gitGUIToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.gitGUIToolStripMenuItem.Text = "Git &GUI";
            // 
            // kGitToolStripMenuItem
            // 
            this.kGitToolStripMenuItem.Image = global::GitUI.Properties.Images.Gitk;
            this.kGitToolStripMenuItem.Name = "kGitToolStripMenuItem";
            this.kGitToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.kGitToolStripMenuItem.Text = "Git&K";
            // 
            // toolStripSeparator46
            // 
            this.toolStripSeparator46.Name = "toolStripSeparator46";
            this.toolStripSeparator46.Size = new System.Drawing.Size(268, 6);
            // 
            // toolStripSeparator41
            // 
            this.toolStripSeparator41.Name = "toolStripSeparator41";
            this.toolStripSeparator41.Size = new System.Drawing.Size(189, 6);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(189, 6);
            // 
            // PuTTYToolStripMenuItem
            // 
            this.PuTTYToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startAuthenticationAgentToolStripMenuItem,
            this.generateOrImportKeyToolStripMenuItem});
            this.PuTTYToolStripMenuItem.Image = global::GitUI.Properties.Images.Putty;
            this.PuTTYToolStripMenuItem.Name = "PuTTYToolStripMenuItem";
            this.PuTTYToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.PuTTYToolStripMenuItem.Text = "&PuTTY";
            // 
            // startAuthenticationAgentToolStripMenuItem
            // 
            this.startAuthenticationAgentToolStripMenuItem.Image = global::GitUI.Properties.Images.Pageant16;
            this.startAuthenticationAgentToolStripMenuItem.Name = "startAuthenticationAgentToolStripMenuItem";
            this.startAuthenticationAgentToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.startAuthenticationAgentToolStripMenuItem.Text = "Start authentication agent";
            // 
            // generateOrImportKeyToolStripMenuItem
            // 
            this.generateOrImportKeyToolStripMenuItem.Image = global::GitUI.Properties.Images.PuttyGen;
            this.generateOrImportKeyToolStripMenuItem.Name = "generateOrImportKeyToolStripMenuItem";
            this.generateOrImportKeyToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.generateOrImportKeyToolStripMenuItem.Text = "Generate or import key";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Image = global::GitUI.Properties.Images.Settings;
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.settingsToolStripMenuItem.Text = "&Settings";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.userManualToolStripMenuItem,
            this.changelogToolStripMenuItem,
            this.toolStripSeparator3,
            this.translateToolStripMenuItem,
            this.toolStripSeparator16,
            this.donateToolStripMenuItem,
            this.tsmiTelemetryEnabled,
            this.reportAnIssueToolStripMenuItem,
            this.checkForUpdatesToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 19);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // userManualToolStripMenuItem
            // 
            this.userManualToolStripMenuItem.Image = global::GitUI.Properties.Images.GotoManual;
            this.userManualToolStripMenuItem.Name = "userManualToolStripMenuItem";
            this.userManualToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.userManualToolStripMenuItem.Text = "User &manual";
            // 
            // changelogToolStripMenuItem
            // 
            this.changelogToolStripMenuItem.Image = global::GitUI.Properties.Images.Changelog;
            this.changelogToolStripMenuItem.Name = "changelogToolStripMenuItem";
            this.changelogToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.changelogToolStripMenuItem.Text = "&Changelog";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(181, 6);
            // 
            // translateToolStripMenuItem
            // 
            this.translateToolStripMenuItem.Image = global::GitUI.Properties.Images.Translate;
            this.translateToolStripMenuItem.Name = "translateToolStripMenuItem";
            this.translateToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.translateToolStripMenuItem.Text = "&Translate";
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(181, 6);
            // 
            // donateToolStripMenuItem
            // 
            this.donateToolStripMenuItem.Image = global::GitUI.Properties.Images.DollarSign;
            this.donateToolStripMenuItem.Name = "donateToolStripMenuItem";
            this.donateToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.donateToolStripMenuItem.Text = "&Donate";
            // 
            // tsmiTelemetryEnabled
            // 
            this.tsmiTelemetryEnabled.Checked = true;
            this.tsmiTelemetryEnabled.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsmiTelemetryEnabled.Name = "tsmiTelemetryEnabled";
            this.tsmiTelemetryEnabled.Size = new System.Drawing.Size(184, 22);
            this.tsmiTelemetryEnabled.Text = "&Yes, I allow telemetry";
            // 
            // reportAnIssueToolStripMenuItem
            // 
            this.reportAnIssueToolStripMenuItem.Image = global::GitUI.Properties.Images.BugReport;
            this.reportAnIssueToolStripMenuItem.Name = "reportAnIssueToolStripMenuItem";
            this.reportAnIssueToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.reportAnIssueToolStripMenuItem.Text = "&Report an issue";
            // 
            // checkForUpdatesToolStripMenuItem
            // 
            this.checkForUpdatesToolStripMenuItem.Image = global::GitUI.Properties.Images.CheckForUpdates;
            this.checkForUpdatesToolStripMenuItem.Name = "checkForUpdatesToolStripMenuItem";
            this.checkForUpdatesToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.checkForUpdatesToolStripMenuItem.Text = "Check for &updates";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::GitUI.Properties.Images.Information;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.gitBashToolStripMenuItem,
            this.gitGUIToolStripMenuItem,
            this.kGitToolStripMenuItem,
            this.toolStripSeparator6,
            this.PuTTYToolStripMenuItem,
            this.toolStripSeparator41,
            this.gitcommandLogToolStripMenuItem,
            this.toolStripSeparator7,
            this.settingsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 19);
            this.toolsToolStripMenuItem.Text = "&Tools";
            // 
            // gitcommandLogToolStripMenuItem
            // 
            this.gitcommandLogToolStripMenuItem.Image = global::GitUI.Properties.Images.GitCommandLog;
            this.gitcommandLogToolStripMenuItem.Name = "gitcommandLogToolStripMenuItem";
            this.gitcommandLogToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F12;
            this.gitcommandLogToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
            this.gitcommandLogToolStripMenuItem.Text = "Git &command log";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(189, 6);
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.ClickThrough = true;
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(4);
            this.mainMenuStrip.Size = new System.Drawing.Size(760, 27);
            this.mainMenuStrip.TabIndex = 0;
            // 
            // pnlContent
            // 
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.Location = new System.Drawing.Point(0, 27);
            this.pnlContent.Margin = new System.Windows.Forms.Padding(0);
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Padding = new System.Windows.Forms.Padding(6);
            this.pnlContent.Size = new System.Drawing.Size(760, 494);
            this.pnlContent.TabIndex = 1;
            // 
            // FormShell
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ClientSize = new System.Drawing.Size(760, 521);
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.mainMenuStrip);
            this.Name = "FormShell";
            this.Text = "Git Extensions";
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Panel pnlContent;

        private MenuStripEx mainMenuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem tsmiRecentRepositories;
        private ToolStripSeparator toolStripSeparator12;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem gitBashToolStripMenuItem;
        private ToolStripMenuItem gitGUIToolStripMenuItem;
        private ToolStripMenuItem kGitToolStripMenuItem;
        private ToolStripMenuItem cloneToolStripMenuItem;
        private ToolStripMenuItem initNewRepositoryToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private ToolStripMenuItem PuTTYToolStripMenuItem;
        private ToolStripMenuItem startAuthenticationAgentToolStripMenuItem;
        private ToolStripMenuItem generateOrImportKeyToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem userManualToolStripMenuItem;
        private ToolStripMenuItem changelogToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem translateToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator16;
        private ToolStripMenuItem donateToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem reportAnIssueToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator41;
        private ToolStripSeparator toolStripSeparator46;
        private ToolStripMenuItem checkForUpdatesToolStripMenuItem;
        private ToolStripMenuItem gitcommandLogToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem tsmiRecentRepositoriesClear;
        private ToolStripSeparator clearRecentRepositoriesListToolStripMenuItem;
        private ToolStripMenuItem tsmiFavouriteRepositories;
        private ToolStripMenuItem tsmiTelemetryEnabled;
    }
}

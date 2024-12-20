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
            this.fileToolStripMenuItem = new GitUI.CommandsDialogs.Menus.StartToolStripMenuItem();
            this.helpToolStripMenuItem = new GitUI.CommandsDialogs.Menus.HelpToolStripMenuItem();
            this.toolsToolStripMenuItem = new GitUI.CommandsDialogs.Menus.ToolsToolStripMenuItem();
            this.mainMenuStrip = new GitUI.MenuStripEx();
            this.pnlContent = new System.Windows.Forms.Panel();
            this.mainMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(43, 19);
            this.fileToolStripMenuItem.GitModuleChanged += new EventHandler<GitExtensions.Extensibility.Git.GitModuleEventArgs>(this.SetGitModule);
            this.fileToolStripMenuItem.RecentRepositoriesCleared += new System.EventHandler(this.fileToolStripMenuItem_RecentRepositoriesCleared);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 19);
            this.toolsToolStripMenuItem.Text = "&Tools";
            this.toolsToolStripMenuItem.SettingsChanged += new System.EventHandler<GitUI.CommandsDialogs.Menus.SettingsChangedEventArgs>(this.toolsToolStripMenuItem_SettingsChanged);
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
        private GitUI.CommandsDialogs.Menus.StartToolStripMenuItem fileToolStripMenuItem;
        private GitUI.CommandsDialogs.Menus.HelpToolStripMenuItem helpToolStripMenuItem;
        private GitUI.CommandsDialogs.Menus.ToolsToolStripMenuItem toolsToolStripMenuItem;
    }
}

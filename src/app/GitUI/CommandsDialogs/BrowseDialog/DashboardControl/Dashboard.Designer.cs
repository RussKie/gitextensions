namespace GitUI.CommandsDialogs.BrowseDialog.DashboardControl
{
    partial class Dashboard
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
            if (disposing && (components is not null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ToolStripSeparator toolStripSeparator1;
            ToolStripMenuItem mnuRefresh;
            ToolStripMenuItem mnuConfigure;
            pnlLeft = new Panel();
            flpnlStart = new FlowLayoutPanel();
            flpnlContribute = new FlowLayoutPanel();
            lblContribute = new Label();
            pnlLogo = new Panel();
            pbLogo = new PictureBox();
            userRepositoriesList = new UserRepositoriesList();
            tableLayoutPanel1 = new TableLayoutPanel();
            mnuDashboard = new ToolStripMenuItem();
            menuStrip1 = new MenuStrip();
            toolStripSeparator1 = new ToolStripSeparator();
            mnuRefresh = new ToolStripMenuItem();
            mnuConfigure = new ToolStripMenuItem();
            pnlLeft.SuspendLayout();
            flpnlContribute.SuspendLayout();
            pnlLogo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbLogo).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(215, 6);
            // 
            // mnuRefresh
            // 
            mnuRefresh.Image = Properties.Images.ReloadRevisions;
            mnuRefresh.Name = "mnuRefresh";
            mnuRefresh.ShortcutKeys = Keys.F5;
            mnuRefresh.Size = new Size(218, 22);
            mnuRefresh.Text = "&Refresh";
            // 
            // mnuConfigure
            // 
            mnuConfigure.Image = Properties.Images.Settings;
            mnuConfigure.Name = "mnuConfigure";
            mnuConfigure.Size = new Size(218, 22);
            mnuConfigure.Text = "Recent repositories &settings";
            mnuConfigure.Click += mnuConfigure_Click;
            // 
            // pnlLeft
            // 
            pnlLeft.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlLeft.BackColor = Color.FromArgb(223, 228, 235);
            pnlLeft.Controls.Add(flpnlStart);
            pnlLeft.Controls.Add(flpnlContribute);
            pnlLeft.Controls.Add(pnlLogo);
            pnlLeft.Location = new Point(33, 0);
            pnlLeft.Margin = new Padding(0);
            pnlLeft.Name = "pnlLeft";
            pnlLeft.Size = new Size(213, 360);
            pnlLeft.TabIndex = 0;
            // 
            // flpnlStart
            // 
            flpnlStart.BackColor = Color.FromArgb(46, 50, 58);
            flpnlStart.Dock = DockStyle.Fill;
            flpnlStart.FlowDirection = FlowDirection.TopDown;
            flpnlStart.Location = new Point(0, 68);
            flpnlStart.Margin = new Padding(2);
            flpnlStart.Name = "flpnlStart";
            flpnlStart.Padding = new Padding(20);
            flpnlStart.Size = new Size(213, 137);
            flpnlStart.TabIndex = 1;
            flpnlStart.WrapContents = false;
            // 
            // flpnlContribute
            // 
            flpnlContribute.BackColor = Color.Transparent;
            flpnlContribute.Controls.Add(lblContribute);
            flpnlContribute.Dock = DockStyle.Bottom;
            flpnlContribute.FlowDirection = FlowDirection.TopDown;
            flpnlContribute.Location = new Point(0, 205);
            flpnlContribute.Margin = new Padding(2);
            flpnlContribute.Name = "flpnlContribute";
            flpnlContribute.Padding = new Padding(20, 20, 20, 30);
            flpnlContribute.Size = new Size(213, 155);
            flpnlContribute.TabIndex = 2;
            flpnlContribute.WrapContents = false;
            // 
            // lblContribute
            // 
            lblContribute.AutoSize = true;
            lblContribute.Font = new Font("Segoe UI", 14.25F);
            lblContribute.ForeColor = SystemColors.GrayText;
            lblContribute.Location = new Point(22, 20);
            lblContribute.Margin = new Padding(2, 0, 2, 8);
            lblContribute.Name = "lblContribute";
            lblContribute.Size = new Size(102, 25);
            lblContribute.TabIndex = 0;
            lblContribute.Text = "Contribute";
            // 
            // pnlLogo
            // 
            pnlLogo.BackColor = Color.FromArgb(30, 40, 57);
            pnlLogo.Controls.Add(pbLogo);
            pnlLogo.Dock = DockStyle.Top;
            pnlLogo.Location = new Point(0, 0);
            pnlLogo.Margin = new Padding(8);
            pnlLogo.Name = "pnlLogo";
            pnlLogo.Padding = new Padding(20, 0, 20, 14);
            pnlLogo.Size = new Size(213, 68);
            pnlLogo.TabIndex = 0;
            // 
            // pbLogo
            // 
            pbLogo.Image = Properties.Images.GitExtensionsLogoWide;
            pbLogo.Location = new Point(14, 15);
            pbLogo.Name = "pbLogo";
            pbLogo.Size = new Size(185, 44);
            pbLogo.SizeMode = PictureBoxSizeMode.Zoom;
            pbLogo.TabIndex = 0;
            pbLogo.TabStop = false;
            // 
            // userRepositoriesList
            // 
            userRepositoriesList.AllowDrop = true;
            userRepositoriesList.BranchNameColor = SystemColors.HotTrack;
            userRepositoriesList.Dock = DockStyle.Fill;
            userRepositoriesList.HeaderBackColor = Color.FromArgb(160, 183, 226);
            userRepositoriesList.HeaderColor = Color.Empty;
            userRepositoriesList.HeaderHeight = 70;
            userRepositoriesList.HoverColor = Color.Empty;
            userRepositoriesList.Location = new Point(246, 0);
            userRepositoriesList.MainBackColor = Color.Empty;
            userRepositoriesList.Margin = new Padding(0);
            userRepositoriesList.Name = "userRepositoriesList";
            userRepositoriesList.Size = new Size(407, 360);
            userRepositoriesList.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = Color.Transparent;
            tableLayoutPanel1.ColumnCount = 4;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142857F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 213F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 85.71428F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 7.142857F));
            tableLayoutPanel1.Controls.Add(pnlLeft, 1, 0);
            tableLayoutPanel1.Controls.Add(userRepositoriesList, 2, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 24);
            tableLayoutPanel1.Margin = new Padding(0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(688, 336);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // mnuDashboard
            // 
            mnuDashboard.DropDownItems.AddRange(new ToolStripItem[] { mnuRefresh, toolStripSeparator1, mnuConfigure });
            mnuDashboard.MergeAction = MergeAction.Insert;
            mnuDashboard.MergeIndex = 1;
            mnuDashboard.Name = "mnuDashboard";
            mnuDashboard.Size = new Size(76, 20);
            mnuDashboard.Text = "&Dashboard";
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { mnuDashboard });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(688, 24);
            menuStrip1.TabIndex = 3;
            menuStrip1.Text = "menuStrip1";
            menuStrip1.Visible = false;
            // 
            // Dashboard
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;
            BackColor = Color.FromArgb(238, 243, 253);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(menuStrip1);
            DoubleBuffered = true;
            Margin = new Padding(2, 1, 2, 1);
            Name = "Dashboard";
            Size = new Size(688, 360);
            ParentChanged += dashboard_ParentChanged;
            pnlLeft.ResumeLayout(false);
            flpnlContribute.ResumeLayout(false);
            flpnlContribute.PerformLayout();
            pnlLogo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbLogo).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private FlowLayoutPanel flpnlStart;
        private Panel pnlLeft;
        private FlowLayoutPanel flpnlContribute;
        private Label lblContribute;
        private Panel pnlLogo;
        private UserRepositoriesList userRepositoriesList;
        private TableLayoutPanel tableLayoutPanel1;
        private PictureBox pbLogo;
        private ToolStripMenuItem mnuDashboard;
        private MenuStrip menuStrip1;
    }
}

#pragma warning disable SA1005
#pragma warning disable SA1507
#pragma warning disable SA1515

using System.Diagnostics;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git;
using GitCommands.UserRepositoryHistory;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.Hotkey;
using GitUI.Infrastructure.Telemetry;
using GitUI.Shells;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.Win32;

namespace GitUI
{
    public sealed partial class FormShell : GitModuleForm
    {
        public static readonly string HotkeySettingsName = "Browse";

        #region Mnemonics
        /*
            MENUS
            ═════
                    ABEFGIJKLMOQUWXYZ
                C   Commands
                D   Dashboard
                H   Help
                N   Navigate (inserted by FormBrowseMenus)
                P   Plugins
                R   Repository
                S   Start
                T   Tools
                V   View (inserted by FormBrowseMenus)
                    GitHub (inserted dynamically)

            START menu
            ══════════
                    ABDEGHIJKMNPQSTUVWYZ
                C   Create new repository...
                F   Favorite repositories
                L   Clone repository...
                O   Open...
                R   Recent repositories
                X   Exit

            DASHBOARD menu
            ══════════════
                R   Refresh
                S   Recent repositories settings

            REPOSITORY menu
            ═══════════════
                    DFHJLNPQVYZ
                A   Edit .gitattributes
                B   Synchronize all submodules
                C   Close (go to Dashboard)
                E   Edit .git/info/exclude
                G   Git maintenance
                I   Edit .gitignore
                K   Sparse Working Copy
                M   Edit .mailmap
                O   Repository settings
                R   Refresh
                S   Manage submodules...
                T   Remote repositories...
                U   Update all submodules
                W   Worktrees
                X   File Explorer

            REPOSITORY ▷ WORKTREES submenu
            ══════════════════════════════
                C   Create a worktree...
                M   Manage worktrees...

            REPOSITORY ▷ GIT MAINTENANCE submenu
            ════════════════════════════════════
                C   Compress git database
                R   Recover lost objects...
                D   Delete index.lock
                E   Edit .git/config

            COMMANDS menu
            ═════════════
                    JQXZ
                /   Pull/Fetch...
                A   Apply patch...
                B   Create branch...
                C   Commit...
                D   Delete tag...
                E   Rebase...
                F   Format patch...
                G   Show reflog...
                H   View patch file...
                I   Bisect...
                K   Checkout branch...
                L   Delete branch...
                M   Merge branches...
                N   Manage stashes...
                O   Checkout revision...
                P   Push...
                R   Reset changes...
                S   Solve merge conflicts...
                T   Create tag...
                U   Undo last commit
                V   Archive revision...
                W   Clean working directory...
                Y   Cherry pick...

            GITHUB (Repository hosts) menu
            ══════════════════════════════
                    BDEGHIJKLMNOQRSTUVWXYZ
                A   Add upstream remote
                C   Create pull requests...
                F   Fork/Clone repository
                P   View pull requests...

            PLUGINS menu
            ════════════
                S   Plugin Settings

            TOOLS menu
            ══════════
                    ADEFHIJLMNOQRTUVWXYZ
                B   Git bash
                C   Git command log
                G   Git GUI
                K   GitK
                P   PuTTY
                S   Settings

            HELP menu
            ═════════
                    BEFGHIJKLNOPQSVWXZ
                A   About
                C   Changelog
                D   Donate
                M   User manual
                R   Report an issue
                T   Translate
                U   Check for updates
                Y   Yes, I allow telemetry
        */
        #endregion

        //private readonly GitStatusMonitor _gitStatusMonitor;
        //private readonly IFormBrowseController _controller;
        //private readonly ICommitDataManager _commitDataManager;
        //private readonly IAppTitleGenerator _appTitleGenerator;
        //private readonly IAheadBehindDataProvider? _aheadBehindDataProvider;
        //private readonly IWindowsJumpListManager _windowsJumpListManager;
        //private readonly ISubmoduleStatusProvider _submoduleStatusProvider;
        //private List<ToolStripItem>? _currentSubmoduleMenuItems;
        private readonly FormBrowseDiagnosticsReporter _formBrowseDiagnosticsReporter;
        private readonly BrowseArguments? _browseArguments;

        //private BuildReportTabPageExtension? _buildReportTabPageExtension;
        //private readonly ShellProvider _shellProvider = new();
        //private ConEmuControl? _terminal;
        private Dashboard? _dashboard;
        private RepoBrowser? _repoBrowser;
        //private bool _isFileBlameHistory;
        //private bool _fileBlameHistorySidePanelStartupState;

        //private TabPage? _consoleTabPage;

        //private UpdateTargets _selectedRevisionUpdatedTargets = UpdateTargets.None;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormShell()
        {
            InitializeComponent();
            InitializeComplete();
        }

        /// <summary>
        /// Opens the application shell.
        /// </summary>
        /// <param name="commands">The commands in the current form.</param>
        /// <param name="args">The start up arguments.</param>
        public FormShell(GitUICommands commands, BrowseArguments args)
            : base(commands)
        {
            // TODO:
            _browseArguments = args;

            SystemEvents.SessionEnding += (s, e) => SaveApplicationSettings();

            _formBrowseDiagnosticsReporter = new FormBrowseDiagnosticsReporter(this);

            InitializeComponent();
            BackColor = OtherColors.BackgroundColor;

            mainMenuStrip.ForeColor = SystemColors.WindowText;
            mainMenuStrip.BackColor = Color.Transparent;

            HotkeysEnabled = true;
            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);

            SetShortcutKeyDisplayStringsFromHotkeySettings();
            RegisterMenuCommands();

            UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;

            InitializeComplete();
        }

        private void ControlAdd(Control control)
        {
            if (control is IMainMenuExtender mainMenuExtender)
            {
                if (!ToolStripManager.Merge(mainMenuExtender.ControlMenu, mainMenuStrip))
                {
                    Debug.Fail("Failed to merge the menu");
                }

                // HACK: the menu often fails to refresh itself until some kind of user action.
                mainMenuStrip.Refresh();
            }

            pnlContent.Controls.Add(control);
        }

        private void ControlRemove(Control control)
        {
            if (control is IMainMenuExtender mainMenuExtender)
            {
                if (!ToolStripManager.RevertMerge(mainMenuStrip, mainMenuExtender.ControlMenu))
                {
                    Debug.Fail("Failed to unmerge the menu");
                }
            }

            pnlContent.Controls.Remove(control);
        }

        private void DashboardClose()
        {
            if (_dashboard is null)
            {
                return;
            }

            ControlRemove(_dashboard);

            _dashboard.GitModuleChanged -= SetGitModule;
            _dashboard.Dispose();
            _dashboard = null;
        }

        private void DashboardOpen()
        {
            if (_dashboard is null)
            {
                _dashboard = new()
                {
                    Dock = DockStyle.Fill,
                    Visible = true
                };
                _dashboard.RefreshContent();
                _dashboard.GitModuleChanged += SetGitModule;
            }

            ControlAdd(_dashboard);
            DiagnosticsClient.TrackPageView("Dashboard");

            // Explicit call: Title is normally updated on RevisionGrid filter change
            ////Text = _appTitleGenerator.Generate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                //_gitStatusMonitor?.Dispose();
                //_windowsJumpListManager?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeView(GitModule gitModule)
        {
            // We need to process all the pending messages, and, thus, allow the currently visible controls to finish processing their
            // respective message queues before we dispose these controls.
            BeginInvoke(() =>
            {
                if (_dashboard is not null || (_repoBrowser is null && gitModule is not null && gitModule.IsValidGitWorkingDir()))
                {
                    DashboardClose();
                    RepositoryOpen(gitModule);
                }
                else if (gitModule is not null && gitModule.IsValidGitWorkingDir())
                {
                    RepositorySwitch(gitModule);
                }
                else
                {
                    RepositoryClose();
                    DashboardOpen();
                }
            });
        }

        protected override void OnRuntimeLoad(EventArgs e)
        {
            base.OnRuntimeLoad(e);

#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
            BeginInvoke(async () =>
            {
                // HACK: for some strange reason the main menu disappears, and requires a mouse hover
                // for it to appear agaon. With this everything works just find. No time to debug Windows Forms code.
                await Task.Delay(10);

                InitializeView(Module);
            });
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates

            using (WaitCursorScope.Enter())
            {
                // check for updates
                if (AppSettings.CheckForUpdates && AppSettings.LastUpdateCheck.AddDays(7) < DateTime.Now)
                {
                    AppSettings.LastUpdateCheck = DateTime.Now;
                    FormUpdates updateForm = new(AppSettings.AppVersion);
                    updateForm.SearchForUpdatesAndShow(ownerWindow: this, alwaysShow: false);
                }
            }

            _formBrowseDiagnosticsReporter.Report();
        }

        private void RegisterMenuCommands()
        {
            // File
            MenuCommandService.Instance.Register(initNewRepositoryToolStripMenuItem, MenuCommands.InitRepo, InitNewRepositoryToolStripMenuItemClick);
            MenuCommandService.Instance.Register(openToolStripMenuItem, MenuCommands.OpenRepo, OpenToolStripMenuItemClick);
            MenuCommandService.Instance.Register(tsmiFavouriteRepositories, MenuCommands.FavouriteRepos);
            MenuCommandService.Instance.Register(tsmiRecentRepositories, MenuCommands.RecentRepos);
            MenuCommandService.Instance.Register(tsmiRecentRepositoriesClear, MenuCommands.ClearRecentRepos, tsmiRecentRepositoriesClear_Click);
            MenuCommandService.Instance.Register(cloneToolStripMenuItem, MenuCommands.CloneRepo, CloneToolStripMenuItemClick);
            MenuCommandService.Instance.Register(exitToolStripMenuItem, MenuCommands.ExitApp, ExitToolStripMenuItemClick);

            // Tools
            MenuCommandService.Instance.Register(gitBashToolStripMenuItem, MenuCommands.GitBash, gitBashToolStripMenuItem_Click);
            MenuCommandService.Instance.Register(gitGUIToolStripMenuItem, MenuCommands.GitGui, GitGuiToolStripMenuItemClick);
            MenuCommandService.Instance.Register(kGitToolStripMenuItem, MenuCommands.GitGitK, KGitToolStripMenuItemClick);
            MenuCommandService.Instance.Register(startAuthenticationAgentToolStripMenuItem, MenuCommands.StartAuthenticationAgent, StartAuthenticationAgentToolStripMenuItemClick);
            MenuCommandService.Instance.Register(generateOrImportKeyToolStripMenuItem, MenuCommands.GenerateOrImportKey, GenerateOrImportKeyToolStripMenuItemClick);
            MenuCommandService.Instance.Register(gitcommandLogToolStripMenuItem, MenuCommands.GitCommandLog, GitcommandLogToolStripMenuItemClick);
            MenuCommandService.Instance.Register(settingsToolStripMenuItem, MenuCommands.OpenSettings, RepoSettingsToolStripMenuItemClick);

            // Help
            MenuCommandService.Instance.Register(userManualToolStripMenuItem, MenuCommands.UserManual, UserManualToolStripMenuItemClick);
            MenuCommandService.Instance.Register(changelogToolStripMenuItem, MenuCommands.ChangeLog, ChangelogToolStripMenuItemClick);
            MenuCommandService.Instance.Register(translateToolStripMenuItem, MenuCommands.Translate, TranslateToolStripMenuItemClick);
            MenuCommandService.Instance.Register(donateToolStripMenuItem, MenuCommands.Donate, DonateToolStripMenuItemClick);
            MenuCommandService.Instance.Register(tsmiTelemetryEnabled, MenuCommands.Telemetry, TsmiTelemetryEnabled_Click);
            MenuCommandService.Instance.Register(reportAnIssueToolStripMenuItem, MenuCommands.ReportIssue, reportAnIssueToolStripMenuItem_Click);
            MenuCommandService.Instance.Register(checkForUpdatesToolStripMenuItem, MenuCommands.CheckUpdate, checkForUpdatesToolStripMenuItem_Click);
            MenuCommandService.Instance.Register(aboutToolStripMenuItem, MenuCommands.About, AboutToolStripMenuItemClick);
        }

        private void RepositoryClose()
        {
            //            HideVariableMainMenuItems();
            //            PluginRegistry.Unregister(UICommands);


            if (_repoBrowser is null)
            {
                return;
            }

            ControlRemove(_repoBrowser);

            _repoBrowser.Dispose();
            _repoBrowser = null;
        }

        private void RepositoryOpen(GitModule gitModule)
        {
            //            RegisterPlugins();

            if (_repoBrowser is null)
            {
                _repoBrowser = new(_browseArguments)
                {
                    Dock = DockStyle.Fill,
                    Visible = true
                };
            }

            ControlAdd(_repoBrowser);

            DiagnosticsClient.TrackPageView("Revision graph");

            UICommands = new(gitModule);
            _repoBrowser.SetWorkingDir(gitModule.WorkingDir);
        }

        private void RepositorySwitch(GitModule gitModule)
        {
            Debug.Assert(_repoBrowser is not null, "Boo!");

            // TODO: this likely must be executed at the beginning of InitializeView.

            string originalWorkingDir = Module.WorkingDir;
            if (string.Equals(originalWorkingDir, Module.WorkingDir, StringComparison.Ordinal))
            {
                return;
            }

            UICommands = new(gitModule);
            _repoBrowser.SetWorkingDir(gitModule.WorkingDir);
        }

        //protected override void OnFormClosing(FormClosingEventArgs e)
        //{
        //    SaveApplicationSettings();

        //    foreach (var control in this.FindDescendants())
        //    {
        //        control.DragEnter -= FormBrowse_DragEnter;
        //        control.DragDrop -= FormBrowse_DragDrop;
        //    }

        //    base.OnFormClosing(e);
        //}

        //        protected override void OnClosed(EventArgs e)
        //        {
        //            PluginRegistry.Unregister(UICommands);
        //            base.OnClosed(e);
        //        }

        protected override void OnUICommandsChanged(GitUICommandsChangedEventArgs e)
        {
            var oldCommands = e.OldCommands;

            if (oldCommands is not null)
            {
                oldCommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
            }

            UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;

            base.OnUICommandsChanged(e);
        }

        private void UICommands_PostRepositoryChanged(object sender, GitUIEventArgs e)
        {
            // Note that this called in most FormBrowse context to "be sure"
            // that the repo has not been updated externally.
            //RefreshRevisions(e.GetRefs);
        }

        //        private void ShowDashboard()
        //        {
        //            toolPanel.SuspendLayout();
        //            toolPanel.TopToolStripPanelVisible = false;
        //            toolPanel.BottomToolStripPanelVisible = false;
        //            toolPanel.LeftToolStripPanelVisible = false;
        //            toolPanel.RightToolStripPanelVisible = false;
        //            toolPanel.ResumeLayout();

        //            MainSplitContainer.Visible = false;

        //            if (_dashboard is null)
        //            {
        //                _dashboard = new Dashboard { Dock = DockStyle.Fill };
        //                _dashboard.GitModuleChanged += SetGitModule;
        //                toolPanel.ContentPanel.Controls.Add(_dashboard);
        //            }

        //            Text = _appTitleGenerator.Generate(branchName: TranslatedStrings.NoBranch);

        //            _dashboard.RefreshContent();
        //            _dashboard.Visible = true;
        //            _dashboard.BringToFront();

        //            DiagnosticsClient.TrackPageView("Dashboard");
        //        }

        //        private void HideDashboard()
        //        {
        //            MainSplitContainer.Visible = true;
        //            if (!_dashboard?.Visible ?? true)
        //            {
        //                return;
        //            }

        //            _dashboard.Visible = false;
        //            toolPanel.SuspendLayout();
        //            toolPanel.TopToolStripPanelVisible = true;
        //            toolPanel.BottomToolStripPanelVisible = true;
        //            toolPanel.LeftToolStripPanelVisible = true;
        //            toolPanel.RightToolStripPanelVisible = true;
        //            toolPanel.ResumeLayout();

        //            DiagnosticsClient.TrackPageView("Revision graph");
        //        }

        private void RegisterPlugins()
        {
            //const string PluginManagerName = "Plugin Manager";
            //var existingPluginMenus = pluginsToolStripMenuItem.DropDownItems.OfType<ToolStripMenuItem>().ToLookup(c => c.Tag);

            //lock (PluginRegistry.Plugins)
            //{
            //    var pluginEntries = PluginRegistry.Plugins
            //        .OrderByDescending(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase);

            //    // pluginsToolStripMenuItem.DropDownItems menu already contains at least 2 items:
            //    //    [1] Separator
            //    //    [0] Plugin Settings
            //    // insert all plugins except 'Plugin Manager' above the separator
            //    foreach (var plugin in pluginEntries)
            //    {
            //        // don't add the plugin to the Plugins menu, if already added
            //        if (existingPluginMenus.Contains(plugin))
            //        {
            //            continue;
            //        }

            //        ToolStripMenuItem item = new()
            //        {
            //            Text = plugin.Name,
            //            Image = plugin.Icon,
            //            Tag = plugin
            //        };
            //        item.Click += delegate
            //        {
            //            if (plugin.Execute(new GitUIEventArgs(this, UICommands)))
            //            {
            //                //_gitStatusMonitor.InvalidateGitWorkingDirectoryStatus();
            //                //RefreshRevisions();
            //            }
            //        };

            //        if (plugin.Name == PluginManagerName)
            //        {
            //            // insert Plugin Manager below the separator
            //            pluginsToolStripMenuItem.DropDownItems.Insert(pluginsToolStripMenuItem.DropDownItems.Count - 1, item);
            //        }
            //        else
            //        {
            //            pluginsToolStripMenuItem.DropDownItems.Insert(0, item);
            //        }
            //    }

            //    //if (_dashboard?.Visible ?? false)
            //    //{
            //    //    // now that plugins are registered, populate Git-host-plugin actions on Dashboard, like "Clone GitHub repository"
            //    //    _dashboard.RefreshContent();
            //    //}

            //    mainMenuStrip?.Refresh();
            //}

            //// Allow the plugin to perform any self-registration actions
            //PluginRegistry.Register(UICommands);

            //UICommands.RaisePostRegisterPlugin(this);

            //// Show "Repository hosts" menu item when there is at least 1 repository host plugin loaded
            //_repositoryHostsToolStripMenuItem.Visible = PluginRegistry.GitHosters.Count > 0;
            //if (PluginRegistry.GitHosters.Count == 1)
            //{
            //    _repositoryHostsToolStripMenuItem.Text = PluginRegistry.GitHosters[0].Name;
            //}

            //UpdatePluginMenu(Module.IsValidGitWorkingDir());
        }

        //        private void InternalInitialize()
        //        {
        //            toolPanel.SuspendLayout();
        //            toolPanel.TopToolStripPanel.SuspendLayout();

        //            using (WaitCursorScope.Enter())
        //            {
        //                // check for updates
        //                if (AppSettings.CheckForUpdates && AppSettings.LastUpdateCheck.AddDays(7) < DateTime.Now)
        //                {
        //                    AppSettings.LastUpdateCheck = DateTime.Now;
        //                    FormUpdates updateForm = new(AppSettings.AppVersion);
        //                    updateForm.SearchForUpdatesAndShow(ownerWindow: this, alwaysShow: false);
        //                }

        //                bool hasWorkingDir = !string.IsNullOrEmpty(Module.WorkingDir);
        //                if (hasWorkingDir)
        //                {
        //                    HideDashboard();
        //                }
        //                else
        //                {
        //                    ShowDashboard();
        //                }

        //                bool bareRepository = Module.IsBareRepository();
        //                bool isDashboard = _dashboard?.Visible ?? false;
        //                bool validBrowseDir = !isDashboard && Module.IsValidGitWorkingDir();

        //                branchSelect.Text = validBrowseDir
        //                    ? !string.IsNullOrWhiteSpace(RevisionGrid.CurrentBranch.Value)
        //                        ? RevisionGrid.CurrentBranch.Value
        //                        : DetachedHeadParser.DetachedBranch
        //                    : "";
        //                toolStripButtonLevelUp.Enabled = hasWorkingDir && !bareRepository;
        //                CommitInfoTabControl.Visible = validBrowseDir;
        //                fileExplorerToolStripMenuItem.Enabled = validBrowseDir;
        //                manageRemoteRepositoriesToolStripMenuItem1.Enabled = validBrowseDir;
        //                branchSelect.Enabled = validBrowseDir;
        //                toolStripButtonCommit.Enabled = validBrowseDir && !bareRepository;

        //                toolStripButtonPull.Enabled = validBrowseDir;
        //                toolStripButtonPush.Enabled = validBrowseDir;
        //                dashboardToolStripMenuItem.Visible = isDashboard;
        //                pluginsToolStripMenuItem.Visible = validBrowseDir;
        //                repositoryToolStripMenuItem.Visible = validBrowseDir;
        //                commandsToolStripMenuItem.Visible = validBrowseDir;
        //                toolStripFileExplorer.Enabled = validBrowseDir;
        //                if (!isDashboard)
        //                {
        //                    refreshToolStripMenuItem.ShortcutKeys = Keys.F5;
        //                }
        //                else
        //                {
        //                    refreshDashboardToolStripMenuItem.ShortcutKeys = Keys.F5;
        //                }

        //                UpdatePluginMenu(validBrowseDir);
        //                gitMaintenanceToolStripMenuItem.Enabled = validBrowseDir;
        //                editgitignoreToolStripMenuItem1.Enabled = validBrowseDir;
        //                editGitAttributesToolStripMenuItem.Enabled = validBrowseDir;
        //                editmailmapToolStripMenuItem.Enabled = validBrowseDir;
        //                toolStripSplitStash.Enabled = validBrowseDir && !bareRepository;
        //                _createPullRequestsToolStripMenuItem.Enabled = validBrowseDir;
        //                _viewPullRequestsToolStripMenuItem.Enabled = validBrowseDir;

        //                // repositoryToolStripMenuItem.Visible
        //                if (validBrowseDir)
        //                {
        //                    manageSubmodulesToolStripMenuItem.Enabled = !bareRepository;
        //                    updateAllSubmodulesToolStripMenuItem.Enabled = !bareRepository;
        //                    synchronizeAllSubmodulesToolStripMenuItem.Enabled = !bareRepository;
        //                    editgitignoreToolStripMenuItem1.Enabled = !bareRepository;
        //                    editGitAttributesToolStripMenuItem.Enabled = !bareRepository;
        //                    editmailmapToolStripMenuItem.Enabled = !bareRepository;
        //                }

        //                // commandsToolStripMenuItem.Visible
        //                if (validBrowseDir)
        //                {
        //                    commitToolStripMenuItem.Enabled = !bareRepository;
        //                    mergeToolStripMenuItem.Enabled = !bareRepository;
        //                    rebaseToolStripMenuItem1.Enabled = !bareRepository;
        //                    pullToolStripMenuItem1.Enabled = !bareRepository;
        //                    cleanupToolStripMenuItem.Enabled = !bareRepository;
        //                    stashToolStripMenuItem.Enabled = !bareRepository;
        //                    checkoutBranchToolStripMenuItem.Enabled = !bareRepository;
        //                    mergeBranchToolStripMenuItem.Enabled = !bareRepository;
        //                    rebaseToolStripMenuItem.Enabled = !bareRepository;
        //                    applyPatchToolStripMenuItem.Enabled = !bareRepository;
        //                }

        //                stashChangesToolStripMenuItem.Enabled = !bareRepository;
        //                stashStagedToolStripMenuItem.Visible = Module.GitVersion.SupportStashStaged;
        //                gitGUIToolStripMenuItem.Enabled = !bareRepository;

        //                SetShortcutKeyDisplayStringsFromHotkeySettings();

        //                RefreshWorkingDirComboText();

        //                OnActivate();

        //                LoadUserMenu();

        //                if (validBrowseDir)
        //                {
        //                    _windowsJumpListManager.AddToRecent(Module.WorkingDir);

        //                    // add Navigate and View menu
        //                    _formBrowseMenus.ResetMenuCommandSets();
        //                    _formBrowseMenus.AddMenuCommandSet(MainMenuItem.NavigateMenu, RevisionGrid.MenuCommands.NavigateMenuCommands);
        //                    _formBrowseMenus.AddMenuCommandSet(MainMenuItem.ViewMenu, RevisionGrid.MenuCommands.ViewMenuCommands);

        //                    _formBrowseMenus.InsertRevisionGridMainMenuItems(repositoryToolStripMenuItem);

        //                    toolStripButtonPush.DisplayAheadBehindInformation(RevisionGrid.CurrentBranch.Value);

        //                    ActiveControl = RevisionGrid;
        //                }
        //                else
        //                {
        //                    _windowsJumpListManager.DisableThumbnailToolbar();
        //                }

        //                UICommands.RaisePostBrowseInitialize(this);
        //            }

        //            toolPanel.TopToolStripPanel.ResumeLayout();
        //            toolPanel.ResumeLayout();

        //            return;

        //        }

        private void SetShortcutKeyDisplayStringsFromHotkeySettings()
        {
            // Add shortcuts to the menu items
            openToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.OpenRepo).ToShortcutKeyDisplayString();
            gitBashToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.GitBash).ToShortcutKeyDisplayString();
            gitGUIToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.GitGui).ToShortcutKeyDisplayString();
            kGitToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.GitGitK).ToShortcutKeyDisplayString();
            settingsToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Command.OpenSettings).ToShortcutKeyDisplayString();
        }

        private Keys GetShortcutKeys(Command cmd)
        {
            return GetShortcutKeys((int)cmd);
        }

        private void OpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            GitModule? module = FormOpenDirectory.OpenModule(this, Module);
            if (module is not null)
            {
                SetGitModule(this, new GitModuleEventArgs(module));
            }
        }

        private void CloneToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCloneDialog(this, string.Empty, false, SetGitModule);
        }

        private void InitNewRepositoryToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartInitializeDialog(this, gitModuleChanged: SetGitModule);
        }

        private void AboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            using FormAbout frm = new();
            frm.ShowDialog(this);
        }

        private void GitGuiToolStripMenuItemClick(object sender, EventArgs e)
        {
            Module.RunGui();
        }


        private void GitcommandLogToolStripMenuItemClick(object sender, EventArgs e)
        {
            FormGitCommandLog.ShowOrActivate(this);
        }


        //private void OnShowSettingsClick(object sender, EventArgs e)
        //{
        //    //            var translation = AppSettings.Translation;
        //    //            var commitInfoPosition = AppSettings.CommitInfoPosition;

        //    //            UICommands.StartSettingsDialog(this);

        //    //            if (translation != AppSettings.Translation)
        //    //            {
        //    //                Translator.Translate(this, AppSettings.CurrentTranslation);
        //    //            }

        //    //            if (commitInfoPosition != AppSettings.CommitInfoPosition)
        //    //            {
        //    //                LayoutRevisionInfo();
        //    //            }

        //    //            Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
        //    //            RevisionGrid.ReloadHotkeys();
        //    //            RevisionGrid.ReloadTranslation();
        //    //            fileTree.ReloadHotkeys();
        //    //            revisionDiff.ReloadHotkeys();
        //    //            repoObjectsTree.ReloadHotkeys();
        //    //            SetShortcutKeyDisplayStringsFromHotkeySettings();

        //    //            // Clear the separate caches for diff/merge tools
        //    //            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        //    //            {
        //    //                await new CustomDiffMergeToolProvider().ClearAsync(isDiff: false);
        //    //            }).FileAndForget();
        //    //            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
        //    //            {
        //    //                revisionDiff.CancelLoadCustomDifftools();
        //    //                RevisionGrid.CancelLoadCustomDifftools();

        //    //                await new CustomDiffMergeToolProvider().ClearAsync(isDiff: true);

        //    //                // The tool lists are created when the forms are init, must be redone after clearing the cache
        //    //                revisionDiff.LoadCustomDifftools();
        //    //                RevisionGrid.LoadCustomDifftools();
        //    //            }).FileAndForget();

        //    //            _dashboard?.RefreshContent();

        //    //            _gitStatusMonitor.Active = NeedsGitStatusMonitor() && Module.IsValidGitWorkingDir();

        //    //            RefreshDefaultPullAction();
        //}

        private void KGitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Module.RunGitK();
        }

        private void DonateToolStripMenuItemClick(object sender, EventArgs e)
        {
            using FormDonate frm = new();
            frm.ShowDialog(this);
        }

        private static void SaveApplicationSettings()
        {
            AppSettings.SaveSettings();
        }

        private void StartAuthenticationAgentToolStripMenuItemClick(object sender, EventArgs e)
        {
            new Executable(AppSettings.Pageant, Module.WorkingDir).Start();
        }

        private void GenerateOrImportKeyToolStripMenuItemClick(object sender, EventArgs e)
        {
            new Executable(AppSettings.Puttygen, Module.WorkingDir).Start();
        }

        private void ChangelogToolStripMenuItemClick(object sender, EventArgs e)
        {
            using FormChangeLog frm = new();
            frm.ShowDialog(this);
        }

        private void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        private void tsmiFavouriteRepositories_DropDownOpening(object sender, EventArgs e)
        {
            tsmiFavouriteRepositories.DropDown.SuspendLayout();
            tsmiFavouriteRepositories.DropDownItems.Clear();
            PopulateFavouriteRepositoriesMenu(tsmiFavouriteRepositories);
            tsmiFavouriteRepositories.DropDown.ResumeLayout();
        }

        private void tsmiRecentRepositories_DropDownOpening(object sender, EventArgs e)
        {
            tsmiRecentRepositories.DropDown.SuspendLayout();
            tsmiRecentRepositories.DropDownItems.Clear();
            PopulateRecentRepositoriesMenu(tsmiRecentRepositories);
            if (tsmiRecentRepositories.DropDownItems.Count < 1)
            {
                return;
            }

            tsmiRecentRepositories.DropDownItems.Add(clearRecentRepositoriesListToolStripMenuItem);
            TranslateItem(tsmiRecentRepositoriesClear.Name, tsmiRecentRepositoriesClear);
            tsmiRecentRepositories.DropDownItems.Add(tsmiRecentRepositoriesClear);
            tsmiRecentRepositories.DropDown.ResumeLayout();
        }

        private void tsmiRecentRepositoriesClear_Click(object sender, EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                var repositoryHistory = Array.Empty<Repository>();
                await RepositoryHistoryManager.Locals.SaveRecentHistoryAsync(repositoryHistory);

                await this.SwitchToMainThreadAsync();
                _dashboard?.RefreshContent();
            });
        }

        private void RepoSettingsToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartRepoSettingsDialog(this);
        }

        private void UserManualToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Point to the default documentation, will work also if the old doc version is removed
            OsShellUtil.OpenUrlInDefaultBrowser(AppSettings.DocumentationBaseUrl);
        }

        private void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container)
        {
            var repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.LoadFavouriteHistoryAsync());
            if (repositoryHistory.Count < 1)
            {
                return;
            }

            PopulateFavouriteRepositoriesMenu(container, repositoryHistory);
        }

        private void PopulateFavouriteRepositoriesMenu(ToolStripDropDownItem container, in IList<Repository> repositoryHistory)
        {
            List<RecentRepoInfo> pinnedRepos = new();
            List<RecentRepoInfo> allRecentRepos = new();

            using (var graphics = CreateGraphics())
            {
                RecentRepoSplitter splitter = new()
                {
                    MeasureFont = container.Font,
                    Graphics = graphics
                };

                splitter.SplitRecentRepos(repositoryHistory, pinnedRepos, allRecentRepos);
            }

            foreach (var repo in pinnedRepos.Union(allRecentRepos).GroupBy(k => k.Repo.Category).OrderBy(k => k.Key))
            {
                AddFavouriteRepositories(repo.Key, repo.ToList());
            }

            void AddFavouriteRepositories(string? category, IList<RecentRepoInfo> repos)
            {
                ToolStripMenuItem menuItemCategory;
                if (!container.DropDownItems.ContainsKey(category))
                {
                    menuItemCategory = new ToolStripMenuItem(category);
                    container.DropDownItems.Add(menuItemCategory);
                }
                else
                {
                    menuItemCategory = (ToolStripMenuItem)container.DropDownItems[category];
                }

                menuItemCategory.DropDown.SuspendLayout();
                foreach (var r in repos)
                {
                    //_controller.AddRecentRepositories(menuItemCategory, r.Repo, r.Caption, SetGitModule);
                }

                menuItemCategory.DropDown.ResumeLayout();
            }
        }

        private void PopulateRecentRepositoriesMenu(ToolStripDropDownItem container)
        {
            List<RecentRepoInfo> pinnedRepos = new();
            List<RecentRepoInfo> allRecentRepos = new();

            var repositoryHistory = ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.LoadRecentHistoryAsync());
            if (repositoryHistory.Count < 1)
            {
                return;
            }

            using (var graphics = CreateGraphics())
            {
                RecentRepoSplitter splitter = new()
                {
                    MeasureFont = container.Font,
                    Graphics = graphics
                };

                splitter.SplitRecentRepos(repositoryHistory, pinnedRepos, allRecentRepos);
            }

            foreach (var repo in pinnedRepos)
            {
                //_controller.AddRecentRepositories(container, repo.Repo, repo.Caption, SetGitModule);
            }

            if (allRecentRepos.Count > 0)
            {
                if (pinnedRepos.Count > 0 && (AppSettings.SortPinnedRepos || AppSettings.SortAllRecentRepos))
                {
                    container.DropDownItems.Add(new ToolStripSeparator());
                }

                foreach (var repo in allRecentRepos)
                {
                    //_controller.AddRecentRepositories(container, repo.Repo, repo.Caption, SetGitModule);
                }
            }
        }

        //        public void SetWorkingDir(string? path, ObjectId? selectedId = null, ObjectId? firstId = null)
        //        {
        //            RevisionGrid.SelectedId = selectedId;
        //            RevisionGrid.FirstId = firstId;
        //            SetGitModule(this, new GitModuleEventArgs(new GitModule(path)));
        //        }

        private void SetGitModule(object sender, GitModuleEventArgs e) => InitializeView(e.GitModule);

        private void TranslateToolStripMenuItemClick(object sender, EventArgs e)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/wiki/Translations");
        }




        //        #region Hotkey commands

        //        public static readonly string HotkeySettingsName = "Browse";

        //        internal enum Command
        //        {
        //            // Focus or visuals
        //            FocusRevisionGrid = 3,
        //            FocusCommitInfo = 4,
        //            FocusDiff = 5,
        //            FocusFileTree = 6,
        //            FocusFilter = 18,
        //            ToggleBranchTreePanel = 21,
        //            FocusBranchTree = 25,
        //            FocusGpgInfo = 26,
        //            FocusGitConsole = 29,
        //            FocusBuildServerStatus = 30,
        //            FocusNextTab = 31,
        //            FocusPrevTab = 32,

        //            // START menu
        //            OpenRepo = 45,

        //            // DASHBOARD menu

        //            // REPOSITORY menu
        //            CloseRepository = 15,

        //            // COMMANDS menu
        //            Commit = 7,
        //            CheckoutBranch = 10,
        //            PullOrFetch = 39,
        //            Push = 40,
        //            CreateBranch = 41,
        //            MergeBranches = 42,
        //            CreateTag = 43,
        //            Rebase = 44,

        //            // PLUGINS menu

        //            // TOOLS menu
        //            GitBash = 0,
        //            GitGui = 1,
        //            GitGitK = 2,
        //            OpenSettings = 20,

        //            // HELP menu

        //            // Toolbar
        //            AddNotes = 8,
        //            FindFileInSelectedCommit = 9,
        //            QuickFetch = 11,
        //            QuickPull = 12,
        //            QuickPush = 13,
        //            Stash = 16,
        //            StashPop = 17,
        //            GoToSuperproject = 27,
        //            GoToSubmodule = 28,

        //            // Diff or File Tree tab
        //            OpenWithDifftool = 19,
        //            EditFile = 22,
        //            OpenAsTempFile = 23,
        //            OpenAsTempFileWith = 24,
        //            OpenWithDifftoolFirstToLocal = 33,
        //            OpenWithDifftoolSelectedToLocal = 34,

        //            // Revision grid
        //            OpenCommitsWithDifftool = 35,
        //            ToggleBetweenArtificialAndHeadCommits = 36,
        //            GoToChild = 37,
        //            GoToParent = 38

        //            /* deprecated: RotateApplicationIcon = 14, */
        //        }

        //        internal Keys GetShortcutKeys(Command cmd)
        //        {
        //            return GetShortcutKeys((int)cmd);
        //        }

        //        public override bool ProcessHotkey(Keys keyData)
        //        {
        //            if (IsDesignMode || !HotkeysEnabled)
        //            {
        //                return false;
        //            }

        //            // generic handling of this form's hotkeys (upstream)
        //            if (base.ProcessHotkey(keyData))
        //            {
        //                return true;
        //            }

        //            // downstream (without keys for quick search and without keys for text selection and copy e.g. in CommitInfo)
        //            // but allow routing Ctrl+A away from RevisionGridControl in order to not select all revisions
        //            if (GitExtensionsControl.IsTextEditKey(keyData)
        //                && !(keyData == (Keys.Control | Keys.A) && RevisionGridControl.ContainsFocus))
        //            {
        //                return false;
        //            }

        //            // route to visible controls which have their own hotkeys
        //            return (keyData != (Keys.Control | Keys.A) && RevisionGridControl.ProcessHotkey(keyData))
        //                || (CommitInfoTabControl.SelectedTab == DiffTabPage && revisionDiff.ProcessHotkey(keyData))
        //                || (CommitInfoTabControl.SelectedTab == TreeTabPage && fileTree.ProcessHotkey(keyData));
        //        }

        //        protected override CommandStatus ExecuteCommand(int cmd)
        //        {
        //            switch ((Command)cmd)
        //            {
        //                case Command.GitBash: userShell.PerformButtonClick(); break;
        //                case Command.GitGui: Module.RunGui(); break;
        //                case Command.GitGitK: Module.RunGitK(); break;
        //                case Command.FocusBranchTree: FocusBranchTree(); break;
        //                case Command.FocusRevisionGrid: RevisionGrid.Focus(); break;
        //                case Command.FocusCommitInfo: FocusCommitInfo(); break;
        //                case Command.FocusDiff: FocusTabOf(revisionDiff, (c, alreadyContainedFocus) => c.SwitchFocus(alreadyContainedFocus)); break;
        //                case Command.FocusFileTree: FocusTabOf(fileTree, (c, alreadyContainedFocus) => c.SwitchFocus(alreadyContainedFocus)); break;
        //                case Command.FocusGpgInfo when AppSettings.ShowGpgInformation.Value: FocusTabOf(revisionGpgInfo1, (c, alreadyContainedFocus) => c.Focus()); break;
        //                case Command.FocusGitConsole: FocusGitConsole(); break;
        //                case Command.FocusBuildServerStatus: FocusTabOf(_buildReportTabPageExtension?.Control, (c, alreadyContainedFocus) => c.Focus()); break;
        //                case Command.FocusNextTab: FocusNextTab(); break;
        //                case Command.FocusPrevTab: FocusNextTab(forward: false); break;
        //                case Command.FocusFilter: ToolStripFilters.SetFocus(); break;
        //                case Command.OpenRepo: openToolStripMenuItem.PerformClick(); break;
        //                case Command.Commit: UICommands.StartCommitDialog(this); break;
        //                case Command.AddNotes: AddNotes(); break;
        //                case Command.FindFileInSelectedCommit: FindFileInSelectedCommit(); break;
        //                case Command.CheckoutBranch: UICommands.StartCheckoutBranch(this); break;
        //                case Command.QuickFetch: QuickFetch(); break;
        //                case Command.QuickPull: DoPull(pullAction: AppSettings.PullAction.Merge, isSilent: true); break;
        //                case Command.QuickPush: UICommands.StartPushDialog(this, true); break;
        //                case Command.CloseRepository: SetWorkingDir(""); break;
        //                case Command.Stash: UICommands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash); break;
        //                case Command.StashPop: UICommands.StashPop(this); break;
        //                case Command.OpenCommitsWithDifftool: RevisionGrid.DiffSelectedCommitsWithDifftool(); break;
        //                case Command.OpenWithDifftool: OpenWithDifftool(); break;
        //                case Command.OpenWithDifftoolFirstToLocal: OpenWithDifftoolFirstToLocal(); break;
        //                case Command.OpenWithDifftoolSelectedToLocal: OpenWithDifftoolSelectedToLocal(); break;
        //                case Command.OpenSettings: EditSettings.PerformClick(); break;
        //                case Command.ToggleBranchTreePanel: toggleBranchTreePanel.PerformClick(); break;
        //                case Command.EditFile: EditFile(); break;
        //                case Command.OpenAsTempFile when fileTree.Visible: fileTree.ExecuteCommand(RevisionFileTreeControl.Command.OpenAsTempFile); break;
        //                case Command.OpenAsTempFileWith when fileTree.Visible: fileTree.ExecuteCommand(RevisionFileTreeControl.Command.OpenAsTempFileWith); break;
        //                case Command.GoToSuperproject: toolStripButtonLevelUp.PerformClick(); break;
        //                case Command.GoToSubmodule: toolStripButtonLevelUp.ShowDropDown(); break;
        //                case Command.ToggleBetweenArtificialAndHeadCommits: RevisionGrid?.ExecuteCommand(RevisionGridControl.Command.ToggleBetweenArtificialAndHeadCommits); break;
        //                case Command.GoToChild: RestoreFileStatusListFocus(() => RevisionGrid?.ExecuteCommand(RevisionGridControl.Command.GoToChild)); break;
        //                case Command.GoToParent: RestoreFileStatusListFocus(() => RevisionGrid?.ExecuteCommand(RevisionGridControl.Command.GoToParent)); break;
        //                case Command.PullOrFetch: DoPull(pullAction: AppSettings.FormPullAction, isSilent: false); break;
        //                case Command.Push: UICommands.StartPushDialog(this, pushOnShow: ModifierKeys.HasFlag(Keys.Shift)); break;
        //                case Command.CreateBranch: UICommands.StartCreateBranchDialog(this, RevisionGrid.LatestSelectedRevision?.ObjectId); break;
        //                case Command.MergeBranches: UICommands.StartMergeBranchDialog(this, null); break;
        //                case Command.CreateTag: UICommands.StartCreateTagDialog(this, RevisionGrid.LatestSelectedRevision); break;
        //                case Command.Rebase: rebaseToolStripMenuItem.PerformClick(); break;
        //                default: return base.ExecuteCommand(cmd);
        //            }

        //            return true;

        //            void FocusBranchTree()
        //            {
        //                if (!MainSplitContainer.Panel1Collapsed)
        //                {
        //                    repoObjectsTree.Focus();
        //                }
        //            }

        //            void FocusCommitInfo()
        //            {
        //                if (AppSettings.CommitInfoPosition == CommitInfoPosition.BelowList)
        //                {
        //                    CommitInfoTabControl.SelectedTab = CommitInfoTabPage;
        //                }

        //                RevisionInfo.Focus();
        //            }

        //            void FocusTabOf<T>(T? control, Action<T, bool> switchFocus) where T : Control
        //            {
        //                if (control is not null)
        //                {
        //                    var tabPage = control.Parent as TabPage;
        //                    if (CommitInfoTabControl.TabPages.IndexOf(tabPage) >= 0)
        //                    {
        //                        bool alreadyContainedFocus = control.ContainsFocus;

        //                        if (CommitInfoTabControl.SelectedTab != tabPage)
        //                        {
        //                            CommitInfoTabControl.SelectedTab = tabPage;
        //                        }

        //                        switchFocus(control, alreadyContainedFocus);
        //                    }
        //                }
        //            }

        //            void FocusGitConsole()
        //            {
        //                FillTerminalTab();
        //                if (_consoleTabPage is not null && CommitInfoTabControl.TabPages.Contains(_consoleTabPage))
        //                {
        //                    CommitInfoTabControl.SelectedTab = _consoleTabPage;
        //                }
        //            }

        //            void FocusNextTab(bool forward = true)
        //            {
        //                int tabIndex = CommitInfoTabControl.SelectedIndex;
        //                tabIndex += forward ? 1 : (CommitInfoTabControl.TabCount - 1);
        //                CommitInfoTabControl.SelectedIndex = tabIndex % CommitInfoTabControl.TabCount;
        //            }

        //            void OpenWithDifftool()
        //            {
        //                if (revisionDiff.Visible)
        //                {
        //                    revisionDiff.ExecuteCommand(RevisionDiffControl.Command.OpenWithDifftool);
        //                }
        //                else if (fileTree.Visible)
        //                {
        //                    fileTree.ExecuteCommand(RevisionFileTreeControl.Command.OpenWithDifftool);
        //                }
        //            }

        //            void OpenWithDifftoolFirstToLocal()
        //            {
        //                if (revisionDiff.Visible)
        //                {
        //                    revisionDiff.ExecuteCommand(RevisionDiffControl.Command.OpenWithDifftoolFirstToLocal);
        //                }
        //            }

        //            void OpenWithDifftoolSelectedToLocal()
        //            {
        //                if (revisionDiff.Visible)
        //                {
        //                    revisionDiff.ExecuteCommand(RevisionDiffControl.Command.OpenWithDifftoolSelectedToLocal);
        //                }
        //            }

        //            void EditFile()
        //            {
        //                if (revisionDiff.Visible)
        //                {
        //                    revisionDiff.ExecuteCommand(RevisionDiffControl.Command.EditFile);
        //                }
        //                else if (fileTree.Visible)
        //                {
        //                    fileTree.ExecuteCommand(RevisionFileTreeControl.Command.EditFile);
        //                }
        //            }

        //            void RestoreFileStatusListFocus(Action action)
        //            {
        //                bool restoreFocus = revisionDiff.ContainsFocus;

        //                action();

        //                if (restoreFocus)
        //                {
        //                    revisionDiff.SwitchFocus(alreadyContainedFocus: false);
        //                }
        //            }
        //        }

        //        internal CommandStatus ExecuteCommand(Command cmd)
        //        {
        //            return ExecuteCommand((int)cmd);
        //        }

        //        #endregion

        //        }

        private void TsmiTelemetryEnabled_Click(object sender, EventArgs e)
        {
            UICommands.StartGeneralSettingsDialog(this);
        }

        private void HelpToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            tsmiTelemetryEnabled.Checked = AppSettings.TelemetryEnabled ?? false;
        }

        private void gitBashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (userShell.DropDownButtonPressed)
            //{
            //    return;
            //}

            if ((sender as ToolStripItem)?.Tag is not IShellDescriptor shell)
            {
                return;
            }

            try
            {
                Validates.NotNull(shell.ExecutablePath);

                Executable executable = new(shell.ExecutablePath, Module.WorkingDir);
                executable.Start(createWindow: true, throwOnErrorExit: false); // throwOnErrorExit would redirect the output
            }
            catch (Exception exception)
            {
                MessageBoxes.FailedToRunShell(this, shell.Name, exception);
            }
        }

        private void reportAnIssueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UserEnvironmentInformation.CopyInformation();
            OsShellUtil.OpenUrlInDefaultBrowser(@"https://github.com/gitextensions/gitextensions/issues");
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormUpdates updateForm = new(AppSettings.AppVersion);
            updateForm.SearchForUpdatesAndShow(Owner, true);
        }
    }

    internal enum Command
    {
        // Focus or visuals
        FocusRevisionGrid = 3,
        FocusCommitInfo = 4,
        FocusDiff = 5,
        FocusFileTree = 6,
        FocusFilter = 18,
        ToggleBranchTreePanel = 21,
        FocusBranchTree = 25,
        FocusGpgInfo = 26,
        FocusGitConsole = 29,
        FocusBuildServerStatus = 30,
        FocusNextTab = 31,
        FocusPrevTab = 32,

        // START menu
        OpenRepo = 45,

        // DASHBOARD menu

        // REPOSITORY menu
        CloseRepository = 15,

        // COMMANDS menu
        Commit = 7,
        CheckoutBranch = 10,
        PullOrFetch = 39,
        Push = 40,
        CreateBranch = 41,
        MergeBranches = 42,
        CreateTag = 43,
        Rebase = 44,

        // PLUGINS menu

        // TOOLS menu
        GitBash = 0,
        GitGui = 1,
        GitGitK = 2,
        OpenSettings = 20,

        // HELP menu

        // Toolbar
        AddNotes = 8,
        FindFileInSelectedCommit = 9,
        QuickFetch = 11,
        QuickPull = 12,
        QuickPush = 13,
        Stash = 16,
        StashStaged = 46,
        StashPop = 17,
        GoToSuperproject = 27,
        GoToSubmodule = 28,

        // Diff or File Tree tab
        OpenWithDifftool = 19,
        EditFile = 22,
        OpenAsTempFile = 23,
        OpenAsTempFileWith = 24,
        OpenWithDifftoolFirstToLocal = 33,
        OpenWithDifftoolSelectedToLocal = 34,

        // Revision grid
        OpenCommitsWithDifftool = 35,
        ToggleBetweenArtificialAndHeadCommits = 36,
        GoToChild = 37,
        GoToParent = 38

        /* deprecated: RotateApplicationIcon = 14, */
    }

    public enum MenuCommands
    {
        // File
        InitRepo,
        OpenRepo,
        FavouriteRepos,
        RecentRepos,
        ClearRecentRepos,
        CloneRepo,
        ExitApp,

        // Tools
        GitBash,
        GitGui,
        GitGitK,
        GitCommandLog,
        StartAuthenticationAgent,
        GenerateOrImportKey,
        OpenSettings,

        // Help
        UserManual,
        ChangeLog,
        Translate,
        Donate,
        Telemetry,
        ReportIssue,
        CheckUpdate,
        About,
    }

    public class MenuCommandService
    {
        private Dictionary<ToolStripItem, MenuCommands> _toolStripItemCommands = new();

        private MenuCommandService()
        {
        }

        public static MenuCommandService Instance { get; } = new();

        public void Register(ToolStripItem menuItem, MenuCommands command, EventHandler? onClick = null)
        {
            if (_toolStripItemCommands.ContainsKey(menuItem))
            {
                throw new InvalidOperationException("TODO");
            }

            if (_toolStripItemCommands.ContainsValue(command))
            {
                throw new InvalidOperationException("TODO");
            }

            _toolStripItemCommands[menuItem] = command;

            if (onClick is not null)
            {
                menuItem.Click += onClick;
            }
        }
    }

    public interface IMainMenuExtender
    {
        MenuStrip ControlMenu { get; }
    }
}
#pragma warning restore SA1515
#pragma warning restore SA1507
#pragma warning restore SA1005

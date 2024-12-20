using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ConEmu.WinForms;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitCommands.Gpg;
using GitCommands.Submodules;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Avatars;
using GitUI.CommandsDialogs.WorktreeDialog;
using GitUI.HelperDialogs;
using GitUI.Infrastructure.Telemetry;
using GitUI.LeftPanel;
using GitUI.NBugReports;
using GitUI.Properties;
using GitUI.ScriptsEngine;
using GitUI.Shells;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.Win32;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class RepoBrowser : GitModuleControl, IMainMenuExtender
    {
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

        #region Translation

        private readonly TranslationString _closeAll = new("Close all windows");

        private readonly TranslationString _noSubmodulesPresent = new("No submodules");
        private readonly TranslationString _topProjectModuleFormat = new("Top project: {0}");
        private readonly TranslationString _superprojectModuleFormat = new("Superproject: {0}");
        private readonly TranslationString _goToSuperProject = new("Go to superproject");

        private readonly TranslationString _indexLockCantDelete = new("Failed to delete index.lock");

        private readonly TranslationString _loading = new("Loading...");

        private readonly TranslationString _noReposHostPluginLoaded = new("No repository host plugin loaded.");
        private readonly TranslationString _noReposHostFound = new("Could not find any relevant repository hosts for the currently open repository.");

        private readonly TranslationString _updateCurrentSubmodule = new("Update current submodule");

        private readonly TranslationString _pullFetch = new("Fetch");
        private readonly TranslationString _pullFetchAll = new("Fetch all");
        private readonly TranslationString _pullFetchPruneAll = new("Fetch and prune all");
        private readonly TranslationString _pullMerge = new("Pull - merge");
        private readonly TranslationString _pullRebase = new("Pull - rebase");
        private readonly TranslationString _pullOpenDialog = new("Open pull dialog");

        private readonly TranslationString _buildReportTabCaption = new("Build Report");
        private readonly TranslationString _consoleTabCaption = new("Console");
        private readonly TranslationString _outputHistoryTabCaption = new("Output");

        private readonly TranslationString _commitButtonText = new("Commit");

        private readonly TranslationString _undoLastCommitText = new("You will still be able to find all the commit's changes in the staging area\n\nDo you want to continue?");
        private readonly TranslationString _undoLastCommitCaption = new("Undo last commit");

        #endregion

        private readonly SplitterManager _splitterManager;
        private readonly FormBrowseMenus _formBrowseMenus;
        private readonly IAheadBehindDataProvider? _aheadBehindDataProvider;
        private readonly IWindowsJumpListManager _windowsJumpListManager;
        private readonly ISubmoduleStatusProvider _submoduleStatusProvider;
        private readonly IScriptsManager _scriptsManager;
        private readonly IRepositoryHistoryUIService _repositoryHistoryUIService;
        private List<ToolStripItem>? _currentSubmoduleMenuItems;
        private readonly ShellProvider _shellProvider = new();
        private bool _isFileHistoryMode;

        /// <summary>
        /// Open Browse - main GUI including dashboard.
        /// </summary>
        /// <param name="commands">The commands in the current form.</param>
        /// <param name="args">The start up arguments.</param>
        public RepoBrowser(IGitUICommands commands, BrowseArguments args)
#pragma warning disable CS0618 // Type or member is obsolete
            : this(commands, args, new AppSettingsPath("FormBrowse"))
#pragma warning restore CS0618 // Type or member is obsolete
        {
        }

        [Obsolete("Test only!")]
        internal RepoBrowser(IGitUICommands commands, BrowseArguments args, SettingsSource settingsSource)
        {
            _splitterManager = new(settingsSource);

            SystemEvents.SessionEnding += (sender, args) => SaveApplicationSettings();

            _isFileHistoryMode = args.IsFileHistoryMode;
            InitializeComponent();

            _repositoryHistoryUIService = commands.GetRequiredService<IRepositoryHistoryUIService>();

            _NO_TRANSLATE_WorkingDir.Initialize(() => UICommands, _repositoryHistoryUIService, closeToolStripMenuItem);

            _repositoryHistoryUIService.GitModuleChanged += OpenGitModule;

            BackColor = OtherColors.BackgroundColor;

            WorkaroundPaddingIncreaseBug();

            _windowsJumpListManager = commands.GetRequiredService<IWindowsJumpListManager>();
            _scriptsManager = commands.GetRequiredService<IScriptsManager>();

            mnuRepositoryHosts.Visible = false;

            MainSplitContainer.Visible = false;
            MainSplitContainer.SplitterDistance = DpiUtil.Scale(260);

            _formBrowseMenus = new FormBrowseMenus(mainMenuStrip);

            // TODO:
            ////ToolStripFilters.Bind(() => Module, RevisionGrid);

            // Please do not ask me why the setting in the Designer has no effect!
            Color splitterBackColor = LeftSplitContainer.BackColor;
            LeftSplitContainer.Invalidated += FixupSplitterColor;

            InitializeComplete();

            HotkeysEnabled = true;
            LoadHotkeys(HotkeySettingsName);
            SetShortcutKeyDisplayStringsFromHotkeySettings();
            InitMenusAndToolbars(args.RevFilter, args.PathFilter.ToPosixPath());

            _submoduleStatusProvider = commands.GetRequiredService<ISubmoduleStatusProvider>();
            _submoduleStatusProvider.StatusUpdating += SubmoduleStatusProvider_StatusUpdating;
            _submoduleStatusProvider.StatusUpdated += SubmoduleStatusProvider_StatusUpdated;

            _aheadBehindDataProvider = new AheadBehindDataProvider(() => Module.GitExecutable);

            // Application is init, the repo related operations are triggered in OnLoad()
            return;

            void FixupSplitterColor(object? sender, EventArgs eventArgs)
            {
                LeftSplitContainer.BackColor = splitterBackColor;
                LeftSplitContainer.Invalidated -= FixupSplitterColor;
            }

            void WorkaroundPaddingIncreaseBug()
            {
                MainSplitContainer.Panel1.Padding = new Padding(1);
            }
        }

        MenuStrip IMainMenuExtender.ControlMenu => mainMenuStrip;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_repositoryHistoryUIService is not null)
                {
                    _repositoryHistoryUIService.GitModuleChanged -= OpenGitModule;
                }

                _formBrowseMenus?.Dispose();
                components?.Dispose();
                _windowsJumpListManager?.Dispose();
            }

            base.Dispose(disposing);
        }

        /*
        protected override void OnRuntimeLoad()
        {
            base.OnRuntimeLoad();

            ////InitCountArtificial(out _gitStatusMonitor);

            InitRevisionGrid(_args.SelectedId, _args.FirstId, _args.IsFileBlameHistory);
            InitCommitDetails();

            toolStripButtonPush.Initialize(_aheadBehindDataProvider);
            repoObjectsTree.Initialize(_aheadBehindDataProvider, filterRevisionGridBySpaceSeparatedRefs: ToolStripFilters.SetBranchFilter, RevisionGrid, RevisionGrid, RevisionGrid);
            revisionDiff.Bind(RevisionGrid, fileTree, RefreshGitStatusMonitor);

            // Show blame by default if not started from command line
            fileTree.Bind(RevisionGrid, RefreshGitStatusMonitor, _isFileBlameHistory);

            RevisionGrid.ResumeRefreshRevisions();
        }
        */

        protected override void OnUICommandsSourceSet(IGitUICommandsSource source)
        {
            base.OnUICommandsSourceSet(source);

            source.UICommandsChanged += UICommandsSource_UICommandsChanged;

            // TODO: work out how to not call this, and delegate everything to UICommandsSource_UICommandsChanged
            OpenGitModule(this, new(source.UICommands.Module));
            return;

            void UICommandsSource_UICommandsChanged(object? sender, GitUICommandsChangedEventArgs e)
            {
                IGitUICommands oldCommands = e.OldCommands;

                if (oldCommands is not null)
                {
                    oldCommands.PostRepositoryChanged -= UICommands_PostRepositoryChanged;
                }

                UICommands.PostRepositoryChanged += UICommands_PostRepositoryChanged;

                OpenGitModule(this, new(UICommands.Module));
                return;

                void UICommands_PostRepositoryChanged(object sender, GitUIEventArgs e)
                {
                    // Note that this called in most FormBrowse context to "be sure"
                    // that the repo has not been updated externally.
                }
            }
        }

        ////protected override void OnLoad(EventArgs e)
        ////{
        ////    _formBrowseMenus.CreateToolbarsMenus(ToolStripMain, ToolStripFilters, ToolStripScripts);

        ////    RefreshSplitViewLayout();
        ////    SetSplitterPositions();
        ////    base.OnLoad(e);

        ////    // All app init is done, make all repo related similar to switching repos
        ////    OpenGitModule(this, new GitModuleEventArgs(new GitModule(Module.WorkingDir)));
        ////}

        public override void AddTranslationItems(ITranslation translation)
        {
            base.AddTranslationItems(translation);
            TranslationUtils.AddTranslationItemsFromFields(Name, ToolStripFilters, translation);
        }

        public override void TranslateItems(ITranslation translation)
        {
            base.TranslateItems(translation);
            TranslationUtils.TranslateItemsFromFields(Name, ToolStripFilters, translation);
        }

        /// <summary>
        /// Set the path filter.
        /// </summary>
        /// <param name="pathFilter">Zero or more quoted paths, separated by spaces.</param>
        public void SetPathFilter(string pathFilter)
        {
        }

        private void UpdatePluginMenu(bool validWorkingDir)
        {
            foreach (ToolStripItem item in mnuPlugins.DropDownItems)
            {
                if (item == pluginsLoadingToolStripMenuItem)
                {
                    continue;
                }

                item.Enabled = !(item.Tag is IGitPluginForRepository) || validWorkingDir;
            }
        }

        private void RegisterPlugins()
        {
            const string PluginManagerName = "Plugin Manager";
            ILookup<object, ToolStripMenuItem> existingPluginMenus = mnuPlugins.DropDownItems.OfType<ToolStripMenuItem>().ToLookup(c => c.Tag);

            lock (PluginRegistry.Plugins)
            {
                if (PluginRegistry.Plugins.Count == 0)
                {
                    return;
                }

                if (mnuPlugins.DropDownItems.Contains(pluginsLoadingToolStripMenuItem))
                {
                    mnuPlugins.DropDownItems.Remove(pluginsLoadingToolStripMenuItem);
                }

                IOrderedEnumerable<IGitPlugin> pluginEntries = PluginRegistry.Plugins
                    .OrderByDescending(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase);

                // pluginsToolStripMenuItem.DropDownItems menu already contains at least 2 items:
                //    [1] Separator
                //    [0] Plugin Settings
                // insert all plugins except 'Plugin Manager' above the separator
                foreach (IGitPlugin plugin in pluginEntries)
                {
                    // don't add the plugin to the Plugins menu, if already added
                    if (existingPluginMenus.Contains(plugin))
                    {
                        continue;
                    }

                    ToolStripMenuItem item = new()
                    {
                        Text = plugin.Name,
                        Image = plugin.Icon,
                        Tag = plugin
                    };
                    item.Click += delegate
                    {
                        if (plugin.Execute(new GitUIEventArgs(this, UICommands)))
                        {
                            UICommands.RepoChangedNotifier.Notify();
                        }
                    };

                    if (plugin.Name == PluginManagerName)
                    {
                        // insert Plugin Manager below the separator
                        mnuPlugins.DropDownItems.Insert(mnuPlugins.DropDownItems.Count - 1, item);
                    }
                    else
                    {
                        // Handle special case where Git Hosting plugin has already been loaded
                        ToolStripItem first = mnuPlugins.DropDownItems[0];
                        int index = first is ToolStripSeparator || string.CompareOrdinal(first.Text, item.Text) >= 0 ? 0 : 1;

                        mnuPlugins.DropDownItems.Insert(index, item);
                    }
                }

                mainMenuStrip?.Refresh();
            }

            // Allow the plugin to perform any self-registration actions
            PluginRegistry.Register(UICommands);

            UICommands.RaisePostRegisterPlugin(this);

            UpdatePluginMenu(Module.IsValidGitWorkingDir());
        }

        private void InternalInitialize()
        {
            using (WaitCursorScope.Enter())
            {
                bool hasWorkingDir = !string.IsNullOrEmpty(Module.WorkingDir);

                bool bareRepository = Module.IsBareRepository();
                bool validBrowseDir = Module.IsValidGitWorkingDir();

                // TODO:
                branchSelect.Text = "TODO:";
                ////branchSelect.Text = validBrowseDir
                ////    ? !string.IsNullOrWhiteSpace(RevisionGrid.CurrentBranch.Value)
                ////        ? RevisionGrid.CurrentBranch.Value
                ////        : DetachedHeadParser.DetachedBranch
                ////    : "";
                toolStripButtonLevelUp.Enabled = hasWorkingDir && !bareRepository;
                fileExplorerToolStripMenuItem.Enabled = validBrowseDir;
                manageRemoteRepositoriesToolStripMenuItem1.Enabled = validBrowseDir;
                branchSelect.Enabled = validBrowseDir;
                toolStripButtonCommit.Enabled = validBrowseDir && !bareRepository;

                toolStripButtonPull.Enabled = validBrowseDir;
                toolStripButtonPush.Enabled = validBrowseDir;
                toolStripButtonPush.ResetBeforeUpdate();
                mnuPlugins.Visible = validBrowseDir;
                mnuRepository.Visible = validBrowseDir;
                mnuCommands.Visible = validBrowseDir;
                toolStripFileExplorer.Enabled = validBrowseDir;
                refreshToolStripMenuItem.ShortcutKeys = Keys.F5;

                UpdatePluginMenu(validBrowseDir);
                gitMaintenanceToolStripMenuItem.Enabled = validBrowseDir;
                editgitignoreToolStripMenuItem1.Enabled = validBrowseDir;
                editGitAttributesToolStripMenuItem.Enabled = validBrowseDir;
                editmailmapToolStripMenuItem.Enabled = validBrowseDir;
                toolStripSplitStash.Enabled = validBrowseDir && !bareRepository;
                _createPullRequestsToolStripMenuItem.Enabled = validBrowseDir;
                _viewPullRequestsToolStripMenuItem.Enabled = validBrowseDir;
                _addUpstreamRemoteToolStripMenuItem.Enabled = validBrowseDir;

                // repositoryToolStripMenuItem.Visible
                if (validBrowseDir)
                {
                    manageSubmodulesToolStripMenuItem.Enabled = !bareRepository;
                    updateAllSubmodulesToolStripMenuItem.Enabled = !bareRepository;
                    synchronizeAllSubmodulesToolStripMenuItem.Enabled = !bareRepository;
                    editgitignoreToolStripMenuItem1.Enabled = !bareRepository;
                    editGitAttributesToolStripMenuItem.Enabled = !bareRepository;
                    editmailmapToolStripMenuItem.Enabled = !bareRepository;
                }

                // commandsToolStripMenuItem.Visible
                if (validBrowseDir)
                {
                    commitToolStripMenuItem.Enabled = !bareRepository;
                    mergeToolStripMenuItem.Enabled = !bareRepository;
                    rebaseToolStripMenuItem1.Enabled = !bareRepository;
                    pullToolStripMenuItem1.Enabled = !bareRepository;
                    cleanupToolStripMenuItem.Enabled = !bareRepository;
                    stashToolStripMenuItem.Enabled = !bareRepository;
                    checkoutBranchToolStripMenuItem.Enabled = !bareRepository;
                    mergeBranchToolStripMenuItem.Enabled = !bareRepository;
                    rebaseToolStripMenuItem.Enabled = !bareRepository;
                    applyPatchToolStripMenuItem.Enabled = !bareRepository;
                }

                stashChangesToolStripMenuItem.Enabled = !bareRepository;
                stashStagedToolStripMenuItem.Visible = Module.GitVersion.SupportStashStaged;

                _NO_TRANSLATE_WorkingDir.RefreshContent();

                LoadUserMenu();
                toolStripButtonLevelUp.Image = validBrowseDir && Module.SuperprojectModule is not null ? Images.NavigateUp : Images.SubmodulesManage;

                if (validBrowseDir)
                {
                    _windowsJumpListManager.AddToRecent(Module.WorkingDir);

                    // add Navigate and View menu

                    _formBrowseMenus.InsertRevisionGridMainMenuItems(mnuRepository);

                    if (AppSettings.ShowAheadBehindData)
                    {
                        string currentBranch = "";
                        ThreadHelper.FileAndForget(async () =>
                        {
                            // Always query only current branch here
                            // because, due to race condition with left panel async refresh:
                            // * when there are a lot of branches, we end up here 1st (and so, we want only the current branch data
                            // because getting ahead - behind data for all branches will be (very ?) long
                            // * when there are few branches, we will end up here not in 1st
                            // and the data will be taken from cache (so what we pass as argument is kind of useless)
                            IDictionary<string, AheadBehindData> aheadBehindData = _aheadBehindDataProvider?.GetData(currentBranch);
                            await this.SwitchToMainThreadAsync();

                            // TODO:
                            ////toolStripButtonPush.DisplayAheadBehindInformation(aheadBehindData, currentBranch, GetShortcutKeyTooltipString(Command.Push));
                        });
                    }

                    // TODO:
                    ////ActiveControl = RevisionGrid;
                }
                else
                {
                    _windowsJumpListManager.EnableThumbnailToolbar(Module.IsValidGitWorkingDir());
                }

                UICommands.RaisePostBrowseInitialize(this);
            }

            return;

            void LoadUserMenu()
            {
                List<ScriptInfo> scripts = _scriptsManager.GetScripts()
                    .Where(script => script.Enabled && script.OnEvent == ScriptEvent.ShowInUserMenuBar)
                    .ToList();

                for (int i = ToolStripScripts.Items.Count - 1; i >= 0; i--)
                {
                    if (ToolStripScripts.Items[i].Tag as string == "userscript")
                    {
                        ToolStripScripts.Items.RemoveAt(i);
                    }
                }

                if (scripts.Count == 0)
                {
                    ToolStripScripts.GripStyle = ToolStripGripStyle.Hidden;
                    return;
                }

                ToolStripScripts.GripStyle = ToolStripGripStyle.Visible;
                foreach (ScriptInfo script in scripts)
                {
                    ToolStripButton button = new()
                    {
                        // store scriptname
                        Text = script.Name,
                        Tag = "userscript",
                        Enabled = true,
                        Visible = true,
                        Image = script.GetIcon(),
                        DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
                    };

                    UpdateTooltipWithShortcut(button, (Command)script.HotkeyCommandIdentifier);

                    button.Click += (s, e) => ExecuteCommand(script.HotkeyCommandIdentifier);

                    // add to toolstrip
                    ToolStripScripts.Items.Add(button);
                }
            }
        }

        private void SetShortcutKeyDisplayStringsFromHotkeySettings()
        {
            // Add shortcuts to the menu items
            commitToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.Commit);
            stashChangesToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.Stash);
            stashStagedToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.StashStaged);
            stashPopToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.StashPop);
            closeToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.CloseRepository);
            checkoutBranchToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.CheckoutBranch);
            branchToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.CreateBranch);
            tagToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.CreateTag);
            mergeBranchToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.MergeBranches);
            pullToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.PullOrFetch);
            pullToolStripMenuItem1.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.PullOrFetch);
            pushToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.Push);
            rebaseToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.Rebase);

            // TODO:
            ////fileToolStripMenuItem.RefreshShortcutKeys(Hotkeys);
            ////helpToolStripMenuItem.RefreshShortcutKeys(Hotkeys);
            ////toolsToolStripMenuItem.RefreshShortcutKeys(Hotkeys);
            ////ToolStripFilters.RefreshBrowseDialogShortcutKeys(Hotkeys);
            ToolStripFilters.RefreshRevisionGridShortcutKeys(GetHotkeys(RevisionGridControl.HotkeySettingsName));

            // Set shortcuts on the Browse toolbar with commands in RevGrid
            // TODO:
            ////RevisionGrid.SetFilterShortcutKeys(ToolStripFilters);

            // TODO: add more
            UpdateTooltipWithShortcut(toggleLeftPanel, Command.ToggleLeftPanel);
            UpdateTooltipWithShortcut(toolStripButtonCommit, Command.Commit);
            UpdateTooltipWithShortcut(EditSettings, Command.OpenSettings);
            UpdateTooltipWithShortcut(branchSelect, Command.CheckoutBranch);
            UpdateTooltipWithShortcut(toolStripFileExplorer, fileExplorerToolStripMenuItem.ShortcutKeys);
            UpdateTooltipWithShortcut(RefreshButton, Keys.F5);
            UpdateTooltipWithShortcut(userShell, Command.GitBash);
        }

        private void UpdateStashCount()
        {
            if (AppSettings.ShowStashCount && !Module.IsBareRepository())
            {
                ThreadHelper.FileAndForget(async () =>
                {
                    // Add a delay to not interfere with GUI updates when switching repository
                    await Task.Delay(500);

                    int result = Module.GetStashes(noLocks: true).Count;

                    await this.SwitchToMainThreadAsync();

                    toolStripSplitStash.Text = $"({result})";
                });
            }
            else
            {
                toolStripSplitStash.Text = string.Empty;
            }
        }

        private void RefreshLeftPanel(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs, Lazy<IReadOnlyCollection<GitRevision>> getStashRevs, bool forceRefresh)
        {
            repoObjectsTree.RefreshRevisionsLoading(getRefs, getStashRevs, forceRefresh);
        }

        private void CheckoutToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCheckoutRevisionDialog(this);
        }

        private void CommitToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCommitDialog(this);
        }

        private void PushToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartPushDialog(this, pushOnShow: ModifierKeys.HasFlag(Keys.Shift));
        }

        private void RefreshToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Broadcast RepoChanged in case repo was changed outside of GE
            UICommands.RepoChangedNotifier.Notify();
        }

        private void PatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartViewPatchDialog(this);
        }

        private void ApplyPatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartApplyPatchDialog(this);
        }

        private void userShell_Click(object sender, EventArgs e)
        {
            if (userShell.DropDownButtonPressed)
            {
                return;
            }

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

        private void FormatPatchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartFormatPatchDialog(this);
        }

        private void CheckoutBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCheckoutBranch(this);
        }

        private void StashToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartStashDialog(this);
            UpdateStashCount();
        }

        private void ResetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UICommands.StartResetChangesDialog(this, Module.GetWorkTreeFiles(), onlyWorkTree: false);

            // TODO:
            ////revisionDiff.RefreshArtificial();
        }

        private void RunMergetoolToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartResolveConflictsDialog(this);
        }

        private void CurrentBranchClick(object sender, EventArgs e)
        {
            branchSelect.ShowDropDown();
        }

        private void DeleteBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartDeleteBranchDialog(this, string.Empty);
        }

        private void DeleteTagToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartDeleteTagDialog(this, null);
        }

        private void CherryPickToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO:
            IReadOnlyList<GitRevision> revisions = []; // RevisionGrid.GetSelectedRevisions(SortDirection.Descending);

            UICommands.StartCherryPickDialog(this, revisions);
        }

        private void MergeBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartMergeBranchDialog(this, null);
        }

        private void OnShowSettingsClick(object sender, EventArgs e)
        {
            string translation = AppSettings.Translation;
            CommitInfoPosition commitInfoPosition = AppSettings.CommitInfoPosition;

            UICommands.StartSettingsDialog(this);

            HandleSettingsChanged(translation, commitInfoPosition);
        }

        private void HandleSettingsChanged(string oldTranslation, CommitInfoPosition oldCommitInfoPosition)
        {
            if (oldTranslation != AppSettings.Translation)
            {
                Translator.Translate(this, AppSettings.CurrentTranslation);
            }

            LoadHotkeys(HotkeySettingsName);
            repoObjectsTree.ReloadHotkeys();
            SetShortcutKeyDisplayStringsFromHotkeySettings();
            AvatarService.UpdateAvatarInitialFontsSettings();

            // Clear the separate caches for diff/merge tools
            ThreadHelper.FileAndForget(() => new CustomDiffMergeToolProvider().ClearAsync(isDiff: false));

            RefreshDefaultPullAction();
        }

        private void TagToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO:
        }

        private static void SaveApplicationSettings()
        {
            AppSettings.SaveSettings();
        }

        private void EditGitignoreToolStripMenuItem1Click(object sender, EventArgs e)
        {
            UICommands.StartEditGitIgnoreDialog(this, false);
        }

        private void EditGitInfoExcludeToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartEditGitIgnoreDialog(this, true);
        }

        private void ArchiveToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO:
            IReadOnlyList<GitRevision> revisions = []; // RevisionGrid.GetSelectedRevisions();
            if (revisions.Count is (< 1 or > 2))
            {
                MessageBoxes.SelectOnlyOneOrTwoRevisions(this);
                return;
            }

            GitRevision mainRevision = revisions[0];
            GitRevision? diffRevision = revisions.Count == 2 ? revisions[1] : null;

            UICommands.StartArchiveDialog(this, mainRevision, diffRevision);
        }

        private void EditMailMapToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartMailMapDialog(this);
        }

        private void EditLocalGitConfigToolStripMenuItemClick(object sender, EventArgs e)
        {
            string fileName = Path.Combine(Module.ResolveGitInternalPath("config"));
            UICommands.StartFileEditorDialog(fileName, true);
        }

        private void CompressGitDatabaseToolStripMenuItemClick(object sender, EventArgs e)
        {
            FormProcess.ReadDialog(this, UICommands, arguments: "gc", Module.WorkingDir, input: null, useDialogSettings: true);
        }

        private void recoverLostObjectsToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartVerifyDatabaseDialog(this);
        }

        private void ManageRemoteRepositoriesToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartRemotesDialog(this);
        }

        private void RebaseToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO:
            IReadOnlyList<GitRevision> revisions = []; // RevisionGrid.GetSelectedRevisions();

            if (revisions.Count == 0 || revisions[0].IsArtificial)
            {
                return;
            }

            string onto = revisions[0].ObjectId.ToString(); // 2nd selected commit
            if (revisions.Count == 2)
            {
                // Set defaults in rebase form to rebase commits defined by the range *from* first selected commit *to* HEAD
                // *onto* 2nd selected commit
                string from = revisions[1].ObjectId.ToShortString(); // 1st selected commit (excluded from rebase)

                // TODO:
                string to = ""; // RevisionGrid.CurrentBranch.Value; // current branch checked out (HEAD)

                UICommands.StartRebaseDialog(this, from, to, onto, interactive: false, startRebaseImmediately: false);
            }
            else
            {
                UICommands.StartRebaseDialog(this, onto);
            }
        }

        private void ToolStripButtonPushClick(object sender, EventArgs e)
        {
            PushToolStripMenuItemClick(sender, e);
        }

        private void ManageSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartSubmodulesDialog(this);
            UpdateSubmodulesStructure();
        }

        private void UpdateSubmoduleToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem toolStripMenuItem)
            {
                string submodule = toolStripMenuItem.Tag as string;
                Validates.NotNull(Module.SuperprojectModule);
                FormProcess.ShowDialog(this, UICommands, arguments: Commands.SubmoduleUpdate(submodule), Module.SuperprojectModule.WorkingDir, input: null, useDialogSettings: true);
            }
        }

        private void UpdateAllSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartUpdateSubmodulesDialog(this);
            UpdateSubmodulesStructure();
        }

        private void SynchronizeAllSubmodulesToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartSyncSubmodulesDialog(this);
            UpdateSubmodulesStructure();
        }

        private void ToolStripSplitStashButtonClick(object sender, EventArgs e)
        {
            UICommands.StartStashDialog(this);
            UpdateStashCount();
        }

        private void StashChangesToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
            UpdateStashCount();
        }

        private void StashStagedToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StashStaged(this);
            UpdateStashCount();
        }

        private void StashPopToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StashPop(this);
            UpdateStashCount();
        }

        private void ManageStashesToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartStashDialog(this);
            UpdateStashCount();
        }

        private void CreateStashToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartStashDialog(this, false);
            UpdateStashCount();
        }

        private void PluginSettingsToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartPluginSettingsDialog(this);
        }

        private void RepoSettingsToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartRepoSettingsDialog(this);
        }

        private void CloseToolStripMenuItemClick(object sender, EventArgs e)
        {
            SetWorkingDir("");
        }

        private void CleanupToolStripMenuItemClick(object sender, EventArgs e)
        {
            UICommands.StartCleanupRepositoryDialog(this);
        }

        public void SetWorkingDir(string? path, ObjectId? selectedId = null, ObjectId? firstId = null)
        {
            OpenGitModule(this, new GitModuleEventArgs(new GitModule(path)));
        }

        private void OpenGitModule(object sender, GitModuleEventArgs e)
        {
            PluginRegistry.Unregister(UICommands);
            _submoduleStatusProvider.Init();

            repoObjectsTree.ClearTrees();

            if (!Module.IsValidGitWorkingDir())
            {
                return;
            }

            string path = Module.WorkingDir;
            AppSettings.RecentWorkingDir = path;

#if DEBUG
            // Current encodings
            Debug.WriteLine($"Encodings for {Module.WorkingDir}");
            Debug.WriteLine($"Files content encoding: {Module.FilesEncoding.EncodingName}");
            Debug.WriteLine($"Commit encoding: {Module.CommitEncoding.EncodingName}");
            if (Module.LogOutputEncoding.CodePage != Module.CommitEncoding.CodePage)
            {
                Debug.WriteLine($"Log output encoding: {Module.LogOutputEncoding.EncodingName}");
            }
#endif

            // Reset the filter when switching repos

            // If we're applying custom branch or revision filters - reset them
            ToolStripFilters.ClearQuickFilters();

            SetShortcutKeyDisplayStringsFromHotkeySettings();

            RegisterPlugins();
        }

        private void FileExplorerToolStripMenuItemClick(object sender, EventArgs e)
        {
            OsShellUtil.OpenWithFileExplorer(Module.WorkingDir);
        }

        private void CreateBranchToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO:
            ////            UICommands.StartCreateBranchDialog(this, RevisionGrid.LatestSelectedRevision?.ObjectId);
        }

        private void editGitAttributesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UICommands.StartEditGitAttributesDialog(this);
        }

        private void deleteIndexLockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Module.UnlockIndex(true);
            }
            catch (FileDeleteException ex)
            {
                throw new UserExternalOperationException(_indexLockCantDelete.Text,
                    new ExternalOperationException(arguments: ex.FileName, workingDirectory: Module.WorkingDir, innerException: ex));
            }
        }

        private void BisectClick(object sender, EventArgs e)
        {
            // TODO:
        }

        private void CurrentBranchDropDownOpening(object sender, EventArgs e)
        {
            branchSelect.DropDown.SuspendLayout();
            branchSelect.DropDownItems.Clear();

            AddCheckoutBranchMenuItem();
            branchSelect.DropDownItems.Add(new ToolStripSeparator());
            AddBranchesMenuItems();

            branchSelect.DropDown.ResumeLayout();

            void AddCheckoutBranchMenuItem()
            {
                ToolStripMenuItem checkoutBranchItem = new(checkoutBranchToolStripMenuItem.Text, Images.BranchCheckout)
                {
                    // TODO:
                    // ShortcutKeys = GetShortcutKeys(Command.CheckoutBranch),
                    ShortcutKeyDisplayString = GetShortcutKeyDisplayString(Command.CheckoutBranch)
                };

                branchSelect.DropDownItems.Add(checkoutBranchItem);
                checkoutBranchItem.Click += CheckoutBranchToolStripMenuItemClick;
            }

            void AddBranchesMenuItems()
            {
                foreach (IGitRef branch in GetBranches())
                {
                    Validates.NotNull(branch.ObjectId);
                    bool isBranchVisible = false;

                    ToolStripItem toolStripItem = branchSelect.DropDownItems.Add(branch.Name);
                    toolStripItem.ForeColor = isBranchVisible ? branchSelect.ForeColor : Color.Silver.AdaptTextColor();
                    toolStripItem.Image = isBranchVisible ? Images.Branch : Images.EyeClosed;
                    toolStripItem.Click += (s, e) => UICommands.StartCheckoutBranch(this, toolStripItem.Text);
                    toolStripItem.AdaptImageLightness();
                }

                IEnumerable<IGitRef> GetBranches()
                {
                    // Make sure there are never more than a 100 branches added to the menu
                    // Git Extensions will hang when the drop down is too large...
                    return Module
                        .GetRefs(RefsFilter.Heads)
                        .Take(100);
                }
            }
        }

        private void _forkCloneMenuItem_Click(object sender, EventArgs e)
        {
            if (PluginRegistry.GitHosters.Count > 0)
            {
                UICommands.StartCloneForkFromHoster(this, PluginRegistry.GitHosters[0], OpenGitModule);
            }
            else
            {
                MessageBox.Show(this, _noReposHostPluginLoaded.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void _viewPullRequestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetRepositoryHost(out IRepositoryHostPlugin? repoHost))
            {
                return;
            }

            UICommands.StartPullRequestsDialog(this, repoHost);
        }

        private void _createPullRequestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetRepositoryHost(out IRepositoryHostPlugin? repoHost))
            {
                return;
            }

            UICommands.StartCreatePullRequest(this, repoHost);
        }

        private void _addUpstreamRemoteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!TryGetRepositoryHost(out IRepositoryHostPlugin? repoHost))
            {
                return;
            }

            UICommands.AddUpstreamRemote(this, repoHost);
        }

        private bool TryGetRepositoryHost([NotNullWhen(returnValue: true)] out IRepositoryHostPlugin? repoHost)
        {
            repoHost = PluginRegistry.TryGetGitHosterForModule(Module);
            if (repoHost is null)
            {
                MessageBox.Show(this, _noReposHostFound.Text, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        #region Hotkey commands

        public static readonly string HotkeySettingsName = "Browse";

        internal enum Command
        {
            // Focus or visuals
            FocusLeftPanel = 25,
            FocusRevisionGrid = 3,
            FocusCommitInfo = 4,
            FocusDiff = 5,
            FocusFileTree = 6,
            FocusGpgInfo = 26,
            FocusGitConsole = 29,
            FocusBuildServerStatus = 30,
            FocusOutputHistoryAndToggleIfPanel = 47,
            FocusNextTab = 31,
            FocusPrevTab = 32,

            FocusFilter = 18,

            ToggleLeftPanel = 21,

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
            QuickPullOrFetch = 48, // Default user action configured in toolbar
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

        private void QuickFetch()
        {
            // TODO:
            /*
            bool success = ScriptsRunner.RunEventScripts(ScriptEvent.BeforeFetch, this);
            if (!success)
            {
                return;
            }

            success = FormProcess.ShowDialog(this, UICommands, arguments: Module.FetchCmd(string.Empty, string.Empty, string.Empty), Module.WorkingDir, input: null, useDialogSettings: true);
            if (!success)
            {
                return;
            }

            ScriptsRunner.RunEventScripts(ScriptEvent.AfterFetch, this);
            RefreshRevisions();
            */
        }

        public override bool ProcessHotkey(Keys keyData)
        {
            if (IsDesignMode || !HotkeysEnabled)
            {
                return false;
            }

            // generic handling of this form's hotkeys (upstream)
            if (base.ProcessHotkey(keyData))
            {
                return true;
            }

            return false;
        }

        protected override bool ExecuteCommand(int cmd)
        {
            switch ((Command)cmd)
            {
                case Command.GitBash: userShell.PerformButtonClick(); break;
                case Command.GitGui: Module.RunGui(); break;
                case Command.GitGitK: Module.RunGitK(); break;
                case Command.FocusLeftPanel: FocusLeftPanel(); break;
                case Command.FocusFilter: ToolStripFilters.SetFocus(); break;
                case Command.Commit: UICommands.StartCommitDialog(this); break;
                case Command.CheckoutBranch: UICommands.StartCheckoutBranch(this); break;
                case Command.QuickFetch: QuickFetch(); break;
                case Command.QuickPull: DoPull(pullAction: GitPullAction.Merge, isSilent: true); break;
                case Command.QuickPullOrFetch: toolStripButtonPull.PerformButtonClick(); break;
                case Command.QuickPush: UICommands.StartPushDialog(this, true); break;
                case Command.CloseRepository: SetWorkingDir(""); break;
                case Command.Stash: UICommands.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash); break;
                case Command.StashStaged: UICommands.StashStaged(this); break;
                case Command.StashPop: UICommands.StashPop(this); break;
                case Command.OpenSettings: EditSettings.PerformClick(); break;
                case Command.ToggleLeftPanel: toggleLeftPanel.PerformClick(); break;
                case Command.GoToSuperproject: toolStripButtonLevelUp.PerformClick(); break;
                case Command.GoToSubmodule: toolStripButtonLevelUp.ShowDropDown(); break;
                case Command.PullOrFetch: DoPull(pullAction: AppSettings.FormPullAction, isSilent: false); break;
                case Command.Push: UICommands.StartPushDialog(this, pushOnShow: ModifierKeys.HasFlag(Keys.Shift)); break;
                case Command.MergeBranches: UICommands.StartMergeBranchDialog(this, null); break;
                case Command.Rebase: rebaseToolStripMenuItem.PerformClick(); break;
                default: return base.ExecuteCommand(cmd);
            }

            return true;

            void FocusLeftPanel()
            {
                if (!MainSplitContainer.Panel1Collapsed)
                {
                    repoObjectsTree.Focus();
                }
            }
        }

        #endregion

        private void SetSplitterPositions()
        {
            _splitterManager.AddSplitter(MainSplitContainer, nameof(MainSplitContainer));
            _splitterManager.AddSplitter(LeftSplitContainer, nameof(LeftSplitContainer));

            _splitterManager.RestoreSplitters();
            RefreshLayoutToggleButtonStates();
            if (_isFileHistoryMode)
            {
                MainSplitContainer.Panel1Collapsed = true;
            }

            LeftSplitContainer.Panel2Collapsed = !AppSettings.OutputHistoryPanelVisible.Value;
        }

        private void CommandsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            // Most options do not make sense for artificial commits or no revision selected at all
            IReadOnlyList<GitRevision> selectedRevisions = [];
            bool singleNormalCommit = selectedRevisions.Count == 1 && !selectedRevisions[0].IsArtificial;

            // Some commands like stash, undo commit etc has no relation to selections

            // Require that a single commit is selected
            // Some commands like delete branch could be available for artificial as no default is used,
            // but hide for consistency
            branchToolStripMenuItem.Enabled =
            deleteBranchToolStripMenuItem.Enabled =
            mergeBranchToolStripMenuItem.Enabled =
            checkoutBranchToolStripMenuItem.Enabled =
            cherryPickToolStripMenuItem.Enabled =
            checkoutToolStripMenuItem.Enabled =
            bisectToolStripMenuItem.Enabled =
                singleNormalCommit && !Module.IsBareRepository();

            rebaseToolStripMenuItem.Enabled = selectedRevisions.Count is (1 or 2) && selectedRevisions.All(r => !r.IsArtificial) && !Module.IsBareRepository();

            tagToolStripMenuItem.Enabled =
            deleteTagToolStripMenuItem.Enabled =
            archiveToolStripMenuItem.Enabled =
                singleNormalCommit;

            // Not operating on selected revision
            commitToolStripMenuItem.Enabled =
            undoLastCommitToolStripMenuItem.Enabled =
            runMergetoolToolStripMenuItem.Enabled =
            stashToolStripMenuItem.Enabled =
            resetToolStripMenuItem.Enabled =
            cleanupToolStripMenuItem.Enabled =
            toolStripMenuItemReflog.Enabled =
            applyPatchToolStripMenuItem.Enabled =
                !Module.IsBareRepository();
        }

        private void PullToolStripMenuItemClick(object sender, EventArgs e)
        {
            // "Pull/Fetch..." menu item always opens the dialog
            DoPull(pullAction: AppSettings.FormPullAction, isSilent: false);
        }

        private void ToolStripButtonPullClick(object sender, EventArgs e)
        {
            // Clicking on the Pull button toolbar button will perform the default selected action silently,
            // except if that action is to open the dialog (PullAction.None)
            bool isSilent = AppSettings.DefaultPullAction != GitPullAction.None;
            GitPullAction pullAction = AppSettings.DefaultPullAction != GitPullAction.None ?
                AppSettings.DefaultPullAction : AppSettings.FormPullAction;
            DoPull(pullAction: pullAction, isSilent: isSilent);
        }

        private void pullToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // "Open Pull Dialog..." toolbar menu item always open the dialog with the current default action
            DoPull(pullAction: AppSettings.FormPullAction, isSilent: false);
        }

        private void mergeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoPull(pullAction: GitPullAction.Merge, isSilent: true);
        }

        private void rebaseToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DoPull(pullAction: GitPullAction.Rebase, isSilent: true);
        }

        private void fetchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoPull(pullAction: GitPullAction.Fetch, isSilent: true);
        }

        private void fetchAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoPull(pullAction: GitPullAction.FetchAll, isSilent: true);
        }

        private void fetchPruneAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoPull(pullAction: GitPullAction.FetchPruneAll, isSilent: true);
        }

        private void DoPull(GitPullAction pullAction, bool isSilent)
        {
            if (isSilent)
            {
                UICommands.StartPullDialogAndPullImmediately(this, pullAction: pullAction);
            }
            else
            {
                UICommands.StartPullDialog(this, pullAction: pullAction);
            }
        }

        private void branchSelect_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                CheckoutBranchToolStripMenuItemClick(sender, e);
            }
        }

        private void SubmoduleToolStripButtonClick(object sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem menuSender)
            {
                return;
            }

            string path = menuSender.Tag as string;
            if (!Directory.Exists(path))
            {
                MessageBoxes.SubmoduleDirectoryDoesNotExist(this, path);
                return;
            }

            SetWorkingDir(path);
        }

        #region Submodules

        private ToolStripItem CreateSubmoduleMenuItem(SubmoduleInfo info, string textFormat = "{0}")
        {
            ToolStripMenuItem item = new(string.Format(textFormat, info.Text))
            {
                Width = 200,
                Tag = info.Path,
                Image = Images.FolderSubmodule
            };

            if (info.Bold)
            {
                item.Font = new Font(item.Font, FontStyle.Bold);
            }

            item.Click += SubmoduleToolStripButtonClick;

            return item;
        }

        private static void UpdateSubmoduleMenuItemStatus(ToolStripItem item, SubmoduleInfo info, string textFormat = "{0}")
        {
            if (info.Detailed is not null)
            {
                item.Image = GetSubmoduleItemImage(info.Detailed);
                item.Text = string.Format(textFormat, info.Text + info.Detailed.AddedAndRemovedText);
            }

            return;

            static Image GetSubmoduleItemImage(DetailedSubmoduleInfo details)
            {
                return (details.Status, details.IsDirty) switch
                {
                    (null, _) => Images.FolderSubmodule,
                    (SubmoduleStatus.FastForward, true) => Images.SubmoduleRevisionUpDirty,
                    (SubmoduleStatus.FastForward, false) => Images.SubmoduleRevisionUp,
                    (SubmoduleStatus.Rewind, true) => Images.SubmoduleRevisionDownDirty,
                    (SubmoduleStatus.Rewind, false) => Images.SubmoduleRevisionDown,
                    (SubmoduleStatus.NewerTime, true) => Images.SubmoduleRevisionSemiUpDirty,
                    (SubmoduleStatus.NewerTime, false) => Images.SubmoduleRevisionSemiUp,
                    (SubmoduleStatus.OlderTime, true) => Images.SubmoduleRevisionSemiDownDirty,
                    (SubmoduleStatus.OlderTime, false) => Images.SubmoduleRevisionSemiDown,
                    (_, true) => Images.SubmoduleDirty,
                    (_, false) => Images.FileStatusModified
                };
            }
        }

        private void UpdateSubmodulesStructure()
        {
            // Submodule status is updated on git-status updates. To make sure supermodule status is updated, update immediately (once)
            bool updateStatus = AppSettings.ShowSubmoduleStatus;

            toolStripButtonLevelUp.ToolTipText = "";

            ThreadHelper.FileAndForget(async () =>
            {
                try
                {
                    await _submoduleStatusProvider.UpdateSubmodulesStructureAsync(Module.WorkingDir, TranslatedStrings.NoBranch, updateStatus);
                }
                catch (GitConfigurationException ex)
                {
                    await this.SwitchToMainThreadAsync();
                    MessageBoxes.ShowGitConfigurationExceptionMessage(this, ex);
                }
            });
        }

        private void SubmoduleStatusProvider_StatusUpdating(object sender, EventArgs e)
        {
            this.InvokeAndForget(() =>
            {
                RemoveSubmoduleButtons();
                toolStripButtonLevelUp.DropDownItems.Add(_loading.Text);
            });
        }

        private void SubmoduleStatusProvider_StatusUpdated(object sender, SubmoduleStatusEventArgs e)
        {
            this.InvokeAndForget(() =>
            {
                if (e.StructureUpdated || _currentSubmoduleMenuItems is null)
                {
                    _currentSubmoduleMenuItems = PopulateToolbar(e.Info);
                }

                UpdateSubmoduleMenuStatus(e.Info);
            },
            cancellationToken: e.Token);
        }

        private List<ToolStripItem> PopulateToolbar(SubmoduleInfoResult result)
        {
            // Second task: Populate submodule toolbar menu on UI thread.
            // Suspend before clearing dropdowns to show loading text until updated
            toolStripButtonLevelUp.DropDown.SuspendLayout();
            RemoveSubmoduleButtons();

            List<ToolStripItem> newItems = result.OurSubmodules
                .Select(submodule => CreateSubmoduleMenuItem(submodule))
                .ToList();

            if (result.OurSubmodules.Count == 0)
            {
                newItems.Add(new ToolStripMenuItem(_noSubmodulesPresent.Text));
            }

            if (result.SuperProject is not null)
            {
                newItems.Add(new ToolStripSeparator());

                // Show top project only if it's not our super project
                if (result.TopProject is not null && result.TopProject != result.SuperProject)
                {
                    newItems.Add(CreateSubmoduleMenuItem(result.TopProject, _topProjectModuleFormat.Text));
                }

                newItems.Add(CreateSubmoduleMenuItem(result.SuperProject, _superprojectModuleFormat.Text));
                newItems.AddRange(result.AllSubmodules.Select(submodule => CreateSubmoduleMenuItem(submodule)));
                toolStripButtonLevelUp.ToolTipText = _goToSuperProject.Text;
            }

            newItems.Add(new ToolStripSeparator());

            ToolStripMenuItem mi = new(updateAllSubmodulesToolStripMenuItem.Text, Images.SubmodulesUpdate);
            mi.Click += UpdateAllSubmodulesToolStripMenuItemClick;
            newItems.Add(mi);

            if (result.CurrentSubmoduleName is not null)
            {
                ToolStripMenuItem item = new(_updateCurrentSubmodule.Text)
                {
                    Width = 200,
                    Tag = Module.WorkingDir,
                    Image = Images.FolderSubmodule
                };
                item.Click += UpdateSubmoduleToolStripMenuItemClick;
                newItems.Add(item);
            }

            // Using AddRange is critical: if you used Add to add menu items one at a
            // time, performance would be extremely slow with many submodules (> 100).
            toolStripButtonLevelUp.DropDownItems.AddRange(newItems.ToArray());
            toolStripButtonLevelUp.DropDown.ResumeLayout();

            return newItems;
        }

        private void UpdateSubmoduleMenuStatus(SubmoduleInfoResult result)
        {
            if (_currentSubmoduleMenuItems is null)
            {
                return;
            }

            Validates.NotNull(result.TopProject);
            Dictionary<string, SubmoduleInfo> infos = result.AllSubmodules.ToDictionary(info => info.Path, info => info);
            infos[result.TopProject.Path] = result.TopProject;
            foreach (ToolStripItem item in _currentSubmoduleMenuItems)
            {
                string path = item.Tag as string;
                if (string.IsNullOrWhiteSpace(path))
                {
                    // not a submodule
                    continue;
                }

                if (infos.TryGetValue(path, out SubmoduleInfo? info))
                {
                    UpdateSubmoduleMenuItemStatus(item, info);
                }
                else
                {
                    DebugHelpers.Fail($"Status info for {path} ({1 + result.AllSubmodules.Count} records) has no match in current nodes ({_currentSubmoduleMenuItems.Count})");
                }
            }
        }

        private void RemoveSubmoduleButtons()
        {
            foreach (object item in toolStripButtonLevelUp.DropDownItems)
            {
                if (item is ToolStripMenuItem toolStripButton)
                {
                    toolStripButton.Click -= SubmoduleToolStripButtonClick;
                }
            }

            toolStripButtonLevelUp.DropDownItems.Clear();
        }

        #endregion

        private void toolStripButtonLevelUp_ButtonClick(object sender, EventArgs e)
        {
            if (Module.SuperprojectModule is not null)
            {
                OpenGitModule(this, new GitModuleEventArgs(Module.SuperprojectModule));
            }
            else
            {
                toolStripButtonLevelUp.ShowDropDown();
            }
        }

        private void menuitemSparseWorkingCopy_Click(object sender, EventArgs e)
        {
            UICommands.StartSparseWorkingCopyDialog(this);
        }

        private void toolStripMenuItemReflog_Click(object sender, EventArgs e)
        {
            using FormReflog formReflog = new(UICommands);
            formReflog.ShowDialog();
        }

        #region Layout management

        private void toggleSplitViewLayout_Click(object sender, EventArgs e)
        {
            AppSettings.ShowSplitViewLayout = !AppSettings.ShowSplitViewLayout;
            DiagnosticsClient.TrackEvent("Layout change",
                new Dictionary<string, string> { { nameof(AppSettings.ShowSplitViewLayout), AppSettings.ShowSplitViewLayout.ToString() } });

            RefreshSplitViewLayout();
        }

        private void toggleLeftPanel_Click(object sender, EventArgs e)
        {
            MainSplitContainer.Panel1Collapsed = !MainSplitContainer.Panel1Collapsed;
            DiagnosticsClient.TrackEvent("Layout change",
                new Dictionary<string, string> { { "ShowLeftPanel", MainSplitContainer.Panel1Collapsed.ToString() } });

            RefreshLayoutToggleButtonStates();

            if (!MainSplitContainer.Panel1Collapsed)
            {
                // Refresh the left panel, update visibility of objects separately
                // Get the "main" stash commit, including the reflog selector
                Lazy<IReadOnlyCollection<GitRevision>> getStashRevs = new(() =>
                    !AppSettings.ShowStashes
                    ? Array.Empty<GitRevision>()
                    : new RevisionReader(new GitModule(UICommands.Module.WorkingDir)).GetStashes(CancellationToken.None));

                RefreshLeftPanel(new FilteredGitRefsProvider(UICommands.Module).GetRefs, getStashRevs, forceRefresh: true);
                repoObjectsTree.RefreshRevisionsLoaded();
            }
        }

        private void CommitInfoPositionClick(object sender, EventArgs e)
        {
            if (!menuCommitInfoPosition.DropDownButtonPressed)
            {
                SetCommitInfoPosition((CommitInfoPosition)(
                    ((int)AppSettings.CommitInfoPosition + 1) %
                    Enum.GetValues(typeof(CommitInfoPosition)).Length));
            }
        }

        private void CommitInfoBelowClick(object sender, EventArgs e) =>
            SetCommitInfoPosition(CommitInfoPosition.BelowList);

        private void CommitInfoLeftwardClick(object sender, EventArgs e) =>
            SetCommitInfoPosition(CommitInfoPosition.LeftwardFromList);

        private void CommitInfoRightwardClick(object sender, EventArgs e) =>
            SetCommitInfoPosition(CommitInfoPosition.RightwardFromList);

        private void SetCommitInfoPosition(CommitInfoPosition position)
        {
            AppSettings.CommitInfoPosition = position;
            DiagnosticsClient.TrackEvent("Layout change",
                new Dictionary<string, string> { { nameof(AppSettings.CommitInfoPosition), AppSettings.CommitInfoPosition.ToString() } });

            RefreshLayoutToggleButtonStates();
        }

        private void RefreshSplitViewLayout()
        {
            DiagnosticsClient.TrackEvent("Layout change",
                new Dictionary<string, string> { { nameof(AppSettings.ShowSplitViewLayout), AppSettings.ShowSplitViewLayout.ToString() } });

            RefreshLayoutToggleButtonStates();
        }

        private void RefreshLayoutToggleButtonStates()
        {
            toggleLeftPanel.Checked = !MainSplitContainer.Panel1Collapsed;
            toggleSplitViewLayout.Checked = AppSettings.ShowSplitViewLayout;

            int commitInfoPositionNumber = (int)AppSettings.CommitInfoPosition;
            ToolStripItem selectedMenuItem = menuCommitInfoPosition.DropDownItems[commitInfoPositionNumber];
            menuCommitInfoPosition.Image = selectedMenuItem.Image;
            menuCommitInfoPosition.ToolTipText = selectedMenuItem.Text?.Replace("&", string.Empty);
        }

        #endregion

        private void manageWorktreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using FormManageWorktree formManageWorktree = new(UICommands);
            formManageWorktree.ShowDialog(this);
            if (formManageWorktree.ShouldRefreshRevisionGrid)
            {
                // TODO:
                ////RefreshRevisions();
            }
        }

        private void undoLastCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AppSettings.DontConfirmUndoLastCommit || MessageBox.Show(this, _undoLastCommitText.Text, _undoLastCommitCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                ArgumentString args = Commands.Reset(ResetMode.Soft, "HEAD~1");
                Module.GitExecutable.GetOutput(args);
                refreshToolStripMenuItem.PerformClick();
            }
        }

        internal TestAccessor GetTestAccessor()
            => new(this);

        internal readonly struct TestAccessor
        {
            private readonly RepoBrowser _form;

            public TestAccessor(RepoBrowser form)
            {
                _form = form;
            }

            public RepoObjectsTree RepoObjectsTree => _form.repoObjectsTree;
            public SplitterManager SplitterManager => _form._splitterManager;
            public FilterToolBar ToolStripFilters => _form.ToolStripFilters;
        }

        /* TODO:
        private void HandleDrop(DragEventArgs e)
        {
            if (TreeTabPage.Parent is null)
            {
                return;
            }

            string itemPath = (e.Data.GetData(DataFormats.Text) ?? e.Data.GetData(DataFormats.UnicodeText)) as string;
            if (IsFileExistingInRepo(itemPath))
            {
                CommitInfoTabControl.SelectedTab = TreeTabPage;
                fileTree.SelectFileOrFolder(itemPath);
                return;
            }

            if (e.Data.GetData(DataFormats.FileDrop) is not string[] paths)
            {
                return;
            }

            foreach (string path in paths)
            {
                if (!IsFileExistingInRepo(path))
                {
                    continue;
                }

                if (CommitInfoTabControl.SelectedTab != TreeTabPage)
                {
                    CommitInfoTabControl.SelectedTab = TreeTabPage;
                }

                if (fileTree.SelectFileOrFolder(path))
                {
                    return;
                }
            }

            bool IsPathExists([NotNullWhen(returnValue: true)] string? path) => path is not null && (File.Exists(path) || Directory.Exists(path));

            bool IsFileExistingInRepo([NotNullWhen(returnValue: true)] string? path) => IsPathExists(path) && path.StartsWith(Module.WorkingDir, StringComparison.InvariantCultureIgnoreCase);
        }

        private void FormBrowse_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)
                || e.Data.GetDataPresent(DataFormats.Text)
                || e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private static void FormBrowse_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)
                || e.Data.GetDataPresent(DataFormats.Text)
                || e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                e.Effect = DragDropEffects.Move;
            }
        }
        */
    }
}

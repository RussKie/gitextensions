#pragma warning disable SA1005
#pragma warning disable SA1507
#pragma warning disable SA1515

using System.Diagnostics;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.Infrastructure.Telemetry;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using Microsoft.Win32;

namespace GitUI
{
    public sealed partial class FormShell : GitModuleForm, IBrowseRepo
    {
        public static readonly string HotkeySettingsName = "Browse";

        private readonly DiagnosticsReporter _diagnosticsReporter;
        private readonly BrowseArguments? _browseArguments;
        //private readonly GitStatusMonitor _gitStatusMonitor;
        //private readonly IWindowsJumpListManager _windowsJumpListManager;

        private Dashboard? _dashboard;
        private RepoBrowser? _repoBrowser;

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
        public FormShell(IGitUICommands? commands, BrowseArguments args)
            : base(commands)
        {
            // TODO:
            _browseArguments = args;

            SystemEvents.SessionEnding += (s, e) => SaveApplicationSettings();

            _diagnosticsReporter = new(this);

            InitializeComponent();
            BackColor = OtherColors.BackgroundColor;

            mainMenuStrip.ForeColor = SystemColors.WindowText;
            mainMenuStrip.BackColor = Color.Transparent;

            HotkeysEnabled = true;

            fileToolStripMenuItem.Initialize(commands, () => UICommands);
            helpToolStripMenuItem.Initialize(commands, () => UICommands);
            toolsToolStripMenuItem.Initialize(commands, () => UICommands);

            InitializeComplete();

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await TaskScheduler.Default;
                PluginRegistry.InitializeAll();
            }).FileAndForget();
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

            control.DragDrop += control_DragDrop;
            control.DragEnter += control_DragEnter;

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

            control.DragDrop -= control_DragDrop;
            control.DragEnter -= control_DragEnter;

            pnlContent.Controls.Remove(control);
        }

        private void DashboardClose()
        {
            if (_dashboard is null)
            {
                return;
            }

            _dashboard.GitModuleChanged -= SetGitModule;
            ControlRemove(_dashboard);
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
            }

            Text = UICommands.GetRequiredService<IAppTitleGenerator>().Generate();

            ControlAdd(_dashboard);
            _dashboard.GitModuleChanged += SetGitModule;

            DiagnosticsClient.TrackPageView("Dashboard");
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

        private void InitializeView(IGitModule gitModule)
        {
            // We need to process all the pending messages, and, thus, allow the currently visible controls to finish processing their
            // respective message queues before we dispose these controls.
            BeginInvoke(() =>
            {
                if (_dashboard?.Parent is not null || (_repoBrowser?.Parent is null && gitModule is not null && gitModule.IsValidGitWorkingDir()))
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

                bool bareRepository = Module.IsBareRepository();
                toolsToolStripMenuItem.RefreshState(bareRepository);
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveApplicationSettings();

            foreach (Control control in this.FindDescendants())
            {
                control.DragEnter -= FormShell_DragEnter;
                control.DragDrop -= FormShell_DragDrop;
            }

            base.OnFormClosing(e);
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

            _diagnosticsReporter.Report();
        }

        private void RepositoryClose()
        {
            if (_repoBrowser is null)
            {
                return;
            }

            _repoBrowser.TextChanged -= repoBrowser_TextChanged;

            ControlRemove(_repoBrowser);
            UICommands = new GitUICommands(UICommands, new GitModule(workingDir: null));
        }

        private void RepositoryOpen(IGitModule gitModule)
        {
            if (_repoBrowser is null)
            {
                _repoBrowser = new(serviceProvider: UICommands, _browseArguments)
                {
                    BorderStyle = BorderStyle.FixedSingle,
                    BackColor = Color.AliceBlue,

                    Dock = DockStyle.Fill,
                    Visible = false
                };

                _repoBrowser.TextChanged += repoBrowser_TextChanged;
            }

            ControlAdd(_repoBrowser);

            _repoBrowser.Visible = true;

            DiagnosticsClient.TrackPageView("Revision graph");

            RepositorySwitch(gitModule);
        }

        private void RepositorySwitch(IGitModule gitModule)
        {
            Debug.Assert(_repoBrowser is not null, "Boo!");

            // TODO: this likely must be executed at the beginning of InitializeView.

            string originalWorkingDir = Module.WorkingDir;
            if (string.Equals(originalWorkingDir, gitModule.WorkingDir, StringComparison.Ordinal))
            {
                return;
            }

            UICommands = new GitUICommands(UICommands, gitModule);
        }

        private static void SaveApplicationSettings()
        {
            AppSettings.SaveSettings();
        }

        private void SetGitModule(object? sender, GitModuleEventArgs e) => InitializeView(e.GitModule);

        GitRevision? IBrowseRepo.GetLatestSelectedRevision()
        {
            throw new NotImplementedException();
        }

        Point IBrowseRepo.GetQuickItemSelectorLocation()
        {
            throw new NotImplementedException();
        }

        IReadOnlyList<GitRevision> IBrowseRepo.GetSelectedRevisions()
        {
            throw new NotImplementedException();
        }

        void IBrowseRepo.GoToRef(string refName, bool showNoRevisionMsg, bool toggleSelection)
        {
            throw new NotImplementedException();
        }

        void IBrowseRepo.SetWorkingDir(string? path, ObjectId? selectedId, ObjectId? firstId)
        {
            InitializeView(new GitModule(path));
        }

        private void control_DragDrop(object sender, DragEventArgs e)
        {
            // TODO:
        }

        private void control_DragEnter(object sender, DragEventArgs e)
        {
            // TODO:
        }

        private void FormShell_DragDrop(object sender, DragEventArgs e)
        {
            // TODO: HandleDrop(e);
        }

        private void FormShell_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)
                || e.Data.GetDataPresent(DataFormats.Text)
                || e.Data.GetDataPresent(DataFormats.UnicodeText))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void fileToolStripMenuItem_RecentRepositoriesCleared(object sender, EventArgs e)
        {
            _dashboard?.RefreshContent();
        }

        private void repoBrowser_TextChanged(object? sender, EventArgs e)
        {
            Text = _repoBrowser.Text;
        }

        private void toolsToolStripMenuItem_SettingsChanged(object sender, CommandsDialogs.Menus.SettingsChangedEventArgs e)
        {
            _dashboard?.RefreshContent();

            helpToolStripMenuItem.RefreshShortcutKeys(Hotkeys);
            toolsToolStripMenuItem.RefreshShortcutKeys(Hotkeys);
        }
    }
}
#pragma warning restore SA1515
#pragma warning restore SA1507
#pragma warning restore SA1005

using System.Runtime.InteropServices;
using GitExtUtils.GitUI;
using GitUI.Interops.DwmApi;
using GitUI.Theming;
using ResourceManager;

namespace GitUI
{
    // NOTE do not make this class abstract as it breaks the WinForms designer in VS

    /// <summary>Base class for a Git Extensions <see cref="Form"/>.</summary>
    /// <remarks>Includes support for font, hotkey, icon, translation, and position restore.</remarks>
    public class GitExtensionsForm : GitExtensionsFormBase
    {
        private IWindowPositionManager _windowPositionManager = new WindowPositionManager();
        private Func<IReadOnlyList<Rectangle>> _getScreensWorkingArea = () => Screen.AllScreens.Select(screen => screen.WorkingArea).ToArray();
        private bool _needsPositionRestore;

        /// <summary>Creates a new <see cref="GitExtensionsForm"/> without position restore.</summary>
        public GitExtensionsForm()
            : this(enablePositionRestore: false)
        {
        }

        /// <summary>Creates a new <see cref="GitExtensionsForm"/> indicating position restore.</summary>
        /// <param name="enablePositionRestore">Indicates whether the <see cref="Form"/>'s position
        /// will be restored upon being re-opened.</param>
        protected GitExtensionsForm(bool enablePositionRestore)
        {
            bool needsPositionSave = enablePositionRestore;
            _needsPositionRestore = enablePositionRestore;

            FormClosing += GitExtensionsForm_FormClosing;

            Button cancelButton = new();
            cancelButton.Click += CancelButtonClick;
            CancelButton = cancelButton;

            if (ThemeModule.IsDarkTheme)
            {
                // Warning: This call freezes the CI in AppVeyor, however dark theme is not used on build machines
                DwmApi.UseImmersiveDarkMode(Handle, true);
            }

            void GitExtensionsForm_FormClosing(object sender, FormClosingEventArgs e)
            {
                if (!needsPositionSave)
                {
                    return;
                }

                needsPositionSave = false;
                _windowPositionManager.SavePosition(this);
                TaskbarProgress.Clear();
            }
        }

        public virtual void CancelButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            RestorePosition();

            // Should be called after restoring position
            base.OnLoad(e);

            if (!IsDesignMode)
            {
                OnRuntimeLoad(e);
            }
        }

        /// <summary>Invoked at runtime during the <see cref="OnLoad"/> method.</summary>
        /// <remarks>In particular, this method is not invoked when running in a designer.</remarks>
        protected virtual void OnRuntimeLoad(EventArgs e)
        {
        }

#pragma warning disable SA1305 // Field names should not use Hungarian notation
        /// <summary>
        /// Retrieves a handle to the display monitor that contains a specified point.
        /// </summary>
        /// <param name="pt"> Specifies the point of interest in virtual-screen coordinates. </param>
        /// <param name="dwFlags"> Determines the function's return value if the point is not contained within any display monitor. </param>
        /// <returns> If the point is contained by a display monitor, the return value is an HMONITOR handle to that display monitor. </returns>
        /// <remarks>
        /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-monitorfrompoint"/>
        /// </remarks>
        [DllImport("User32.dll")]
        internal static extern nint MonitorFromPoint(Point pt, uint dwFlags);

        /// <summary>
        /// Queries the dots per inch (dpi) of a display.
        /// </summary>
        /// <param name="hmonitor"> Handle of the monitor being queried. </param>
        /// <param name="dpiType"> The type of DPI being queried. </param>
        /// <param name="dpiX"> The value of the DPI along the X axis. </param>
        /// <param name="dpiY"> The value of the DPI along the Y axis. </param>
        /// <returns> Status success </returns>
        /// <remarks>
        /// <see href="https://learn.microsoft.com/en-us/windows/win32/api/shellscalingapi/nf-shellscalingapi-getdpiformonitor"/>
        /// </remarks>
        [DllImport("Shcore.dll")]
        private static extern nint GetDpiForMonitor(nint hmonitor, /*DpiType*/ int dpiType, out uint dpiX, out uint dpiY);
#pragma warning restore SA1305 // Field names should not use Hungarian notation

        /// <summary>
        ///   Restores the position of a form from the user settings. Does
        ///   nothing if there is no entry for the form in the settings, or the
        ///   setting would be invisible on the current display configuration.
        /// </summary>
        protected virtual void RestorePosition()
        {
            if (!_needsPositionRestore)
            {
                return;
            }

            if (WindowState == FormWindowState.Minimized)
            {
                // TODO: do we still need to assert when restored it is shown on the correct monitor?
                return;
            }

            WindowPosition position = _windowPositionManager.LoadPosition(this);
            if (position is null)
            {
                return;
            }

            _needsPositionRestore = false;

            IReadOnlyList<Rectangle> workingArea = _getScreensWorkingArea();
            if (!workingArea.Any(screen => screen.IntersectsWith(position.Rect)))
            {
                if (position.State == FormWindowState.Maximized)
                {
                    WindowState = FormWindowState.Maximized;
                }

                return;
            }

            SuspendLayout();

            bool windowCentred = StartPosition == FormStartPosition.CenterParent;
            StartPosition = FormStartPosition.Manual;

#pragma warning disable SA1305 // Field names should not use Hungarian notation
            uint dpiX;
            uint dpiY;
            /*HMONITOR*/
            nint hMonitor = MonitorFromPoint(position.Rect.Location, 0); // MONITOR_DEFAULTTONEAREST
            nint result = GetDpiForMonitor(hMonitor, /*DpiType.Effective*/0, out dpiX, out dpiY);
#pragma warning restore SA1305 // Field names should not use Hungarian notation

            if (FormBorderStyle == FormBorderStyle.Sizable ||
                FormBorderStyle == FormBorderStyle.SizableToolWindow)
            {
                Size = DpiUtil.Scale(position.Rect.Size, originalDpi: (int)dpiX);
            }

            if (Owner is null || !windowCentred)
            {
                Point calculatedLocation = DpiUtil.Scale(position.Rect.Location, originalDpi: position.DeviceDpi);

                DesktopLocation = WindowPositionManager.FitWindowOnScreen(new Rectangle(calculatedLocation, Size), workingArea);
            }
            else
            {
                // Calculate location for modal form with parent
                Point calculatedLocation = new(
                    Owner.Left + (Owner.Width / 2) - (Width / 2),
                    Owner.Top + (Owner.Height / 2) - (Height / 2));
                Location = WindowPositionManager.FitWindowOnScreen(new Rectangle(calculatedLocation, Size), workingArea);
            }

            if (WindowState != position.State)
            {
                WindowState = position.State;
            }

            ResumeLayout();
        }

        // This is a base class for many forms, which have own GetTestAccessor() methods. This has to be unique
        internal GitExtensionsFormTestAccessor GetGitExtensionsFormTestAccessor() => new(this);

        internal readonly struct GitExtensionsFormTestAccessor
        {
            private readonly GitExtensionsForm _form;

            public GitExtensionsFormTestAccessor(GitExtensionsForm form)
            {
                _form = form;
            }

            public IWindowPositionManager WindowPositionManager
            {
                get => _form._windowPositionManager;
                set => _form._windowPositionManager = value;
            }

            public Func<IReadOnlyList<Rectangle>> GetScreensWorkingArea
            {
                get => _form._getScreensWorkingArea;
                set => _form._getScreensWorkingArea = value;
            }
        }
    }
}

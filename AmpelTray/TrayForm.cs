using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using AmpelTray.Properties;

namespace AmpelTray
{
    public delegate void ToggleIconDelegate(IconStatus iconStatus);

    public enum IconStatus  { Enabled, Disabled }

    public class TrayForm : Form
    {
        [STAThread]
        public static void Main()
        {
            Application.Run(new TrayForm());
        }

        private readonly NotifyIcon _trayIcon;
        private readonly ContextMenu _trayMenu;
        private readonly Icon _iconEnabled = new Icon("ampel.ico");
        private readonly Icon _iconDisabled = new Icon("ampel_disabled.ico");
        private readonly AmpelLoopWorker _ampelLoopWorker;
        private readonly Thread _workerThread;
        private readonly Dictionary<IconStatus, Icon> _iconStatusMap;

        public TrayForm()
        {
            _ampelLoopWorker = new AmpelLoopWorker(ToggleIcon);
            _workerThread = new Thread(_ampelLoopWorker.DoAmpelLoop);
            _iconStatusMap = new Dictionary<IconStatus, Icon> {
                                                                    {IconStatus.Enabled, _iconEnabled},
                                                                    {IconStatus.Disabled, _iconDisabled},
                                                              };
            _trayMenu = new ContextMenu();
            _trayMenu.MenuItems.Add("Exit", OnExit);

            _trayIcon = new NotifyIcon
                           {
                               Text = Resources.TrayForm_TrayForm_AmpelTray,
                               Icon = _iconEnabled,
                               ContextMenu = _trayMenu,
                               Visible = true
                           };

        }

        public void ToggleIcon(IconStatus iconStatus)
        {
            _trayIcon.Icon = _iconStatusMap[iconStatus];
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);

            _workerThread.Start();

        }

        private void OnExit(object sender, EventArgs e)
        {
            _ampelLoopWorker.RequestStop();
            _workerThread.Join();
            Application.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                // Release the icon resource.
                _trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}

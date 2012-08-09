using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using AmpelLib;
using AmpelTray.Properties;

namespace AmpelTray
{
    public class Worker
    {
        private readonly ToggleIconDelegate _toggleIconDelegate;
        
        public Worker(ToggleIconDelegate toggleIconDelegate)
        {
            _toggleIconDelegate = toggleIconDelegate;
        }
        
        public void AmpelLoop()
        {
            var ag = new Ampel();
            var fb = new FBGateway();
            var ampelService = new AmpelService();

            while (!_shouldStop)
            {
                try
                {
                    ampelService.ToggleAmpel(ag, fb.GetAllProjects().FindAll(x => x.Group.Contains(ConfigurationManager.AppSettings.Get("ProjectsMask"))));
                    _toggleIconDelegate(IconStatus.Enabled);
                }
                catch
                {
                    _toggleIconDelegate(IconStatus.Disabled);
                    ag.Off();
                }

                Thread.Sleep(5000);
                GC.Collect();
            }
            ag.Off();
            Console.WriteLine(Resources.Worker_DoWork_worker_thread__terminating_gracefully_);
        }

        public void RequestStop()
        {
            _shouldStop = true;
        }

        private volatile bool _shouldStop;
    }

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
        private readonly Worker _workerObj;
        private readonly Thread _workerThread;
        private readonly Dictionary<IconStatus, Icon> _iconStatusMap;

        public TrayForm()
        {
            _workerObj = new Worker(ToggleIcon);
            _workerThread = new Thread(_workerObj.AmpelLoop);
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
            _workerObj.RequestStop();
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

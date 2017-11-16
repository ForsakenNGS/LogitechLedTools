using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;

namespace LogitechLedTools
{
    class TrayApp : Form
    {

        public static void Main()
        {
            Application.Run(new TrayApp());
        }

        private WebinterfaceHttp    webinterface;
        private EventLog            eventlog;

        private Assembly _assembly;
        private NotifyIcon  trayIcon;
        private ContextMenu trayMenu;
        private MenuItem    trayMenuActive;
        private MenuItem    trayMenuExit;

        public TrayApp()
        {
            _assembly = Assembly.GetExecutingAssembly();
            // Init native classes
            WebinterfaceNative.Init();
            // Init Eventlog
            eventlog = new EventLog();
            eventlog.Source = "LogitechLedTools";
            // Init webinterface
            webinterface = new WebinterfaceHttp(eventlog);
            webinterface.Start();
            // Init context menu
            trayMenu = new ContextMenu();
            trayMenuActive = trayMenu.MenuItems.Add("Active", OnToggleActive);
            trayMenuActive.Checked = true;
            trayMenuExit = trayMenu.MenuItems.Add("Exit", OnExit);
            // Init icon
            trayIcon = new NotifyIcon();
            trayIcon.Text = "LogitechLedTools";
            trayIcon.Icon = new Icon( _assembly.GetManifestResourceStream("LogitechLedTools.tray.ico"));
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
            trayIcon.DoubleClick += OnOpen;
        }

        private void OnToggleActive(object sender, EventArgs e)
        {
            var active = !trayMenuActive.Checked;
            if (active)
            {
                webinterface.Start();
            } else
            {
                webinterface.Stop();
            }
            trayMenuActive.Checked = active;
        }

        private void OnOpen(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://localhost:8042");
        }

        private void OnExit(object sender, EventArgs e)
        {
            if (webinterface.IsActive())
            {
                // Stop the webinterface
                webinterface.Stop();
            }
            Application.Exit();
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;        // Hide form window.
            ShowInTaskbar = false;  // Remove from taskbar.

            base.OnLoad(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (webinterface.IsActive())
            {
                // Stop the webinterface
                webinterface.Stop();
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

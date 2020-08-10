using System;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Windows.Networking.Connectivity;
using Windows.Networking.NetworkOperators;

namespace Hotspot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static NotifyIcon notifyIcon;
        public static ToolStripMenuItem enable = new ToolStripMenuItem("Enable");
        public static ToolStripMenuItem disable = new ToolStripMenuItem("Disable");
        public static ToolStripMenuItem exit = new ToolStripMenuItem("Exit");
        public static EventLogQuery eventLogQuery = new EventLogQuery("Microsoft-Windows-WLAN-AutoConfig/Operational", PathType.LogName, "*[System/EventID=8003]");
        public static EventLogWatcher eventLogWatcher = new EventLogWatcher(eventLogQuery);
        public static ConnectionProfile connectionProfile;
        public static NetworkOperatorTetheringManager networkOperatorTetheringManager;

        public MainWindow()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
            enable.Click += new EventHandler(Enable);
            disable.Click += new EventHandler(Disable);
            exit.Click += new EventHandler(Exit);
            notifyIcon = new NotifyIcon
            {
                Text = "Hotspot",
                Visible = true,
                Icon = Properties.Resources.wlanpref_dll_10408_,
                ContextMenuStrip = new ContextMenuStrip()
            };
            notifyIcon.ContextMenuStrip.Items.Add(enable);
            notifyIcon.ContextMenuStrip.Items.Add(exit);
            connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            while (connectionProfile == null)
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("There is no network connection now, please connect to the network first.\nClick Cancel to exit.\n\n当前暂无网络连接，请先连接一次网络。\n点击取消退出。\n", "No Connection", MessageBoxButton.OKCancel);
                if (messageBoxResult == MessageBoxResult.Cancel)
                {
                    Environment.Exit(0);
                }
                connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            }
            networkOperatorTetheringManager = NetworkOperatorTetheringManager.CreateFromConnectionProfile(connectionProfile);
            eventLogWatcher.EventRecordWritten += new EventHandler<EventRecordWrittenEventArgs>(HotspotRecover);
        }

        public static void HotspotRecover(object obj, EventRecordWrittenEventArgs arg)
        {
            Thread.Sleep(3000);
            Task.FromResult(networkOperatorTetheringManager.StartTetheringAsync());
        }

        public void Enable(object sender, EventArgs e)
        {
            eventLogWatcher.Enabled = true;
            notifyIcon.ContextMenuStrip.Items.Remove(enable);
            notifyIcon.ContextMenuStrip.Items.Insert(0, disable);
        }

        public void Disable(object sender, EventArgs e)
        {
            eventLogWatcher.Enabled = false;
            notifyIcon.ContextMenuStrip.Items.Remove(disable);
            notifyIcon.ContextMenuStrip.Items.Insert(0, enable);
        }

        public void Exit(object sender, EventArgs e)
        {
            Close();
        }
    }
}

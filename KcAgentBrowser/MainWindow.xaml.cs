using Fiddler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KcAgentBrowser
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string URL_LOGIN = "http://www.dmm.com/netgame/social/application/-/detail/=/app_id=854854/";
        private int PROXY_PORT = 19988;

        /// <summary>
        /// ウィンドウを開くとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FiddlerApplication.Startup(PROXY_PORT, false, true);
            // SetIESettings("localhost:" + PROXY_PORT);
            URLMonInterop.SetProxyInProcess("127.0.0.1:" + PROXY_PORT.ToString(), "<-loopback>");
            FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
            this.wb.Navigate(URL_LOGIN);
        }

        void FiddlerApplication_AfterSessionComplete(Session oSession)
        {
            string url = oSession.fullUrl;
            Debug.WriteLine(url);
            if (_win != null)
            {
                var item = new JsonData {URL = url, JSON = oSession.GetResponseBodyAsString() };
                _win.lst.Dispatcher.Invoke(
                    new Action(() => _win.lst.Items.Add(item))); 
            }
        }

        /// <summary>
        /// ウィンドウを閉じるとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            FiddlerApplication.Shutdown();
        }

        internal struct INTERNET_PROXY_INFO
        {
            public int dwAccessType;
            public IntPtr proxy;
            public IntPtr proxyBypass;
        }
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);
        
        private void SetIESettings(string proxyUri)
        {
            const int INTERNET_OPTION_PROXY = 38;
            const int INTERNET_OPEN_TYPE_PROXY = 3;

            INTERNET_PROXY_INFO proxyInfo;
            proxyInfo.dwAccessType = INTERNET_OPEN_TYPE_PROXY;
            proxyInfo.proxy = Marshal.StringToHGlobalAnsi(proxyUri);
            proxyInfo.proxyBypass = Marshal.StringToHGlobalAnsi("local");

            var proxyInfoSize = Marshal.SizeOf(proxyInfo);
            var proxyInfoPtr = Marshal.AllocCoTaskMem(proxyInfoSize);
            Marshal.StructureToPtr(proxyInfo, proxyInfoPtr, true);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_PROXY, proxyInfoPtr, proxyInfoSize);
        }

        DebugWindow _win;
        /// <summary>
        /// F12キーの時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F12)
            {
                _win = new DebugWindow();
                _win.Show();
            }
        }
    }

    public class JsonData
    {
        public string URL { get; set; }
        public string JSON { get; set; }
        public override string ToString()
        {
            return this.URL;
        }
        public string GetData()
        {
            return this.URL + "\n" + this.JSON;
        }
    }
}

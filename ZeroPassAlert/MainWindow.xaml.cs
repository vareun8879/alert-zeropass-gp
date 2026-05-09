using System;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using ZeroPassAlert.Enum;
using ZeroPassAlert.Models;
using ZeroPassAlert.Overlays;
using ZeroPassAlert.Utils;
using ZeroPassAlert.Views;

namespace ZeroPassAlert
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private static DateTime _lastSoundTime = DateTime.MinValue;
        private static MediaPlayer _player = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();

            AppGlobal.BaseUrl = ConfigurationManager.AppSettings["BASE_URL"];
            AppGlobal.APIUrl = AppGlobal.BaseUrl + AppGlobal.AlertUrl;

            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AuthUtil.SetAuthInfo("0000");
            AppGlobal.CorpCode = ConfigurationManager.AppSettings["CORP_CODE"];

            AppGlobal.GuardId = ConfigurationManager.AppSettings["GUARD_ID"];

            ShowHome();
        }

        public void ShowHome()
        {
            AppGlobal.CurrentViewType = ViewType.Home;
            mainContent.Content = new AlertView(this);
        }
        public void ShowAlertOverlay(VisitorEventVO evt)
        {
            var overlay = new AlertOverlay(evt);

            if (mainContent.Content is AlertView view)
            {
                view.BodyGrid.Children.Add(overlay);
            }

            PlayAlert();
        }

        public void PlayAlert()
        {
            if ((DateTime.Now - _lastSoundTime).TotalMilliseconds < 800)
                return; // 0.8초 내 중복 재생 방지

            _lastSoundTime = DateTime.Now;

            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", "notification.mp3");
                _player.Open(new Uri(path));
                _player.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Sound Error] " + ex.Message);
            }
        }

        /// <summary>
        /// App.xaml 의 ShutdownMode="OnExplicitShutdown" 설정상 메인 윈도우만 닫혀도
        /// 앱은 자동 종료되지 않아 App.OnExit 가 호출되지 않는다.
        /// → 메인 윈도우가 닫히면 명시적으로 Shutdown 을 호출해 OnExit → SafeUnregister 흐름을 태운다.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }

}

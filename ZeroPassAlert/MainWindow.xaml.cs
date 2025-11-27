using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
using ZeroPassAlert.Enum;
using ZeroPassAlert.LongPolling;
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
        public MainWindow()
        {
            InitializeComponent();

            AppGlobal.BaseUrl = ConfigurationManager.AppSettings["BASE_URL"];
            AppGlobal.APIUrl = AppGlobal.BaseUrl + AppGlobal.KioskUrl;

            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AuthUtil.SetAuthInfo("0000");
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
        }
    }

}

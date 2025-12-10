using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
using ZeroPassAlert.LongPolling;
using ZeroPassAlert.Utils;

namespace ZeroPassAlert.Views
{
    /// <summary>
    /// AlertView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AlertView : UserControl
    {
        public AlertView() : this(null) { }

        public AlertView(MainWindow parent)
        {
            InitializeComponent();
        }
        private async void AlertView_Loaded(object sender, RoutedEventArgs e)
        {
            ResponseResult response = await RestUtil.GetDataAsync("visitor/alert/today/" + AppGlobal.CorpCode, null);

            // 방문자 수 최초 로딩
            if (response.ExceptionMessage != null)
            {
                AppAlert.Instance.TodayVisitorCount = 0;
                AppAlert.Instance.TodayDateText = "";
            }
            else
            {
                dynamic jsonObject = JsonConvert.DeserializeObject<IDictionary<string, object>>(response.Data);
                AppAlert.Instance.TodayVisitorCount = jsonObject["count"];

                AppAlert.Instance.TodayDateText = DateTime.Parse(jsonObject["todayDate"]).ToString("yyyy년 MM월 dd일");
            }


            LongPollingManager.Initialize(Application.Current);
            _ = Task.Run(() => LongPollingManager.Start());
        }
    }
}

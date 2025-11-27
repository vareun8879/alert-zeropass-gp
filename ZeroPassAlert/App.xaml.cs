using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ZeroPassAlert;
using ZeroPassAlert.Utils;

namespace ZeroPassAlert
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    /// // splash를 static 필드로 선언
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            await InitApplication();
        }

        private async Task InitApplication()
        {
            AppGlobal.BaseUrl = ConfigurationManager.AppSettings["BASE_URL"];
            AppGlobal.APIUrl = AppGlobal.BaseUrl + AppGlobal.KioskUrl;

            //mainWindow = new MainWindow();
            //Application.Current.MainWindow = mainWindow;
            //mainWindow.Show();
        }
    }
}

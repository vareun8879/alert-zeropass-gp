using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ZeroPassAlert.Sse;
using ZeroPassAlert.Utils;

namespace ZeroPassAlert.Views
{
    /// <summary>
    /// AlertView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AlertView : UserControl
    {
        private readonly MainWindow _parent;

        public AlertView() : this(null) { }

        public AlertView(MainWindow parent)
        {
            _parent = parent;
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _parent?.Close();
        }
        private async void AlertView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 같은 corpCode/guardId 로 이미 사용 중인 단말이 있는지 백엔드에 확인.
                // 있으면 중복 실행이므로 안내 후 즉시 종료 (SSE 핑퐁 무한 루프 차단).
                if (await IsDuplicateSession())
                {
                    MessageBox.Show(
                        $"이미 다른 곳에서 사용 중인 단말 ID({AppGlobal.GuardId}) 입니다.\n프로그램을 종료합니다.",
                        "제로패스 알림 서비스",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    Application.Current.Shutdown();
                    return;
                }

                ResponseResult response = await RestUtil.GetDataAsync("visitor/today/" + AppGlobal.CorpCode, null);

                // 방문자 수 최초 로딩
                if (response == null || response.ExceptionMessage != null || string.IsNullOrEmpty(response.Data))
                {
                    AppAlert.Instance.TodayVisitorCount = 0;
                    AppAlert.Instance.TodayDateText = "";
                }
                else
                {
                    try
                    {
                        var jsonObject = JsonConvert.DeserializeObject<IDictionary<string, object>>(response.Data);
                        if (jsonObject != null)
                        {
                            if (jsonObject.TryGetValue("count", out var countObj) && countObj != null)
                                AppAlert.Instance.TodayVisitorCount = Convert.ToInt64(countObj);

                            if (jsonObject.TryGetValue("todayDate", out var dateObj) && dateObj != null
                                && DateTime.TryParse(dateObj.ToString(), out var parsedDate))
                                AppAlert.Instance.TodayDateText = parsedDate.ToString("yyyy년 MM월 dd일");
                            else
                                AppAlert.Instance.TodayDateText = "";
                        }
                    }
                    catch (Exception parseEx)
                    {
                        Debug.WriteLine($"[AlertView 초기 로딩 파싱 오류] {parseEx.Message}");
                        AppAlert.Instance.TodayVisitorCount = 0;
                        AppAlert.Instance.TodayDateText = "";
                    }
                }

                SseManager.Initialize(Application.Current);
                SseManager.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AlertView_Loaded 오류] {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static async Task<bool> IsDuplicateSession()
        {
            try
            {
                ResponseResult check = await RestUtil.GetDataAsync(
                    "visitor/events/check/" + AppGlobal.CorpCode + "/" + AppGlobal.GuardId, null);

                if (check == null || string.IsNullOrEmpty(check.Data)) return false;

                var json = JsonConvert.DeserializeObject<IDictionary<string, object>>(check.Data);
                if (json != null && json.TryGetValue("exists", out var existsObj) && existsObj != null)
                {
                    return Convert.ToBoolean(existsObj);
                }
                return false;
            }
            catch (Exception ex)
            {
                // 체크 자체가 실패하면(서버 다운/네트워크 오류) 정상 진입을 허용.
                // SSE 연결 시점에 어차피 다시 실패하므로 별도 처리 불필요.
                Debug.WriteLine($"[중복 체크 오류] {ex.Message}");
                return false;
            }
        }
    }
}

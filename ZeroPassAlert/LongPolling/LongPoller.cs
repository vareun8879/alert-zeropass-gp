using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ZeroPassAlert.Utils;

namespace ZeroPassAlert.LongPolling
{
    public class LongPoller
    {
        private bool _isRunning;
        private readonly Application _app;

        // 서버 부하 조절을 위한 대기 시간 설정 (밀리초)
        private const int DELAY_ON_SUCCESS = 100;   // 데이터 수신 후 아주 잠깐 대기 (연속 처리 부하 방지)
        private const int DELAY_ON_EMPTY = 100;    // 데이터가 없을 때 대기 (3초)
        private const int DELAY_ON_ERROR = 100;    // 에러 발생 시 대기 (5초)

        public LongPoller(Application app)
        {
            _app = app;
        }

        public async Task Start()
        {
            if (_isRunning) return;
            _isRunning = true;

            while (true) // 혹은 _isRunning 체크
            {
                try
                {
                    var url = $"visitor/alert/{AppGlobal.CorpCode}";

                    // 서버에 요청 (서버쪽에서 타임아웃까지 대기하다 응답하는 구조여야 함)
                    ResponseResult response = await RestUtil.GetDataAsync(url, null);

                    // 1. 응답 데이터가 비어있는 경우 (타임아웃 등)
                    if (response.Data == null)
                    {
                        await Task.Delay(DELAY_ON_EMPTY);
                        continue;
                    }

                    var evt = JsonConvert.DeserializeObject<VisitorEventVO>(response.Data);

                    // 2. 파싱 실패 혹은 유효하지 않은 데이터
                    if (evt == null)
                    {
                        await Task.Delay(DELAY_ON_EMPTY);
                        continue;
                    }

                    // 정상 데이터 처리
                    AppAlert.Instance.TodayVisitorCount = evt.Count;
                    AppAlert.Instance.TodayDateText = evt.TodayDate.ToString("yyyy년 MM월 dd일");

                    _app.Dispatcher.Invoke(() =>
                    {
                        var main = Application.Current.MainWindow as MainWindow;
                        main?.ShowAlertOverlay(evt);
                    });

                    var ackUrl = $"visitor/alert";
                    Hashtable ack = new Hashtable();
                    ack.Add("corpCode", AppGlobal.CorpCode);
                    // ack.Add("eventId", null);
                    await RestUtil.PostDataAsync(ackUrl, ack);

                    // 3. 성공 후에도 너무 빠르게 재요청하지 않도록 잠깐 대기
                    await Task.Delay(DELAY_ON_SUCCESS);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Poller Error] {ex.Message}");
                    // 4. 에러 발생 시 잠깐 대기
                    await Task.Delay(DELAY_ON_ERROR);
                }
            }
        }
    }

}

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ZeroPassAlert.Models;
using ZeroPassAlert.Utils;

namespace ZeroPassAlert.Sse
{
    public class SseClient
    {
        private readonly Application _app;

        // SSE 전용 HttpClient — 연결을 장시간 유지해야 하므로 타임아웃 무한
        private static readonly HttpClient _sseHttp = new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        private const int RECONNECT_DELAY_MS = 5_000; // 재연결 대기 5초

        public SseClient(Application app)
        {
            _app = app;
        }

        public async Task Start(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await ConnectAndListen(ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SSE] 연결 오류, {RECONNECT_DELAY_MS / 1000}초 후 재연결: {ex.Message}");
                    await Task.Delay(RECONNECT_DELAY_MS, ct);
                }
            }
        }

        private async Task ConnectAndListen(CancellationToken ct)
        {
            Hashtable param = RestUtil.CreateCommonParams();
            string url = AppGlobal.APIUrl + "visitor/events/stream/" + AppGlobal.CorpCode + "/" + AppGlobal.GuardId;

            using (var req = new HttpRequestMessage(HttpMethod.Get, url))
            {
                RestUtil.AddCommonHeaders(req);
                req.Headers.Accept.ParseAdd("text/event-stream");
                req.Headers.Accept.ParseAdd("application/json");
                req.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                using (var res = await _sseHttp.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct))
                {
                    res.EnsureSuccessStatusCode();
                    Debug.WriteLine("[SSE] 서버 연결 성공");

                    using (var stream = await res.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        string eventName = null;
                        var dataBuffer = new StringBuilder();

                        while (!ct.IsCancellationRequested)
                        {
                            // 서버가 30초마다 Heartbeat를 보내므로, 65초 동안 아무 소식이 없으면 끊긴 것으로 간주
                            using (var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(65)))
                            // 외부 취소(ct)와 타임아웃(timeoutCts) 중 하나라도 발생하면 취소되도록 연결
                            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token))
                            {
                                try
                                {
                                    var readTask = reader.ReadLineAsync();
                                    var timeoutTask = Task.Delay(Timeout.Infinite, linkedCts.Token);

                                    // ReadLineAsync와 CancellationToken을 연동하기 위한 Task.WhenAny 패턴
                                    var completedTask = await Task.WhenAny(readTask, timeoutTask);

                                    if (completedTask == timeoutTask)
                                    {
                                        // 타임아웃 발생! linkedCts.Token에 의해 취소됨
                                        linkedCts.Token.ThrowIfCancellationRequested();
                                    }

                                    string line = await readTask; // 실제 읽은 데이터 가져오기

                                    if (line == null)
                                    {
                                        // 스트림 종료 → outer catch (Exception) 로 보내 5초 대기 후 재연결 (무한 루프 방지)
                                        Debug.WriteLine("[SSE] 스트림 끝 도달 (서버 정상 종료). 재연결 시도");
                                        throw new IOException("SSE stream closed by server");
                                    }

                                    if (string.IsNullOrWhiteSpace(line)) // 빈 줄 = 이벤트 블록 완료
                                    {
                                        if (!string.IsNullOrEmpty(eventName) && dataBuffer.Length > 0)
                                        {
                                            await HandleEvent(eventName, dataBuffer.ToString());
                                        }
                                        eventName = null;
                                        dataBuffer.Clear();
                                        continue; // 다음 줄 읽기로 바로 넘어감
                                    }

                                    if (line.StartsWith(":"))
                                    {
                                        // Heartbeat 수신 - 타임아웃(timeoutCts)이 다음 루프에서 초기화되므로 생명 연장!
                                        continue;
                                    }
                                    else if (line.StartsWith("event:"))
                                    {
                                        eventName = line.Substring(6).Trim();
                                    }
                                    else if (line.StartsWith("data:"))
                                    {
                                        dataBuffer.Append(line.Substring(5).Trim());
                                    }
                                    else if (line == "") // 빈 줄 = 이벤트 블록 완료
                                    {
                                        if (!string.IsNullOrEmpty(eventName) && dataBuffer.Length > 0)
                                        {
                                            await HandleEvent(eventName, dataBuffer.ToString());
                                        }
                                        eventName = null;
                                        dataBuffer.Clear();
                                    }
                                }
                                catch (OperationCanceledException)
                                {
                                    // 사용자가 앱을 종료해서 ct가 취소되었거나
                                    if (ct.IsCancellationRequested) throw;

                                    // 60초 동안 하트비트/이벤트가 없어서 timeoutCts가 취소된 경우
                                    Debug.WriteLine("[SSE] 60초 응답 없음 (타임아웃). 좀비 커넥션 방지 - 강제 재연결 시도");
                                    break; // while 루프를 빠져나가 5초 뒤 재연결 로직으로 감
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"[SSE 읽기 오류] {ex.Message}");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private Task HandleEvent(string eventName, string data)
        {
            if (eventName == "connect")
            {
                return Task.CompletedTask;
            }

            if (eventName == "visitor")
            {
                try
                {
                    var evt = JsonConvert.DeserializeObject<VisitorEventVO>(data);
                    if (evt == null) return Task.CompletedTask;

                    _app.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            AppAlert.Instance.TodayVisitorCount = evt.Count;
                            AppAlert.Instance.TodayDateText = evt.Today.ToString("yyyy년 MM월 dd일");
                            (Application.Current.MainWindow as MainWindow)?.ShowAlertOverlay(evt);
                        }
                        catch (Exception uiEx)
                        {
                            Debug.WriteLine($"[SSE UI 오류] {uiEx.Message}");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SSE 파싱 오류] {ex.Message}");
                }
            }

            return Task.CompletedTask;
        }
    }
}

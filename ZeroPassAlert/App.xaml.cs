using Microsoft.Win32;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ZeroPassAlert.Sse;
using ZeroPassAlert.Utils;

namespace ZeroPassAlert
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        // unregister 멱등 보장 (여러 종료 후크에서 동시/중복 호출 방지)
        private static int _unregisterCalled = 0;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // 글로벌 미처리 예외 핸들러 (앱 종료 방지 및 원인 추적)
            this.DispatcherUnhandledException += (s, args) =>
            {
                Debug.WriteLine($"[DispatcherUnhandledException] {args.Exception.Message}");
                SafeUnregister("DispatcherException");
                args.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                Debug.WriteLine($"[AppDomain UnhandledException] {ex?.Message}");
                SafeUnregister("AppDomainException");
            };

            TaskScheduler.UnobservedTaskException += (s, args) =>
            {
                Debug.WriteLine($"[UnobservedTaskException] {args.Exception.Message}");
                args.SetObserved();
            };

            // CLR 프로세스 정상 종료(2초 이내 처리 보장)
            AppDomain.CurrentDomain.ProcessExit += (s, args) => SafeUnregister("ProcessExit");

            // Windows 로그오프 / 셧다운
            SystemEvents.SessionEnding += (s, args) => SafeUnregister("SessionEnding:" + args.Reason);

            await InitApplication();
        }

        private Task InitApplication()
        {
            AppGlobal.BaseUrl = ConfigurationManager.AppSettings["BASE_URL"];
            AppGlobal.APIUrl = AppGlobal.BaseUrl + AppGlobal.AlertUrl;
            return Task.CompletedTask;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SafeUnregister("OnExit");
            base.OnExit(e);
        }

        /// <summary>
        /// 종료 시 백엔드 SSE 세션 등록 해제. 어떤 종료 경로에서 호출되더라도 단 한 번만 실행됨.
        /// 작업 관리자 등의 강제 종료(TerminateProcess)는 OS 가 프로세스를 즉시 죽이므로 호출 불가 →
        /// 그 경우는 백엔드 SseEmitter.onError 의 자동 정리에 의존한다.
        /// </summary>
        private static void SafeUnregister(string trigger)
        {
            if (Interlocked.Exchange(ref _unregisterCalled, 1) != 0) return;

            try
            {
                SseManager.Stop();

                if (string.IsNullOrEmpty(AppGlobal.CorpCode) || string.IsNullOrEmpty(AppGlobal.GuardId))
                {
                    Debug.WriteLine($"[Unregister] 스킵 (trigger={trigger}): CorpCode/GuardId 미설정");
                    return;
                }

                RestUtil.PostData("visitor/events/unregister/" + AppGlobal.CorpCode + "/" + AppGlobal.GuardId, null);
                Debug.WriteLine($"[Unregister] 완료 (trigger={trigger})");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Unregister] 실패 (trigger={trigger}): {ex.Message}");
            }
        }
    }
}

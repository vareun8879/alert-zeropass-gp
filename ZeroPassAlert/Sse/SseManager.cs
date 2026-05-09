using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ZeroPassAlert.Sse
{
    public static class SseManager
    {
        private static Application _app;
        private static CancellationTokenSource _cts;
        private static bool _isRunning;

        public static void Initialize(Application app)
        {
            _app = app;
        }

        public static void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _cts = new CancellationTokenSource();
            var client = new SseClient(_app);
            Task.Run(() => client.Start(_cts.Token))
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        Debug.WriteLine($"[SseManager] Task FAULTED: {t.Exception?.GetBaseException().Message}\n{t.Exception?.GetBaseException().StackTrace}");
                    else if (t.IsCanceled)
                        Debug.WriteLine("[SseManager] Task CANCELED");
                    else
                        Debug.WriteLine("[SseManager] Task COMPLETED (정상 종료)");
                }, TaskScheduler.Default);
        }

        public static void Stop()
        {
            Debug.WriteLine("[SseManager] Stop 호출됨");
            _cts?.Cancel();
            _cts = null;
            _isRunning = false;
        }
    }
}

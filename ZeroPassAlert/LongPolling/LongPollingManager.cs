using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;   // Application 포함
using ZeroPassAlert.Utils;

namespace ZeroPassAlert.LongPolling
{
    public static class LongPollingManager
    {
        //private static bool _isRunning = false;
        //private static Application _app;

        //public static void Initialize(Application app)
        //{
        //    _app = app;
        //}

        //private static List<LongPoller> _pollers = new List<LongPoller>();

        //public static void StartMultiple()
        //{
        //    // 첫 Poller 즉시 실행
        //    StartPoller(_app);

        //    // 두 번째 Poller는 10초 뒤
        //    Task.Run(async () =>
        //    {
        //        await Task.Delay(10000);
        //        StartPoller(_app);
        //    });
        //}

        //private static void StartPoller(Application app)
        //{
        //    var poller = new LongPoller(app);
        //    _pollers.Add(poller);

        //    Task.Run(poller.Start);
        //}
            private static Application _app;
            private static LongPoller _poller; // 리스트 대신 단일 객체 사용

            public static void Initialize(Application app)
            {
                _app = app;
            }

            public static void Start()
            {
                // 이미 실행 중이면 중복 실행 방지
                if (_poller != null) return;

                // 하나의 폴러만 실행 (서버 부하 감소)
                StartPoller(_app);
            }

            private static void StartPoller(Application app)
            {
                _poller = new LongPoller(app);
                // 별도 스레드(Task)에서 루프 실행
                Task.Run(_poller.Start);
            }
        }

        public class VisitorEventVO
    {
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string VisitLoc { get; set; }
        public string VisitPurpose { get; set; }
        public long Count { get; set; }
        public DateTime TodayDate { get; set; }
    }
}

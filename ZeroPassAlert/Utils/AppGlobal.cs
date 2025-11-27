using System.Collections;
using System.Collections.Specialized;
using ZeroPassAlert.Enum;

namespace ZeroPassAlert.Utils
{
    internal class AppGlobal
    {
        public static string ServiceType = "ZEROPASS";

        //public const string UserAgent = "ZeroPass-KioskClient";

        // 테스트용 설정
        public const string UserAgent = "KIOSK";

        public static long TodayVisitorCount = 0;

        public static string MachineGuid = "";
        public static string CpuId = "";
        public static string LoginPwd = "";

        public static string LoginId = "";

        public static string AccessToken = "";

        public static string KioskId = "";
        public static string KioskSerialNo = "";
        
        public static string AppTitle = "";

        public static string CorpId = "";
        public static string CorpCode = "";
        public static string CorpName = "";

        public static string PrivacyAgreeYn = "N";
        public static string SharingAgreeYn = "N";
        public static string VisitPurposeCode = "";
        public static string VisitPurposeName = "";
        public static string VisitLocCode = "";
        public static string VisitLocName = "";
        public static string AuthType = "";
        public static string Name = "";
        public static string Mobile = "";
        public static string Birthday = "";

        public static string ManagerTel = "";

        public static string FeverCheckYn = "";
        public static string WeekdayEndYn = "";
        public static string WeekdayEndHour = "";
        public static string WeekdayEndMinute = "";

        public static string AccessLimitYn = "N";
        public static string AccessLimitStartHour = "";
        public static string AccessLimitStartMinute = "";
        public static string AccessLimitEndHour = "";
        public static string AccessLimitEndMinute = "";

        // 인증 완료시 정보 저장 변수 (확인증 수령에서 데이터 저장시 사용)
        public static Hashtable SignatureData = new Hashtable();

        public static string BaseUrl = "";
        //public static string KioskUrl = "/zeropass/kiosk/";

        // 테스트용 설정
        public static string KioskUrl = "/digitalsign/kiosk/";

        public static string APIUrl = $"{BaseUrl}{KioskUrl}";

        // Barocert 인증 - 접수 아이디
        public static string ReceiptID = "";

        // 얼굴 인식 - Haar Cascade 분류기
        public const string HaarCascadeFile = @"Haarcascade\haarcascade_frontalface_alt.xml";
        public static string FaceDetectionCascadePath = "";
        public static float ScaleFactor = 1.1f;
        public static int MinNeighbors = 4;

        // 발열인식 - 온도 요청 데이터
        public const string ModbusTemperatureRequestData = "43616C63540D0A";

        // 발열 카메라
        public static string FeverPortName = "";

        public static int FeverVideoIndex = 0;
        public static string FeverDeviceName = "USB-SERIAL CH340";
        public static int FeverBaudRate = 115200;
        public static float ThresholdTemperature = 39.5f;

        // 라벨 프린터
        public static string LabelPrintLdn = "TM-L100";

        // 화면 유지시간 이동 시간
        public static int DisplayFirstExpiredTime = 60;              // 60초
        public static int DisplayExpiredTime = 180;                 // 180초
        public static int DisplayLastExpiredTime = 30;              // 30초

        // 프린트 사용여부
        public static string PrintYn = "Y";

        // 현재 날짜
        public static string Today = "";

        // 온도 측정 관련
        public const string DialogConfirm = "C";             // 확인

        public const string DialogReceiveConfirm = "RC";     // 확인증 수령

        // updater 실행 파일명
        public static string UpdaterExecFileName = "ZeroPassUpdater.exe";

        // 방문 목적
        public static OrderedDictionary VisitPurposeList = new OrderedDictionary();

        // 방문 장소
        public static OrderedDictionary VisitLocList = new OrderedDictionary();

        // 카메라 사용 여부
        public static string CameraUseYn = "Y";

        public static ViewType CurrentViewType { get; set; } = ViewType.None;

        // 잭 연결 상태별 기본 오디오 장치 이름
        public static string JackInsertedDeviceName = "USB Audio Device";               // 이어폰

        public static string JackRemovedDeviceName = "Realtek High Definition Audio";   // 스피커

        public const string DialogYes = "Y";
        public const string DialogNo = "N";
        public const string DialogApply = "Apply";
        public const string DialogCancel = "Cancel";

        // 고대비 여부(true: 고대비 / false: 일반 모드)
        public static bool HighContrastMode = false;        

        // 음성 안내 화면 표시
        public static bool VoiceGuideMode = false;          
        
        // 인증서 발급 도움말
        public static string HelpType = "";

        // 이어폰 연결 여부
        public static bool IsJackInserted = false;

        // ZOOM 여부
        public static bool IsZoom = false;

        // 다시듣기 여부
        public static bool IsReplay = false;

        // 남은 시간
        public static int RemainSeconds = 20;
    }
}
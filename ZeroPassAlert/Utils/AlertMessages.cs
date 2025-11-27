using System.Collections;
using System.Collections.Specialized;
using ZeroPassAlert.Enum;

namespace ZeroPassAlert.Utils
{
    public static class AlertMessages
    {
        public const string AbnormalTempDetectedMessage = "이상 온도가 감지되었습니다.";
        public const string TempMeasureFailMessage = "온도측정이 불가합니다.";

        // 알림 메세지
        public const string AdminSupportMessage = "관리자에게 문의하세요.";

        public const string NoPrinterMessage = "연결된 프린트가 없습니다.";
        public const string NoCameraMessage = "카메라를 찾을 수 없습니다.";
        public const string NoPrintInfo = "출력할 정보가 없습니다.";

        // 생년월일 자리수 체크
        public const string BirthFormatInvalid = "생년월일 형식이 올바르지 않습니다.";

        // 휴대전화번호 자리수 체크
        public const string MobileFormatInvalid = "휴대전화번호 형식이 올바르지 않습니다.";

        // 대기시간 연장 메시지
        public const string WaitTimeExtensionPrompt = "대기시간을 연장하시겠습니까?";

        // 미서명(인증 미완료) 안내 메시지
        public const string UnsignedAuthenticationNotice = 
            "서명되지 않았습니다.\n" +
            "휴대전화에서 인증 메시지를 확인 후\n인증을 진행해 주세요.\n\n" +
            "인증 요청이 오지 않았다면\n이전 화면으로 돌아가\n 다시 실행 부탁드립니다.";

        // 요청 만료 안내 메시지
        public const string ExpiredRequestNotice = "만료된 요청입니다.\n이전 화면으로 이동합니다.";

        // 인증 취소 안내 메시지
        public const string AuthenticationCanceledNotice = "인증이 취소되었습니다.\n이전 화면으로 이동합니다.";

        // 이상 온도 감지 안내 메시지
        public const string AbnormalTemperatureDetected = "{0} 님에게\n이상 온도가 감지되었습니다";

        // 남은 시간 안내 메시지
        public const string RemainingTimeNotice = "남은 시간: {0}초";

        // 남은 시간 시:분 형식 안내 메시지
        public const string RemainingTimeFormattedNotice = "남은 시간: {0:D2}:{1:D2}";

        // 네이버 출입증 QR 인증 실패 안내 메시지
        public const string QrAuthFailed =
            "인증에 실패했습니다.\n\n네이버 출입증 QR을 다시 스캔해 주세요.";
    }
}
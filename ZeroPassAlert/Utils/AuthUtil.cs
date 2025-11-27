using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Text;

namespace ZeroPassAlert.Utils
{
    public static class AuthUtil
    {

        // 인증 관련 전역값 설정
        public static void SetAuthInfo(string loginPwd)
        {

            var jsonObject = new Dictionary<string, string>
            {
                ["id"] = "1",
                ["machineGuid"] = "D14B6898-F2DE-43DF-8343-43FCD4550984",
                ["cpuId"] = "178BBFF00A40F41",
                ["accessToken"] = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzUxMiJ9.eyJs2dpbkkljoiRDE0QjY4OUltRjJERS00M0RGLTgzNDMtNDNGQ0Q0NTUwOTg0",
                ["corpId"] = "1",
                ["corpCode"] = "10000",
                ["corpName"] = "바른정보기술",
                ["serialNo"] = "02219-165-430-000"
            };

            if (jsonObject != null)
            {
                AppGlobal.AccessToken = jsonObject["accessToken"];
                AppGlobal.CorpId = jsonObject["corpId"];
                AppGlobal.CorpCode = jsonObject["corpCode"];
                AppGlobal.CorpName = jsonObject["corpName"];

                AppGlobal.LoginId = AppGlobal.MachineGuid + "_" + AppGlobal.CpuId;
                
                AppGlobal.KioskId = jsonObject["id"];
                AppGlobal.KioskSerialNo = jsonObject["serialNo"];

                AppGlobal.LoginPwd = loginPwd;
            }
        }

        // 인증 관련 전역값 초기화
        public static void ClearAuthInfo()
        {
            AppGlobal.AccessToken = null;
            AppGlobal.LoginId = null;
            AppGlobal.LoginPwd = null;
            AppGlobal.CorpId = null;
            AppGlobal.CorpCode = null;
            AppGlobal.CorpName = null;
            AppGlobal.KioskId = null;
            AppGlobal.MachineGuid = null;
            AppGlobal.CpuId = null;
        }
    }
}
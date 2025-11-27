using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;
using System.IO;

namespace ZeroPassAlert.Utils
{
    public class ResponseResult
    {
        public string Status { get; set; }
        public string Data { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionCode { get; set; }
    }

    public static class RestUtil
    {
        public const string ServerCommunicationError = "서버 통신 중 오류가 발생했습니다";
        public const string CannotConnectToServer = "서버에 연결할 수 없습니다";
        public const string UnexpectedError = "예기치 못한 오류가 발생했습니다";

        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public static HttpClient Client => _http;

        private static StringContent ToJsonContent(object obj)
        {
            object payload = obj;

            if (obj is Hashtable ht)
            {
                payload = ht.Cast<DictionaryEntry>()
                            .ToDictionary(d => Convert.ToString(d.Key) ?? "",
                                          d => d.Value);
            }

            string json = JsonConvert.SerializeObject(payload);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        // 테스트용
        public static ResponseResult GetDataCert(string url, Hashtable param)
        {
            try
            {
                string responseString = string.Empty;

                url = AppGlobal.BaseUrl + "/digitalsign/" + url;

                if (param != null)
                {
                    url += Map2GetParams(param);
                }

                using (var req = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    AddCommonHeaders(req);

                    HttpResponseMessage response = _http.SendAsync(req).Result;

                    responseString = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return new ResponseResult
                        {
                            Data = responseString
                        };
                    }
                    else
                    {
                        return ParseError(responseString);
                    }
                }

            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public static ResponseResult GetData(string url, Hashtable param)
        {
            try
            {
                string responseString = string.Empty;

                url = AppGlobal.APIUrl + url;

                if (param != null)
                {
                    url += Map2GetParams(param);
                }

                using (var req = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    AddCommonHeaders(req);

                    HttpResponseMessage response = _http.SendAsync(req).Result;

                    responseString = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return new ResponseResult
                        {
                            Data = responseString
                        };
                    }
                    else
                    {
                        return ParseError(responseString);
                    }
                }

            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public static ResponseResult PostData(string url, Hashtable param)
        {
            try
            {
                url = AppGlobal.APIUrl + url;

                using (var req = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    AddCommonHeaders(req);
                    req.Content = ToJsonContent(param);

                    HttpResponseMessage response = _http.SendAsync(req).Result;
                    string responseString = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return new ResponseResult
                        {
                            Data = responseString
                        };
                    }
                    else
                    {
                        return ParseError(responseString, includeCode: true);
                    }
                }
            }
            catch (AggregateException ex)
            {
                var first = ex.InnerExceptions.FirstOrDefault();
                return new ResponseResult
                {
                    ExceptionMessage = ServerCommunicationError + first?.Message
                };
            }
            catch (HttpRequestException ex)
            {
                return new ResponseResult
                {
                    ExceptionMessage = CannotConnectToServer + ex.Message
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    ExceptionMessage = UnexpectedError + ex.Message
                };
            }
        }

        public static ResponseResult PostDataWithFile(string url, Hashtable param)
        {
            url = AppGlobal.APIUrl + url;

            // 파일 경로를 Hashtable에서 가져오기
            string filePath =  null;
            string folderPath = null;

            param.Add("kioskSerialNo", AppGlobal.KioskSerialNo);

            string json = JsonConvert.SerializeObject(param);

            // HttpClient 인스턴스 생성
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(AppGlobal.UserAgent);

                using (var multipartFormData = new MultipartFormDataContent())
                {
                    // JSON 데이터 전송 및 응답 수신
                    StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    multipartFormData.Add(content, "formData");

                    // 얼굴 파일이 있으면
                    if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    {
                        byte[] fileBytes = File.ReadAllBytes(filePath);
                        ByteArrayContent byteContent = new ByteArrayContent(fileBytes);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                        multipartFormData.Add(byteContent, "file", Path.GetFileName(filePath));
                    }

                    HttpResponseMessage response = client.PostAsync(url, multipartFormData).Result;

                    string responseString = response.Content.ReadAsStringAsync().Result;

                    // 응답 확인
                    if (response.IsSuccessStatusCode)
                    {
                        return new ResponseResult
                        {
                            Data = responseString
                        };
                    }
                    else
                    {
                        dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                        if (jsonObject.errors != null && jsonObject.errors.Count == 1)
                        {
                            return new ResponseResult
                            {
                                ExceptionMessage = jsonObject.errors[0].message
                            };
                        }
                        else
                        {
                            string msg = AlertMessages.AdminSupportMessage;

                            if (jsonObject.message != null)
                            {
                                msg = jsonObject.message;
                            }

                            return new ResponseResult
                            {
                                ExceptionMessage = msg
                            };
                        }
                    }
                }
            }
        }

        public static ResponseResult PostDataCert(string url, Hashtable param)
        {
            try
            {
                string responseString = string.Empty;

                url = AppGlobal.APIUrl + url;

                if (param != null)
                {
                    url += Map2GetParams(param);
                }

                using (var req = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    AddCommonHeaders(req);
                    req.Content = ToJsonContent(param);

                    HttpResponseMessage response = _http.SendAsync(req).Result;

                    responseString = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return new ResponseResult
                        {
                            Data = responseString
                        };
                    }
                    else
                    {
                        return ParseError(responseString);
                    }
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public static ResponseResult PostDataHistory(string url, Hashtable param)
        {
            try
            {
                url = AppGlobal.APIUrl + url;

                using (var req = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    AddCommonHeaders(req);
                    req.Content = ToJsonContent(param);

                    HttpResponseMessage response = _http.SendAsync(req).Result;
                    string responseString = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return new ResponseResult
                        {
                            Data = responseString
                        };
                    }
                    else
                    {
                        return ParseError(responseString);
                    }
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        // 버전 정보 조회
        public static ResponseResult GetDataVersion(string url, Hashtable param)
        {
            try
            {
                string responseString = string.Empty;

                url = AppGlobal.APIUrl + url;

                if (param != null)
                {
                    url += Map2GetParams(param);
                }

                using (var req = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    AddCommonHeaders(req);

                    HttpResponseMessage response = _http.SendAsync(req).Result;

                    responseString = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        return new ResponseResult
                        {
                            Data = responseString
                        };
                    }
                    else
                    {
                        return ParseError(responseString);
                    }
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public static async Task<ResponseResult> GetDataAsync(string url, Hashtable param, CancellationToken ct = default)
        {
            try
            {
                string reqUrl = AppGlobal.APIUrl + url;
                if (param != null) reqUrl += Map2GetParams(param);

                using (var req = new HttpRequestMessage(HttpMethod.Get, reqUrl))
                {
                    AddCommonHeaders(req);

                    using (var res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
                    {
                        var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);

                        if (res.IsSuccessStatusCode)
                            return new ResponseResult { Data = body };

                        return ParseError(body);
                    }
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public static async Task<ResponseResult> PostDataAsync(string url, Hashtable param, CancellationToken ct = default)
        {
            try
            {
                string reqUrl = AppGlobal.APIUrl + url;

                using (var req = new HttpRequestMessage(HttpMethod.Post, reqUrl))
                {
                    AddCommonHeaders(req);

                    req.Content = ToJsonContent(param);

                    using (var res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
                    {
                        var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (res.IsSuccessStatusCode)
                            return new ResponseResult { Data = body };

                        return ParseError(body, includeCode: true);
                    }
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public static async Task<ResponseResult> PostDataCertAsync(string url, Hashtable param, CancellationToken ct = default)
        {
            try
            {
                string reqUrl = AppGlobal.APIUrl + url;
                if (param != null) reqUrl += Map2GetParams(param);

                using (var req = new HttpRequestMessage(HttpMethod.Post, reqUrl))
                {
                    AddCommonHeaders(req);
                    req.Content = ToJsonContent(param);

                    using (var res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
                    {
                        var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (res.IsSuccessStatusCode)
                            return new ResponseResult { Data = body };

                        return ParseError(body);
                    }
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public static async Task<ResponseResult> PostDataHistoryAsync(string url, Hashtable param, CancellationToken ct = default)
        {
            try
            {
                string reqUrl = AppGlobal.APIUrl + url;

                using (var req = new HttpRequestMessage(HttpMethod.Post, reqUrl))
                {
                    AddCommonHeaders(req);

                    req.Content = ToJsonContent(param);

                    using (var res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
                    {
                        var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (res.IsSuccessStatusCode)
                            return new ResponseResult { Data = body };

                        return ParseError(body);
                    }
                }
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        public static async Task<ResponseResult> GetDataVersionAsync(string url, Hashtable param, CancellationToken ct = default)
        {
            return await GetDataAsync(url, param, ct).ConfigureAwait(false);
        }

        private static ResponseResult ParseError(string responseString, bool includeCode = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(responseString))
                    return new ResponseResult { ExceptionMessage = AlertMessages.AdminSupportMessage };

                dynamic json = JsonConvert.DeserializeObject(responseString);

                string code = null;
                string message = null;

                if (json?.errors != null && json.errors.Count == 1)
                {
                    code = (string)json.errors[0].code;
                    message = (string)json.errors[0].message;
                }
                else
                {
                    message = (string)(json?.message ?? AlertMessages.AdminSupportMessage);
                }

                // 내부 코드나 시스템 오류 문구 필터링
                if (!string.IsNullOrEmpty(code))
                {
                    // 내부 시스템 코드(8000~8999)
                    if (int.TryParse(code, out int codeNum) && codeNum >= 8000 && codeNum <= 8999)
                    {
                        return new ResponseResult
                        {
                            ExceptionMessage = AlertMessages.AdminSupportMessage,
                            ExceptionCode = code
                        };
                    }
                }

                // 메시지에 바인딩,SQL,Exception 등의 단어가 포함되어 있으면 마스킹
                if (!string.IsNullOrEmpty(message) &&
                    (message.Contains("바인딩") ||
                     message.Contains("SQL") ||
                     message.Contains("Exception") ||
                     message.Contains("Stack")))
                {
                    return new ResponseResult
                    {
                        ExceptionMessage = AlertMessages.AdminSupportMessage,
                        ExceptionCode = code
                    };
                }

                return new ResponseResult
                {
                    ExceptionMessage = message,
                    ExceptionCode = includeCode ? code : null
                };
            }
            catch (JsonException)
            {
                return new ResponseResult { ExceptionMessage = AlertMessages.AdminSupportMessage };
            }
            catch
            {
                return new ResponseResult { ExceptionMessage = AlertMessages.AdminSupportMessage };
            }
        }

        private static ResponseResult HandleException(Exception ex)
        {

            switch (ex)
            {
                case AggregateException ae:
                    var inner = ae.Flatten().InnerExceptions.FirstOrDefault();
                    if (inner is HttpRequestException)
                        return new ResponseResult { ExceptionMessage = CannotConnectToServer };
                    if (inner is TaskCanceledException)
                        return new ResponseResult { ExceptionMessage = CannotConnectToServer };

                    return new ResponseResult { ExceptionMessage = ServerCommunicationError };

                case HttpRequestException hre:
                    return new ResponseResult { ExceptionMessage = CannotConnectToServer };

                case TaskCanceledException tce:
                    return new ResponseResult { ExceptionMessage = CannotConnectToServer };

                default:
                    return new ResponseResult { ExceptionMessage = UnexpectedError };
            }
        }


        public static string Map2GetParams(Hashtable param)
        {
            if (param == null || param.Count == 0) return string.Empty;

            var sb = new StringBuilder("?");
            bool first = true;

            foreach (DictionaryEntry entry in param)
            {
                if (!first) sb.Append('&'); else first = false;

                string key = Uri.EscapeDataString(Convert.ToString(entry.Key) ?? "");
                string val = Uri.EscapeDataString(Convert.ToString(entry.Value) ?? "");
                sb.Append(key).Append('=').Append(val);
            }
            return sb.ToString();
        }

        public static string Map2PostParams(Hashtable param)
        {
            string p = string.Empty;

            foreach (DictionaryEntry entry in param)
            {
                string key = JsonUtil.ConvertSnakeToCamel(entry.Key.ToString());

                if (p == string.Empty)
                {
                    p += string.Format(@"{0}={1}", key, entry.Value);
                }
                else
                {
                    p += string.Format(@"&{0}={1}", key, entry.Value);
                }
            }

            return p;
        }

        public static void AddCommonHeaders(HttpRequestMessage req)
        {
            // User-Agent (요청마다 명시적으로 추가)
            if (!string.IsNullOrEmpty(AppGlobal.UserAgent))
            {
                req.Headers.UserAgent.Clear();
                req.Headers.UserAgent.ParseAdd(AppGlobal.UserAgent);
            }

            /*
            // Access Token 헤더 추가
            if (!string.IsNullOrEmpty(AppGlobal.AccessToken))
            {
                req.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", AppGlobal.AccessToken);
            }
            */

        }

        public static Hashtable CreateCommonParams(Hashtable extraParams = null)
        {
            var param = new Hashtable
            {
                { "corpId", AppGlobal.CorpId },
                { "corpCode", AppGlobal.CorpCode },
                { "corpName", AppGlobal.CorpName },
                { "serialNo", AppGlobal.KioskSerialNo },
                { "kioskId", AppGlobal.KioskId },
                { "loginId", AppGlobal.LoginId }
            };

            if (extraParams != null)
            {
                foreach (DictionaryEntry entry in extraParams)
                    param[entry.Key] = entry.Value;
            }

            return param;
        }

    }
}
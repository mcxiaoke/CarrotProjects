using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CarrotCommon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GenshinLib {

    public static class GenshinHelper {

        // Guid.NewGuid().ToString("D")
        public static string DEVICE_ID = "701f23bd-3b79-4adb-85f2-0e9ac3ba3a6b";

        public static void SetExtraHeadres(HttpRequestMessage request, bool newDS = true) {
            var version = newDS ? GenshinConst.MHY_VER_NEW : GenshinConst.MHY_VER_OLD;
            //Console.WriteLine($"SetExtraHeadres for {version} newDS={newDS}");
            var defaultHeaders = new Dictionary<string, string>() {
                {"Accept-Language", "zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7" },
                {"Accept-Encoding", "gzip, deflate" },
                { "Accept", "application/json"},
                {"Referer", $"https://webstatic.mihoyo.com/app/community-game-records/index.html?v=6" },
                //{"Origin", "https://webstatic.mihoyo.com" },
                { "User-Agent", $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) miHoYoBBS/{version}"},
                {"X-Requested-With", "com.mihoyo.hyperion" },
                { "x-rpc-device_id", DEVICE_ID},
                {"x-rpc-client_type", "5" },
                {"x-rpc-app_version", version },
            };
            HttpHeaders headers = request.Headers;
            foreach (var header in defaultHeaders) {
                headers.Add(header.Key, header.Value);
            }
        }

        public static string OldDS() {
            const string salt = GenshinConst.MHY_SALT_OLD;
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var randomStr = Utility.GetRandomString(6);
            // for old salt sign reward
            // 14bmu1mz0yuljprsfgpvjh3ju2ni468r only working on 2.7.x
            var str = $"salt={salt}&t={timestamp}&r={randomStr}";
            var sign = Utility.GetComputedMd5(str);
            var ds = $"{timestamp},{randomStr},{sign}";
            //Console.WriteLine($"OldDS str={str}");
            return ds;
        }

        public static string NewDS(IDictionary<string, string>? queryDict,
            object? bodyObj = null) {
            const string salt = GenshinConst.MHY_SALT_NEW;
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var randomStr = Utility.GetRandomString(6);
            var query = Utility.CreateQueryString(queryDict);
            var body = Utility.Stringify(bodyObj);
            // for mobile web new salt daily note
            // xV8v4Qu54lUKrEYFZkJhB8cuOh9Asafs 2.27.2
            var str = $"salt={salt}&t={timestamp}&r={randomStr}&b={body}&q={query}";
            var sign = Utility.GetComputedMd5(str);
            var ds = $"{timestamp},{randomStr},{sign}";
            //Console.WriteLine($"NewDS str={str}");
            return ds;
        }
    }

    public class TokenException : Exception {

        public TokenException(string message) : base(message) {
        }

        public TokenException(string message, Exception innerException) : base(message, innerException) {
        }

        public TokenException() : base() {
        }
    }

    public class ServerException : Exception {

        public ServerException(string message) : base(message) {
        }

        public ServerException(string message, Exception innerException) : base(message, innerException) {
        }

        public ServerException() : base() {
        }
    }

    public class ClientException : Exception {

        public ClientException(string message) : base(message) {
        }

        public ClientException(string message, Exception innerException) : base(message, innerException) {
        }

        public ClientException() : base() {
        }
    }

    public class GenshinAPI {
        private readonly HttpClientHandler handler;
        private readonly HttpClient client;

        public GenshinAPI() : this(String.Empty) {
        }

        public GenshinAPI(string cookie) {
            this.Cookie = cookie;
            handler = new HttpClientHandler {
                UseCookies = false, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            client = new HttpClient(handler);
        }

        public string? Cookie { get; set; }
        public UserGameRole? User { get; set; }
        public bool Ready => Cookie != null && User != null;
        public string UID => User?.GameUid ?? "0";
        public IDictionary<string, string> CookieDict => Utility.ParseCookieString(Cookie);

        private void CheckReady(bool checkUser = false) {
            if (String.IsNullOrEmpty(Cookie)) {
                throw new ArgumentException("未设置Cookie");
            }
            if (!Utility.ValiteCookieFields(Cookie)) {
                throw new ArgumentException("错误的Cookie");
            }
            if (checkUser && User == null) {
                throw new ArgumentException("未找到角色信息");
            }
        }

        private int RequestID = 0;

        private async Task<string> SendRequestAsync(HttpMethod method,
           string url,
           IDictionary<string, string>? queryDict,
           object? bodyObj,
           bool newDS = true) {
            var rid = ++RequestID;
            var builder = new UriBuilder(url) { Query = Utility.CreateQueryString(queryDict) };
            url = builder.ToString();
            using var request = new HttpRequestMessage(method, url);
            GenshinHelper.SetExtraHeadres(request, newDS);
            request.Headers.Add("Cookie", Cookie);
            request.Headers.Add("DS", newDS ? GenshinHelper.NewDS(queryDict, bodyObj) : GenshinHelper.OldDS());
            if (method == HttpMethod.Post && bodyObj != null) {
                request.Content = new StringContent(Utility.Stringify(bodyObj), Encoding.UTF8, "application/json");
                Logger.Debug($"[API][{rid}] {method} {url} data={Utility.Stringify(bodyObj)}");
            }
            Logger.Info($"[API][{rid}][Req] {method} {url} ({UID})");
            var response = await client.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            Logger.Debug($"[API][{rid}][Res] {response.StatusCode} @{json.SafeSubstring(0, 128)}@");
            if ((int)response.StatusCode >= 500) {
                throw new ServerException($"Server Error {response.StatusCode}");
            } else if (response.IsSuccessStatusCode) {
                if (json.Contains("登录") || json.Contains("login")) {
                    throw new TokenException(json);
                } else {
                    dynamic? jsonObj = JsonConvert.DeserializeObject(json);
                    if (jsonObj?.retcode == -100 || jsonObj?.retcode == 10001) {
                        throw new TokenException(json);
                    }
                    return json;
                }
            } else {
                throw new ClientException(json);
            }
        }

        private async Task<string> GetAsync(string url,
           IDictionary<string, string> queryDict, bool newDS = true) {
            return await SendRequestAsync(HttpMethod.Get, url, queryDict, null, newDS);
        }

        private async Task<string> PostAsync(string url,
           IDictionary<string, string>? queryDict,
           object? bodyObj,
           bool newDS = true) {
            return await SendRequestAsync(HttpMethod.Post, url, queryDict, bodyObj, newDS);
        }

        public async Task<(string?, Exception?)> PostSignReward() {
            string? data = null;
            Exception? error = null;
            if (User is UserGameRole u) {
                try {
                    CheckReady(true);
                    Logger.Debug($"API.PostSignReward with {u.GameUid}");
                    var url = $"{GenshinConst.TAKUMI_API}/event/bbs_sign_reward/sign";
                    var body = new Dictionary<string, string>() {
                { "act_id","e202009291139501"},
                {"region",u.Region},
                {"uid",u.GameUid}
                };
                    data = await PostAsync(url, null, body, false);
                } catch (Exception ex) {
                    error = ex;
                }
            }
            return (data, error);
        }

        public async Task<(string?, Exception?)> GetSignReward() {
            string? data = null;
            Exception? error = null;
            if (User is UserGameRole u) {
                try {
                    CheckReady(true);
                    Logger.Debug($"API.GetSignReward with {u.GameUid}");
                    var url = $"{GenshinConst.TAKUMI_API}/event/bbs_sign_reward/info";
                    var query = new Dictionary<string, string>() {
                {"region",u.Region},
                {"uid",u.GameUid},
                { "act_id","e202009291139501"}
            };
                    data = await GetAsync(url, query);
                } catch (Exception ex) {
                    error = ex;
                }
            }

            return (data, error);
        }

        public async Task<(string?, Exception?)> GetMonthInfo() {
            string? data = null;
            Exception? error = null;
            if (User is UserGameRole u) {
                try {
                    CheckReady(true);
                    Logger.Debug($"API.GetMonthInfo with {u.GameUid}");
                    var url = $"{GenshinConst.HK4E_APK}/event/ys_ledger/monthInfo";
                    var query = new Dictionary<string, string>() {
                {"bind_region",u.Region},
                {"bind_uid",u.GameUid},
                { "month","0"}
            };
                    data = await GetAsync(url, query);
                } catch (Exception ex) {
                    error = ex;
                }
            }
            return (data, error);
        }

        public async Task<(DailyNote?, Exception?)> GetDailyNote() {
            DailyNote? data = null;
            Exception? error = null;
            if (User is UserGameRole u) {
                try {
                    CheckReady(true);
                    Logger.Debug($"API.GetDailyNote for {u.GameUid}");
                    var url = $"{GenshinConst.TAKUMI_RECORD_API}/game_record/app/genshin/api/dailyNote";
                    var query = new Dictionary<string, string>() {
                {"server",u.Region},
                {"role_id",u.GameUid},
            };
                    var json = await GetAsync(url, query);
                    dynamic? jsonObj = JsonConvert.DeserializeObject(json);
                    if (jsonObj?.data is JObject o) {
                        data = o.ToObject<DailyNote>();
                        if (data is not null) {
                            data.CreatedAt = DateTime.Now;
                        }
                    }
                } catch (Exception ex) {
                    error = ex;
                }
            }
            return (data, error);
        }

        // base user info, prepare for other request
        public async Task<UserGameRole?> GetGameRoleInfo() {
            CheckReady(false);
            Logger.Debug($"API.GetGameRoleInfo with cookie");
            var url = $"{GenshinConst.TAKUMI_API}/binding/api/getUserGameRolesByCookie";
            var query = new Dictionary<string, string>() {
                {"game_biz","hk4e_cn"},
            };
            var json = await GetAsync(url, query);
            //Console.WriteLine(json);
            dynamic? jsonObj = JsonConvert.DeserializeObject(json);

            if (jsonObj?.data.list[0] is JObject o) {
                var data = o.ToObject<UserGameRole>();
                if (data is not null) {
                    data.CreatedAt = DateTime.Now;
                }
                return data;
            }
            return null;
        }
    }
}
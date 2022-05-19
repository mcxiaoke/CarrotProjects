using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;

namespace GenshinNotifier.Net {

    class Helper {
        // Guid.NewGuid().ToString("D")
        public static string DEVICE_ID = "701f23bd-3b79-4adb-85f2-0e9ac3ba3a6b";

        public static void SetExtraHeadres(HttpRequestMessage request, bool newDS = true) {
            var version = newDS ? Const.MHY_VER_NEW : Const.MHY_VER_OLD;
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
            var salt = Const.MHY_SALT_OLD;
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

        public static string NewDS(IDictionary<string, string> queryDict,
            object bodyObj = null) {
            var salt = Const.MHY_SALT_NEW;
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

    public class API {
        private HttpClientHandler handler = null;
        private HttpClient client = null;

        public API() : this(null) {
        }

        public API(string cookie) {
            this.Cookie = cookie;
            SetupHttpClient();
        }

        public string Cookie { get; set; }
        public UserGameRole User { get; set; }
        public bool Ready => Cookie != null && User != null;


        private void CheckReady(bool checkUser = false) {
            if (Cookie == null) {
                throw new ArgumentException("Cookie Not Found");
            }
            if (!Cookie.Contains("login_ticket")) {
                throw new ArgumentException("Invalid Cookie");
            }
            if (checkUser && User == null) {
                throw new ArgumentException("UserGameRole Not Found");
            }
        }

        private void SetupHttpClient() {
            handler = new HttpClientHandler() { UseCookies = false };
            handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            client = new HttpClient(handler);
        }

        async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method,
           string url,
           IDictionary<string, string> queryDict,
           object bodyObj,
           bool newDS = true) {
            var builder = new UriBuilder(url) { Query = Utility.CreateQueryString(queryDict) };
            url = builder.ToString();
            Logger.Debug($"SendRequestAsync {method} {url} ({newDS})");
            using (var request = new HttpRequestMessage(method, url)) {
                Helper.SetExtraHeadres(request, newDS);
                request.Headers.Add("Cookie", Cookie);
                request.Headers.Add("DS", newDS ? Helper.NewDS(queryDict, bodyObj) : Helper.OldDS());
                if (method == HttpMethod.Post && bodyObj != null) {
                    //HttpContent body = new StringContent(Stringify(bodyDict));
                    //body.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                    request.Content = new StringContent(Utility.Stringify(bodyObj), Encoding.UTF8, "application/json");
                    Logger.Debug($"SendRequestAsync data={Utility.Stringify(bodyObj)}");
                }
                return await client.SendAsync(request);
            }
        }

        async Task<HttpResponseMessage> GetAsync(string url,
           IDictionary<string, string> queryDict, bool newDS = true) {
            return await SendRequestAsync(HttpMethod.Get, url, queryDict, null, newDS);
        }

        async Task<HttpResponseMessage> PostAsync(string url,
           IDictionary<string, string> queryDict,
           object bodyObj,
           bool newDS = true) {
            return await SendRequestAsync(HttpMethod.Post, url, queryDict, bodyObj, newDS);
        }

        public async Task<UserGameRole> Prepare() {
            var (data, error) = await GetGameRoleInfo();
            if (data != null && error == null) {
                this.User = data;
                Logger.Debug($"Prepare {User}");
            } else {
                Logger.Error("Prepare", error);
            }
            return this.User;
        }

        public async Task<(string, Exception)> PostSignReward() {
            string data = null;
            Exception error = null;
            try {
                CheckReady(true);
                Logger.Debug($"PostSignReward with {User}");
                var url = $"{Const.TAKUMI_API}/event/bbs_sign_reward/sign";
                var body = new Dictionary<string, string>() {
                { "act_id","e202009291139501"},
                {"region",User.Region},
                {"uid",User.GameUid}
            };
                var response = await PostAsync(url, null, body, false);
                response.EnsureSuccessStatusCode();
                data = await response.Content.ReadAsStringAsync();
            } catch (Exception ex) {
                error = ex;
            }
            return (data, error);

        }

        public async Task<(string, Exception)> GetSignReward() {
            string data = null;
            Exception error = null;
            try {
                CheckReady(true);
                Logger.Debug($"GetSignReward with {User}");
                var url = $"{Const.TAKUMI_API}/event/bbs_sign_reward/info";
                var query = new Dictionary<string, string>() {
                {"region",User.Region},
                {"uid",User.GameUid},
                { "act_id","e202009291139501"}
            };
                var response = await GetAsync(url, query);
                response.EnsureSuccessStatusCode();
                data = await response.Content.ReadAsStringAsync();
            } catch (Exception ex) {
                error = ex;
            }
            return (data, error);
        }

        public async Task<(string, Exception)> GetMonthInfo() {
            string data = null;
            Exception error = null;
            try {
                CheckReady(true);
                Logger.Debug($"GetMonthInfo with {User}");
                var url = $"{Const.HK4E_APK}/event/ys_ledger/monthInfo";
                var query = new Dictionary<string, string>() {
                {"bind_region",User.Region},
                {"bind_uid",User.GameUid},
                { "month","0"}
            };
                var response = await GetAsync(url, query);
                response.EnsureSuccessStatusCode();
                data = await response.Content.ReadAsStringAsync();
            } catch (Exception ex) {
                error = ex;
            }
            return (data, error);
        }

        public async Task<(DailyNote, Exception)> GetDailyNote() {
            DailyNote data = null;
            Exception error = null;
            try {
                CheckReady(true);
                Logger.Debug($"GetDailyNote for {User.GameUid}");
                var url = $"{Const.TAKUMI_RECORD_API}/game_record/app/genshin/api/dailyNote";
                var query = new Dictionary<string, string>() {
                {"server",User.Region},
                {"role_id",User.GameUid},
            };
                var response = await GetAsync(url, query);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(json);
                dynamic jsonObj = JsonConvert.DeserializeObject(json);
                JObject o = jsonObj.data;
                data = o.ToObject<DailyNote>();
            } catch (Exception ex) {
                error = ex;
            }
            return (data, error);

        }

        // base user info, prepare for other request
        public async Task<(UserGameRole, Exception)> GetGameRoleInfo() {
            UserGameRole data = null;
            Exception error = null;
            try {
                CheckReady(false);
                Logger.Debug($"GetGameRoleInfo with cookie");
                var url = $"{Const.TAKUMI_API}/binding/api/getUserGameRolesByCookie";
                var query = new Dictionary<string, string>() {
                {"game_biz","hk4e_cn"},
            };
                var response = await GetAsync(url, query);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                dynamic jsonObj = JsonConvert.DeserializeObject(json);
                //Console.WriteLine(jsonObj);
                JObject o = jsonObj.data.list[0];
                //Console.WriteLine(o);
                // https://www.newtonsoft.com/json/help/html/SerializingJSONFragments.htm
                data = o.ToObject<UserGameRole>();
                if (data != null) {
                    // update user info
                    this.User = data;
                }
            } catch (Exception ex) {
                error = ex;
            }
            //Console.WriteLine($"GetGameRoleInfo data={data}");
            //Console.WriteLine($"GetGameRoleInfo error={error}");
            return (data, error);
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Carrot.Common;
using GenshinLib;
using Newtonsoft.Json;

namespace GenshinNotifier {

    public sealed class DataController {

        // https://csharpindepth.com/articles/singleton
        public static DataController Default { get { return lazy.Value; } }

        private static readonly Lazy<DataController> lazy =
        new Lazy<DataController>(() => new DataController());

        //public static DataController Default = new DataController();

        private readonly GenshinAPI Api;
        public readonly CacheManager Cache;
        public UserGameRole? UserCached {
            get => this.ViewModel.User;
            set => this.ViewModel.User = value;
        }
        public DailyNote? NoteCached {
            get => this.ViewModel.Note;
            set => this.ViewModel.Note = value;

        }
        public WidgetViewModel ViewModel = new WidgetViewModel();

        private long _userRefreshing = 0;
        private long _noteRefreshing = 0;

        public bool IsUserRefreshing {
            get {
                return Interlocked.Read(ref _userRefreshing) == 1;
            }
            set {
                Interlocked.Exchange(ref _userRefreshing, Convert.ToInt64(value));
            }
        }

        public bool IsNoteRefreshing {
            get {
                return Interlocked.Read(ref _noteRefreshing) == 1;
            }
            set {
                Interlocked.Exchange(ref _noteRefreshing, Convert.ToInt64(value));
            }
        }

        private string? _uid;

        public string? UID {
            get => User?.GameUid ?? _uid;
            set => _uid = value;
        }

        public string? Cookie {
            get => Api.Cookie;
            private set {
                Api.Cookie = value;
            }
        }

        private UserGameRole? User {
            get => Api.User; set {
                Api.User = value;
                Cache.Name = value?.GameUid ?? string.Empty;
            }
        }

        public bool Ready { get { return this.Api.Ready; } }

        private DataController() {
            Logger.Debug("DataController()");
            this.Cache = new CacheManager();
            this.Api = new GenshinAPI();
            LoadUserData();
        }

        private void LoadUserData() {
            var cookie = Properties.Settings.Default.MihoyoCookie;
            var userJson = Properties.Settings.Default.MihoyoUser;
            var cookieValid = Utility.ValiteCookieFields(cookie);
            UserGameRole? user;
            bool userValid;
            try {
                user = JsonConvert.DeserializeObject<UserGameRole>(userJson);
                userValid = !String.IsNullOrEmpty(user?.GameUid) && !String.IsNullOrEmpty(user?.GameBiz);
            } catch (Exception) {
                user = null;
                userValid = false;
            }
            if (cookieValid && userValid) {
                this.Cookie = cookie;
                this.User = user;
                Logger.Debug($"LoadUserData uid={UID} cookie={cookie}");
                Task.Run(async () => {
                    this.ViewModel.User = await this.Cache.LoadCache2<UserGameRole>();
                    this.ViewModel.Note = await this.Cache.LoadCache2<DailyNote>();
                    Logger.Debug($"LoadUserData data async loaded {this.ViewModel.User?.GameUid} {this.ViewModel.Note?.CurrentResin}");
                });

            } else {
                ClearUserData();
            }
        }

        public void ClearUserData() {
            Logger.Debug("ClearUserData");
            this.Cookie = null;
            this.User = null;
            Properties.Settings.Default.MihoyoCookie = null;
            Properties.Settings.Default.MihoyoUserID = null;
            Properties.Settings.Default.MihoyoUser = null;
            Properties.Settings.Default.Save();
        }

        public void SaveUserData(string? newCookie, UserGameRole? user = null) {
            Logger.Debug($"SaveUserData cookie={newCookie} uid={user?.GameUid}");
            if (!String.IsNullOrEmpty(newCookie) && newCookie != this.Cookie) {
                this.Cookie = newCookie;
                Properties.Settings.Default.MihoyoCookie = newCookie;
            }
            if (user != null) {
                this.User = user;
                this.UserCached = user;
                Properties.Settings.Default.MihoyoUserID = user.GameUid;
                Properties.Settings.Default.MihoyoUser = user.ToString();
            }
            Properties.Settings.Default.Save();
        }

        public async Task<(UserGameRole?, Exception?)> Initialize() {
            return await GetGameRoleInfo("Initialize", true);
        }

        public async Task<(UserGameRole?, Exception?)> GetGameRoleInfo(string source, bool forInit = false) {
            if (string.IsNullOrEmpty(Cookie)) {
                return default;
            }
            if (IsUserRefreshing) {
                Logger.Debug($"DataController.GetGameRoleInfo is refreshing, ignore ({source})");
                return (UserCached, null);
            }
            IsUserRefreshing = true;
            try {
                if (forInit) {
                    UserCached = await Cache.LoadCache2<UserGameRole>();
                    NoteCached = await Cache.LoadCache2<DailyNote>();
                    Logger.Debug($"DataController.GetGameRoleInfo cached uid={UserCached?.GameUid} resin={NoteCached?.CurrentResin}");
                }
                var user = await Api.GetGameRoleInfo();
                Logger.Info($"DataController.GetGameRoleInfo uid={UID} ({source})");
                if (user != null) {
                    SaveUserData(null, user);
                    await Cache.SaveCache2(user);
                }
                return (user, null);
            } catch (TokenException ex) {
                Logger.Error($"DataController.GetGameRoleInfo ({source})", ex);
                ClearUserData();
                return (null, ex);
            } catch (Exception ex) {
                Logger.Error($"DataController.GetGameRoleInfo ({source})", ex);
                return (null, ex);
            } finally {
                IsUserRefreshing = false;
            }
        }

        public async Task<(DailyNote?, Exception?)> GetDailyNote() {
            if (string.IsNullOrEmpty(Cookie)) {
                return default;
            }
            if (IsNoteRefreshing) {
                Logger.Debug($"DataController.GetDailyNote is refreshing, ignore");
                return (NoteCached, null);
            }
            IsNoteRefreshing = true;
            try {
                var (note, error) = await Api.GetDailyNote();
                Logger.Debug($"DataController.GetDailyNote resin={note?.CurrentResin} error={error?.Message}");
                if (note != null) {
                    NoteCached = note;
                    await Cache.SaveCache2(note);
                }
                return (note, null);
            } catch (TokenException ex) {
                Logger.Error("DataController.GetDailyNote", ex);
                ClearUserData();
                return (null, ex);
            } catch (Exception ex) {
                Logger.Error("DataController.GetDailyNote", ex);
                return (null, ex);
            } finally {
                IsNoteRefreshing = false;
            }
        }

        private async Task<(bool, string)> PostSignRewardReal() {
            if (string.IsNullOrEmpty(Cookie)) {
                return (false, "未登录或Cookie失效");
            }
            var (resp, error) = await Api.PostSignReward();
            Logger.Debug($"DataController.PostSignReward sign={resp} error={error?.Message}");
            if (resp == null || error != null) {
                return (false, $"{error}");
            }
            if (resp.Data == null) {
                return resp.ReturnCode switch {
                    0 => (true, "签到成功"),
                    -5003 => (true, resp.Message!),
                    _ => (false, resp.Message!),
                };
            } else {
                if (resp.Data.RiskCode == 0 && resp.Data.Success == 0) {
                    return (true, "签到成功");
                } else {
                    return (false, $"[RiskCode: {resp.Data.RiskCode} | Success: {resp.Data.Success}] 帐号受到风控，请稍后重试");
                }
            }
        }

        public async Task<(bool, string, Response<SignInInfo>?)> PostSignReward() {
            var (extra, _) = await Api.GetSignReward();
            if (extra?.Data?.IsSign == true) {
                // already signed today
                return (true, "已经签到过了哦", extra);
            }
            var (success, message) = await PostSignRewardReal();
            return (success, message, extra);
        }

        public static async Task<UserGameRole?> ValidateCookie(string tempCookie) {
            // use temp api instance to verify cookie
            GenshinAPI tempApi = new GenshinAPI(tempCookie);
            try {
                var user = await tempApi.GetGameRoleInfo();
                Logger.Info($"ValidateCookie uid={user?.GameUid}");
                return user;
            } catch (Exception ex) {
                Logger.Info($"ValidateCookie {ex.Message}");
                return null;
            }
        }
    }
}
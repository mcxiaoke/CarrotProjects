using System;
using System.Threading.Tasks;
using CarrotCommon;
using GenshinLib;
using Newtonsoft.Json;

namespace GenshinNotifier {

    public sealed class DataController {

        // https://csharpindepth.com/articles/singleton
        public static DataController Default { get { return lazy.Value; } }

        private static readonly Lazy<DataController> lazy =
        new Lazy<DataController>(() => new DataController());

        private readonly GenshinAPI Api;
        public readonly CacheManager Cache;
        public UserGameRole UserCached { get; private set; }
        public DailyNote NoteCached { get; private set; }

        private string _uid;

        public string UID {
            get => User?.GameUid ?? _uid;
            set => _uid = value;
        }

        public string Cookie {
            get => Api.Cookie;
            private set {
                Api.Cookie = value;
            }
        }

        private UserGameRole User {
            get => Api.User; set {
                Api.User = value;
                Cache.Name = value?.GameUid;
            }
        }

        public bool Ready { get { return this.Api.Ready; } }

        private DataController() {
            this.Cache = new CacheManager();
            this.Api = new GenshinAPI();
            LoadUserData();
        }

        private void LoadUserData() {
            var cookie = Properties.Settings.Default.MihoyoCookie;
            var userJson = Properties.Settings.Default.MihoyoUser;
            var cookieValid = Utility.ValiteCookieFields(cookie);
            UserGameRole user;
            bool userValid;
            try {
                user = JsonConvert.DeserializeObject<UserGameRole>(userJson);
                userValid = !String.IsNullOrEmpty(user.GameUid) && !String.IsNullOrEmpty(user.GameBiz);
            } catch (Exception) {
                user = null;
                userValid = false;
            }
            if (cookieValid && userValid) {
                this.Cookie = cookie;
                this.User = user;
                this.UserCached = user;
                Logger.Debug($"LoadUserData uid={UID} cookie={cookie}");
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

        public void SaveUserData(string cookie, UserGameRole user = null) {
            Logger.Debug($"SaveUserData cookie={cookie} uid={user?.GameUid}");
            if (!String.IsNullOrEmpty(cookie)) {
                this.Cookie = cookie;
                Properties.Settings.Default.MihoyoCookie = cookie;
            }
            if (user != null) {
                this.User = user;
                this.UserCached = user;
                Properties.Settings.Default.MihoyoUserID = user.GameUid;
                Properties.Settings.Default.MihoyoUser = user.ToString();
            }
            Properties.Settings.Default.Save();
        }

        public async Task<(UserGameRole, Exception)> Initialize() {
            return await GetGameRoleInfo(true);
        }

        public async Task<(UserGameRole, Exception)> GetGameRoleInfo(bool forInit = false) {
            if (string.IsNullOrEmpty(Cookie)) {
                return default;
            }
            try {
                if (forInit) {
                    UserCached = await Cache.LoadCache2<UserGameRole>();
                    NoteCached = await Cache.LoadCache2<DailyNote>();
                    Logger.Debug($"DataController.GetGameRoleInfo cached uid={UserCached?.GameUid} resin={NoteCached?.CurrentResin}");
                }
                var user = await Api.GetGameRoleInfo();
                Logger.Info($"DataController.GetGameRoleInfo uid={UID}");
                if (user != null) {
                    SaveUserData(null, user);
                    await Cache.SaveCache2(user);
                }
                return (user, null);
            } catch (TokenException ex) {
                Logger.Error("DataController.GetGameRoleInfo", ex);
                ClearUserData();
                return (null, ex);
            } catch (Exception ex) {
                Logger.Error("DataController.GetGameRoleInfo", ex);
                return (null, ex);
            }
        }

        public async Task<(DailyNote, Exception)> GetDailyNote() {
            if (string.IsNullOrEmpty(Cookie)) {
                return default;
            }
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
            }
        }

        public async Task<(int, string, Exception)> PostSignReward() {
            if (string.IsNullOrEmpty(Cookie)) {
                return default;
            }
            var (result, error) = await Api.PostSignReward();
            Logger.Debug($"DataController.PostSignReward sign={result} error={error?.Message}");
            dynamic obj = JsonConvert.DeserializeObject(result);
            // retcode == 0 success sign
            // retcode == -5003 already sign
            if (obj.retcode == 0 || obj.retcode == -5003) {
                (result, error) = await Api.GetSignReward();
                Logger.Debug($"DataController.PostSignReward reward={result} error={error?.Message}");
                dynamic robj = JsonConvert.DeserializeObject(result);
                if (robj.retcode == 0) {
                    return (obj.retcode, result, error);
                }
            }
            return (obj.retcode, result, error);
        }

        public static async Task<UserGameRole> ValidateCookie(string tempCookie) {
            // use temp api instance to verify cookie
            GenshinAPI tempApi = new GenshinAPI(tempCookie);
            try {
                var user = await tempApi.GetGameRoleInfo();
                Logger.Info($"ValidateCookie uid={user.GameUid}");
                return user;
            } catch (Exception ex) {
                Logger.Info($"ValidateCookie {ex.Message}");
                return null;
            }
        }
    }
}
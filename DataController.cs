using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenshinNotifier.Net;
using System.IO;
using Newtonsoft.Json;
using System.Net;

namespace GenshinNotifier {
    public sealed class DataController {
        public static DataController Default = new DataController();

        private readonly API Api;
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
            this.Api = new API();
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
            if (string.IsNullOrEmpty(Cookie)) {
                return default;
            }
            UserCached = await Cache.LoadCache2<UserGameRole>();
            NoteCached = await Cache.LoadCache2<DailyNote>();
            Logger.Debug($"DataController.Initialize cached uid={UserCached?.GameUid} resin={NoteCached?.CurrentResin}");
            try {
                if (string.IsNullOrEmpty(Cookie)) {
                    throw new TokenException("No Cookie");
                }
                var user = await Api.GetGameRoleInfo();
                Logger.Info($"DataController.Initialize uid={UID}");
                if (user != null) {
                    SaveUserData(null, user);
                    await Cache.SaveCache2(user);
                }
                return (user, null);
            } catch (TokenException ex) {
                Logger.Error("Initialize", ex);
                ClearUserData();
                return (null, ex);
            } catch (Exception ex) {
                Logger.Error("Initialize", ex);
                return (null, ex);
            }
        }

        public async Task<(UserGameRole, DailyNote)> GetDailyNote() {

            try {
                var (note, error) = await Api.GetDailyNote();
                Logger.Debug($"GetDailyNote resin={note?.CurrentResin} error={error?.Message}");
                if (note != null) {
                    NoteCached = note;
                    await Cache.SaveCache2(note);
                }
                return (this.User, note);
            } catch (TokenException ex) {
                Logger.Error("GetDailyNote", ex);
                ClearUserData();
                return (this.User, null);
            } catch (Exception ex) {
                Logger.Error("GetDailyNote", ex);
                return (this.User, null);
            }
        }

        public static async Task<UserGameRole> ValidateCookie(string tempCookie) {
            // use temp api instance to verify cookie
            API tempApi = new API(tempCookie);
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

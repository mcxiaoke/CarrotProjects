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

        public UserGameRole User {
            get => Api.User; private set {
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
                Properties.Settings.Default.MihoyoUserID = user.GameUid;
                Properties.Settings.Default.MihoyoUser = user.ToString();
            }
            Properties.Settings.Default.Save();
        }

        public async Task<(UserGameRole, Exception)> Initialize() {
            if (String.IsNullOrEmpty(Cookie)) {
                return default;
            }
            try {
                if (String.IsNullOrEmpty(Cookie)) {
                    throw new TokenException("No Cookie");
                }
                var user = await Api.GetGameRoleInfo();
                Logger.Info($"Initialize uid={UID}");
                if (user != null) {
                    await Cache.SaveCache2(user);
                    SaveUserData(this.Cookie, user);
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
                Logger.Info($"GetDailyNote resin={note?.CurrentResin} error={error?.Message}");
                if (note != null) {
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

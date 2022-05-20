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

namespace GenshinNotifier {
    internal class DataController {
        public static DataController Default = new DataController();

        private readonly API Api;
        public readonly CacheManager Cache;

        private string _uid;
        public string UID {
            get { return User?.GameUid ?? _uid; }
            set => _uid = value;
        }

        public string Cookie {
            get { return Api.Cookie; }
            set { this.Api.Cookie = value; }
        }

        public UserGameRole User {
            get { return Api.User; }
            set { this.Api.User = value; }
        }

        public bool Ready { get { return this.Api.Ready; } }

        private DataController() {
            var cookie = Properties.Settings.Default.MihoyoCookie;
            var uid = Properties.Settings.Default.MihoyoUserID;
            this.Api = new API(cookie);
            this.Cache = new CacheManager(uid);
            this.UID = uid;
        }

        public void ClearUserData() {
            Logger.Info("ClearUserData");
            this.Cookie = null;
            this.User = null;
            this.Cache.Name = null;
            Properties.Settings.Default.MihoyoCookie = null;
            Properties.Settings.Default.MihoyoUserID = null;
            Properties.Settings.Default.MihoyoUser = null;
            Properties.Settings.Default.Save();
        }

        public void SaveUserData(string cookie, UserGameRole user) {
            Logger.Info($"SaveUserData for {user.GameUid}");
            this.Cookie = cookie;
            this.User = user;
            this.Cache.Name = user.GameUid;
            Properties.Settings.Default.MihoyoCookie = cookie;
            Properties.Settings.Default.MihoyoUserID = user.GameUid;
            Properties.Settings.Default.MihoyoUser = user.ToString();
            Properties.Settings.Default.Save();
        }

        public async Task<(UserGameRole, Exception)> Initialize() {
            try {
                var user = await Api.GetGameRoleInfo();
                Logger.Info($"Initialize uid={user?.GameUid}");
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
                Logger.Error("ValidateCookie", ex);
                return null;
            }
        }

    }
}

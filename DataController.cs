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

        public static string GetDailyNoteCacheName(string uid) {
            var root = Application.UserAppDataPath;
            var path= Path.Combine(root, $"{uid}-dailyNote-cache.json");
            Logger.Debug($"GetDailyNoteCacheName {path}");
            return path;
        }

        public static async Task<DailyNote> GetDailyNoteCache(string uid) {
            try {
                var path = GetDailyNoteCacheName(uid);
                if (!File.Exists(path)) {
                    return null;
                }
                var json = await Task.Run(() => File.ReadAllText(path));
                return JsonConvert.DeserializeObject<DailyNote>(json);
            } catch (Exception ex) {
                Logger.Error($"GetDailyNoteCache {uid} error={ex.Message}");
                return null;
            }
        }

        public static async Task SetDailyNoteCache(string uid, DailyNote note) {
            Logger.Debug($"SetDailyNoteCache for {uid}");
            try {
                var path = GetDailyNoteCacheName(uid);
                await Task.Run(() => File.WriteAllText(path, note.ToString()));
            } catch (Exception ex) {
                Logger.Error($"SetDailyNoteCache {uid} error={ex.Message}");

            }
        }


        public static DataController Default = new DataController();

        private readonly API Api;

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
            this.Api = new API();
        }

        public void ClearCookie() {
            Logger.Info("ClearCookie");
            this.Cookie = null;
            this.User = null;
            Properties.Settings.Default.MihoyoCookie = null;
            Properties.Settings.Default.MihoyoUser = null;
            Properties.Settings.Default.Save();
        }

        public void SaveCookie(string cookie, UserGameRole user) {
            Logger.Info($"SaveCookie for {user.GameUid}");
            this.Cookie = cookie;
            this.User = user;
            Properties.Settings.Default.MihoyoCookie = cookie;
            Properties.Settings.Default.MihoyoUser = user.ToString();
            Properties.Settings.Default.Save();
        }

        public async Task<UserGameRole> Initialize() {
            this.Cookie = Properties.Settings.Default.MihoyoCookie;
            try {
                var (user, error) = await Api.GetGameRoleInfo();
                Logger.Info($"GetGameRoleInfo data={user?.GameUid} error={error?.Message}");
                if (user != null) {
                    SaveCookie(this.Cookie, user);
                }
                return user;
            } catch (Exception ex) {
                Logger.Error("GetGameRoleInfo", ex);
                ClearCookie();
                return null;
            }
        }

        public async Task<(UserGameRole, DailyNote)> GetDailyNote() {

            try {
                var (note, error) = await Api.GetDailyNote();
                Logger.Info($"GetDailyNote resin={note?.CurrentResin} error={error?.Message}");
                if (note != null) {
                    await SetDailyNoteCache(this.User.GameUid, note);
                }
                return (this.User, note);
            } catch (Exception ex) {
                Logger.Error("GetDailyNote", ex);
                return (null, null);
            }
        }

        public static async Task<UserGameRole> ValidateCookie(string tempCookie) {
            // use temp api instance to verify cookie
            API tempApi = new API(tempCookie);
            try {
                var (user, error) = await tempApi.GetGameRoleInfo();
                Logger.Info($"ValidateCookie uid={user.GameUid} error={error?.Message}");
                return user;
            } catch (Exception ex) {
                Logger.Error("ValidateCookie", ex);
                return null;
            }
        }

    }
}

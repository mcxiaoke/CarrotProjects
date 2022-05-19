using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GenshinNotifier.Net;

namespace GenshinNotifier {
    internal class DataController {

        public static DataController Default = new DataController();

        private readonly API Api;

        public string Cookie {
            get { return Api.Cookie; }
            set {
                if (value != this.Api.Cookie) {
                    this.Api.Cookie = value;
                    CookieChanged = true;
                }
            }
        }

        public bool CookieChanged { get; set; }

        public bool Ready { get { return this.Api.Ready; } }

        private DataController() {
            this.Api = new API();
        }


        public async Task<UserGameRole> Initialize() {
            this.Api.Cookie = Properties.Settings.Default.MihoyoCookie;
            try {
                var (user, error) = await Api.GetGameRoleInfo();
                Logger.Info($"GetGameRoleInfo data={user?.GameUid} error={error?.Message}");
                return user;
            } catch (Exception ex) {
                Logger.Error("GetGameRoleInfo", ex);
                return null;
            }
        }

        public async Task<(UserGameRole, DailyNote)> GetDailyNote() {

            try {
                var (data, error) = await Api.GetDailyNote();
                Logger.Info($"GetDailyNote data={data?.CurrentResin} error={error?.Message}");
                return (Api.User, data);
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
                Logger.Info($"ValidateCookie data={user} error={error?.Message}");
                return user;
            } catch (Exception ex) {
                Logger.Error("ValidateCookie", ex);
                return null;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GenshinNotifier.Net;

namespace GenshinNotifier {
    internal class DataController {
        private readonly API Api;

        public DataController() : this(AppConfig.COOKIE) { }

        public DataController(string cookie) {
            this.Api = new API(cookie);
        }

        public async Task<(UserGameRole, DailyNote)> FetchData() {

            try {
                var (data, error) = await Api.GetDailyNote();
                Logger.Info($"FetchData data={data?.CurrentResin} error={error?.StackTrace}");
                return (Api.User, data);
            } catch (Exception ex) {
                Logger.Error("FetchData", ex);
                return (null, null);
            }

        }

    }
}

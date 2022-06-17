using System;
using System.Diagnostics;
using Carrot.ProCom.Common;
using Carrot.ProCom.Pipe;
using GenshinLib;
using CarrotCommon;
using Newtonsoft.Json;

namespace GenshinNotifier {

    internal class DataBox {
        public UserGameRole User { get; set; }
        public DailyNote Note { get; set; }
    }

    internal static class Service {
        public const string PIPE_NAME = ProComConst.PIPE_MAIN;

        // from Carrot Notifier AppService.cs
        public static string CmdShowWindow = $"{ProComConst.CMD_PREFIX}/action/showWindow";

        public static string CmdDailyNoteInfo = $"{ProComConst.CMD_PREFIX}/api/dailyNote/info";
        public static string CmdDailyNoteRefresh = $"{ProComConst.CMD_PREFIX}/api/dailyNote/refresh";

        public static void Refresh() {
            try {
                var (response, error) = PipeService.SendAndReceive(PIPE_NAME, CmdDailyNoteRefresh);
                Logger.Debug($"GetData response={response}");
                return;
            } catch (Exception ex) {
                Logger.Debug($"GetData error={ex.StackTrace}");
                return;
            }
        }

        public static DataBox GetData() {
            try {
                var (response, error) = PipeService.SendAndReceive(PIPE_NAME, CmdDailyNoteInfo);
                Logger.Debug($"GetData response={response}");
                if (response == null) {
                    return default;
                }
                return JsonConvert.DeserializeObject<DataBox>(response);
            } catch (Exception ex) {
                Logger.Debug($"GetData error={ex.StackTrace}");
                return default;
            }
        }
    }
}
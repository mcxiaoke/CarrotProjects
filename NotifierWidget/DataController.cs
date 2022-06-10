using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Carrot.ProCom.Common;
using Carrot.ProCom.Pipe;
using CarrotCommon;
using GenshinLib;
using Newtonsoft.Json;

namespace NotifierWidget {

    internal class DataBox {
        public UserGameRole User { get; set; }
        public DailyNote Note { get; set; }
    }

    internal static class Service {

        public const string MAIN_APP_GUID = "{82761839-E200-402E-8C1D-2FDE9571239C}";
        public const string PIPE_NAME = MAIN_APP_GUID;

        // from Carrot Notifier AppService.cs
        public static string CmdShowWindow = $"{Const.CMD_PREFIX}/action/showWindow";
        public static string CmdDailyNoteInfo = $"{Const.CMD_PREFIX}/api/dailyNote/info";
        public static string CmdDailyNoteRefresh = $"{Const.CMD_PREFIX}/api/dailyNote/refresh";

        public static void Refresh() {
            try {
                var (response, error) = PipeService.SendAndReceive(PIPE_NAME, CmdDailyNoteRefresh);
                Debug.WriteLine($"GetData response={response}");
                return;
            } catch (Exception ex) {
                Debug.WriteLine($"GetData error={ex.StackTrace}");
                return;
            }
        }

        public static DataBox GetData() {
            try {
                var (response, error) = PipeService.SendAndReceive(PIPE_NAME, CmdDailyNoteInfo);
                Debug.WriteLine($"GetData response={response}");
                if (response == null) {
                    return default;
                }
                return JsonConvert.DeserializeObject<DataBox>(response);

            } catch (Exception ex) {
                Debug.WriteLine($"GetData error={ex.StackTrace}");
                return default;
            }
        }
    }
}

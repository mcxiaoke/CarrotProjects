﻿using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading.Tasks;
using Carrot.ProCom.Common;
using Carrot.ProCom.Pipe;
using Carrot.Common;

namespace GenshinNotifier {

    internal static class AppService {
        public static MainForm? appMainForm;

        public static void Start() {
            PipeService.Default.Handlers += OnPipeMessage;
            PipeService.Default.MessageHandler = InterceptPipeMessage;
            Logger.Debug($"AppService.Start {ProComConst.PIPE_MAIN}");
            PipeService.Default.StartServer(ProComConst.PIPE_MAIN);
        }

        public static void Stop() {
            appMainForm = null;
            PipeService.Default.MessageHandler = null;
            PipeService.Default.Handlers -= OnPipeMessage;
            PipeService.Default.StopServer();
        }

        public static void SendCmdShowWindow() {
            Logger.Debug("SendCmdShowWindow");
            PipeService.SendAndReceive(ProComConst.PIPE_MAIN, CmdShowWindow);
        }

        public static string CmdShowWindow = $"{ProComConst.CMD_PREFIX}/action/showWindow";
        public static string CmdDailyNoteInfo = $"{ProComConst.CMD_PREFIX}/api/dailyNote/info";
        public static string CmdDailyNoteRefresh = $"{ProComConst.CMD_PREFIX}/api/dailyNote/refresh";

        private static bool InterceptPipeMessage(NamedPipeServerStream server, string message) {
            Logger.Debug($"InterceptPipeMessage message={message}");
            if (string.IsNullOrEmpty(message) || !message.StartsWith(ProComConst.CMD_PREFIX)) {
                return false;
            }
            if (string.Compare(message, CmdDailyNoteInfo, true) == 0) {
                Logger.Debug("InterceptPipeMessage CmdDailyNoteInfo");
                //  dailynote info cmd, return cached user and dailynote
                var user = DataController.Default.UserCached;
                var note = DataController.Default.NoteCached;
                var obj = new { note, user };
                var json = Utility.Stringify(obj);
                var stream = new StreamString(server);
                stream.WriteString(json);
                return true;
            } else if (string.Compare(message, CmdDailyNoteRefresh, true) == 0) {
                Logger.Debug("InterceptPipeMessage CmdDailyNoteRefresh");
                // dailynote refresh cmd, refresh user and dailynote
                Task.Run(async () => {
                    await DataController.Default.GetDailyNote();
                    //await DataController.Default.GetGameRoleInfo("InterceptPipeMessage");
                });
                return true;
            } else if (string.Compare(message, CmdShowWindow, true) == 0) {
                Logger.Debug("InterceptPipeMessage CmdShowWindow");
                appMainForm?.ShowMyWindow(message, EventArgs.Empty);
                return true;
            }

            return false;
        }

        private static void OnPipeMessage(object sender, EventArgs e) {
            try {
                Logger.Debug($"OnPipeMessage sender={sender} e={e}");
                var args = e as PipeServiceEventArgs;
                if (args?.Failed == true) {
                    Logger.Debug($"OnPipeMessage failed={args.Error}");
                } else {
                    Logger.Debug($"OnPipeMessage message={args?.Message}");
                }
            } catch (Exception ex) {
                Logger.Debug($"OnPipeMessage error={ex.Message}");
            }
        }
    }
}
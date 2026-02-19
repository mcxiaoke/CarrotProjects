namespace Carrot.ProCom.Common {

    /// <summary>
    /// \class ProComConst
    /// 定义跨进程通信的常量。
    /// </summary>
    public static class ProComConst {
        // 响应状态：成功
        public const string RES_OK = "@OK@";
        // 响应状态：错误
        public const string RES_ERR = "@ERR@";
        // 命令前缀
        public const string CMD_PREFIX = "@CMD@";

        // 主管道名称
        public const string PIPE_MAIN = "{82761839-E200-402E-8C1D-2FDE9571239C}";
        // 小部件管道名称
        public const string PIPE_WIDGET = "{82761839-E300-402E-8C1D-2FDE9571239C}";
    }
}
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Carrot.ProCom.Common {

    public static class Const {
        public const string RES_OK = "@OK@";
        public const string RES_ERR = "@ERR@";
        public const string CMD_PREFIX = "@CMD@";

        public static Guid AppGuid {
            get {
                Assembly asm = Assembly.GetEntryAssembly();
                object[] attr = (asm.GetCustomAttributes(typeof(GuidAttribute), true));
                return new Guid((attr[0] as GuidAttribute).Value);
            }
        }

        public static string AppGuidStr => AppGuid.ToString("B").ToUpper();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinNotifier.Net {

    public class Const {
        public const string HK4E_APK = "https://hk4e-api.mihoyo.com";
        public const string BBS_API = "https://bbs-api.mihoyo.com";
        public const string TAKUMI_API = "https://api-takumi.mihoyo.com";
        public const string TAKUMI_RECORD_API = "https://api-takumi-record.mihoyo.com";
        // 2.27.2 dailyNote worked version must>2.11.0
        public const string MHY_SALT_NEW = "xV8v4Qu54lUKrEYFZkJhB8cuOh9Asafs";//new
        public const string MHY_VER_NEW = "2.27.2";
        public const string MHY_SIGN_ACT_ID = "e202009291139501";

        // another salt 4a8knnbk5pbjqsrudp3dq484m9axoc5g // old
        // another salt h8w582wxwgqvahcdkpvdhbh2w9casgfl // mweb old
        //2.7.x sign worked for simple ds, sign api
        public const string MHY_SALT_OLD = "14bmu1mz0yuljprsfgpvjh3ju2ni468r"; // mweb
        public const string MHY_VER_OLD = "2.7.0";
    };
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace CarrotCommon {
    public class Utility {

        public static Random GlobalRandom = new Random();

        public static string GetRandomString2() {
            return GlobalRandom.Next(100000, 200000).ToString();
        }

        public static string GetRandomString(int length) {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[GlobalRandom.Next(s.Length)]).ToArray());
        }

        public static string Stringify(object value, bool indented = false) {
            if (value == null) { return ""; }
            var jst = new JsonSerializerSettings() {
                DateFormatString = "yyyy'-'MM'-'dd' 'HH':'mm':'ss.FFFFFFFK",
                Formatting = indented ? Formatting.Indented : Formatting.None,
            };
            return JsonConvert.SerializeObject(value, jst);
        }

        public static bool ValiteCookieFields(string value) {
            var cookieDict = ParseCookieString(value);
            var validKeys = new string[] { "cookie_token", "login_ticket", "account_id" };
            return validKeys.Any(it => cookieDict.ContainsKey(it));
        }

        public static Dictionary<string, string> ParseCookieString(string str) {
            var cookieDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var values = str.TrimEnd(';').Split(';');
            foreach (var parts in values.Select(c => c.Split(new[] { '=' }, 2))) {
                var cookieName = parts[0].Trim();
                string cookieValue;

                if (parts.Length == 1) {
                    //Cookie attribute
                    cookieValue = string.Empty;
                } else {
                    cookieValue = parts[1];
                }

                cookieDictionary[cookieName] = cookieValue;
            }

            return cookieDictionary;
        }

        // Form data (for GET or POST) is usually encoded as application/x-www-form-urlencoded: this specifies + for spaces.
        // URLs are encoded as RFC 1738 which specifies %20.

        public static string CreateQueryString(IDictionary<string, string> dict) {
            if (dict == null || dict.Count == 0) { return String.Empty; }
            return string.Join("&", dict.OrderBy(x => x.Key).Select(kvp =>
               string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value))));
        }

        public static string CreateQueryString2(IDictionary<string, string> dict) {
            if (dict == null || dict.Count == 0) { return String.Empty; }
            var sb = new StringBuilder();
            foreach (var kvp in dict.OrderBy(x => x.Key)) {
                sb.Append(Uri.EscapeDataString(kvp.Key)).Append("=").Append(Uri.EscapeDataString(kvp.Value)).Append("&");
            }
            sb.Length--;
            return sb.ToString();
        }


        public static Dictionary<string, string> ParseQueryString(String query) {
            Dictionary<String, String> queryDict = new Dictionary<string, string>();
            foreach (String token in query.TrimStart(new char[] { '?' }).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)) {
                string[] parts = token.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                    queryDict[parts[0].Trim()] = Uri.EscapeDataString(parts[1]).Trim();
                else
                    queryDict[parts[0].Trim()] = "";
            }
            return queryDict;
        }

        public static bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash) {
            // Hash the input.
            var hashOfInput = GetHash(hashAlgorithm, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return comparer.Compare(hashOfInput, hash) == 0;
        }

        public static string GetHash(HashAlgorithm hashAlgorithm, string input) {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++) {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static string GetMD5Hash(string input) {
            return GetHash(MD5.Create(), input);
        }

        public static string GetComputedMd5(string source) {
            using (MD5 md5 = MD5.Create()) {
                byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(source));

                StringBuilder builder = new StringBuilder();

                foreach (byte b in result) {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }

    }
}

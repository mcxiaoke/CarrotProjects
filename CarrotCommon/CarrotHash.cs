using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace CarrotCommon {

    public static class CarrotHash {

        public static string ToHexString(byte[] array, bool toUpper = false, bool grouping = false) {
            var sb = new StringBuilder();
            for (int i = 0; i < array.Length; i++) {
                sb.Append($"{array[i]:x2}");
                if ((i % 4) == 3 && grouping) {
                    sb.Append(" ");
                }
            }
            return toUpper ? sb.ToString().ToUpper() : sb.ToString();
        }

        public static string FileHash(string filepath, HashAlgorithm hasher, bool toUpper = false) {
            var fInfo = new FileInfo(filepath);
            using (FileStream stream = fInfo.Open(FileMode.Open)) {
                try {
                    // Create a fileStream for the file.
                    // Be sure it's positioned to the beginning of the stream.
                    stream.Position = 0;
                    // Compute the hash of the fileStream.
                    byte[] hashValue = hasher.ComputeHash(stream);
                    return ToHexString(hashValue, toUpper);
                } catch (IOException e) {
                    Logger.Warning($"GetHash I/O Exception: {e.Message}");
                } catch (UnauthorizedAccessException e) {
                    Logger.Warning($"GetHash Access Exception: {e.Message}");
                }
            }
            return null;
        }


        public static string FileMD5(string filepath, bool toUpper = false) {
            using (HashAlgorithm hasher = MD5.Create()) {
                return FileHash(filepath, hasher, toUpper);
            }
        }

        public static string FileSHA1(string filepath, bool toUpper = false) {
            using (HashAlgorithm hasher = SHA1.Create()) {
                return FileHash(filepath, hasher, toUpper);
            }
        }

        public static string FileSHA256(string filepath, bool toUpper = false) {
            using (HashAlgorithm hasher = SHA256.Create()) {
                return FileHash(filepath, hasher, toUpper);
            }
        }


        public static string FileSHA512(string filepath, bool toUpper = false) {
            using (HashAlgorithm hasher = SHA512.Create()) {
                return FileHash(filepath, hasher, toUpper);
            }
        }

        public static string GetHash(byte[] buffer, HashAlgorithm hasher, bool toUpper = false) {
            try {
                byte[] hashValue = hasher.ComputeHash(buffer);
                return ToHexString(hashValue, toUpper);
            } catch (IOException e) {
                Logger.Warning($"GetHash I/O Exception: {e.Message}");
            } catch (UnauthorizedAccessException e) {
                Logger.Warning($"GetHash Access Exception: {e.Message}");
            }
            return null;
        }

        public static string GetHash(string text, HashAlgorithm hasher, bool toUpper = false) {
            return GetHash(Encoding.UTF8.GetBytes(text), hasher, toUpper);
        }


        public static string GetMD5(string text, bool toUpper = false) {
            using (HashAlgorithm hasher = MD5.Create()) {
                return GetHash(text, hasher, toUpper);
            }
        }

        public static string GetSHA1(string text, bool toUpper = false) {
            using (HashAlgorithm hasher = SHA1.Create()) {
                return GetHash(text, hasher, toUpper);
            }
        }

        public static string GetSHA256(string text, bool toUpper = false) {
            using (HashAlgorithm hasher = SHA256.Create()) {
                return GetHash(text, hasher, toUpper);
            }
        }


        public static string GetSHA512(string text, bool toUpper = false) {
            using (HashAlgorithm hasher = SHA512.Create()) {
                return GetHash(text, hasher, toUpper);
            }
        }



    }
}

using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;

namespace GTAIVDowngrader {

    #region Public Enums
    public enum FileSizes
    {
        Byte,
        Kilobyte,
        Megabyte,
        Gigabyte,
        Terabyte,
        Petabyte,
        Exabyte,
    }
    #endregion

    internal class Helper {

        #region Structs
        public struct FileSize
        {
            #region Properties
            public FileSizes FileSizes { get; private set; }
            public double Size { get; private set; }
            #endregion

            #region Constructor
            internal FileSize(double size, FileSizes fileSizes)
            {
                FileSizes = fileSizes;
                Size = size;
            }
            #endregion
        }
        #endregion

        #region Extensions
        public class ParseExtension
        {
            public static bool Parse(string s, bool defaultValue = false)
            {
                bool result;
                bool parseResult = bool.TryParse(s, out result);
                if (parseResult) {
                    return result;
                }
                return defaultValue;
            }

            public static int Parse(string s, int defaultValue = 0)
            {
                int result;
                bool parseResult = int.TryParse(s, out result);
                if (parseResult) {
                    return result;
                }
                return defaultValue;
            }

            public static ushort Parse(string s, ushort defaultValue = 0)
            {
                ushort result;
                bool parseResult = ushort.TryParse(s, out result);
                if (parseResult) {
                    return result;
                }
                return defaultValue;
            }

            public static double Parse(string s, double defaultValue = 0)
            {
                double result;
                bool parseResult = double.TryParse(s, out result);
                if (parseResult) {
                    return result;
                }
                return defaultValue;
            }

            public static float Parse(string s, float defaultValue = 0f)
            {
                float result;
                bool parseResult = float.TryParse(s, out result);
                if (parseResult) {
                    return result;
                }
                return defaultValue;
            }
        }
        #endregion

        #region MD5
        public static byte[] GetMD5Hash(string file)
        {
            try {
                using (MD5 md5 = MD5.Create()) {
                    using (var stream = File.OpenRead(file)) {
                        return md5.ComputeHash(stream);
                    }
                }
            }
            catch (Exception) {
                return new byte[0];
            }
        }
        #endregion

        public static FileSize GetExactFileSize(long byteCount)
        {
            try {
                if (byteCount == 0)
                    return new FileSize(0, FileSizes.Byte);

                long bytes = Math.Abs(byteCount);
                int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
                double num = Math.Round(bytes / Math.Pow(1024, place), 1);
                return new FileSize((Math.Sign(byteCount) * num) * 1024, (FileSizes)place);
            }
            catch (Exception) { }
            return new FileSize(0, FileSizes.Byte);
        }

        public static double GetVRAM()
        {
            try {
                using (ManagementClass c = new ManagementClass("Win32_VideoController")) {
                    foreach (ManagementObject o in c.GetInstances()) {
                        FileSize size = GetExactFileSize(long.Parse(o["AdapterRam"].ToString()));
                        return size.Size;
                    }
                }
            }
            catch (Exception) { }
            return 1024;
        }

    }
}

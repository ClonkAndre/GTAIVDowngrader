using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEncrypter {
    internal static class Helper {

        #region Classes
        /// <summary>
        /// Compression stuff.
        /// </summary>
        public class DataCompression {

            #region Byte Array
            /// <summary>
            /// Compresses and returns a byte array.
            /// </summary>
            /// <param name="uncompressedByteArray">Byte array to compress</param>
            public static byte[] CompressByteArray(byte[] uncompressedByteArray, CompressionLevel compressionLevel = CompressionLevel.Optimal)
            {
                try {
                    byte[] compressedBytes;

                    using (var uncompressedStream = new MemoryStream(uncompressedByteArray)) {
                        using (var compressedStream = new MemoryStream()) {
                            using (var compressorStream = new DeflateStream(compressedStream, compressionLevel, true)) {
                                uncompressedStream.CopyTo(compressorStream);
                            }
                            compressedBytes = compressedStream.ToArray();
                        }
                    }

                    return compressedBytes;
                }
                catch (Exception) {
                    return null;
                }
            }

            /// <summary>
            /// Decompresses and returns a compressed byte array.
            /// </summary>
            /// <param name="compressedByteArray">Byte array to decompress.</param>
            public static byte[] DecompressByteArray(byte[] compressedByteArray)
            {
                try {
                    byte[] decompressedBytes;

                    var compressedStream = new MemoryStream(compressedByteArray);

                    using (var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress)) {
                        using (var decompressedStream = new MemoryStream()) {
                            decompressorStream.CopyTo(decompressedStream);

                            decompressedBytes = decompressedStream.ToArray();
                        }
                    }

                    return decompressedBytes;
                }
                catch (Exception) {
                    return null;
                }
            }
            #endregion

            #region String
            /// <summary>
            /// Compresses a string and returns a deflate compressed, Base64 encoded string.
            /// </summary>
            /// <param name="uncompressedString">String to compress</param>
            public static string CompressString(string uncompressedString, string returnStringOnError = "")
            {
                try {
                    byte[] compressedBytes;

                    using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString))) {
                        using (var compressedStream = new MemoryStream()) {
                            using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Optimal, true)) {
                                uncompressedStream.CopyTo(compressorStream);
                            }
                            compressedBytes = compressedStream.ToArray();
                        }
                    }

                    return Convert.ToBase64String(compressedBytes);
                }
                catch (Exception) {
                    return returnStringOnError;
                }
            }

            /// <summary>
            /// Decompresses a deflate compressed, Base64 encoded string and returns an uncompressed string.
            /// </summary>
            /// <param name="compressedString">String to decompress.</param>
            public static string DecompressString(string compressedString, string returnStringOnError = "")
            {
                try {
                    byte[] decompressedBytes;

                    var compressedStream = new MemoryStream(Convert.FromBase64String(compressedString));

                    using (var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress)) {
                        using (var decompressedStream = new MemoryStream()) {
                            decompressorStream.CopyTo(decompressedStream);

                            decompressedBytes = decompressedStream.ToArray();
                        }
                    }

                    return Encoding.UTF8.GetString(decompressedBytes);
                }
                catch (Exception) {
                    return returnStringOnError;
                }
            }
            #endregion

        }
        #endregion

        #region Functions
        public static byte[] GetByteArray(Stream input)
        {
            using (MemoryStream ms = new MemoryStream()) {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
        #endregion

    }
}

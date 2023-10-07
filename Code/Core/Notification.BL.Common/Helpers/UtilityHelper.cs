// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.Common.Helpers
{
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;
    using Notification.BL.Common.Interface;

    public class UtilityHelper : IUtilityHelper
    {
        #region Public Methods

        /// <summary>
        /// Compress Message using GZip compression.
        /// </summary>
        /// <param name="messageBody">message to be compressed</param>
        /// <returns>Compressed message byte array</returns>
        public byte[] CompressMessage(string messageBody)
        {
            var notificationStream = new MemoryStream(Encoding.UTF8.GetBytes(messageBody));
            byte[] compressedBody;

            using (var dataStream = (Stream)notificationStream)
            {
                var compressedStream = new MemoryStream();
                using (var compressionStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    dataStream.CopyTo(compressionStream);
                }
                compressedStream = new MemoryStream(compressedStream.ToArray());
                compressedBody = compressedStream.ToArray();
            }
            return compressedBody;
        }

        /// <summary>
        /// Decompress Message using GZip decompression.
        /// </summary>
        /// <param name="payload">payload to be decompressed</param>
        /// <returns>Decompressed message string</returns>
        public async Task<string> DecompressMessage(byte[] payload)
        {
            string decompressedPayload;
            using (var dataStream = new MemoryStream(payload))
            {
                var decompressedStream = new MemoryStream();
                using (var compressionStream = new GZipStream(dataStream, CompressionMode.Decompress))
                {
                    compressionStream.CopyTo(decompressedStream);
                }

                decompressedStream.Seek(0, SeekOrigin.Begin);
                decompressedPayload = await new StreamReader(decompressedStream).ReadToEndAsync();
            }
            return decompressedPayload;
        }

        #endregion Public Methods
    }
}
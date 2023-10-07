// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Notification.BL.Common.Interface
{
    using System.Threading.Tasks;

    public interface IUtilityHelper
    {
        /// <summary>
        /// Compress Message using GZip compression.
        /// </summary>
        /// <param name="messageBody">message to be compressed</param>
        /// <returns>Compressed message byte array</returns>
        byte[] CompressMessage(string messageBody);

        /// <summary>
        /// Decompress Message using GZip decompression.
        /// </summary>
        /// <param name="payload">payload to be decompressed</param>
        /// <returns>Decompressed message string</returns>
        Task<string> DecompressMessage(byte[] payload);
    }
}
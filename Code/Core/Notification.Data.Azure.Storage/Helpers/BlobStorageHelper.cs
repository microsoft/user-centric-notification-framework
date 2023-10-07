// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Notification.Data.Azure.Storage.Interface;

namespace Notification.Data.Azure.Storage.Helpers
{
    /// <summary>
    /// The BlobStorageHelper class
    /// </summary>
    public class BlobStorageHelper : IBlobStorageHelper
    {
        #region Variables

        private readonly BlobServiceClient _blobServiceClient;

        #endregion Variables

        #region Constructor

        public BlobStorageHelper(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Gets the Blob Uri
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public string GetBlobUri(string containerName, string blobName)
        {
            return string.Format(@"https://{0}.blob.core.windows.net/{1}/{2}", _blobServiceClient.AccountName, containerName, blobName);
        }

        /// <summary>
        /// Check if container name exist at storage account
        /// </summary>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>bool</returns>
        public async Task<bool> DoesExist(string containerName, string blobName)
        {
            return await (_blobServiceClient.GetBlobContainerClient(blobName)).ExistsAsync();
        }

        /// <summary>
        /// List blobs hierarchical listing
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="prefix"></param>
        /// <param name="segmentSize"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public async Task<List<BlobItem>> ListBlobsHierarchicalListing(string containerName, string? prefix, int? segmentSize, BlobContainerClient? container = null)
        {
            if (prefix is null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            container ??= await GetBlobContainerClient(containerName);

            // Call the listing operation and return pages of the specified size.
            var resultSegment = container.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/")
                .AsPages(default, segmentSize);

            List<BlobItem> lstBlobItem = new List<BlobItem>();

            // Enumerate the blobs returned for each page.
            await foreach (Page<BlobHierarchyItem> blobPage in resultSegment)
            {
                // A hierarchical listing may return both virtual directories and blobs.
                foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
                {
                    if (blobhierarchyItem.IsPrefix)
                    {
                        // Write out the prefix of the virtual directory.
                        Console.WriteLine("Virtual directory prefix: {0}", blobhierarchyItem.Prefix);

                        // Call recursively with the prefix to traverse the virtual directory.
                        var blobChildList = await ListBlobsHierarchicalListing(containerName, blobhierarchyItem.Prefix, null, container);
                        lstBlobItem.AddRange(blobChildList);
                    }
                    else
                    {
                        lstBlobItem.Add(blobhierarchyItem.Blob);
                    }
                }
            }
            return lstBlobItem;
        }

        /// <summary>
        /// Save the byte arrray to blob
        /// </summary>
        /// <param name="data">Byte array data</param>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Task</returns>
        public async Task UploadByteArray(byte[] data, string containerName, string blobName)
        {
            MemoryStream stream = new MemoryStream(data);
            await UploadStreamData(stream, containerName, blobName);
        }

        /// <summary>
        /// Save the stream data to blob
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Task</returns>
        public async Task<string> UploadStreamData(Stream stream, string containerName, string blobName)
        {
            var blobContainerClient = await GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            // added overwrite as true to handle race conditions
            await blobClient.UploadAsync(stream, true);

            return string.Empty;
        }

        /// <summary>
        /// Saves text to blob
        /// </summary>
        /// <param name="data">Text data </param>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Task</returns>
        public async Task UploadText(string data, string containerName, string blobName)
        {
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            await UploadStreamData(stream, containerName, blobName);
        }

        /// <summary>
        /// Upload file to blob
        /// </summary>
        /// <param name="files"></param>
        /// <param name="blobName"></param>
        /// <param name="containerName"></param>
        /// <param name="filename"></param>
        public async Task UploadFileToBlob(List<IFormFile> files, string blobName, string containerName, string filename)
        {
            foreach (var file in files)
            {
                var content = string.Empty;

                using (var reader = new StreamReader(file.OpenReadStream()))
                {
                    content = reader.ReadToEnd();
                }
                string mimeType = file.ContentType;
                filename = (string.IsNullOrWhiteSpace(filename)) ? file.FileName : filename;
                var blobFullName = (!string.IsNullOrWhiteSpace(blobName) ? string.Format("{0}/{1}", blobName, filename) : filename);
                await UploadText(content, containerName, blobFullName);
            }
        }

        /// <summary>
        /// Upload file to blob async
        /// </summary>
        /// <param name="strFileName"></param>
        /// <param name="fileData"></param>
        /// <param name="fileMimeType"></param>
        /// <param name="blobFolderName"></param>
        /// <param name="containerName"></param>
        public async Task UploadFileToBlobAsync(string strFileName, string fileData, string fileMimeType, string blobFolderName, string containerName)
        {
            if (strFileName != null && fileData != null)
            {
                var blobName = (!string.IsNullOrWhiteSpace(blobFolderName) ? string.Format("{0}/{1}", blobFolderName, strFileName) : strFileName);
                await UploadText(fileData, containerName, blobName);
            }
        }

        /// <summary>
        /// Upload image file to blob async
        /// </summary>
        /// <param name="strFileName"></param>
        /// <param name="file"></param>
        /// <param name="fileMimeType"></param>
        /// <param name="containerName"></param>
        public async Task UploadImageFileToBlobAsync(string strFileName, IFormFile file, string fileMimeType, string containerName)
        {
            if (strFileName != null)
            {
                await UploadStreamData(file.OpenReadStream(), containerName, strFileName);
            }
        }

        /// <summary>
        /// Download text data from blob
        /// </summary>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>string</returns>
        public async Task<string> DownloadText(string containerName, string blobName)
        {
            // convert byteArray to stream
            byte[] byteArray = (await DownloadStreamData(containerName, blobName)).ToArray();
            MemoryStream stream = new MemoryStream(byteArray);

            // convert stream to string
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Download blob from storage
        /// </summary>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Byte array data</returns>
        public async Task<byte[]> DownloadByteArray(string containerName, string blobName)
        {
            return (await DownloadStreamData(containerName, blobName)).ToArray();
        }

        /// <summary>
        /// Download stream data from blob
        /// </summary>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Memory Stream</returns>
        public async Task<MemoryStream> DownloadStreamData(string containerName, string blobName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blockBlob = containerClient.GetBlobClient(blobName);

            using (var stream = new MemoryStream())
            {
                await blockBlob.DownloadToAsync(stream);
                return stream;
            }
        }

        /// <summary>
        /// Deletes data from blob
        /// </summary>
        /// <param name="containerName">Storage container name</param>
        /// <param name="blobName">Blob name</param>
        /// <returns>Task</returns>
        public async Task DeleteBlob(string containerName, string blobName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blockBlob = containerClient.GetBlobClient(blobName);

            var fileExists = await blockBlob.ExistsAsync();
            if (fileExists)
            {
                await blockBlob.DeleteAsync();
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Get Blob Conatiner
        /// </summary>
        /// <param name="containerName">Storage container name</param>
        /// <returns>CloudBlobContainer</returns>
        private async Task<BlobContainerClient> GetBlobContainerClient(string containerName)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();
            return blobContainerClient;
        }

        #endregion Private Methods
    }
}
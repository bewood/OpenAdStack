//-----------------------------------------------------------------------
// <copyright file="BlobExtensions.cs" company="Rare Crowds Inc">
// Copyright 2012-2013 Rare Crowds, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

using System.IO;
using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureUtilities.Storage
{
    /// <summary>Extensions for CloudBlobs</summary>
    public static class BlobExtensions
    {
        /// <summary>Checks whether or not a blob exists</summary>
        /// <param name="blob">Blob to check the existence of</param>
        /// <returns>True if the blob exists; otherwise, false.</returns>
        public static bool Exists(this ICloudBlob blob)
        {
            try
            {
                blob.FetchAttributes();
                return true;
            }
            catch (StorageException se)
            {
                var statusCode = se.RequestInformation.HttpStatusCode;
                if (statusCode == (int)HttpStatusCode.NotFound)
                {
                    return false;
                }

                throw;
            }
        }

        /// <summary>Reads a blob as text</summary>
        /// <param name="blob">Blob to read</param>
        /// <returns>Blob contents as text</returns>
        public static string DownloadText(this ICloudBlob blob)
        {
            using (var reader = new StreamReader(blob.OpenRead()))
            {
                return reader.ReadToEnd();
            }
        }
    }
}

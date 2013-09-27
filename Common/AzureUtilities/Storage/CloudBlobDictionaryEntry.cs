﻿//-----------------------------------------------------------------------
// <copyright file="CloudBlobDictionaryEntry.cs" company="Rare Crowds Inc">
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

using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Utilities.Storage;

namespace AzureUtilities.Storage
{
    /// <summary>
    /// ICloudBlob wrapping IPersistentDictionaryEntry implementation
    /// </summary>
    internal class CloudBlobDictionaryEntry : IPersistentDictionaryEntry
    {
        /// <summary>Default blob request timeout (5 minutes)</summary>
        private static readonly TimeSpan DefaultBlobRequestTimeout = new TimeSpan(0, 5, 0);

        /// <summary>
        /// Name of the metadata value indicating whether the
        /// entry's content is compressed.
        /// </summary>
        private const string CompressedMetadataValue = "COMPRESSED";

        /// <summary>Blob for the entry contents</summary>
        private readonly ICloudBlob blob;

        /// <summary>Options for blob requests</summary>
        private readonly BlobRequestOptions options;

        /// <summary>
        /// Initializes a new instance of the CloudBlobDictionaryEntry class.
        /// </summary>
        /// <param name="blob">The blob to wrap</param>
        public CloudBlobDictionaryEntry(ICloudBlob blob)
        {
            this.blob = blob;
            this.options = new BlobRequestOptions
            {
                 ServerTimeout = DefaultBlobRequestTimeout,
                 RetryPolicy = new ExponentialRetry(),
            };
        }

        /// <summary>Gets the name for the store entry</summary>
        public string Name
        {
            get { return this.blob.Name; }
        }

        /// <summary>Gets the ETag for the store entry</summary>
        public string ETag
        {
            get { return this.blob.Properties.ETag; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether
        /// the entry content is compressed.
        /// </summary>
        private bool Compressed
        {
            get
            {
                var compressed = this.blob.Metadata[CompressedMetadataValue];
                return !string.IsNullOrEmpty(compressed) ?
                    bool.Parse(compressed) :
                    false;
            }

            set
            {
                this.blob.Metadata[CompressedMetadataValue] = value.ToString();
                this.blob.SetMetadata();
            }
        }

        /// <summary>Reads the content from the entry</summary>
        /// <returns>The content</returns>
        public byte[] ReadAllBytes()
        {
            if (!this.blob.Exists())
            {
                return new byte[0];
            }

            using (var stream = new MemoryStream())
            {
                this.blob.DownloadToStream(stream, null, this.options);
                stream.Seek(0, SeekOrigin.Begin);
                var bytes = stream.ToArray();
                return this.Compressed ? bytes.Inflate() : bytes;
            }
        }

        /// <summary>Writes the content to the entry</summary>
        /// <param name="content">The content</param>
        /// <param name="compress">Whether to compress the content</param>
        public void WriteAllBytes(byte[] content, bool compress)
        {
            try
            {
                var stream = new MemoryStream(compress ? content.Deflate() : content);
                this.blob.Properties.ContentMD5 = null;
                this.blob.UploadFromStream(stream, null, this.options);
                this.Compressed = compress;
            }
            catch (Exception e)
            {
                throw new IOException(
                    "Unable to write bytes to element '{0}' of '{1}'"
                    .FormatInvariant(this.Name, this.blob.Container.Name),
                    e);
            }
        }
    }
}

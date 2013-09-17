﻿//-----------------------------------------------------------------------
// <copyright file="SqlDictionaryEntry.cs" company="Rare Crowds Inc">
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Diagnostics;
using Utilities.Data;
using Utilities.Storage;

namespace SqlUtilities.Storage
{
    /// <summary>
    /// CloudBlob wrapping IPersistentDictionaryEntry implementation
    /// </summary>
    internal class SqlDictionaryEntry : IPersistentDictionaryEntry
    {
        /// <summary>SQL client used to read and write the entry</summary>
        private ISqlClient sqlClient;

        /// <summary>The store name</summary>
        private string storeName;

        /// <summary>Entry Name</summary>
        private string name;

        /// <summary>Entry ETag</summary>
        private Guid etag;

        /// <summary>Entry content</summary>
        private byte[] content;

        /// <summary>
        /// Initializes a new instance of the SqlDictionaryEntry class.
        /// </summary>
        /// <param name="sqlClient">The sql client</param>
        /// <param name="storeName">The store name</param>
        /// <param name="name">The entry name</param>
        public SqlDictionaryEntry(ISqlClient sqlClient, string storeName, string name)
        {
            this.etag = Guid.Empty;
            this.content = new byte[0];
            this.sqlClient = sqlClient;
            this.storeName = storeName;
            this.name = name;

            // Get the entry values from SQL (if an entry already exists)
            while (true)
            {
                var resultRows = SqlDictionary<object>.ExecuteStoredProcedure(
                    this.sqlClient,
                    Constants.StoredProcedures.GetEntry,
                    this.StoreNameParameter,
                    this.EntryNameParameter);
                var result = resultRows.SingleOrDefault();
                if (result != null)
                {
                    this.etag = (Guid)result[Constants.SqlResultValues.ETag];
                    this.content = (byte[])result[Constants.SqlResultValues.Content];
                    if ((bool)result[Constants.SqlResultValues.Compressed])
                    {
                        this.content = this.content.Inflate();
                    }
                }

                return;
            }
        }

        /// <summary>Gets the ETag for the store entry</summary>
        public string ETag
        {
            get { return this.etag.ToString(); }
        }

        /// <summary>Gets a SqlParameter containing the store name</summary>
        private SqlParameter StoreNameParameter
        {
            get
            {
                return new SqlParameter(
                    Constants.SqlParameterNames.StoreName,
                    SqlDbType.NVarChar)
                    {
                        Value = this.storeName
                    };
            }
        }

        /// <summary>Gets a SqlParameter containing the entry name</summary>
        private SqlParameter EntryNameParameter
        {
            get
            {
                return new SqlParameter(
                    Constants.SqlParameterNames.EntryName,
                    SqlDbType.NVarChar)
                    {
                        Value = this.name
                    };
            }
        }
        
        /// <summary>Gets a SqlParameter containing the eTag</summary>
        private SqlParameter ETagParameter
        {
            get
            {
                return new SqlParameter(
                    Constants.SqlParameterNames.ETag,
                    SqlDbType.UniqueIdentifier)
                    {
                        Value = this.etag != Guid.Empty ?
                            (object)this.etag :
                            DBNull.Value
                    };
            }
        }

        /// <summary>Reads the content from the entry</summary>
        /// <remarks>Content is already read from SQL at initialization</remarks>
        /// <returns>The content</returns>
        public byte[] ReadAllBytes()
        {
            return this.content;
        }

        /// <summary>Writes the content to the entry</summary>
        /// <param name="content">The content</param>
        /// <param name="compress">Whether to compress the content</param>
        /// <exception cref="System.InvalidOperationException">
        /// The ETag has changed since the entry was initialized
        /// </exception>
        /// <exception cref="System.IO.IOException">
        /// An unknown error occured calling the stored procedure
        /// </exception>
        public void WriteAllBytes(byte[] content, bool compress)
        {
            // Compose the parameters for the SP call
            var bytes = compress ? content.Deflate() : content;
            var contentParameter = new SqlParameter(
                Constants.SqlParameterNames.Content,
                SqlDbType.VarBinary)
                {
                    Value = bytes,
                    Size = -1
                };
            var compressedParameter = new SqlParameter(
                Constants.SqlParameterNames.Compressed,
                SqlDbType.Bit)
                {
                    Value = compress
                };

            // Call the update SP
            IEnumerable<IDictionary<string, object>> resultRows = null;
            SqlError error = null;
            try
            {
                resultRows = SqlDictionary<object>.ExecuteStoredProcedure(
                    this.sqlClient,
                    Constants.StoredProcedures.SetEntry,
                    this.StoreNameParameter,
                    this.EntryNameParameter,
                    contentParameter,
                    compressedParameter,
                    this.ETagParameter);
            }
            catch (SqlException sqle)
            {
                // Convert invalid etag errors to exceptions
                error = sqle.Errors.OfType<SqlError>().FirstOrDefault();
                if (error != null &&
                    error.Number == 50000 &&
                    error.Message.ToUpperInvariant().Contains("ETAG"))
                {
                    throw new InvalidETagException(this.storeName, this.name, this.ETag, sqle);
                }
            }

            var result = resultRows == null ? null : resultRows.SingleOrDefault();
            if (result == null || !result.ContainsKey(Constants.SqlResultValues.ETag))
            {
                var message = "The stored procedure '{0}' did not return a valid result\nError: '{1}'"
                    .FormatInvariant(Constants.StoredProcedures.SetEntry, error);
                LogManager.Log(LogLevels.Error, message);
                throw new InvalidOperationException(message);
            }

            this.etag = (Guid)result[Constants.SqlResultValues.ETag];
            return;
        }
    }
}

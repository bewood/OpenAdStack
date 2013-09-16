// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AzureStorageKey.cs" company="Rare Crowds Inc">
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
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using DataAccessLayer;

namespace ConcreteDataStore
{
    /// <summary>
    /// Implementation of a storage key for an Azure Table based store.
    /// </summary>
    internal class AzureStorageKey : IStorageKey
    {
        /// <summary>Field name for Azure Table Name</summary>
        public const string TableNameFieldName = "AzureTableName";

        /// <summary>Field name for Azure Table Partition</summary>
        public const string PartitionFieldName = "AzureTablePartition";
        
        /// <summary>Field name for Azure Table RowId</summary>
        public const string RowIdFieldName = "AzureTableRowId";

        /// <summary>Initializes a new instance of the <see cref="AzureStorageKey"/> class.</summary>
        /// <param name="accountId">The account id.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="partition">The partition.</param>
        /// <param name="rowId">The row id.</param>
        public AzureStorageKey(string accountId, string tableName, string partition, EntityId rowId) 
            : this(accountId, tableName, partition, rowId, 0, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AzureStorageKey"/> class.</summary>
        /// <param name="existingKey">An existing key to copy.</param>
        public AzureStorageKey(AzureStorageKey existingKey)
            : this(existingKey.StorageAccountName, existingKey.TableName, existingKey.Partition, existingKey.RowId, existingKey.LocalVersion, existingKey.VersionTimestamp)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AzureStorageKey"/> class.</summary>
        /// <param name="accountId">The account id.</param>
        /// <param name="tableName">The table name.</param>
        /// <param name="partition">The partition.</param>
        /// <param name="rowId">The row id.</param>
        /// <param name="localVersion">The entity version local to this storage account.</param>
        /// <param name="versionTimestamp">The version timestamp.</param>
        public AzureStorageKey(
            string accountId, string tableName, string partition, EntityId rowId, int localVersion, DateTime? versionTimestamp)
        {
            this.StorageAccountName = accountId;
            this.TableName = tableName;
            this.Partition = partition;
            this.RowId = rowId;
            this.LocalVersion = localVersion;
            this.VersionTimestamp = versionTimestamp;
        }

        /// <summary>Gets or sets RowId.</summary>
        public EntityId RowId { get; internal set; }

        /// <summary>Gets or sets Partition.</summary>
        public string Partition { get; internal set; }

        /// <summary>Gets or sets TableName.</summary>
        public string TableName { get; internal set; }

        ////
        // Begin IStorageKey members
        ////

        /// <summary>Gets or sets StorageAccountName (e.g. - account).</summary>
        public string StorageAccountName { get; set; }

        /// <summary>Gets or sets VersionTimestamp.</summary>
        public DateTime? VersionTimestamp { get; set; }

        /// <summary>Gets or sets LocalVersion.</summary>
        public int LocalVersion { get; set; }

        /// <summary>Gets a map of key field name/value pairs.</summary>
        public IDictionary<string, string> KeyFields
        {
            get 
            { 
                return new Dictionary<string, string>
                {
                    { TableNameFieldName, this.TableName },
                    { PartitionFieldName, this.Partition },
                    { RowIdFieldName, this.RowId },
                }; 
            }
        }

        /// <summary>Interface method to determine equality of keys.</summary>
        /// <param name="otherKey">The key to compare with this key.</param>
        /// <returns>True if the keys refer to the same storage entity.</returns>
        public bool IsEqual(IStorageKey otherKey)
        {
            var otherAzureKey = otherKey as AzureStorageKey;
            return otherAzureKey != null 
                && otherAzureKey.StorageAccountName == this.StorageAccountName
                && otherAzureKey.LocalVersion == this.LocalVersion 
                && otherAzureKey.RowId == this.RowId
                && otherAzureKey.Partition == this.Partition 
                && otherAzureKey.TableName == this.TableName;
        }

        ////
        // End IStorageKey members
        ////
    }
}

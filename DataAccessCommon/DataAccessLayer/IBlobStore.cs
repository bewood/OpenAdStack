// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IBlobStore.cs" company="Rare Crowds Inc">
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

using System.Diagnostics.CodeAnalysis;

namespace DataAccessLayer
{
    /// <summary>Interface for accessing Blob stores independent of underlying technology.</summary>
    public interface IBlobStore
    {
        /// <summary>Gets a storage key factory for this blob store.</summary>
        /// <returns>An IStorageKeyFactory</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Factory method.")]
        IStorageKeyFactory GetStorageKeyFactory();

        /// <summary>Get a blob entity given a storage key.</summary>
        /// <param name="key">An IStorageKey key.</param>
        /// <returns>A blob entity that is not deserialized.</returns>
        IEntity GetBlobByKey(IStorageKey key);

        /// <summary>Save an entity in the entity store.</summary>
        /// <param name="rawEntity">The raw entity.</param>
        /// <param name="company">The company (for storage auditing).</param>
        void SaveBlob(IEntity rawEntity, string company);
    }
}

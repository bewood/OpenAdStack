// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEntitySchema.cs" company="Rare Crowds Inc">
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

namespace DataAccessLayer
{
    /// <summary>Interface defining entity schema behavior.</summary>
    public interface IEntitySchema
    {
        /// <summary>Gets the current schema version for IRawEntity.</summary>
        int CurrentSchemaVersion { get; }

        /// <summary>Check that a schema version supports a particular schema feature.</summary>
        /// <param name="schemaVersion">The schema version of the entity.</param>
        /// <param name="featureId">The feature id being checked against the schema version.</param>
        /// <returns>True if the feature is supported</returns>
        bool CheckSchemaFeature(int schemaVersion, EntitySchemaFeatureId featureId);
    }
}

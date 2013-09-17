// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompanyEntity.cs" company="Rare Crowds Inc">
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
    /// <summary>
    /// Wrapped Entity with validating members for Company.
    /// No Default construction - use Enity or JSON string
    /// to initialize.
    /// </summary>
    public class CompanyEntity : EntityWrapperBase
    {
        /// <summary>Category Name for Company Entities.</summary>
        public const string CategoryName = "Company";
        
        /// <summary>Initializes a new instance of the <see cref="CompanyEntity"/> class.</summary>
        /// <param name="externalEntityId">The external entity id to assign the entity.</param>
        /// <param name="rawEntity">The raw entity from which to construct.</param>
        public CompanyEntity(EntityId externalEntityId, IEntity rawEntity)
        {
            this.Initialize(externalEntityId, CategoryName, rawEntity);
        }

        /// <summary>Initializes a new instance of the <see cref="CompanyEntity"/> class.</summary>
        /// <param name="entity">The IEntity object from which to construct.</param>
        public CompanyEntity(IEntity entity)
        {
            this.Initialize(entity);
        }

        /// <summary>Initializes a new instance of the <see cref="CompanyEntity"/> class.</summary>
        public CompanyEntity()
        {
        }

        /// <summary>Abstract method to validate type of entity.</summary>
        /// <param name="entity">The entity.</param>
        protected override void ValidateEntityType(IEntity entity)
        {
            // TODO: Determine appropriate type validation for campaign
            ThrowIfCategoryMismatch(entity, CategoryName);
        }
    }
}

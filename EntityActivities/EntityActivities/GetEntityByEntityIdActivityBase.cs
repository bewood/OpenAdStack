﻿//-----------------------------------------------------------------------
// <copyright file="GetEntityByEntityIdActivityBase.cs" company="Rare Crowds Inc">
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
using System.Linq;
using Activities;
using DataAccessLayer;
using EntityUtilities;

namespace EntityActivities
{
    /// <summary>
    /// Base activity for getting entities by their ExternalEntityId
    /// </summary>
    public abstract class GetEntityByEntityIdActivityBase : EntityActivity
    {
        /// <summary>
        /// Gets the name of the request value containing the context company's ExternalEntityId
        /// </summary>
        protected virtual string ContextCompanyEntityIdValue
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the name of the request value containing the ExternalEntityId
        /// </summary>
        protected virtual string EntityIdValue
        {
            get { return EntityActivityValues.EntityId; }
        }

        /// <summary>
        /// Gets the expected EntityCategory of the returned entity
        /// </summary>
        protected abstract string EntityCategory { get; }

        /// <summary>
        /// Gets the name of the result value in which to return the entity
        /// </summary>
        protected abstract string ResultValue { get; }

        /// <summary>
        /// Create a return json string for the Campaign entity type
        /// </summary>
        /// <param name="entity">the entity</param>
        /// <returns>a json string</returns>
        protected virtual string CreateJsonResult(IEntity entity)
        {
            return entity.SerializeToJson();
        }

        /// <summary>
        /// Create a return json string for the Campaign entity type
        /// </summary>
        /// <param name="entity">the entity</param>
        /// <param name="queryValues">Query values dictionary</param>
        /// <returns>a json string</returns>
        protected virtual string CreateJsonResult(
            IEntity entity,
            Dictionary<string, string> queryValues)
        {
            return entity.SerializeToJson(new EntitySerializationFilter(queryValues));
        }

        /// <summary>Process the request</summary>
        /// <param name="request">The request containing input values</param>
        /// <returns>The result containing output values</returns>
        protected override ActivityResult ProcessRequest(ActivityRequest request)
        {
            var externalContext = CreateRepositoryContext(RepositoryContextType.ExternalEntityGet, request, this.ContextCompanyEntityIdValue);
            var entityId = new EntityId(request.Values[this.EntityIdValue]);

            IEntity entity = null;
            
            try
            {
                entity = this.Repository.GetEntitiesById(externalContext, new EntityId[] { entityId }).Single();
            }
            catch (DataAccessEntityNotFoundException)
            {
                return EntityNotFoundError(entityId);
            }

            // Check that the entity is actually of the expected category
            if ((string)entity.EntityCategory != this.EntityCategory)
            {
                return ErrorResult(
                    ActivityErrorId.GenericError,
                    "The Entity '{0}' is not a valid {1}",
                    entityId,
                    this.EntityCategory);
            }

            return this.SuccessResult(new Dictionary<string, string>
            {
                {
                    this.ResultValue,
                    this.CreateJsonResult(entity, request.QueryValues)
                }
            });
        }
    }
}

﻿// -----------------------------------------------------------------------
// <copyright file="SaveCompanyActivity.cs" company="Rare Crowds Inc">
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
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Activities;
using DataAccessLayer;
using EntityUtilities;
using ResourceAccess;

namespace EntityActivities
{
    /// <summary>
    /// Activity for updating existing companies
    /// </summary>
    /// <remarks>
    /// Updates an existing company
    /// RequiredValues
    ///   CompanyEntityId - ExternalEntityId of the company
    ///   Company - The updated company as json
    /// ResultValues
    ///   Company - The updated company as json and including any additional values added by the DAL
    /// </remarks>
    [Name(EntityActivityTasks.SaveCompany)]
    [RequiredValues(EntityActivityValues.EntityId, EntityActivityValues.MessagePayload)]
    [ResultValues(EntityActivityValues.Company)]
    public class SaveCompanyActivity : EntityActivity
    {
        /// <summary>Process the request</summary>
        /// <param name="request">The request containing input values</param>
        /// <returns>The result containing output values</returns>
        protected override ActivityResult ProcessRequest(ActivityRequest request)
        {
            var externalContext = CreateRepositoryContext(
                RepositoryContextType.ExternalEntitySave, request, EntityActivityValues.EntityId);
            var internalContext = CreateRepositoryContext(
                RepositoryContextType.InternalEntityGet, request);

            var companyEntityId = new EntityId(request.Values[EntityActivityValues.EntityId]);
            var company = EntityJsonSerializer.DeserializeCompanyEntity(
                companyEntityId,
                request.Values[EntityActivityValues.MessagePayload]);

            // Check that the company already exists
            var original = this.Repository.TryGetEntity(internalContext, companyEntityId) as CompanyEntity;
            if (original == null)
            {                
                return EntityNotFoundError(companyEntityId);
            }

            // verify the user can write to this company
            var userId = request.Values[EntityActivityValues.AuthUserId];
            UserEntity user = null;
            try
            {
                // Get the user
                user = this.Repository.GetUser(internalContext, userId);
            }
            catch (ArgumentException)
            {
                return UserNotFoundError(userId);
            }

            var canonicalResource =
                new CanonicalResource(
                    new Uri("https://localhost/api/entity/company/{0}".FormatInvariant(companyEntityId.ToString()), UriKind.Absolute), "POST");
            if (!this.AccessHandler.CheckAccess(canonicalResource, user.ExternalEntityId))
            {
                return UserNotAuthorized(companyEntityId.ToString());
            }

            // Copy unset properties from original
            CopyPropertiesFromOriginal(original, ref company);

            // Updating the existing Company
            this.Repository.SaveEntity(externalContext, company);
            
            return this.SuccessResult(new Dictionary<string, string>
            {
                { EntityActivityValues.Company, company.SerializeToJson(new EntitySerializationFilter(request.QueryValues)) }
            });
        }
    }
}

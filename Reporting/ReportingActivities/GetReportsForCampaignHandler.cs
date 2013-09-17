// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetReportsForCampaignHandler.cs" company="Rare Crowds Inc">
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

using System.Collections.Generic;
using Activities;
using DataAccessLayer;
using ReportingUtilities;
using Utilities;

namespace ReportingActivities
{
    /// <summary>Class to get reports for a campaign.</summary>
    public class GetReportsForCampaignHandler : IActivityHandler
    {
        /// <summary>Initializes a new instance of the <see cref="GetReportsForCampaignHandler"/> class.</summary>
        /// <param name="repository">Entity repository instance.</param>
        /// <param name="companyEntityId">Company entity id.</param>
        /// <param name="campaignEntityId">Campaign entity id.</param>
        public GetReportsForCampaignHandler(
            IEntityRepository repository,
            EntityId companyEntityId,
            EntityId campaignEntityId)
        {
            this.Repository = repository;
            this.CompanyEntityId = companyEntityId;
            this.CampaignEntityId = campaignEntityId;
        }

        /// <summary>Gets the Repository.</summary>
        internal IEntityRepository Repository { get; private set; }

        /// <summary>Gets company entity id.</summary>
        internal EntityId CompanyEntityId { get; private set; }

        /// <summary>Gets campaign entity id.</summary>
        internal EntityId CampaignEntityId { get; private set; }

        /// <summary>Execute the activity handler.</summary>
        /// <returns>The activity result.</returns>
        public IDictionary<string, string> Execute()
        {
            var context = new RequestContext
            {
                ExternalCompanyId = this.CompanyEntityId,
                EntityFilter = new RepositoryEntityFilter(true, false, true, false)
            };

            // Get the list of existing report items
            var campaignEntity = this.Repository.GetEntity<CampaignEntity>(context, this.CampaignEntityId);
            var reportItemsJson = campaignEntity.TryGetPropertyByName<string>(ReportingPropertyNames.CurrentReports, "[]");
            return new Dictionary<string, string> { { ReportingActivityValues.Reports, reportItemsJson } };
        }
    }
}

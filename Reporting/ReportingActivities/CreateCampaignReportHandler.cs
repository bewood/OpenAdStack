// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateCampaignReportHandler.cs" company="Rare Crowds Inc">
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
using System.Linq;
using System.Text;
using Activities;
using DataAccessLayer;
using DynamicAllocation;
using EntityUtilities;
using ReportingTools;
using ReportingUtilities;
using Utilities;
using Utilities.Serialization;

namespace ReportingActivities
{
    /// <summary>Class to build a dynamic allocation report.</summary>
    public class CreateCampaignReportHandler : IActivityHandler
    {
        /// <summary>Initializes a new instance of the <see cref="CreateCampaignReportHandler"/> class.</summary>
        /// <param name="repository">Entity repository instance.</param>
        /// <param name="reportGenerators">The report generator for the report type.</param>
        /// <param name="companyEntityId">Company entity id.</param>
        /// <param name="campaignEntityId">Campaign entity id.</param>
        /// <param name="reportEntityId">New Report entity id.</param>
        /// <param name="buildVerbose">Set true to build a verbose report.</param>
        /// <param name="reportType">The report type to build.</param>
        public CreateCampaignReportHandler(
            IEntityRepository repository, 
            IDictionary<DeliveryNetworkDesignation, IReportGenerator> reportGenerators, 
            EntityId companyEntityId, 
            EntityId campaignEntityId, 
            EntityId reportEntityId, 
            bool buildVerbose, 
            string reportType)
        {
            if (reportGenerators == null)
            {
                throw new AppsGenericException("Null report generators collection passed to CampaignReportHandler constructor.");
            }

            if (reportEntityId == null)
            {
                throw new AppsGenericException("Null report entity id passed to CampaignReportHandler constructor.");
            }

            if (string.IsNullOrEmpty(reportType))
            {
                throw new AppsGenericException("Null or empty report type passed to CampaignReportHandler constructor.");
            }

            this.Repository = repository;
            this.ReportGenerators = reportGenerators;
            this.CompanyEntityId = companyEntityId;
            this.CampaignEntityId = campaignEntityId;
            this.ReportEntityId = reportEntityId;
            this.BuildVerbose = buildVerbose;
            this.ReportType = reportType;
        }

        /// <summary>Gets the Repository.</summary>
        internal IEntityRepository Repository { get; private set; }

        /// <summary>Gets the report generator for the type of report.</summary>
        internal IDictionary<DeliveryNetworkDesignation, IReportGenerator> ReportGenerators { get; private set; }

        /// <summary>Gets company entity id.</summary>
        internal EntityId CompanyEntityId { get; private set; }

        /// <summary>Gets campaign entity id.</summary>
        internal EntityId CampaignEntityId { get; private set; }

        /// <summary>Gets report entity id.</summary>
        internal EntityId ReportEntityId { get; private set; }

        /// <summary>Gets a value indicating whether to build a verbose report.</summary>
        internal bool BuildVerbose { get; private set; }

        /// <summary>Gets a constant representing the type of report to build.</summary>
        internal string ReportType { get; private set; }

        /// <summary>Execute the activity handler.</summary>
        /// <returns>The activity result.</returns>
        public IDictionary<string, string> Execute()
        {
            if (this.ReportGenerators.Count == 0)
            {
                throw new AppsGenericException("No report generators specified for report {0} on campaign {1}."
                    .FormatInvariant(this.ReportType, this.CampaignEntityId));
            }

            // TODO: Support multiple report generator merge
            if (this.ReportGenerators.Count > 1)
            {
                throw new AppsGenericException("Multiple report generators not supported yet on campaign {0}."
                    .FormatInvariant(this.CampaignEntityId));
            }

            var deliveryNetwork = this.ReportGenerators.First().Key;
            if (deliveryNetwork != DeliveryNetworkDesignation.AppNexus)
            {
                throw new AppsGenericException("Delivery network {0} not currently supported."
                    .FormatInvariant(deliveryNetwork));
            }

            var reportGenerator = this.ReportGenerators[deliveryNetwork];
            var report = reportGenerator.BuildReport(this.ReportType, this.BuildVerbose);
            this.SaveReport(report);

            // No result values are returned on success
            return new Dictionary<string, string> { { EntityActivityValues.EntityId, this.ReportEntityId } };
        }

        /// <summary>Build an updated report items json.</summary>
        /// <param name="currentReportsJson">The json of the current report items list.</param>
        /// <param name="reportEntity">The new report entity to add to list.</param>
        /// <returns>An updated report items json.</returns>
        internal string BuildUpdatedReportsJson(string currentReportsJson, ReportEntity reportEntity)
        {
            var currentReports = new List<ReportItem>();
            if (!string.IsNullOrEmpty(currentReportsJson))
            {
                currentReports = AppsJsonSerializer.DeserializeObject<List<ReportItem>>(currentReportsJson);
            }

            // Add report item
            var newReportItem = new ReportItem
            {
                ReportDate = reportEntity.LastModifiedDate,
                ReportEntityId = reportEntity.ExternalEntityId.ToString(),
                ReportType = this.ReportType
            };
            currentReports.Add(newReportItem);
            currentReportsJson = AppsJsonSerializer.SerializeObject(currentReports);
            return currentReportsJson;
        }

        /// <summary>Save the report as a blob and update the reference in the campaign.</summary>
        /// <param name="report">The report</param>
        private void SaveReport(StringBuilder report)
        {
            var context = new RequestContext
                {
                    ExternalCompanyId = this.CompanyEntityId,
                    EntityFilter = new RepositoryEntityFilter(true, false, true, false)
                };
            
            // Build and save the report entity
            var reportEntity = ReportEntity.BuildReportEntity(this.ReportEntityId, "CampaignReport", this.ReportType, report.ToString());
            if (!this.Repository.TrySaveEntity(context, reportEntity))
            {
                throw new AppsGenericException(
                    "Report entity could not be saved. Campaign {0}, Report Entity {1}.".FormatInvariant(
                        this.CampaignEntityId, this.ReportEntityId));
            }

            // Get the list of existing report items
            var campaignEntity = this.Repository.GetEntity<CampaignEntity>(context, this.CampaignEntityId);
            var reportItemsJson = campaignEntity.TryGetPropertyByName<string>(ReportingPropertyNames.CurrentReports, null);

            // Build the updated json
            reportItemsJson = this.BuildUpdatedReportsJson(reportItemsJson, reportEntity);

            // Update the entity.
            campaignEntity.SetPropertyByName(ReportingPropertyNames.CurrentReports, reportItemsJson, PropertyFilter.Extended);
            var reportSaved = this.Repository.TryForceUpdateEntity(
                context, campaignEntity, new List<string> { ReportingPropertyNames.CurrentReports }, null);

            if (!reportSaved)
            {
                throw new AppsGenericException(
                    "Campaign could not be saved with new report reference. Campaign {0}, Report Entity {1}.".FormatInvariant(
                        this.CampaignEntityId, this.ReportEntityId));
            }
        }
    }
}

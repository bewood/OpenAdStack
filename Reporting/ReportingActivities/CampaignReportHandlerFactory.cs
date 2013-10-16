// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CampaignReportHandlerFactory.cs" company="Rare Crowds Inc">
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
using Activities;
using DataAccessLayer;
using DynamicAllocation;
using DynamicAllocationActivities;
using EntityUtilities;
using ReportingTools;
using ReportingUtilities;
using Utilities;

namespace ReportingActivities
{
    /// <summary>
    /// Factory class for CampaignReportActivity handlers.
    /// </summary>
    public class CampaignReportHandlerFactory : IActivityHandlerFactory
    {
        /// <summary>Initializes a new instance of the <see cref="CampaignReportHandlerFactory"/> class.</summary>
        public CampaignReportHandlerFactory()
        {
            this.CampaignFactory = new DynamicAllocationCampaignFactory();
        }

        /// <summary>Initializes a new instance of the <see cref="CampaignReportHandlerFactory"/> class.</summary>
        /// <param name="campaignFactory">Injection constructor for alternate campaign factory.</param>
        public CampaignReportHandlerFactory(IDynamicAllocationCampaignFactory campaignFactory)
        {
            this.CampaignFactory = campaignFactory;
        }

        /// <summary>Gets the DynamicAllocationCampaign factory</summary>
        internal IDynamicAllocationCampaignFactory CampaignFactory { get; private set; }

        /// <summary>Create the activity handler.</summary>
        /// <param name="request">The activity request.</param>
        /// <param name="context">The activity context.</param>
        /// <returns>An IActivityHandler instance.</returns>
        public IActivityHandler CreateActivityHandler(ActivityRequest request, IDictionary<Type, object> context)
        {
            var supportedTasks = new Dictionary<string, Func<ActivityRequest, EntityId, EntityId, IEntityRepository, IActivityHandler>>
                {
                    { ReportingActivityTasks.CreateCampaignReport, this.BuildCreateCampaignReportHandler },
                    { ReportingActivityTasks.GetReportsForCampaign, this.BuildGetReportsForCampaignHandler },
                    { ReportingActivityTasks.GetCampaignReportData, this.BuildGetCampaignReportDataHandler }
                };

            if (string.IsNullOrEmpty(request.Task) || !supportedTasks.ContainsKey(request.Task))
            {
                throw new AppsGenericException("Missing or unrecognized task in CampaignReportHandlerFactory.");
            }

            if (!context.ContainsKey(typeof(IEntityRepository)))
            {
                throw new AppsGenericException("Missing IEntityRepository in CampaignReportHandlerFactory.");
            }

            if (!request.Values.ContainsKey(EntityActivityValues.CompanyEntityId))
            {
                throw new AppsGenericException("Missing CompanyEntityId in CampaignReportHandlerFactory.");
            }

            if (!request.Values.ContainsKey(EntityActivityValues.CampaignEntityId))
            {
                throw new AppsGenericException("Missing CampaignEntityId in CampaignReportHandlerFactory.");
            }

            var repository = (IEntityRepository)context[typeof(IEntityRepository)];
            var companyEntityId = new EntityId(request.Values[EntityActivityValues.CompanyEntityId]);
            var campaignEntityId = new EntityId(request.Values[EntityActivityValues.CampaignEntityId]);

            return supportedTasks[request.Task](request, campaignEntityId, companyEntityId, repository);
        }

        /// <summary>Build a handler to get reports for a campaign.</summary>
        /// <param name="request">The activity request.</param>
        /// <param name="campaignEntityId">The campaign entity id.</param>
        /// <param name="companyEntityId">The company entity id.</param>
        /// <param name="repository">The entity repository interface.</param>
        /// <returns>The handler.</returns>
        private IActivityHandler BuildGetReportsForCampaignHandler(
            ActivityRequest request,
            EntityId campaignEntityId,
            EntityId companyEntityId,
            IEntityRepository repository)
        {
            return new GetReportsForCampaignHandler(repository, companyEntityId, campaignEntityId);
        }

        /// <summary>Build a handler to get campaign report data for a specified report on a campaign.</summary>
        /// <param name="request">The activity request.</param>
        /// <param name="campaignEntityId">The campaign entity id.</param>
        /// <param name="companyEntityId">The company entity id.</param>
        /// <param name="repository">The entity repository interface.</param>
        /// <returns>The handler.</returns>
        private IActivityHandler BuildGetCampaignReportDataHandler(
            ActivityRequest request,
            EntityId campaignEntityId,
            EntityId companyEntityId,
            IEntityRepository repository)
        {
            var reportEntityId = new EntityId(request.Values[EntityActivityValues.EntityId]);
            return new GetCampaignReportDataHandler(repository, companyEntityId, campaignEntityId, reportEntityId);
        }

        /// <summary>Build a handler to create a campaign report.</summary>
        /// <param name="request">The activity request.</param>
        /// <param name="campaignEntityId">The campaign entity id.</param>
        /// <param name="companyEntityId">The company entity id.</param>
        /// <param name="repository">The entity repository interface.</param>
        /// <returns>The handler.</returns>
        private IActivityHandler BuildCreateCampaignReportHandler(
            ActivityRequest request,
            EntityId campaignEntityId,
            EntityId companyEntityId,
            IEntityRepository repository)
        {
            this.CampaignFactory.BindRuntime(repository);
            var dynamicAllocationCampaign = this.CampaignFactory.BuildDynamicAllocationCampaign(companyEntityId, campaignEntityId);

            var buildVerbose = request.Values.ContainsKey(ReportingActivityValues.VerboseReport);

            // Set report type if present (or default)
            var reportType = ReportTypes.ClientCampaignBilling;
            if (request.QueryValues.ContainsKey(ReportingActivityValues.ReportType))
            {
                reportType = request.QueryValues[ReportingActivityValues.ReportType];
            }

            var reportEntityId = new EntityId(request.Values[EntityActivityValues.EntityId]);

            // TODO: Generalize to non-DA campaigns
            // TODO: Support other networks and multiple networks
            var generatorMap = new Dictionary<DeliveryNetworkDesignation, IReportGenerator>();
            if (dynamicAllocationCampaign.DeliveryNetwork == DeliveryNetworkDesignation.AppNexus)
            {
                var reportGenerator = new AppNexusBillingReport(repository, dynamicAllocationCampaign);
                generatorMap.Add(DeliveryNetworkDesignation.AppNexus, reportGenerator);
            }

            return new CreateCampaignReportHandler(
                repository, generatorMap, companyEntityId, campaignEntityId, reportEntityId, buildVerbose, reportType);
        }
    }
}
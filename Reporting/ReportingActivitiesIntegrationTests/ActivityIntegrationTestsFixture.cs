// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActivityIntegrationTestsFixture.cs" company="Rare Crowds Inc">
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
using System.Linq;
using Activities;
using ActivityTestUtilities;
using DataAccessLayer;
using Diagnostics;
using DynamicAllocation;
using DynamicAllocationTestUtilities;
using EntityUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReportingActivities;
using ReportingUtilities;
using Rhino.Mocks;
using SimulatedDataStore;
using TestUtilities;
using Utilities.Serialization;
using Utilities.Storage;
using Utilities.Storage.Testing;

namespace ReportingActivitiesIntegrationTests
{
    /// <summary>Integration test fixture for Reporting activities</summary>
    [TestClass]
    public class ActivityIntegrationTestsFixture
    {
        /// <summary>DA campaign stub for testing.</summary>
        private static readonly DynamicAllocationCampaignTestStub campaignStub = new DynamicAllocationCampaignTestStub();

        /// <summary>Default repository for testing.</summary>
        private IEntityRepository repository;

        /// <summary>company entity id for testing</summary>
        private EntityId companyEntityId;

        /// <summary>campaign entity id for testing.</summary>
        private EntityId campaignEntityId;

        /// <summary>Request report entity id for testing.</summary>
        private EntityId reportEntityId;

        /// <summary>campaign owner id for testing</summary>
        private string campaignOwnerId;

        /// <summary>activity request for testing.</summary>
        private ActivityRequest request;

        /// <summary>One time class initialization</summary>
        /// <param name="context">The context.</param>
        [ClassInitialize]
        public static void InitializeClass(TestContext context)
        {
            LogManager.Initialize(new[] { MockRepository.GenerateMock<ILogger>() });

            MeasureSourceFactory.Initialize(new IMeasureSourceProvider[]
            {
                new AppNexusActivities.Measures.AppNexusLegacyMeasureSourceProvider(),
                new AppNexusActivities.Measures.AppNexusMeasureSourceProvider(),
                new GoogleDfpActivities.Measures.DfpMeasureSourceProvider()
            });
        }

        /// <summary>Per test initialization.</summary>
        [TestInitialize]
        public void InitializeTest()
        {
            // Initialize simulated storage
            SimulatedPersistentDictionaryFactory.Initialize();
            var datacosts = PersistentDictionaryFactory.CreateDictionary<string>("datacosts");
            var legacyMeasures = EmbeddedResourceHelper.GetEmbeddedResourceAsString(this.GetType(), "Resources.LegacyMeasureMap.js");
            datacosts["LegacyMeasureMap.js"] = legacyMeasures;

            // Set up an in-memory simulated repository. There are too many interior saves
            // for Rhino to be effective.
            this.repository = new SimulatedEntityRepository();

            this.companyEntityId = new EntityId();
            this.campaignEntityId = new EntityId();
            this.reportEntityId = new EntityId();
            this.campaignOwnerId = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));

            // Set up this test with data to support the whole call chain of the activity
            campaignStub.SetupCampaign(this.repository, this.companyEntityId, this.campaignEntityId, this.campaignOwnerId);
            var context = new RequestContext
            {
                ExternalCompanyId = this.companyEntityId,
                EntityFilter = new RepositoryEntityFilter(true, true, true, true)
            };
            var campaignEntity = this.repository.GetEntity<CampaignEntity>(context, this.campaignEntityId);

            var reportEntity1 = ReportEntity.BuildReportEntity(new EntityId(), "reportName", "reportType1", "reportData");
            var reportEntity2 = ReportEntity.BuildReportEntity(new EntityId(), "reportName", "reportType2", "reportData");
            this.repository.SaveEntity(context, reportEntity1);
            this.repository.SaveEntity(context, reportEntity2);

            var existingReports = new List<ReportItem>
                {
                    new ReportItem { ReportDate = reportEntity1.LastModifiedDate, ReportEntityId = reportEntity1.ExternalEntityId.ToString(), ReportType = reportEntity1.ReportType },
                    new ReportItem { ReportDate = reportEntity2.LastModifiedDate, ReportEntityId = reportEntity2.ExternalEntityId.ToString(), ReportType = reportEntity2.ReportType },
                };
            var existingReportsJson = AppsJsonSerializer.SerializeObject(existingReports);
            campaignEntity.TrySetPropertyByName(ReportingPropertyNames.CurrentReports, existingReportsJson, PropertyFilter.Extended);
            this.repository.SaveEntity(context, campaignEntity);
        }

        /// <summary>Happy path campaign report activities scenario.</summary>
        [TestMethod]
        public void CampaignReportActivitiesSuccess()
        {
            ////
            // Set up activity to create new report
            ////
            
            // Use the real handler factory
            this.request = new ActivityRequest
            {
                Task = ReportingActivityTasks.CreateCampaignReport,
                Values =
                {
                    { EntityActivityValues.AuthUserId, this.campaignOwnerId },
                    { EntityActivityValues.CompanyEntityId, this.companyEntityId },
                    { EntityActivityValues.CampaignEntityId, this.campaignEntityId },
                    { EntityActivityValues.EntityId, this.reportEntityId },
                    { ReportingActivityValues.ReportType, ReportTypes.ClientCampaignBilling }
                }
            };
            var activity = Activity.CreateActivity(
                    typeof(CreateCampaignReportActivity),
                    new Dictionary<Type, object> { { typeof(IEntityRepository), this.repository } },
                    ActivityTestHelpers.SubmitActivityRequest);

            this.request.Values[ReportingActivityValues.VerboseReport] = string.Empty;
            var result = activity.Run(this.request);

            // Assert activity completed successfully
            ActivityTestHelpers.AssertValidSuccessResult(result);

            // Assert all the data in the chain to the generated report is present
            var reportEntity = GetReportFromCampaign(
                this.repository, null, this.campaignEntityId, ReportTypes.ClientCampaignBilling);
            Assert.IsFalse(string.IsNullOrEmpty(reportEntity.ReportData));
            Assert.AreEqual(this.reportEntityId, (EntityId)reportEntity.ExternalEntityId);

            // Get the new report id from the activity result
            var newReportId = result.Values[EntityActivityValues.EntityId];
            Assert.AreEqual((string)this.reportEntityId, newReportId);

            ////
            // Set up activity to request report data
            ////
            this.request = new ActivityRequest
            {
                Task = ReportingActivityTasks.GetCampaignReportData,
                Values =
                {
                    { EntityActivityValues.AuthUserId, this.campaignOwnerId },
                    { EntityActivityValues.CompanyEntityId, this.companyEntityId },
                    { EntityActivityValues.CampaignEntityId, this.campaignEntityId },
                    { EntityActivityValues.EntityId, newReportId },
                }
            };
            activity = Activity.CreateActivity(
                    typeof(GetCampaignReportDataActivity),
                    new Dictionary<Type, object> { { typeof(IEntityRepository), this.repository } },
                    ActivityTestHelpers.SubmitActivityRequest);
            result = activity.Run(this.request);

            // Assert activity completed successfully
            ActivityTestHelpers.AssertValidSuccessResult(result);

            var reportData = result.Values[ReportingActivityValues.ReportData];
            Assert.IsFalse(string.IsNullOrEmpty(reportData));
            Assert.IsTrue(reportData.Contains("Lotame"));

            ////
            // Set up activity to request reports metadata for campaign
            ////
            this.request = new ActivityRequest
            {
                Task = ReportingActivityTasks.GetReportsForCampaign,
                Values =
                {
                    { EntityActivityValues.AuthUserId, this.campaignOwnerId },
                    { EntityActivityValues.CompanyEntityId, this.companyEntityId },
                    { EntityActivityValues.CampaignEntityId, this.campaignEntityId },
                }
            };
            activity = Activity.CreateActivity(
                    typeof(GetReportsForCampaignActivity),
                    new Dictionary<Type, object> { { typeof(IEntityRepository), this.repository } },
                    ActivityTestHelpers.SubmitActivityRequest);
            result = activity.Run(this.request);

            // Assert activity completed successfully
            ActivityTestHelpers.AssertValidSuccessResult(result);

            var reportJson = result.Values[ReportingActivityValues.Reports];
            var reportItems = AppsJsonSerializer.DeserializeObject<List<ReportItem>>(reportJson);
            Assert.AreEqual(3, reportItems.Count);
            var newReportItem = reportItems.Single(r => r.ReportEntityId == (string)newReportId);
            Assert.AreEqual(ReportTypes.ClientCampaignBilling, newReportItem.ReportType);
        }

        /// <summary>Get a report from a campaign</summary>
        /// <param name="repository">the repository</param>
        /// <param name="context">the repository request context</param>
        /// <param name="campaignEntityId">the campaign id</param>
        /// <param name="reportType">The report type.</param>
        /// <returns>The report entity.</returns>
        internal static ReportEntity GetReportFromCampaign(
            IEntityRepository repository, 
            RequestContext context, 
            EntityId campaignEntityId, 
            string reportType)
        {
            var campaignEntity = repository.GetEntity<CampaignEntity>(context, campaignEntityId);
            var reportsJson = campaignEntity.TryGetPropertyByName<string>(ReportingPropertyNames.CurrentReports, null);
            var reportItems = AppsJsonSerializer.DeserializeObject<List<ReportItem>>(reportsJson);
            var latestReport = reportItems.Where(r => r.ReportType == reportType).OrderByDescending(r => r.ReportDate).First();
            return repository.GetEntity<ReportEntity>(context, latestReport.ReportEntityId);
        }
    }
}

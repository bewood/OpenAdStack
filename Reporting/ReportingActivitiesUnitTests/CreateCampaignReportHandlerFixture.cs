// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CreateCampaignReportHandlerFixture.cs" company="Rare Crowds Inc">
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
using System.Text;
using DataAccessLayer;
using DynamicAllocation;
using EntityTestUtilities;
using EntityUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReportingActivities;
using ReportingTools;
using ReportingUtilities;
using Rhino.Mocks;
using Utilities;
using Utilities.Serialization;

namespace ReportingActivitiesUnitTests
{
    /// <summary>
    /// Unit test fixture for CreateCampaignReportHandler
    /// </summary>
    [TestClass]
    public class CreateCampaignReportHandlerFixture
    {
        /// <summary>Default repository for testing.</summary>
        private IEntityRepository repository;

        /// <summary>campaign id for testing</summary>
        private EntityId campaignEntityId;

        /// <summary>company id for testing</summary>
        private EntityId companyEntityId;

        /// <summary>report id for testing</summary>
        private EntityId reportEntityId;

        /// <summary>report generators for testing.</summary>
        private IDictionary<DeliveryNetworkDesignation, IReportGenerator> generators;

        /// <summary>fake report for testing.</summary>
        private StringBuilder expectedReport;

        /// <summary>Per-test intitialization.</summary>
        [TestInitialize]
        public void InitializeTest()
        {
            this.companyEntityId = new EntityId();
            this.campaignEntityId = new EntityId();
            this.reportEntityId = new EntityId();
            this.repository = MockRepository.GenerateStub<IEntityRepository>();
            this.expectedReport = new StringBuilder("the report");
            var generator = MockRepository.GenerateStub<IReportGenerator>();
            generator.Stub(f => f.BuildReport(Arg<string>.Is.Anything, Arg<bool>.Is.Anything))
                .Return(this.expectedReport);
            this.generators = new Dictionary<DeliveryNetworkDesignation, IReportGenerator> { { DeliveryNetworkDesignation.AppNexus, generator } };
        }

        /// <summary>Happy path construction success.</summary>
        [TestMethod]
        public void ConstructorSuccess()
        {
            var handler = new CreateCampaignReportHandler(
                this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, "SomeReport");
            Assert.AreSame(this.repository, handler.Repository);
            Assert.AreSame(this.generators, handler.ReportGenerators);
            Assert.AreEqual(this.campaignEntityId, handler.CampaignEntityId);
            Assert.AreEqual(true, handler.BuildVerbose);
            Assert.AreEqual("SomeReport", handler.ReportType);
        }

        /// <summary>Null generators should throw.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void ConstructorNullGeneratorsFail()
        {
            new CreateCampaignReportHandler(this.repository, null, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, "SomeReport");
        }

        /// <summary>Null report entity id should throw.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void ConstructorNullReportIdFail()
        {
            new CreateCampaignReportHandler(this.repository, this.generators, this.companyEntityId, null, null, true, "SomeReport");
        }

        /// <summary>Null report type should throw.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void ConstructorNullReportTypeFail()
        {
            new CreateCampaignReportHandler(this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, null);
        }

        /// <summary>Null report type should throw.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void ConstructorEmptyReportTypeFail()
        {
            new CreateCampaignReportHandler(this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, string.Empty);
        }

        /// <summary>No generators should fail.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void ExecuteNoGeneratorsFail()
        {
            this.generators = new Dictionary<DeliveryNetworkDesignation, IReportGenerator>();
            var handler = new CreateCampaignReportHandler(
                this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, "SomeReport");
            handler.Execute();
        }

        /// <summary>Multiple generators should fail.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void MultipleGeneratorsNotSupportedYet()
        {
            var extraGenerator = MockRepository.GenerateStub<IReportGenerator>();
            this.generators.Add(DeliveryNetworkDesignation.GoogleDfp, extraGenerator);
            var handler = new CreateCampaignReportHandler(
                this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, "SomeReport");
            handler.Execute();
        }

        /// <summary>Anything but AppNexus generator should fail.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void OnlyAppNexusSupported()
        {
            var extraGenerator = MockRepository.GenerateStub<IReportGenerator>();
            this.generators = new Dictionary<DeliveryNetworkDesignation, IReportGenerator>();
            this.generators.Add(DeliveryNetworkDesignation.GoogleDfp, extraGenerator);
            var handler = new CreateCampaignReportHandler(
                this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, "SomeReport");
            handler.Execute();
        }

        /// <summary>Happy path Execute.</summary>
        [TestMethod]
        public void ExecuteSuccess()
        {
            ReportEntity savedReportEntity = null;
            CampaignEntity savedCampaignEntity = null;
            this.SetupRepositoryStubs(e => savedReportEntity = e, e => savedCampaignEntity = e, false, false, false);

            var handler = new CreateCampaignReportHandler(
                this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, "SomeReport");
            var result = handler.Execute();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual((string)this.reportEntityId, result[EntityActivityValues.EntityId]);
            Assert.AreEqual(this.expectedReport.ToString(), savedReportEntity.ReportData);
            var currentReportsJson = savedCampaignEntity.GetPropertyByName<string>(ReportingPropertyNames.CurrentReports);
            var currentReports = AppsJsonSerializer.DeserializeObject<List<ReportItem>>(currentReportsJson);
            var currentItem = currentReports.Single();
            Assert.AreEqual(this.reportEntityId, (EntityId)savedReportEntity.ExternalEntityId);
            Assert.AreEqual(this.reportEntityId, (EntityId)currentItem.ReportEntityId);
            Assert.AreEqual((DateTime)savedReportEntity.LastModifiedDate, currentItem.ReportDate);
        }

        /// <summary>New report reference merged with existing references on campaign.</summary>
        [TestMethod]
        public void ExecuteSuccessExistingReports()
        {
            // Set up a campaign with existing report references
            var campaignEntity = EntityTestHelpers.CreateTestCampaignEntity(
                    this.campaignEntityId, "foo", 0, DateTime.UtcNow, DateTime.UtcNow, "foo");

            var existingReports = new List<ReportItem>
                {
                    new ReportItem { ReportDate = DateTime.UtcNow.AddDays(-1), ReportEntityId = new EntityId(), ReportType = "SomeReport" },
                    new ReportItem { ReportDate = DateTime.UtcNow.AddDays(-1), ReportEntityId = new EntityId(), ReportType = "SomeOtherReport" },
                };
            var existingReportsJson = AppsJsonSerializer.SerializeObject(existingReports);

            campaignEntity.TrySetPropertyByName(ReportingPropertyNames.CurrentReports, existingReportsJson, PropertyFilter.Extended);

            // Setup the repository
            ReportEntity savedReport = null;
            CampaignEntity savedCampaign = null;
            this.SetupRepositoryStubs(e => savedReport = e, c => savedCampaign = c, false, false, false, campaignEntity);

            var handler = new CreateCampaignReportHandler(
                this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, "SomeReport");
            var result = handler.Execute();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual((string)this.reportEntityId, result[EntityActivityValues.EntityId]);

            // Assert the campaign has the new report
            Assert.AreEqual(
                PropertyFilter.Extended, campaignEntity.GetEntityPropertyByName(ReportingPropertyNames.CurrentReports).Filter);
            var newReportsJson = savedCampaign.GetPropertyByName<string>(ReportingPropertyNames.CurrentReports);
            var newReports =
                AppsJsonSerializer.DeserializeObject<List<ReportItem>>(newReportsJson);
            Assert.AreEqual(3, newReports.Count);

            var addedReport =
                newReports.Where(r => r.ReportType == "SomeReport").OrderByDescending(r => r.ReportDate).First();
            Assert.AreEqual(this.reportEntityId, (EntityId)savedReport.ExternalEntityId);
            Assert.AreEqual(this.reportEntityId, (EntityId)addedReport.ReportEntityId);
        }

        /// <summary>Fail to retrieve campaign during execute.</summary>
        [TestMethod]
        [ExpectedException(typeof(DataAccessEntityNotFoundException))]
        public void ExecuteFailCampaignNotFound()
        {
            this.SetupRepositoryStubs(e => { }, c => { }, false, false, true);

            var handler = new CreateCampaignReportHandler(
                this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, "SomeReport");
            handler.Execute();
        }

        /// <summary>Fail to save report blob during execute.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void ExecuteFailSaveBlob()
        {
            this.SetupRepositoryStubs(e => { }, c => { }, true, false, false);

            var handler = new CreateCampaignReportHandler(
                this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, "SomeReport");
            handler.Execute();
        }

        /// <summary>Fail to save updated campaign during execute.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void ExecuteFailSaveCampaign()
        {
            this.SetupRepositoryStubs(e => { }, c => { }, false, true, false);

            var handler = new CreateCampaignReportHandler(
                this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, "SomeReport");
            handler.Execute();
        }

        /// <summary>Happy-path build updated reports json</summary>
        [TestMethod]
        public void BuildUpdatedReportsJsonSuccess()
        {
            var handler = new CreateCampaignReportHandler(
                this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, "SomeReport");

            var existingReports = new List<ReportItem>
                {
                    new ReportItem { ReportDate = DateTime.UtcNow.AddDays(-1), ReportEntityId = new EntityId(), ReportType = "SomeReport" },
                    new ReportItem { ReportDate = DateTime.UtcNow.AddDays(-1), ReportEntityId = new EntityId(), ReportType = "SomeOtherReport" },
                };
            var existingJson = AppsJsonSerializer.SerializeObject(existingReports);

            var reportEntity = ReportEntity.BuildReportEntity(new EntityId(), "ReportName", "SomeReport", "Report Data");
            var updatedReportJson = handler.BuildUpdatedReportsJson(existingJson, reportEntity);
            var updatedReportList = AppsJsonSerializer.DeserializeObject<List<ReportItem>>(updatedReportJson);
            Assert.AreEqual(3, updatedReportList.Count);
            Assert.AreEqual(2, updatedReportList.Count(r => r.ReportType == "SomeReport"));
            var addedReport =
                updatedReportList.Where(r => r.ReportType == "SomeReport").OrderByDescending(r => r.ReportDate).First();
            Assert.AreEqual(reportEntity.ExternalEntityId.ToString(), addedReport.ReportEntityId);
            Assert.AreEqual((DateTime)reportEntity.LastModifiedDate, addedReport.ReportDate);
            Assert.AreEqual(reportEntity.ReportType, addedReport.ReportType);
        }

        /// <summary>Build updated reports json with null for existing.</summary>
        [TestMethod]
        public void BuildUpdatedReportsJsonNull()
        {
            var handler = new CreateCampaignReportHandler(
                this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, "SomeReport");

            var reportEntity = ReportEntity.BuildReportEntity(new EntityId(), "ReportName", "SomeReport", "Report Data");
            var updatedReportJson = handler.BuildUpdatedReportsJson(null, reportEntity);
            var updatedReportList = AppsJsonSerializer.DeserializeObject<List<ReportItem>>(updatedReportJson);
            Assert.AreEqual(1, updatedReportList.Count);
            Assert.AreEqual(reportEntity.ExternalEntityId.ToString(), updatedReportList.Single().ReportEntityId);
        }

        /// <summary>Build updated reports json with empty string for existing.</summary>
        [TestMethod]
        public void BuildUpdatedReportsJsonEmpty()
        {
            var handler = new CreateCampaignReportHandler(
                this.repository, this.generators, this.companyEntityId, this.campaignEntityId, this.reportEntityId, true, "SomeReport");

            var reportEntity = ReportEntity.BuildReportEntity(new EntityId(), "ReportName", "SomeReport", "Report Data");
            var updatedReportJson = handler.BuildUpdatedReportsJson(string.Empty, reportEntity);
            var updatedReportList = AppsJsonSerializer.DeserializeObject<List<ReportItem>>(updatedReportJson);
            Assert.AreEqual(1, updatedReportList.Count);
            Assert.AreEqual(reportEntity.ExternalEntityId.ToString(), updatedReportList.Single().ReportEntityId);
        }

        /// <summary>Setup repository stubs</summary>
        /// <param name="captureReportEntity">Lambda to capture saved report entity.</param>
        /// <param name="captureCampaignEntity">Lambda to capture saved campaign entity.</param>
        /// <param name="reportEntitySaveFail">True if report entity save should fail.</param>
        /// <param name="campaignSaveFail">True if campaign save should fail.</param>
        /// <param name="campaignGetFail">True if campaign get should fail.</param>
        /// <param name="campaignToGet">Campaign to return from get or null for default.</param>
        private void SetupRepositoryStubs(
            Action<ReportEntity> captureReportEntity,
            Action<CampaignEntity> captureCampaignEntity,
            bool reportEntitySaveFail,
            bool campaignSaveFail,
            bool campaignGetFail,
            CampaignEntity campaignToGet = null)
        {
            var campaignEntity = campaignToGet ?? EntityTestHelpers.CreateTestCampaignEntity(
                    this.campaignEntityId, "foo", 0, DateTime.UtcNow, DateTime.UtcNow, "foo");

            RepositoryStubUtilities.SetupGetEntityStub(this.repository, this.campaignEntityId, campaignEntity, campaignGetFail);
            RepositoryStubUtilities.SetupSaveEntityStub(this.repository, captureReportEntity, reportEntitySaveFail);
            RepositoryStubUtilities.SetupSaveEntityStub(this.repository, captureCampaignEntity, campaignSaveFail);
        }
    }
}

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetReportsForCampaignHandlerFixture.cs" company="Rare Crowds Inc">
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
using DataAccessLayer;
using EntityTestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReportingActivities;
using ReportingUtilities;
using Rhino.Mocks;
using Utilities.Serialization;

namespace ReportingActivitiesUnitTests
{
    /// <summary>
    /// Unit test fixture for GetReportsForCampaignHandlerFixture
    /// </summary>
    [TestClass]
    public class GetReportsForCampaignHandlerFixture
    {
        /// <summary>Default repository for testing.</summary>
        private IEntityRepository repository;

        /// <summary>campaign id for testing</summary>
        private EntityId campaignEntityId;

        /// <summary>company id for testing</summary>
        private EntityId companyEntityId;

        /// <summary>Per-test intitialization.</summary>
        [TestInitialize]
        public void InitializeTest()
        {
            this.companyEntityId = new EntityId();
            this.campaignEntityId = new EntityId();
            this.repository = MockRepository.GenerateStub<IEntityRepository>();
        }

        /// <summary>Happy path construction success.</summary>
        [TestMethod]
        public void ConstructorSuccess()
        {
            var handler = new GetReportsForCampaignHandler(this.repository, this.companyEntityId, this.campaignEntityId);
            Assert.AreSame(this.repository, handler.Repository);
            Assert.AreEqual(this.companyEntityId, handler.CompanyEntityId);
            Assert.AreEqual(this.campaignEntityId, handler.CampaignEntityId);
        }

        /// <summary>Happy path Execute.</summary>
        [TestMethod]
        public void ExecuteSuccess()
        {
            var reportEntity1 = ReportEntity.BuildReportEntity(new EntityId(), "reportName", "reportType1", "reportData");
            var reportEntity2 = ReportEntity.BuildReportEntity(new EntityId(), "reportName", "reportType2", "reportData");
            var existingReports = new List<ReportItem>
                {
                    new ReportItem { ReportDate = reportEntity1.LastModifiedDate, ReportEntityId = reportEntity1.ExternalEntityId.ToString(), ReportType = reportEntity1.ReportType },
                    new ReportItem { ReportDate = reportEntity2.LastModifiedDate, ReportEntityId = reportEntity2.ExternalEntityId.ToString(), ReportType = reportEntity2.ReportType },
                };
            var existingReportsJson = AppsJsonSerializer.SerializeObject(existingReports);

            var campaignEntity = EntityTestHelpers.CreateTestCampaignEntity(
                    this.campaignEntityId, "foo", 0, DateTime.UtcNow, DateTime.UtcNow, "foo");
            campaignEntity.TrySetPropertyByName(ReportingPropertyNames.CurrentReports, existingReportsJson, PropertyFilter.Extended);

            RepositoryStubUtilities.SetupGetEntityStub(this.repository, this.campaignEntityId, campaignEntity, false);

            var handler = new GetReportsForCampaignHandler(this.repository, this.companyEntityId, this.campaignEntityId);
            var result = handler.Execute();
            var resultJson = result[ReportingActivityValues.Reports];
            var actualReports = AppsJsonSerializer.DeserializeObject<List<ReportItem>>(resultJson);

            Assert.AreEqual(2, actualReports.Count);
            var reportItem1 = actualReports.Single(r => r.ReportEntityId == reportEntity1.ExternalEntityId.ToString());
            var reportItem2 = actualReports.Single(r => r.ReportEntityId == reportEntity2.ExternalEntityId.ToString());
            Assert.AreEqual(reportEntity1.LastModifiedDate, reportItem1.ReportDate);
            Assert.AreEqual(reportEntity1.ReportType, reportItem1.ReportType);
            Assert.AreEqual(reportEntity2.LastModifiedDate, reportItem2.ReportDate);
            Assert.AreEqual(reportEntity2.ReportType, reportItem2.ReportType);
        }
    }
}

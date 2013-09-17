// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetCampaignReportDataHandlerFixture.cs" company="Rare Crowds Inc">
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
using Utilities;
using Utilities.Serialization;

namespace ReportingActivitiesUnitTests
{
    /// <summary>
    /// Unit test fixture for GetCampaignReportDataHandler
    /// </summary>
    [TestClass]
    public class GetCampaignReportDataHandlerFixture
    {
        /// <summary>Default repository for testing.</summary>
        private IEntityRepository repository;

        /// <summary>campaign id for testing</summary>
        private EntityId campaignEntityId;

        /// <summary>company id for testing</summary>
        private EntityId companyEntityId;

        /// <summary>report id for testing</summary>
        private EntityId reportEntityId;

        /// <summary>Per-test intitialization.</summary>
        [TestInitialize]
        public void InitializeTest()
        {
            this.companyEntityId = new EntityId();
            this.campaignEntityId = new EntityId();
            this.reportEntityId = new EntityId();
            this.repository = MockRepository.GenerateStub<IEntityRepository>();
        }

        /// <summary>Happy path construction success.</summary>
        [TestMethod]
        public void ConstructorSuccess()
        {
            var handler = new GetCampaignReportDataHandler(this.repository, this.companyEntityId, this.campaignEntityId, this.reportEntityId);
            Assert.AreSame(this.repository, handler.Repository);
            Assert.AreEqual(this.companyEntityId, handler.CompanyEntityId);
            Assert.AreEqual(this.campaignEntityId, handler.CampaignEntityId);
            Assert.AreEqual(this.reportEntityId, handler.ReportEntityId);
        }

        /// <summary>Missing report entity id should throw.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void ConstructorNullGeneratorsFail()
        {
            new GetCampaignReportDataHandler(this.repository, this.companyEntityId, this.campaignEntityId, null);
        }

        /// <summary>Happy path Execute.</summary>
        [TestMethod]
        public void ExecuteSuccess()
        {
            var reportEntity1 = ReportEntity.BuildReportEntity(this.reportEntityId, "reportName", "reportType1", "reportData1");
            var reportEntity2 = ReportEntity.BuildReportEntity(new EntityId(), "reportName", "reportType2", "reportData2");
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
            RepositoryStubUtilities.SetupGetEntityStub(this.repository, this.reportEntityId, reportEntity1, false);

            var handler = new GetCampaignReportDataHandler(this.repository, this.companyEntityId, this.campaignEntityId, this.reportEntityId);
            var result = handler.Execute();
            var actualReport = result[ReportingActivityValues.ReportData];
            Assert.AreEqual("reportData1", actualReport);
        }
    }
}

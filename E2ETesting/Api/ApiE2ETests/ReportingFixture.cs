//-----------------------------------------------------------------------
// <copyright file="ReportingFixture.cs" company="Rare Crowds Inc">
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
using System.Net;
using DataAccessLayer;
using DynamicAllocationTestUtilities;
using E2ETestUtilities;
using EntityUtilities;
using Microsoft.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TestUtilities;

namespace ApiIntegrationTests
{
    /// <summary>Tests for campaign API</summary>
    [TestClass]
    public class ReportingFixture : ApiFixtureBase
    {
        /// <summary>Entity repository instance</summary>
        private IEntityRepository entityRepository;

        /// <summary>User access repository instance</summary>
        private IUserAccessRepository userAccessRepository;

        /// <summary>Repository request context</summary>
        private RequestContext context;
        
        /// <summary>Test company entity id</summary>
        private EntityId companyEntityId;

        /// <summary>Test campaign entity id</summary>
        private EntityId campaignEntityId;

        /// <summary>Test campaign owner id</summary>
        private string campaignOwnerId;

        /// <summary>DA campaign test stub</summary>
        private DynamicAllocationCampaignTestStub campaignStub;
     
        /// <summary>Per-test initialization</summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.entityRepository = RuntimeIocHelper.Resolve<IEntityRepository>();
            this.userAccessRepository = RuntimeIocHelper.Resolve<IUserAccessRepository>();

            var ownerIdBytes = new byte[16];
            new Random().NextBytes(ownerIdBytes);
            this.campaignOwnerId = Convert.ToBase64String(ownerIdBytes);
            this.companyEntityId = new EntityId();
            this.campaignEntityId = new EntityId();
            this.context = new RequestContext
            {
                ExternalCompanyId = this.companyEntityId,
                UserId = this.campaignOwnerId,
            };

            this.campaignStub = new DynamicAllocationCampaignTestStub();
            this.campaignStub.SetupCampaign(
                this.entityRepository,
                this.userAccessRepository,
                this.companyEntityId,
                this.campaignEntityId,
                this.campaignOwnerId);

            this.RestClient.Claims[NameIdentifierClaim] = this.campaignOwnerId;
        }

        /// <summary>Round-trip a campaign</summary>
        [TestMethod]
        public void RequestReport()
        {
            // Get the campaign
            var campaignUrl =
                "entity/company/{0}/campaign/{1}"
                .FormatInvariant(this.companyEntityId, this.campaignEntityId);

            var response = this.RestClient.SendRequestUntil(
                HttpMethod.GET,
                campaignUrl,
                (r) => r.StatusCode != HttpStatusCode.Accepted);
            response.AssertStatusCode(HttpStatusCode.OK);
            response.AssertContentIsJson();
            var campaignJson = response.Content.ReadAsString();
            Assert.IsNotNull(campaignJson);

            // Request a report (?)
            var createReportUrl = campaignUrl + "/report";
            var createReportBody = JsonConvert.SerializeObject(
                new Dictionary<string, string>
                {
                    { EntityActivityValues.CompanyEntityId, this.companyEntityId },
                    { EntityActivityValues.CampaignEntityId, this.campaignEntityId },
                });

            response = this.RestClient.SendRequest(HttpMethod.POST, createReportUrl, createReportBody)
                .AssertStatusCode(HttpStatusCode.SeeOther)
                .FollowIfRedirect(this.RestClient);
            response.AssertStatusCode(HttpStatusCode.OK);
        }
    }
}

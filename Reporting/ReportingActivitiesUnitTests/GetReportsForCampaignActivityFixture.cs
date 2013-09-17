﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetReportsForCampaignActivityFixture.cs" company="Rare Crowds Inc">
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
using ActivityTestUtilities;
using DataAccessLayer;
using EntityUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReportingActivities;
using Rhino.Mocks;

namespace ReportingActivitiesUnitTests
{
    /// <summary>Unit test fixture for GetReportsForCampaignActivity.</summary>
    [TestClass]
    public class GetReportsForCampaignActivityFixture
    {
        /// <summary>The activity under test</summary>
        private GetReportsForCampaignActivity activity;

        /// <summary>Entity repository</summary>
        private IEntityRepository entityRepository;

        /// <summary>Request user id for testing.</summary>
        private string userId;

        /// <summary>Request company entity id for testing.</summary>
        private EntityId companyEntityId;

        /// <summary>Request campaign entity id for testing.</summary>
        private EntityId campaignEntityId;

        /// <summary>Request for testing.</summary>
        private ActivityRequest request;

        /// <summary>Stubbed activity handler for testing.</summary>
        private IActivityHandler handler;

        /// <summary>Stubbed activity handler factory for testing.</summary>
        private IActivityHandlerFactory handlerFactory;

        /// <summary>Per-test initialization.</summary>
        [TestInitialize]
        public void InitializeTest()
        {
            this.entityRepository = MockRepository.GenerateMock<IEntityRepository>();
            this.userId = "abcdef012345";
            this.companyEntityId = new EntityId();
            this.campaignEntityId = new EntityId();
            this.request = new ActivityRequest
            {
                Values =
                {
                    { EntityActivityValues.AuthUserId, this.userId },
                    { EntityActivityValues.CompanyEntityId, this.companyEntityId },
                    { EntityActivityValues.CampaignEntityId, this.campaignEntityId },
                }
            };

            this.handler = MockRepository.GenerateStub<IActivityHandler>();
            this.handlerFactory = MockRepository.GenerateStub<IActivityHandlerFactory>();

            // Set up our activity
            // Inject our stubbed handler factory instead of the real one
            this.activity = Activity.CreateActivity(
                    this.handlerFactory,
                    typeof(GetReportsForCampaignActivity),
                    new Dictionary<Type, object> { { typeof(IEntityRepository), this.entityRepository } },
                    ActivityTestHelpers.SubmitActivityRequest) as GetReportsForCampaignActivity;
        }

        /// <summary>Happy path process request</summary>
        [TestMethod]
        public void ProcessRequestSuccess()
        {
            this.handler.Stub(f => f.Execute()).Return(new Dictionary<string, string>());
            this.handlerFactory.Stub(f => f.CreateActivityHandler(null, null)).IgnoreArguments().Return(this.handler);
            var result = this.activity.Run(this.request);

            // Assert activity completed successfully
            ActivityTestHelpers.AssertValidSuccessResult(result);
        }

        /// <summary>Process request returns error result when handler throws.</summary>
        [TestMethod]
        public void ProcessRequestHandlerException()
        {
            this.handler.Stub(f => f.Execute())
                .Throw(new DataAccessEntityNotFoundException("error message"));
            this.handlerFactory.Stub(f => f.CreateActivityHandler(null, null)).IgnoreArguments().Return(this.handler);
            var result = this.activity.Run(this.request);

            // Assert activity completed successfully
            ActivityTestHelpers.AssertValidErrorResult(result, ActivityErrorId.GenericError, "error message");
        }

        /// <summary>Process request returns error when handler factory throws.</summary>
        [TestMethod]
        public void ProcessRequestFactoryException()
        {
            this.handlerFactory.Stub(f => f.CreateActivityHandler(null, null))
                .IgnoreArguments().Throw(new ArgumentException("error message"));
            var result = this.activity.Run(this.request);

            // Assert activity completed successfully
            ActivityTestHelpers.AssertValidErrorResult(result, ActivityErrorId.GenericError, "error message");
        }
    }
}

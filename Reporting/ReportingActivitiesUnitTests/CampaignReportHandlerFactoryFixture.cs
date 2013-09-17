// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CampaignReportHandlerFactoryFixture.cs" company="Rare Crowds Inc">
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReportingActivities;
using ReportingTools;
using ReportingUtilities;
using Rhino.Mocks;
using Utilities;

namespace ReportingActivitiesUnitTests
{
    /// <summary>
    /// Unit test fixture for CreateCampaignReportHandlerFactory
    /// </summary>
    [TestClass]
    public class CampaignReportHandlerFactoryFixture
    {
        /// <summary>Default repository for testing.</summary>
        private IEntityRepository repository;

        /// <summary>campaign id for testing</summary>
        private EntityId campaignEntityId;

        /// <summary>company id for testing</summary>
        private EntityId companyEntityId;

        /// <summary>Request report entity id for testing.</summary>
        private EntityId reportEntityId;

        /// <summary>Activity request for testing</summary>
        private ActivityRequest activityRequest;

        /// <summary>Activity context for testing</summary>
        private Dictionary<Type, object> activityContext;

        /// <summary>Per-test intitialization.</summary>
        [TestInitialize]
        public void InitializeTest()
        {
            this.companyEntityId = new EntityId();
            this.campaignEntityId = new EntityId();
            this.reportEntityId = new EntityId();
            this.repository = MockRepository.GenerateStub<IEntityRepository>();

            // load all fields
            this.activityRequest = new ActivityRequest();
            this.activityRequest.Task = ReportingActivityTasks.CreateCampaignReport;
            this.activityRequest.Values.Add(EntityActivityValues.CompanyEntityId, this.companyEntityId);
            this.activityRequest.Values.Add(EntityActivityValues.CampaignEntityId, this.campaignEntityId);
            this.activityRequest.Values.Add(EntityActivityValues.EntityId, this.reportEntityId);
            this.activityRequest.Values.Add(ReportingActivityValues.VerboseReport, null);
            this.activityRequest.Values.Add(ReportingActivityValues.SaveLegacyConversion, null);
            this.activityRequest.QueryValues.Add(ReportingActivityValues.ReportType, "SomeReport");

            this.activityContext = new Dictionary<Type, object>
                {
                    { typeof(IEntityRepository), this.repository }
                };
        }

        /// <summary>Default constructor test.</summary>
        [TestMethod]
        public void DefaultConstructor()
        {
            var factory = new CampaignReportHandlerFactory();
            Assert.IsInstanceOfType(factory.CampaignFactory, typeof(DynamicAllocationCampaignFactory));
        }

        /// <summary>Happy path GetReportsForCampaignHandler</summary>
        [TestMethod]
        public void CreateActivityHandlerGetReportsForCampaignSuccess()
        {
            this.activityRequest.Task = ReportingActivityTasks.GetReportsForCampaign;
            var factory = new CampaignReportHandlerFactory(MockRepository.GenerateStub<IDynamicAllocationCampaignFactory>());
            var handler = factory.CreateActivityHandler(this.activityRequest, this.activityContext) as GetReportsForCampaignHandler;
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(GetReportsForCampaignHandler));
            Assert.AreSame(this.repository, handler.Repository);
            Assert.AreEqual(this.campaignEntityId, handler.CampaignEntityId);
            Assert.AreEqual(this.companyEntityId, handler.CompanyEntityId);
        }

        /// <summary>Happy path GetCampaignReportDataHandler</summary>
        [TestMethod]
        public void CreateActivityHandlerGetCampaignReportDataSuccess()
        {
            this.activityRequest.Task = ReportingActivityTasks.GetCampaignReportData;
            var factory = new CampaignReportHandlerFactory(MockRepository.GenerateStub<IDynamicAllocationCampaignFactory>());
            var handler = factory.CreateActivityHandler(this.activityRequest, this.activityContext) as GetCampaignReportDataHandler;
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(GetCampaignReportDataHandler));
            Assert.AreSame(this.repository, handler.Repository);
            Assert.AreEqual(this.campaignEntityId, handler.CampaignEntityId);
            Assert.AreEqual(this.companyEntityId, handler.CompanyEntityId);
            Assert.AreEqual(this.reportEntityId, handler.ReportEntityId);
        }

        /// <summary>Happy path CreateCampaignReportHandler</summary>
        [TestMethod]
        public void CreateActivityHandlerCreateCampaignReportSuccess()
        {
            // Setup the DynamicAllocationCampaign stub and factory
            var dynamicAllocationCampaign = MockRepository.GenerateStub<IDynamicAllocationCampaign>();
            dynamicAllocationCampaign.Stub(f => f.DeliveryNetwork).Return(DeliveryNetworkDesignation.AppNexus);
            
            // setup campaign factory stub so it returns a dynamicAllocationCampaign stub.
            var campaignFactory = MockRepository.GenerateStub<IDynamicAllocationCampaignFactory>();
            campaignFactory.Stub(f => f.BuildDynamicAllocationCampaign(this.companyEntityId, this.campaignEntityId))
                .Return(dynamicAllocationCampaign);

            var factory = new CampaignReportHandlerFactory(campaignFactory);
            var handler = factory.CreateActivityHandler(this.activityRequest, this.activityContext) as CreateCampaignReportHandler;
            Assert.IsNotNull(handler);
            Assert.IsInstanceOfType(handler, typeof(CreateCampaignReportHandler));
            Assert.AreSame(this.repository, handler.Repository);
            Assert.AreEqual(this.campaignEntityId, handler.CampaignEntityId);
            Assert.AreEqual(this.companyEntityId, handler.CompanyEntityId);
            Assert.AreEqual(true, handler.BuildVerbose);
            Assert.IsInstanceOfType(handler.ReportGenerators[DeliveryNetworkDesignation.AppNexus], typeof(AppNexusBillingReport));
            Assert.AreEqual("SomeReport", handler.ReportType);
        }

        /// <summary>No matching generator test.</summary>
        [TestMethod]
        public void CreateActivityHandlerNoMatchingReportGenerator()
        {
            // Setup the DynamicAllocationCampaign stub and factory
            var dynamicAllocationCampaign = MockRepository.GenerateStub<IDynamicAllocationCampaign>();
            
            // Unsupported network
            dynamicAllocationCampaign.Stub(f => f.DeliveryNetwork).Return(DeliveryNetworkDesignation.GoogleDfp);

            // setup campaign factory stub so it only returns or dynamicAllocationCampaign stub if the entity id's
            // and SaveLegacyConversion flag match what is base in the activity request.
            var campaignFactory = MockRepository.GenerateStub<IDynamicAllocationCampaignFactory>();
            campaignFactory.Stub(f => f.BuildDynamicAllocationCampaign(this.companyEntityId, this.campaignEntityId))
                .Return(dynamicAllocationCampaign);

            var factory = new CampaignReportHandlerFactory(campaignFactory);
            var handler = factory.CreateActivityHandler(this.activityRequest, this.activityContext) as CreateCampaignReportHandler;
            Assert.IsNotNull(handler);
            Assert.AreEqual(0, handler.ReportGenerators.Count);
        }

        /// <summary>Create handler throws when no task is specified in request.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void CreateActivityHandlerNoTask()
        {
            var campaignFactory = MockRepository.GenerateStub<IDynamicAllocationCampaignFactory>();
            var factory = new CampaignReportHandlerFactory(campaignFactory);
            this.activityRequest.Task = null;
            factory.CreateActivityHandler(this.activityRequest, this.activityContext);
        }

        /// <summary>Create handler throws when unrecognized task is specified in request.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void CreateActivityHandlerUnrecognizedTask()
        {
            var campaignFactory = MockRepository.GenerateStub<IDynamicAllocationCampaignFactory>();
            var factory = new CampaignReportHandlerFactory(campaignFactory);
            this.activityRequest.Task = "NoTheOne";
            factory.CreateActivityHandler(this.activityRequest, this.activityContext);
        }

        /// <summary>Create handler throws when no repository specified.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void CreateActivityHandlerMissingRepository()
        {
            var campaignFactory = MockRepository.GenerateStub<IDynamicAllocationCampaignFactory>();
            var factory = new CampaignReportHandlerFactory(campaignFactory);
            this.activityContext = new Dictionary<Type, object>();
            factory.CreateActivityHandler(this.activityRequest, this.activityContext);
        }

        /// <summary>Create handler throws when company id missing in request.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void CreateActivityHandlerMissingCompanyId()
        {
            var campaignFactory = MockRepository.GenerateStub<IDynamicAllocationCampaignFactory>();
            var factory = new CampaignReportHandlerFactory(campaignFactory);
            this.activityRequest.Values.Remove(EntityActivityValues.CompanyEntityId);
            factory.CreateActivityHandler(this.activityRequest, this.activityContext);
        }

        /// <summary>Create handler throws when campiagn id missing in request.</summary>
        [TestMethod]
        [ExpectedException(typeof(AppsGenericException))]
        public void CreateActivityHandlerMissingCampaignId()
        {
            var campaignFactory = MockRepository.GenerateStub<IDynamicAllocationCampaignFactory>();
            var factory = new CampaignReportHandlerFactory(campaignFactory);
            this.activityRequest.Values.Remove(EntityActivityValues.CampaignEntityId);
            factory.CreateActivityHandler(this.activityRequest, this.activityContext);
        }
    }
}

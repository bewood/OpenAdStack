// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceBaseFixture.cs" company="Rare Crowds Inc">
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
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using Activities;
using ApiLayer;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Queuing;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using WorkItems;

namespace ApiLayerUnitTests
{
    using System.Net;

    /// <summary>Test fixture for the Service Base</summary>
    [TestClass]
    public class ServiceBaseFixture : ServiceBase
    {
        /// <summary>Activity result timeout</summary>
        private const long ActivityResultTimeout = 10;

        /// <summary>Mock Web contect used for testing</summary>
        private IWebOperationContext webContextMock;
        
        /// <summary>Mock outgoing web response context</summary>
        private IOutgoingWebResponseContext outgoingWebResponseContextMock;

        /// <summary>
        /// Initialize the mock queuer before each test and set the timeout values for waiting on the queue
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.webContextMock = MockRepository.GenerateStub<IWebOperationContext>();
            WebContext = this.webContextMock;
            this.outgoingWebResponseContextMock = MockRepository.GenerateStub<IOutgoingWebResponseContext>();
            this.webContextMock.Stub(f => f.OutgoingResponse).Return(this.outgoingWebResponseContextMock);
        }

        /// <summary>
        /// Tests if RunActivity responsds with expected result
        /// </summary>
        [TestMethod]
        public void RunActivityTest()
        {
            var resultWorkItem = new WorkItem();
            ConfigurationManager.AppSettings["ApiLayer.QueueResponsePollTime"] = "0";
            ConfigurationManager.AppSettings["ApiLayer.MaxQueueResponseWaitTime"] = ActivityResultTimeout.ToString(CultureInfo.InvariantCulture);
            IQueuer queuerMock = MockRepository.GenerateStub<IQueuer>();
            queuerMock.Stub(f => f.EnqueueWorkItem(ref Arg<WorkItem>.Ref(Is.Anything(), resultWorkItem).Dummy))
                .Return(true);
            queuerMock.Stub(f => f.CheckWorkItem(Arg<string>.Is.Anything));
            ServiceBase.Queuer = queuerMock;
            ActivityRequest request = new ActivityRequest();
            ActivityResult result = RunActivity(request, true, ActivityResultTimeout);
            Assert.IsFalse(this.Context.Success);
            Assert.AreEqual(this.Context.ErrorDetails.Message, "Message Accepted and Queued successfully");
        }

        /// <summary>
        /// Tests if success json response is built correctly
        /// </summary>
        [TestMethod]
        public void BuildSuccessResponseTest()
        {
            ActivityResult result = new ActivityResult();
            result.Succeeded = true;
            result.Values.Add("Test", "Some Value");
            Context.Success = true;
            var response = this.BuildResponseFromResult(result);
            var responseJson = new StreamReader(response).ReadToEnd();
            Assert.AreEqual(@"{""Test"":""Some Value""}", responseJson);
        }

        /// <summary>
        /// Tests if failure json response is built correctly
        /// </summary>
        [TestMethod]
        public void BuildFailResponseTest()
        {
            var response = this.BuildErrorResponse(HttpStatusCode.InternalServerError, "Fail JSON response");
            Assert.IsFalse(Context.Success);
            Assert.AreEqual("Fail JSON response", Context.ErrorDetails.Message);
            var responseJson = new StreamReader(response).ReadToEnd();
            Assert.IsTrue(responseJson.Contains("Fail JSON response"));
        }

        /// <summary>Implemented by derived classes to write activity results to response content streams</summary>
        /// <param name="result">Result returned from the activity</param>
        /// <param name="writer">Text writer to which the response is written</param>
        protected override void WriteResponse(ActivityResult result, TextWriter writer)
        {
            writer.Write(JsonConvert.SerializeObject(result.Values));
        }
    }
}

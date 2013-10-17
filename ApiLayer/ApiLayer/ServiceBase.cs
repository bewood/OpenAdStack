// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServiceBase.cs" company="Rare Crowds Inc">
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using Activities;
using ConfigManager;
using Diagnostics;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Queuing;
using RuntimeIoc.WebRole;
using Utilities.IdentityFederation;
using WorkItems;

namespace ApiLayer
{
    /// <summary>Common base class for ApiLayer services</summary>
    /// <remarks>The ServiceBase assumes InstanceContextMode.PerCall</remarks>
    public abstract class ServiceBase
    {
        /// <summary>The name identifier claim</summary>
        internal const string NameIdentifierClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        /// <summary>Activity result value key for processing times information</summary>
        protected const string ActivityResultProcessingTimesKey = "ProcessingTimes";

        /// <summary>Activity processed log entry format</summary>
        private const string ProcessedActivityLogEntryFormat = "Processed {0} request - {1} {2}\nProcessing times: {3}\nBuild response time: {4}s\nTotal time: {5}s";

        /// <summary>Queuer used to submit activity work items</summary>
        private static IQueuer queuer;

        /// <summary>Claim retriever used to get the name identifier claim</summary>
        private static IClaimRetriever claimRetriever;

        /// <summary>WebContext used to retrieve the query params </summary>
        private static IWebOperationContext webContext;

        /// <summary>Initializes a new instance of the ServiceBase class.</summary>
        protected ServiceBase()
        {
            if (WebOperationContext.Current != null)
            {
                this.VerifyPerCallInstanceContextMode();
            }

            this.NameIdentifierClaimValue = ClaimRetriever.GetClaimValue(NameIdentifierClaim);
            this.Context = new CallContext();
        }

        /// <summary>Gets or sets CallContext for each request</summary>
        public CallContext Context { get; protected set; }

        /// <summary>Gets or sets WebContext.</summary>
        internal static IWebOperationContext WebContext
        {
            get { return webContext ?? new WebOperationContextWrapper(WebOperationContext.Current); }
            set { webContext = value; }
        }

        /// <summary>Gets or sets the queuer</summary>
        internal static IQueuer Queuer
        {
            get { return queuer = queuer ?? RuntimeIocContainer.Instance.Resolve<IQueuer>(); }
            set { queuer = value; }
        }

        /// <summary>Gets or sets the claim retriever</summary>
        internal static IClaimRetriever ClaimRetriever
        {
            get { return claimRetriever = claimRetriever ?? RuntimeIocContainer.Instance.Resolve<IClaimRetriever>(); }
            set { claimRetriever = value; }
        }

        /// <summary>
        /// Gets the time (in milliseconds) to wait between checks for if a
        /// queued work item has been processed.
        /// </summary>
        internal static int QueueResponsePollTime
        {
            get { return Config.GetIntValue("ApiLayer.QueueResponsePollTime"); }
        }

        /// <summary>
        /// Gets the default time (in milliseconds) to wait for a
        /// queued work item to be processed before giving up.
        /// </summary>
        internal static int DefaultMaxQueueResponseWaitTime
        {
            get { return Config.GetIntValue("ApiLayer.MaxQueueResponseWaitTime"); }
        }

        /// <summary>Gets or sets the NameIdentifierClaimValue from HttpContext for this call</summary>
        protected string NameIdentifierClaimValue { get; set; }

        /// <summary>
        /// Gets the time (in milliseconds) to wait for a queued
        /// work item to be processed before giving up.
        /// </summary>
        protected virtual int MaxQueueResponseWaitTime
        {
            get { return DefaultMaxQueueResponseWaitTime; }
        }

        /// <summary>Serializes entity result values</summary>
        /// <remarks>Creates a JSON response including JSON values inline and serializing non-JSON values</remarks>
        /// <param name="values">Result values</param>
        /// <returns>Response JSON</returns>
        protected static string SerializeValuesAsJson(IDictionary<string, string> values)
        {
            var result = values
                .Where(kvp => kvp.Key != ActivityResultProcessingTimesKey)
                .Select(kvp => new KeyValuePair<string, object>(kvp.Key, TryParseJson(kvp.Value) ?? kvp.Value))
                .ToDictionary();
            return JsonConvert.SerializeObject(result);
        }

        /// <summary>Attempts to parse a json string</summary>
        /// <param name="json">String to parse</param>
        /// <returns>If successful, the deserialized object; otherwise, null.</returns>
        [SuppressMessage("Microsoft.Design", "CA1031", Justification = "Try pattern must not throw")]
        protected static object TryParseJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject(json);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Implemented by derived classes to write activity results to response content streams</summary>
        /// <param name="result">Result returned from the activity</param>
        /// <param name="writer">Text writer to which the response is written</param>
        protected abstract void WriteResponse(ActivityResult result, TextWriter writer);

        /// <summary>Builds an API response from a activity result.</summary>
        /// <param name="result">Result returned from the activity</param>
        /// <returns>Stream that contains the response body</returns>
        protected virtual Stream BuildResponseFromResult(ActivityResult result)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                this.WriteResponse(result, writer);
                writer.Flush();
                return new MemoryStream(Encoding.UTF8.GetBytes(writer.ToString()));
            }
        }

        /// <summary>Builds an error response with details serialized as JSON and an error message</summary>
        /// <param name="code">HTTP status code</param>
        /// <param name="errorMessage">Error message</param>
        /// <param name="errorMessageArgs">Args for the error message (optional)</param>
        /// <returns>Stream that contains the response body</returns>
        protected Stream BuildErrorResponse(HttpStatusCode code, string errorMessage, params object[] errorMessageArgs)
        {
            WebContext.OutgoingResponse.StatusCode = code;
            WebContext.OutgoingResponse.ContentType = "application/json";
            this.SetContextErrorState(code, errorMessage, errorMessageArgs);
            var errorJson = JsonConvert.SerializeObject(this.Context.ErrorDetails);
            return new MemoryStream(Encoding.UTF8.GetBytes(errorJson));
        }

        /// <summary>Runs an activity request and builds a response</summary>
        /// <param name="request">The request to run</param>
        /// <param name="fetchOnly">True if the operation is just a fetch; otherwise, false.</param>
        /// <returns>Stream containing the JSON response.</returns>
        protected Stream ProcessActivity(ActivityRequest request, bool fetchOnly)
        {
            return this.ProcessActivity(request, fetchOnly, this.MaxQueueResponseWaitTime);
        }

        /// <summary>Runs an activity request and builds a response</summary>
        /// <param name="request">The request to run</param>
        /// <param name="fetchOnly">True if the operation is just a fetch; otherwise, false.</param>
        /// <param name="activityResultTimeout">How long to wait for an activity result.</param>
        /// <returns>Stream containing the JSON response.</returns>
        protected Stream ProcessActivity(ActivityRequest request, bool fetchOnly, long activityResultTimeout)
        {
            if (request == null)
            {
                this.SetContextErrorState(HttpStatusCode.InternalServerError, "Error while creating activity request");
                return this.BuildResponseFromResult(null);
            }

            var submitTime = DateTime.UtcNow;
            var result = this.RunActivity(request, fetchOnly, activityResultTimeout);
            if (this.Context.Success)
            {
                if (result == null)
                {
                    this.SetContextErrorState(HttpStatusCode.InternalServerError, "Error while processing activity in ActivityLayer");
                }
                else if (!result.Succeeded)
                {
                    if (result.Error.ErrorId == (int)ActivityErrorId.InvalidEntityId)
                    {
                        this.SetContextErrorState(
                            HttpStatusCode.NotFound,
                            "Invalid entity id");
                    }
                    else if (result.Error.ErrorId == (int)ActivityErrorId.UserAccessDenied)
                    {
                        this.SetContextErrorState(
                            HttpStatusCode.Unauthorized,
                            "Access denied");
                    }
                    else if (result.Error.ErrorId == (int)ActivityErrorId.MissingRequiredInput)
                    {
                        this.SetContextErrorState(
                            HttpStatusCode.BadRequest,
                            "Missing required input: {0}",
                            result.Error.Message);
                    }
                    else
                    {
                        this.SetContextErrorState(
                            HttpStatusCode.InternalServerError,
                            "Error while processing activity in ActivityLayer ({0})",
                            result.Error.ErrorId);
                    }
                }
            }

            var buildResponseStartTime = DateTime.UtcNow;
            var response = this.BuildResponseFromResult(result);
            var buildResponseTime = DateTime.UtcNow - buildResponseStartTime;

            this.LogProcessedActivityStats(result, submitTime, buildResponseTime);

            return response;
        }

        /// <summary>
        /// Creates a work item for the activity, submits it to the queue and 
        /// waits for a response.
        /// </summary>
        /// <param name="request">Request for the activity to be run</param>
        /// <param name="fetchOnly">True if the operation is just a fetch; otherwise, false.</param>
        /// <param name="activityResultTimeout">How long to wait for the activity result.</param>
        /// <returns>The result of processing the activity</returns>     
        [SuppressMessage("Microsoft.Design", "CA1011", Justification = "Will have to check if this can be resolved")]
        protected ActivityResult RunActivity(ActivityRequest request, bool fetchOnly, long activityResultTimeout)
        {
            WorkItem workItem = new WorkItem
            {
                Id = request.Id,
                Category = (fetchOnly ?
                           ActivityRuntimeCategory.InteractiveFetch :
                           ActivityRuntimeCategory.Interactive)
                           .ToString(),
                ResultType = WorkItemResultType.Polled,
                Source = this.GetType().FullName,
                Content = request.SerializeToXml()
            };

            if (!Queuer.EnqueueWorkItem(ref workItem))
            {
                this.SetContextErrorState(HttpStatusCode.InternalServerError, "Unable to queue message");
                return null;
            }

            var enqueuedTime = DateTime.UtcNow;
            while (workItem.Status != WorkItemStatus.Processed && workItem.Status != WorkItemStatus.Failed)
            {
                if ((DateTime.UtcNow - enqueuedTime).TotalMilliseconds > activityResultTimeout)
                {
                    this.Context.Success = false;

                    // TODO : Hide this and pass some other message to api call; need better message
                    this.SetContextErrorState(HttpStatusCode.Accepted, "Message Accepted and Queued successfully");
                    return null;
                }

                Thread.Sleep(QueueResponsePollTime);
                workItem = Queuer.CheckWorkItem(workItem.Id);
            }

            var result = ActivityResult.DeserializeFromXml(workItem.Result);

            // Add processing times to the result for performance auditing
            var processingTimes =
                "in queue: {0}s; in processing: {1}s; results awaiting retrieval: {2}s"
                .FormatInvariant(
                    workItem.TimeInQueue.TotalSeconds,
                    workItem.TimeInProcessing.TotalSeconds,
                    (DateTime.UtcNow - workItem.ProcessingCompleteTime).TotalSeconds);
            result.Values.Add(ActivityResultProcessingTimesKey, processingTimes);

            return result;
        }

        /// <summary>
        /// Sets the context error state
        /// </summary>
        /// <param name="httpStatus">http response desired to responde to client</param>
        /// <param name="responseMessage">error message passed in http response</param>
        /// <param name="responseMessageArgs">args for the error message</param>
        protected void SetContextErrorState(HttpStatusCode httpStatus, string responseMessage, params object[] responseMessageArgs)
        {
            this.Context.Success = false;
            this.Context.ResponseCode = httpStatus;
            if (responseMessage != null)
            {
                this.Context.ErrorDetails.Id = (int)httpStatus;
                this.Context.ErrorDetails.Message = responseMessage.FormatInvariant(responseMessageArgs);
            }
        }

        /// <summary>Verify that the InstanceContextMode is per-call</summary>
        private void VerifyPerCallInstanceContextMode()
        {
            var serviceBehavior = this.GetType().GetCustomAttributes(true).OfType<ServiceBehaviorAttribute>().Single();
            if (serviceBehavior.InstanceContextMode != InstanceContextMode.PerCall)
            {
                var message = "Services inheriting from ServiceBase must use InstanceContextMode.PerCall. '{0}' uses {1}"
                    .FormatInvariant(this.GetType().FullName, serviceBehavior.InstanceContextMode);
                throw new InvalidOperationException(message);
            }
        }

        /// <summary>Logs statistics about the processed activity</summary>
        /// <param name="result">The activity result</param>
        /// <param name="submitTime">Time when the activity was submitted</param>
        /// <param name="buildResponseTime">Time taken to build the response</param>
        private void LogProcessedActivityStats(ActivityResult result, DateTime submitTime, TimeSpan buildResponseTime)
        {
            var processingTimes =
                (result != null && result.Values != null && result.Values.ContainsKey(ActivityResultProcessingTimesKey)) ?
                result.Values[ActivityResultProcessingTimesKey] : "(unavailable)";

            LogManager.Log(
                LogLevels.Trace,
                ProcessedActivityLogEntryFormat,
                this.GetType().Name,
                HttpContext.Current != null ? HttpContext.Current.Request.HttpMethod : string.Empty,
                HttpContext.Current != null ? HttpContext.Current.Request.RawUrl : "(unavailable)",
                result != null ? result.RequestId : "(unavailable)",
                processingTimes,
                buildResponseTime.TotalSeconds,
                (DateTime.UtcNow - submitTime).TotalSeconds);
        }
    }
}
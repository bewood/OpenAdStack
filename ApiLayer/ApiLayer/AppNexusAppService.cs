// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppNexusAppService.cs" company="Rare Crowds Inc">
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
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Xml;
using Activities;
using AppNexusUtilities;
using DataServiceUtilities;
using DynamicAllocationUtilities;
using EntityUtilities;
using Newtonsoft.Json;

namespace ApiLayer
{
    /// <summary>Service for AppNexus App specific functionality</summary>
    /// <remarks>The ServiceBase assumes InstanceContextMode.PerCall</remarks>
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class AppNexusAppService : ServiceBase
    {
        /// <summary>Registration service POST handler</summary>
        /// <param name="postBody">JSON string of the post</param>
        /// <returns>Response stream for HTTP response</returns>
        [WebInvoke(UriTemplate = "register", Method = "POST")]
        public Stream RegisterUser(Stream postBody)
        {
            this.Context.BuildRequestContext(postBody);
            var request = new ActivityRequest
            {
                Task = AppNexusActivityTasks.AppUserRegistration,
                Values =
                {
                    { EntityActivityValues.AuthUserId, this.NameIdentifierClaimValue },
                    { EntityActivityValues.MessagePayload, this.Context.RequestData }
                }
            };
            this.ProcessActivity(request, false);

            // Always return an empty response body
            return new MemoryStream();
        }

        /// <summary>GET handler for AppNexus app creatives</summary>
        /// <returns>Response stream for HTTP response</returns>
        [WebGet(UriTemplate = "creatives")]
        [SuppressMessage("Microsoft.Design", "CA1024", Justification = "Should be method rather than property for consistency")]
        public Stream GetCreatives()
        {
            var companyId = WebContext.IncomingRequest.UriTemplateMatch.QueryParameters["Company"];
            var campaignId = WebContext.IncomingRequest.UriTemplateMatch.QueryParameters["Campaign"];
            var request = new ActivityRequest
            {
                Task = AppNexusActivityTasks.GetCreatives,
                Values =
                {
                    { EntityActivityValues.AuthUserId, this.NameIdentifierClaimValue },
                    { EntityActivityValues.CompanyEntityId, companyId },
                    { EntityActivityValues.CampaignEntityId, campaignId },
                }
            };
            return this.ProcessActivity(request, false);
        }

        /// <summary>GET handler for AppNexus app segment data costs CSV</summary>
        /// <returns>Response stream for HTTP response</returns>
        [WebGet(UriTemplate = "datacost.csv")]
        [SuppressMessage("Microsoft.Design", "CA1024", Justification = "Should be method rather than property for consistency")]
        public Stream GetDataCostCsv()
        {
            var request = new ActivityRequest
            {
                Task = AppNexusActivityTasks.GetDataCostCsv,
                Values =
                {
                    { EntityActivityValues.AuthUserId, this.NameIdentifierClaimValue }
                }
            };
            return this.ProcessActivity(request, false, 30000);
        }

        /// <summary>
        /// Sets the web context status code, content type, and headers
        /// then writes the entity service results to the response
        /// </summary>
        /// <param name="result">Result returned from the activity</param>
        /// <param name="writer">Text writer to which the response is to be written</param>
        protected override void WriteResponse(ActivityResult result, TextWriter writer)
        {
            WebContext.OutgoingResponse.StatusCode = this.Context.ResponseCode;
            WebContext.OutgoingResponse.ContentType = "application/json";
            WebContext.OutgoingResponse.Headers.Add(HttpResponseHeader.CacheControl, "private,no-cache");

            if (result != null && result.Values != null && result.Values.Count > 0)
            {
                writer.Write(result.Values.First().Value);
            }
        }
    }
}
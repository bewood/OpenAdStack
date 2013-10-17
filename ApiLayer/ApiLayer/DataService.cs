﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataService.cs" company="Rare Crowds Inc">
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
using ConfigManager;
using DataServiceUtilities;
using DynamicAllocationUtilities;
using EntityUtilities;
using Newtonsoft.Json;

namespace ApiLayer
{
    /// <summary>Service for retrieving data</summary>
    /// <remarks>The ServiceBase assumes InstanceContextMode.PerCall</remarks>
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class DataService : ServiceBase
    {
        /// <summary>Per-data type request mappings</summary>
        internal static readonly IDictionary<string, RequestMapping> RequestMappings =
            new RequestMapping[]
            {
                new RequestMapping
                {
                    DataType = "measures",
                    ActivityTask = DynamicAllocationActivityTasks.GetMeasures,
                    ParameterMappings =
                    {
                        { "company", EntityActivityValues.CompanyEntityId },
                        { "campaign", EntityActivityValues.CampaignEntityId },
                        { "includecosttypes", DynamicAllocationActivityValues.IncludeCostTypes },
                        { "excludecosttypes", DynamicAllocationActivityValues.ExcludeCostTypes },
                    },
                    ParameterTransforms =
                    {
                        { "id", str => HttpUtility.UrlDecode(str) },
                    }
                },
                new RequestMapping
                {
                    DataType = "apnxadvertisers",
                    ActivityTask = AppNexusActivityTasks.GetAdvertisers,
                },
            }
            .ToDictionary(mapping => mapping.DataType, mapping => mapping);

        /// <summary>
        /// Gets the time (in milliseconds) to wait for a queued
        /// work item to be processed before giving up.
        /// </summary>
        protected override int MaxQueueResponseWaitTime
        {
            get { return Config.GetIntValue("ApiLayer.DataService.MaxQueueResponseWaitTime"); }
        }

        /// <summary>XML data service get handler</summary>
        /// <param name="dataType">Type of data being requested</param>
        /// <returns>response stream for HTTP response</returns>
        [WebGet(UriTemplate = "{dataType}.xml", ResponseFormat = WebMessageFormat.Xml)]
        public Stream GetXmlData(string dataType)
        {
            return this.GetData(
                dataType,
                WebContext.IncomingRequest.UriTemplateMatch.QueryParameters,
                DataServiceResultsFormat.Xml);
        }

        /// <summary>JSON data service get handler</summary>
        /// <param name="dataType">Type of data being requested</param>
        /// <returns>response stream for HTTP response</returns>
        [WebGet(UriTemplate = "{dataType}.js", ResponseFormat = WebMessageFormat.Json)]
        public Stream GetJsonData(string dataType)
        {
            return this.GetData(
                dataType,
                WebContext.IncomingRequest.UriTemplateMatch.QueryParameters,
                DataServiceResultsFormat.Json);
        }

        /// <summary>
        /// Sets the web context status code and content type then
        /// writes the data service results to the response
        /// </summary>        
        /// <param name="result">Result returned from the activity</param>
        /// <param name="writer">Text writer to which the response is to be written</param>
        protected override void WriteResponse(ActivityResult result, TextWriter writer)
        {
            WebContext.OutgoingResponse.StatusCode = this.Context.ResponseCode;

            if (result == null || string.IsNullOrWhiteSpace(result.Values[DataServiceActivityValues.Results]))
            {
                WebContext.OutgoingResponse.StatusCode = HttpStatusCode.NotFound;
            }
            else
            {
                WebContext.OutgoingResponse.Headers.Add(HttpResponseHeader.CacheControl, "private,max-age=7200");
                var format = result.Values[DataServiceActivityValues.ResultsFormat].ToLowerInvariant();
                WebContext.OutgoingResponse.ContentType =
                    format == "xml" ? "text/xml" : "application/json";
                writer.Write(result.Values[DataServiceActivityValues.Results]);
            }
        }

        /// <summary>Gets data from a data service activity</summary>
        /// <param name="dataType">Data type used to look up the request mapping</param>
        /// <param name="queryParameters">Query parameters to map to request values</param>
        /// <param name="format">Result format to request</param>
        /// <returns>Data service response value</returns>
        private Stream GetData(
            string dataType,
            NameValueCollection queryParameters,
            DataServiceResultsFormat format)
        {
            if (string.IsNullOrWhiteSpace(dataType) ||
                !RequestMappings.ContainsKey(dataType))
            {
                return this.BuildErrorResponse(
                    HttpStatusCode.NotFound,
                    "Unknown data type: {0}",
                    dataType);
            }

            var requestMapping = RequestMappings[dataType];
            var request = requestMapping.CreateActivityRequest(
                this.NameIdentifierClaimValue,
                queryParameters,
                format);
            return this.ProcessActivity(request, true);
        }

        /// <summary>
        /// Helper class used to map service requests to activity requests
        /// </summary>
        internal class RequestMapping
        {
            /// <summary>Parameter mappings common to all requests</summary>
            private static IDictionary<string, string> commonParameterMappings =
                new Dictionary<string, string>
            {
                { "mode", DataServiceActivityValues.Mode },
                { "id", DataServiceActivityValues.SubtreePath },
                { "depth", DataServiceActivityValues.Depth },
                { "offset", DataServiceActivityValues.Offset },
                { "count", DataServiceActivityValues.MaxResults },
                { "include", DataServiceActivityValues.Include },
                { "exclude", DataServiceActivityValues.Exclude },
                { "ids", DataServiceActivityValues.Ids },
            };

            /// <summary>
            /// Initializes a new instance of the RequestMapping class.
            /// </summary>
            public RequestMapping()
            {
                this.ParameterMappings = new Dictionary<string, string>();
                this.ParameterTransforms = new Dictionary<string, Func<string, string>>();
            }

            /// <summary>Gets or sets the service request data type</summary>
            public string DataType { get; set; }

            /// <summary>Gets or sets the activity task name</summary>
            public string ActivityTask { get; set; }

            /// <summary>Gets query parameter to activity request value mappings</summary>
            public IDictionary<string, string> ParameterMappings { get; private set; }

            /// <summary>Gets the query parameter to activty request value transforms</summary>
            public IDictionary<string, Func<string, string>> ParameterTransforms { get; private set; }

            /// <summary>
            /// Gets both the instance and the common parameter mappings
            /// </summary>
            private IDictionary<string, string> AllParameterMappings
            {
                get
                {
                    return commonParameterMappings
                        .Concat(this.ParameterMappings)
                        .ToDictionary();
                }
            }

            /// <summary>
            /// Creates an activity request from query parameters using the mapping
            /// </summary>
            /// <param name="namedIdentifierClaim">Named identifier claim</param>
            /// <param name="parameters">Service request query parameters</param>
            /// <param name="format">Results format</param>
            /// <returns>The data service activity request</returns>
            public ActivityRequest CreateActivityRequest(
                string namedIdentifierClaim,
                NameValueCollection parameters,
                DataServiceResultsFormat format)
            {
                // Create the request
                var request = new ActivityRequest
                {
                    Task = this.ActivityTask,
                    Values =
                    {
                        { EntityActivityValues.AuthUserId, namedIdentifierClaim },
                        { DataServiceActivityValues.ResultsFormat, format.ToString() },
                    }
                };

                // Add mapped request values
                request.Values.Add(
                        parameters.AllKeys
                        .Where(key =>
                            this.AllParameterMappings.ContainsKey(key))
                        .ToDictionary(
                            key => this.AllParameterMappings[key],
                            key =>
                                this.ParameterTransforms.ContainsKey(key) ?
                                this.ParameterTransforms[key](parameters.GetValues(key).First()) :
                                parameters.GetValues(key).First()));
                return request;
            }
        }
    }
}
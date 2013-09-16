﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CallContext.cs" company="Rare Crowds Inc">
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
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace ApiLayer
{
    /// <summary>
    /// Placeholder for api call request and response details
    /// </summary>
    public class CallContext
    {
        /// <summary>
        /// Initializes a new instance of the CallContext class
        /// </summary>
        public CallContext()
        {
            this.ErrorDetails = new ErrorResponse();
            this.ResponseCode = HttpStatusCode.OK;
            this.Success = true;
        }

        /// <summary>
        /// Gets or sets Request Json 
        /// </summary>
        public string RequestData { get; set; }        

        /// <summary>
        /// Gets or sets StatusCode of the response 
        /// </summary>
        public HttpStatusCode ResponseCode { get; set; }

        /// <summary>
        /// Gets or sets the entityid for a Get/Put/Delete call
        /// </summary>
        public string RequestEntityId { get; set; }

        /// <summary>
        /// Gets or sets the entityid for a Get/Put/Delete call
        /// </summary>
        public string RequestParentEntityId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether call passed or failed at any point during processing
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets error details of the call
        /// </summary>
        public ErrorResponse ErrorDetails { get; set; }        

        /// <summary>
        /// Sets up the CallContext with input json
        /// </summary>
        /// <param name="requestStream">Request passed into the api call</param>
        public void BuildRequestContext(Stream requestStream)
        {
            using (var reader = new StreamReader(requestStream))
            {
                this.RequestData = reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Sets up the CallContext with entityid
        /// </summary>
        /// <param name="id">entityid passed into the call via URL</param>
        public void BuildRequestContext(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                this.RequestEntityId = id.Trim();
            }
        }

        /// <summary>
        /// Sets up the CallContext with entityid
        /// </summary>
        /// <param name="id">entityid passed into the call via URL</param>
        public void BuildRequestParentContext(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                this.RequestParentEntityId = id.Trim();
            }
        }
    }
}
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Global.asax.cs" company="Rare Crowds Inc">
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
using ConfigManager;
using Utilities.Web;

namespace ApiLayer
{
    /// <summary>
    /// Class for asp.net application which has events related to global application/session objects
    /// </summary>
    public class Global : SecureHttpApplication
    {
        /// <summary>
        /// Gets a value indicating whether to redirect to sign-off when auth errors
        /// occur instead of logging the error and translating the response to a 401
        /// </summary>
        protected override bool RedirectOnAuthErrors
        {
            get { return false; }
        }

        /// <summary>Gets uri patterns for which authentication is not required</summary>
        protected override IEnumerable<string> AnonymousAccessUriPatterns
        {
            get
            {
                return new[]
                {
                    "/api/apnx/register"
                };
            }
        }

        /// <summary>Gets the route prefix to service type mappings</summary>
        protected override IDictionary<string, Type> ServiceRouteMappings
        {
            get
            {
                var routes = new Dictionary<string, Type>
                {
                    { "Entity", typeof(EntityService) },
                    { "Data", typeof(DataService) },
                };

                if (Config.GetBoolValue("AppNexus.IsApp"))
                {
                    routes.Add("Apnx", typeof(AppNexusAppService));
                }

                return routes;
            }
        }

        /// <summary>Gets the runtime dependency container</summary>
        protected override Microsoft.Practices.Unity.IUnityContainer RuntimeIocContainer
        {
            get { return RuntimeIoc.WebRole.RuntimeIocContainer.Instance; }
        }

        /// <summary>Global end request handler</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            // Convert ACS login redirects to 401 responses
            if (this.Context.Response.StatusCode == 302 &&
                this.Context.Response.RedirectLocation != null &&
                this.Context.Response.RedirectLocation.ToLowerInvariant().Contains(".accesscontrol."))
            {
                this.Context.Response.StatusCode = 401;
                this.Context.Response.RedirectLocation = null;
            }
        }
    }
}

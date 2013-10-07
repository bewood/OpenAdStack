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
using System.Globalization;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Web;
using ConfigManager;
using Diagnostics;
using RuntimeIoc.WebRole;

namespace WebLayer
{
    /// <summary>
    /// Class for asp.net application which has events related to global application/session objects
    /// </summary>
    public class Global : HttpApplication
    {
        /// <summary>Backing field for SecurityEnabled</summary>
        private static bool? securityEnabled;
        
        /// <summary>Backing field for SecurityTokenDuration</summary>
        private static TimeSpan? securityTokenDuration;

        /// <summary>Backing field for SecurityTokenAutoRenewal</summary>
        private static TimeSpan? securityTokenAutoRenewal;

        /// <summary>Gets a value indicating whether security is enabled</summary>
        private static bool SecurityEnabled
        {
            get
            {
                return (securityEnabled = securityEnabled ??
                    !Config.GetBoolValue("ACS.SecurityEnabled")).Value;
            }
        }

        /// <summary>Gets how long security tokens should be renewed for</summary>
        private static TimeSpan SecurityTokenDuration
        {
            get
            {
                return (securityTokenDuration = securityTokenDuration ??
                    Config.GetTimeSpanValue("ACS.SecurityTokenDuration")).Value;
            }
        }

        /// <summary>Gets how long before a token is to be automatically renewed</summary>
        /// <remarks>For sliding session expiration</remarks>
        private static TimeSpan SecurityTokenAutoRenewal
        {
            get
            {
                return (securityTokenAutoRenewal = securityTokenAutoRenewal ??
                    Config.GetTimeSpanValue("ACS.SecurityTokenAutoRenewal")).Value;
            }
        }

        /// <summary>Event is called when the application starts</summary>
        /// <param name="sender">caller of the event</param>
        /// <param name="e">parameter sent to the event by the callee</param>
        protected void Application_Start(object sender, EventArgs e)
        {
            LogManager.Initialize(RuntimeIocContainer.Instance.ResolveAll(typeof(ILogger)).Cast<ILogger>());
            LogManager.Log(LogLevels.Information, "WebLayer Application_Start");
        }

        /// <summary>Event that manages sliding session</summary>
        /// <param name="sender">sender of this event</param>
        /// <param name="e">event arguments</param>
        protected void SessionAuthenticationModule_SessionSecurityTokenReceived(object sender, SessionSecurityTokenReceivedEventArgs e)
        {
            if (!SecurityEnabled)
            {
                return;
            }

            var symmetricKey = e.SessionToken.SecurityKeys.OfType<SymmetricSecurityKey>().FirstOrDefault();
            if (DateTime.UtcNow < e.SessionToken.ValidFrom + SecurityTokenAutoRenewal || symmetricKey == null)
            {
                return;
            }

            e.SessionToken = new SessionSecurityToken(
                        e.SessionToken.ClaimsPrincipal,
                        e.SessionToken.ContextId,
                        e.SessionToken.Context,
                        e.SessionToken.EndpointId,
                        SecurityTokenDuration,
                        symmetricKey);
            e.ReissueCookie = true;
        }
        
        /// <summary>Event that occurs during application error</summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arguments</param>
        protected void Application_Error(object sender, EventArgs e)
        {
            var lastError = Server.GetLastError();
            LogManager.Log(LogLevels.Error, "Error: {0}".FormatInvariant(lastError));

            if (lastError is System.Security.SecurityException ||
                lastError is System.Security.Cryptography.CryptographicException ||
                lastError is System.InvalidOperationException)
            {
                Response.Redirect("LogOff.aspx", true);
            }
        }
    }
}
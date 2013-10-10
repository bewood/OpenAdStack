// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecureHttpApplication.cs" company="Rare Crowds Inc">
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
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.ServiceModel.Activation;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;
using ConfigManager;
using Diagnostics;
using Microsoft.Practices.Unity;
using Utilities.IdentityFederation;
using Utilities.Storage;

namespace Utilities.Web
{
    /// <summary>
    /// Class for asp.net application which has events related to global application/session objects
    /// Includes handling for basic, configurable authN/authZ
    /// </summary>
    public abstract class SecureHttpApplication : HttpApplication
    {
        /// <summary>Backing field for securityMode</summary>
        private static ApplicationSecurityMode? securityMode;

        /// <summary>Backing field for SecurityTokenDuration</summary>
        private static TimeSpan? securityTokenDuration;

        /// <summary>Backing field for SecurityTokenAutoRenewal</summary>
        private static TimeSpan? securityTokenAutoRenewal;

        /// <summary>Backing field for AuthErrorRedirect</summary>
        private static string authErrorRedirect;

        /// <summary>Backing field for AuthenticationManager</summary>
        private IAuthenticationManager authenticationManager;

        /// <summary>Backing field for AuthorizationManager</summary>
        private IAuthorizationManager authorizationManager;

        /// <summary>Backing field for AnonymousAccessUris</summary>
        private Regex[] anonymousAccessUris;

        /// <summary>Gets the runtime unity dependency container</summary>
        protected abstract IUnityContainer RuntimeIocContainer { get; }

        /// <summary>Gets the route prefix to service type mappings</summary>
        protected virtual IDictionary<string, Type> ServiceRouteMappings
        {
            get { return null; }
        }

        /// <summary>Gets uri patterns for which authentication is not required</summary>
        protected virtual IEnumerable<string> AnonymousAccessUriPatterns
        {
            get { return null; }
        }

        /// <summary>Gets the authentication manager</summary>
        protected IAuthenticationManager AuthenticationManager
        {
            get
            {
                return this.authenticationManager = this.authenticationManager ??
                    this.RuntimeIocContainer.Resolve<IAuthenticationManager>();
            }
        }

        /// <summary>Gets the authorization manager</summary>
        protected IAuthorizationManager AuthorizationManager
        {
            get
            {
                return this.authorizationManager = this.authorizationManager ??
                    this.RuntimeIocContainer.Resolve<IAuthorizationManager>();
            }
        }

        /// <summary>Gets a value indicating whether security is enabled</summary>
        private static ApplicationSecurityMode SecurityMode
        {
            get
            {
                return (securityMode = securityMode ??
                    Config.GetEnumValue<ApplicationSecurityMode>("ApplicationSecurityMode")).Value;
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
                    Config.GetTimeSpanValue("ACS.SecurityTokenAutoRenew")).Value;
            }
        }

        /// <summary>Gets how long before a token is to be automatically renewed</summary>
        /// <remarks>For sliding session expiration</remarks>
        private static string AuthErrorRedirect
        {
            get
            {
                return authErrorRedirect = authErrorRedirect ??
                    Config.GetValue("ACS.AuthErrorRedirect");
            }
        }

        /// <summary>Gets RegEx patterns matching URIs to which anonymous access is allowed</summary>
        private Regex[] AnonymousAccessUris
        {
            get
            {
                return this.anonymousAccessUris = this.anonymousAccessUris ??
                    (this.AnonymousAccessUriPatterns == null ? new Regex[0] :
                    this.AnonymousAccessUriPatterns.Select(p => new Regex(p)).ToArray());
            }
        }

        /// <summary>Event is called when the application starts</summary>
        /// <param name="sender">caller of the event</param>
        /// <param name="e">parameter sent to the event by the callee</param>
        protected virtual void Application_Start(object sender, EventArgs e)
        {
            LogManager.Initialize(this.RuntimeIocContainer.ResolveAll<ILogger>());
            LogManager.Log(LogLevels.Trace, "{0}.Application_Start", this.GetType().FullName);
            PersistentDictionaryFactory.Initialize(this.RuntimeIocContainer.ResolveAll<IPersistentDictionaryFactory>());

            this.RegisterRoutes();
        }

        /// <summary>Event that manages sliding session</summary>
        /// <param name="sender">sender of this event</param>
        /// <param name="e">event arguments</param>
        protected virtual void SessionAuthenticationModule_SessionSecurityTokenReceived(object sender, SessionSecurityTokenReceivedEventArgs e)
        {
            if (SecurityMode != ApplicationSecurityMode.AcsTokens)
            {
                return;
            }

            // Get the symmetric key and check the token freshness
            var symmetricKey = e.SessionToken.SecurityKeys.OfType<SymmetricSecurityKey>().FirstOrDefault();
            if (DateTime.UtcNow < e.SessionToken.ValidFrom + SecurityTokenAutoRenewal || symmetricKey == null)
            {
                return;
            }

            // Renew "stale" tokens older than SecurityTokenAutoRenewal
            e.SessionToken = new SessionSecurityToken(
                        e.SessionToken.ClaimsPrincipal,
                        e.SessionToken.ContextId,
                        e.SessionToken.Context,
                        e.SessionToken.EndpointId,
                        SecurityTokenDuration,
                        symmetricKey);
            e.ReissueCookie = true;
        }

        /// <summary>Performs authentication using custom IAuthenticationManager</summary>
        /// <param name="sender">caller of the event</param>
        /// <param name="e">parameter sent to the event by the callee</param>
        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (SecurityMode != ApplicationSecurityMode.Custom || this.AnonymousAccessAllowed())
            {
                return;
            }

            // Verify user is valid
            if (!this.AuthenticationManager.CheckValidUser())
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                HttpContext.Current.Response.AddHeader("WWW-Authenticate", "INVALID USER");
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
            }
        }

        /// <summary>Performs authorization using custom IAuthorizationManager</summary>
        /// <param name="sender">caller of the event</param>
        /// <param name="e">parameter sent to the event by the callee</param>
        protected void Application_AuthorizeRequest(object sender, EventArgs e)
        {
            if (SecurityMode != ApplicationSecurityMode.Custom || this.AnonymousAccessAllowed())
            {
                return;
            }

            // Verify user has authorization to access the resource
            if (!this.AuthorizationManager.CheckAccess(
                HttpContext.Current.Request.HttpMethod,
                HttpContext.Current.Request.Url.AbsoluteUri))
            {
                HttpContext.Current.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                HttpContext.Current.Response.AddHeader("WWW-Authenticate", "ACCESS DENIED");
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
            }
        }

        /// <summary>Event that occurs during application error</summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event arguments</param>
        protected virtual void Application_Error(object sender, EventArgs e)
        {
            var lastError = Server.GetLastError();
            LogManager.Log(LogLevels.Error, "Error: {0}".FormatInvariant(lastError));

            if (lastError is System.Security.SecurityException ||
                lastError is System.Security.Cryptography.CryptographicException ||
                lastError is System.InvalidOperationException)
            {
                Response.Redirect(AuthErrorRedirect, true);
            }
        }

        /// <summary>Registers routes handled by this application</summary>
        private void RegisterRoutes()
        {
            if (this.ServiceRouteMappings == null)
            {
                return;
            }

            LogManager.Log(LogLevels.Information, "Registering Routes");

            foreach (var serviceRouteMapping in this.ServiceRouteMappings)
            {
                var routePrefix = serviceRouteMapping.Key;
                var service = serviceRouteMapping.Value;
                var route = new ServiceRoute(routePrefix, new WebServiceHostFactory(), service);
                
                LogManager.Log(LogLevels.Trace, "\tAdding route {0} -> {1}", route.Url, service.FullName);
                RouteTable.Routes.Add(route);
            }
        }

        /// <summary>Checks if anonymous access is allowed to the resource being requested</summary>
        /// <returns>True if anonymous access is allowed; otherwise, false.</returns>
        private bool AnonymousAccessAllowed()
        {
            var uri = HttpContext.Current.Request.Url.AbsolutePath.ToLowerInvariant();
            return this.AnonymousAccessUris.Any(u => u.IsMatch(uri));
        }
    }
}
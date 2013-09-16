//-----------------------------------------------------------------------
// <copyright file="AzureWebRole.cs" company="Rare Crowds Inc">
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
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using ConfigManager;
using Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Utilities.Runtime;
using Utilities.Storage;

namespace WebRole
{
    /// <summary>Host for all workers</summary>
    public class AzureWebRole : RoleEntryPoint
    {
        /// <summary>Initializes the worker role</summary>
        /// <returns>True if initialization succeeds, False if it fails.</returns>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This for the Windows Azure SDK.")]
        public override bool OnStart()
        {
            // Subscribe to the unhandled exception event so they can be logged
            AppDomain.CurrentDomain.UnhandledException += this.UnhandledExceptionLogger;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += delegate(object sender, RoleEnvironmentChangingEventArgs e)
            {
                // If a configuration setting is changing
                if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange))
                {
                    // Set e.Cancel to true to restart this role instance
                    e.Cancel = true;
                }
            };

            QuotaLogger.InitializeDiagnostics();
            return base.OnStart();
        }

        /// <summary>Cleans up the worker role</summary>
        public override void OnStop()
        {
            base.OnStop();
            AppDomain.CurrentDomain.UnhandledException -= this.UnhandledExceptionLogger;
        }

        /// <summary>Logs unhandled exceptions using the LogManager (if available)</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event args</param>
        private void UnhandledExceptionLogger(object sender, UnhandledExceptionEventArgs e)
        {
            LogManager.Log(LogLevels.Error, "!UNHANDLED EXCEPTION! - {0}", e.ExceptionObject);
        }
    }
}

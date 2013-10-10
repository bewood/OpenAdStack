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
using Utilities.Web;

namespace WebLayer
{
    /// <summary>
    /// Class for asp.net application which has events related to global application/session objects
    /// </summary>
    public class Global : SecureHttpApplication
    {
        /// <summary>Gets the runtime dependency container</summary>
        protected override Microsoft.Practices.Unity.IUnityContainer RuntimeIocContainer
        {
            get { return RuntimeIoc.WebRole.RuntimeIocContainer.Instance; }
        }
    }
}
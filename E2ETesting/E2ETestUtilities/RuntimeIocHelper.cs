//-----------------------------------------------------------------------
// <copyright file="RuntimeIocHelper.cs" company="Rare Crowds Inc">
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

using System.Collections.Generic;
using Microsoft.Practices.Unity;
using RuntimeIoc.WorkerRole;

namespace E2ETestUtilities
{
    /// <summary>RuntimeIoc helpers</summary>
    /// <remarks>Provides quick access to the same RuntimeIoc used by the worker role</remarks>
    public static class RuntimeIocHelper
    {
        /// <summary>Resolve an instance the specified type</summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <returns>The resolved type instance</returns>
        public static T Resolve<T>()
        {
            return RuntimeIocContainer.Instance.Resolve<T>();
        }

        /// <summary>Resolve all instances of the specified type</summary>
        /// <typeparam name="T">Type to resolve</typeparam>
        /// <returns>The resolved type instances</returns>
        public static IEnumerable<T> ResolveAll<T>()
        {
            return RuntimeIocContainer.Instance.ResolveAll<T>();
        }
    }
}

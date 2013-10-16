//-----------------------------------------------------------------------
// <copyright file="RestTestClientExtensions.cs" company="Rare Crowds Inc">
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
using System.Threading;
using Microsoft.Http;
using TestUtilities;

namespace E2ETestUtilities
{
    /// <summary>Extensions for the RestTestClient</summary>
    public static class RestTestClientExtensions
    {
        /// <summary>Default retries</summary>
        private const int DefaultRetries = 5;

        /// <summary>Default retry wait</summary>
        private const int DefaultRetryWait = 1000;

        /// <summary>Send the request until the condition is met (or default retries exceeded)</summary>
        /// <param name="client">RestTestClient instance</param>
        /// <param name="method">HTTP method</param>
        /// <param name="objectPath">Object path</param>
        /// <param name="condition">Test condition</param>
        /// <returns>The HttpResponseMessage</returns>
        public static HttpResponseMessage SendRequestUntil(
            this RestTestClient client,
            HttpMethod method,
            string objectPath,
            Func<HttpResponseMessage, bool> condition)
        {
            return SendRequestUntil(client, method, objectPath, null, condition);
        }

        /// <summary>Send the request until the condition is met (or default retries exceeded)</summary>
        /// <param name="client">RestTestClient instance</param>
        /// <param name="method">HTTP method</param>
        /// <param name="objectPath">Object path</param>
        /// <param name="content">Request content</param>
        /// <param name="condition">Test condition</param>
        /// <returns>The HttpResponseMessage</returns>
        public static HttpResponseMessage SendRequestUntil(
            this RestTestClient client,
            HttpMethod method,
            string objectPath,
            string content,
            Func<HttpResponseMessage, bool> condition)
        {
            return SendRequestUntil(client, method, objectPath, content, condition, DefaultRetries, DefaultRetryWait);
        }

        /// <summary>Send the request until the condition is met (or default retries exceeded)</summary>
        /// <param name="client">RestTestClient instance</param>
        /// <param name="method">HTTP method</param>
        /// <param name="objectPath">Object path</param>
        /// <param name="content">Request content</param>
        /// <param name="condition">Test condition</param>
        /// <param name="maxRetries">Maximum retries</param>
        /// <param name="retryWait">Retry wait</param>
        /// <returns>The HttpResponseMessage</returns>
        public static HttpResponseMessage SendRequestUntil(
            this RestTestClient client,
            HttpMethod method,
            string objectPath,
            string content,
            Func<HttpResponseMessage, bool> condition,
            int maxRetries,
            int retryWait)
        {
            var retries = 0;
            HttpResponseMessage response;
            do
            {
                if (retries > 0)
                {
                    Thread.Sleep(retryWait);
                }

                response = client.SendRequest(method, objectPath, content);
            }
            while (!condition(response) && retries++ < maxRetries);

            return response;
        }
    }
}

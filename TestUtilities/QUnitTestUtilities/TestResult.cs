﻿// -----------------------------------------------------------------------
// <copyright file="TestResult.cs" company="Rare Crowds Inc">
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
// <remarks>
//      You may find some additional information about this class at https://github.com/robdmoore/NQUnit and https://github.com/Sqdw/SQUnit.
// </remarks>
// -----------------------------------------------------------------------

using System;
using System.Globalization;

namespace TestUtilities
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Encapsulates the information about a QUnit test, including the pass or fail status.
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Gets or sets file name the QUnit test was run from.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702", Justification = "We should consider turning this rule off.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets name of the test.
        /// </summary>
        public string TestName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the the test is passed or not.
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// Gets or sets the message that specifies the reason for test failure. If the test failed this contains more information explaining why.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the exception occured during execution of qunit test cases. This will be thrown if there was a problem initializing the QUnit test.
        /// </summary>
        public Exception InitializationException { get; set; }

        /// <summary>
        /// Provides a concise string representation of the test so that unit testing libraries can show a reasonable description of the test.
        /// </summary>
        /// <returns>A concise string representation of the test</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}", this.TestName);
        }
    }
}

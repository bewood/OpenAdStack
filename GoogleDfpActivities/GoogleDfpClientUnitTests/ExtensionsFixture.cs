﻿//-----------------------------------------------------------------------
// <copyright file="ExtensionsFixture.cs" company="Rare Crowds Inc">
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
using System.Configuration;
using Google.Api.Ads.Dfp.Lib;
using GoogleDfpClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dfp = Google.Api.Ads.Dfp.v201206;

namespace GoogleDfpClientUnitTests
{
    /// <summary>Tests for GoogleDfpClient.Extensions</summary>
    [TestClass]
    public class ExtensionsFixture
    {
        /// <summary>Initialize test settings, etc</summary>
        [TestInitialize]
        public void TestInitialize()
        {
            ConfigurationManager.AppSettings["GoogleDfp.NetworkTimezone"] = "Pacific Standard Time";
        }

        /// <summary>Test round-tripping a System.DateTime to a DFP DateTime and back</summary>
        [TestMethod]
        public void DateTimeRoundtrip()
        {
            var systemTime = System.DateTime.UtcNow.FloorToSeconds();
            var dfpTime = systemTime.ToDfpDateTime();
            var roundtripTime = dfpTime.ToSystemDateTime(TimeZoneInfo.Utc);
            Assert.AreEqual(systemTime, roundtripTime);
        }

        /// <summary>Test converting a local time DFP DateTime (such as the server returns) into a UTC System.DateTime</summary>
        [TestMethod]
        public void SystemDateTimeFromLocalDfpDateTime()
        {
            var timezoneId = "Pacific Standard Time";
            var phpTimezone = "America/Los_Angeles";
            var expected = new System.DateTime(2012, 4, 2, 1, 22, 30, System.DateTimeKind.Utc);
            var dfpDateTime = new Dfp.DateTime
            {
                date = new Dfp.Date
                {
                    year = 2012,
                    month = 4,
                    day = 1
                },
                hour = 18,
                minute = 22,
                second = 30,
                timeZoneID = phpTimezone
            };
            var systemDateTime = dfpDateTime.ToSystemDateTime(TimeZoneInfo.FindSystemTimeZoneById(timezoneId));
            Assert.AreEqual(expected, systemDateTime);
        }
    }
}

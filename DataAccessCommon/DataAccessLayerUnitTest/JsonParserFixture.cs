// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JSONParserFixture.cs" company="Rare Crowds Inc">
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
using System.Web.Script.Serialization;
using DataAccessLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace TestUtilitiesUnitTests
{
    /// <summary>Test fixture verifying behavior of JSON libraries</summary>
    [TestClass]
    public class JsonParserFixture
    {
        /// <summary>Verify roundtripping of datetimes</summary>
        [TestMethod]
        public void RoundtripDateTime()
        {
            var today = DateTime.UtcNow.Date;
            var todayString = StringConversions.NativeDateTimeToString(today);
            var graph = new Dictionary<string, object>
            {
                { "Today", todayString },
            };

            // Serialize with Newtonsoft.Json
            var json = JsonConvert.SerializeObject(graph);
            Assert.IsNotNull(json);

            // Deserialize with Newtonsoft
            var deserializedN = JsonConvert.DeserializeObject<IDictionary<string, object>>(json);
            Assert.IsNotNull(deserializedN);
            Assert.IsTrue(deserializedN.ContainsKey("Today"));
            Assert.AreEqual(today, deserializedN["Today"]);

            // Deserialize with Microsoft's System.Web.Script.Serialization
            var serializer = new JavaScriptSerializer();
            var deserializedM = serializer.Deserialize<IDictionary<string, object>>(json);
            Assert.IsNotNull(deserializedM);
            Assert.IsTrue(deserializedM.ContainsKey("Today"));
            
            // Parse string datetime using DAL parser
            var parsedDateTime = StringConversions.StringToNativeDateTime((string)deserializedM["Today"]);
            Assert.AreEqual(today, parsedDateTime);            
        }
    }
}

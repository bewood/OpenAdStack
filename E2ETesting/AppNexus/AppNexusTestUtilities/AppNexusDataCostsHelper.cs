//-----------------------------------------------------------------------
// <copyright file="AppNexusDataCostsHelper.cs" company="Rare Crowds Inc">
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

using TestUtilities;
using Utilities.Storage;

namespace AppNexusTestUtilities
{
    /// <summary>Test helpers for loading datacosts</summary>
    public static class AppNexusDataCostsHelper
    {
        /// <summary>Upload data costs from embedded resource to persistent dictionary storage</summary>
        /// <remarks>Persistent dictionary storage must be initialized BEFORE calling this</remarks>
        public static void UploadDataCosts()
        {
            var datacosts = PersistentDictionaryFactory.CreateDictionary<string>("datacosts");
            
            var legacyMeasures = EmbeddedResourceHelper.GetEmbeddedResourceAsString(typeof(AppNexusDataCostsHelper), "Resources.LegacyMeasureMap.js");
            datacosts["LegacyMeasureMap.js"] = legacyMeasures;

            var segmentDataCosts = EmbeddedResourceHelper.GetEmbeddedResourceAsString(typeof(AppNexusDataCostsHelper), "Resources.SegmentDataCosts-2131[RareCrowds].csv");
            datacosts["SegmentDataCosts-2131[RareCrowds].csv"] = segmentDataCosts;
        }
    }
}

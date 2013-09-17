﻿//-----------------------------------------------------------------------
// <copyright file="AdUnitMeasureSource.cs" company="Rare Crowds Inc">
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
using System.Globalization;
using System.Linq;
using DataAccessLayer;
using DynamicAllocation;
using GoogleDfpClient;
using Newtonsoft.Json;
using Utilities.Storage;
using Dfp = Google.Api.Ads.Dfp.v201206;

namespace GoogleDfpActivities.Measures
{
    /// <summary>Measure source for Google DFP AdUnits</summary>
    internal sealed class AdUnitMeasureSource : DfpMeasureSourceBase, IMeasureSource
    {
        /// <summary>Measure type for technology measures</summary>
        public const string TargetingType = "adUnit";

        /// <summary>Measure id network prefix for Google DFP measures</summary>
        public const byte AdUnitMeasureIdPrefix = 3;

        /// <summary>Name for the AdUnit measure source</summary>
        public const string AdUnitMeasureSourceName = "adunits";

        /// <summary>Longevity of the AdUnit measures cache</summary>
        internal static readonly TimeSpan AdUnitCacheLongevity = new TimeSpan(6, 0, 0);

        /// <summary>Initializes a new instance of the AdUnitMeasureSource class</summary>
        /// <param name="companyEntity">CompanyEntity (for config)</param>
        /// <param name="campaignEntity">CampaignEntity (for config)</param>
        public AdUnitMeasureSource(CompanyEntity companyEntity, CampaignEntity campaignEntity)
            : base(AdUnitMeasureIdPrefix, AdUnitMeasureSourceName, companyEntity, campaignEntity)
        {
        }

        /// <summary>Gets the category display name</summary>
        protected override string CategoryDisplayName
        {
            get { return "AdUnit"; }
        }

        /// <summary>Measure type for technology measures</summary>
        protected override string MeasureType
        {
            get { return TargetingType; }
        }

        /// <summary>Fetch the latest AdUnit measure map</summary>
        /// <returns>The latest MeasureMap</returns>
        protected override MeasureMapCacheEntry FetchLatestMeasureMap()
        {
            var adUnits = this.DfpClient.GetAllAdUnits();
            var measures = adUnits
                .ToDictionary(
                    adunit => Convert.ToInt64(adunit.id, CultureInfo.InvariantCulture),
                    adunit => adunit.name)
                .ToDictionary(
                    kvp => this.GetMeasureId(kvp.Key),
                    kvp => this.CreateDfpMeasure(kvp.Value, kvp.Key));
            return new MeasureMapCacheEntry
            {
                Expiry = DateTime.UtcNow + AdUnitCacheLongevity,
                MeasureMapJson = JsonConvert.SerializeObject(measures)
            };
        }
    }
}

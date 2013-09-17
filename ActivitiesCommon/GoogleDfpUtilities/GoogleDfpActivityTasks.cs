﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GoogleDfpActivityTasks.cs" company="Rare Crowds Inc">
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

namespace GoogleDfpUtilities
{
    /// <summary>
    /// Activity task names for Google DFP activities
    /// </summary>
    public static class GoogleDfpActivityTasks
    {
        /// <summary>Google DFP Export DA Campaign Activity</summary>
        public const string ExportDACampaign = "DFPExportDACampaign";

        /// <summary>Google DFP Delete Order Activity</summary>
        public const string DeleteOrder = "DFPDeleteOrder";

        /// <summary>Google DFP Export Creative Activity</summary>
        public const string ExportCreative = "DFPExportCreative";

        /// <summary>Google DFP Update Creative Status Activity</summary>
        public const string UpdateCreativeStatus = "DFPUpdateCreativeStatus";

        /// <summary>Google DFP RequestCampaignReport activity</summary>
        public const string RequestCampaignReport = "DFPRequestCampaignReport";

        /// <summary>Google DFP RetrieveCampaignReport activity</summary>
        public const string RetrieveCampaignReport = "DFPRetrieveCampaignReport";
    }
}

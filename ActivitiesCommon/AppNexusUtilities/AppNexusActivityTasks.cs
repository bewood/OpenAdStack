﻿//-----------------------------------------------------------------------
// <copyright file="AppNexusActivityTasks.cs" company="Rare Crowds Inc">
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

namespace AppNexusUtilities
{
    /// <summary>
    /// Activity task names for AppNexus activities
    /// </summary>
    public static class AppNexusActivityTasks
    {
        /// <summary>AppNexus ExportDACampaign activity</summary>
        public const string ExportDACampaign = "APNXExportDACampaign";

        /// <summary>AppNexus DeleteLineItem activity</summary>
        public const string DeleteLineItem = "APNXDeleteLineItem";

        /// <summary>AppNexus ExportCreative activity</summary>
        public const string ExportCreative = "APNXExportCreative";

        /// <summary>AppNexus UpdateCreativeAuditStatus activity</summary>
        public const string UpdateCreativeAuditStatus = "APNXUpdateCreativeAuditStatus";

        /// <summary>AppNexus ReallocateNow activity</summary>
        public const string ReallocateNow = "APNXReallocateNow";

        /// <summary>AppNexus RequestCampaignReport activity</summary>
        public const string RequestCampaignReport = "APNXRequestCampaignReport";

        /// <summary>AppNexus RetrieveCampaignReport activity</summary>
        public const string RetrieveCampaignReport = "APNXRetrieveCampaignReport";

        /// <summary>AppNexus App User Registration activity</summary>
        public const string AppUserRegistration = "APNXAppUserRegistration";

        /// <summary>AppNexus App New User activity</summary>
        public const string NewAppUser = "APNXNewAppUser";

        /// <summary>AppNexus GetAdvertisers activity</summary>
        public const string GetAdvertisers = "APNXGetAdvertisers";

        /// <summary>AppNexus GetCreatives activity</summary>
        public const string GetCreatives = "APNXGetCreatives";

        /// <summary>AppNexus GetDataCostCsv activity</summary>
        public const string GetDataCostCsv = "APNXGetDataCostCsv";
    }
}

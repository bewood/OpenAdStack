//-----------------------------------------------------------------------
// <copyright file="AppNexusReportIntervalType.cs" company="Rare Crowds Inc">
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

namespace AppNexusClient
{
    /// <summary>
    /// Types of time ranges available for AppNexus delivery reports
    /// </summary>
    /// <seealso href="https://wiki.appnexus.com/display/api/Advertiser+Analytics"/>
    public enum AppNexusReportIntervalType
    {
        /// <summary>Current hour</summary>
        /// <remarks>report_interval value "current_hour"</remarks>
        CurrentHour,

        /// <summary>Last hour</summary>
        /// <remarks>report_interval value "last_hour"</remarks>
        LastHour,

        /// <summary>Current day</summary>
        /// <remarks>report_interval value "today"</remarks>
        Today,

        /// <summary>Previous day</summary>
        /// <remarks>report_interval value "yesterday"</remarks>
        Yesterday,

        /// <summary>Last 48 hours</summary>
        /// <remarks>report_interval value "last_48_hours"</remarks>
        Last48Hours,

        /// <summary>Last 2 days</summary>
        /// <remarks>report_interval value "last_2_days"</remarks>
        Last2Days,

        /// <summary>Last 7 days</summary>
        /// <remarks>report_interval value "last_7_days"</remarks>
        Last7Days,

        /// <summary>Month to date</summary>
        /// <remarks>report_interval value "month_to_date"</remarks>
        MonthToDate,

        /// <summary>Month to yesterday</summary>
        /// <remarks>report_interval value "month_to_yesterday"</remarks>
        MonthToYesterday,

        /// <summary>Quarter to date</summary>
        /// <remarks>report_interval value "quarter_to_date"</remarks>
        QuarterToDate,

        /// <summary>Last month</summary>
        /// <remarks>report_interval value "last_month"</remarks>
        LastMonth,

        /// <summary>Line-item lifetime</summary>
        /// <remarks>report_interval value "lifetime"</remarks>
        Lifetime,
    }
}

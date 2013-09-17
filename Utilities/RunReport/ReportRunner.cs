// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportRunner.cs" company="Rare Crowds Inc">
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
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using Activities;
using DataAccessLayer;
using Diagnostics;
using DynamicAllocation;
using EntityUtilities;
using ReportingActivities;
using ReportingUtilities;
using RunReport.MeasureSourceProviders;
using SimulatedDataStore;
using TestUtilities;
using Utilities.Serialization;
using Utilities.Storage.Testing;
using daName = DynamicAllocationUtilities.DynamicAllocationEntityProperties;

namespace RunReport
{
    /// <summary>Class to manage running a report.</summary>
    public class ReportRunner
    {
        /// <summary>
        /// Gets or sets Repository.
        /// </summary>
        private IEntityRepository Repository { get; set; }

        /// <summary>Main run method.</summary>
        /// <param name="arguments">The command-line arguments.</param>
        public void Run(RunReportArgs arguments)
        {
            this.Initialize(arguments);

            // Setup measure source providers
            SetUpSimulatedPersistantStorage();
            SetUpMeasureSourceProviders("Production");
            
            var companyId = arguments.CompanyEntityId;
            var campaignId = arguments.CampaignEntityId;
            var outputDir = arguments.OutFile.FullName;

            // Load the company and campaign to touch the local cache (and provide an early fail)
            var context = new RequestContext { ExternalCompanyId = companyId };
            this.Repository.GetEntity(context, companyId);
            var campaign = this.Repository.GetEntity(context, campaignId);

            // Setup campaign owner
            var owner = campaign.GetOwnerId();
            var userId = owner;
            var campaignOwnerEntity = CreateTestUserEntity(userId, "Test User");
            this.Repository.SaveUser(null, campaignOwnerEntity);

            var reportType = ReportTypes.ClientCampaignBilling;
            if (arguments.IsDataProviderReport)
            {
                reportType = ReportTypes.DataProviderBilling;
            }

            // Setup activity request
            var request = new ActivityRequest
            {
                Values =
                {
                    { EntityActivityValues.AuthUserId, userId },
                    { EntityActivityValues.CompanyEntityId, companyId },
                    { EntityActivityValues.CampaignEntityId, campaignId },
                    { ReportingActivityValues.ReportType, reportType }
                }
            };

            if (arguments.IsLegacy)
            {
                request.Values[ReportingActivityValues.SaveLegacyConversion] = string.Empty;
            }

            if (arguments.IsVerbose)
            {
                request.Values[ReportingActivityValues.VerboseReport] = string.Empty;
            }

            // Run the activity
            // Set up our activity
            var activity = Activity.CreateActivity(
                    typeof(CreateCampaignReportActivity),
                    new Dictionary<Type, object> { { typeof(IEntityRepository), this.Repository } },
                    SubmitActivityRequest) as CreateCampaignReportActivity;
            var result = activity.Run(request);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "CreateCampaignReportActivity failed. {0}".FormatInvariant(result.Error.Message));
            }

            // Load the report
            var campaignEntity = this.Repository.GetEntity<CampaignEntity>(context, campaignId);
            var reportsJson = campaignEntity.TryGetPropertyByName<string>(ReportingPropertyNames.CurrentReports, null);
            var reportItems = AppsJsonSerializer.DeserializeObject<List<ReportItem>>(reportsJson);
            var latestReport = reportItems.Where(r => r.ReportType == reportType).OrderByDescending(r => r.ReportDate).First();
            var reportDate = latestReport.ReportDate;
            var reportEntity = this.Repository.GetEntity<ReportEntity>(context, latestReport.ReportEntityId);
            var report = reportEntity.ReportData;

            // Write report to file
            WriteReport(outputDir, reportType, campaignEntity.ExternalName, report, reportDate);
        }

        /// <summary>
        /// Creates a test User entity with the specified values
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="externalName">External Name</param>
        /// <returns>The user entity</returns>
        private static UserEntity CreateTestUserEntity(string userId, string externalName)
        {
            var user = new UserEntity(
                new EntityId(),
                new Entity { ExternalName = externalName, LocalVersion = 1 })
            {
                UserId = userId,
            };
            user.SetUserType(UserType.StandAlone);
            return user;
        }

        /// <summary>
        /// Set Up Simulated Persistant Storage
        /// </summary>
        private static void SetUpSimulatedPersistantStorage()
        {
            // Setup simulated persistent storage
            SimulatedPersistentDictionaryFactory.Initialize();
        }

        /// <summary>
        /// Set Up Measure Source Provider Factories
        /// </summary>
        /// <param name="targetProfile">The target profile</param>
        private static void SetUpMeasureSourceProviders(string targetProfile)
        {
            var measureSourceProvider = new AppNexusMeasureSourceProvider(targetProfile);
            MeasureSourceFactory.Initialize(new IMeasureSourceProvider[]
            {
                measureSourceProvider
            });
        }

        /// <summary>Dummy method for activity call</summary>
        /// <param name="request">The activity request.</param>
        /// <param name="sourceName">The source name.</param>
        /// <returns>always true</returns>
        private static bool SubmitActivityRequest(ActivityRequest request, string sourceName)
        {
            return true;
        }

        /// <summary>Write a report to a csv file.</summary>
        /// <param name="path">Directory to write report.</param>
        /// <param name="reportType">The report type.</param>
        /// <param name="name">The report name.</param>
        /// <param name="report">The report string.</param>
        /// <param name="reportDate">The report date.</param>
        private static void WriteReport(string path, string reportType, string name, string report, DateTime reportDate)
        {
            var reportDay = reportDate.ToString("yyyy_MM_dd", CultureInfo.InvariantCulture);
            var reportTime = reportDate.ToString("HH_mm", CultureInfo.InvariantCulture);

            var reportFile = @"{0}_{1}_{2}_{3}.csv".FormatInvariant(
                reportType, name, reportDay, reportTime);

            var fullPath = Path.Combine(Path.GetFullPath(path), reportFile);

            File.WriteAllText(fullPath, report);
        }

        /// <summary>Initialize runtime dependencies.</summary>
        /// <param name="arguments">The command-line arguments.</param>
        private void Initialize(RunReportArgs arguments)
        {
            var logFilePath = @"C:\logs\ReportRuns.log";
            if (arguments.LogFile != null)
            {
                logFilePath = arguments.LogFile.FullName;
            }

            LogManager.Initialize(new[]
                {
                    new FileLogger(logFilePath)
                });

            // TODO: This is a hacky way to get allocation params initialized. How do we have the same defaults
            // as prod?
            AllocationParametersDefaults.Initialize();
            ConfigurationManager.AppSettings["DynamicAllocation.Margin"] = "{0}".FormatInvariant(1 / 0.87);
            ConfigurationManager.AppSettings["DynamicAllocation.PerMilleFees"] = "{0}".FormatInvariant(0);

            MeasureSourceFactory.Initialize(new IMeasureSourceProvider[]
                {
                    new AppNexusActivities.Measures.AppNexusLegacyMeasureSourceProvider(),
                    new AppNexusActivities.Measures.AppNexusMeasureSourceProvider(),
                    new GoogleDfpActivities.Measures.DfpMeasureSourceProvider()
                });

            this.Repository = new SimulatedEntityRepository(
                ConfigurationManager.AppSettings["IndexLocal.ConnectionString"],
                ConfigurationManager.AppSettings["EntityLocal.ConnectionString"],
                ConfigurationManager.AppSettings["IndexReadOnly.ConnectionString"],
                ConfigurationManager.AppSettings["EntityReadOnly.ConnectionString"],
                true);
        }
    }
}

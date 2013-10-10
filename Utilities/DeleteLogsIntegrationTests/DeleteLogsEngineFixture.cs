// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DeleteLogsEngineFixture.cs" company="Rare Crowds Inc">
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
using System.Globalization;
using System.Linq;
using AzureUtilities.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using TestUtilities;
using Utilities.DeleteLogs;

namespace DeleteLogsIntegrationTests
{
    /// <summary>
    /// Test class for the DeleteLogsEngine
    /// </summary>
    [TestClass]
    public class DeleteLogsEngineFixture
    {
        /// <summary>
        /// blobClient to use for testing
        /// </summary>
        private static CloudBlobClient blobClient;

        /// <summary> Initialize the assembly </summary>
        /// <param name="context">the context</param>
        [AssemblyInitialize]
        public static void InitializeAssembly(TestContext context)
        {
            // Force Azure emulated storage to start. DSService can still be running
            // but the emulated storage not available. The most reliable way to make sure
            // it's running and available is to stop it then start again.
            var emulatorRunnerPath = @"C:\Program Files\Microsoft SDKs\Windows Azure\Emulator\csrun.exe";
            AzureEmulatorHelper.StopStorageEmulator(emulatorRunnerPath);
            AzureEmulatorHelper.StartStorageEmulator(emulatorRunnerPath);

            // connect to storage to set up test
            var storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.RetryPolicy = new ExponentialRetry();
        }

        /// <summary>Initialize Azure storage emulator and create log blobs used by tests.</summary>
        [TestInitialize]
        public void InitializeTest()
        {
            // create a log container for testing
            var container = blobClient.GetContainerReference("testlogs");
            container.CreateIfNotExists();

            // add some log blobs to storage for the purposes of deleting them 
            for (var i = 0; i < 12; i++)
            {
                // TODO: the hours to subract (-54) depend on the app config (48). Make that dependency active.
                var logBlob = container.GetBlockBlobReference(
                    "/deployment17(63)/WorkerRole/deployment17(63).Azure.WorkerRole_IN_0/" +
                    DateTime.Now.AddHours(-54 + i).ToString("yyyyMMddHH", CultureInfo.InvariantCulture) +
                    ".0");

                logBlob.UploadText("test junk data");
            }
        }

        /// <summary>
        /// Test for the DeleteLogsEngine
        /// </summary>
        [TestMethod]
        public void DeleteLogsEngineTest()
        {
            // create the input
            var arguments = new DeleteLogsArgs();
            arguments.ConnectionString = "UseDevelopmentStorage=true";
            arguments.HoursAgoThresholdForDeleting = 48;
            arguments.LogContainers = "testlogs";

            var container = blobClient.GetContainerReference("testlogs");
            var blobs = container.ListBlobs(useFlatBlobListing: true).ToList();
            Assert.AreEqual(12, blobs.Count);

            var deleteDate = DateTime.Now.AddHours(-1 * arguments.HoursAgoThresholdForDeleting)
                .ToString("yyyyMMddHH", CultureInfo.InvariantCulture);
            DeleteLogsEngine.DeleteLogs(arguments);

            blobs = container.ListBlobs(useFlatBlobListing: true).ToList();
            Assert.AreEqual(6, blobs.Count);

            foreach (var item in container.ListBlobs(useFlatBlobListing: true))
            {
                var blob = (CloudBlockBlob)item;
                var blobDate = blob.Name.Split('/').Last().Split('.').First();

                Assert.IsFalse(string.Compare(blobDate, deleteDate, StringComparison.OrdinalIgnoreCase) < 0);
            }
        }

        /// <summary>
        /// Clean up
        /// </summary>
        [TestCleanup]
        public void CleanUpTest()
        {   
            // delete the log container used for testing
            var container = blobClient.GetContainerReference("testlogs");
            container.Delete();
        }
    }
}

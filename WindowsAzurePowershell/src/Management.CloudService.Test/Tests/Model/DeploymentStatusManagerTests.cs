﻿// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests.Model
{
    using System.ServiceModel;
    using CloudService.Model;
    using CloudService.Properties;
    using Extensions;
    using Management.Test.Stubs;
    using Services;
    using TestData;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;

    [TestClass]
    public class DeploymentStatusManagerTests : TestBase
    {
        private const string serviceName = "AzureService";
        private readonly string slot = ArgumentConstants.Slots[Slot.Production];

        [TestInitialize]
        public void SetupTest()
        {
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void SetDeploymentStatusProcessTest()
        {
            SimpleServiceManagement channel = new SimpleServiceManagement();
            string newStatus = DeploymentStatus.Suspended;
            string currentStatus = DeploymentStatus.Running;
            bool statusUpdated = false;
            Deployment expectedDeployment = new Deployment(serviceName, slot, newStatus);
            channel.UpdateDeploymentStatusBySlotThunk = ar => 
            {
                statusUpdated = true;
                channel.GetDeploymentBySlotThunk = ar2 => expectedDeployment;
            };
            channel.GetDeploymentBySlotThunk = ar => new Deployment(serviceName, slot, currentStatus);

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                var deploymentManager = new DeploymentStatusManager(channel);
                deploymentManager.ShareChannel = true;
                deploymentManager.CommandRuntime = new MockCommandRuntime();
                deploymentManager.SetDeploymentStatusProcess(service.Paths.RootPath, newStatus, slot, Data.ValidSubscriptionNames[0], serviceName);

                Assert.IsTrue(statusUpdated);
                Deployment actual = ((MockCommandRuntime)deploymentManager.CommandRuntime).OutputPipeline[0] as Deployment;
                Assert.AreEqual<string>(expectedDeployment.Name, actual.Name);
                Assert.AreEqual<string>(expectedDeployment.Status, actual.Status);
                Assert.AreEqual<string>(expectedDeployment.DeploymentSlot, actual.DeploymentSlot);
            }
        }

        [TestMethod]
        public void SetDeploymentStatusProcessSetTransitioningServiceTest()
        {
            SimpleServiceManagement channel = new SimpleServiceManagement();
            string newStatus = DeploymentStatus.Suspended;
            string currentStatus = DeploymentStatus.RunningTransitioning;
            string resultMessage;
            string expectedMessage = string.Format(Resources.ServiceIsInTransitionState, slot, serviceName, currentStatus);
            bool statusUpdated = false;
            channel.UpdateDeploymentStatusBySlotThunk = ar =>
            {
                statusUpdated = true;
                channel.GetDeploymentBySlotThunk = ar2 => new Deployment(serviceName, slot, newStatus);
            };
            channel.GetDeploymentBySlotThunk = ar => new Deployment(serviceName, slot, currentStatus);

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                var deploymentManager = new DeploymentStatusManager(channel);
                deploymentManager.ShareChannel = true;
                deploymentManager.CommandRuntime = new MockCommandRuntime();
                deploymentManager.SetDeploymentStatusProcess(service.Paths.RootPath, newStatus, slot, Data.ValidSubscriptionNames[0], serviceName);
                resultMessage = ((MockCommandRuntime)deploymentManager.CommandRuntime).VerboseStream[0];

                Assert.IsFalse(statusUpdated);
                Assert.IsTrue(resultMessage.Contains(expectedMessage));
            }
        }

        [TestMethod]
        public void SetDeploymentStatusProcessSetStatusToActualStatusTest()
        {
            SimpleServiceManagement channel = new SimpleServiceManagement();
            string newStatus = DeploymentStatus.Suspended;
            string currentStatus = DeploymentStatus.Suspended;
            string resultMessage;
            string expectedMessage = string.Format(Resources.DeploymentAlreadyInState, slot, serviceName, currentStatus);
            bool statusUpdated = false;
            channel.UpdateDeploymentStatusBySlotThunk = ar =>
            {
                statusUpdated = true;
                channel.GetDeploymentBySlotThunk = ar2 => new Deployment(serviceName, slot, newStatus);
            };
            channel.GetDeploymentBySlotThunk = ar => new Deployment(serviceName, slot, currentStatus);

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                var deploymentManager = new DeploymentStatusManager(channel);
                deploymentManager.ShareChannel = true;
                deploymentManager.CommandRuntime = new MockCommandRuntime();
                deploymentManager.SetDeploymentStatusProcess(service.Paths.RootPath, newStatus, slot, Data.ValidSubscriptionNames[0], serviceName);
                resultMessage = ((MockCommandRuntime)deploymentManager.CommandRuntime).VerboseStream[0];

                Assert.IsFalse(statusUpdated);
                Assert.IsTrue(resultMessage.Contains(expectedMessage));
            }
        }

        [TestMethod]
        public void SetDeploymentStatusProcessDeploymentDoesNotExistTest()
        {
            SimpleServiceManagement channel = new SimpleServiceManagement();
            string newStatus = DeploymentStatus.Running;
            string resultMessage;
            string expectedMessage = string.Format(Resources.ServiceSlotDoesNotExist, serviceName, slot);
            bool statusUpdated = false;
            channel.UpdateDeploymentStatusBySlotThunk = ar =>
            {
                statusUpdated = true;
                channel.GetDeploymentBySlotThunk = ar2 => new Deployment(serviceName, slot, newStatus);
            };
            channel.GetDeploymentBySlotThunk = ar => { throw new EndpointNotFoundException(); };

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                AzureService service = new AzureService(files.RootPath, serviceName, null);
                var deploymentManager = new DeploymentStatusManager(channel);
                deploymentManager.ShareChannel = true;
                deploymentManager.CommandRuntime = new MockCommandRuntime();
                deploymentManager.SetDeploymentStatusProcess(service.Paths.RootPath, newStatus, slot, Data.ValidSubscriptionNames[0], serviceName);
                resultMessage = ((MockCommandRuntime)deploymentManager.CommandRuntime).VerboseStream[0];

                Assert.IsFalse(statusUpdated);
                Assert.IsTrue(resultMessage.Contains(expectedMessage));
                Assert.IsTrue(((MockCommandRuntime)deploymentManager.CommandRuntime).OutputPipeline.Count.Equals(0));
            }
        }
    }
}
﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Cryptography.X509Certificates;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Set Windows Azure Service Remote Desktop Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureServiceRemoteDesktopExtension"), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureServiceRemoteDesktopExtensionCommand : BaseAzureServiceRemoteDesktopExtensionCmdlet
    {
        public SetAzureServiceRemoteDesktopExtensionCommand()
            : base()
        {
        }

        public SetAzureServiceRemoteDesktopExtensionCommand(IServiceManagement channel)
            : base(channel)
        {
        }

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Cloud Service Name")]
        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Cloud Service Name")]
        [ValidateNotNullOrEmpty]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Production (default) or Staging.")]
        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Production (default) or Staging.")]
        [ValidateSet(DeploymentSlotType.Production, DeploymentSlotType.Staging, IgnoreCase = true)]
        public override string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [Parameter(Position = 2, Mandatory = false, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        [ValidateNotNullOrEmpty]
        public override string[] Roles
        {
            get;
            set;
        }

        [Parameter(Position = 6, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "X509Certificate used to encrypt password.")]
        [ValidateNotNullOrEmpty]
        public override X509Certificate2 X509Certificate
        {
            get;
            set;
        }

        [Parameter(Position = 6, Mandatory = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Thumbprint of a certificate used for encryption.")]
        [ValidateNotNullOrEmpty]
        public override string Thumbprint
        {
            get;
            set;
        }

        [Parameter(Position = 7, Mandatory = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "ThumbprintAlgorithm associated with the Thumbprint.")]
        [ValidateNotNullOrEmpty]
        public override string ThumbprintAlgorithm
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = true, ParameterSetName = "SetExtension", HelpMessage = "Remote Desktop Credential")]
        [Parameter(Position = 3, Mandatory = true, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Remote Desktop Credential ")]
        [ValidateNotNullOrEmpty]
        public PSCredential Credential
        {
            get;
            set;
        }

        [Parameter(Position = 5, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Remote Desktop User Expiration Date")]
        [Parameter(Position = 5, Mandatory = false, ParameterSetName = "SetExtensionUsingThumbprint", HelpMessage = "Remote Desktop User Expiration Date")]
        [ValidateNotNullOrEmpty]
        public DateTime Expiration
        {
            get;
            set;
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();
            ValidateDeployment();
            ValidateRoles();
            ValidateThumbprint();
            Expiration = Expiration.Equals(default(DateTime)) ? DateTime.Now.AddMonths(6) : Expiration;
        }

        public void ExecuteCommand()
        {
            ValidateParameters();
            ExtensionConfiguration extConfig = ExtensionManager.NewExtensionConfig(Deployment.ExtensionConfiguration);
            ExtensionConfigurationContext configContext = new ExtensionConfigurationContext
            {
                ProviderNameSpace = ExtensionNameSpace,
                Type = ExtensionType,
                Thumbprint = Thumbprint,
                ThumbprintAlgorithm = ThumbprintAlgorithm,
                X509Certificate = X509Certificate,
                PublicConfiguration = string.Format(PublicConfigurationXmlTemplate.ToString(), Credential.UserName, Expiration.ToString("yyyy-MM-dd")),
                PrivateConfiguration = string.Format(PrivateConfigurationXmlTemplate.ToString(), Credential.Password.ConvertToUnsecureString()),
                Roles = Roles != null ? Roles.Select(r => new ExtensionRole(r)).ToList() : new List<ExtensionRole>(new ExtensionRole[] { new ExtensionRole() }),
            };
            ExtensionManager.InstallExtension(configContext, Slot, ref extConfig);
            ChangeDeployment(extConfig);
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}

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
    using System.Management.Automation;
    using Utilities.Common;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Remove Windows Azure Service Diagnostics Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureServiceDiagnosticsExtension"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzureServiceDiagnosticsExtensionCommand : BaseAzureServiceDiagnosticsExtensionCmdlet
    {
        public RemoveAzureServiceDiagnosticsExtensionCommand()
            : base()
        {
        }

        public RemoveAzureServiceDiagnosticsExtensionCommand(IServiceManagement channel)
            : base(channel)
        {
        }

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "RemoveByRoles", HelpMessage = "Cloud Service Name")]
        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "RemoveAll", HelpMessage = "Cloud Service Name")]
        public override string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "RemoveByRoles", HelpMessage = "Deployment Slot: Production (default) or Staging.")]
        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "RemoveAll", HelpMessage = "Deployment Slot: Production (default) or Staging.")]
        [ValidateSet(DeploymentSlotType.Production, DeploymentSlotType.Staging, IgnoreCase = true)]
        public override string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = false, ParameterSetName = "RemoveByRoles", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        public override string[] Roles
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = "RemoveAll", HelpMessage = "If specified uninstall all Diagnostics configurations from the cloud service.")]
        public SwitchParameter UninstallConfiguration
        {
            get;
            set;
        }

        protected override void ValidateParameters()
        {
            base.ValidateParameters();
            ValidateDeployment();
            ValidateRoles();
        }

        public void ExecuteCommand()
        {
            ValidateParameters();

            ExtensionConfiguration extConfig = ExtensionManager.NewExtensionConfig(Deployment);

            bool removed = false;
            if (UninstallConfiguration && ExtensionManager.ExistAnyExtension(extConfig, ExtensionNameSpace, ExtensionType))
            {
                extConfig = ExtensionManager.RemoveAllExtension(extConfig, ExtensionNameSpace, ExtensionType);
                removed = true;
            }
            else if (ExtensionManager.ExistExtension(extConfig, Roles, ExtensionNameSpace, ExtensionType))
            {
                extConfig = ExtensionManager.RemoveExtension(extConfig, Roles, ExtensionNameSpace, ExtensionType);
                removed = true;
            }

            if (removed)
            {
                ChangeDeployment(extConfig);
            }
            else
            {
                WriteWarning(string.Format("No existing {0}.{1} extensions enabled on given roles.", ExtensionNameSpace, ExtensionType));
            }
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}

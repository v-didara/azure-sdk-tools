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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS
{
    using System;
    using System.Management.Automation;
    using Helpers;
    using Model;

    [Cmdlet(VerbsData.Import, "AzureVM"), OutputType(typeof(PersistentVM))]
    public class ImportAzureVMCommand : Cmdlet
    {   
        [Parameter(Position = 0, Mandatory = true, HelpMessage = "Path to the file with the persistent VM role state previously serialized.")]
        [ValidateNotNullOrEmpty]
        public string Path
        {
            get;
            set;
        }

        internal void ExecuteCommand()
        {
            PersistentVM  role = PersistentVMHelper.LoadStateFromFile(Path);
            WriteObject(role, true);
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                ExecuteCommand();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, string.Empty, ErrorCategory.CloseError, null));
            }
        }
    }
}
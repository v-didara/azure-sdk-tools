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

using System;
using System.Collections.Generic;
using System.Security;
using Microsoft.WindowsAzure.Commands.Common.Model;

namespace Microsoft.WindowsAzure.Commands.Common
{
    public interface IAuthenticationFactory
    {
        IEnumerable<Guid> Authenticate(AzureEnvironment environment, out string userId);
        IEnumerable<Guid> Authenticate(AzureEnvironment environment, string userId);
        IEnumerable<Guid> Authenticate(AzureEnvironment environment, string userId, SecureString password);
        IEnumerable<Guid> RefreshUserToken(AzureEnvironment environment, string userId);
        SubscriptionCloudCredentials GetSubscriptionCloudCredentials(Guid subscriptionId);
    }
}

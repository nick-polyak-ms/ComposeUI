﻿/// ********************************************************************************************************
///
/// Morgan Stanley makes this available to you under the Apache License, Version 2.0 (the "License").
/// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
/// See the NOTICE file distributed with this work for additional information regarding copyright ownership.
/// Unless required by applicable law or agreed to in writing, software distributed under the License
/// is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and limitations under the License.
/// Microsoft Visual Studio Solution File, Format Version 12.00
/// 
/// ********************************************************************************************************

using MorganStanley.ComposeUI.Tryouts.Core.Abstractions.Modules;
using NP.Utilities.Attributes;

namespace MorganStanley.ComposeUI.Tryouts.Plugins.Services.ModulesService;

[Implements(typeof(IModuleHostFactory), IsSingleton = true)]
public class ModuleHostFactory : IModuleHostFactory
{
    public IModule CreateModuleHost(ModuleManifest manifest, Guid instanceId)
    {
        switch (manifest.StartupType, manifest.UIType)
        {
            case (StartupType.Executable, UIType.Window):
                return new ExecutableModule(manifest.Name, manifest.Path!, instanceId);
            case (StartupType.None, UIType.Web):
                return new WebpageModule(manifest.Name, manifest.Url!, instanceId);
            default:
                throw new NotSupportedException("Unsupported module type");
        }
    }
}

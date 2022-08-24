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
using System.Reactive.Subjects;

namespace MorganStanley.ComposeUI.Tryouts.Plugins.Services.ModulesService;

public abstract class ModuleBase : IModule
{
    public ModuleBase(string name, Guid instanceId)
    {
        Name = name;
        InstanceId = instanceId;
    }

    public string Name { get; }

    public Guid InstanceId { get; }

    protected readonly Subject<LifecycleEvent> _lifecycleEvents = new Subject<LifecycleEvent>();
    public IObservable<LifecycleEvent> LifecycleEvents => _lifecycleEvents;

    public abstract Task Initialize();

    public abstract Task Launch();

    public abstract Task Teardown();
}

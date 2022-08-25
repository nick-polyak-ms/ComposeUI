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
using System.Diagnostics;

namespace MorganStanley.ComposeUI.Tryouts.Plugins.Services.ModulesService;

internal class ExecutableModule : ModuleBase
{
    private string _launchPath;
    private Process? _mainProcess;
    private bool exitRequested = false;

    public ProcessInfo ProcessInfo { get; }

    public ExecutableModule(string name, string launchPath, Guid instanceId) : base(name, instanceId)
    {
        _launchPath = launchPath;

        ProcessInfo = new ProcessInfo(Name, UIType.Window);
    }

    public override Task Initialize()
    {
        var mainProcess = new Process();
        mainProcess.StartInfo.FileName = _launchPath;
        mainProcess.EnableRaisingEvents = true;
        mainProcess.Exited += ProcessExited;
        _mainProcess = mainProcess;
        return Task.CompletedTask;
    }

    public override async Task Launch()
    {
        _mainProcess!.Start();

        while (((long)_mainProcess.MainWindowHandle) == 0)
        {
            await Task.Delay(1000);
        };


        long mainWindowHndl = (long)_mainProcess.MainWindowHandle;

        ProcessInfo.UiHint = _mainProcess!.Id.ToString();
        ProcessInfo.ProcessMainWindowHandle = mainWindowHndl;

        _lifecycleEvents.OnNext(LifecycleEvent.Started(ProcessInfo, InstanceId));
    }

    private void ProcessExited(object? sender, EventArgs e)
    {
        _lifecycleEvents.OnNext(LifecycleEvent.Stopped(ProcessInfo, InstanceId, exitRequested));
    }

    public async override Task Teardown()
    {
        if (_mainProcess == null)
        {
            _lifecycleEvents.OnNext(LifecycleEvent.Stopped(ProcessInfo, InstanceId, true));
            return;
        }
        try
        {
            exitRequested = true;
            var killNecessary = true;

            if (_mainProcess.CloseMainWindow())
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                if (_mainProcess.HasExited)
                {
                    killNecessary = false;
                }
            }

            if (killNecessary)
            {
                _mainProcess.Kill();
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            if (_mainProcess.HasExited)
            {
                _lifecycleEvents.OnNext(LifecycleEvent.Stopped(ProcessInfo, InstanceId, true));
            }
            else
            {
                _lifecycleEvents.OnNext(LifecycleEvent.StoppingCanceled(ProcessInfo, InstanceId, false));
            }
        }
        finally
        {
            exitRequested = false;
        }
    }
}

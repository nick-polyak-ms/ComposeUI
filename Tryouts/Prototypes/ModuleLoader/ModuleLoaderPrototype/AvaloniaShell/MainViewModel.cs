﻿using Avalonia.Threading;
using ModuleLoaderPrototype;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaShell
{
    internal class MainViewModel : ReactiveObject
    {

        private readonly MessageBasedModuleLoader _moduleLoader = new MessageBasedModuleLoader();


        public MainViewModel()
        {
            _moduleLoader = new MessageBasedModuleLoader();
            _moduleLoader.LifecycleEvents.Subscribe(HandleAppLifecycleEvent);
        }

        private string _app1Path = string.Empty;
        public string App1Path
        {
            get => _app1Path;
            set => this.RaiseAndSetIfChanged(ref _app1Path, value);
        }

        private string _app2Path = string.Empty;
        public string App2Path
        {
            get => _app2Path;
            set => this.RaiseAndSetIfChanged(ref _app2Path, value);
        }

        private int? _pid1;
        public int? Pid1
        {
            get => _pid1;
            set => this.RaiseAndSetIfChanged(ref _pid1, value);
        }

        private int? _pid2;
        public int? Pid2
        {
            get => _pid2;
            set => this.RaiseAndSetIfChanged(ref _pid2, value);
        }

        public void StartApp1()
        {
            _moduleLoader.RequestStartProcess(new LaunchRequest() { name = "app1", path = _app1Path });
        }
        public void StopApp1()
        {
            _moduleLoader.RequestStopProcess("app1");
        }

        public void StartApp2()
        {
            _moduleLoader.RequestStartProcess(new LaunchRequest() { name = "app2", path = _app1Path });
        }
        public void StopApp2()
        {
            _moduleLoader.RequestStopProcess("app2");
        }

        private async void HandleAppLifecycleEvent(LifecycleEvent lifecycleEvent)
        {
            switch (lifecycleEvent.eventType)
            {
                case LifecycleEventType.Started:
                    await SetPid(lifecycleEvent.name, lifecycleEvent.pid);
                    break;
                case LifecycleEventType.Stopped:
                    await SetPid(lifecycleEvent.name, null);
                    break;
            }
        }

        private async Task SetPid(string name, int? pid)
        {
            if (name == "app1")
            {
                Pid1 = pid;
            }
            else if (name == "app2")
            {
                Pid2 = pid;
            }
        }
    }
}

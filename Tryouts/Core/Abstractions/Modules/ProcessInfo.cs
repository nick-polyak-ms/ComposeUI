﻿/// ********************************************************************************************************
///
/// Morgan Stanley makes this available to you under the Apache License, Version 2.0 (the "License").
/// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0.
/// See the NOTICE file distributed with this work for additional information regarding copyright ownership.
/// Unless required by applicable law or agreed to in writing, software distributed under the License
/// is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and limitations under the License.
/// 
/// ********************************************************************************************************

namespace MorganStanley.ComposeUI.Tryouts.Core.Abstractions.Modules;

public class ProcessInfo
{
    public ProcessInfo(string name, string uiType, string? uiHint = null)
    {
        Name = name;
        UiType = uiType;
        UiHint = uiHint;
    }
        
    /// <summary>
    /// The name of the module
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The UI type of the module
    /// </summary>
    public string UiType { get; }

    /// <summary>
    /// Hint to displying the UI of the module.
    ///  - Web: URL to navigate
    ///  - Window: ProcessID of the process owning the main window
    /// </summary>
    public string? UiHint 
    { 
        get; 
        set; 
    }

    long _processMainWindowHandle = 0;
    public long ProcessMainWindowHandle 
    {
        get => _processMainWindowHandle;
        set
        {
            if (_processMainWindowHandle == value)
                return;

            _processMainWindowHandle = value;
        }
    }
}

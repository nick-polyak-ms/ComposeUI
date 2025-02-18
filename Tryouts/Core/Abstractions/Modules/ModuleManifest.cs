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

namespace MorganStanley.ComposeUI.Tryouts.Core.Abstractions.Modules;

public class ModuleManifest
{
    public string Name { get; set; }
    public string StartupType { get; set; }
    public string UIType { get; set; }
    public string? Path { get; set; }
    public string? Url { get; set; }
    public string[]? Arguments { get; set; }
    public int? Port { get; set; }
}

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

using NP.IoCy;
using System.Windows;

namespace AnotherWpfThemeReceiverApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IoCContainer Container { get; } = new IoCContainer();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Container.InjectPluginsFromSubFolders("Plugins/Services");
            Container.InjectPluginsFromSubFolders("Plugins/WpfServices");
            Container.MapSingleton<MainWindow, MainWindow>();

            Container.CompleteConfiguration();

            MainWindow = Container.Resolve<MainWindow>();

            MainWindow.Show();
        }
    }
}

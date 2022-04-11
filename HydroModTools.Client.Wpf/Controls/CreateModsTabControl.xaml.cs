﻿using HydroModTools.Client.Wpf.ControlModels;
using HydroModTools.Client.Wpf.DI;
using ReactiveUI;
using System.Windows.Controls;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Linq;

namespace HydroModTools.Client.Wpf.Controls
{
    internal partial class CreateModsTabControl : UserControl, IViewFor<CreateModsTabControlModel>
    {
        public CreateModsTabControl()
        {
            ViewModel = WpfFactory.CreateViewModel<CreateModsTabControlModel>();

            InitializeComponent();

            var projectsTab = WpfFactory.CreateControl<ProjectsTabControl>();
            projectsTab.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            projectsTab.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            ProjectsTab.Content = projectsTab;

            /*var guidsTab = WpfFactory.CreateControl<CreateModsTabControl>();
            guidsTab.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            guidsTab.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            GUIDsTab.Content = guidsTab;*/

            this.WhenActivated(d => {
                TabSelector
                    .Events()
                    .SelectionChanged
                    .Select(o => o.Source as TabControl)
                    .WhereNotNull()
                    .Select(tabSelector => tabSelector.SelectedIndex)
                    .InvokeCommand(ViewModel.SetSelectedTabCommand);
            });
        }

        object? IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (CreateModsTabControlModel)value!;
        }

        public CreateModsTabControlModel? ViewModel { get; set; }
    }
}

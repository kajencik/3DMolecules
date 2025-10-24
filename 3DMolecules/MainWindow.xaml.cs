using System.Windows;
using ThreeDMolecules.ViewModels;

namespace ThreeDMolecules
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => helixViewport.ZoomExtents();
            Closed += OnClosed;
        }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (DataContext is SimulationViewModel vm)
            {
                vm.ResetPerformed += (_, _) => helixViewport.ZoomExtents();
            }
        }

        private void OnClosed(object? sender, System.EventArgs e)
        {
            // Properly dispose ViewModel resources
            if (DataContext is SimulationViewModel viewModel)
            {
                viewModel.Dispose();
            }
        }
    }
}
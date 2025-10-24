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
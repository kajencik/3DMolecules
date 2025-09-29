using System.Windows;

namespace ThreeDMolecules
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => helixViewport.ZoomExtents();
        }
    }
}
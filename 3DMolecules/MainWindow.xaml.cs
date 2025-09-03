using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;
using System.Collections.Generic;
using System.Diagnostics;

namespace ThreeDMolecules
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly Model3DGroup _sphereGroup;
        private readonly Model3DGroup _moleculeGroup;
        private readonly List<Molecule> _molecules;
        private DateTime _lastFrameTime;
        private int _frameCount;
        private double _fps;

        public MainWindow()
        {
            InitializeComponent();

            _sphereGroup = new Model3DGroup();
            _moleculeGroup = new Model3DGroup();
            _molecules = new List<Molecule>();

            // Add test spheres
            //AddTestSpheres();

            var random = new Random();

            // Create and add multiple water molecules at different positions with different rotation speeds and axes
            for (int i = 0; i < 120; i++)
            {
                var position = new Point3D(
                    random.NextDouble() * 16 - 8, // X position between -8 and 8
                    random.NextDouble() * 16 - 8, // Y position between -8 and 8
                    random.NextDouble() * 16 - 8  // Z position between -8 and 8
                );

                var velocity = new Vector3D(
                    random.NextDouble() * 0.04 - 0.02, // X velocity between -0.02 and 0.02
                    random.NextDouble() * 0.04 - 0.02, // Y velocity between -0.02 and 0.02
                    random.NextDouble() * 0.04 - 0.02  // Z velocity between -0.02 and 0.02
                );

                var rotationSpeed = random.NextDouble() * 4 + 1; // Rotation speed between 1 and 5
                var rotationAxis = new Vector3D(
                    random.NextDouble() * 2 - 1, // X axis between -1 and 1
                    random.NextDouble() * 2 - 1, // Y axis between -1 and 1
                    random.NextDouble() * 2 - 1  // Z axis between -1 and 1
                );

                AddMolecule(position, velocity, rotationSpeed, rotationAxis);
            }

            var sphereVisual = new ModelVisual3D { Content = _sphereGroup };
            var moleculeVisual = new ModelVisual3D { Content = _moleculeGroup };
            helixViewport.Children.Add(moleculeVisual); // Add molecules first
            helixViewport.Children.Add(sphereVisual); // Add spheres second

            // Add the cylindrical boundary to the scene LAST for correct transparency rendering
            var boundary = new CylindricalBoundary(10.0, 20.0); // Use the same radius/height as in Molecule.cs
            var boundaryVisual = new ModelVisual3D { Content = boundary.Model };
            helixViewport.Children.Add(boundaryVisual);

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(15) };
            _timer.Tick += OnTimerTick;
            _timer.Start();

            _lastFrameTime = DateTime.Now;
        }

        private void AddMolecule(Point3D center, Vector3D velocity, double rotationSpeed, Vector3D rotationAxis)
        {
            var molecule = new Molecule(center, velocity, rotationSpeed, rotationAxis);
            _molecules.Add(molecule);
            _moleculeGroup.Children.Add(molecule.Model);
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            foreach (var molecule in _molecules)
            {
                molecule.Update();
            }

            // Check for collisions
            for (int i = 0; i < _molecules.Count; i++)
            {
                for (int j = i + 1; j < _molecules.Count; j++)
                {
                    if (_molecules[i].IsCollidingWith(_molecules[j]))
                    {
                        _molecules[i].HandleCollision(_molecules[j]);
                    }
                }
            }

            // Update FPS counter
            _frameCount++;
            var currentTime = DateTime.Now;
            var elapsedTime = (currentTime - _lastFrameTime).TotalSeconds;
            if (elapsedTime >= 1)
            {
                _fps = _frameCount / elapsedTime;
                _frameCount = 0;
                _lastFrameTime = currentTime;
                Debug.WriteLine($"FPS: {_fps:F2}");
            }
        }

        //private void AddTestSpheres()
        //{
        //    var sphereMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(160, 255, 0, 0))); // Barely visible red
        //    var sphereMeshBuilder = new MeshBuilder();
        //    sphereMeshBuilder.AddSphere(new Point3D(0, 0, 0), 3.0); // Example sphere at the center with intermediate radius
        //    var sphereMesh = sphereMeshBuilder.ToMesh();
        //    var sphereModel = new GeometryModel3D(sphereMesh, sphereMaterial);

        //    // Set the material to be transparent
        //    sphereModel.Material = sphereMaterial;
        //    sphereModel.BackMaterial = sphereMaterial;

        //    _sphereGroup.Children.Add(sphereModel);

        //    // Add more test spheres at different positions with intermediate radius
        //    sphereMeshBuilder = new MeshBuilder();
        //    sphereMeshBuilder.AddSphere(new Point3D(5, 5, 4), 3.0); // Example sphere at (5, 5, 4)
        //    sphereMesh = sphereMeshBuilder.ToMesh();
        //    sphereModel = new GeometryModel3D(sphereMesh, sphereMaterial);

        //    // Set the material to be transparent
        //    sphereModel.Material = sphereMaterial;
        //    sphereModel.BackMaterial = sphereMaterial;

        //    _sphereGroup.Children.Add(sphereModel);

        //    sphereMeshBuilder = new MeshBuilder();
        //    sphereMeshBuilder.AddSphere(new Point3D(-5, -5, -4), 3.0); // Example sphere at (-5, -5, -4)
        //    sphereMesh = sphereMeshBuilder.ToMesh();
        //    sphereModel = new GeometryModel3D(sphereMesh, sphereMaterial);

        //    // Set the material to be transparent
        //    sphereModel.Material = sphereMaterial;
        //    sphereModel.BackMaterial = sphereMaterial;

        //    _sphereGroup.Children.Add(sphereModel);
        //}
    }
}
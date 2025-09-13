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
    /// Main application window for 3DMolecules.
    /// Responsible for initializing the 3D scene, adding molecules and boundaries, and running the simulation loop.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _timer; // Timer for animation loop
        private readonly Model3DGroup _sphereGroup; // Group for test spheres (optional)
        private readonly Model3DGroup _moleculeGroup; // Group for all molecule models
        private readonly List<Molecule> _molecules; // List of molecule objects
        private DateTime _lastFrameTime;
        private int _frameCount;
        private double _fps;

        /// <summary>
        /// Initializes the main window, sets up the 3D scene, and starts the simulation timer.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _sphereGroup = new Model3DGroup();
            _moleculeGroup = new Model3DGroup();
            _molecules = new List<Molecule>();

            // Add test spheres (optional, for debugging)
            //AddTestSpheres();

            var random = new Random();

            // Create and add multiple water molecules at random positions, velocities, and rotations
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

            // Add molecule and sphere visuals to the viewport
            var sphereVisual = new ModelVisual3D { Content = _sphereGroup };
            var moleculeVisual = new ModelVisual3D { Content = _moleculeGroup };
            helixViewport.Children.Add(moleculeVisual); // Add molecules first
            helixViewport.Children.Add(sphereVisual); // Add spheres second

            // Add the cylindrical boundary to the scene LAST for correct transparency rendering
            var boundary = new CylindricalBoundary();
            var boundaryVisual = new ModelVisual3D { Content = boundary.Model };
            helixViewport.Children.Add(boundaryVisual);

            // Set the camera to fit the entire cylinder at startup
            this.Loaded += (s, e) => helixViewport.ZoomExtents();

            // Set up and start the animation timer
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(15) };
            _timer.Tick += OnTimerTick;
            _timer.Start();

            _lastFrameTime = DateTime.Now;
        }

        /// <summary>
        /// Adds a molecule to the scene at the specified position, velocity, and rotation.
        /// </summary>
        private void AddMolecule(Point3D center, Vector3D velocity, double rotationSpeed, Vector3D rotationAxis)
        {
            var molecule = new Molecule(center, velocity, rotationSpeed, rotationAxis);
            _molecules.Add(molecule);
            _moleculeGroup.Children.Add(molecule.Model);
        }

        /// <summary>
        /// Animation loop: updates molecule positions, handles collisions, and tracks FPS.
        /// </summary>
        private void OnTimerTick(object sender, EventArgs e)
        {
            foreach (var molecule in _molecules)
            {
                molecule.Update();
            }

            // Check for collisions between all pairs of molecules
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

        // Uncomment to add test spheres for debugging
        //private void AddTestSpheres()
        //{
        //    var sphereMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(160, 255, 0, 0))); // Barely visible red
        //    var sphereMeshBuilder = new MeshBuilder();
        //    sphereMeshBuilder.AddSphere(new Point3D(0, 0, 0), 3.0); // Example sphere at the center with intermediate radius
        //    var sphereMesh = sphereMeshBuilder.ToMesh();
        //    var sphereModel = new GeometryModel3D(sphereMesh, sphereMaterial);
        //    sphereModel.Material = sphereMaterial;
        //    sphereModel.BackMaterial = sphereMaterial;
        //    _sphereGroup.Children.Add(sphereModel);
        //    // Add more test spheres at different positions with intermediate radius
        //    sphereMeshBuilder = new MeshBuilder();
        //    sphereMeshBuilder.AddSphere(new Point3D(5, 5, 4), 3.0); // Example sphere at (5, 5, 4)
        //    sphereMesh = sphereMeshBuilder.ToMesh();
        //    sphereModel = new GeometryModel3D(sphereMesh, sphereMaterial);
        //    sphereModel.Material = sphereMaterial;
        //    sphereModel.BackMaterial = sphereMaterial;
        //    _sphereGroup.Children.Add(sphereModel);
        //    sphereMeshBuilder = new MeshBuilder();
        //    sphereMeshBuilder.AddSphere(new Point3D(-5, -5, -4), 3.0); // Example sphere at (-5, -5, -4)
        //    sphereMesh = sphereMeshBuilder.ToMesh();
        //    sphereModel = new GeometryModel3D(sphereMesh, sphereMaterial);
        //    sphereModel.Material = sphereMaterial;
        //    sphereModel.BackMaterial = sphereMaterial;
        //    _sphereGroup.Children.Add(sphereModel);
        //}
    }
}
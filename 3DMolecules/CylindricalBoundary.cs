using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace ThreeDMolecules
{
    /// <summary>
    /// Represents a cylindrical boundary for the simulation.
    /// Builds a semi-transparent cylinder with opaque bottom and boundary rings for visualization.
    /// </summary>
    public class CylindricalBoundary
    {
        /// <summary>
        /// The 3D model group containing the cylinder, bottom, skirt, and rings.
        /// </summary>
        public Model3DGroup Model { get; }

        private readonly AxisAngleRotation3D _tiltX = new(new Vector3D(1, 0, 0), 0);
        private readonly AxisAngleRotation3D _tiltY = new(new Vector3D(0, 1, 0), 0);
        private readonly Transform3DGroup _transformGroup = new();

        /// <summary>
        /// The transform applied to the boundary (exposed so physics can use it too).
        /// </summary>
        public Transform3D Transform => _transformGroup;

        /// <summary>
        /// Initializes a new instance of the CylindricalBoundary class.
        /// </summary>
        public CylindricalBoundary()
        {
            Model = new Model3DGroup();

            // Build transform chain: rotate around X then Y.
            _transformGroup.Children.Add(new RotateTransform3D(_tiltX));
            _transformGroup.Children.Add(new RotateTransform3D(_tiltY));
            Model.Transform = _transformGroup;

            double radius = SimulationSettings.CylinderRadius;
            double height = SimulationSettings.CylinderHeight;

            // Cylinder wall (semi-transparent, open on top)
            var meshBuilder = new MeshBuilder();
            // IMPORTANT: last bool parameter controls whether end caps are generated. False => no caps (open tube)
            meshBuilder.AddCylinder(new Point3D(0, 0, -height / 2 + 0.01), new Point3D(0, 0, height / 2), radius, 72, false);
            var cylinderMesh = meshBuilder.ToMesh();

            // OUTER wall (front faces): very light frosted gray (more transparent)
            var outerMaterial = new MaterialGroup();
            outerMaterial.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(40, 240, 240, 240))));
            outerMaterial.Children.Add(new SpecularMaterial(new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)), 30));

            // INNER wall (back faces): richer blue tint (less transparent) so interior stands out
            var innerMaterial = new MaterialGroup();
            innerMaterial.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(160, 0, 120, 255))));
            innerMaterial.Children.Add(new SpecularMaterial(new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)), 60));

            var cylinderModel = new GeometryModel3D(cylinderMesh, outerMaterial)
            {
                BackMaterial = innerMaterial
            };

            // Opaque bottom disk, slightly larger and lower than the wall
            double bottomRadius = radius + 0.1;
            double bottomZ1 = -height / 2 - 0.02;
            double bottomZ2 = -height / 2 - 0.01;
            var bottomMeshBuilder = new MeshBuilder();
            bottomMeshBuilder.AddCylinder(new Point3D(0, 0, bottomZ1), new Point3D(0, 0, bottomZ2), bottomRadius, 48);
            var bottomMesh = bottomMeshBuilder.ToMesh();
            var bottomMaterial = new MaterialGroup();
            bottomMaterial.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, 0, 150, 255))));
            var bottomModel = new GeometryModel3D(bottomMesh, bottomMaterial)
            {
                BackMaterial = bottomMaterial
            };

            // Opaque "skirt" at the very bottom to block view from below
            var skirtMeshBuilder = new MeshBuilder();
            skirtMeshBuilder.AddCylinder(new Point3D(0, 0, -height / 2 - 0.01), new Point3D(0, 0, -height / 2 + 0.01), bottomRadius, 36, true);
            var skirtMesh = skirtMeshBuilder.ToMesh();
            var skirtMaterial = new MaterialGroup();
            skirtMaterial.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(255, 0, 150, 255))));
            var skirtModel = new GeometryModel3D(skirtMesh, skirtMaterial)
            {
                BackMaterial = skirtMaterial
            };

            // Add in correct order: bottom, skirt, then cylinder wall
            Model.Children.Add(bottomModel);
            Model.Children.Add(skirtModel);
            Model.Children.Add(cylinderModel);

            // Add red boundary rings at the top and bottom
            AddBoundaryRing(Model, radius, height / 2, Colors.Red);
            AddBoundaryRing(Model, radius, -height / 2, Colors.Red);

            // OPTIONAL: Add a "liquid" surface inside (semi-transparent horizontal disc)
            // Simulated fill level at 40% of height
            double fillLevel = -height / 2 + height * 0.4;
            var liquidBuilder = new MeshBuilder();
            // Very thin cylinder to imitate a surface (a few hundredths high)
            liquidBuilder.AddCylinder(new Point3D(0, 0, fillLevel), new Point3D(0, 0, fillLevel + 0.02), radius * 0.985, 72, true);
            var liquidMesh = liquidBuilder.ToMesh();
            var liquidMaterial = new MaterialGroup();
            // Deeper color, slightly emissive look with specular
            liquidMaterial.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(140, 0, 90, 200))));
            liquidMaterial.Children.Add(new SpecularMaterial(new SolidColorBrush(Color.FromArgb(220, 180, 220, 255)), 80));
            var liquidModel = new GeometryModel3D(liquidMesh, liquidMaterial)
            {
                BackMaterial = liquidMaterial
            };
            Model.Children.Add(liquidModel);
        }

        /// <summary>
        /// Adjust tilt angles (in degrees) around X and Y axes.
        /// </summary>
        public void SetTilt(double degreesX, double degreesY)
        {
            _tiltX.Angle = degreesX;
            _tiltY.Angle = degreesY;
        }

        /// <summary>
        /// Adds a colored ring at the specified Z position on the cylinder.
        /// </summary>
        /// <param name="group">The model group to add the ring to.</param>
        /// <param name="radius">Radius of the ring.</param>
        /// <param name="z">Z position of the ring.</param>
        /// <param name="color">Color of the ring.</param>
        private void AddBoundaryRing(Model3DGroup group, double radius, double z, Color color)
        {
            var ringBuilder = new MeshBuilder();
            int segments = 72;
            double thickness = 0.07; // Thin ring
            for (int i = 0; i < segments; i++)
            {
                double angle1 = 2 * Math.PI * i / segments;
                double angle2 = 2 * Math.PI * (i + 1) / segments;
                var p1 = new Point3D(radius * Math.Cos(angle1), radius * Math.Sin(angle1), z);
                var p2 = new Point3D(radius * Math.Cos(angle2), radius * Math.Sin(angle2), z);
                ringBuilder.AddCylinder(p1, p2, thickness, 6);
            }
            var ringMesh = ringBuilder.ToMesh();
            var ringMaterial = new DiffuseMaterial(new SolidColorBrush(color));
            var ringModel = new GeometryModel3D(ringMesh, ringMaterial)
            {
                BackMaterial = ringMaterial
            };
            group.Children.Add(ringModel);
        }
    }
}

using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace ThreeDMolecules
{
    public class CylindricalBoundary
    {
        public Model3DGroup Model { get; }

        public CylindricalBoundary(double radius, double height)
        {
            Model = new Model3DGroup();

            // Cylinder wall (semi-transparent, open on top)
            var meshBuilder = new MeshBuilder();
            meshBuilder.AddCylinder(new Point3D(0, 0, -height / 2 + 0.01), new Point3D(0, 0, height / 2), radius, 36, true);
            var cylinderMesh = meshBuilder.ToMesh();
            var wallMaterial = new MaterialGroup();
            wallMaterial.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(60, 0, 150, 255))));
            var cylinderModel = new GeometryModel3D(cylinderMesh, wallMaterial)
            {
                BackMaterial = wallMaterial
            };

            // Opaque bottom disk, slightly larger and lower than the wall
            double bottomRadius = radius + 0.1;
            double bottomZ1 = -height / 2 - 0.02;
            double bottomZ2 = -height / 2 - 0.01;
            var bottomMeshBuilder = new MeshBuilder();
            bottomMeshBuilder.AddCylinder(new Point3D(0, 0, bottomZ1), new Point3D(0, 0, bottomZ2), bottomRadius, 36);
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

            // Add in correct order
            Model.Children.Add(bottomModel);
            Model.Children.Add(skirtModel);
            Model.Children.Add(cylinderModel);

            // Add red boundary rings at the top and bottom
            AddBoundaryRing(Model, radius, height / 2, Colors.Red);
            AddBoundaryRing(Model, radius, -height / 2, Colors.Red);
        }

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

using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace ThreeDMolecules
{
    /// <summary>
    /// Represents a water molecule (H2O) as a 3D model with atoms and bonds.
    /// </summary>
    public class Molecule
    {
        /// <summary>
        /// The 3D model group containing all atoms and bonds of the molecule.
        /// </summary>
        public Model3DGroup Model { get; }
        private Vector3D _moleculeVelocity;
        private double _moleculeRotationSpeed;
        private Vector3D _moleculeRotationAxis;
        private const double MinRotationSpeed = 0.1; // Minimum rotation speed to ensure rotation
        private TranslateTransform3D _moleculeTranslateTransform;
        private RotateTransform3D _moleculeRotateTransform;
        private AxisAngleRotation3D _moleculeAxisAngleRotation;
        private const double Radius = 0.6; // Approximate radius of the molecule for collision detection

        // Define the cylindrical boundary
        private const double CylinderRadius = 10.0;
        private const double CylinderHeight = 20.0;

        /// <summary>
        /// Initializes a new instance of the Molecule class.
        /// Builds a water molecule (H2O) with 1 oxygen and 2 hydrogens, and their bonds.
        /// </summary>
        /// <param name="center">Initial center position of the molecule.</param>
        /// <param name="velocity">Initial velocity vector.</param>
        /// <param name="rotationSpeed">Initial rotation speed.</param>
        /// <param name="rotationAxis">Initial rotation axis.</param>
        public Molecule(Point3D center, Vector3D velocity, double rotationSpeed, Vector3D rotationAxis)
        {
            _moleculeVelocity = velocity;
            _moleculeRotationSpeed = rotationSpeed;
            _moleculeRotationAxis = rotationAxis;
            _moleculeTranslateTransform = new TranslateTransform3D(center.X, center.Y, center.Z);
            _moleculeAxisAngleRotation = new AxisAngleRotation3D(rotationAxis, 0);
            _moleculeRotateTransform = new RotateTransform3D(_moleculeAxisAngleRotation);

            var transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(_moleculeRotateTransform);
            transformGroup.Children.Add(_moleculeTranslateTransform);

            Model = new Model3DGroup();

            // Create oxygen atom (blue sphere at the center)
            var oxygenMaterial = new MaterialGroup
            {
                Children = new MaterialCollection
                    {
                        new DiffuseMaterial(new SolidColorBrush(Colors.Blue)),
                        new SpecularMaterial(new SolidColorBrush(Colors.White), 50) // 50% reflective
                    }
            };

            var oxygen = new SphereVisual3D
            {
                Center = new Point3D(0, 0, 0), // Relative position
                Radius = 0.6,
                Material = oxygenMaterial,
                Transform = transformGroup
            };
            Model.Children.Add(oxygen.Content);

            // Calculate hydrogen positions based on bond angle (104.5°) and bond length (0.96 Å)
            double bondAngle = 104.5 * Math.PI / 180; // Convert to radians
            double bondLength = 0.96; // Bond length in angstroms

            var hydrogen1Position = new Point3D(
                bondLength * Math.Cos(bondAngle / 2),
                bondLength * Math.Sin(bondAngle / 2),
                0
            );

            var hydrogen2Position = new Point3D(
                bondLength * Math.Cos(bondAngle / 2),
                -bondLength * Math.Sin(bondAngle / 2),
                0
            );

            // Create hydrogen atoms (green spheres at calculated positions)
            var hydrogenMaterial = new MaterialGroup
            {
                Children = new MaterialCollection
                    {
                        new DiffuseMaterial(new SolidColorBrush(Colors.Green)),
                        new SpecularMaterial(new SolidColorBrush(Colors.White), 50) // 50% reflective
                    }
            };

            var hydrogen1 = new SphereVisual3D
            {
                Center = hydrogen1Position, // Relative position
                Radius = 0.3,
                Material = hydrogenMaterial,
                Transform = transformGroup
            };
            Model.Children.Add(hydrogen1.Content);

            var hydrogen2 = new SphereVisual3D
            {
                Center = hydrogen2Position, // Relative position
                Radius = 0.3,
                Material = hydrogenMaterial,
                Transform = transformGroup
            };
            Model.Children.Add(hydrogen2.Content);

            // Create bonds (gray cylinders) between oxygen and each hydrogen
            var bond1 = new PipeVisual3D
            {
                Point1 = new Point3D(0, 0, 0), // Relative position
                Point2 = hydrogen1Position, // Relative position
                Diameter = 0.1,
                Fill = new SolidColorBrush(Colors.Gray),
                Transform = transformGroup
            };
            Model.Children.Add(bond1.Content);

            var bond2 = new PipeVisual3D
            {
                Point1 = new Point3D(0, 0, 0), // Relative position
                Point2 = hydrogen2Position, // Relative position
                Diameter = 0.1,
                Fill = new SolidColorBrush(Colors.Gray),
                Transform = transformGroup
            };
            Model.Children.Add(bond2.Content);

            // Apply the transform group to the entire molecule group
            Model.Transform = transformGroup;
        }

        public void Update()
        {
            _moleculeTranslateTransform.OffsetX += _moleculeVelocity.X;
            _moleculeTranslateTransform.OffsetY += _moleculeVelocity.Y;
            _moleculeTranslateTransform.OffsetZ += _moleculeVelocity.Z;

            var center = new Point3D(
                _moleculeTranslateTransform.OffsetX,
                _moleculeTranslateTransform.OffsetY,
                _moleculeTranslateTransform.OffsetZ
            );

            // Update rotation angle
            _moleculeAxisAngleRotation.Angle += Math.Max(_moleculeRotationSpeed, MinRotationSpeed);

            // Adjust the boundary conditions to match the cylindrical boundary
            double distanceFromCenter = Math.Sqrt(center.X * center.X + center.Y * center.Y);

            if (distanceFromCenter > CylinderRadius)
            {
                // Reflect the velocity vector off the cylindrical wall
                Vector3D normal = new Vector3D(center.X, center.Y, 0);
                normal.Normalize();
                _moleculeVelocity -= 2 * Vector3D.DotProduct(_moleculeVelocity, normal) * normal;

                // Ensure the molecule stays within the cylindrical boundary
                _moleculeTranslateTransform.OffsetX = normal.X * CylinderRadius;
                _moleculeTranslateTransform.OffsetY = normal.Y * CylinderRadius;
            }

            if (center.Z > CylinderHeight / 2 || center.Z < -CylinderHeight / 2)
            {
                _moleculeVelocity.Z = -_moleculeVelocity.Z;

                // Ensure the molecule stays within the cylindrical boundary
                _moleculeTranslateTransform.OffsetZ = Math.Max(Math.Min(_moleculeTranslateTransform.OffsetZ, CylinderHeight / 2), -CylinderHeight / 2);
            }
        }

        public bool IsCollidingWith(Molecule other)
        {
            var distance = (GetCenter() - other.GetCenter()).Length;
            return distance < 2 * Radius;
        }

        public void HandleCollision(Molecule other)
        {
            // Calculate the normal vector of the collision
            var normal = GetCenter() - other.GetCenter();
            normal.Normalize();

            // Calculate relative velocity
            var relativeVelocity = _moleculeVelocity - other._moleculeVelocity;

            // Calculate the velocity along the normal
            var velocityAlongNormal = Vector3D.DotProduct(relativeVelocity, normal);

            // If the velocities are separating, do nothing
            if (velocityAlongNormal > 0)
                return;

            // Calculate restitution (elasticity)
            const double restitution = 0.8; // Adjust this value for more or less bounciness

            // Calculate impulse scalar
            var impulseScalar = -(1 + restitution) * velocityAlongNormal;
            impulseScalar /= 2; // Divide by the sum of the masses (assuming equal mass)

            // Apply impulse to the velocities
            var impulse = impulseScalar * normal;
            _moleculeVelocity += impulse;
            other._moleculeVelocity -= impulse;

            // Apply a small separation force to push the molecules apart
            const double separationDistance = 0.1; // Adjust this value for more or less separation
            _moleculeTranslateTransform.OffsetX += normal.X * separationDistance;
            _moleculeTranslateTransform.OffsetY += normal.Y * separationDistance;
            _moleculeTranslateTransform.OffsetZ += normal.Z * separationDistance;
            other._moleculeTranslateTransform.OffsetX -= normal.X * separationDistance;
            other._moleculeTranslateTransform.OffsetY -= normal.Y * separationDistance;
            other._moleculeTranslateTransform.OffsetZ -= normal.Z * separationDistance;

            // Update rotation based on collision
            var rotationChange = Vector3D.CrossProduct(relativeVelocity, normal);
            _moleculeRotationAxis += rotationChange;
            other._moleculeRotationAxis -= rotationChange;

            // Normalize the rotation axis
            _moleculeRotationAxis.Normalize();
            other._moleculeRotationAxis.Normalize();

            // Adjust rotation speed
            _moleculeRotationSpeed += rotationChange.Length * 0.1; // Adjust the factor for more or less rotation change
            other._moleculeRotationSpeed += rotationChange.Length * 0.1; // Adjust the factor for more or less rotation change
        }

        public Point3D GetCenter()
        {
            return new Point3D(
                _moleculeTranslateTransform.OffsetX,
                _moleculeTranslateTransform.OffsetY,
                _moleculeTranslateTransform.OffsetZ
            );
        }
    }
}

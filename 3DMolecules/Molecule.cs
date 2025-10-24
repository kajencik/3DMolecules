using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using ThreeDMolecules.Models;

namespace ThreeDMolecules
{
    /// <summary>
    /// Represents the 3D visual representation of a water molecule (H2O).
    /// This class handles only rendering - physics logic is in MoleculeModel and the physics engine.
    /// </summary>
    public class Molecule
    {
        public Model3DGroup Model { get; }
        
        private readonly TranslateTransform3D _translateTransform;
        private readonly AxisAngleRotation3D _axisAngleRotation;

        // Visual constants
        private const double BondDiameter = 0.1;

        public Molecule(Point3D center, Vector3D velocity, double rotationSpeed, Vector3D rotationAxis)
        {
            _translateTransform = new TranslateTransform3D(center.X, center.Y, center.Z);
            _axisAngleRotation = new AxisAngleRotation3D(rotationAxis, 0);
            var rotateTransform = new RotateTransform3D(_axisAngleRotation);

            var transformGroup = new Transform3DGroup();
            transformGroup.Children.Add(rotateTransform);
            transformGroup.Children.Add(_translateTransform);

            Model = new Model3DGroup();

            // Create oxygen atom (blue sphere at the center)
            var oxygenMaterial = new MaterialGroup
            {
                Children = new MaterialCollection
                {
                    new DiffuseMaterial(new SolidColorBrush(Colors.Blue)),
                    new SpecularMaterial(new SolidColorBrush(Colors.White), 50)
                }
            };

            var oxygen = new SphereVisual3D
            {
                Center = new Point3D(0, 0, 0),
                Radius = MoleculeModel.OxygenRadius,
                Material = oxygenMaterial,
                Transform = transformGroup
            };
            Model.Children.Add(oxygen.Content);

            // Calculate hydrogen positions based on bond angle
            double bondAngle = MoleculeModel.BondAngleDegrees * Math.PI / 180;

            var hydrogen1Position = new Point3D(
                MoleculeModel.BondLength * Math.Cos(bondAngle / 2),
                MoleculeModel.BondLength * Math.Sin(bondAngle / 2),
                0
            );

            var hydrogen2Position = new Point3D(
                MoleculeModel.BondLength * Math.Cos(bondAngle / 2),
                -MoleculeModel.BondLength * Math.Sin(bondAngle / 2),
                0
            );

            // Create hydrogen atoms (green spheres)
            var hydrogenMaterial = new MaterialGroup
            {
                Children = new MaterialCollection
                {
                    new DiffuseMaterial(new SolidColorBrush(Colors.Green)),
                    new SpecularMaterial(new SolidColorBrush(Colors.White), 50)
                }
            };

            var hydrogen1 = new SphereVisual3D
            {
                Center = hydrogen1Position,
                Radius = MoleculeModel.HydrogenRadius,
                Material = hydrogenMaterial,
                Transform = transformGroup
            };
            Model.Children.Add(hydrogen1.Content);

            var hydrogen2 = new SphereVisual3D
            {
                Center = hydrogen2Position,
                Radius = MoleculeModel.HydrogenRadius,
                Material = hydrogenMaterial,
                Transform = transformGroup
            };
            Model.Children.Add(hydrogen2.Content);

            // Create bonds (gray cylinders)
            var bond1 = new PipeVisual3D
            {
                Point1 = new Point3D(0, 0, 0),
                Point2 = hydrogen1Position,
                Diameter = BondDiameter,
                Fill = new SolidColorBrush(Colors.Gray),
                Transform = transformGroup
            };
            Model.Children.Add(bond1.Content);

            var bond2 = new PipeVisual3D
            {
                Point1 = new Point3D(0, 0, 0),
                Point2 = hydrogen2Position,
                Diameter = BondDiameter,
                Fill = new SolidColorBrush(Colors.Gray),
                Transform = transformGroup
            };
            Model.Children.Add(bond2.Content);

            Model.Transform = transformGroup;
        }

        /// <summary>
        /// Updates the 3D visual transform based on the physics model state.
        /// </summary>
        public void UpdateFromModel(MoleculeModel model)
        {
            _translateTransform.OffsetX = model.Position.X;
            _translateTransform.OffsetY = model.Position.Y;
            _translateTransform.OffsetZ = model.Position.Z;

            _axisAngleRotation.Axis = model.RotationAxis;
            _axisAngleRotation.Angle = model.RotationAngle;
        }
    }
}

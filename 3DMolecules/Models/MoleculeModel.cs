using System.Windows.Media.Media3D;

namespace ThreeDMolecules.Models;

/// <summary>
/// Pure data model representing the physical state of a molecule.
/// Contains no 3D rendering logic - only physics data.
/// </summary>
public class MoleculeModel
{
    // Physics constants
    public const double OxygenRadius = 0.6;
    public const double HydrogenRadius = 0.3;
    public const double BondLength = 0.96; // Angstroms
    public const double BondAngleDegrees = 104.5;
    public const double CollisionRadius = OxygenRadius; // Simplified collision sphere
  public const double VisualExtentRadius = 1.5; // Covers rotated hydrogens

    // Physical state
    public Point3D Position { get; set; }
    public Vector3D Velocity { get; set; }
    public Vector3D RotationAxis { get; set; }
    public double RotationSpeed { get; set; }
    public double RotationAngle { get; set; }

    public MoleculeModel(Point3D position, Vector3D velocity, Vector3D rotationAxis, double rotationSpeed)
    {
        Position = position;
        Velocity = velocity;
        RotationAxis = rotationAxis;
        RotationSpeed = rotationSpeed;
        RotationAngle = 0;
 }

    /// <summary>
    /// Creates a copy of this molecule's state.
    /// </summary>
    public MoleculeModel Clone()
    {
      return new MoleculeModel(Position, Velocity, RotationAxis, RotationSpeed)
        {
            RotationAngle = RotationAngle
   };
    }
}

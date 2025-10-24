using System;
using System.Windows.Media.Media3D;
using ThreeDMolecules.Models;

namespace ThreeDMolecules.Services;

/// <summary>
/// Factory for creating randomly initialized molecules.
/// </summary>
public class MoleculeFactory
{
    private readonly Random _random;

    public MoleculeFactory(Random? random = null)
    {
        _random = random ?? new Random();
    }

    /// <summary>
    /// Creates a molecule with random position, velocity, and rotation within the simulation bounds.
    /// </summary>
    public MoleculeModel CreateRandom()
    {
        // Random position within cylinder (with some margin)
        double maxRadius = SimulationSettings.CylinderRadius - MoleculeModel.VisualExtentRadius - 1.0;
      double maxHeight = SimulationSettings.CylinderHeight / 2 - MoleculeModel.VisualExtentRadius - 1.0;

        var position = new Point3D(
   _random.NextDouble() * maxRadius * 2 - maxRadius,
            _random.NextDouble() * maxRadius * 2 - maxRadius,
            _random.NextDouble() * maxHeight * 2 - maxHeight
  );

        // Random velocity
        var velocity = new Vector3D(
    _random.NextDouble() * (SimulationSettings.MaxVelocity - SimulationSettings.MinVelocity) + SimulationSettings.MinVelocity,
    _random.NextDouble() * (SimulationSettings.MaxVelocity - SimulationSettings.MinVelocity) + SimulationSettings.MinVelocity,
     _random.NextDouble() * (SimulationSettings.MaxVelocity - SimulationSettings.MinVelocity) + SimulationSettings.MinVelocity
        );

     // Random rotation
        double rotationSpeed = _random.NextDouble() * 
     (SimulationSettings.MaxRotationSpeedInit - SimulationSettings.MinRotationSpeedInit) + 
  SimulationSettings.MinRotationSpeedInit;

      var rotationAxis = new Vector3D(
_random.NextDouble() * 2 - 1,
         _random.NextDouble() * 2 - 1,
       _random.NextDouble() * 2 - 1
        );
 rotationAxis.Normalize();

        return new MoleculeModel(position, velocity, rotationAxis, rotationSpeed);
    }

    /// <summary>
    /// Creates a molecule at a specific position with random velocity and rotation.
    /// </summary>
    public MoleculeModel CreateAt(Point3D position)
    {
        var molecule = CreateRandom();
        molecule.Position = position;
        return molecule;
    }
}

using Xunit;
using ThreeDMolecules.Models;
using ThreeDMolecules.Services;
using System.Windows.Media.Media3D;
using System.Collections.Generic;

namespace ThreeDMolecules.Tests;

/// <summary>
/// Unit tests for the physics engine.
/// </summary>
public class PhysicsEngineTests
{
    [Fact]
    public void HeadOnCollision_ReversesVelocities()
    {
   // Arrange - Place molecules further apart so they collide during simulation
        var engine = new CpuPhysicsEngine();
        var molecules = new List<MoleculeModel>
        {
  // Molecule 1: Further LEFT, moving RIGHT
    new(new Point3D(-2, 0, 0), new Vector3D(0.5, 0, 0), new Vector3D(0, 0, 1), 1.0),
            
 // Molecule 2: Further RIGHT, moving LEFT
      new(new Point3D(2, 0, 0), new Vector3D(-0.5, 0, 0), new Vector3D(0, 0, 1), 1.0)
      };

 // Store initial velocities
        var initialVel1 = molecules[0].Velocity.X;
        var initialVel2 = molecules[1].Velocity.X;

  // Act - Run multiple steps to allow collision to happen
      for (int i = 0; i < 10; i++)
        {
 engine.Update(molecules, 1.0);
            
   // Check if velocities have reversed (collision happened)
            if (molecules[0].Velocity.X < 0 && molecules[1].Velocity.X > 0)
    break;
        }

        // Assert - Velocities should have reversed direction
        Assert.True(molecules[0].Velocity.X < 0, 
 $"First molecule should bounce back (was moving right at {initialVel1}, now {molecules[0].Velocity.X})");
      Assert.True(molecules[1].Velocity.X > 0, 
      $"Second molecule should bounce back (was moving left at {initialVel2}, now {molecules[1].Velocity.X})");
    }

    [Fact]
    public void BoundaryCollision_ReflectsVelocity()
    {
        // Arrange
        var engine = new CpuPhysicsEngine();
        var outOfBounds = SimulationSettings.CylinderRadius + 1;
    var molecules = new List<MoleculeModel>
  {
       new(new Point3D(outOfBounds, 0, 0), new Vector3D(1, 0, 0), new Vector3D(0, 0, 1), 1.0)
 };

    // Act
  engine.Update(molecules, 1.0);

     // Assert
        Assert.True(molecules[0].Velocity.X < 0, "Velocity should reflect off boundary");
      var distance = System.Math.Sqrt(
         molecules[0].Position.X * molecules[0].Position.X +
   molecules[0].Position.Y * molecules[0].Position.Y
      );
     Assert.True(distance <= SimulationSettings.CylinderRadius, "Should be within boundary");
    }

    [Fact]
    public void NoCollision_MaintainsVelocity()
    {
    // Arrange
        var engine = new CpuPhysicsEngine();
        var initialVelocity = new Vector3D(0.5, 0.3, 0.2);
      var molecules = new List<MoleculeModel>
     {
          new(new Point3D(0, 0, 0), initialVelocity, new Vector3D(0, 0, 1), 1.0),
         new(new Point3D(5, 5, 5), new Vector3D(0, 0, 0), new Vector3D(0, 0, 1), 1.0)
      };

        // Act
        engine.Update(molecules, 1.0);

        // Assert (velocity should be approximately the same)
        Assert.True(System.Math.Abs(molecules[0].Velocity.X - initialVelocity.X) < 0.01);
        Assert.True(System.Math.Abs(molecules[0].Velocity.Y - initialVelocity.Y) < 0.01);
        Assert.True(System.Math.Abs(molecules[0].Velocity.Z - initialVelocity.Z) < 0.01);
    }

    [Fact]
    public void MoleculeFactory_CreatesValidMolecules()
    {
        // Arrange
        var factory = new MoleculeFactory(new System.Random(42)); // Seeded for reproducibility

      // Act
        var molecule = factory.CreateRandom();

        // Assert
        Assert.NotNull(molecule);
        Assert.True(molecule.Velocity.Length > 0, "Should have non-zero velocity");
        Assert.True(molecule.RotationSpeed > 0, "Should have rotation");
     Assert.True(molecule.RotationAxis.Length > 0.99, "Rotation axis should be normalized");
    }

    [Fact]
    public void GetDiagnostics_ReturnsInfo()
    {
        // Arrange
        var engine = new CpuPhysicsEngine();
    var molecules = new List<MoleculeModel>
        {
        new(new Point3D(0, 0, 0), new Vector3D(1, 0, 0), new Vector3D(0, 0, 1), 1.0),
    new(new Point3D(1, 0, 0), new Vector3D(-1, 0, 0), new Vector3D(0, 0, 1), 1.0)
        };

        // Act
        engine.Update(molecules, 1.0);
        var diagnostics = engine.GetDiagnostics();

      // Assert
        Assert.NotNull(diagnostics);
   Assert.Contains("CPU Engine", diagnostics);
        Assert.Contains("Checks:", diagnostics);
 }
}

using Xunit;
using ThreeDMolecules.Models;
using ThreeDMolecules.Services;
using System.Windows.Media.Media3D;
using System.Collections.Generic;

namespace ThreeDMolecules.Tests;

/// <summary>
/// Unit tests for the physics engine with updated fluid dynamics.
/// Tests disable or isolate specific forces to keep assertions stable and focused.
/// </summary>
public class PhysicsEngineTests
{
 private class TestParameters : ISimulationParameters
 {
 public double CohesionDistance { get; init; } =0; // default: off
 public double LiquidStiffness { get; init; } =0;
 public double LiquidNearStiffness { get; init; } =0;
 public double Viscosity { get; init; } =0;
 public double CohesionStrength { get; init; } =0;
 public double PreferredSpacing { get; init; } = SimulationSettings.PreferredSpacing;
 public double LinearDamping { get; init; } =0;
 public double Gravity { get; init; } =0;
 public double CollisionRestitution { get; init; } =1.0; // elastic for simple assertions
 public double SeparationForce { get; init; } = SimulationSettings.SeparationForce;
 public double MaxSpeed { get; init; } = double.PositiveInfinity;
 public double MinRotationSpeed { get; init; } = SimulationSettings.MinRotationSpeed;
 public double BoundaryEpsilon { get; init; } = SimulationSettings.BoundaryEpsilon;
 public double DefaultTimeStep { get; init; } =1.0;
 }

 private static TestParameters NoFluidForces() => new TestParameters
 {
 CohesionDistance =0,
 LiquidStiffness =0,
 LiquidNearStiffness =0,
 Viscosity =0,
 CohesionStrength =0,
 LinearDamping =0,
 Gravity =0,
 CollisionRestitution =1.0,
 MaxSpeed = double.PositiveInfinity
 };

 [Fact]
 public void HeadOnCollision_ReversesVelocities()
 {
 // Arrange - elastic collision, no fluid forces
 var engine = new CpuPhysicsEngine { Parameters = NoFluidForces() };
 var molecules = new List<MoleculeModel>
 {
 new(new Point3D(-2,0,0), new Vector3D(0.5,0,0), new Vector3D(0,0,1),1.0),
 new(new Point3D(2,0,0), new Vector3D(-0.5,0,0), new Vector3D(0,0,1),1.0)
 };

 // Act - Run multiple steps to allow collision to happen
 for (int i =0; i <10; i++)
 {
 engine.Update(molecules,1.0);
 if (molecules[0].Velocity.X <0 && molecules[1].Velocity.X >0)
 break;
 }

 // Assert - Velocities should have reversed direction
 Assert.True(molecules[0].Velocity.X <0, "First molecule should bounce back");
 Assert.True(molecules[1].Velocity.X >0, "Second molecule should bounce back");
 }

 [Fact]
 public void BoundaryCollision_ReflectsVelocity()
 {
 // Arrange - no fluid forces, no gravity
 var engine = new CpuPhysicsEngine { Parameters = NoFluidForces() };
 var outOfBounds = SimulationSettings.CylinderRadius +1;
 var molecules = new List<MoleculeModel>
 {
 new(new Point3D(outOfBounds,0,0), new Vector3D(1,0,0), new Vector3D(0,0,1),1.0)
 };

 // Act
 engine.Update(molecules,1.0);

 // Assert
 Assert.True(molecules[0].Velocity.X <0, "Velocity should reflect off boundary");
 var distance = System.Math.Sqrt(molecules[0].Position.X * molecules[0].Position.X + molecules[0].Position.Y * molecules[0].Position.Y);
 Assert.True(distance <= SimulationSettings.CylinderRadius, "Should be within boundary");
 }

 [Fact]
 public void NoCollision_MaintainsVelocity_WhenFluidForcesDisabled()
 {
 // Arrange - disable fluid forces so velocity is conserved
 var engine = new CpuPhysicsEngine { Parameters = NoFluidForces() };
 var initialVelocity = new Vector3D(0.5,0.3,0.2);
 var molecules = new List<MoleculeModel>
 {
 new(new Point3D(0,0,0), initialVelocity, new Vector3D(0,0,1),1.0),
 new(new Point3D(5,5,5), new Vector3D(0,0,0), new Vector3D(0,0,1),1.0)
 };

 // Act
 engine.Update(molecules,1.0);

 // Assert (velocity should be unchanged)
 Assert.Equal(initialVelocity.X, molecules[0].Velocity.X,6);
 Assert.Equal(initialVelocity.Y, molecules[0].Velocity.Y,6);
 Assert.Equal(initialVelocity.Z, molecules[0].Velocity.Z,6);
 }

 [Fact]
 public void Viscosity_ReducesRelativeVelocity()
 {
 // Arrange - isolate viscosity within interaction radius, beyond repulsion range
 var engine = new CpuPhysicsEngine
 {
 Parameters = new TestParameters
 {
 CohesionDistance =3.0,
 LiquidStiffness =0,
 LiquidNearStiffness =0,
 CohesionStrength =0,
 Viscosity =0.5,
 Gravity =0,
 LinearDamping =0,
 CollisionRestitution =1.0,
 MaxSpeed = double.PositiveInfinity,
 PreferredSpacing =2.0, // Set below actual distance to avoid repulsion
 DefaultTimeStep =1.0
 }
 };
 // Place molecules 2.5 units apart (beyond PreferredSpacing, within CohesionDistance)
 // This ensures only viscosity acts, not repulsion
 var a = new MoleculeModel(new Point3D(0,0,0), new Vector3D(1,0,0), new Vector3D(0,0,1),1.0);
 var b = new MoleculeModel(new Point3D(2.5,0,0), new Vector3D(-1,0,0), new Vector3D(0,0,1),1.0);
 var molecules = new List<MoleculeModel> { a, b };
 var initialRelative = (b.Velocity - a.Velocity).Length;

 // Act
 engine.Update(molecules,0.5); // half step is enough to damp
 var finalRelative = (b.Velocity - a.Velocity).Length;

 // Assert
 Assert.True(finalRelative < initialRelative, $"Viscosity should reduce relative velocity (from {initialRelative} to {finalRelative})");
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
 Assert.True(molecule.Velocity.Length >0, "Should have non-zero velocity");
 Assert.True(molecule.RotationSpeed >0, "Should have rotation");
 Assert.True(molecule.RotationAxis.Length >0.99, "Rotation axis should be normalized");
 }

 [Fact]
 public void GetDiagnostics_ReturnsInfo()
 {
 // Arrange
 var engine = new CpuPhysicsEngine { Parameters = NoFluidForces() };
 var molecules = new List<MoleculeModel>
 {
 new(new Point3D(0,0,0), new Vector3D(1,0,0), new Vector3D(0,0,1),1.0),
 new(new Point3D(1,0,0), new Vector3D(-1,0,0), new Vector3D(0,0,1),1.0)
 };

 // Act
 engine.Update(molecules,1.0);
 var diagnostics = engine.GetDiagnostics();

 // Assert
 Assert.NotNull(diagnostics);
 Assert.Contains("CPU Engine", diagnostics);
 Assert.Contains("Checks:", diagnostics);
 }
}

using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using ThreeDMolecules.Models;

namespace ThreeDMolecules.Services;

/// <summary>
/// CPU-based physics engine for molecular simulation.
/// Handles position updates, boundary collisions, and inter-molecular collisions.
/// </summary>
public class CpuPhysicsEngine : IPhysicsEngine
{
 private int _collisionChecks;
    private int _collisionsDetected;

    public void Update(IList<MoleculeModel> molecules, double deltaTime)
    {
     _collisionChecks = 0;
        _collisionsDetected = 0;

        // Update positions and handle boundary collisions
        foreach (var molecule in molecules)
        {
         UpdateMolecule(molecule, deltaTime);
        }

   // Handle inter-molecular collisions
        for (int i = 0; i < molecules.Count; i++)
     {
            for (int j = i + 1; j < molecules.Count; j++)
        {
         _collisionChecks++;
     if (IsColliding(molecules[i], molecules[j]))
      {
             _collisionsDetected++;
  HandleCollision(molecules[i], molecules[j]);
                }
            }
 }
    }

    public string GetDiagnostics()
    {
        return $"CPU Engine | Checks: {_collisionChecks:N0} | Collisions: {_collisionsDetected}";
  }

    private void UpdateMolecule(MoleculeModel molecule, double deltaTime)
    {
        // Update position
        molecule.Position = new Point3D(
            molecule.Position.X + molecule.Velocity.X * deltaTime,
            molecule.Position.Y + molecule.Velocity.Y * deltaTime,
   molecule.Position.Z + molecule.Velocity.Z * deltaTime
    );

   // Update rotation
     molecule.RotationAngle += Math.Max(molecule.RotationSpeed, SimulationSettings.MinRotationSpeed) * deltaTime;

        // Enforce cylindrical bounds
  EnforceBoundary(molecule);
    }

    private void EnforceBoundary(MoleculeModel molecule)
    {
double effectiveRadius = SimulationSettings.CylinderRadius - 
      MoleculeModel.VisualExtentRadius - 
       SimulationSettings.BoundaryEpsilon;
        
        double zMax = SimulationSettings.CylinderHeight / 2 - 
  MoleculeModel.VisualExtentRadius - 
           SimulationSettings.BoundaryEpsilon;

 var pos = molecule.Position;
     double distanceFromCenter = Math.Sqrt(pos.X * pos.X + pos.Y * pos.Y);

        // Cylindrical wall collision
  if (distanceFromCenter > effectiveRadius)
        {
            var normal = new Vector3D(pos.X, pos.Y, 0);
 if (normal.LengthSquared > 0)
            {
     normal.Normalize();
       
       // Reflect velocity
        molecule.Velocity -= 2 * Vector3D.DotProduct(molecule.Velocity, normal) * normal;

   // Clamp position
            molecule.Position = new Point3D(
           normal.X * effectiveRadius,
            normal.Y * effectiveRadius,
   pos.Z
  );
     }
        }

        // Top/bottom boundary collisions
 if (pos.Z > zMax)
        {
       molecule.Velocity = new Vector3D(molecule.Velocity.X, molecule.Velocity.Y, -Math.Abs(molecule.Velocity.Z));
         molecule.Position = new Point3D(pos.X, pos.Y, zMax);
        }
     else if (pos.Z < -zMax)
        {
        molecule.Velocity = new Vector3D(molecule.Velocity.X, molecule.Velocity.Y, Math.Abs(molecule.Velocity.Z));
            molecule.Position = new Point3D(pos.X, pos.Y, -zMax);
        }
    }

    private bool IsColliding(MoleculeModel a, MoleculeModel b)
    {
   var delta = a.Position - b.Position;
        double distanceSq = delta.X * delta.X + delta.Y * delta.Y + delta.Z * delta.Z;
        double minDistance = 2 * MoleculeModel.CollisionRadius;
     return distanceSq < minDistance * minDistance;
    }

    private void HandleCollision(MoleculeModel a, MoleculeModel b)
    {
        // Calculate collision normal
        var normal = a.Position - b.Position;
        double length = normal.Length;
        
        if (length < 1e-6)
  {
            // Molecules are exactly on top of each other - use random separation
  normal = new Vector3D(1, 0, 0);
        }
    else
        {
            normal.Normalize();
        }

        // Calculate relative velocity
        var relativeVelocity = a.Velocity - b.Velocity;
        double velocityAlongNormal = Vector3D.DotProduct(relativeVelocity, normal);

        // If separating, don't apply collision
      if (velocityAlongNormal > 0)
  return;

        // Calculate impulse (assuming equal mass)
    double restitution = SimulationSettings.CollisionRestitution;
        double impulseScalar = -(1 + restitution) * velocityAlongNormal / 2;

        var impulse = impulseScalar * normal;
        
  // Apply velocity changes
    a.Velocity += impulse;
  b.Velocity -= impulse;

     // Separate molecules slightly to prevent sticking
        double separation = SimulationSettings.SeparationForce;
        a.Position = new Point3D(
            a.Position.X + normal.X * separation,
            a.Position.Y + normal.Y * separation,
      a.Position.Z + normal.Z * separation
        );
        b.Position = new Point3D(
            b.Position.X - normal.X * separation,
            b.Position.Y - normal.Y * separation,
       b.Position.Z - normal.Z * separation
   );

        // Update rotation based on collision
        var rotationChange = Vector3D.CrossProduct(relativeVelocity, normal);
        
        a.RotationAxis = SafeNormalize(a.RotationAxis + rotationChange, new Vector3D(0, 0, 1));
      b.RotationAxis = SafeNormalize(b.RotationAxis - rotationChange, new Vector3D(0, 0, 1));

        double rotationSpeedChange = rotationChange.Length * SimulationSettings.RotationChangeScale;
        a.RotationSpeed += rotationSpeedChange;
   b.RotationSpeed += rotationSpeedChange;
    }

    private static Vector3D SafeNormalize(Vector3D v, Vector3D fallback)
    {
        double lenSq = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
        if (lenSq < 1e-6)
    return fallback;
        
        v.Normalize();
      return v;
    }
}

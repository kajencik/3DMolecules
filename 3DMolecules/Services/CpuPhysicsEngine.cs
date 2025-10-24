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
 private Transform3D? _boundaryTransform;
 private Matrix3D _boundaryMatrix = Matrix3D.Identity;
 private Matrix3D _boundaryInverse = Matrix3D.Identity;

 public ISimulationParameters Parameters { get; set; } = new DefaultParameters();

 private class DefaultParameters : ISimulationParameters
 {
 public double CohesionDistance => SimulationSettings.CohesionDistance;
 public double LiquidStiffness => SimulationSettings.LiquidStiffness;
 public double LiquidNearStiffness => SimulationSettings.LiquidNearStiffness;
 public double Viscosity => SimulationSettings.Viscosity;
 public double CohesionStrength => SimulationSettings.CohesionStrength;
 public double PreferredSpacing => SimulationSettings.PreferredSpacing;
 public double LinearDamping => SimulationSettings.LinearDamping;
 public double Gravity => SimulationSettings.Gravity;
 public double CollisionRestitution => SimulationSettings.CollisionRestitution;
 public double SeparationForce => SimulationSettings.SeparationForce;
 public double MaxSpeed => SimulationSettings.MaxSpeed;
 public double MinRotationSpeed => SimulationSettings.MinRotationSpeed;
 public double BoundaryEpsilon => SimulationSettings.BoundaryEpsilon;
 public double DefaultTimeStep => SimulationSettings.DefaultTimeStep;
 }

 public void SetBoundaryTransform(Transform3D? transform)
 {
 _boundaryTransform = transform;
 }

 public void Update(IList<MoleculeModel> molecules, double deltaTime)
 {
 // Refresh boundary matrices each frame, to pick up tilt changes
 if (_boundaryTransform is null)
 {
 _boundaryMatrix = Matrix3D.Identity;
 _boundaryInverse = Matrix3D.Identity;
 }
 else
 {
 _boundaryMatrix = _boundaryTransform.Value;
 _boundaryInverse = _boundaryMatrix;
 _boundaryInverse.Invert();
 }

 _collisionChecks =0;
 _collisionsDetected =0;

 // Update positions and apply boundary separately from inter-particle forces
 for (int i =0; i < molecules.Count; i++)
 {
 UpdateMolecule(molecules[i], deltaTime);
 }

 // Pairwise interactions: stable repulsion core, mild cohesion, viscosity
 for (int i =0; i < molecules.Count; i++)
 {
 for (int j = i +1; j < molecules.Count; j++)
 {
 _collisionChecks++;
 var a = molecules[i];
 var b = molecules[j];
 var r = b.Position - a.Position;
 double distSq = r.LengthSquared;
 double h = Parameters.CohesionDistance; // interaction radius
 double h2 = h * h;
 if (distSq > h2 || distSq <1e-12)
 {
 // Too far or too close to compute safely
 if (IsColliding(a, b)) { _collisionsDetected++; HandleCollision(a, b); }
 continue;
 }
 double dist = Math.Sqrt(distSq);
 var n = r / dist; // normalized direction a->b

 // Stable repulsion when closer than preferred spacing
 double preferred = Parameters.PreferredSpacing;
 if (dist < preferred)
 {
 double q =1.0 - (dist / preferred); //0 at preferred,1 at zero
 double repulse = q * Parameters.LiquidStiffness + q * q * Parameters.LiquidNearStiffness;
 var impulse = n * (repulse * deltaTime);
 a.Velocity -= impulse; // push apart symmetrically
 b.Velocity += impulse;
 }
 else
 {
 // Mild cohesion up to h to keep puddle together without clumping
 double q =1.0 - (dist - preferred) / (h - preferred); //1 at preferred,0 at h
 if (q >0)
 {
 var impulse = n * (Parameters.CohesionStrength * q * deltaTime);
 a.Velocity += impulse; // pull slightly together
 b.Velocity -= impulse;
 }
 }

 // XSPH-like viscosity to reduce jitter
 var relV = b.Velocity - a.Velocity;
 double w =1.0 - (dist / h); // linear kernel weight within radius
 if (w >0)
 {
 var visc = relV * (Parameters.Viscosity * w * deltaTime);
 a.Velocity += visc;
 b.Velocity -= visc;
 }

 // Finally, check hard-sphere collision to avoid interpenetration
 if (IsColliding(a, b))
 {
 _collisionsDetected++;
 HandleCollision(a, b);
 }
 }
 }

 // Per-particle linear damping
 if (Parameters.LinearDamping >0)
 {
 double damp = Math.Max(0,1.0 - Parameters.LinearDamping * deltaTime);
 for (int i =0; i < molecules.Count; i++)
 {
 molecules[i].Velocity *= damp;
 }
 }

 // Global speed cap if configured
 if (!double.IsPositiveInfinity(Parameters.MaxSpeed))
 {
 double maxSpeed = Parameters.MaxSpeed;
 double maxSpeedSq = maxSpeed * maxSpeed;
 foreach (var m in molecules)
 {
 var v = m.Velocity;
 double lenSq = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
 if (lenSq > maxSpeedSq)
 {
 var scaled = v; scaled.Normalize();
 m.Velocity = scaled * maxSpeed;
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
 // Apply world gravity
 molecule.Velocity += new Vector3D(0,0, Parameters.Gravity * deltaTime);

 // Integrate
 molecule.Position = new Point3D(
 molecule.Position.X + molecule.Velocity.X * deltaTime,
 molecule.Position.Y + molecule.Velocity.Y * deltaTime,
 molecule.Position.Z + molecule.Velocity.Z * deltaTime
 );

 // Rotation
 molecule.RotationAngle += Math.Max(molecule.RotationSpeed, Parameters.MinRotationSpeed) * deltaTime;

 // Boundary in container local space
 EnforceBoundary(molecule);
 }

 private void EnforceBoundary(MoleculeModel molecule)
 {
 // To boundary-local space
 Point3D localPos = molecule.Position;
 Vector3D localVel = molecule.Velocity;
 if (_boundaryTransform is not null)
 {
 localPos = _boundaryInverse.Transform(molecule.Position);
 localVel = _boundaryInverse.Transform(molecule.Velocity);
 }

 double effectiveRadius = SimulationSettings.CylinderRadius - MoleculeModel.VisualExtentRadius - Parameters.BoundaryEpsilon;
 double zMax = SimulationSettings.CylinderHeight /2 - MoleculeModel.VisualExtentRadius - Parameters.BoundaryEpsilon;
 double distanceFromCenter = Math.Sqrt(localPos.X * localPos.X + localPos.Y * localPos.Y);

 // Side wall
 if (distanceFromCenter > effectiveRadius)
 {
 var normal = new Vector3D(localPos.X, localPos.Y,0);
 if (normal.LengthSquared >0)
 {
 normal.Normalize();
 localVel -=2 * Vector3D.DotProduct(localVel, normal) * normal;
 localPos = new Point3D(normal.X * effectiveRadius, normal.Y * effectiveRadius, localPos.Z);
 }
 }
 // Bottom/top
 if (localPos.Z > zMax)
 {
 localVel = new Vector3D(localVel.X, localVel.Y, -Math.Abs(localVel.Z));
 localPos = new Point3D(localPos.X, localPos.Y, zMax);
 }
 else if (localPos.Z < -zMax)
 {
 localVel = new Vector3D(localVel.X, localVel.Y, Math.Abs(localVel.Z));
 localPos = new Point3D(localPos.X, localPos.Y, -zMax);
 }

 // Back to world
 if (_boundaryTransform is not null)
 {
 molecule.Position = _boundaryMatrix.Transform(localPos);
 molecule.Velocity = _boundaryMatrix.Transform(localVel);
 }
 else
 {
 molecule.Position = localPos;
 molecule.Velocity = localVel;
 }
 }

 private bool IsColliding(MoleculeModel a, MoleculeModel b)
 {
 var delta = a.Position - b.Position;
 double distanceSq = delta.X * delta.X + delta.Y * delta.Y + delta.Z * delta.Z;
 double minDistance =2 * MoleculeModel.CollisionRadius;
 return distanceSq < minDistance * minDistance;
 }

 private void HandleCollision(MoleculeModel a, MoleculeModel b)
 {
 var normal = a.Position - b.Position;
 double length = normal.Length;
 if (length <1e-6) normal = new Vector3D(1,0,0); else normal.Normalize();
 var relativeVelocity = a.Velocity - b.Velocity;
 double velocityAlongNormal = Vector3D.DotProduct(relativeVelocity, normal);
 if (velocityAlongNormal >0) return;
 double restitution = Parameters.CollisionRestitution;
 double impulseScalar = -(1 + restitution) * velocityAlongNormal /2;
 var impulse = impulseScalar * normal;
 a.Velocity += impulse;
 b.Velocity -= impulse;
 double separation = Parameters.SeparationForce;
 a.Position = new Point3D(a.Position.X + normal.X * separation, a.Position.Y + normal.Y * separation, a.Position.Z + normal.Z * separation);
 b.Position = new Point3D(b.Position.X - normal.X * separation, b.Position.Y - normal.Y * separation, b.Position.Z - normal.Z * separation);
 var rotationChange = Vector3D.CrossProduct(relativeVelocity, normal);
 a.RotationAxis = SafeNormalize(a.RotationAxis + rotationChange, new Vector3D(0,0,1));
 b.RotationAxis = SafeNormalize(b.RotationAxis - rotationChange, new Vector3D(0,0,1));
 double rotationSpeedChange = rotationChange.Length * SimulationSettings.RotationChangeScale;
 a.RotationSpeed += rotationSpeedChange;
 b.RotationSpeed += rotationSpeedChange;
 }

 private static Vector3D SafeNormalize(Vector3D v, Vector3D fallback)
 {
 double lenSq = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
 if (lenSq <1e-6) return fallback;
 v.Normalize();
 return v;
 }
}

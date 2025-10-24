namespace ThreeDMolecules.Services
{
 /// <summary>
 /// Runtime-adjustable simulation parameters used by the physics engine.
 /// Values typically originate from UI sliders. Defaults are taken from SimulationSettings.
 /// </summary>
 public interface ISimulationParameters
 {
 // Core liquid interaction
 double CohesionDistance { get; }
 double LiquidStiffness { get; }
 double LiquidNearStiffness { get; }
 double Viscosity { get; }
 double CohesionStrength { get; }
 double PreferredSpacing { get; }
 double LinearDamping { get; }

 // Global forces
 double Gravity { get; }

 // Collisions/limits
 double CollisionRestitution { get; }
 double SeparationForce { get; }
 double MaxSpeed { get; }
 double MinRotationSpeed { get; }
 double BoundaryEpsilon { get; }

 // Timing
 double DefaultTimeStep { get; }
 }
}

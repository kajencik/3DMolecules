namespace ThreeDMolecules
{
    /// <summary>
    /// Centralized configuration for the simulation.
    /// All magic numbers and settings should be defined here.
    /// </summary>
    public static class SimulationSettings
    {
        // Cylinder dimensions
        public const double CylinderRadius = 10.0;
        public const double CylinderHeight = 20.0;

        // Physics settings
        public const double DefaultTimeStep = 0.2; // Smaller time step slows the simulation in UI (~5x slower than 1.0)
        public const double CollisionRestitution = 0.8; // Elasticity (0=inelastic, 1=perfectly elastic)
        public const double SeparationForce = 0.1; // Push-apart distance on collision
        public const double MinRotationSpeed = 0.1; // Minimum rotation to prevent stopping
        public const double BoundaryEpsilon = 1e-3; // Small inset to avoid visual clipping

        // Liquid simulation settings
        public const double CohesionDistance = 2.5; // Max distance for molecules to attract each other
        public const double LiquidStiffness = 0.08; // Repulsion core (reduced for stability)
        public const double LiquidNearStiffness = 0.3; // Stronger repulsion when very close
        public const double Viscosity = 0.2; // Smooths motion (XSPH-like)
        public const double CohesionStrength = 0.02; // Mild attraction beyond preferred spacing
        public const double LinearDamping = 0.02; // Global velocity damping per second
        public const double PreferredSpacing = 2.2; // Desired spacing between molecule centers
        public const double Gravity = -0.02; // Stronger downward force for clear puddle behavior

        // Speed limiting
        // Set to a finite value (e.g., 0.05) to cap molecule speeds; default Infinity keeps tests/behavior unchanged.
        public const double MaxSpeed = double.PositiveInfinity;

        // Initial molecule generation
        public const double MinVelocity = -0.01;
        public const double MaxVelocity = 0.01;
        public const double MinRotationSpeedInit = 1.0;
        public const double MaxRotationSpeedInit = 5.0;

        // Collision physics
        public const double RotationChangeScale = 0.1; // How much collisions affect rotation

        // Simulation timing
        public const int UpdateIntervalMs = 15; // ~67 FPS target
        public const double FpsUpdateIntervalSeconds = 1.0;

        // UI defaults
        public const int DefaultMoleculeCount = 120;
        public const int MinMoleculeCount = 0;
        public const int MaxMoleculeCount = 480;
    }
}
